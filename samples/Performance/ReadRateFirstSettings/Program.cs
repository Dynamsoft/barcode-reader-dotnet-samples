using System;
using Dynamsoft;
using Dynamsoft.DBR;

namespace ReadRateFirstSettings
{
    class Program
    {
        static void configReadRateFirst(BarcodeReader dbr)
        {
            // Obtain current runtime settings of instance.
            PublicRuntimeSettings sts = dbr.GetRuntimeSettings();

            // Parameter 1. Set expected barcode formats
            // Here the barcode scanner will try to find the maximal barcode formats.
            sts.BarcodeFormatIds = (int)EnumBarcodeFormat.BF_ALL;
            sts.BarcodeFormatIds_2 = (int)(EnumBarcodeFormat_2.BF2_DOTCODE | EnumBarcodeFormat_2.BF2_POSTALCODE);

            // Parameter 2. Set expected barcode count.
            // Here the barcode scanner will try to find 64 barcodes.
            // If the result count does not reach the expected amount, the barcode scanner will try other algorithms in the setting list to find enough barcodes.
            sts.ExpectedBarcodesCount = 64;

            // Parameter 3. Set more binarization modes.
            sts.BinarizationModes[0] = EnumBinarizationMode.BM_LOCAL_BLOCK;
            sts.BinarizationModes[1] = EnumBinarizationMode.BM_THRESHOLD;

            // Parameter 4. Set more localization modes.
            // LocalizationModes are all enabled as default. Barcode reader will automatically switch between the modes and try decoding continuously until timeout or the expected barcode count is reached.
            // Please manually update the enabled modes list or change the expected barcode count to promote the barcode scanning speed.
            // Read more about localization mode members: https://www.dynamsoft.com/barcode-reader/parameters/enum/parameter-mode-enums.html?ver=latest#localizationmode
            sts.LocalizationModes[0] = EnumLocalizationMode.LM_CONNECTED_BLOCKS;
            sts.LocalizationModes[1] = EnumLocalizationMode.LM_SCAN_DIRECTLY;
            sts.LocalizationModes[2] = EnumLocalizationMode.LM_STATISTICS;
            sts.LocalizationModes[3] = EnumLocalizationMode.LM_LINES;
            sts.LocalizationModes[4] = EnumLocalizationMode.LM_STATISTICS_MARKS;
            sts.LocalizationModes[5] = EnumLocalizationMode.LM_STATISTICS_POSTAL_CODE;

            // Parameter 5. Set more deblur modes.
            // DeblurModes are all enabled as default. Barcode reader will automatically switch between the modes and try decoding continuously until timeout or the expected barcode count is reached.
            // Please manually update the enabled modes list or change the expected barcode count to promote the barcode scanning speed.
            //Read more about deblur mode members: https://www.dynamsoft.com/barcode-reader/parameters/enum/parameter-mode-enums.html#deblurmode
            sts.DeblurModes[0] = EnumDeblurMode.DM_DIRECT_BINARIZATION;
            sts.DeblurModes[1] = EnumDeblurMode.DM_THRESHOLD_BINARIZATION;
            sts.DeblurModes[2] = EnumDeblurMode.DM_GRAY_EQUALIZATION;
            sts.DeblurModes[3] = EnumDeblurMode.DM_SMOOTHING;
            sts.DeblurModes[4] = EnumDeblurMode.DM_MORPHING;
            sts.DeblurModes[5] = EnumDeblurMode.DM_DEEP_ANALYSIS;
            sts.DeblurModes[6] = EnumDeblurMode.DM_SHARPENING;

            // Parameter 6. Set scale up modes.
            // It is a parameter to control the process for scaling up an image used for detecting barcodes with small module size
            sts.ScaleUpModes[0] = EnumScaleUpMode.SUM_AUTO;
            
            // Parameter 7. Set grayscale transformation modes.
            // By default, the library can only locate the dark barcodes that stand on a light background. "GTM_INVERTED":The image will be transformed into inverted grayscale.
            sts.FurtherModes.GrayscaleTransformationModes[0] = EnumGrayscaleTransformationMode.GTM_ORIGINAL;
            sts.FurtherModes.GrayscaleTransformationModes[1] = EnumGrayscaleTransformationMode.GTM_INVERTED;

            // Parameter 8. Enable dpm modes.
            // It is a parameter to control how to read direct part mark (DPM) barcodes.
            sts.FurtherModes.DPMCodeReadingModes[0] = EnumDPMCodeReadingMode.DPMCRM_GENERAL;

            // Parameter 9. Increase timeout(ms). The barcode scanner will have more chances to find the expected barcode until it times out
            sts.Timeout = 30000;
            
            // Apply the new settings to the instance
            dbr.UpdateRuntimeSettings(sts);

        }

        static void configReadRateFirstByTemplate(BarcodeReader dbr) {
            // Compared with PublicRuntimeSettings, parameter templates have a richer ability to control parameter details.
		    // Please refer to the parameter explanation in "ReadRateFirstTemplate.json" to understand how to control read rate first.
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
                // 1.Initialize license.
		        // The string "DLS2eyJvcmdhbml6YXRpb25JRCI6IjIwMDAwMSJ9" here is a free public trial license. Note that network connection is required for this license to work.
	            // You can also request a 30-day trial license in the customer portal: https://www.dynamsoft.com/customer/license/trialLicense?product=dbr&utm_source=github&package=dotnet
                string errorMsg;
                EnumErrorCode errorCode = BarcodeReader.InitLicense("DLS2eyJvcmdhbml6YXRpb25JRCI6IjIwMDAwMSJ9", out errorMsg);
                if (errorCode != EnumErrorCode.DBR_SUCCESS)
                {
                    Console.WriteLine(errorMsg);
                }
                
                // Create an instance of Barcode Reader
                BarcodeReader dbr = new BarcodeReader();
                TextResult[] results = null;
                string fileName = "../../../../../images/AllSupportedBarcodeTypes.png";

                // There are two ways to configure runtime parameters. One is through PublicRuntimeSettings, the other is through parameters template.
                Console.WriteLine("Decode through PublicRuntimeSettings:");
                {
                    // config through PublicRuntimeSettings
                    configReadRateFirst(dbr);
                    
                    // Decode barcodes from an image file by current runtime settings. The second parameter value "" means to decode through the current PublicRuntimeSettings.
                    results = dbr.DecodeFile(fileName, "");
                    
                    // Output the barcode format and barcode text.
                    outputResults(results);
                }

                Console.WriteLine("\r\n");

                Console.WriteLine("Decode through parameters template:");
                {
                    // config through parameters template
                    configReadRateFirstByTemplate(dbr);
                    
                    // Decode barcodes from an image file by template.
                    results = dbr.DecodeFile(fileName, "");
                    
                    // Output the barcode format and barcode text.
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
