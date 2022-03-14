using System;
using Dynamsoft;
using Dynamsoft.DBR;

namespace GeneralSettings
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

                // 2. Create an instance of Barcode Reader
                BarcodeReader dbr = new BarcodeReader();

                // 3. Configure settings

                // 3.1 Through PublicRuntimeSetting

                // 3.1.1 Call GetRuntimeSettings to get current runtime settings. 
                PublicRuntimeSettings settings = dbr.GetRuntimeSettings();

                // 3.1.2 Configure one or more specific settings
                // In this sample, we configure three settings: 
                // try to finnd PDF 417 and DotCode
                settings.BarcodeFormatIds = (int)EnumBarcodeFormat.BF_PDF417;
                settings.BarcodeFormatIds_2 = (int)EnumBarcodeFormat_2.BF2_DOTCODE;
                // try to find 2 barcodes
                settings.ExpectedBarcodesCount = 2;
                // try to find barcodes in the lower part of the image
                settings.Region.RegionLeft = 0;
                settings.Region.RegionRight = 100;
                settings.Region.RegionTop = 50;
                settings.Region.RegionBottom = 100;
                settings.Region.RegionMeasuredByPercentage = 1;

                // 3.1.3 Call UpdateRuntimeSettings to apply above settings
                dbr.UpdateRuntimeSettings(settings);
                
                // 3.2 Through JSON template
                //string errorMessage;
                //dbr.InitRuntimeSettingsWithString("{\"ImageParameter\":{\"Name\":\"S1\",\"RegionDefinitionNameArray\":[\"R1\"]},\"RegionDefinition\":{\"Name\":\"R1\",\"BarcodeFormatIds\":[\"BF_PDF417\"],\"BarcodeFormatIds_2\":[\"BF2_POSTALCODE\"],\"ExpectedBarcodesCount\":2,\"Left\":0,\"Right\":100,\"Top\":50,\"Bottom\":100,\"MeasuredByPercentage\":1}}", EnumConflictMode.CM_IGNORE, out errorMessage);


                try
                {
                    TextResult[] results = null;

                    // 4. Read barcode from an image file
                    results = dbr.DecodeFile("../../../../images/AllSupportedBarcodeTypes.png", "");

                    if (results != null && results.Length > 0)
                    {
                        for (int i = 0; i < results.Length; ++i)
                        {
                            Console.WriteLine("Result " + (i + 1).ToString() + ":");

                            // 5. Get format of each barcode
                            if (results[i].BarcodeFormat != EnumBarcodeFormat.BF_NULL)
                                Console.WriteLine("    Barcode Format: " + results[i].BarcodeFormatString);
                            else
                                Console.WriteLine("    Barcode Format: " + results[i].BarcodeFormatString_2);

                            // 6. Get text result of each barcode
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
