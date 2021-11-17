using System;
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
            TextResult[] barcodes = null;
            EnumErrorCode ec = EnumErrorCode.DBR_SUCCESS;
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
                    imgData.format = EnumImagePixelFormat.IPF_ABGR_8888;
                }
                catch (Exception exp)
                {
                    Console.WriteLine(exp.Message);
                }
            }
            return imgData;
        }

        static Bitmap getBufferedImage(string filePath)
	    {
            Bitmap bmp = null;
            try
		    {
			    bmp = new Bitmap(filePath);
            }
		    catch(Exception exp)
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
			    fis = new FileStream(filePath,FileMode.Open,FileAccess.Read);
		    }
		    catch(Exception exp)
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
		        if(null!=bos){
                    try
                    {
                        bos.Close();
                    }
                    catch(Exception exp)
                    {
                        Console.WriteLine(exp.Message);
                    }
		        }
		        if(null!=fis){
			        try
			        {
				        fis.Close();
			        }
			        catch(Exception exp)
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
		    Console.WriteLine("   1: decodeFile");
		    Console.WriteLine("   2: decodeBase64String");
		    Console.WriteLine("   3: decodeBufferedImage");
		    Console.WriteLine("   4: decodeFileInMemory-file bytes");;
		    Console.WriteLine("   5: decodeBuffer");
			      

            int iNum = -1;
		    try
		    {
			    iNum = int.Parse(Console.ReadLine());
		    }
		    catch(Exception exp)
		    {
			    Console.WriteLine(exp.Message);
		    }
		    return iNum;
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
             

                string filePath = "../../../../images/AllSupportedBarcodeTypes.png";
                TextResult[] results = null;
                
                // Configure settings

                // Through PublicRuntimeSetting

                // Call GetRuntimeSettings to get current runtime settings.
                PublicRuntimeSettings settings = dbr.GetRuntimeSettings();

		        while(true)
		        {
	                int iNum = -1;
	                while(true)
	                {
                        iNum = chooseNumber();
		                if(iNum < 0 || iNum > 6)
                            Console.WriteLine("Please choose a valid number.");
		                else
		                    break;
		            }
		            if(0 == iNum)
		                break;
			        switch(iNum)
			        {
				        case 1: {
	            		        //Decoding with file path
	            	        results = dbr.DecodeFile(filePath, "");
	            	        break;
				        }
				        case 2: {
					        string base64Str = getFileBase64(filePath);
							
					        //Decoding with base64 encoded file
					        results = dbr.DecodeBase64String(base64Str, "");
					        break;
				        }         	
				        case 3: {
					        Bitmap bufferedImage = getBufferedImage(filePath);
							
					        //Decoding with buffered image
					        results = dbr.DecodeBitmap(bufferedImage, "");
					        break;
				        }
				        case 4: {         		
					        byte[] bytes = getFileBytes(filePath);
							
					        //Decoding with file bytes
					        results = dbr.DecodeFileInMemory(bytes, "");
					        break;
				        }
				        case 5: {
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
					        string barcodeFormat = result.BarcodeFormat == 0 ? result.BarcodeFormatString_2 : result.BarcodeFormatString;
					        string message = "Barcode" + i + ":" + barcodeFormat + "," + result.BarcodeText;
					        Console.WriteLine(message);
					        i++;
				        }
			        }
			        else
			        {
				        Console.WriteLine("No data detected.");
			        }
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
