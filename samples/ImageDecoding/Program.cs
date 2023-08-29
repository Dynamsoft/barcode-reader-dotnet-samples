﻿using System;
using System.Drawing;
using System.IO;
using Dynamsoft;
using Dynamsoft.DBR;
using System.Windows.Forms;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;


namespace ImageDecoding
{

    class Program
    {
        class ImageData
        {
            public byte[] bytes;
            public int width;
            public int height;
            public int stride;
            public EnumImagePixelFormat format;
        }
        [DllImport("kernel32.dll")]
        internal static extern void CopyMemory(IntPtr destination, IntPtr source, int length);

        static ImageData cvtToImageData(string filePath)
        {
            Bitmap bitMap = new Bitmap(filePath);
            ImageData imgData = new ImageData();
            EnumImagePixelFormat iDBRFormat;
            bool bUsePalette = false;
            byte[] arrGray = null;
            byte[] tempBytes = null;

            if (bitMap != null)
            {
                try
                {
                    PixelFormat format = bitMap.PixelFormat;
                    if (format == PixelFormat.Format32bppArgb || format == PixelFormat.Format32bppPArgb || format == PixelFormat.Format32bppRgb)
                    {
                        iDBRFormat = EnumImagePixelFormat.IPF_ARGB_8888;
                    }
                    else if (format == PixelFormat.Format24bppRgb)
                    {
                        iDBRFormat = EnumImagePixelFormat.IPF_RGB_888;
                    }
                    else if (format == PixelFormat.Format1bppIndexed)
                    {
                        iDBRFormat = EnumImagePixelFormat.IPF_BINARY;
                    }
                    else if (format == PixelFormat.Format8bppIndexed)
                    {
                        iDBRFormat = EnumImagePixelFormat.IPF_GRAYSCALED;
                        if (bitMap.Palette != null && bitMap.Palette.Entries != null)
                        {
                            bUsePalette = true;
                            arrGray = new byte[bitMap.Palette.Entries.Length];
                            for (int i = 0; i < bitMap.Palette.Entries.Length; ++i)
                            {
                                arrGray[i] = (byte)((bitMap.Palette.Entries[i].R * 19562 + bitMap.Palette.Entries[i].G * 38550 + bitMap.Palette.Entries[i].B * 7424) >> 16);
                            }
                        }
                    }
                    else
                    {
                        format = PixelFormat.Format24bppRgb;
                        iDBRFormat = EnumImagePixelFormat.IPF_RGB_888;
                    }

                    BitmapData bmpData = null;
                    IntPtr pBuffer = IntPtr.Zero;

                    bmpData = bitMap.LockBits(new Rectangle(0, 0, bitMap.Width, bitMap.Height), ImageLockMode.ReadOnly, format);
                    int imageSize = bmpData.Stride * bmpData.Height;

                    pBuffer = System.Runtime.InteropServices.Marshal.AllocHGlobal(imageSize);

                    int rowSize = bmpData.Stride;

                    if (!bUsePalette)
                    {
                        CopyMemory(pBuffer, bmpData.Scan0, imageSize);
                    }
                    else
                    {
                        tempBytes = new byte[imageSize];
                        System.Runtime.InteropServices.Marshal.Copy(bmpData.Scan0, tempBytes, 0, imageSize);
                        for (int i = 0; i < tempBytes.Length; ++i)
                        {
                            tempBytes[i] = arrGray[tempBytes[i]];
                        }
                        System.Runtime.InteropServices.Marshal.Copy(tempBytes, 0, pBuffer, imageSize);
                    }

                    byte[] bt = new byte[imageSize];
                    Marshal.Copy(pBuffer, bt, 0, imageSize);
                    bitMap.UnlockBits(bmpData);


                    imgData.width = bmpData.Width;
                    imgData.height = bmpData.Height;
                    imgData.bytes = bt;
                    imgData.stride = bmpData.Stride;
                    imgData.format = iDBRFormat;
                }
                catch (Exception exp)
                {
                    Console.WriteLine(exp.Message);
                }
            }
            return imgData;
        }

        static Bitmap getBitmap(string filePath)
        {
            Bitmap bmp = null;
            try
            {
                bmp = new Bitmap(filePath);
            }
            catch (Exception exp)
            {
                Console.WriteLine(exp.Message);
            }
            return bmp;
        }

        static string getFileBase64(string filePath)
        {
            byte[] fileBytes = getFileBytes(filePath);

            string encodedText = null;

            encodedText = System.Convert.ToBase64String(fileBytes);

            return encodedText;
        }

        static FileStream getFileStream(string filePath)
        {
            FileStream fis = null;
            try
            {
                fis = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            }
            catch (Exception exp)
            {
                Console.WriteLine(exp.Message);
            }
            return fis;
        }


