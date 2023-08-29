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
            int countForThisDevice = 1; // The count value should be set based on your purchased license count
            int countForThisProcess = 1; // The count value should be set based on your purchased license count
            BarcodeReader.SetMaxConcurrentInstanceCount(countForThisDevice, countForThisProcess);
            //contact us at https://www.dynamsoft.com/company/contact/ to get a concurrent instance license
            EnumErrorCode error = BarcodeReader.InitLicense("YOUR-LICENSE-KEY", out string errorMsg);
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