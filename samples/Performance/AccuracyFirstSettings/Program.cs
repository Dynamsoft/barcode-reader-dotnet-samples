using System;
using Dynamsoft;
using Dynamsoft.DBR;

namespace AccuracyFirstSettings
{
    class Program
    {
        static public void configAccuracyFirst(ref BarcodeReader dbr)
        {
            // Obtain current runtime settings of instance.
            PublicRuntimeSettings settings = dbr.GetRuntimeSettings();
            
            // 1. Set expected barcode format
            // The more precise the barcode format is set, the higher the accuracy.
            // Mostly, misreading only occurs on reading oneD barcode. So here we use OneD barcode format to demonstrate.
            settings.BarcodeFormatIds = (int)EnumBarcodeFormat.BF_ONED;
            settings.BarcodeFormatIds_2 = (int)EnumBarcodeFormat.BF_NULL;
            
            // 2. Set the minimal result confidence.
            // The greater the confidence, the higher the accuracy.
            // Filter by minimal confidence of the decoded barcode. We recommend using 30 as the default minimal confidence value
            settings.MinResultConfidence = 30;
            
            // 3. Sets the minimum length of barcode text for filtering.
            // The more precise the barcode text length is set, the higher the accuracy.
            settings.MinBarcodeTextLength = 6;
            
            // Apply the new settings to the instance
            dbr.UpdateRuntimeSettings(settings);

        }
        static public void configAccuracyFirstByTemplate(ref BarcodeReader mBarcodeReader)
        {
            // Compared with PublicRuntimeSettings, parameter templates have a richer ability to control parameter details.
		    // Please refer to the parameter explanation in "AccuracyFirstTemplate.json" to understand how to control accuracy first.
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
                    string barcodeFormat = result.BarcodeFormatString;
                    string message = "Barcode" + i + ":" + barcodeFormat + "," +result.BarcodeText;
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

                // Accuracy = The number of correctly decoded barcodes/the number of all decoded barcodes
		        // There are two ways to configure runtime parameters. One is through PublicRuntimeSettings, the other is through parameters template.
                Console.WriteLine("Decode through PublicRuntimeSettings:");
                {
                    // config through PublicRuntimeSettings
                    configAccuracyFirst(ref dbr);
                    
                    // Decode barcodes from an image file by current runtime settings. The second parameter value "" means to decode through the current PublicRuntimeSettings.
                    results = dbr.DecodeFile(fileName, "");
                    
                    // Output the barcode format and barcode text.
                    outputResults(results);
                }

                Console.WriteLine("\r\n");

                Console.WriteLine("Decode through parameters template:");
                {
                    // config through parameters template
                    configAccuracyFirstByTemplate(ref dbr);
                    
                    // Decode barcodes from an image file by template.
                    results = dbr.DecodeFile(fileName,"");
                    
                    // Output the barcode format and barcode text.
                    outputResults(results);
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
 
