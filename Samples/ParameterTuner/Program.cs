
using Dynamsoft.Core;
using Dynamsoft.CVR;
using Dynamsoft.DBR;
using Dynamsoft.License;
using Dynamsoft.Utility;
using System;
using System.IO;

namespace ParameterTuner
{
    internal class Program
    {
        private static int InputImage(out string imagePath)
        {
            int imageChoice = 0;
            imagePath = string.Empty;
            ImageIO imageIO = new ImageIO();
            while (true)
            {
                Console.WriteLine(
                    "\nAvailable Sample Scenarios:\n" +
                    "[1] Blurred barcode\n" +
                    "[2] Multiple barcodes\n" +
                    "[3] Colour Inverted Barcode\n" +
                    "[4] Direct Part Marking\n" +
                    "[5] Custom image\n");

                Console.WriteLine("Enter the number of the image to test, or provide a full path to your own image:");
                string strChoice = Console.ReadLine();
                if (int.TryParse(strChoice, out imageChoice))
                {
                    if (imageChoice == 1)
                        imagePath = "../../../../../Images/blurry.png";
                    else if (imageChoice == 2)
                        imagePath = "../../../../../Images/GeneralBarcodes.png";
                    else if (imageChoice == 3)
                        imagePath = "../../../../../Images/inverted-barcode.png";
                    else if (imageChoice == 4)
                        imagePath = "../../../../../Images/DPM.png";
                    else if (imageChoice == 5)
                    {
                        Console.WriteLine("Please enter the full path to your custom image:");
                        imagePath = Console.ReadLine();
                    }
                    else
                    {
                        Console.WriteLine("Invalid choice. Please try again.");
                        continue;
                    }
                }
                else
                {
                    imagePath = strChoice;
                }

                imagePath = imagePath.Trim('\"');
                if (!System.IO.File.Exists(imagePath))
                {
                    Console.WriteLine("The specified image file does not exist. Please try again.");
                    continue;
                }

                imagePath = Path.GetFullPath(imagePath);
                return imageChoice;
            }
        }

        private static void InputTemplate(CaptureVisionRouter cvRouter, int imageChoice, out string templatePath)
        {
            string firstTemplateDescription = null;
            switch (imageChoice)
            {
                case 1: firstTemplateDescription = "[1] ReadBlurryBarcode.json  (Suitable for blurred barcode)"; break;
                case 2: firstTemplateDescription = "[1] ReadMultipleBarcode.json  (Suitable for multiple barcodes)"; break;
                case 3: firstTemplateDescription = "[1] ReadInvertedBarcode.json  (Suitable for colour inverted barcode)"; break;
                case 4: firstTemplateDescription = "[1] ReadDPM.json  (Suitable for Direct Part Marking barcode)"; break;
                default: firstTemplateDescription = null; break;
            }

            templatePath = string.Empty;
            while (true)
            {
                Console.WriteLine(
                    "\nSelect template for this image:\n" +
                    (firstTemplateDescription != null ? firstTemplateDescription + "\n" : "") +
                    "[2] ReadBarcodes_Default.json (General purpose settings)\n" +
                    "[3] Custom template (Use your own template)\n");

                Console.WriteLine("Enter the number of the template to test, or provide a full path to your own template:");
                string strChoice = Console.ReadLine();
                if (int.TryParse(strChoice, out int number))
                {
                    if (number == 1 && firstTemplateDescription != null)
                    {
                        switch (imageChoice)
                        {
                            case 1: templatePath = "../../../../../CustomTemplates/ReadBlurryBarcode.json"; break;
                            case 2: templatePath = "../../../../../CustomTemplates/ReadMultipleBarcode.json"; break;
                            case 3: templatePath = "../../../../../CustomTemplates/ReadInvertedBarcode.json"; break;
                            case 4: templatePath = "../../../../../CustomTemplates/ReadDPM.json"; break;
                            default: templatePath = ""; break;
                        }
                    }
                    else if (number == 2)
                    {
                        templatePath = "../../../../../CustomTemplates/ReadBarcodes_Default.json";
                    }
                    else if (number == 3)
                    {
                        Console.WriteLine("Please enter the full path to your custom template:");
                        templatePath = Console.ReadLine();
                    }
                    else
                    {
                        Console.WriteLine("Invalid choice. Please try again.");
                        continue;
                    }
                }
                else
                {
                    templatePath = strChoice;
                }

                templatePath = templatePath.Trim('\"');
                if (!System.IO.File.Exists(templatePath))
                {
                    Console.WriteLine("The specified template file does not exist. Please try again.");
                    continue;
                }

                templatePath = Path.GetFullPath(templatePath);
                int errorCode = cvRouter.InitSettingsFromFile(templatePath, out string errorMsg);
                if (errorCode == (int)EnumErrorCode.EC_UNSUPPORTED_JSON_KEY_WARNING)
                {
                    Console.WriteLine("Warning: " + errorCode + ", " + errorMsg);
                }
                else if (errorCode != (int)EnumErrorCode.EC_OK)
                {
                    Console.WriteLine($"\nFailed to initialize settings, Error: {errorCode}, {errorMsg}. Please try again.");
                    continue;
                }

                return;
            }
        }

