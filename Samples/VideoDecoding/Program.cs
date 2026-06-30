using Dynamsoft.Core;
using Dynamsoft.CVR;
using Dynamsoft.DBR;
using Dynamsoft.License;
using Dynamsoft.Utility;
using OpenCvSharp;
using System.Runtime.InteropServices;
namespace VideoDecoding
{
    internal class MyCapturedResultReceiver:CapturedResultReceiver
    {
        public override void OnDecodedBarcodesReceived(DecodedBarcodesResult result)
        {
            if (result.GetErrorCode() == (int)EnumErrorCode.EC_UNSUPPORTED_JSON_KEY_WARNING)
            {
                Console.WriteLine($"Warning: {result.GetErrorCode()}, {result.GetErrorString()}");
            }
            else if (result.GetErrorCode() != (int)EnumErrorCode.EC_OK)
            {
                Console.WriteLine($"Error: {result.GetErrorCode()}, {result.GetErrorString()}");
            }

            var tag = result.GetOriginalImageTag();
            var items = result.GetItems();
            if (items.Length > 0)
            {
                if (tag != null)
                {
                    Console.WriteLine($"ImageID: {tag.GetImageId()}");
                }
                Console.WriteLine($"Decoded {items.Length} barcodes");
                for (int i = 0; i < items.Length; i++)
                {
                    Console.WriteLine($"Result {i}:");
                    Console.WriteLine($"Barcode Format: {items[i].GetFormatString()}");
                    Console.WriteLine($"Barcode Text: {items[i].GetText()}");
                }
                Console.WriteLine();
            }
        }
    }
    internal class MyVideoFetcher : ImageSourceAdapter
    {
        public override bool HasNextImageToFetch()
        {
            return true;
        }
        public void MyAddImageToBuffer(ImageData imageData)
        {
            AddImageToBuffer(imageData);
        }
    }
    internal class Program
    {
        private static TimeSpan _intervalMs = TimeSpan.FromMilliseconds(33);
        static void Main(string[] args)
        {
            Console.WriteLine("Opening camera...");
            VideoCapture capture = new VideoCapture(0);
            if(!capture.IsOpened())
            {
                Console.WriteLine("ERROR: Can't initialize camera capture.");
                return;
            }

            int errorCode = 1;
            string? errorMsg;

            // 1. Initialize license.
            // You can request and extend a trial license from https://www.dynamsoft.com/customer/license/trialLicense?product=dbr&utm_source=samples&package=dotnet
            // The string 'DLS2eyJvcmdhbml6YXRpb25JRCI6IjIwMDAwMSJ9' here is a free public trial license. Note that network connection is required for this license to work.
            errorCode = LicenseManager.InitLicense("DLS2eyJvcmdhbml6YXRpb25JRCI6IjIwMDAwMSJ9", out errorMsg);
            if (errorCode != (int)EnumErrorCode.EC_OK && errorCode != (int)EnumErrorCode.EC_UNSUPPORTED_JSON_KEY_WARNING)
            {
                Console.WriteLine("License initialization failed: ErrorCode: " + errorCode + ", ErrorString: " + errorMsg);
            }
            else
            {
                // 2. Create an instance of CaptureVisionRouter.
                using (CaptureVisionRouter cvRouter = new CaptureVisionRouter())
                {
                    MyVideoFetcher fetcher = new MyVideoFetcher();
                    fetcher.SetMaxImageCount(100);
                    fetcher.SetBufferOverflowProtectionMode(EnumBufferOverflowProtectionMode.BOPM_UPDATE);
                    fetcher.SetColourChannelUsageType(EnumColourChannelUsageType.CCUT_AUTO);
                    errorCode = cvRouter.SetInput(fetcher);

                    MyCapturedResultReceiver crr = new MyCapturedResultReceiver();
                    cvRouter.AddResultReceiver(crr);

                    errorCode = cvRouter.StartCapturing(PresetTemplate.PT_READ_BARCODES, false, out errorMsg);
                    if(errorCode != (int)EnumErrorCode.EC_OK)
                    {
                        Console.WriteLine($"StartCapturing failed: ErrorCode: {errorCode}, ErrorString: {errorMsg}");
                        return;
                    }

                    var mat = new Mat();
                    int i= 0;
                    while (true)
                    {
                        i++;
                        var start = DateTime.Now;
                        capture.Read(mat);
                        if (mat.Empty())
                        {
                            Console.WriteLine("ERROR: Can't grab camera frame.");
                            continue;
                        }

                        int height = mat.Rows;
                        int width = mat.Cols;
                        int stride = (int)mat.Step();
                        int channels = mat.Channels();

                        int totalBytes = height * stride;
                        byte[] data = new byte[totalBytes];
                        Marshal.Copy(mat.Data, data, 0, totalBytes);
                        VideoFrameTag tag = new VideoFrameTag();
                        tag.SetImageId(i);
                        using ImageData imageData = new ImageData(
                            data,
                            width,
                            height,
                            stride,
                            EnumImagePixelFormat.IPF_RGB_888);
                        imageData.SetImageTag(tag);
                        fetcher.MyAddImageToBuffer(imageData);
                        Cv2.ImShow("Frame", mat);
                        var end = DateTime.Now;
                        var usingTime = end - start;
                        int key;
                        if(usingTime < _intervalMs)
                        {
                            key = Cv2.WaitKey((int)(_intervalMs - usingTime).TotalMilliseconds);
                        }
                        else
                        {
                            key = Cv2.WaitKey(1);
                        }
                        if (key == 27)
                            break;
                    }

                    mat.Dispose();
                    cvRouter.StopCapturing(false, true);
                }
            }
            Console.WriteLine("Press Enter to quit...");
            Console.ReadLine();
        }
    }
}
