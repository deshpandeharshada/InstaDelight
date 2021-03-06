﻿using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System;
using System.Threading;
using System.Globalization;
using System.Web.Helpers;

namespace MerchantApp
{
    public class MvcApplication : HttpApplication
    {
        protected void Application_Start()
        {            
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        void Application_BeginRequest(Object sender, EventArgs e)
        {
            string lang = string.Empty;//default to the invariant culture
            HttpCookie cookie = Request.Cookies["LanguageSelected"];

            if (cookie != null && cookie.Value != null)
                lang = cookie.Value.ToString();
            else
                lang = "en";

            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(lang);
            Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture(lang);
        }
    }


}