        private static int InputNextState()
        {
            while (true)
            {
                Console.WriteLine(
                    "\nWhat would you like to do next?\n" +
                    "[1] Try a different template\n" +
                    "[2] Load another image\n" +
                    "[3] Exit\n");

                Console.WriteLine("Enter your choice:");
                string strChoice = Console.ReadLine();
                if (int.TryParse(strChoice, out int number))
                {
                    if (number >= 1 && number <= 3)
                        return number;
                    else
                        Console.WriteLine("Invalid choice. Please try again.");
                }
                else
                {
                    Console.WriteLine("Invalid input. Please enter a number.");
                }
            }
        }

        internal static void Main(string[] args)
        {
            System.IO.Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);

            int errorCode = 1;
            string errorMsg;

            // Initialize license.
            // You can request and extend a trial license from https://www.dynamsoft.com/customer/license/trialLicense?product=dcv&utm_source=samples&package=dotnet
            // The string 'DLS2eyJvcmdhbml6YXRpb25JRCI6IjIwMDAwMSJ9' here is a free public trial license. Note that network connection is required for this license to work.
            errorCode = LicenseManager.InitLicense("DLS2eyJvcmdhbml6YXRpb25JRCI6IjIwMDAwMSJ9", out errorMsg);
            if (errorCode != (int)EnumErrorCode.EC_OK && errorCode != (int)EnumErrorCode.EC_UNSUPPORTED_JSON_KEY_WARNING)
            {
                Console.WriteLine("License initialization failed: ErrorCode: " + errorCode + ", ErrorString: " + errorMsg);
            }
            else
            {
                Console.WriteLine("Welcome to ParameterTuner!");

                int state = 2;
                int imageChoice = 0;
                string imagePath = string.Empty;
                string templatePath = string.Empty;
                while (state != 3)
                {
                    if (state == 2)
                    {
                        // 2. Input image.
                        imageChoice = InputImage(out imagePath);
                    }

                    using (CaptureVisionRouter cvRouter = new CaptureVisionRouter())
                    {
                        // 3. Input template.
                        InputTemplate(cvRouter, imageChoice, out templatePath);

                        // 4. Decode barcodes from the image.
                        DateTime beginTime = DateTime.Now;
                        CapturedResult[] results = cvRouter.CaptureMultiPages(imagePath);
                        DateTime endTime = DateTime.Now;
                        TimeSpan timeElapsed = endTime - beginTime;

                        Console.WriteLine(
                            "\nRunning with\n" +
                            $"Image: {imagePath}\n" +
                            $"Template: {templatePath}");

                        if (results == null || results.Length == 0)
                        {
                            Console.WriteLine("No captured result.");
                        }
                        else
                        {
                            for (int index = 0; index < results.Length; index++)
                            {
                                CapturedResult result = results[index];

                                if (result.GetErrorCode() == (int)EnumErrorCode.EC_UNSUPPORTED_JSON_KEY_WARNING)
                                    Console.WriteLine("Warning: " + result.GetErrorCode() + ", " + result.GetErrorString());
                                else if (result.GetErrorCode() != (int)EnumErrorCode.EC_OK)
                                    Console.WriteLine("Error: " + result.GetErrorCode() + ", " + result.GetErrorString());

                                // 5. Output the barcode format and barcode text.
                                DecodedBarcodesResult barcodeResult = result.GetDecodedBarcodesResult();
                                BarcodeResultItem[] items = barcodeResult != null ? barcodeResult.GetItems() : null;
                                if (items == null || items.Length == 0)
                                {
                                    Console.WriteLine("Page-" + (index + 1) + " No barcode found.");
                                }
                                else
                                {
                                    Console.WriteLine("Page-" + (index + 1) + " Decoded " + items.Length + " barcodes.");
                                    for (int i = 0; i < items.Length; i++)
                                    {
                                        Console.WriteLine("Result " + (i + 1));
                                        Console.WriteLine("Barcode Format: " + items[i].GetFormatString());
                                        Console.WriteLine("Barcode Text: " + items[i].GetText());
                                    }
                                }
                            }
                        }

                        Console.WriteLine($"Time used: {timeElapsed.TotalMilliseconds} ms...");
                    }

                    state = InputNextState();
                }
            }

            Console.WriteLine("Press Enter to quit...");
            Console.ReadLine();
        }
    }
}