        static byte[] getFileBytes(string filePath)
        {
            FileInfo fi = null;
            byte[] buffer = null;
            FileStream fis = null;
            BinaryWriter bos = null;
            // MemoryStream ms = null;
            try
            {
                fi = new FileInfo(filePath);
                buffer = new byte[fi.Length];

                fis = fi.OpenRead();
                fis.Read(buffer, 0, Convert.ToInt32(fis.Length));
            }
            catch (Exception exp)
            {
                Console.WriteLine(exp.Message);
            }
            finally
            {
                if (null != bos)
                {
                    try
                    {
                        bos.Close();
                    }
                    catch (Exception exp)
                    {
                        Console.WriteLine(exp.Message);
                    }
                }
                if (null != fis)
                {
                    try
                    {
                        fis.Close();
                    }
                    catch (Exception exp)
                    {
                        Console.WriteLine(exp.Message);
                    }
                }
            }
            return buffer;
        }

        static int chooseNumber()
        {
            Console.WriteLine();
            Console.WriteLine(">> Choose a number for diffent decoding interfaces:");
            Console.WriteLine("   0: exit program");
            Console.WriteLine("   1: DecodeFile");
            Console.WriteLine("   2: DecodeBase64String");
            Console.WriteLine("   3: DecodeBitmap");
            Console.WriteLine("   4: DecodeFileInMemory");
            Console.WriteLine("   5: DecodeBuffer");


            int iNum = -1;
            try
            {
                iNum = int.Parse(Console.ReadLine());
            }
            catch (Exception exp)
            {
                Console.WriteLine(exp.Message);
            }
            return iNum;
        }
        static void Main(string[] args)
        {
            try
            {
                // Initialize license.
                // The string "DLS2eyJvcmdhbml6YXRpb25JRCI6IjIwMDAwMSJ9" here is a free public trial license. Note that network connection is required for this license to work.
                // You can also request a 30-day trial license in the customer portal: https://www.dynamsoft.com/customer/license/trialLicense?architecture=dcv&product=dbr&utm_source=github&package=dotnet
                string errorMsg;
                EnumErrorCode errorCode = BarcodeReader.InitLicense("DLS2eyJvcmdhbml6YXRpb25JRCI6IjIwMDAwMSJ9", out errorMsg);
                if (errorCode != EnumErrorCode.DBR_SUCCESS)
                {
                    Console.WriteLine(errorMsg);
                }

                // Create an instance of Barcode Reader
                BarcodeReader dbr = BarcodeReader.GetInstance();
                if (dbr == null)
                {
                    throw new Exception("Get Instance Failed.");
                }


                string filePath = "../../../../images/AllSupportedBarcodeTypes.png";
                TextResult[] results = null;

                while (true)
                {
                    int iNum = -1;
                    while (true)
                    {
                        iNum = chooseNumber();
                        if (iNum < 0 || iNum > 6)
                            Console.WriteLine("Please choose a valid number.");
                        else
                            break;
                    }
                    if (0 == iNum)
                        return;
                    switch (iNum)
                    {
                        case 1:
                            {
                                results = dbr.DecodeFile(filePath, "");
                                break;
                            }
                        case 2:
                            {
                                string base64Str = getFileBase64(filePath);
                                results = dbr.DecodeBase64String(base64Str, "");
                                break;
                            }
                        case 3:
                            {
                                Bitmap bmp = getBitmap(filePath);
                                results = dbr.DecodeBitmap(bmp, "");
                                break;
                            }
                        case 4:
                            {
                                byte[] bytes = getFileBytes(filePath);
                                results = dbr.DecodeFileInMemory(bytes, "");
                                break;
                            }
                        case 5:
                            {
                                ImageData img = cvtToImageData(filePath);
                                results = dbr.DecodeBuffer(img.bytes, img.width, img.height, img.stride, img.format, "");
                                break;
                            }
                        default:
                            break;
                    }

                    // Output the barcode format and barcode text.
                    if (results != null && results.Length > 0)
                    {
                        int i = 1;
                        foreach (TextResult result in results)
                        {
                            string barcodeFormat = result.BarcodeFormatString;
                            string message = "Barcode " + i + ": " + barcodeFormat + ", " + result.BarcodeText;
                            Console.WriteLine(message);
                            i++;
                        }
                    }
                    else
                    {
                        Console.WriteLine("No data detected.");
                    }
                }
                dbr.Recycle();
            }
            catch (Exception exp)
            {
                Console.WriteLine(exp.Message);
            }
            Console.WriteLine("Press any key to quit...");
            Console.Read();
        }
    }
}