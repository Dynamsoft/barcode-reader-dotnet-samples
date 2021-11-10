using System;
using Dynamsoft;
using Dynamsoft.DBR;

namespace ReadRateFirstSettings
{
    class Program
    {
        static void configReadRateFirst(BarcodeReader dbr)
        {
            PublicRuntimeSettings sts = dbr.GetRuntimeSettings();

            sts.BarcodeFormatIds = (int)EnumBarcodeFormat.BF_ALL;
            sts.BarcodeFormatIds_2 = (int)EnumBarcodeFormat_2.BF2_DOTCODE | (int)EnumBarcodeFormat_2.BF2_POSTALCODE;

            sts.ExpectedBarcodesCount = 64;

            sts.BinarizationModes[0] = EnumBinarizationMode.BM_LOCAL_BLOCK;
            sts.BinarizationModes[1] = EnumBinarizationMode.BM_THRESHOLD;
            sts.BinarizationModes[2] = EnumBinarizationMode.BM_SKIP;
            sts.BinarizationModes[3] = EnumBinarizationMode.BM_SKIP;
            sts.BinarizationModes[4] = EnumBinarizationMode.BM_SKIP;
            sts.BinarizationModes[5] = EnumBinarizationMode.BM_SKIP;
            sts.BinarizationModes[6] = EnumBinarizationMode.BM_SKIP;
            sts.BinarizationModes[7] = EnumBinarizationMode.BM_SKIP;

            sts.LocalizationModes[0] = EnumLocalizationMode.LM_CONNECTED_BLOCKS;
            sts.LocalizationModes[1] = EnumLocalizationMode.LM_SCAN_DIRECTLY;
            sts.LocalizationModes[2] = EnumLocalizationMode.LM_STATISTICS;
            sts.LocalizationModes[3] = EnumLocalizationMode.LM_LINES;
            sts.LocalizationModes[4] = EnumLocalizationMode.LM_STATISTICS_MARKS;
            sts.LocalizationModes[5] = EnumLocalizationMode.LM_STATISTICS_POSTAL_CODE;
            sts.LocalizationModes[6] = EnumLocalizationMode.LM_SKIP;
            sts.LocalizationModes[7] = EnumLocalizationMode.LM_SKIP;

            sts.DeblurModes[0] = EnumDeblurMode.DM_DIRECT_BINARIZATION;
            sts.DeblurModes[1] = EnumDeblurMode.DM_THRESHOLD_BINARIZATION;
            sts.DeblurModes[2] = EnumDeblurMode.DM_GRAY_EQUALIZATION;
            sts.DeblurModes[3] = EnumDeblurMode.DM_SMOOTHING;
            sts.DeblurModes[4] = EnumDeblurMode.DM_MORPHING;
            sts.DeblurModes[5] = EnumDeblurMode.DM_DEEP_ANALYSIS;
            sts.DeblurModes[6] = EnumDeblurMode.DM_SHARPENING;
            sts.DeblurModes[7] = EnumDeblurMode.DM_SKIP;
            sts.DeblurModes[8] = EnumDeblurMode.DM_SKIP;
            sts.DeblurModes[9] = EnumDeblurMode.DM_SKIP;

            sts.ScaleUpModes[0] = EnumScaleUpMode.SUM_AUTO;
            sts.ScaleUpModes[1] = EnumScaleUpMode.SUM_SKIP;
            sts.ScaleUpModes[2] = EnumScaleUpMode.SUM_SKIP;
            sts.ScaleUpModes[3] = EnumScaleUpMode.SUM_SKIP;
            sts.ScaleUpModes[4] = EnumScaleUpMode.SUM_SKIP;
            sts.ScaleUpModes[5] = EnumScaleUpMode.SUM_SKIP;
            sts.ScaleUpModes[6] = EnumScaleUpMode.SUM_SKIP;
            sts.ScaleUpModes[7] = EnumScaleUpMode.SUM_SKIP;

            sts.FurtherModes.GrayscaleTransformationModes[0] = EnumGrayscaleTransformationMode.GTM_ORIGINAL;
            sts.FurtherModes.GrayscaleTransformationModes[1] = EnumGrayscaleTransformationMode.GTM_INVERTED;
            sts.FurtherModes.GrayscaleTransformationModes[2] = EnumGrayscaleTransformationMode.GTM_SKIP;
            sts.FurtherModes.GrayscaleTransformationModes[3] = EnumGrayscaleTransformationMode.GTM_SKIP;
            sts.FurtherModes.GrayscaleTransformationModes[4] = EnumGrayscaleTransformationMode.GTM_SKIP;
            sts.FurtherModes.GrayscaleTransformationModes[5] = EnumGrayscaleTransformationMode.GTM_SKIP;
            sts.FurtherModes.GrayscaleTransformationModes[6] = EnumGrayscaleTransformationMode.GTM_SKIP;
            sts.FurtherModes.GrayscaleTransformationModes[7] = EnumGrayscaleTransformationMode.GTM_SKIP;

            sts.FurtherModes.DPMCodeReadingModes[0] = EnumDPMCodeReadingMode.DPMCRM_GENERAL;
            sts.FurtherModes.DPMCodeReadingModes[1] = EnumDPMCodeReadingMode.DPMCRM_SKIP;
            sts.FurtherModes.DPMCodeReadingModes[2] = EnumDPMCodeReadingMode.DPMCRM_SKIP;
            sts.FurtherModes.DPMCodeReadingModes[3] = EnumDPMCodeReadingMode.DPMCRM_SKIP;
            sts.FurtherModes.DPMCodeReadingModes[4] = EnumDPMCodeReadingMode.DPMCRM_SKIP;
            sts.FurtherModes.DPMCodeReadingModes[5] = EnumDPMCodeReadingMode.DPMCRM_SKIP;
            sts.FurtherModes.DPMCodeReadingModes[6] = EnumDPMCodeReadingMode.DPMCRM_SKIP;
            sts.FurtherModes.DPMCodeReadingModes[7] = EnumDPMCodeReadingMode.DPMCRM_SKIP;

            sts.Timeout = 30000;
            dbr.UpdateRuntimeSettings(sts);

        }

        static void configReadRateFirstByTemplate(BarcodeReader dbr) {
            string errorMessage;
            EnumErrorCode ret=dbr.InitRuntimeSettingsWithFile("ReadRateFirstTemplate.json", EnumConflictMode.CM_OVERWRITE,out errorMessage);
        }

        static void outputResults(TextResult[] results) {
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
                    configReadRateFirst(dbr);
                    results = dbr.DecodeFile(fileName, "");
                    outputResults(results);
                }

                Console.WriteLine("\r\n");

                Console.WriteLine("Decode through parameters template:");
                {
                    configReadRateFirstByTemplate(dbr);
                    results = dbr.DecodeFile(fileName, "");
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
