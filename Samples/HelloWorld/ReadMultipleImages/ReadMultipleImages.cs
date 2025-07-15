using Dynamsoft.Core;
using Dynamsoft.CVR;
using Dynamsoft.DBR;
using Dynamsoft.License;
using Dynamsoft.Utility;
using System;

namespace ReadMultipleImages
{
    class MyCapturedResultReceiver : CapturedResultReceiver
    {
        public override void OnDecodedBarcodesReceived(DecodedBarcodesResult result)
        {
            FileImageTag tag = (FileImageTag)result.GetOriginalImageTag();
            Console.WriteLine("File: " + tag.GetFilePath());
            if (result.GetErrorCode() == (int)EnumErrorCode.EC_UNSUPPORTED_JSON_KEY_WARNING)
            {
                Console.WriteLine("Warning: " + result.GetErrorCode() + ", " + result.GetErrorString());
            }
            else if (result.GetErrorCode() != (int)EnumErrorCode.EC_OK)
            {
                Console.WriteLine("Error: " + result.GetErrorString());
            }
            else
            {
                BarcodeResultItem[] items = result.GetItems();
                Console.WriteLine("Decoded " + items.Length + " barcodes");
                for (int i = 0; i < items.Length; i++)
                {
                    BarcodeResultItem item = items[i];
                    Console.WriteLine("Result " + (i + 1));
                    Console.WriteLine("Barcode Format: " + item.GetFormatString());
                    Console.WriteLine("Barcode Text: " + item.GetText());
                }
            }
            Console.WriteLine();
        }
    }
    class MyImageSourceStateListener : IImageSourceStateListener
    {
        private CaptureVisionRouter cvRouter = null;
        public MyImageSourceStateListener(CaptureVisionRouter cvRouter)
        {
            this.cvRouter = cvRouter;
        }

        public void OnImageSourceStateReceived(EnumImageSourceState state)
        {
            if (state == EnumImageSourceState.ISS_EXHAUSTED)
            {
                if (cvRouter != null)
                {
                    cvRouter.StopCapturing();
                }
            }
        }
    }
    internal class Program
    {
        static void Main(string[] args)
        {
            System.IO.Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);

            int errorCode = 1;
            string errorMsg;

            // Initialize license.
            // The string 'DLS2eyJvcmdhbml6YXRpb25JRCI6IjIwMDAwMSJ9' here is a free public trial license. Note that network connection is required for this license to work.
            // You can also request a 30-day trial license in the customer portal: https://www.dynamsoft.com/customer/license/trialLicense?product=dbr&utm_source=samples&package=dotnet
            errorCode = LicenseManager.InitLicense("DLS2eyJvcmdhbml6YXRpb25JRCI6IjIwMDAwMSJ9", out errorMsg);
            if (errorCode != (int)EnumErrorCode.EC_OK && errorCode != (int)EnumErrorCode.EC_LICENSE_WARNING)
            {
                Console.WriteLine("License initialization failed: ErrorCode: " + errorCode + ", ErrorString: " + errorMsg);
            }
            else
            {
                using (CaptureVisionRouter cvRouter = new CaptureVisionRouter())
                using (DirectoryFetcher fetcher = new DirectoryFetcher())
                {
                    fetcher.SetDirectory("../../../../../../Images");
                    cvRouter.SetInput(fetcher);

                    CapturedResultReceiver receiver = new MyCapturedResultReceiver();
                    cvRouter.AddResultReceiver(receiver);

                    MyImageSourceStateListener listener = new MyImageSourceStateListener(cvRouter);
                    cvRouter.AddImageSourceStateListener(listener);

                    errorCode = cvRouter.StartCapturing(PresetTemplate.PT_READ_BARCODES, true, out errorMsg);
                    if (errorCode != (int)EnumErrorCode.EC_OK)
                    {
                        Console.WriteLine("error: " + errorMsg);
                    }
                }
            }
            Console.WriteLine("Press any key to quit...");
            Console.Read();
        }
    }
}