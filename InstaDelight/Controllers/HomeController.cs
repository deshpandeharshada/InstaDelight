using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using InstaDelight.Models;

namespace InstaDelight.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            if (Request.IsAuthenticated)
            {
                if (Session["AdminUserId"] != null)
                {
                    instadelightEntities dataContext = new instadelightEntities();
                    string userid = Session["AdminUserId"].ToString();
                    user currentuser = dataContext.users.Where(x => x.Id == userid).FirstOrDefault();

                    //deleted user is present in database but has allow logon = false
                    //if (currentuser != null)
                    //{
                    //    if (User.IsInRole("TECHBMSSAdmin"))
                    //    {
                    //        if (currentuser.VARCode != "UAE-TECHBMSS")
                    //        {
                    //            currentuser.VARCode = "UAE-TECHBMSS";
                    //            dataContext.SaveChanges();
                    //        }
                    //    }

                    //}
                    return View();
                }
                else
                    return RedirectToAction("Login", "Account");
            }
            else
            {
                return RedirectToAction("Login", "Account");
            }
        }
    }
}
