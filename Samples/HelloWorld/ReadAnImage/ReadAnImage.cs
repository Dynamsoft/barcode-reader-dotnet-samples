using Dynamsoft.DBR;
using Dynamsoft.Core;
using Dynamsoft.CVR;
using Dynamsoft.License;
using System;

namespace ReadAnImage
{
    internal class Program
    {
        static void Main(string[] args)
        {
            System.IO.Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);

            int errorCode = 1;
            string errorMsg;

            // Initialize license.
            // The string 'DLS2eyJvcmdhbml6YXRpb25JRCI6IjIwMDAwMSJ9' here is a free public trial license. Note that network connection is required for this license to work.
            // You can also request a 30-day trial license in the customer portal: https://www.dynamsoft.com/customer/license/trialLicense?product=dbr&utm_source=samples&package=dotnet
            errorCode = LicenseManager.InitLicense("DLS2eyJvcmdhbml6YXRpb25JRCI6IjIwMDAwMSJ9", out errorMsg);

            if (errorCode != (int)EnumErrorCode.EC_OK && errorCode != (int)EnumErrorCode.EC_LICENSE_WARNING)
            {
                Console.WriteLine("License initialization failed: ErrorCode: " + errorCode + ", ErrorString: " + errorMsg);
            }
            else
            {
                using (CaptureVisionRouter cvRouter = new CaptureVisionRouter())
                {
                    string imageFile = "../../../../../../Images/GeneralBarcodes.png";
                    CapturedResult[] results = cvRouter.CaptureMultiPages(imageFile, PresetTemplate.PT_READ_BARCODES);
                    if (results == null)
                    {
                        Console.WriteLine("No barcode detected.");
                    }
                    else
                    {
                        for (int index = 0; index < results.Length; index++)
                        {
                            CapturedResult result = results[index];
                            if (result.GetErrorCode() == (int)EnumErrorCode.EC_UNSUPPORTED_JSON_KEY_WARNING)
                            {
                                Console.WriteLine("Warning: " + result.GetErrorCode() + ", " + result.GetErrorString());
                            }
                            if (result.GetErrorCode() != (int)EnumErrorCode.EC_OK)
                            {
                                Console.WriteLine("Error: " + result.GetErrorCode() + ", " + result.GetErrorString());
                            }
                            DecodedBarcodesResult barcodesResult = result.GetDecodedBarcodesResult();
                            BarcodeResultItem[] items = barcodesResult != null ? barcodesResult.GetItems() : null;
                            if (items == null || items.Length == 0)
                            {
                                Console.WriteLine("Page-" + (index + 1) + " No barcode detected.");
                            }
                            else
                            {
                                Console.WriteLine("Page-" + (index + 1) + " Decoded " + items.Length + " barcodes");
                                for (int i = 0; i < items.Length; i++)
                                {
                                    BarcodeResultItem barcodeResultItem = items[i];
                                    Console.WriteLine("Result " + (i + 1));
                                    Console.WriteLine("Barcode Format: " + barcodeResultItem.GetFormatString());
                                    Console.WriteLine("Barcode Text: " + barcodeResultItem.GetText());
                                }
                            }
                        }
                    }
                }
            }
            Console.WriteLine("Press any key to quit...");
            Console.Read();
        }
    }
}