using System;
using System.Drawing;
using System.IO;
using Dynamsoft;
using Dynamsoft.DBR;
using System.Windows.Forms;
using Dynamsoft.Core;


namespace ImageDecoding
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                DMDLSConnectionParameters connectionInfo = BarcodeReader.InitDLSConnectionParameters();
                connectionInfo.OrganizationID = "200001";
                EnumErrorCode errorCode = BarcodeReader.InitLicenseFromDLS(connectionInfo, out string errorMsg);
                if (errorCode != EnumErrorCode.DBR_SUCCESS)
                {
                    Console.WriteLine(errorMsg);
                }

                BarcodeReader dbr = new BarcodeReader();
             

                string filePath = "../../../../images/AllSupportedBarcodeTypes.png";
                TextResult[] results = null;

                PublicRuntimeSettings settings = dbr.GetRuntimeSettings();

                ImageCore ImageCore1 = new ImageCore();
                ImageCore1.IO.LoadImage(filePath);

                Bitmap bmp = (Bitmap)(ImageCore1.ImageBuffer.GetBitmap(ImageCore1.ImageBuffer.CurrentImageIndexInBuffer));
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
