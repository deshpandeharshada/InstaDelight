using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Filters;
using System.Web.Routing;

namespace MerchantApp.Filters
{
    public class CustomAuthFilterAttribute:ActionFilterAttribute,IAuthenticationFilter
    {
        public void OnAuthentication(AuthenticationContext filterContext)
        {

            if (filterContext.ActionDescriptor.GetCustomAttributes(typeof(SkipCustomAuthFilterAttribute), false).Any())
            {
                return;
            }

            if (HttpContext.Current.Session["UserId"] == null || HttpContext.Current.Session["UserRole"] == null)
            {
                if (filterContext.HttpContext.Request.IsAjaxRequest())
                {
                    filterContext.Result = new HttpStatusCodeResult(403);
                }
                else
                {
                    filterContext.Result = new RedirectResult("../Account/Login");
                    filterContext.HttpContext.Response.StatusCode = 403;
                }
            }           
        }

        public void OnAuthenticationChallenge(AuthenticationChallengeContext filterContext)
        {

        }
    }
}