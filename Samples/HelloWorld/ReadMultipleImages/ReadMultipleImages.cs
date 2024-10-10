using Dynamsoft.Core;
using Dynamsoft.CVR;
using Dynamsoft.DBR;
using Dynamsoft.License;
using Dynamsoft.Utility;

namespace ReadMultipleImages
{
    class MyCapturedResultReceiver : CapturedResultReceiver
    {
        public override void OnDecodedBarcodesReceived(DecodedBarcodesResult result)
        {
            FileImageTag? tag = (FileImageTag?)result.GetOriginalImageTag();
            Console.WriteLine("File: " + tag.GetFilePath());
            if (result.GetErrorCode() != (int)EnumErrorCode.EC_OK)
            {
                Console.WriteLine("Error: " + result.GetErrorString());
            }
            else
            {
                BarcodeResultItem[] items = result.GetItems();
                Console.WriteLine("Decoded " + items.Length + " barcodes");
                foreach (BarcodeResultItem item in items)
                {
                    Console.WriteLine("Result " + (Array.IndexOf(items, item) + 1));
                    Console.WriteLine("Barcode Format: " + item.GetFormatString());
                    Console.WriteLine("Barcode Text: " + item.GetText());
                }
            }
            Console.WriteLine();
        }
    }
    class MyImageSourceStateListener : IImageSourceStateListener
    {
        private CaptureVisionRouter? cvr = null;
        public MyImageSourceStateListener(CaptureVisionRouter cvr)
        {
            this.cvr = cvr;
        }

        public void OnImageSourceStateReceived(EnumImageSourceState state)
        {
            if (state == EnumImageSourceState.ISS_EXHAUSTED)
            {
                if (cvr != null)
                {
                    cvr.StopCapturing();
                }
            }
        }
    }
    internal class Program
    {
        static void Main(string[] args)
        {
            int errorCode = 1;
            string errorMsg;

            // Initialize license.
            // You can request and extend a trial license from https://www.dynamsoft.com/customer/license/trialLicense?product=dbr&utm_source=samples&package=dotnet
            // The string 'DLS2eyJvcmdhbml6YXRpb25JRCI6IjIwMDAwMSJ9' here is a free public trial license. Note that network connection is required for this license to work.
            errorCode = LicenseManager.InitLicense("DLS2eyJvcmdhbml6YXRpb25JRCI6IjIwMDAwMSJ9", out errorMsg);
            if (errorCode != (int)EnumErrorCode.EC_OK && errorCode != (int)EnumErrorCode.EC_LICENSE_CACHE_USED)
            {
                Console.WriteLine("License initialization failed: ErrorCode: " + errorCode + ", ErrorString: " + errorMsg);
            }
            else
            {
                using (CaptureVisionRouter cvr = new CaptureVisionRouter())
                using (DirectoryFetcher fetcher = new DirectoryFetcher())
                {
                    fetcher.SetDirectory("../../../../../../Images");
                    cvr.SetInput(fetcher);

                    CapturedResultReceiver receiver = new MyCapturedResultReceiver();
                    cvr.AddResultReceiver(receiver);

                    MyImageSourceStateListener listener = new MyImageSourceStateListener(cvr);
                    cvr.AddImageSourceStateListener(listener);

                    errorCode = cvr.StartCapturing(PresetTemplate.PT_READ_BARCODES, true, out errorMsg);
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