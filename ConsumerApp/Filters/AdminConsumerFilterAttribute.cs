using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace ConsumerApp.Filters
{
    public class AdminConsumerFilterAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {

            if (HttpContext.Current.Session["UserId"] == null)
            {
                if (filterContext.HttpContext.Request.IsAjaxRequest())
                {
                    filterContext.Result = new HttpStatusCodeResult(403);
                }
                else
                {
                    filterContext.Result = new RedirectResult("/Account/Login");
                    filterContext.HttpContext.Response.StatusCode = 403;
                }
            }

            if (HttpContext.Current.Session["UserId"] != null)
            {
                if (HttpContext.Current.Session["UserRole"] != null)
                {
                    if (HttpContext.Current.Session["UserRole"].ToString() != "Consumer")
                    {
                        if (filterContext.HttpContext.Request.IsAjaxRequest())
                        {
                            filterContext.Result = new HttpStatusCodeResult(403);
                        }
                        else
                        {

                            filterContext.Result = new RedirectResult("/Account/Login");
                            filterContext.HttpContext.Response.StatusCode = 403;
                        }
                    }
                }
            }

            base.OnActionExecuting(filterContext);
        }
    }
}