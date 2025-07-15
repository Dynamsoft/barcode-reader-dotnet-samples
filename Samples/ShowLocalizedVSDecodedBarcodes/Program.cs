
using Dynamsoft.Core;
using Dynamsoft.CVR;
using Dynamsoft.DBR;
using Dynamsoft.DBR.intermediate_results;
using Dynamsoft.License;
using Dynamsoft.Utility;
using System;
using System.Collections.Generic;
using System.IO;

namespace ShowLocalizedVSDecodedBarcodes
{
    internal class MyIntermediateResultReceiver : IntermediateResultReceiver
    {
        public List<Quadrilateral> locations = new List<Quadrilateral>();

        public override void OnLocalizedBarcodesReceived(LocalizedBarcodesUnit result, IntermediateResultExtraInfo info)
        {
            if (info.isSectionLevelResult)
            {
                LocalizedBarcodeElement[] elements = result.GetLocalizedBarcodes();
                foreach (var element in elements)
                    locations.Add(element.GetLocation());
            }
        }
    }

    internal class Program
    {
        private static bool IsWithin10Percent(int area1, int area2)
        {
            if (area1 == 0 || area2 == 0)
                return false;
            double ratio = (double)Math.Abs(area1 - area2) / Math.Max(area1, area2);
            return ratio <= 0.10;
        }

        private static bool SeemAsSameLocation(Quadrilateral location, Quadrilateral resultLoc)
        {
            int x = 0, y = 0;
            foreach (var point in location.points)
            {
                x += point[0];
                y += point[1];
            }

            x = (x + location.points.Length / 2) / location.points.Length;
            y = (y + location.points.Length / 2) / location.points.Length;
            if (!resultLoc.Contains(new Point(x, y)))
                return false;
            if (IsWithin10Percent(location.GetArea(), resultLoc.GetArea()))
                return true;
            return false;
        }

        private static List<Quadrilateral> RemoveIfLocExistsInResultLocs(List<Quadrilateral> locations, List<Quadrilateral> resultLocs)
        {
            if (locations == null || resultLocs == null)
                return null;

            List<Quadrilateral> retLoc = new List<Quadrilateral>();
            HashSet<int> excludedLoc = new HashSet<int>();
            for (int i = 0; i < locations.Count; i++)
            {
                for (int j = 0; j < resultLocs.Count; j++)
                {
                    if (SeemAsSameLocation(locations[i], resultLocs[j]))
                    {
                        excludedLoc.Add(i);
                        break;
                    }
                }
            }
            for (int i = 0; i < locations.Count; i++)
            {
                if (!excludedLoc.Contains(i))
                    retLoc.Add(locations[i]);
            }

            return retLoc;
        }

        private static ImageData DrawOnImage(ImageData image, List<Quadrilateral> locations, List<Quadrilateral> resultLocs)
        {
            ImageDrawer drawer = new ImageDrawer();
            locations = RemoveIfLocExistsInResultLocs(locations, resultLocs);
            if (locations != null)
                image = drawer.DrawOnImage(image, locations.ToArray(), unchecked((int)0xFFFF0000), 2);
            if (resultLocs != null)
                image = drawer.DrawOnImage(image, resultLocs.ToArray(), unchecked((int)0xFF00FF00), 2);
            return image;
        }

        public static void Main(string[] args)
        {
            System.IO.Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);

            int errorCode = 1;
            string errorMsg;

            // 1. Initialize license.
            // You can request and extend a trial license from https://www.dynamsoft.com/customer/license/trialLicense?product=dcv&utm_source=samples&package=dotnet
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
                    IntermediateResultManager irm = cvRouter.GetIntermediateResultManager();
                    MyIntermediateResultReceiver irr = new MyIntermediateResultReceiver();
                    irm.AddResultReceiver(irr);

                    // 3. Replace by your own image path.
                    string imagePath = "../../../../../Images/GeneralBarcodes.png";
                    ImageIO imageIO = new ImageIO();

                    // 4. Read image from file.
                    ImageData image = imageIO.ReadFromFile(imagePath, out errorCode);
                    if (errorCode == (int)EnumErrorCode.EC_OK)
                    {
                        // 5. Decode barcodes from the image.
                        CapturedResult result = cvRouter.Capture(image, PresetTemplate.PT_READ_BARCODES);

                        // 6. Check error code.
                        if (result.GetErrorCode() == (int)EnumErrorCode.EC_UNSUPPORTED_JSON_KEY_WARNING)
                            Console.WriteLine("Warning: " + result.GetErrorCode() + ", " + result.GetErrorString());
                        else if (result.GetErrorCode() != (int)EnumErrorCode.EC_OK)
                            Console.WriteLine("Error: " + result.GetErrorCode() + ", " + result.GetErrorString());

                        List<Quadrilateral> resultLocs = new List<Quadrilateral>();
                        DecodedBarcodesResult decodedBarcodes = result.GetDecodedBarcodesResult();
                        BarcodeResultItem[] decodedItems = decodedBarcodes != null ? decodedBarcodes.GetItems() : null;
                        if (decodedItems != null && decodedItems.Length > 0)
                        {
                            foreach (BarcodeResultItem item in decodedItems)
                                resultLocs.Add(item.GetLocation());
                        }

                        // 7. Draw the outline border of the barcodes on the image.
                        // The barcodes that have been decode successfully are marked as green(0xFF00FF00)
                        // The barcodes that only have been localized are marked as red(0xFFFF0000)
                        if (irr.locations.Count > 0 || resultLocs.Count > 0)
                        {
                            ImageData imageComplete = DrawOnImage(image, irr.locations, resultLocs);

                            // 8. Save the image.
                            string resultPath = "result.png";
                            imageIO.SaveToFile(imageComplete, resultPath);
                            Console.WriteLine("Image saved to: " + Path.GetFullPath(resultPath));
                        }
                        else
                        {
                            Console.WriteLine("No barcodes detected.");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Failed to read image.");
                    }
                }
            }

            Console.WriteLine("Press Enter to quit...");
            Console.ReadLine();
        }
    }
}