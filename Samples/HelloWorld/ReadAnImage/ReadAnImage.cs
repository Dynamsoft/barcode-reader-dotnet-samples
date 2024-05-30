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
            errorCode = LicenseManager.InitLicense("DLS2eyJvcmdhbml6YXRpb25JRCI6IjIwMDAwMSJ9", out errorMsg);
            if (errorCode != (int)EnumErrorCode.EC_OK)
                Console.WriteLine("License initialization error: " + errorMsg);
            using (CaptureVisionRouter cvr = new CaptureVisionRouter())
            {
                string imageFile = "../../../../../../Images/AllSupportedBarcodeTypes.png";
                CapturedResult? result = cvr.Capture(imageFile, PresetTemplate.PT_READ_BARCODES);
                if (result == null)
                {
                    Console.WriteLine("No barcode detected.");
                }
                else if (result.GetErrorCode() != 0)
                {
                    Console.WriteLine("Error: " + result.GetErrorCode() + ", " + result.GetErrorString());
                }
                else
                {
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
            Console.WriteLine("Press any key to quit...");
            Console.Read();
        }
    }
}