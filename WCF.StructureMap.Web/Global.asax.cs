using StructureMap;
using Wcf.StructureMap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;

namespace WCF.StructureMap.Web
{
    public class Global : System.Web.HttpApplication
    {
        protected void Application_Start(object sender, EventArgs e)
        {
            StructureMapServiceHostFactory.SetServiceLoactorIntf<Wcf.StructureMap.Services.IServiceLocator>();
            StructureMapServiceHostFactory.Container = new Container(_ =>
             {
                 _.Scan(a =>
                 {
                     a.TheCallingAssembly();
                     a.LookForRegistries();
                 });
             });
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