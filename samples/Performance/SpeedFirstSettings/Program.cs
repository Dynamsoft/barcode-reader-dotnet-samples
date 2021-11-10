using System;
using Dynamsoft;
using Dynamsoft.DBR;

namespace SpeedFirstSettings
{
    class Program
    {
        static public void configSpeedFirst(ref BarcodeReader dbr)
        {
            PublicRuntimeSettings settings = dbr.GetRuntimeSettings();
            settings.BarcodeFormatIds = (int)EnumBarcodeFormat.BF_EAN_13;
            settings.ExpectedBarcodesCount = 1;
            settings.ScaleDownThreshold = 1200;

            settings.BinarizationModes[0] = EnumBinarizationMode.BM_LOCAL_BLOCK;
            settings.BinarizationModes[1] = EnumBinarizationMode.BM_SKIP;
            settings.BinarizationModes[2] = EnumBinarizationMode.BM_SKIP;
            settings.BinarizationModes[3] = EnumBinarizationMode.BM_SKIP;
            settings.BinarizationModes[4] = EnumBinarizationMode.BM_SKIP;
            settings.BinarizationModes[5] = EnumBinarizationMode.BM_SKIP;
            settings.BinarizationModes[6] = EnumBinarizationMode.BM_SKIP;
            settings.BinarizationModes[7] = EnumBinarizationMode.BM_SKIP;

            settings.LocalizationModes[0] = EnumLocalizationMode.LM_SCAN_DIRECTLY;
            settings.LocalizationModes[1] = EnumLocalizationMode.LM_SKIP;
            settings.LocalizationModes[2] = EnumLocalizationMode.LM_SKIP;
            settings.LocalizationModes[3] = EnumLocalizationMode.LM_SKIP;
            settings.LocalizationModes[4] = EnumLocalizationMode.LM_SKIP;
            settings.LocalizationModes[5] = EnumLocalizationMode.LM_SKIP;
            settings.LocalizationModes[6] = EnumLocalizationMode.LM_SKIP;
            settings.LocalizationModes[7] = EnumLocalizationMode.LM_SKIP;

            settings.DeblurModes[0] = EnumDeblurMode.DM_BASED_ON_LOC_BIN;
            settings.DeblurModes[1] = EnumDeblurMode.DM_THRESHOLD_BINARIZATION;
            settings.DeblurModes[2] = EnumDeblurMode.DM_SKIP;
            settings.DeblurModes[3] = EnumDeblurMode.DM_SKIP;
            settings.DeblurModes[4] = EnumDeblurMode.DM_SKIP;
            settings.DeblurModes[5] = EnumDeblurMode.DM_SKIP;
            settings.DeblurModes[6] = EnumDeblurMode.DM_SKIP;
            settings.DeblurModes[7] = EnumDeblurMode.DM_SKIP;
            settings.DeblurModes[8] = EnumDeblurMode.DM_SKIP;
            settings.DeblurModes[9] = EnumDeblurMode.DM_SKIP;

            settings.Timeout = 100;
            dbr.UpdateRuntimeSettings(settings);

            string strErrorMessage;
            dbr.SetModeArgument("BinarizationModes", 0, "EnableFillBinaryVacancy", "0", out strErrorMessage);
            dbr.SetModeArgument("LocalizationModes", 0, "ScanDirection", "0", out strErrorMessage);
        }
        static public void configSpeedFirstByTemplate(ref BarcodeReader dbr)
        {
            string strErrorMessage;
            EnumErrorCode ret = dbr.InitRuntimeSettingsWithFile("SpeedFirstTemplate.json", EnumConflictMode.CM_OVERWRITE, out strErrorMessage);
        }

        static public void outputResults(TextResult[] results, long costTime)
        {
            Console.WriteLine("Cost time:{0}ms", costTime);
            if (results != null && results.Length > 0)
            {
                int i = 1;
                foreach (TextResult result in results)
                {
                    string barcodeFormat = result.BarcodeFormat == 0 ? result.BarcodeFormatString_2 : result.BarcodeFormatString;
                    Console.WriteLine("Barcode {0}:{1},{2}", i, barcodeFormat, result.BarcodeText);
                    i++;
                }
            }
            else
            {
                Console.WriteLine("No data detected~");
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
                    configSpeedFirst(ref dbr);
                    DateTime beforeRead = DateTime.Now;
                    results = dbr.DecodeFile(fileName, "");
                    DateTime afterRead = DateTime.Now;
                    int timeElapsed = (int)(afterRead - beforeRead).TotalMilliseconds;
                    outputResults(results, timeElapsed);
                }
                
                Console.WriteLine("\r\n");

                Console.WriteLine("Decode through parameters template:");
                {
                    
                    configSpeedFirstByTemplate(ref dbr);
                    DateTime beforeRead = DateTime.Now;
                    results = dbr.DecodeFile(fileName, "");
                    DateTime afterRead = DateTime.Now;
                    int timeElapsed = (int)(afterRead - beforeRead).TotalMilliseconds;
                    outputResults(results, timeElapsed);
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
