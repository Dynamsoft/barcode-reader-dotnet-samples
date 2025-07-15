using Dynamsoft.DBR;
using Dynamsoft.CVR;
using Dynamsoft.License;
using Dynamsoft.Core;
using System;

namespace GeneralSettings
{
    internal class Program
    {
        static void Main(string[] args)
        {
            System.IO.Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);

            try
            {
                int errorCode = 1;
                string errorMsg;

                // 1. Initialize license.
                // The string 'DLS2eyJvcmdhbml6YXRpb25JRCI6IjIwMDAwMSJ9' here is a free public trial license. Note that network connection is required for this license to work.
                // You can also request a 30-day trial license in the customer portal: https://www.dynamsoft.com/customer/license/trialLicense?product=dbr&utm_source=samples&package=dotnet
                errorCode = LicenseManager.InitLicense("DLS2eyJvcmdhbml6YXRpb25JRCI6IjIwMDAwMSJ9", out errorMsg);

                if (errorCode != (int)EnumErrorCode.EC_OK && errorCode != (int)EnumErrorCode.EC_LICENSE_WARNING)
                    throw new Exception("License initialization failed: ErrorCode: " + errorCode + ", ErrorString: " + errorMsg);

                // 2. Create an instance of CaptureVisionRouter.
                using (CaptureVisionRouter cvRouter = new CaptureVisionRouter())
                {
                    // 3. General settings (including barcode format, barcode count and scan region) through SimplifiedCaptureVisionSettings
                    // 3.1 Obtain current runtime settings of instance.
                    SimplifiedCaptureVisionSettings settings = new SimplifiedCaptureVisionSettings();
                    errorCode = cvRouter.GetSimplifiedSettings(PresetTemplate.PT_READ_BARCODES, out settings);
                    if (errorCode != (int)EnumErrorCode.EC_OK)
                        throw new Exception("Get simplified settings failed, Error: " + errorCode);

                    // 3.2 Set the expected barcode format you want to read.
                    settings.barcodeSettings.barcodeFormatIds = (ulong)(EnumBarcodeFormat.BF_QR_CODE | EnumBarcodeFormat.BF_CODE_128);

                    // 3.3 Set the expected barcode count you want to read. 
                    settings.barcodeSettings.expectedBarcodesCount = 10;
                    
                    // 3.4 Set the grayscale transformation modes.
                    settings.barcodeSettings.grayscaleTransformationModes[0] = EnumGrayscaleTransformationMode.GTM_AUTO;
                    
                    // 3.5 Set the ROI(region of interest) to speed up the barcode reading process. 
                    // Note: DBR supports setting coordinates by pixels or percentages. The origin of the coordinate system is the upper left corner point.
                    settings.roiMeasuredInPercentage = 1;
                    settings.roi.points[0] = new Point(0, 0);
                    settings.roi.points[1] = new Point(100, 0);
                    settings.roi.points[2] = new Point(100, 100);
                    settings.roi.points[3] = new Point(0, 100);
                    
                    // 3.6 Apply the new settings to the instance.
                    errorCode = cvRouter.UpdateSettings(PresetTemplate.PT_READ_BARCODES, settings, out errorMsg);
                    if (errorCode != (int)EnumErrorCode.EC_OK)
                        throw new Exception("Update settings failed: ErrorCode: " + errorCode + ", ErrorString : " + errorMsg);
                    
                    // 4. Replace by your own image path
                    string imageFile = "../../../../../Images/GeneralBarcodes.png";

                    // 5. Decode barcodes from the image file.
                    CapturedResult[] results = cvRouter.CaptureMultiPages(imageFile, PresetTemplate.PT_READ_BARCODES);
                    if (results == null)
                    {
                        Console.WriteLine("No Captured result.");
                    }
                    else
                    {
                        for (int index = 0; index < results.Length; index++)
                        {
                            CapturedResult result = results[index];
                            if (result.GetErrorCode() == (int)EnumErrorCode.EC_UNSUPPORTED_JSON_KEY_WARNING)
                                Console.WriteLine("Warning: " + result.GetErrorCode() + "," + result.GetErrorString());
                            else if (result.GetErrorCode() != (int)EnumErrorCode.EC_OK)
                                Console.WriteLine("Error: " + result.GetErrorCode() + "," + result.GetErrorString());

                            // 6. Output the barcode text.
                            DecodedBarcodesResult barcodeResult = result.GetDecodedBarcodesResult();
                            if (barcodeResult == null || barcodeResult.GetItems().Length == 0)
                                Console.WriteLine("Page-" + (index + 1) + " No barcode detected.");
                            BarcodeResultItem[] items = barcodeResult.GetItems();
                            Console.WriteLine("Page-" + (index + 1) + " Decoded " + items.Length + " barcodes.");
                            for (int i = 0; i < items.Length; i++)
                            {
                                Console.WriteLine("Result " + (i + 1));
                                Console.WriteLine("Barcode Format: " + items[i].GetFormatString());
                                Console.WriteLine("Barcode Text: " + items[i].GetText());
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            Console.WriteLine("Press any key to quit...");
            Console.Read();
        }
    }
}
