using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json.Linq;
using Dynamsoft;
using Dynamsoft.DBR;

namespace asp.net_test
{
    /// <summary>
    /// Summary description for ReadBarcode
    /// </summary>
    public class ReadBarcode : IHttpHandler
    {

        public void ProcessRequest(HttpContext context)
        {
            // Get an idle instance to process the barcode decoding request.
            BarcodeReader barcodeReader = BarcodeReader.GetInstance();
            if (barcodeReader == null)
            {
                context.Response.Write("No barcode decoding instance available.");
            }
            else
            {
                context.Response.ContentType = "application/json";
                HttpPostedFile imgBinary = context.Request.Files["imgBinary"];
                if (imgBinary == null)
                    return;
                byte[] buffer = new byte[imgBinary.ContentLength];
                imgBinary.InputStream.Read(buffer, 0, imgBinary.ContentLength);
                TextResult[] textResults = barcodeReader.DecodeFileInMemory(buffer, "");

                JArray jAryResults = JArray.FromObject(textResults);
                JObject jobj = new JObject(
                    new JProperty("results", jAryResults)
                );
                context.Response.Write(jobj);
                // Recycle the instance to make it idle for other concurrent tasks
                barcodeReader.Recycle();
            }
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}