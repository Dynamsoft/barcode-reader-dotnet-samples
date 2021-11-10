using System;
using Dynamsoft;
using Dynamsoft.DBR;

namespace AccuracyFirstSettings
{
    class Program
    {
        static public void configAccuracyFirst(ref BarcodeReader dbr)
        {
            PublicRuntimeSettings settings = dbr.GetRuntimeSettings();
            settings.BarcodeFormatIds = (int)EnumBarcodeFormat.BF_ONED;
            settings.BarcodeFormatIds_2 = (int)EnumBarcodeFormat.BF_NULL;
            settings.MinResultConfidence = 30;
            settings.MinBarcodeTextLength = 6;
            dbr.UpdateRuntimeSettings(settings);

        }
        static public void configAccuracyFirstByTemplate(ref BarcodeReader mBarcodeReader)
        {
            string strErrorMessage;
            EnumErrorCode ret = mBarcodeReader.InitRuntimeSettingsWithFile("AccuracyFirstTemplate.json", EnumConflictMode.CM_OVERWRITE, out strErrorMessage);
        }
        static public void outputResults(TextResult[] results)
        {
            if (results != null && results.Length > 0)
            {
                int i = 1;
                foreach (TextResult result in results)
                {
                    string barcodeFormat = result.BarcodeFormat == 0 ? result.BarcodeFormatString_2 : result.BarcodeFormatString;
                    string message = "Barcode" + i + ":" + barcodeFormat + result.BarcodeText;
                    Console.WriteLine(message);
                    i++;
                }
            }
            else
            {
                Console.WriteLine("No data detected.");
            }
        }

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
                TextResult[] results = null;
                string fileName = "../../../../../images/AllSupportedBarcodeTypes.png";

                Console.WriteLine("Decode through PublicRuntimeSettings:");
                {
                    configAccuracyFirst(ref dbr);
                    results = dbr.DecodeFile(fileName, "");
                    outputResults(results);
                }

                Console.WriteLine("\r\n");

                Console.WriteLine("Decode through parameters template:");
                {
                    configAccuracyFirstByTemplate(ref dbr);
                    results = dbr.DecodeFile(fileName,"");
                    outputResults(results);
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
 