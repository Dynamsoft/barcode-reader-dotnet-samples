using System;
using Dynamsoft;
using Dynamsoft.DBR;

namespace HelloWorld
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
                string errorMsg;
                EnumErrorCode errorCode = BarcodeReader.InitLicense("DLS2eyJvcmdhbml6YXRpb25JRCI6IjIwMDAwMSJ9", out errorMsg);
                if (errorCode != EnumErrorCode.DBR_SUCCESS)
                {
                    Console.WriteLine(errorMsg);
                }

                // 2. Create an instance of Barcode Reader
                BarcodeReader dbr = new BarcodeReader();


                try
                {
                    TextResult[] results = null;

                    // 3. Read barcode from an image file
                    results = dbr.DecodeFile("../../../../images/AllSupportedBarcodeTypes.png", "");

                    if (results != null && results.Length > 0)
                    {
                        for (int i = 0; i < results.Length; ++i)
                        {
                            Console.WriteLine("Result " + (i + 1).ToString() + ":");

                            // 4. Get format of each barcode
                            Console.WriteLine("    Barcode Format: " + results[i].BarcodeFormatString);

                            // 5. Get text result of each barcode
                            Console.WriteLine("    Barcode Text: " + results[i].BarcodeText);
                        }
                    }
                    else
                    {
                        Console.WriteLine("No barcode detected.");
                    }
                }
                catch (BarcodeReaderException exp)
                {
                    Console.WriteLine(exp.Message);
                }
            }
            catch (Exception exp)
            {
                Console.WriteLine(exp.Message);
            }
            Console.WriteLine("Press any key to quit...");
            console.Read();
        }
    }
}
