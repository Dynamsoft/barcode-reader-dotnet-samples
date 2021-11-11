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
                // 1. Initialize license
                /*
                // By setting organization ID as "200001", a 7-day trial license will be used for license verification.
                // Note that network connection is required for this license to work.
                //
                // When using your own license, locate the following line and specify your Organization ID.
                // organizationID = "200001";
                //
                // If you don't have a license yet, you can request a trial from https://www.dynamsoft.com/customer/license/trialLicense?product=dbr&utm_source=samples&package=dotnet
                */
                DMDLSConnectionParameters connectionInfo = BarcodeReader.InitDLSConnectionParameters();
                connectionInfo.OrganizationID = "200001";
                EnumErrorCode errorCode = BarcodeReader.InitLicenseFromDLS(connectionInfo, out string errorMsg);
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
                            if (results[i].BarcodeFormat != EnumBarcodeFormat.BF_NULL)
                                Console.WriteLine("    Barcode Format: " + results[i].BarcodeFormatString);
                            else
                                Console.WriteLine("    Barcode Format: " + results[i].BarcodeFormatString_2);

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
            Console.ReadKey();
        }
    }
}
