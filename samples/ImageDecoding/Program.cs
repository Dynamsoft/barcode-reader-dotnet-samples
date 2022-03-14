using System;
using System.Drawing;
using System.IO;
using Dynamsoft;
using Dynamsoft.DBR;
using System.Windows.Forms;


namespace ImageDecoding
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                // 1.Initialize license.
		// The string "DLS2eyJvcmdhbml6YXRpb25JRCI6IjIwMDAwMSJ9" here is a free public trial license. Note that network connection is required for this license to work.
	        // You can also request a 30-day trial license in the customer portal: https://www.dynamsoft.com/customer/license/trialLicense?product=dbr&utm_source=github&package=dotnet
                EnumErrorCode errorCode = BarcodeReader.InitLicense("DLS2eyJvcmdhbml6YXRpb25JRCI6IjIwMDAwMSJ9", out string errorMsg);
                if (errorCode != EnumErrorCode.DBR_SUCCESS)
                {
                    Console.WriteLine(errorMsg);
                }
                
                // Create an instance of Barcode Reader
                BarcodeReader dbr = new BarcodeReader();
             

                string filePath = "../../../../images/AllSupportedBarcodeTypes.png";
                TextResult[] results = null;
                
                // Configure settings

                // Through PublicRuntimeSetting

                // Call GetRuntimeSettings to get current runtime settings.
                PublicRuntimeSettings settings = dbr.GetRuntimeSettings();

                Bitmap bmp = new Bitmap(filePath);
                
                results = dbr.DecodeBitmap(bmp, "");

                if (results != null && results.Length > 0)
                {
                    int i = 1;
                    foreach (TextResult result in results)
                    {
                        string barcodeFormat = result.BarcodeFormat == 0 ? result.BarcodeFormatString_2 : result.BarcodeFormatString;
                        string message = "Barcode" + i + ":" + barcodeFormat + "," + result.BarcodeText;
                        Console.WriteLine(message);
                        i++;
                    }
                }
                else
                {
                    Console.WriteLine("No data detected.");
                }
            }
            catch (Exception exp)
            {
                Console.WriteLine(exp.Message);
            }
            Console.WriteLine("Press any key to quit...");
            Console.ReadKey();
        }
    }
}
