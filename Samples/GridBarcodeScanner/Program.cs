using Dynamsoft.CVR;
using Dynamsoft.DBR;
using Dynamsoft.Utility;
using Dynamsoft.License;
using Dynamsoft.Core;
using OpenCvSharp;
using System.Text;

namespace GridBarcodeScanner
{
    #region Constants
    internal static class Config
    {
        public const string SampleImagePath = "../../../../../Images/sample_grid.png";
        public const string LightweightTemplatePath = "../../../../../CustomTemplates/GridFastScan.json";
        public const string LightweightTemplateName = "GridFastScan";
        public const string DeepDecodeTemplatePath = "../../../../../CustomTemplates/GridDeepDecode.json";
        public const string DeepDecodeTemplateName = "GridDeepDecode";
        public const string ResultDir = "./Result/";
        public const float ScaleFactor = 2.0f;
    }
    #endregion

    #region Data Structures
    internal enum LayoutResultItemType
    {
        DecodeFailed,
        Decoded,
        Inferred
    }

    internal struct InputParams
    {
        public bool Exit;
        public string ImagePath;
        public int Row;
        public int Column;

        public InputParams()
        {
            Exit = false;
            ImagePath = "";
            Row = -1;
            Column = -1;
        }
    }

    internal class CommonDecodeResult
    {
        public List<Quadrilateral> Locations { get; } = new();
        public List<string> Texts { get; } = new();
        public int Error { get; set; }

        public CommonDecodeResult(int err = 0) { Error = err; }

        public void PrepareForLayoutAnalysis()
        {
            for (int i = 0; i < Locations.Count; i++)
                Locations[i].id = i;
        }

        public string GetText(int id)
        {
            if (id < 0 || id >= Texts.Count)
                throw new ArgumentOutOfRangeException(nameof(id), $"Invalid id: {id}");
            return Texts[id];
        }
    }

    internal class LayoutResultItem
    {
        public Quadrilateral Location { get; set; } = new();
        public string Text { get; set; } = "";
        public LayoutResultItemType Type { get; set; } = LayoutResultItemType.Inferred;
    }

    internal class GridResult
    {
        private readonly Dictionary<int, Dictionary<int, LayoutResultItem>> _items = new();
        private readonly object _lock = new();

        public IReadOnlyDictionary<int, Dictionary<int, LayoutResultItem>> Items => _items;

