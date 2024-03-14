using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;
using Dynamsoft;
using Dynamsoft.DBR;

namespace asp.net_test
{
    public class Global : System.Web.HttpApplication
    {

        protected void Application_Start(object sender, EventArgs e)
        {
            int countForThisDevice = 4; // The count value should be set based on your purchased license count
            int countForThisProcess = 4; // The count value should be set based on your purchased license count
            int timeout = 5000; // The timeout value should be set based on your application design
            BarcodeReader.SetMaxConcurrentInstanceCount(countForThisDevice, countForThisProcess, timeout);
            // 1.Initialize license.
            // The string "DLS2ey***" here is a free public trial license. Note that network connection is required for this license to work.
            // You can also request a 30-day trial license in the customer portal: https://www.dynamsoft.com/customer/license/trialLicense?architecture=dcv&product=dbr&utm_source=github&package=dotnet
            EnumErrorCode error = BarcodeReader.InitLicense("DLS2eyJoYW5kc2hha2VDb2RlIjoiMjAwMDAxLTEwMjE4MTkyNyIsIm1haW5TZXJ2ZXJVUkwiOiJodHRwczovL21sdHMuZHluYW1zb2Z0LmNvbS8iLCJvcmdhbml6YXRpb25JRCI6IjIwMDAwMSIsInN0YW5kYnlTZXJ2ZXJVUkwiOiJodHRwczovL3NsdHMuZHluYW1zb2Z0LmNvbS8iLCJjaGVja0NvZGUiOi03MjExOTE0Mjl9", out string errorMsg);
            if (error != EnumErrorCode.DBR_SUCCESS)
            {
                throw new Exception("InitLicense failed: " + errorMsg);
            }
        }

        protected void Session_Start(object sender, EventArgs e)
        {

        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {

        }

        protected void Application_AuthenticateRequest(object sender, EventArgs e)
        {

        }

        protected void Application_Error(object sender, EventArgs e)
        {

        }

        protected void Session_End(object sender, EventArgs e)
        {

        }

        protected void Application_End(object sender, EventArgs e)
        {

        }
    }
}