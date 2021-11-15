using System;
using System.Drawing;
using System.IO;
using Dynamsoft;
using Dynamsoft.DBR;
using System.Windows.Forms;


namespace ImageDecoding
{
    class Program
    {
		class ImageData
        { 
            public byte[] bytes;
            public int width;
            public int height;
            public int format;
        }
		
		static BitmapData cvtToImageDate(string filePath)
		{
			
			Bitmap bmp = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format8bppIndexed);
			
			int stride = ((24 * image.getWidth() + 31) / 32) * 4;
			
			BinaryWriter output = new BinaryWriter(bmp.getHeight() * stride);
			
			
			
			
			BitmapData bmpData = bmp.LockBits(new System.Drawing.Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format8bppIndexed);
			bmpData.Width = bmp.;
			bmpData.Height = bmp.;
			bmpData.Bytes = ConvertToByte(output);
			bmpData.Stride = stride;
			bmpData.Format = EnumImagePixelFormat.IPF_BGR_888;
			
			return bmpData;
		}

        static Bitmap getBufferedImage(string filePath)
		{
			try
			{
				Bitmap bmp = new Bitmap(filePath);
            }
			catch(Exception exp)
            {
                Console.WriteLine(exp.Message);
            }
			return bmp;
		}
		
		static string getFileBase64(string filePath)
		{
			//byte[] fileBytes = getFileBytes(filePath);
			string encodedText = null;
			
			string base64string = System.Convert.ToBase64String(System.IO.File.ReadAllBytes(filePath));
			
			byte[] buf = System.Convert.ToBase64String(base64string);
			
			System.IO.File.WriteAllBytes(encodedText,buf);
			
			return encodedText;
		}
		
		static FileStream getFileStream(string filePath)
		{
			try
			{
				FileStream fis = new FileStream(filePath,FileMode.Open,FileAccess.Read);
			}
			catch(Exception exp)
            {
                Console.WriteLine(exp.Message);
            }
			return fis;
		}
		
		
		static byte[] getFileBytes(string filePath)
        {
            byte[] buffer = null;

            try
            {
                FileStream fis = new FileStream(filePath,FileMode.Open,FileAccess.Read);
                BinaryReader bos = new BinaryWriter(fis);
				
				long length = fis.Length;
				byte[] tempBuffer = new byte[length];
				bos.Read(tempBuffer,0,tempBuffer.Length);
				buffer = bos.toByteArray();
            }
            catch (Exception exp)
            {
                Console.WriteLine(exp.Message);
            }
            finally
            {
				if(null!=bos){
					try{
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
			Console.WriteLine("   4: decodeFileInMemory-file bytes");
			Console.WriteLine("   5: decodeFileInMemory-input stream");
			Console.WriteLine("   6: decodeBuffer");
			
			FileStream fs = new FileStream(filePath,FileMode.OpenOrCreate);
			//BufferedStream cin = fileStream.Write(bt);
				
			try
			{
				int iNum =Console.ReadLine();
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
	            		results = dbr.decodeFile(filePath, "");
	            		break;
						}
						case 2: {
							string base64Str = getFileBase64(filePath);
							
							//Decoding with base64 encoded file
							results = dbr.decodeBase64String(base64Str, "");
							break;
						}         	
						case 3: {
							Bitmap bufferedImage = getBufferedImage(filePath);
							
							//Decoding with buffered image
							results = dbr.decodeBufferedImage(bufferedImage, "");
							break;
						}
						case 4: {         		
							byte[] bytes = getFileBytes(filePath);
							
							//Decoding with file bytes
							results = dbr.decodeFileInMemory(bytes, "");
							break;
						}
						case 5: {
							FileStream fs = getFileStream(filePath);
							
							//Decoding with input stream
							results = dbr.decodeFileInMemory(fs, "");
							break;
						}
						case 6: {
							ImageData img = cvtToImageData(filePath);
							
							//Decoding with raw buffer
							results = dbr.decodeBuffer(img.bytes, img.width, img.height, img.stride, img.format, "");
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
