using System;
using Dynamsoft;
using Dynamsoft.DBR;


namespace ReadDPMBarcode
{
    class Program
    {
        static void configReadDPMBarcode(BarcodeReader dbr)
        {
            // Obtain current runtime settings of instance.
            PublicRuntimeSettings sts = dbr.GetRuntimeSettings();

            // Set expected barcode formats. 
            // Generally, the most common dpm barcode is datamatrix or qrcode
            sts.BarcodeFormatIds = (int)(EnumBarcodeFormat.BF_DATAMATRIX | EnumBarcodeFormat.BF_QR_CODE);

            // Set grayscale transformation modes.
            // DPM barcodes may be dark and printed on the light background, or it may be light and printed on the dark background.
            // By default, the library can only locate the dark barcodes that stand on a light background. "GTM_INVERTED":The image will be transformed into inverted grayscale.
            sts.FurtherModes.GrayscaleTransformationModes[0] = EnumGrayscaleTransformationMode.GTM_ORIGINAL;
            sts.FurtherModes.GrayscaleTransformationModes[1] = EnumGrayscaleTransformationMode.GTM_INVERTED;

            // Enable dpm modes.
            // It is a parameter to control how to read direct part mark (DPM) barcodes.
            sts.FurtherModes.DPMCodeReadingModes[0] = EnumDPMCodeReadingMode.DPMCRM_GENERAL;

            // Apply the new settings to the instance
            dbr.UpdateRuntimeSettings(sts);
        }

        static void outputResults(TextResult[] results)
        {
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
                // Initialize license
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

                // Create an instance of Barcode Reader
                BarcodeReader dbr = new BarcodeReader();
                TextResult[] results = null;
                string fileName = "../../../../../images/dpm.jpg";

                // config through PublicRuntimeSettings
                configReadDPMBarcode(dbr);

                // Decode barcodes from an image file by current runtime settings. The second parameter value "" means to decode through the current PublicRuntimeSettings.
                results = dbr.DecodeFile(fileName, "");

                // Output the barcode format and barcode text.
                outputResults(results);
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

