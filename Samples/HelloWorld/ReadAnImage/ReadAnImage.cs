using Dynamsoft.DBR;
using Dynamsoft.Core;
using Dynamsoft.CVR;
using Dynamsoft.License;
namespace ReadAnImage
{
    internal class Program
    {
        static void Main(string[] args)
        {
            int errorCode = 1;
            string errorMsg;

            // Initialize license.
            // You can request and extend a trial license from https://www.dynamsoft.com/customer/license/trialLicense?product=dbr&utm_source=samples&package=dotnet
            // The string 'DLS2eyJvcmdhbml6YXRpb25JRCI6IjIwMDAwMSJ9' here is a free public trial license. Note that network connection is required for this license to work.
            errorCode = LicenseManager.InitLicense("DLS2eyJvcmdhbml6YXRpb25JRCI6IjIwMDAwMSJ9", out errorMsg);

            if (errorCode != (int)EnumErrorCode.EC_OK && errorCode != (int)EnumErrorCode.EC_LICENSE_CACHE_USED)
            {
                Console.WriteLine("License initialization failed: ErrorCode: " + errorCode + ", ErrorString: " + errorMsg);
            }
            else
            {
                using (CaptureVisionRouter cvr = new CaptureVisionRouter())
                {
                    string imageFile = "../../../../../../Images/GeneralBarcodes.png";
                    CapturedResult? result = cvr.Capture(imageFile, PresetTemplate.PT_READ_BARCODES);
                    if (result == null)
                    {
                        Console.WriteLine("No barcode detected.");
                    }
                    else
                    {
                        if (result.GetErrorCode() != 0)
                        {
                            Console.WriteLine("Error: " + result.GetErrorCode() + ", " + result.GetErrorString());
                        }

                        DecodedBarcodesResult? barcodesResult = result.GetDecodedBarcodesResult();
                        if (barcodesResult != null)
                        {
                            BarcodeResultItem[] items = barcodesResult.GetItems();
                            Console.WriteLine("Decoded " + items.Length + " barcodes");
                            foreach (BarcodeResultItem barcodeItem in items)
                            {
                                Console.WriteLine("Result " + (Array.IndexOf(items, barcodeItem) + 1));
                                Console.WriteLine("Barcode Format: " + barcodeItem.GetFormatString());
                                Console.WriteLine("Barcode Text: " + barcodeItem.GetText());
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