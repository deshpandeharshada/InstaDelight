
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AspNet.Identity.MySQL;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.Google;
using Owin;
using InstaDelight.Models;
using LinqKit;
using System.IO;
using QRCoder;
using System.Drawing;

namespace InstaDelight.Controllers
{
    public class MerchantController : Controller
    {
        ApplicationDbContext context = new ApplicationDbContext("DefaultConnection");


        // GET: Merchant
        public ActionResult Index()
        {
            if (Request.IsAuthenticated)
            {
                if (Session["AdminUserId"] != null)
                {
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

        public ActionResult MerchantConfiguration()
        {
            if (Request.IsAuthenticated)
            {
                if (Session["AdminUserId"] != null)
                {
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

        public JsonResult GetCountrycode(string countryid)
        {
            if (Request.IsAuthenticated)
            {
                if (Session["AdminUserId"] != null)
                {
                    try
                    {
                        using (instadelightEntities dataContext = new instadelightEntities())
                        {
                            int ccode = 0;
                            ccode = Convert.ToInt32(countryid);

                            country_master country = dataContext.country_master.Where(x => x.countryid == ccode).FirstOrDefault();
                            if (country != null)
                            {
                                return Json(country.CountryCode, JsonRequestBehavior.AllowGet);
                            }
                            else
                                return Json("+91", JsonRequestBehavior.AllowGet);


                        }
                    }
                    catch (Exception ex)
                    {
                        //call error view here
                        //log the error
                        EventLog.LogErrorData("Error occured Merchant/GetCountrycode." + ex.Message, true);
                        return Json("Error occured while retrieving country code", JsonRequestBehavior.AllowGet);
                    }
                }
                else
                    return Json("Unauthorized access", JsonRequestBehavior.AllowGet);
            }
            else
                return Json("Unauthorized access", JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetCountryFromcode(string countrycode)
        {
            try
            {
                using (instadelight_consumerEntities dataContext = new instadelight_consumerEntities())
                {
                    var country = dataContext.country_master.Where(c => c.CountryCode == countrycode).FirstOrDefault();
                    return Json(country, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                //call error view here
                //log the error
                EventLog.LogErrorData("Error occurred Consumer/GetCountryFromcode." + ex.Message, true);
                return Json(Global.InstaDelight.GetCountryCodeException, JsonRequestBehavior.AllowGet);
            }
        }


        public ActionResult AddMerchant(string Flag, int merchantid)
        {
            if (Request.IsAuthenticated)
            {
                if (Session["AdminUserId"] != null)
                {
                    try
                    {
                        if (Flag != null && merchantid != null)
                        {
                            ViewBag.Flag = Flag;
                            ViewBag.Merchantid = merchantid;
                            return View("AddMerchant");
                        }
                        else
                        {
                            return RedirectToAction("Login", "Account");
                        }
                    }
                    catch (Exception ex)
                    {
                        //call error view here
                        //log the error
                        EventLog.LogErrorData("Error occured Merchant/AddMerchant." + ex.Message, true);
                        return RedirectToAction("Login", "Account");
                    }
                }
                else
                    return RedirectToAction("Login", "Account");
            }
            else
                return RedirectToAction("Login", "Account");
        }

        public JsonResult GetCategories()
        {
            if (Request.IsAuthenticated)
            {
                if (Session["AdminUserId"] != null)
                {
                    try
                    {
                        using (instadelightEntities dataContext = new instadelightEntities())
                        {
                            var categoryList = dataContext.business_category_master.OrderBy(x => x.CategoryName).ToList();

                            return Json(categoryList, JsonRequestBehavior.AllowGet);
                        }
                    }
                    catch (Exception ex)
                    {
                        //call error view here
                        //log the error
                        EventLog.LogErrorData("Error occured Merchant/GetCategories." + ex.Message, true);
                        return Json("Error occured while retrieving business categories", JsonRequestBehavior.AllowGet);
                    }
                }
                else
                    return Json("Unauthorized access", JsonRequestBehavior.AllowGet);
            }
            else
                return Json("Unauthorized access", JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetCountries()
        {
            if (Request.IsAuthenticated)
            {
                if (Session["AdminUserId"] != null)
                {
                    try
                    {
                        using (instadelightEntities dataContext = new instadelightEntities())
                        {
                            var countryList = dataContext.country_master.OrderBy(x => x.countryname).ToList();

                            return Json(countryList, JsonRequestBehavior.AllowGet);
                        }
                    }
                    catch (Exception ex)
                    {
                        //call error view here
                        //log the error
                        EventLog.LogErrorData("Error occured Merchant/GetCountries." + ex.Message, true);
                        return Json("Error occured while retrieving country list", JsonRequestBehavior.AllowGet);
                    }
                }
                else
                    return Json("Unauthorized access", JsonRequestBehavior.AllowGet);
            }
            else
                return Json("Unauthorized access", JsonRequestBehavior.AllowGet);
        }

        public string getCurrency()
        {
            if (Request.IsAuthenticated)
            {
                if (Session["AdminUserId"] != null)
                {
                    try
                    {
                        using (instadelightEntities dataContext = new instadelightEntities())
                        {
                            country_master ctry = dataContext.country_master.FirstOrDefault();

                            return ctry.currency;
                        }
                    }
                    catch (Exception ex)
                    {
                        //call error view here
                        //log the error
                        EventLog.LogErrorData("Error occured Merchant/GetCurrency." + ex.Message, true);
                        return "Error occured while retrieving currency";
                    }
                }
                else
                    return "Unauthorized access";
            }
            else
                return "Unauthorized access";
        }

        public JsonResult GetStates(string countryid)
        {
            if (Request.IsAuthenticated)
            {
                if (Session["AdminUserId"] != null)
                {
                    try
                    {
                        using (instadelightEntities dataContext = new instadelightEntities())
                        {
                            int no = Convert.ToInt32(countryid);
                            var States = dataContext.state_master.Where(x => x.countryid == no).OrderBy(x => x.state).ToList();

                            return Json(States, JsonRequestBehavior.AllowGet);
                        }
                    }
                    catch (Exception ex)
                    {
                        //call error view here
                        //log the error
                        EventLog.LogErrorData("Error occured Merchant/GetStates." + ex.Message, true);
                        return Json("Error occured while retrieving states list from country", JsonRequestBehavior.AllowGet);
                    }
                }
                else
                    return Json("Unauthorized access", JsonRequestBehavior.AllowGet);
            }
            else
                return Json("Unauthorized access", JsonRequestBehavior.AllowGet);
        }


        public JsonResult GetCities()
        {
            if (Request.IsAuthenticated)
            {
                if (Session["AdminUserId"] != null)
                {
                    try
                    {
                        using (instadelightEntities dataContext = new instadelightEntities())
                        {
                            var cityList = dataContext.city_master.OrderBy(x => x.City).ToList();

                            return Json(cityList, JsonRequestBehavior.AllowGet);
                        }
                    }
                    catch (Exception ex)
                    {
                        //call error view here
                        //log the error
                        EventLog.LogErrorData("Error occured Merchant/GetCities." + ex.Message, true);
                        return Json("Error occured while retrieving city list", JsonRequestBehavior.AllowGet);
                    }
                }
                else
                    return Json("Unauthorized access", JsonRequestBehavior.AllowGet);
            }
            else
                return Json("Unauthorized access", JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetLocations(string cityid)
        {
            if (Request.IsAuthenticated)
            {
                if (Session["AdminUserId"] != null)
                {
                    try
                    {
                        using (instadelightEntities dataContext = new instadelightEntities())
                        {
                            int no = Convert.ToInt32(cityid);
                            var Locations = dataContext.location_master.Where(x => x.CityId == no).OrderBy(x => x.Location).ToList();

                            return Json(Locations, JsonRequestBehavior.AllowGet);
                        }
                    }
                    catch (Exception ex)
                    {
                        //call error view here
                        //log the error
                        EventLog.LogErrorData("Error occured Merchant/GetLocations." + ex.Message, true);
                        return Json("Error occured while retrieving locations list", JsonRequestBehavior.AllowGet);
                    }
                }
                else
                    return Json("Unauthorized access", JsonRequestBehavior.AllowGet);
            }
            else
                return Json("Unauthorized access", JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetMerchantById(string merchantid)
        {
            if (Request.IsAuthenticated)
            {
                if (Session["AdminUserId"] != null)
                {
                    try
                    {
                        using (instadelightEntities dataContext = new instadelightEntities())
                        {
                            int no = Convert.ToInt32(merchantid);
                            var merchant = dataContext.merchant_master.Find(no);
                            if (merchant.UserName == null)
                            {
                                user muser = dataContext.users.Where(x => x.Id == merchant.UserId).FirstOrDefault();
                                if (muser != null)
                                {
                                    merchant.UserName = muser.UserName;
                                }
                            }
                            merchantjoiningbonu bonus = dataContext.merchantjoiningbonus.Where(x => x.MerchantId == merchant.UserId).OrderByDescending(x => x.DateCreated).FirstOrDefault();
                            if (bonus != null)
                            {
                                merchant.JoiningBonus = bonus.JoiningBonus;
                            }

                            if (merchant.RunRewardProgram == false)
                            {
                                redeemoption opt = dataContext.redeemoptions.Where(x => x.MerchantId == merchant.UserId).OrderByDescending(x => x.CreationDate).FirstOrDefault();
                                merchant.redeemoptions = opt;
                            }

                            rewardmaster rwd = dataContext.rewardmasters.Where(x => x.MerchantId == merchant.UserId).OrderByDescending(x => x.CreationDate).FirstOrDefault();
                            if (rwd != null)
                            {
                                merchant.RewardPoints = rwd.RewardPoints;
                                merchant.RewardRs = rwd.RewardRs;
                            }

                            redeemmaster rdm = dataContext.redeemmasters.Where(x => x.MerchantId == merchant.UserId).OrderByDescending(x => x.CreationDate).FirstOrDefault();
                            if (rdm != null)
                            {
                                merchant.RedeemPt = rdm.RedeemPt;
                                merchant.RedeemRs = rdm.RedeemRs;
                            }

                            var jsonResult = Json(merchant, JsonRequestBehavior.AllowGet);
                            jsonResult.MaxJsonLength = Int32.MaxValue;

                            return jsonResult;

                            // return Json(merchant, JsonRequestBehavior.AllowGet);
                        }
                    }
                    catch (Exception ex)
                    {
                        //call error view here
                        //log the error
                        EventLog.LogErrorData("Error occured Merchant/GetMerchantById." + ex.Message, true);
                        return Json("Error occured while retrieving merchant by id", JsonRequestBehavior.AllowGet);
                    }
                }
                else
                    return Json("Unauthorized access", JsonRequestBehavior.AllowGet);
            }
            else
                return Json("Unauthorized access", JsonRequestBehavior.AllowGet);
        }

        public string AddNewMerchant(merchant_master mch)
        {
            if (Request.IsAuthenticated)
            {
                if (Session["AdminUserId"] != null)
                {
                    try
                    {
                        if (mch != null)
                        {
                            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));
                            var UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));
                            UserManager.UserValidator = new UserValidator<ApplicationUser>(UserManager)
                            {
                                AllowOnlyAlphanumericUserNames = false,
                                RequireUniqueEmail = true
                            };

                            var user = new ApplicationUser();
                            //user.UserName = mch.PhoneNumber;
                            user.UserName = mch.UserName;
                            string userPWD = "123456";
                            user.FirstName = mch.MerchantName;
                            user.LastName = "";
                            user.Phone = mch.PhoneNumber;
                            user.PhoneNumber = mch.PhoneNumber;
                            user.Email = mch.Email;
                            var chkUser = UserManager.Create(user, userPWD);

                            ////Add default User to Role Admin   
                            if (chkUser.Succeeded)
                            {
                                var rolesForUser = UserManager.GetRoles(user.Id);
                                if (!rolesForUser.Contains("Merchant"))
                                {
                                    UserManager.AddToRole(user.Id, "Merchant");
                                }

                                using (instadelightEntities dataContext = new instadelightEntities())
                                {
                                    user currentuser = dataContext.users.Where(x => x.Id == user.Id).FirstOrDefault();
                                    if (currentuser != null)
                                    {
                                        currentuser.FirstName = mch.MerchantName;
                                        currentuser.LastName = "";
                                        dataContext.SaveChanges();
                                    }

                                    mch.UserId = user.Id;
                                    mch.creation_date = DateTime.Now;

                                    if (mch.MerchantDEC == null)
                                    {
                                        business_category_master cat = dataContext.business_category_master.Where(x => x.categoryid == mch.Category).FirstOrDefault();
                                        if (cat != null)
                                        {
                                            mch.merchantDecFromLibrary = "/Images/BackgroundImages/" + cat.CategoryName.Replace(" ", "_").Replace("/", "_") + ".jpg";
                                        }

                                    }

                                    dataContext.merchant_master.Add(mch);
                                    dataContext.SaveChanges();

                                    //Reward options
                                    if (mch.RunRewardProgram == false)
                                    {
                                        redeemoption opt = new redeemoption();
                                        opt.MerchantId = mch.UserId;
                                        opt.Option1 = mch.redeemoptions.Option1;
                                        opt.Option2 = mch.redeemoptions.Option2;
                                        opt.Option3 = mch.redeemoptions.Option3;
                                        opt.Option4 = mch.redeemoptions.Option4;
                                        opt.Option5 = mch.redeemoptions.Option5;
                                        opt.CreationDate = DateTime.Now;
                                        dataContext.redeemoptions.Add(opt);
                                        dataContext.SaveChanges();
                                    }

                                    if (Convert.ToInt32(mch.JoiningBonus) != 0)
                                    {
                                        merchantjoiningbonu bonus = new merchantjoiningbonu();
                                        bonus.MerchantId = mch.UserId;
                                        bonus.JoiningBonus = Convert.ToInt32(mch.JoiningBonus);
                                        bonus.DateCreated = DateTime.Now;
                                        dataContext.merchantjoiningbonus.Add(bonus);
                                        dataContext.SaveChanges();
                                    }

                                    //Adding reward entries
                                    if (mch.RewardRs != null && mch.RewardPoints != null)
                                    {
                                        if (Convert.ToInt32(mch.RedeemRs) != 0 && Convert.ToInt32(mch.RewardPoints) != 0)
                                        {
                                            rewardmaster rdm = new rewardmaster();
                                            rdm.RewardName = "Reward Points";
                                            rdm.MerchantId = mch.UserId;
                                            rdm.RewardRs = mch.RewardRs;
                                            rdm.RewardPoints = mch.RewardPoints;
                                            rdm.CreationDate = DateTime.Now;
                                            dataContext.rewardmasters.Add(rdm);
                                            dataContext.SaveChanges();
                                        }
                                    }

                                    ////Adding Redeem entries
                                    if (mch.RedeemPt != null && mch.RedeemRs != null)
                                    {
                                        if (Convert.ToInt32(mch.RedeemPt) != 0 && Convert.ToInt32(mch.RedeemRs) != 0)
                                        {
                                            redeemmaster redm = new redeemmaster();
                                            redm.MerchantId = mch.UserId;
                                            redm.RedeemRs = mch.RedeemRs;
                                            redm.RedeemPt = mch.RedeemPt;
                                            redm.CreationDate = DateTime.Now;
                                            dataContext.redeemmasters.Add(redm);
                                            dataContext.SaveChanges();
                                        }
                                    }

                                    //Add default review questions
                                    reviewtemplate rtemp = dataContext.reviewtemplates.Where(x => x.CategoryId == mch.Category).FirstOrDefault();
                                    if (rtemp != null)
                                    {
                                        reviewmaster rev = new reviewmaster();
                                        rev.Question1 = rtemp.Question1;
                                        rev.Question1Type = rtemp.Question1Type;
                                        rev.Question2 = rtemp.Question2;
                                        rev.Question2Type = rtemp.Question2Type;
                                        rev.Question3 = rtemp.Question3;
                                        rev.Question3Type = rtemp.Question3Type;
                                        rev.Question4 = rtemp.Question4;
                                        rev.Question4Type = rtemp.Question4Type;

                                        rev.DefaultQuestion = "Share Our DEC with friends.";
                                        rev.DefaultType = "YesNo";
                                        rev.MerchantId = mch.merchantid;
                                        rev.MerchantUserId = mch.UserId;
                                        rev.CreationDate = DateTime.Now;

                                        dataContext.reviewmasters.Add(rev);
                                        dataContext.SaveChanges();
                                    }
                                    int couponno = 0;
                                    //Add default coupons
                                    List<coupontemplate> ctemp = dataContext.coupontemplates.Where(x => x.CategoryId == mch.Category).ToList();
                                    if (ctemp != null)
                                    {
                                        if (ctemp.Count > 0)
                                        {
                                            for (int i = 0; i < ctemp.Count; i++)
                                            {
                                                coupons_master cpn = new coupons_master();

                                                cpn.CouponTitle = ctemp[i].CouponTitle;

                                                cpn.CouponCode = GenerateCouponCode(mch.merchantid);

                                                cpn.CouponDetails = ctemp[i].CouponDetails;

                                                cpn.MerchantId = mch.merchantid;

                                                cpn.ValidFrom = DateTime.Now;

                                                cpn.ValidTill = DateTime.Now.AddYears(10);

                                                cpn.categoryid = ctemp[i].CategoryId;
                                                cpn.PercentageOff = ctemp[i].PercentageOff;
                                                cpn.Discount = ctemp[i].Discount;
                                                cpn.AboveAmount = ctemp[i].AboveAmount;

                                                cpn.ValidAtLocation = ctemp[i].ValidAtLocation;
                                                //uncomment to generate QR Code
                                                //byte[] byteimage = GenerateQRCode(cpn.CouponCode);
                                                //cpn.QRCode = byteimage;
                                                cpn.MaxDiscount = ctemp[i].MaxDiscount;
                                                cpn.ShareWithAll = 1;

                                                cpn.DateCreated = DateTime.Now;

                                                dataContext.coupons_master.Add(cpn);
                                                dataContext.SaveChanges();
                                            }
                                        }
                                    }



                                    List<eventcoupontemplate> etemp = dataContext.eventcoupontemplates.Where(x => x.CategoryId == mch.Category).ToList();
                                    if (etemp != null)
                                    {
                                        if (etemp.Count > 0)
                                        {
                                            for (int i = 0; i < etemp.Count; i++)
                                            {
                                                coupons_master cpn = new coupons_master();

                                                cpn.CouponTitle = etemp[i].CouponTitle;

                                                cpn.CouponCode = GenerateCouponCode(mch.merchantid);

                                                cpn.CouponDetails = etemp[i].CouponDetails;

                                                cpn.MerchantId = mch.merchantid;

                                                cpn.ValidFrom = DateTime.Now;

                                                //cpn.ValidTill = DateTime.Now.AddMonths(6);

                                                cpn.categoryid = etemp[i].CategoryId;
                                                cpn.PercentageOff = etemp[i].PercentageOff;
                                                cpn.Discount = etemp[i].Discount;
                                                cpn.AboveAmount = etemp[i].AboveAmount;

                                                cpn.ValidAtLocation = etemp[i].ValidAtLocation;
                                                //uncomment to generate QR Code
                                                //byte[] byteimage = GenerateQRCode(cpn.CouponCode);
                                                //cpn.QRCode = byteimage;
                                                cpn.MaxDiscount = etemp[i].MaxDiscount;
                                                cpn.ShareWithAll = 0;

                                                cpn.DateCreated = DateTime.Now;

                                                dataContext.coupons_master.Add(cpn);
                                                dataContext.SaveChanges();

                                                //Add this coupon in eventcouponmaster
                                                eventcoupondetail eventcoupon = new eventcoupondetail();
                                                eventcoupon.CouponId = cpn.couponid;
                                                eventcoupon.EventId = etemp[i].EventId;
                                                eventcoupon.MerchantId = mch.merchantid;
                                                dataContext.eventcoupondetails.Add(eventcoupon);
                                                dataContext.SaveChanges();
                                            }
                                        }
                                    }

                                    if (mch.UserName.Contains("@") == false)
                                    {
                                        SMSUtility sms = new SMSUtility();
                                        string smsresult = sms.sendMessage(mch.UserName, "Dear " + mch.DECName + ", Welcome to Offertraker! Please download the Offertraker Business app for Android at goo.gl/FzvjZi and for Apple at goo.gl/m8QRCE. Your cell number is your login and your password is 123456.");
                                    }
                                    else
                                    {
                                        EmailModel model = new EmailModel();
                                        model.To = mch.UserName;
                                        model.Email = "welcome@offertraker.com";
                                        model.Subject = "Welcome to Offertraker!";

                                        model.Body = "Dear " + mch.DECName + ",<br /><br /> Welcome to Offertraker! Please download the Offertraker Business app for Android at goo.gl/FzvjZi and for Apple at goo.gl/m8QRCE. Your cell number is your login and your password is 123456.";
                                        SendEmail email = new SendEmail();
                                        email.SendEmailToConsumer(model);
                                    }



                                    return "Merchant Added Successfully";
                                }
                            }
                            else
                            {
                                return "Not able to register merchant";
                            }
                        }
                        else
                        {
                            return "Invalid Merchant Details";
                        }
                    }
                    catch (Exception ex)
                    {
                        //call error view here
                        //log the error
                        EventLog.LogErrorData("Error occured Merchant/AddNewMerchant." + ex.Message, true);
                        return "Error occured while adding new merchant";
                    }
                }
                else
                    return "Unauthorized access";
            }
            else
                return "Unauthorized access";
        }


        public string UpdateMerchant(merchant_master mch)
        {
            if (Request.IsAuthenticated)
            {
                if (Session["AdminUserId"] != null)
                {
                    try
                    {
                        if (mch != null)
                        {
                            using (instadelightEntities dataContext = new instadelightEntities())
                            {
                                int no = Convert.ToInt32(mch.merchantid);
                                var merchant = dataContext.merchant_master.Where(x => x.merchantid == no).FirstOrDefault();
                                merchant.MerchantName = mch.MerchantName;
                                merchant.DECName = mch.DECName;
                                merchant.button1_text = mch.button1_text;
                                merchant.button1_url = mch.button1_url;
                                merchant.button2_text = mch.button2_text;
                                merchant.button2_url = mch.button2_url;
                                merchant.button3_text = mch.button3_text;
                                merchant.button3_url = mch.button3_url;
                                merchant.button4_text = mch.button4_text;
                                merchant.button4_url = mch.button4_url;

                                merchant.BuildingName = mch.BuildingName;
                                merchant.SocietyName = mch.SocietyName;
                                merchant.Street = mch.Street;
                                merchant.Location = mch.Location;
                                merchant.City = mch.City;

                                merchant.Country = mch.Country;
                                merchant.State = mch.State;

                                merchant.PinCode = mch.PinCode;
                                merchant.MerchantLogo = mch.MerchantLogo;
                                merchant.MerchantDEC = mch.MerchantDEC;

                                merchant.RunRewardProgram = mch.RunRewardProgram;
                                if (merchant.MerchantDEC == null)
                                {
                                    if (string.IsNullOrEmpty(mch.merchantDecFromLibrary) == true)
                                    {
                                        business_category_master cat = dataContext.business_category_master.Where(x => x.categoryid == mch.Category).FirstOrDefault();
                                        if (cat != null)
                                        {
                                            merchant.merchantDecFromLibrary = "/Images/BackgroundImages/" + cat.CategoryName.Replace(" ", "_").Replace("/", "_") + ".jpg";
                                        }
                                    }
                                    else
                                    {
                                        merchant.merchantDecFromLibrary = mch.merchantDecFromLibrary;
                                    }
                                }

                                merchant.Category = mch.Category;

                                merchant.Email = mch.Email;
                                dataContext.SaveChanges();

                                //Reward options
                                if (mch.RunRewardProgram == false)
                                {
                                    redeemoption opt = new redeemoption();
                                    opt.MerchantId = mch.UserId;
                                    opt.Option1 = mch.redeemoptions.Option1;
                                    opt.Option2 = mch.redeemoptions.Option2;
                                    opt.Option3 = mch.redeemoptions.Option3;
                                    opt.Option4 = mch.redeemoptions.Option4;
                                    opt.Option5 = mch.redeemoptions.Option5;
                                    opt.CreationDate = DateTime.Now;
                                    dataContext.redeemoptions.Add(opt);
                                    dataContext.SaveChanges();
                                }

                                if (Convert.ToInt32(mch.JoiningBonus) != 0)
                                {
                                    merchantjoiningbonu bonus = new merchantjoiningbonu();
                                    bonus.MerchantId = mch.UserId;
                                    bonus.JoiningBonus = Convert.ToInt32(mch.JoiningBonus);
                                    bonus.DateCreated = DateTime.Now;
                                    dataContext.merchantjoiningbonus.Add(bonus);
                                    dataContext.SaveChanges();
                                }

                                if (mch.RewardRs != null && mch.RewardPoints != null)
                                {
                                    if (Convert.ToInt32(mch.RedeemRs) != 0 && Convert.ToInt32(mch.RewardPoints) != 0)
                                    {
                                        //Adding reward entries
                                        rewardmaster rdm = new rewardmaster();
                                        rdm.RewardName = "Reward Points";
                                        rdm.MerchantId = mch.UserId;
                                        rdm.RewardRs = mch.RewardRs;
                                        rdm.RewardPoints = mch.RewardPoints;
                                        rdm.CreationDate = DateTime.Now;
                                        dataContext.rewardmasters.Add(rdm);
                                        dataContext.SaveChanges();
                                    }
                                }

                                ////Adding Redeem entries
                                if (mch.RedeemPt != null && mch.RedeemRs != null)
                                {
                                    if (Convert.ToInt32(mch.RedeemPt) != 0 && Convert.ToInt32(mch.RedeemRs) != 0)
                                    {
                                        redeemmaster redm = new redeemmaster();
                                        redm.MerchantId = mch.UserId;
                                        redm.RedeemRs = mch.RedeemRs;
                                        redm.RedeemPt = mch.RedeemPt;
                                        redm.CreationDate = DateTime.Now;
                                        dataContext.redeemmasters.Add(redm);
                                        dataContext.SaveChanges();
                                    }
                                }

                                //Add default review questions
                                reviewmaster revmaster = dataContext.reviewmasters.Where(x => x.MerchantId == mch.merchantid).FirstOrDefault();
                                if (revmaster == null)
                                {
                                    reviewtemplate rtemp = dataContext.reviewtemplates.Where(x => x.CategoryId == mch.Category).FirstOrDefault();
                                    if (rtemp != null)
                                    {
                                        reviewmaster rev = new reviewmaster();
                                        rev.Question1 = rtemp.Question1;
                                        rev.Question1Type = rtemp.Question1Type;
                                        rev.Question2 = rtemp.Question2;
                                        rev.Question2Type = rtemp.Question2Type;
                                        rev.Question3 = rtemp.Question3;
                                        rev.Question3Type = rtemp.Question3Type;
                                        rev.Question4 = rtemp.Question4;
                                        rev.Question4Type = rtemp.Question4Type;

                                        rev.DefaultQuestion = "Share Our DEC with friends.";
                                        rev.DefaultType = "YesNo";
                                        rev.MerchantId = mch.merchantid;
                                        rev.MerchantUserId = mch.UserId;
                                        rev.CreationDate = DateTime.Now;

                                        dataContext.reviewmasters.Add(rev);
                                        dataContext.SaveChanges();
                                    }
                                }

                                coupons_master cpnmaster = dataContext.coupons_master.Where(x => x.MerchantId == mch.merchantid).FirstOrDefault();
                                if (cpnmaster == null)
                                {
                                    //Add default coupons
                                    List<coupontemplate> ctemp = dataContext.coupontemplates.Where(x => x.CategoryId == mch.Category).ToList();
                                    if (ctemp != null)
                                    {
                                        if (ctemp.Count > 0)
                                        {
                                            for (int i = 0; i < ctemp.Count; i++)
                                            {
                                                coupons_master cpn = new coupons_master();

                                                cpn.CouponTitle = ctemp[i].CouponTitle;


                                                cpn.CouponCode = GenerateCouponCode(mch.merchantid);

                                                cpn.CouponDetails = ctemp[i].CouponDetails;

                                                cpn.MerchantId = mch.merchantid;

                                                cpn.ValidFrom = DateTime.Now;

                                                cpn.ValidTill = DateTime.Now.AddYears(10);

                                                cpn.categoryid = ctemp[i].CategoryId;
                                                cpn.PercentageOff = ctemp[i].PercentageOff;
                                                cpn.Discount = ctemp[i].Discount;
                                                cpn.AboveAmount = ctemp[i].AboveAmount;

                                                cpn.ValidAtLocation = ctemp[i].ValidAtLocation;
                                                //uncomment to generate QR Code
                                                //byte[] byteimage = GenerateQRCode(cpn.CouponCode);
                                                //cpn.QRCode = byteimage;
                                                cpn.MaxDiscount = ctemp[i].MaxDiscount;
                                                cpn.ShareWithAll = 1;

                                                cpn.DateCreated = DateTime.Now;

                                                dataContext.coupons_master.Add(cpn);
                                                dataContext.SaveChanges();
                                            }
                                        }
                                    }
                                }

                                eventcoupondetail evntcpn = dataContext.eventcoupondetails.Where(x => x.MerchantId == mch.merchantid).FirstOrDefault();
                                if (evntcpn == null)
                                {
                                    //Add default event coupons
                                    List<eventcoupontemplate> etemp = dataContext.eventcoupontemplates.Where(x => x.CategoryId == mch.Category).ToList();
                                    if (etemp != null)
                                    {
                                        if (etemp.Count > 0)
                                        {
                                            for (int i = 0; i < etemp.Count; i++)
                                            {
                                                coupons_master cpn = new coupons_master();

                                                cpn.CouponTitle = etemp[i].CouponTitle;

                                                cpn.CouponCode = GenerateCouponCode(mch.merchantid);

                                                cpn.CouponDetails = etemp[i].CouponDetails;

                                                cpn.MerchantId = mch.merchantid;

                                                cpn.ValidFrom = DateTime.Now;

                                                // cpn.ValidTill = DateTime.Now.AddYears(10);

                                                cpn.categoryid = etemp[i].CategoryId;
                                                cpn.PercentageOff = etemp[i].PercentageOff;
                                                cpn.Discount = etemp[i].Discount;
                                                cpn.AboveAmount = etemp[i].AboveAmount;

                                                cpn.ValidAtLocation = etemp[i].ValidAtLocation;
                                                //uncomment to generate QR Code
                                                //byte[] byteimage = GenerateQRCode(cpn.CouponCode);
                                                //cpn.QRCode = byteimage;
                                                cpn.MaxDiscount = etemp[i].MaxDiscount;
                                                cpn.ShareWithAll = 0;

                                                cpn.DateCreated = DateTime.Now;

                                                dataContext.coupons_master.Add(cpn);
                                                dataContext.SaveChanges();

                                                //Add this coupon in eventcouponmaster
                                                eventcoupondetail eventcoupon = new eventcoupondetail();
                                                eventcoupon.CouponId = cpn.couponid;
                                                eventcoupon.EventId = etemp[i].EventId;
                                                eventcoupon.MerchantId = mch.merchantid;
                                                dataContext.eventcoupondetails.Add(eventcoupon);
                                                dataContext.SaveChanges();
                                            }
                                        }
                                    }
                                }
                                return "Merchant Updated";
                            }
                        }
                        else
                        {
                            return "Invalid Merchant";
                        }
                    }
                    catch (Exception ex)
                    {
                        //call error view here
                        //log the error
                        EventLog.LogErrorData("Error occured Merchant/UpdateMerchant." + ex.Message, true);
                        return "Error occured while updating merchant";
                    }
                }
                else
                    return "Unauthorized access";
            }
            else
                return "Unauthorized access";
        }

        public byte[] GenerateQRCode(string cpncode)
        {
            byte[] byteImage = null;
            try
            {
                //create qr code
                string url = @"http://103.233.76.193:8088/Merchant/GetCouponFromCouponCode?couponcode=" + cpncode;
                //string url = @"http://localhost:49989/Merchant/GetCouponFromCouponCode?couponcode=" + cpncode;

                QRCodeGenerator qrGenerator = new QRCodeGenerator();
                QRCodeGenerator.QRCode qrCode = qrGenerator.CreateQrCode(url, QRCodeGenerator.ECCLevel.Q);


                System.Web.UI.WebControls.Image imgBarCode = new System.Web.UI.WebControls.Image();
                imgBarCode.Height = 150;
                imgBarCode.Width = 150;
                using (Bitmap bitMap = qrCode.GetGraphic(20))
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        bitMap.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                        byteImage = ms.ToArray();
                    }
                }
                return byteImage;
            }
            catch (Exception ex)
            {
                EventLog.LogErrorData("Error occured in Merchant/GenerateQRCode: " + ex.Message, true);
                return byteImage;
            }
        }

        public string GenerateCouponCode(int MerchantId)
        {
            string couponCode = "OTA001";
            /*Coupon Code should be autogenerated as OTA<3 digits>.
              Once we reach OTA999, the code should start the series OTB<3 digits>.
                Then once we reach OTB999, it should start the series OTC<3 digits>.
                Once we reach OTZ999, we can start again with OTA<3 digits>.
                This logic applies to each business. So each business will have the first coupon code as OTA001.
            */
            //First coupon
            using (instadelightEntities dataContext = new instadelightEntities())
            {
                int cpncnt = dataContext.coupons_master.Where(x => x.MerchantId == MerchantId).Count();
                if (cpncnt == 0)
                {
                    couponCode = "OTA001";
                }
                else
                {
                    string lastcouponcode = dataContext.coupons_master.Where(x => x.MerchantId == MerchantId).OrderByDescending(x => x.couponid).FirstOrDefault().CouponCode;

                    string initial = "OT";
                    string series = lastcouponcode.Substring(2, 1);
                    string number = lastcouponcode.Substring(3);
                    if (Convert.ToInt32(number) < 999)
                    {
                        int nextno = Convert.ToInt32(number) + 1;
                        couponCode = initial + series + nextno.ToString().PadLeft(3, '0');
                    }
                    else
                    {
                        char letter = Convert.ToChar(series);
                        char nextchar = ' ';
                        if (letter == 'Z')
                            nextchar = 'A';
                        else
                            nextchar = (char)(((int)letter) + 1);

                        couponCode = initial + nextchar + "001";
                    }
                }
            }
            return couponCode;

        }
        public JsonResult getAll()
        {
            if (Request.IsAuthenticated)
            {
                if (Session["AdminUserId"] != null)
                {
                    try
                    {
                        using (instadelightEntities dataContext = new instadelightEntities())
                        {
                            var merchantList = dataContext.merchant_master.ToList();
                            var jsonResult = Json(merchantList, JsonRequestBehavior.AllowGet);
                            jsonResult.ContentType = "application/json";
                            jsonResult.MaxJsonLength = Int32.MaxValue;

                            return jsonResult;

                            //return Json(merchantList, JsonRequestBehavior.AllowGet);
                        }
                    }
                    catch (Exception ex)
                    {
                        //call error view here
                        //log the error
                        EventLog.LogErrorData("Error occured Merchant/getAll." + ex.Message, true);
                        return Json("Error occured while retrieving merchant list", JsonRequestBehavior.AllowGet);
                    }
                }
                else
                    return Json("Unauthorized access", JsonRequestBehavior.AllowGet);
            }
            else
                return Json("Unauthorized access", JsonRequestBehavior.AllowGet);
        }


        public JsonResult getMerchantRedeems(string merchantid)
        {
            if (Request.IsAuthenticated)
            {
                if (Session["AdminUserId"] != null)
                {
                    try
                    {
                        using (instadelightEntities dataContext = new instadelightEntities())
                        {
                            var merchantRedeem = dataContext.redeemmasters.Where(x => x.MerchantId == merchantid).OrderByDescending(x => x.CreationDate).FirstOrDefault();
                            return Json(merchantRedeem, JsonRequestBehavior.AllowGet);
                        }
                    }
                    catch (Exception ex)
                    {
                        //call error view here
                        //log the error
                        EventLog.LogErrorData("Error occured Merchant/getMerchantRedeems." + ex.Message, true);
                        return Json("Error occured while retrieving merchant redeems", JsonRequestBehavior.AllowGet);
                    }
                }
                else
                    return Json("Unauthorized access", JsonRequestBehavior.AllowGet);
            }
            else
                return Json("Unauthorized access", JsonRequestBehavior.AllowGet);
        }

        public JsonResult getMerchantRewards(string merchantid)
        {
            if (Request.IsAuthenticated)
            {
                if (Session["AdminUserId"] != null)
                {
                    try
                    {
                        using (instadelightEntities dataContext = new instadelightEntities())
                        {
                            var merchantRewards = dataContext.rewardmasters.Where(x => x.MerchantId == merchantid).OrderByDescending(x => x.CreationDate).FirstOrDefault();
                            return Json(merchantRewards, JsonRequestBehavior.AllowGet);
                        }
                    }
                    catch (Exception ex)
                    {
                        //call error view here
                        //log the error
                        EventLog.LogErrorData("Error occured Merchant/getMerchantRewards." + ex.Message, true);
                        return Json("Error occured while retrieving merchant rewards", JsonRequestBehavior.AllowGet);
                    }
                }
                else
                    return Json("Unauthorized access", JsonRequestBehavior.AllowGet);
            }
            else
                return Json("Unauthorized access", JsonRequestBehavior.AllowGet);
        }


        //Search businesses

        public JsonResult searchMerchants(clsSearchParameters param)
        {
            if (Request.IsAuthenticated)
            {
                if (Session["AdminUserId"] != null)
                {
                    try
                    {
                        string userid = Session["AdminUserId"].ToString();
                        using (instadelightEntities dataContext = new instadelightEntities())
                        {
                            user user = dataContext.users.Where(x => x.Id == userid).FirstOrDefault();

                            var predicate = PredicateBuilder.New<merchant_master>();
                            if (!string.IsNullOrEmpty(param.country))
                            {
                                country_master country = dataContext.country_master.Where(x => x.countryname == param.country).FirstOrDefault();
                                if (country != null)
                                {
                                    predicate = predicate.And(p => p.Country == country.countryid);
                                }
                            }

                            if (!string.IsNullOrEmpty(param.state))
                            {
                                state_master state = dataContext.state_master.Where(x => x.state == param.state).FirstOrDefault();
                                if (state != null)
                                {
                                    predicate = predicate.And(p => p.State == state.stateid);
                                }

                            }

                            if (!string.IsNullOrEmpty(param.city))
                            {
                                predicate = predicate.And(p => p.City == param.city);
                            }

                            if (!string.IsNullOrEmpty(param.pin))
                            {
                                predicate = predicate.And(p => p.PinCode == Convert.ToInt32(param.pin));
                            }

                            if (!string.IsNullOrEmpty(param.mobile))
                            {
                                predicate = predicate.And(p => p.PhoneNumber == param.mobile);
                            }

                            if (!string.IsNullOrEmpty(param.pin))
                            {
                                predicate = predicate.And(p => p.PinCode == Convert.ToInt32(param.pin));
                            }

                            if (user.VARCode == "INSTADELIGHT")
                            {
                                if (!string.IsNullOrEmpty(param.VAR))
                                {
                                    predicate = predicate.And(p => p.VARCode == param.VAR);
                                }
                            }
                            else if (!string.IsNullOrEmpty(user.VARCode))
                            {
                                predicate = predicate.And(p => p.VARCode == user.VARCode);
                            }

                            if (!string.IsNullOrEmpty(param.Srep))
                            {
                                predicate = predicate.And(p => p.rname == param.Srep);
                            }

                            if (param.validfrom != DateTime.MinValue && param.validtill != DateTime.MinValue)
                            {
                                predicate = predicate.And(p => p.creation_date >= param.validfrom && p.creation_date <= param.validtill);
                            }

                            if (!string.IsNullOrEmpty(param.business))
                            {
                                business_category_master businessCategory = dataContext.business_category_master.Where(x => x.CategoryName.Contains(param.business)).FirstOrDefault();
                                if (businessCategory != null)
                                {
                                    predicate = predicate.And(p => p.Category == businessCategory.categoryid);
                                }
                                else
                                {
                                    predicate = predicate.And(p => p.Category == 0);
                                }
                            }

                            if (predicate.IsStarted)
                            {
                                var merchants = dataContext.merchant_master.Where(predicate.Compile()).ToList();
                                foreach (merchant_master m in merchants)
                                {
                                    business_category_master businessCategory = dataContext.business_category_master.Where(x => x.categoryid == m.Category).FirstOrDefault();
                                    if (businessCategory != null)
                                    {
                                        m.CategoryName = businessCategory.CategoryName;
                                    }

                                    country_master country = dataContext.country_master.Where(x => x.countryid == m.Country).FirstOrDefault();
                                    if (country != null)
                                    {
                                        m.CountryName = country.countryname;
                                    }

                                    state_master state = dataContext.state_master.Where(x => x.stateid == m.State).FirstOrDefault();
                                    if (state != null)
                                    {
                                        m.StateName = state.state;
                                    }

                                    //Getting consumer count here
                                    var consumercount = dataContext.merchantconsumerdetails.Where(x => x.MerchantId == m.UserId).Select(x => x.ConsumerId).Distinct().Count();
                                    if (consumercount > 0)
                                    {
                                        m.ConsumerCount = consumercount;
                                    }

                                    var smscount = dataContext.merchantsmsdetails.Where(x => x.MerchantId == m.UserId).FirstOrDefault();
                                    if (smscount != null)
                                    {
                                        m.SMSCount = smscount.SMSCount;
                                        m.EmailCount = smscount.EmailCount;
                                    }

                                }

                                var jsonResult = Json(merchants, JsonRequestBehavior.AllowGet);
                                jsonResult.ContentType = "application/json";
                                jsonResult.MaxJsonLength = Int32.MaxValue;


                                return jsonResult;
                            }
                            else
                            {
                                var merchants = dataContext.merchant_master.ToList();
                                foreach (merchant_master m in merchants)
                                {
                                    business_category_master businessCategory = dataContext.business_category_master.Where(x => x.categoryid == m.Category).FirstOrDefault();
                                    if (businessCategory != null)
                                    {
                                        m.CategoryName = businessCategory.CategoryName;
                                    }

                                    country_master country = dataContext.country_master.Where(x => x.countryid == m.Country).FirstOrDefault();
                                    if (country != null)
                                    {
                                        m.CountryName = country.countryname;
                                    }

                                    state_master state = dataContext.state_master.Where(x => x.stateid == m.State).FirstOrDefault();
                                    if (state != null)
                                    {
                                        m.StateName = state.state;
                                    }
                                    //Getting consumer count here
                                    var consumercount = dataContext.merchantconsumerdetails.Where(x => x.MerchantId == m.UserId).Select(x => x.ConsumerId).Distinct().Count();
                                    if (consumercount > 0)
                                    {
                                        m.ConsumerCount = consumercount;
                                    }

                                    var smscount = dataContext.merchantsmsdetails.Where(x => x.MerchantId == m.UserId).FirstOrDefault();
                                    if (smscount != null)
                                    {
                                        m.SMSCount = smscount.SMSCount;
                                        m.EmailCount = smscount.EmailCount;
                                    }

                                }
                                var jsonResult = Json(merchants, JsonRequestBehavior.AllowGet);
                                jsonResult.ContentType = "application/json";
                                jsonResult.MaxJsonLength = Int32.MaxValue;


                                return jsonResult;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        //call error view here
                        //log the error
                        EventLog.LogErrorData("Error occured Merchant/searchMerchants." + ex.Message, true);
                        return Json("Error occured while searching merchants", JsonRequestBehavior.AllowGet);
                    }
                }
                else
                    return Json("Unauthorized access", JsonRequestBehavior.AllowGet);
            }
            else
                return Json("Unauthorized access", JsonRequestBehavior.AllowGet);
        }

        public ActionResult exportResult(string Country, string State, string VARCode, string City, string PIN, string MobileNumber, string BusinessName, string ValidFrom, string ValidTill, string rname)
        {
            if (Request.IsAuthenticated)
            {
                if (Session["AdminUserId"] != null)
                {
                    try
                    {
                        string userid = Session["AdminUserId"].ToString();
                        using (instadelightEntities dataContext = new instadelightEntities())
                        {
                            user user = dataContext.users.Where(x => x.Id == userid).FirstOrDefault();
                            var predicate = PredicateBuilder.New<merchant_master>();
                            if (!string.IsNullOrEmpty(Country))
                            {
                                country_master country = dataContext.country_master.Where(x => x.countryname == Country).FirstOrDefault();
                                if (country != null)
                                {
                                    predicate = predicate.And(p => p.Country == country.countryid);
                                }
                            }

                            if (!string.IsNullOrEmpty(State))
                            {
                                state_master state = dataContext.state_master.Where(x => x.state == State).FirstOrDefault();
                                if (state != null)
                                {
                                    predicate = predicate.And(p => p.State == state.stateid);
                                }
                            }

                            if (!string.IsNullOrEmpty(City))
                            {
                                predicate = predicate.And(p => p.City == City);
                            }

                            if (!string.IsNullOrEmpty(PIN))
                            {
                                predicate = predicate.And(p => p.PinCode == Convert.ToInt32(PIN));
                            }

                            if (!string.IsNullOrEmpty(MobileNumber))
                            {
                                predicate = predicate.And(p => p.PhoneNumber == MobileNumber);
                            }

                            if (user.VARCode == "INSTADELIGHT")
                            {
                                if (!string.IsNullOrEmpty(VARCode))
                                {
                                    predicate = predicate.And(p => p.VARCode == VARCode);
                                }
                            }
                            else if (!string.IsNullOrEmpty(user.VARCode))
                            {
                                predicate = predicate.And(p => p.VARCode == user.VARCode);
                            }



                            if (!string.IsNullOrEmpty(rname))
                            {
                                predicate = predicate.And(p => p.rname == rname);
                            }

                            if (string.IsNullOrEmpty(ValidFrom) == false && string.IsNullOrEmpty(ValidTill) == false)
                            {
                                ValidFrom = ValidFrom.Replace("\"", "");
                                ValidTill = ValidTill.Replace("\"", "");
                                DateTime fromdate = Convert.ToDateTime(ValidFrom);
                                DateTime todate = Convert.ToDateTime(ValidTill);

                                predicate = predicate.And(p => p.creation_date >= fromdate && p.creation_date <= todate);
                            }

                            if (!string.IsNullOrEmpty(BusinessName))
                            {
                                business_category_master businessCategory = dataContext.business_category_master.Where(x => x.CategoryName.Contains(BusinessName)).FirstOrDefault();
                                if (businessCategory != null)
                                {
                                    predicate = predicate.And(p => p.Category == businessCategory.categoryid);
                                }
                                else
                                {
                                    predicate = predicate.And(p => p.Category == 0);
                                }
                            }

                            if (predicate.IsStarted)
                            {
                                List<merchant_master> merchants = dataContext.merchant_master.Where(predicate.Compile()).ToList();

                                ExportToCSV(merchants);
                                ViewBag.result = "Results exported successfully.";

                                return View();
                            }
                            else
                            {
                                List<merchant_master> merchants = dataContext.merchant_master.ToList();

                                ExportToCSV(merchants);
                                ViewBag.result = "Results exported successfully.";

                                return View();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        //call error view here
                        //log the error
                        EventLog.LogErrorData("Error occured Merchant/exportResult." + ex.Message, true);
                        return RedirectToAction("Login", "Account");
                    }
                }
                else
                    return RedirectToAction("Login", "Account");
            }
            else
                return RedirectToAction("Login", "Account");
        }

        public void ExportToCSV(List<merchant_master> merchants)
        {
            StringWriter sw = new StringWriter();
            instadelightEntities dataContext = new instadelightEntities();

            sw.WriteLine("\"Merchant Name\",\"Address\",\"City\",\"Pin Code\",\"State\",\"Country\",\"Registered Mobile Number\",\"Email id\",\"Merchant Activation Date\",\"Business type\",\"VAR Code\",\"Number of Consumers\",\"SMS Sent\",\"SMS Balance\",\"Emails Sent\"");

            Response.ClearContent();
            Response.AddHeader("content-disposition", "attachment;filename=ExportResult.csv");
            Response.ContentType = "text/csv";


            foreach (var m in merchants)
            {
                string businessname = "";
                if (m.Category != 0)
                {
                    business_category_master businessCategory = dataContext.business_category_master.Where(x => x.categoryid == m.Category).FirstOrDefault();
                    if (businessCategory != null)
                    {
                        businessname = businessCategory.CategoryName;
                    }
                }

                if (m.Country != 0)
                {
                    country_master country = dataContext.country_master.Where(x => x.countryid == m.Country).FirstOrDefault();
                    if (country != null)
                    {
                        m.CountryName = country.countryname;
                    }
                }

                if (m.State != 0)
                {
                    state_master state = dataContext.state_master.Where(x => x.stateid == m.State).FirstOrDefault();
                    if (state != null)
                    {
                        m.StateName = state.state;
                    }
                }

                //Getting consumer count here
                var consumercount = dataContext.merchantconsumerdetails.Where(x => x.MerchantId == m.UserId).Select(x => x.ConsumerId).Distinct().Count();
                if (consumercount > 0)
                {
                    m.ConsumerCount = consumercount;
                }

                var smscount = dataContext.merchantsmsdetails.Where(x => x.MerchantId == m.UserId).FirstOrDefault();
                if (smscount != null)
                {
                    m.SMSCount = smscount.SMSCount;
                    m.EmailCount = smscount.EmailCount;
                }

                sw.WriteLine(string.Format("\"{0}\",\"{1}\",\"{2}\",\"{3}\",\"{4}\",\"{5}\",\"{6}\",\"{7}\",\"{8}\",\"{9}\",\"{10}\",\"{11}\",\"{12}\",\"{13}\",\"{14}\"",
                    m.MerchantName,
                    m.BuildingName + ", " + m.SocietyName + ", " + m.Street + ", " + m.Location,
                    m.City,
                    m.PinCode,
                    m.StateName,
                    m.CountryName,
                    m.PhoneNumber,
                    m.Email,
                    Convert.ToDateTime(m.creation_date).ToString("dd/MM/yyyy"),
                    businessname,
                    m.VARCode,
                    m.ConsumerCount,
                    m.NoOfSMS,
                    m.SMSCount,
                    m.EmailCount
                ));
            }
            Response.Write(sw.ToString());
            Response.Flush();
            Response.End();

        }

        public string activateMerchants(string merchantids)
        {
            if (Request.IsAuthenticated)
            {
                if (Session["AdminUserId"] != null)
                {
                    try
                    {
                        if (merchantids != "")
                        {
                            merchantids = merchantids.Remove(merchantids.LastIndexOf(","));
                            string[] mnos = merchantids.Split(',');
                            if (mnos.Length > 0)
                            {
                                for (int i = 0; i < mnos.Length; i++)
                                {
                                    using (instadelightEntities dataContext = new instadelightEntities())
                                    {
                                        int no = Convert.ToInt32(mnos[i]);
                                        merchant_master merchant = dataContext.merchant_master.Where(x => x.merchantid == no).FirstOrDefault();
                                        if (merchant != null)
                                        {
                                            merchant.activation = "Active";
                                            dataContext.SaveChanges();
                                        }
                                    }
                                }

                                return "Merchants activated successfully.";
                            }
                            else
                                return "";
                        }
                        else
                        {
                            return "";
                        }
                    }
                    catch (Exception ex)
                    {
                        //call error view here
                        //log the error
                        EventLog.LogErrorData("Error occured Merchant/activateMerchants." + ex.Message, true);
                        return "Error occured while activating merchant";
                    }
                }
                else
                    return "Unauthorized access";
            }
            else
                return "Unauthorized access";
        }

        public string deActivateMerchants(string merchantids)
        {
            if (Request.IsAuthenticated)
            {
                if (Session["AdminUserId"] != null)
                {
                    try
                    {
                        if (merchantids != "")
                        {
                            merchantids = merchantids.Remove(merchantids.LastIndexOf(","));
                            string[] mnos = merchantids.Split(',');
                            if (mnos.Length > 0)
                            {
                                for (int i = 0; i < mnos.Length; i++)
                                {
                                    using (instadelightEntities dataContext = new instadelightEntities())
                                    {
                                        int no = Convert.ToInt32(mnos[i]);
                                        merchant_master merchant = dataContext.merchant_master.Where(x => x.merchantid == no).FirstOrDefault();
                                        if (merchant != null)
                                        {
                                            merchant.activation = "Deactive";
                                            dataContext.SaveChanges();
                                        }
                                    }
                                }

                                return "Merchants deactivated successfully.";
                            }
                            else
                                return "";
                        }
                        else
                        {
                            return "";
                        }
                    }
                    catch (Exception ex)
                    {
                        //call error view here
                        //log the error
                        EventLog.LogErrorData("Error occured Merchant/deActivateMerchants." + ex.Message, true);
                        return "Error occured while deactivating merchant";
                    }
                }
                else
                    return "Unauthorized access";
            }
            else
                return "Unauthorized access";
        }

        public string SetGiftcardDiscount(string merchantids, string discount)
        {
            if (Request.IsAuthenticated)
            {
                if (Session["AdminUserId"] != null)
                {
                    try
                    {
                        if (merchantids != "" && discount != "")
                        {
                            int giftdiscount = Convert.ToInt32(discount);

                            merchantids = merchantids.Remove(merchantids.LastIndexOf(","));
                            string[] mnos = merchantids.Split(',');
                            if (mnos.Length > 0)
                            {
                                for (int i = 0; i < mnos.Length; i++)
                                {
                                    using (instadelightEntities dataContext = new instadelightEntities())
                                    {
                                        int no = Convert.ToInt32(mnos[i]);
                                        merchant_master merchant = dataContext.merchant_master.Where(x => x.merchantid == no).FirstOrDefault();
                                        if (merchant != null)
                                        {
                                            merchant.GiftCardDiscount = giftdiscount;
                                            dataContext.SaveChanges();
                                        }
                                    }
                                }

                                return "Gift discount set successfully for selected merchants.";
                            }
                            else
                                return "";
                        }
                        else
                        {
                            return "";
                        }
                    }
                    catch (Exception ex)
                    {
                        //call error view here
                        //log the error
                        EventLog.LogErrorData("Error occured Merchant/activateMerchants." + ex.Message, true);
                        return "Error occured while activating merchant";
                    }
                }
                else
                    return "Unauthorized access";
            }
            else
                return "Unauthorized access";
        }

        public string SetBenefits(List<bank_benefits> benefits)
        {
            if (Request.IsAuthenticated)
            {
                if (Session["AdminUserId"] != null)
                {
                    try
                    {
                        if (benefits != null)
                        {
                            if (benefits.Count > 0)
                            {
                                for (int i = 0; i < benefits.Count; i++)
                                {
                                    using (instadelightEntities dataContext = new instadelightEntities())
                                    {
                                        int no = Convert.ToInt32(benefits[i].MerchantId);
                                        merchant_master merchant = dataContext.merchant_master.Where(x => x.merchantid == no).FirstOrDefault();

                                        if (merchant != null)
                                        {
                                            bank_benefits b = new bank_benefits();
                                            b.MerchantId = merchant.UserId;
                                            b.BankName = benefits[i].BankName;
                                            b.Benefit = benefits[i].Benefit;
                                            b.URL = benefits[i].URL;
                                            dataContext.bank_benefits.Add(b);
                                            dataContext.SaveChanges();
                                        }
                                    }
                                }

                                return "Bank benefits set successfully for selected merchants.";
                            }
                            else
                            {
                                return "";
                            }
                        }
                        else
                        {
                            return "";
                        }
                    }
                    catch (Exception ex)
                    {
                        //call error view here
                        //log the error
                        EventLog.LogErrorData("Error occured Merchant/SetBanefits." + ex.Message, true);
                        return "Error occured while setting bank benefits for merchant";
                    }
                }
                else
                    return "Unauthorized access";
            }
            else
                return "Unauthorized access";

        }
    }
}