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
            context.Response.ContentType = "application/json";

            HttpPostedFile imgBinary = context.Request.Files["imgBinary"];
            if (imgBinary == null)
                return;
            byte[] buffer = new byte[imgBinary.ContentLength];
            imgBinary.InputStream.Read(buffer, 0, imgBinary.ContentLength);

            BarcodeReader barcodeReader = BarcodeReader.GetInstance();
            if (barcodeReader == null)
            {
                //contact us at https://www.dynamsoft.com/company/contact/ to get a concurrent instance license
                EnumErrorCode error = BarcodeReader.InitLicense("YOUR-LICENSE-KEY", out string errorMsg);
                barcodeReader = BarcodeReader.GetInstance();
                if (barcodeReader == null)
                {
                    throw new Exception("Get Instance From DLS Failed.");
                }
            }
            TextResult[] textResults = barcodeReader.DecodeFileInMemory(buffer, "");

            JArray jAryResults = JArray.FromObject(textResults);
            JObject jobj = new JObject(
                new JProperty("results", jAryResults)
            );
            foreach (JToken result in jobj["results"])
            {
                result["LocalizationResult"]["DocumentName"] = null;
            }
            context.Response.Write(jobj);

            barcodeReader.Recycle();
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