        public GridResult(CommonDecodeResult commonResult, LayoutAnalysisResult layoutResult)
        {
            int rows = layoutResult.elements.GetLength(0);
            int cols = layoutResult.elements.GetLength(1);
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    var element = layoutResult.elements[r, c];
                    switch (element.source)
                    {
                        case EnumLayoutElementSource.LES_INPUT:
                            try
                            {
                                string text = commonResult.GetText(element.quad.id);
                                SetItem(r, c, new LayoutResultItem
                                {
                                    Location = element.quad,
                                    Text = text,
                                    Type = LayoutResultItemType.Decoded
                                });
                            }
                            catch (Exception ex)
                            {
                                Console.Error.WriteLine($"Error retrieving text for quad id {element.quad.id}: {ex.Message}");
                            }
                            break;
                        case EnumLayoutElementSource.LES_INFERRED:
                            SetItem(r, c, new LayoutResultItem
                            {
                                Location = element.quad,
                                Type = LayoutResultItemType.Inferred
                            });
                            break;
                    }
                }
            }
        }

        private void SetItem(int row, int col, LayoutResultItem item)
        {
            if (!_items.ContainsKey(row))
                _items[row] = new Dictionary<int, LayoutResultItem>();
            _items[row][col] = item;
        }

        public void UpdateItem(int row, int col, string text, Quadrilateral quad)
        {
            lock (_lock)
            {
                if (!_items.TryGetValue(row, out var colMap) || !colMap.TryGetValue(col, out var item))
                    throw new ArgumentOutOfRangeException($"Cell ({row},{col}) not found.");
                item.Text = text;
                item.Location = quad;
                item.Type = LayoutResultItemType.Decoded;
            }
        }

        public void UpdateItemWithDecodeFailed(int row, int col)
        {
            lock (_lock)
            {
                if (!_items.TryGetValue(row, out var colMap) || !colMap.TryGetValue(col, out var item))
                    throw new ArgumentOutOfRangeException($"Cell ({row},{col}) not found.");
                item.Text = "";
                item.Type = LayoutResultItemType.DecodeFailed;
            }
        }

        public string ToJson()
        {
            int totalDecoded = 0, totalInferred = 0;
            var sb = new StringBuilder();
            bool first = true;

            foreach (int r in _items.Keys.OrderBy(k => k))
            {
                foreach (int c in _items[r].Keys.OrderBy(k => k))
                {
                    var item = _items[r][c];
                    string status;
                    if (item.Type == LayoutResultItemType.Decoded || item.Type == LayoutResultItemType.Inferred)
                    {
                        status = "Decoded";
                        totalDecoded++;
                    }
                    else if (item.Type == LayoutResultItemType.DecodeFailed)
                    {
                        status = "Inferred";
                        totalInferred++;
                    }
                    else
                    {
                        status = "Failed";
                    }

                    if (!first) sb.Append(",");
                    first = false;
                    sb.Append($"\n\t\t{{ \"row\": {r + 1}, \"col\": {c + 1}, \"status\": \"{status}\", \"text\": \"{EscapeJson(item.Text)}\" }}");
                }
            }

            return $"{{\n\t\"totalDecoded\": {totalDecoded},\n\t\"totalInferred\": {totalInferred},\n\t\"grid\": [{sb}\n\t]\n}}";
        }

        private static string EscapeJson(string s)
        {
            var sb = new StringBuilder(s.Length);
            foreach (char c in s)
            {
                switch (c)
                {
                    case '"': sb.Append("\\\""); break;
                    case '\\': sb.Append("\\\\"); break;
                    case '\b': sb.Append("\\b"); break;
                    case '\f': sb.Append("\\f"); break;
                    case '\n': sb.Append("\\n"); break;
                    case '\r': sb.Append("\\r"); break;
                    case '\t': sb.Append("\\t"); break;
                    default:
                        if (c < 32) sb.Append($"\\u{(int)c:x4}");
                        else sb.Append(c);
                        break;
                }
            }
            return sb.ToString();
        }
    }

    internal struct DeepDecodeTask
    {
        public int Row;
        public int Col;
        public LayoutResultItem Item;
    }
    #endregion

    #region ImageShower
    internal sealed class ImageShower : IDisposable
    {
        private static readonly Lazy<ImageShower> _instance =
            new(() => new ImageShower("Grid Barcode Scanner", 30));
        public static ImageShower Instance => _instance.Value;

        private readonly string _winName;
        private string _windowTitle;
        private readonly int _delayMs;
        private Mat _img = new Mat();
        private readonly object _mutex = new();
        private volatile bool _running;
        private readonly Thread? _thread;
        private readonly bool _headless;

        private static bool IsHeadless()
        {
            if (OperatingSystem.IsMacOS())
                return true;
            if (OperatingSystem.IsLinux())
            {
                string? display = Environment.GetEnvironmentVariable("DISPLAY");
                return string.IsNullOrEmpty(display);
            }
            return false;
        }

        private ImageShower(string windowName, int delayMs)
        {
            _winName = windowName;
            _windowTitle = windowName;
            _delayMs = delayMs;
            _headless = IsHeadless();
            if (_headless)
            {
                _running = false;
                _thread = null;
            }
            else
            {
                _running = true;
                _thread = new Thread(Loop) { IsBackground = true };
                _thread.Start();
            }
        }

        public void Update(Mat img, string title)
        {
            if (_headless) return;
            lock (_mutex)
            {
                img.CopyTo(_img);
                _windowTitle = title;
            }
        }

        public void Stop()
        {
            if (_headless) return;
            _running = false;
            if (_thread != null && _thread.IsAlive) _thread.Join();
        }

        public void Dispose() => Stop();

        private void Loop()
        {
            Cv2.NamedWindow(_winName, WindowFlags.Normal);
            Cv2.ResizeWindow(_winName, 800, 600);
            string lastTitle = _windowTitle;

            while (_running)
            {
                Mat? img = null;
                string title;

                lock (_mutex)
                {
                    if (!_img.Empty())
                    {
                        img = _img.Clone();
                        _img = new Mat();
                    }
                    title = _windowTitle;
                }

                if (img != null && !img.Empty())
                {
                    if (title != lastTitle)
                    {
                        Cv2.SetWindowTitle(_winName, title);
                        lastTitle = title;
                    }
                    var rect = Cv2.GetWindowImageRect(_winName);
                    int winW = rect.Width > 0 ? rect.Width : 800;
                    int winH = rect.Height > 0 ? rect.Height : 600;
                    double scale = Math.Min((double)winW / img.Cols, (double)winH / img.Rows);
                    int newW = (int)(img.Cols * scale);
                    int newH = (int)(img.Rows * scale);
                    Cv2.ResizeWindow(_winName, newW, newH);
                    Cv2.ImShow(_winName, img);
                    img.Dispose();
                }

                int key = Cv2.WaitKey(_delayMs);
                if (key == 27) _running = false;
                Thread.Sleep(1);
            }
        }
    }
    #endregion

    #region ImageHelper
    internal class ImageHelper
    {
        private readonly Mat _img;
        private readonly string _imagePath;

        public int Width => _img.Cols;
        public int Height => _img.Rows;

        public ImageHelper(string path)
        {
            _img = Cv2.ImRead(path, ImreadModes.Color);
            if (_img.Empty())
                throw new Exception($"Failed to read image: {path}");
            _imagePath = path;
        }

        private ImageHelper(Mat mat, string path)
        {
            _img = mat;
            _imagePath = path;
        }

        public static void CreateResultDir(string imgPath)
        {
            string stem = Path.GetFileNameWithoutExtension(imgPath);
            Directory.CreateDirectory(Path.Combine(Config.ResultDir, stem));
        }

        public string GetResultDir()
        {
            string stem = Path.GetFileNameWithoutExtension(_imagePath);
            return Path.Combine(Config.ResultDir, stem);
        }

        public ImageData ToImageData()
        {
            Mat src = _img.IsContinuous() ? _img : _img.Clone();
            byte[] bytes = new byte[src.Total() * src.ElemSize()];
            System.Runtime.InteropServices.Marshal.Copy(src.Data, bytes, 0, bytes.Length);
            return new ImageData(
                bytes,
                src.Cols,
                src.Rows,
                (int)src.Step(),
                EnumImagePixelFormat.IPF_BGR_888);
        }

        public void SaveResultImage(CommonDecodeResult result)
        {
            var img = DrawSolidQuads(result.Locations, new Scalar(0, 255, 0, 255), 2);
            ImageShower.Instance.Update(img._img, "Grid Barcode Scanner [1/4] Fast Scan");
            img.SaveToResultFile("_phase1");
        }

        public void SaveResultImage(LayoutAnalysisResult layoutResult)
        {
            int rows = layoutResult.elements.GetLength(0);
            int cols = layoutResult.elements.GetLength(1);
            var decodeQuads = new List<Quadrilateral>();
            var inferredQuads = new List<Quadrilateral>();
            for (int r = 0; r < rows; r++)
                for (int c = 0; c < cols; c++)
                {
                    var el = layoutResult.elements[r, c];
                    if (el.source == EnumLayoutElementSource.LES_INPUT) decodeQuads.Add(el.quad);
                    else if (el.source == EnumLayoutElementSource.LES_INFERRED) inferredQuads.Add(el.quad);
                }
            var decoded = DrawSolidQuads(decodeQuads, new Scalar(0, 255, 0, 255), 2);
            var inferred = decoded.DrawDashedQuads(inferredQuads, new Scalar(0, 0, 255, 255), 10, 4);
            ImageShower.Instance.Update(inferred._img, "Grid Barcode Scanner [2/4] Layout Analysis");
            inferred.SaveToResultFile("_phase2");
        }

        public void SaveResultImage(GridResult result)
        {
            var (decoded, inferred, undecoded) = GetQuadLists(result);
            var img1 = DrawSolidQuads(decoded, new Scalar(0, 255, 0, 255), 2);
            var img2 = img1.DrawSolidQuads(inferred, new Scalar(0, 255, 0, 255), 2);
            var img3 = img2.DrawDashedQuads(undecoded, new Scalar(0, 0, 255, 255), 10, 4);
            ImageShower.Instance.Update(img3._img, "Grid Barcode Scanner [3/4] Deep Decode");
            img3.SaveToResultFile("_phase3");
        }

        public void SaveFinalResultImage(GridResult result)
        {
            var (decoded, inferred, undecoded) = GetQuadLists(result);
            var img1 = DrawSolidQuads(decoded, new Scalar(0, 255, 0, 255), 2);
            var img2 = img1.DrawSolidQuads(inferred, new Scalar(0, 255, 0, 255), 2);
            var img3 = img2.DrawDashedQuads(undecoded, new Scalar(0, 0, 255, 255), 10, 4);

            foreach (var rowPair in result.Items)
                foreach (var colPair in rowPair.Value)
                {
                    var item = colPair.Value;
                    if (item.Type == LayoutResultItemType.DecodeFailed) continue;
                    var pt = GetTopLeft(item.Location);
                    img3.DrawText(item.Text, pt, new Scalar(0, 255, 0, 255), 0.6);
                }

            ImageShower.Instance.Update(img3._img, "Grid Barcode Scanner [4/4] Final Result");
            img3.SaveToResultFile("_final");
        }

        private static (List<Quadrilateral> decoded, List<Quadrilateral> inferred, List<Quadrilateral> undecoded)
            GetQuadLists(GridResult result)
        {
            var decoded = new List<Quadrilateral>();
            var inferred = new List<Quadrilateral>();
            var undecoded = new List<Quadrilateral>();
            foreach (var rowPair in result.Items)
                foreach (var colPair in rowPair.Value)
                {
                    var item = colPair.Value;
                    if (item.Type == LayoutResultItemType.Decoded) decoded.Add(item.Location);
                    else if (item.Type == LayoutResultItemType.Inferred) inferred.Add(item.Location);
                    else if (item.Type == LayoutResultItemType.DecodeFailed)
                        undecoded.Add(ExpandQuad(item.Location, Config.ScaleFactor));
                }
            return (decoded, inferred, undecoded);
        }

        private void SaveToResultFile(string suffix)
        {
            string stem = Path.GetFileNameWithoutExtension(_imagePath);
            string ext = Path.GetExtension(_imagePath);
            string path = Path.Combine(Config.ResultDir, stem, stem + suffix + ext);
            Cv2.ImWrite(path, _img);
        }

        private ImageHelper DrawSolidQuads(List<Quadrilateral> quads, Scalar color, int thickness)
        {
            var dst = _img.Clone();
            var pts = quads.Select(q => q.points
                .Select(p => new OpenCvSharp.Point(p[0], p[1])).ToArray()).ToArray();
            Cv2.Polylines(dst, pts, true, color, thickness, LineTypes.Link8);
            return new ImageHelper(dst, _imagePath);
        }

        private ImageHelper DrawDashedQuads(List<Quadrilateral> quads, Scalar color, int dashLength, int thickness)
        {
            var dst = _img.Clone();
            foreach (var quad in quads)
                for (int i = 0; i < 4; i++)
                {
                    var p1 = new OpenCvSharp.Point(quad.points[i][0], quad.points[i][1]);
                    var p2 = new OpenCvSharp.Point(quad.points[(i + 1) % 4][0], quad.points[(i + 1) % 4][1]);
                    DrawDashedLine(dst, p1, p2, color, dashLength, thickness);
                }
            return new ImageHelper(dst, _imagePath);
        }

        private static void DrawDashedLine(Mat dst, OpenCvSharp.Point p1, OpenCvSharp.Point p2, Scalar color, int dashLength, int thickness)
        {
            var it = new LineIterator(dst, p1, p2, PixelConnectivity.Connectivity8);
            bool draw = true;
            int count = 0;
            foreach (var pt in it)
            {
                if (draw)
                    Cv2.Circle(dst, pt.Pos, thickness / 2,
                        new Scalar(color[0], color[1], color[2]), -1, LineTypes.AntiAlias);
                count++;
                if (count == dashLength) { count = 0; draw = !draw; }
            }
        }

        private static OpenCvSharp.Point GetTopLeft(Quadrilateral quad)
        {
            int x = quad.points[0][0], y = quad.points[0][1];
            for (int i = 1; i < 4; i++)
            {
                if (quad.points[i][0] < x) x = quad.points[i][0];
                if (quad.points[i][1] < y) y = quad.points[i][1];
            }
            return new OpenCvSharp.Point(x, y);
        }

        private void DrawText(string text, OpenCvSharp.Point org, Scalar color, double scale)
        {
            Cv2.PutText(_img, text, org, HersheyFonts.HersheySimplex, scale, color, 2, LineTypes.AntiAlias);
        }

        public static Quadrilateral ExpandQuad(Quadrilateral quad, float scale)
        {
            float cx = (float)quad.points.Average(p => p[0]);
            float cy = (float)quad.points.Average(p => p[1]);
            var result = new Quadrilateral();
            result.points = new Dynamsoft.Core.Point[4];
            for (int i = 0; i < 4; i++)
            {
                result.points[i] = new Dynamsoft.Core.Point(
                    (int)(cx + (quad.points[i][0] - cx) * scale),
                    (int)(cy + (quad.points[i][1] - cy) * scale));
            }
            return result;
        }
    }
    #endregion

    #region Scanner Logic
    internal static class Scanner
    {
        public static InputParams Welcome()
        {
            var p = new InputParams();
            Console.WriteLine("Grid Barcode Scanner!");
            Console.WriteLine("===========================");
            Console.WriteLine();
            Console.WriteLine("Image path : [press Enter to use sample image (sample_grid.png)]");
            Console.WriteLine("'Q'/'q' to quit");
            string? input = Console.ReadLine();
            if (input == "Q" || input == "q") { p.Exit = true; return p; }
            if (string.IsNullOrEmpty(input))
                p.ImagePath = Config.SampleImagePath;
            else
            {
                if (input.Length >= 2 && input[0] == '"' && input[^1] == '"')
                    input = input[1..^1];
                p.ImagePath = input;
            }

            return p;
        }

        public static CommonDecodeResult CommonDecode(ImageHelper image)
        {
            using var cvRouter = new CaptureVisionRouter();
            int err = cvRouter.InitSettingsFromFile(Config.LightweightTemplatePath, out string errMsg);
            if (err != (int)EnumErrorCode.EC_OK)
            {
                Console.WriteLine($"Failed to init settings: {err}, {errMsg}");
                return new CommonDecodeResult(err);
            }

            var sw = System.Diagnostics.Stopwatch.StartNew();
            using var imageData = image.ToImageData();
            var result = cvRouter.Capture(imageData, Config.LightweightTemplateName);
            sw.Stop();

            int errCode = result.GetErrorCode();
            if (errCode == (int)EnumErrorCode.EC_UNSUPPORTED_JSON_KEY_WARNING)
                Console.WriteLine($"Common decode warning: {errCode}, {result.GetErrorString()}");
            else if (errCode != (int)EnumErrorCode.EC_OK && errCode != (int)EnumErrorCode.EC_TIMEOUT)
            {
                Console.WriteLine($"Common decode error: {errCode}, {result.GetErrorString()}");
                return new CommonDecodeResult(errCode);
            }

            var decodeResult = new CommonDecodeResult();
            foreach (var item in result.GetItems())
            {
                if (item is BarcodeResultItem barcode)
                {
                    decodeResult.Locations.Add(barcode.GetLocation());
                    decodeResult.Texts.Add(barcode.GetText());
                }
            }
            Console.WriteLine($"[Phase 1] Fast scan: {decodeResult.Texts.Count} barcodes decoded in {sw.ElapsedMilliseconds}ms.");
            return decodeResult;
        }

        public static LayoutAnalysisResult? Analyze(ImageHelper imageHelper, CommonDecodeResult decodeResult)
        {
            var layoutParam = new LayoutAnalysisParameter
            {
                pattern = EnumLayoutPattern.LP_MATRIX,
                inputImageWidth = imageHelper.Width,
                inputImageHeight = imageHelper.Height
            };

            Quadrilateral[] quads = new Quadrilateral[decodeResult.Locations.Count];
            for (int i = 0; i < decodeResult.Locations.Count; i++)
                quads[i] = decodeResult.Locations[i];
            var layoutResult = LayoutAnalyzer.Analyze(quads, layoutParam);

            if (layoutResult == null || layoutResult.errorCode != (int)EnumErrorCode.EC_OK)
            {
                Console.WriteLine($"Layout analysis failed: {layoutResult?.errorCode ?? -1}");
                layoutResult?.Dispose();
                return null;
            }

            int rows = layoutResult.elements.GetLength(0);
            int cols = layoutResult.elements.GetLength(1);
            int inferred = layoutResult.inferredQuads?.Length ?? 0;
            Console.WriteLine();
            Console.WriteLine($"[Phase 2] Layout analysis: {rows * cols} grid positions ({rows}x{cols}). {inferred} inferred regions.");
            return layoutResult;
        }

        public static void DeepDecode(ImageHelper image, GridResult gridResult)
        {
            var tasks = new List<DeepDecodeTask>();
            foreach (var rowPair in gridResult.Items)
                foreach (var colPair in rowPair.Value)
                    if (colPair.Value.Type == LayoutResultItemType.Inferred)
                        tasks.Add(new DeepDecodeTask { Row = rowPair.Key, Col = colPair.Key, Item = colPair.Value });

            if (tasks.Count == 0) return;

            var sw = System.Diagnostics.Stopwatch.StartNew();
            int numThreads = Math.Max(1, Environment.ProcessorCount);
            int idx = 0;
            var threads = new List<Thread>();

            for (int t = 0; t < numThreads; t++)
            {
                var th = new Thread(() =>
                {
                    while (true)
                    {
                        int i = Interlocked.Increment(ref idx) - 1;
                        if (i >= tasks.Count) break;
                        var task = tasks[i];
                        try { DeepDecodeInner(image, gridResult, task); }
                        catch (Exception ex)
                        {
                            Console.Error.WriteLine($"Deep decode error at ({task.Row},{task.Col}): {ex.Message}");
                        }
                    }
                });
                th.Start();
                threads.Add(th);
            }
            foreach (var th in threads) th.Join();
            sw.Stop();

            int decoded = tasks.Count(t2 =>
                gridResult.Items.TryGetValue(t2.Row, out var col) &&
                col.TryGetValue(t2.Col, out var item) &&
                item.Type != LayoutResultItemType.DecodeFailed);
            Console.WriteLine();
            Console.WriteLine($"[Phase 3] Deep decode: {decoded} / {tasks.Count} inferred regions decoded in {sw.ElapsedMilliseconds}ms.");
        }

        private static void DeepDecodeInner(ImageHelper image, GridResult gridResult, DeepDecodeTask task)
        {
            using var cvRouter = new CaptureVisionRouter();
            int err = cvRouter.InitSettingsFromFile(Config.DeepDecodeTemplatePath, out string errMsg);
            if (err != (int)EnumErrorCode.EC_OK)
            {
                gridResult.UpdateItemWithDecodeFailed(task.Row, task.Col);
                return;
            }

            err = cvRouter.GetSimplifiedSettings(Config.DeepDecodeTemplateName, out var settings);
            if (err != (int)EnumErrorCode.EC_OK)
            {
                gridResult.UpdateItemWithDecodeFailed(task.Row, task.Col);
                return;
            }

            settings.roi = ImageHelper.ExpandQuad(task.Item.Location, Config.ScaleFactor);
            settings.roiMeasuredInPercentage = 0;
            err = cvRouter.UpdateSettings(Config.DeepDecodeTemplateName, settings, out errMsg);
            if (err != (int)EnumErrorCode.EC_OK)
            {
                gridResult.UpdateItemWithDecodeFailed(task.Row, task.Col);
                return;
            }

            using var imageData = image.ToImageData();
            var result = cvRouter.Capture(imageData, Config.DeepDecodeTemplateName);
            if (result.GetErrorCode() != (int)EnumErrorCode.EC_OK)
            {
                gridResult.UpdateItemWithDecodeFailed(task.Row, task.Col);
                return;
            }

            foreach (var item in result.GetItems())
            {
                if (item is BarcodeResultItem barcode)
                {
                    string? text = barcode.GetText();
                    if (text != null)
                        gridResult.UpdateItem(task.Row, task.Col, text, barcode.GetLocation());
                    else
                        gridResult.UpdateItemWithDecodeFailed(task.Row, task.Col);
                    return;
                }
            }
            gridResult.UpdateItemWithDecodeFailed(task.Row, task.Col);
        }

        public static void PrintAndSaveResult(GridResult gridResult, string jsonPath)
        {
            Console.WriteLine("============================================================");
            Console.WriteLine($" {"Row",-4} | {"Col",-5} | {"Status",-13} | Text");
            Console.WriteLine("============================================================");

            int total = 0, decoded = 0;
            foreach (var rowPair in gridResult.Items.OrderBy(r => r.Key))
            {
                foreach (var colPair in rowPair.Value.OrderBy(c => c.Key))
                {
                    total++;
                    var item = colPair.Value;
                    string status = item.Type switch
                    {
                        LayoutResultItemType.DecodeFailed => "Inferred",
                        LayoutResultItemType.Decoded or LayoutResultItemType.Inferred => "Decoded",
                        _ => "Unknown"
                    };
                    if (item.Type != LayoutResultItemType.DecodeFailed) decoded++;
                    Console.WriteLine($"  {rowPair.Key + 1,-3} | {colPair.Key + 1,-5} | {status,-13} | {item.Text}");
                }
            }
            Console.WriteLine("============================================================");
            Console.WriteLine($"[Done] Total decoded: {decoded} / {total}.");

            try
            {
                File.WriteAllText(jsonPath, gridResult.ToJson());
                Console.WriteLine("Detailed results saved to result.json.");
            }
            catch
            {
                Console.Error.WriteLine("Failed to save detailed results to result.json.");
            }
        }
    }
    #endregion

    internal class Program
    {
        static void Main(string[] args)
        {
            int errorCode = LicenseManager.InitLicense("DLS2eyJvcmdhbml6YXRpb25JRCI6IjIwMDAwMSJ9", out string errorMsg);
            if (errorCode != (int)EnumErrorCode.EC_OK && errorCode != (int)EnumErrorCode.EC_LICENSE_WARNING)
            {
                Console.WriteLine($"License initialization failed: {errorCode}, {errorMsg}");
                return;
            }

            while (true)
            {
                try
                {
                    var param = Scanner.Welcome();
                    if (param.Exit)
                        break;

                    var imageHelper = new ImageHelper(param.ImagePath);
                    ImageHelper.CreateResultDir(param.ImagePath);

                    // Phase 1: Fast scan
                    var commonResult = Scanner.CommonDecode(imageHelper);
                    if (commonResult.Error != (int)EnumErrorCode.EC_OK)
                        continue;
                    commonResult.PrepareForLayoutAnalysis();
                    imageHelper.SaveResultImage(commonResult);

                    Console.Write(" Press Enter for Layout Analysis...");
                    Console.ReadLine();

                    // Phase 2: Layout analysis
                    var layoutResult = Scanner.Analyze(imageHelper, commonResult);
                    if (layoutResult == null)
                        continue;

                    var gridResult = new GridResult(commonResult, layoutResult);
                    imageHelper.SaveResultImage(layoutResult);

                    Console.Write(" Press Enter for Deep Decode...");
                    Console.ReadLine();

                    // Phase 3: Deep decode
                    Scanner.DeepDecode(imageHelper, gridResult);
                    imageHelper.SaveResultImage(gridResult);

                    Console.Write(" Press Enter to view final result...");
                    Console.ReadLine();

                    // Phase 4: Final result
                    string jsonPath = Path.Combine(imageHelper.GetResultDir(), "result.json");
                    Scanner.PrintAndSaveResult(gridResult, jsonPath);
                    imageHelper.SaveFinalResultImage(gridResult);

                    Console.WriteLine("Press Enter for next image (or 'Q'/'q' to quit)...");
                    string? next = Console.ReadLine();
                    if (next == "Q" || next == "q")
                        break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An error occurred: {ex.Message}");
                    break;
                }
            }
        }
    }
}
