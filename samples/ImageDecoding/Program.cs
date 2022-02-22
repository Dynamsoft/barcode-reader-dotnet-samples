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
                // Initialize license
                /*
                // By setting organization ID as "200001", a free public trial license will be used for license verification.
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
