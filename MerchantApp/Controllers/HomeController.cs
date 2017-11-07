using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using MerchantApp.Filters;
using System.Security.Claims;
using MerchantApp.Models;

namespace MerchantApp.Controllers
{
    [AdminMerchantFilter]
    [Authorize]

    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            List<Claim> roles = ((ClaimsIdentity)User.Identity).Claims
               .Where(c => c.Type == ClaimTypes.Role).ToList();

            string isMenuCreationAllowed = "true";
            string isCouponCreationAllowed = "true";
            string isUserCreationAllowed = "true";

            foreach (Claim c in roles)
            {
                if (c.Value == "Staff")
                {
                    isMenuCreationAllowed = "false";
                    isCouponCreationAllowed = "false";
                    isUserCreationAllowed = "false";
                }
                else if (c.Value == "LocationManager")
                {
                    using (MerchantEntities dataContext = new MerchantEntities())
                    {
                        string userid = Session["UserId"].ToString();
                        merchant_master master = dataContext.merchant_master.Where(x => x.UserId == userid).FirstOrDefault();
                        if (master != null)
                        {
                            branch_master locmgr = dataContext.branch_master.Where(x => x.BranchManagerId == master.merchantid).FirstOrDefault();
                            if (locmgr != null)
                            {
                                if (locmgr.IsMenuAllowed == false)
                                {
                                    isMenuCreationAllowed = "false";
                                }
                                if (locmgr.IsCouponAllowed == false)
                                {
                                    isCouponCreationAllowed = "false";
                                }
                                if (locmgr.IsAddUserAllowed == false)
                                {
                                    isUserCreationAllowed = "false";
                                }
                            }
                        }
                    }
                }
            }


            ViewBag.isMenuCreationAllowed = isMenuCreationAllowed;
            ViewBag.isCouponCreationAllowed = isCouponCreationAllowed;
            ViewBag.isUserCreationAllowed = isUserCreationAllowed;

            return View();

        }

        public ActionResult Support()
        {
            return View();

        }
    }
}
