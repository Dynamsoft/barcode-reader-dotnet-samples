using Dynamsoft.DBR;
using Dynamsoft.CVR;
using Dynamsoft.License;
using Dynamsoft.Core;
using System;

namespace ReadDPMBarcode
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

                if (errorCode != (int)EnumErrorCode.EC_OK && errorCode != (int)EnumErrorCode.EC_LICENSE_CACHE_USED)
                    throw new Exception("License initialization failed: ErrorCode: " + errorCode + ", ErrorString: " + errorMsg);

		        // 2. Create an instance of CCaptureVisionRouter.
                using (CaptureVisionRouter cvRouter = new CaptureVisionRouter())
                {
                    errorCode = cvRouter.InitSettingsFromFile("../../../../../CustomTemplates/ReadDPM.json", out errorMsg);
                    if(errorCode!=  (int)EnumErrorCode.EC_OK)
                        throw new Exception("Init settings from file failed: ErrorCode: " + errorCode + ", ErrorString: " + errorMsg);

                    // 3. Replace with your own dpm barcode image path.
                    string imageFile = "../../../../../Images/DPM.png";

                    // 4. Decode barcodes from the image file.
                    CapturedResult[] results = cvRouter.CaptureMultiPages(imageFile, "");
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

                            // 5. Output the barcode format and barcode text.
                            DecodedBarcodesResult barcodeResult = result.GetDecodedBarcodesResult();
                            BarcodeResultItem[] items = barcodeResult != null ? barcodeResult.GetItems() : null;
                            if (items == null || items.Length == 0)
                            {
                                Console.WriteLine("Page-" + (index + 1) + " No barcode found.");
                            }
                            else
                            {
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
