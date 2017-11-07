using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ConsumerApp.Models;
using Microsoft.AspNet.Identity;
using AspNet.Identity.MySQL;
using MySql.Data.MySqlClient;
using ConsumerApp.Filters;
using Microsoft.Owin.Security;

using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity.Owin;
using System.Text;

namespace ConsumerApp.Controllers
{

    public class ConsumerController : Controller
    {
        ApplicationDbContext Consumercontext = new ApplicationDbContext("DefaultConsumerConnection");
        ConsumerCommonFunctions function = new ConsumerCommonFunctions();

        // GET: Consumer
        [AdminConsumerFilter]
        public ActionResult Index()
        {
            string userid = Session["UserId"].ToString();
            instadelight_consumerEntities dataContext = new instadelight_consumerEntities();
            consumermaster consumer = dataContext.consumermasters.Where(x => x.UserId == userid).FirstOrDefault();

            ViewBag.ConsumerId = consumer.id;

            return View();
        }

        [AdminConsumerFilter]
        public ActionResult Redeem()
        {
            return View();
        }

        [AdminConsumerFilter]
        public ActionResult Benefits(string merchantid)
        {
            int no = Convert.ToInt32(merchantid);
            consumerEntities datacontext = new consumerEntities();
            merchant_master master = datacontext.merchant_master.Where(x => x.merchantid == no).FirstOrDefault();
            ViewBag.MerchantName = master.DECName;
            merchant_benefits benefits = datacontext.merchant_benefits.Where(x => x.MerchantId == master.UserId).OrderByDescending(x => x.CreationDate).FirstOrDefault();

            if (benefits != null)
            {
                ViewBag.option1 = benefits.Benefit1;
                ViewBag.option2 = benefits.Benefit2;
                ViewBag.option3 = benefits.Benefit3;
                ViewBag.option4 = benefits.Benefit4;
                ViewBag.option5 = benefits.Benefit5;
            }

            List<bank_benefits> bbenefits = datacontext.bank_benefits.Where(x => x.MerchantId == master.UserId).ToList();


            return View(bbenefits);
        }


        public ActionResult GetNewDEC()
        {
            if (Session["UserId"] != null)
            {
                string userid = Session["UserId"].ToString();
                instadelight_consumerEntities dataContext = new instadelight_consumerEntities();
                user consumer = dataContext.users.Where(x => x.Id == userid).FirstOrDefault();

                ViewBag.ConsumerPhone = consumer.UserName;
            }
            else
            {
                ViewBag.ConsumerPhone = "";
            }
            return View();
        }

        [AdminConsumerFilter]
        public ActionResult ChangeCountry()
        {
            string userid = Session["UserId"].ToString();
            instadelight_consumerEntities dataContext = new instadelight_consumerEntities();
            consumermaster consumer = dataContext.consumermasters.Where(x => x.UserId == userid).FirstOrDefault();
            ViewBag.countryid = consumer.Country;

            ViewBag.languageid = consumer.LanguageId;
            return View();
        }

        //Get merchant pending review page
        [AdminConsumerFilter]
        public ActionResult MerchantPendingReviews()
        {
            return View();
        }

        //Get merchant pending review page comments
        [AdminConsumerFilter]
        public ActionResult MerchantPendingReviewsComment(string merchant, string review)
        {
            ViewBag.merchantid = merchant;
            ViewBag.reviewId = review;
            return View();
        }

        [AdminConsumerFilter]
        public ActionResult PendingReviews(string id)
        {
            TempData["TempMerchantId"] = id;
            ViewBag.merchantid = id;
            ViewBag.consumerid = Session["UserId"].ToString();
            return View();
        }

        [AdminConsumerFilter]
        public ActionResult SendDec(string Id)
        {
            TempData["Id"] = Id;
            ViewBag.MerchantId = Id;

            string userid = Session["UserId"].ToString();
            using (instadelight_consumerEntities dataContext = new instadelight_consumerEntities())
            {
                consumermaster consumer = dataContext.consumermasters.Where(x => x.UserId == userid).FirstOrDefault();
                ViewBag.countryid = consumer.Country;
            }
            return View();
        }

        [AdminConsumerFilter]
        public ActionResult SendCoupon(string MerchantId, string CouponId, string SharedCouponId)
        {
            ViewBag.MerchantId = MerchantId;
            ViewBag.CouponID = CouponId;
            ViewBag.Id = SharedCouponId;

            string userid = Session["UserId"].ToString();
            using (instadelight_consumerEntities dataContext = new instadelight_consumerEntities())
            {
                consumermaster consumer = dataContext.consumermasters.Where(x => x.UserId == userid).FirstOrDefault();
                ViewBag.countryid = consumer.Country;
            }
            return View();
        }

        //save Review Comments
        [AdminConsumerFilter]
        public string SaveReviewComment(string ReviewId, string MerchantId, string Comment)
        {
            try
            {
                string userid = Session["UserId"].ToString();
                int merchantno = Convert.ToInt32(MerchantId);
                int reviewId = Convert.ToInt32(ReviewId);
                string reviewComment = Comment;

                using (consumerEntities dataContext = new consumerEntities())
                {
                    merchantreviewcomment mr = new merchantreviewcomment();
                    mr.MerchantId = MerchantId;
                    mr.ReviewId = reviewId;
                    mr.ConsumerUserId = userid;
                    mr.AddedDatetime = DateTime.Now;
                    mr.Comment = reviewComment;
                    dataContext.merchantreviewcomments.Add(mr);
                    dataContext.SaveChanges();
                    return Global.Consumer.CommentSaved;
                }
            }
            catch (Exception ex)
            {
                //call error view here
                //log the error
                EventLog.LogErrorData("Error occurred Consumer/SaveReviewComment." + ex.Message, true);
                return Global.Consumer.SaveCommentException;
            }
        }

        #region ShareDEC
        [AdminConsumerFilter]
        public string AddNewDECConsumer(string mobileno)
        {
            try
            {
                string MerchantId = TempData["Id"].ToString();
                if (mobileno != null)
                {
                    string userid = Session["UserId"].ToString();


                    string result = function.AddNewDECConsumer(mobileno, MerchantId, userid, Session["UserName"].ToString());
                    return result;
                }
                else
                {
                    return Global.Consumer.InvalidPhoneNumber;
                }
            }
            catch (Exception ex)
            {
                //call error view here
                //log the error
                EventLog.LogErrorData("Error occurred Consumer/AddNewDECConsumer." + ex.Message, true);
                return Global.Consumer.ShareDECException;
            }

        }

        public string GetDECFromMerchant(string mobileno, string businessmobileno)
        {
            try
            {
                if (mobileno != null && businessmobileno != null)
                {
                    consumerEntities dataContext = new consumerEntities();

                    merchantuser isMerchant = dataContext.merchantusers.Where(x => x.UserName == businessmobileno).FirstOrDefault();
                    if (isMerchant == null)
                    {
                        return Global.Consumer.InvalidMerchantErrorMessage.Replace("xxx", businessmobileno);
                    }

                    string result = function.AddNewDECConsumer(mobileno, isMerchant.Id, "", "");


                    return Global.Consumer.ShareDECMessage;
                }
                else
                {
                    return Global.Consumer.InvalidPhoneNumber;
                }
            }

            catch (Exception ex)
            {
                //call error view here
                //log the error
                EventLog.LogErrorData("Error occurred Consumer/GetDECFromMerchant." + ex.Message, true);
                return Global.Consumer.ShareDECException;
            }
        }

        [AdminConsumerFilter]
        public string AddCheckCouponConsumer(string mobileno, string MerchantId, string CouponId, string SharedCouponId)
        {
            try
            {
                if (mobileno != null)
                {
                    merchant_master master = new merchant_master();
                    int no = Convert.ToInt32(MerchantId);

                    using (consumerEntities dataContext = new consumerEntities())
                    {
                        master = dataContext.merchant_master.Where(u => u.merchantid == no).FirstOrDefault();
                    }

                    if (master != null)
                    {
                        string result = function.AddCheckCouponConsumer(mobileno, master.UserId, CouponId, SharedCouponId, Session["UserName"].ToString());
                        return result;
                    }
                    else
                    {
                        return "Merchant does not exist";
                    }
                }
                else
                {
                    return Global.Consumer.MobileNumberPrompt;
                }
            }
            catch (Exception ex)
            {
                //call error view here
                //log the error
                EventLog.LogErrorData("Error occurred Consumer/AddCheckCouponConsumer." + ex.Message, true);
                return Global.Consumer.ShareCouponException;
            }
        }

     
        

   
        [AdminConsumerFilter]
        public void SendReviewsToDECs(reviewmaster rvw)
        {
            try
            {
                using (consumerEntities dataContext = new consumerEntities())
                {

                    MySqlParameter param1 = new MySqlParameter();
                    param1.Value = rvw.MerchantUserId;
                    param1.Direction = System.Data.ParameterDirection.Input;
                    param1.ParameterName = "@MId";
                    param1.DbType = System.Data.DbType.String;


                    List<string> ConsumerList = dataContext.Database.SqlQuery<string>("CALL GetConsumersToShareReview(@MId)", param1)
                                   .Select(x => x).ToList();

                    foreach (string c in ConsumerList)
                    {
                      function.MapMerchantReviewToConsumer(rvw.MerchantUserId, c, rvw.reviewid);
                    }

                }
            }
            catch (Exception ex)
            {
                //call error view here
                //log the error
                EventLog.LogErrorData("Error occurred Consumer/SendReviewsToDECs." + ex.Message, true);
            }

        }

      



        #endregion
        [AdminConsumerFilter]
        public JsonResult getAll()
        {
            try
            {
                using (consumerEntities dataContext = new consumerEntities())
                {
                    string userid = Session["UserId"].ToString();

                    var s = string.Join(",", dataContext.bankconsumerdetails.Where(p => p.ConsumerId == userid)
                                       .Select(p => p.BankId.ToString()));
                    if (s != "")
                    {
                        var idlist = s.Split(',').Select(n => Convert.ToInt32(n)).ToArray();

                        //var bankList = from x in dataContext.bank_master.Select(x => x).ToList()
                        //               where idlist.Contains(Convert.ToInt32(x.bankid))
                        //               select x;

                        var bankList = from x in StaticCache.GetBanks().Select(x => x).ToList()
                                       where idlist.Contains(Convert.ToInt32(x.bankid))
                                       select x;
                        var jsonResult = Json(bankList, JsonRequestBehavior.AllowGet);
                        jsonResult.ContentType = "application/json";
                        jsonResult.MaxJsonLength = Int32.MaxValue;

                        return jsonResult;
                    }
                    else
                    {
                        return Json("", JsonRequestBehavior.AllowGet);
                    }
                }
            }
            catch (Exception ex)
            {
                //call error view here
                //log the error
                EventLog.LogErrorData("Error occurred Consumer/getAll." + ex.Message, true);
                return Json(Global.Consumer.GetAllException, JsonRequestBehavior.AllowGet);
            }
        }

        [AdminConsumerFilter]
        public JsonResult GetAllMerchants()
        {
            try
            {
                //start timer
                //DateTime startImport = DateTime.Now;
                //DateTime endImport = DateTime.Now;
                //TimeSpan span = new TimeSpan();
                //EventLog.LogData("Get coupon list start at " + DateTime.Now + ".", true);

                using (consumerEntities dataContext = new consumerEntities())
                {
                    string userid = Session["UserId"].ToString();

                    var s = string.Join(",", dataContext.merchantconsumerdetails.Where(p => p.ConsumerId == userid)
                                       .Select(p => p.MerchantId.ToString()));
                    if (s != "")
                    {
                        var idlist = s.Split(',').Select(n => n).ToArray();

                        //var merchantList = (from x in dataContext.merchant_master
                        //                    from y in dataContext.coupons_master.Where(o => x.merchantid == o.MerchantId).OrderByDescending(o => o.ValidTill).ThenByDescending(o => o.Discount).ThenByDescending(o => o.PercentageOff).Take(1)
                        //                    where idlist.Contains(x.UserId) && x.activation != "Deactive"
                        //                    select new { merchantid = x.merchantid, MerchantName = x.MerchantName, DECName = x.DECName, MerchantLogo = x.MerchantLogo, CouponTitle = y.CouponTitle, UserId = x.UserId, PercentOff = y.PercentageOff, Discount = y.Discount, button1_text = x.button1_text, button2_text = x.button2_text, button2_url = x.button2_url, button3_text = x.button3_text, button3_url = x.button3_url, button4_text = x.button4_text }).ToList();

                        var merchantList = (from x in dataContext.merchant_master
                                            where idlist.Contains(x.UserId) && x.activation != "Deactive"
                                            select new { merchantid = x.merchantid, MerchantName = x.MerchantName, DECName = x.DECName, MerchantLogo = x.MerchantLogo, UserId = x.UserId, button1_text = x.button1_text, button2_text = x.button2_text, button2_url = x.button2_url, button3_text = x.button3_text, button3_url = x.button3_url, button4_text = x.button4_text }).ToList();

                        //var merchantList = (from x in StaticCache.GetMerchants()
                        //                    where idlist.Contains(x.UserId) && x.activation != "Deactive"
                        //                    select new { merchantid = x.merchantid, MerchantName = x.MerchantName, DECName = x.DECName, MerchantLogo = x.MerchantLogo, UserId = x.UserId, button1_text = x.button1_text, button2_text = x.button2_text, button2_url = x.button2_url, button3_text = x.button3_text, button3_url = x.button3_url, button4_text = x.button4_text }).ToList();


                        //endImport = DateTime.Now;
                        //span = new TimeSpan();
                        //span = endImport.Subtract(startImport);
                        //EventLog.LogData("Total time to get coupons for all merchants", true);
                        //EventLog.LogData(span.TotalSeconds.ToString(), true);
                        var jsonResult = Json(merchantList, JsonRequestBehavior.AllowGet);
                        jsonResult.ContentType = "application/json";
                        jsonResult.MaxJsonLength = Int32.MaxValue;

                        return jsonResult;

                        // return Json(merchantList, JsonRequestBehavior.AllowGet);


                    }
                    else
                    {
                        return Json("", JsonRequestBehavior.AllowGet);
                    }
                }
            }
            catch (Exception ex)
            {
                //call error view here
                //log the error
                EventLog.LogErrorData("Error occurred Consumer/getAllMerchants." + ex.Message, true);
                return Json(Global.Consumer.GetMerchantsException, JsonRequestBehavior.AllowGet);
            }
        }

        [AdminConsumerFilter]
        public JsonResult GetCountrycode()
        {
            try
            {
                using (instadelight_consumerEntities dataContext = new instadelight_consumerEntities())
                {
                    var UserId = User.Identity.GetUserId();
                    var UsercountryId = dataContext.consumermasters.Where(x => x.UserId == UserId).Select(x => x.Country).FirstOrDefault();

                    if (UsercountryId != 0)
                    {
                        var country = dataContext.country_master.Where(c => c.countryid == UsercountryId).FirstOrDefault();
                        return Json(country, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json("", JsonRequestBehavior.AllowGet);
                    }
                }
            }
            catch (Exception ex)
            {
                //call error view here
                //log the error
                EventLog.LogErrorData("Error occurred Consumer/GetCountryCode." + ex.Message, true);
                return Json(Global.Consumer.GetCountryCodeException, JsonRequestBehavior.AllowGet);
            }
        }

        [AdminConsumerFilter]
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
                return Json(Global.Consumer.GetCountryCodeException, JsonRequestBehavior.AllowGet);
            }
        }

        public class CouponParameter
        {
            public string cityid { get; set; }
            public string locationid { get; set; }
            public string categoryid { get; set; }
            public string bankid { get; set; }
        }

        [AdminConsumerFilter]
        public JsonResult GetCoupons(CouponParameter param)
        {
            try
            {
                //start timer
                //DateTime startImport = DateTime.Now;
                //DateTime endImport = DateTime.Now;
                //TimeSpan span = new TimeSpan();
                //EventLog.LogData("Get coupon list start at " + DateTime.Now + ".", true);

                using (consumerEntities dataContext = new consumerEntities())
                {
                    int bankno = Convert.ToInt32(param.bankid);
                    string userid = Session["UserId"].ToString();
                    int category = Convert.ToInt32(param.categoryid);

                    MySqlParameter param1 = new MySqlParameter();
                    param1.Value = param.cityid;
                    param1.Direction = System.Data.ParameterDirection.Input;
                    param1.ParameterName = "@MerchantCity";
                    param1.DbType = System.Data.DbType.String;

                    MySqlParameter param2 = new MySqlParameter();
                    param2.Value = category;
                    param2.Direction = System.Data.ParameterDirection.Input;
                    param2.ParameterName = "@MerchantCategory";
                    param2.DbType = System.Data.DbType.Int32;

                    MySqlParameter param3 = new MySqlParameter();
                    param3.Value = userid;
                    param3.Direction = System.Data.ParameterDirection.Input;
                    param3.ParameterName = "@CId";
                    param3.DbType = System.Data.DbType.String;

                    //List<coupons_master> CouponList = dataContext.Database.SqlQuery<coupons_master>("CALL GetCouponListForAllMerchants(@MerchantCity, @MerchantCategory,@CId)", param1, param2, param3)
                    //               .Select(x => x).ToList();
                    List<CouponList> CouponList = dataContext.Database.SqlQuery<CouponList>("CALL GetCouponListForAllMerchants(@MerchantCity,@MerchantCategory,@CId)", param1, param2, param3)
                                                              .Select(x => new CouponList
                                                              {
                                                                  Id = x.Id,
                                                                  couponid = x.couponid,
                                                                  CouponTitle = x.CouponTitle,
                                                                  CouponCode = x.CouponCode,
                                                                  CouponDetails = x.CouponDetails,
                                                                  MerchantId = x.MerchantId,
                                                                  ValidFrom = x.ValidFrom,
                                                                  ValidTill = x.ValidTill,
                                                                  categoryid = x.categoryid,
                                                                  PercentageOff = x.PercentageOff,
                                                                  Discount = x.Discount,
                                                                  AboveAmount = x.AboveAmount,
                                                                  ValidAtLocation = x.ValidAtLocation,
                                                                  DEC = x.DEC,
                                                                  //QRCode = x.QRCode,
                                                                  MaxDiscount = x.MaxDiscount,
                                                                  MaxCoupons = x.MaxCoupons,
                                                                  ShareWithAll = x.ShareWithAll,
                                                                  DateCreated = x.DateCreated
                                                              }).ToList();

                    var jsonResult = Json(CouponList, JsonRequestBehavior.AllowGet);
                    jsonResult.ContentType = "application/json";
                    jsonResult.MaxJsonLength = Int32.MaxValue;

                    //endImport = DateTime.Now;
                    //span = new TimeSpan();
                    //span = endImport.Subtract(startImport);
                    //EventLog.LogData("Total time to get coupons for all merchants", true);
                    //EventLog.LogData(span.TotalSeconds.ToString(), true);

                    return jsonResult;
                }
            }
            catch (Exception ex)
            {
                //call error view here
                //log the error
                EventLog.LogErrorData("Error occurred Consumer/GetCoupons." + ex.Message, true);
                return Json(Global.Consumer.GetCouponsException, JsonRequestBehavior.AllowGet);
            }
        }

        [AdminConsumerFilter]
        public JsonResult GetMerchantCoupons(string merchantid)
        {
            try
            {
                using (consumerEntities dataContext = new consumerEntities())
                {
                    string userid = Session["UserId"].ToString();
                    int merchantno = Convert.ToInt32(merchantid);
                    merchant_master mch = dataContext.merchant_master.Where(x => x.merchantid == merchantno).FirstOrDefault();

                    MySqlParameter param1 = new MySqlParameter();
                    param1.Value = mch.UserId;
                    param1.Direction = System.Data.ParameterDirection.Input;
                    param1.ParameterName = "@MId";
                    param1.DbType = System.Data.DbType.String;
                    MySqlParameter param2 = new MySqlParameter();
                    param2.Value = userid;
                    param2.Direction = System.Data.ParameterDirection.Input;
                    param2.ParameterName = "@CId";
                    param2.DbType = System.Data.DbType.String;

                    List<CouponList> CouponList = dataContext.Database.SqlQuery<CouponList>("CALL GetCouponListForMerchant(@MId,@CId)", param1, param2)
                                    .Select(x => new CouponList
                                    {
                                        Id = x.Id,
                                        couponid = x.couponid,
                                        CouponTitle = x.CouponTitle,
                                        CouponCode = x.CouponCode,
                                        CouponDetails = x.CouponDetails,
                                        MerchantId = x.MerchantId,
                                        ValidFrom = x.ValidFrom,
                                        ValidTill = x.ValidTill,
                                        categoryid = x.categoryid,
                                        PercentageOff = x.PercentageOff,
                                        Discount = x.Discount,
                                        AboveAmount = x.AboveAmount,
                                        ValidAtLocation = x.ValidAtLocation,
                                        DEC = x.DEC,
                                        //  QRCode = x.QRCode,
                                        MaxDiscount = x.MaxDiscount,
                                        MaxCoupons = x.MaxCoupons,
                                        ShareWithAll = x.ShareWithAll,
                                        DateCreated = x.DateCreated
                                    }).ToList();



                    //List<merchantconsumercoupondetail> CouponList = dataContext.Database.SqlQuery<merchantconsumercoupondetail>("CALL GetCouponListForMerchant(@MId,@CId)", param1, param2)
                    //              .Select(x => x).ToList();

                    var jsonResult = Json(CouponList, JsonRequestBehavior.AllowGet);
                    jsonResult.ContentType = "application/json";
                    jsonResult.MaxJsonLength = Int32.MaxValue;

                    return jsonResult;

                    //Show only coupons which are shared by merchants
                }
            }
            catch (Exception ex)
            {
                //call error view here
                //log the error
                EventLog.LogErrorData("Error occurred Consumer/GetMerchantCoupons." + ex.Message, true);
                return Json(Global.Consumer.GetCouponsException, JsonRequestBehavior.AllowGet);
            }
        }


        public JsonResult GetCountries()
        {
            try
            {
                using (instadelight_consumerEntities dataContext = new instadelight_consumerEntities())
                {
                    var countryList = dataContext.country_master.OrderBy(x => x.countryname).ToList();

                    return Json(countryList, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                //call error view here
                //log the error
                EventLog.LogErrorData("Error occurred Consumer/GetCountries." + ex.Message, true);
                return Json(Global.Consumer.GetCountryListException, JsonRequestBehavior.AllowGet);
            }
        }

        [AdminConsumerFilter]
        public JsonResult GetCities(string stateid)
        {
            try
            {

                using (consumerEntities dataContext = new consumerEntities())
                {
                    if (String.IsNullOrEmpty(stateid) == false)
                    {
                        int no = Convert.ToInt32(stateid);
                        var cityList = dataContext.city_master.Where(x => x.stateid == no).OrderBy(x => x.City).ToList();
                        return Json(cityList, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        var cityList = (from x in dataContext.merchant_master
                                        select new { City = x.City }).Distinct().ToList();
                        return Json(cityList, JsonRequestBehavior.AllowGet);
                    }
                }
            }
            catch (Exception ex)
            {
                //call error view here
                //log the error
                EventLog.LogErrorData("Error occurred Consumer/GetCities." + ex.Message, true);
                return Json(Global.Consumer.GetCityListException, JsonRequestBehavior.AllowGet);
            }
        }

        [AdminConsumerFilter]
        public JsonResult GetCategories()
        {
            try
            {
                using (consumerEntities dataContext = new consumerEntities())
                {
                    var categoryList = dataContext.business_category_master.OrderBy(x => x.CategoryName).ToList();

                    return Json(categoryList, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                //call error view here
                //log the error
                EventLog.LogErrorData("Error occurred Consumer/GetCategories." + ex.Message, true);
                return Json(Global.Consumer.GetCategoriesException, JsonRequestBehavior.AllowGet);
            }
        }

        [AdminConsumerFilter]
        public JsonResult GetLocations(string cityid)
        {
            try
            {
                using (consumerEntities dataContext = new consumerEntities())
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
                EventLog.LogErrorData("Error occurred Consumer/GetLocations." + ex.Message, true);
                return Json(Global.Consumer.GetLocationsException, JsonRequestBehavior.AllowGet);
            }
        }

        [AdminConsumerFilter]
        public JsonResult GetStates(string countryid)
        {
            try
            {
                using (instadelight_consumerEntities dataContext = new instadelight_consumerEntities())
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
                EventLog.LogErrorData("Error occurred Consumer/GetStates." + ex.Message, true);
                return Json(Global.Consumer.GetStatesException, JsonRequestBehavior.AllowGet);
            }
        }

        [AdminConsumerFilter]
        public JsonResult GetDECDetails(string bankid)
        {
            try
            {
                using (consumerEntities dataContext = new consumerEntities())
                {
                    int no = Convert.ToInt32(bankid);
                    var bankDECList = dataContext.bank_dec_details.Where(x => x.bankid == no).FirstOrDefault();

                    var jsonResult = Json(bankDECList, JsonRequestBehavior.AllowGet);
                    jsonResult.ContentType = "application/json";
                    jsonResult.MaxJsonLength = Int32.MaxValue;

                    return jsonResult;

                    //return Json(bankDECList, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                //call error view here
                //log the error
                EventLog.LogErrorData("Error occurred Consumer/GetDECDetails." + ex.Message, true);
                return Json(Global.Consumer.GetBankDECException, JsonRequestBehavior.AllowGet);
            }
        }

        [AdminConsumerFilter]
        public JsonResult GetMerchantDECDetails(string merchantid)
        {
            try
            {
                using (consumerEntities dataContext = new consumerEntities())
                {
                    int no = Convert.ToInt32(merchantid);
                    merchant_master merchantdetails = dataContext.merchant_master.Where(x => x.merchantid == no).FirstOrDefault();//StaticCache.GetMerchants().Where(x => x.merchantid == no).FirstOrDefault();
                    var merchantRewards = dataContext.rewardmasters.Where(x => x.MerchantId == merchantdetails.UserId).OrderByDescending(x => x.CreationDate).FirstOrDefault();
                    if (merchantRewards != null)
                    {
                        merchantdetails.RewardName = merchantRewards.RewardName;
                    }

                    var jsonResult = Json(merchantdetails, JsonRequestBehavior.AllowGet);
                    jsonResult.ContentType = "application/json";
                    jsonResult.MaxJsonLength = Int32.MaxValue;

                    return jsonResult;


                    // return Json(merchantdetails, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                //call error view here
                //log the error
                EventLog.LogErrorData("Error occurred Consumer/GetMerchantDECDetails." + ex.Message, true);
                return Json(Global.Consumer.GetMerchantDECException, JsonRequestBehavior.AllowGet);
            }
        }

        [AdminConsumerFilter]
        public JsonResult GetMerchantDetails(string merchantid)
        {
            try
            {
                using (consumerEntities dataContext = new consumerEntities())
                {
                    int no = Convert.ToInt32(merchantid);
                    var merchantDetails = dataContext.merchant_master.Find(no);

                    //var loc = dataContext.location_master.Find(merchantDetails.Location);
                    //merchantDetails.LocationName = loc.Location;

                    //var cty = dataContext.city_master.Find(merchantDetails.City);
                    //merchantDetails.CityName = cty.City;

                    var jsonResult = Json(merchantDetails, JsonRequestBehavior.AllowGet);
                    jsonResult.ContentType = "application/json";
                    jsonResult.MaxJsonLength = Int32.MaxValue;

                    return jsonResult;

                    //return Json(merchantDetails, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                //call error view here
                //log the error
                EventLog.LogErrorData("Error occurred Consumer/GetMerchantDetails." + ex.Message, true);
                return Json(Global.Consumer.GetMerchantDetails, JsonRequestBehavior.AllowGet);
            }
        }

        [AdminConsumerFilter]
        public JsonResult GetCouponDetails(string couponid, string SharedCouponId)
        {
            try
            {
                using (consumerEntities dataContext = new consumerEntities())
                {
                    int no = Convert.ToInt32(couponid);
                    int sharedcouponno = Convert.ToInt32(SharedCouponId);

                    coupons_master couponDetails = dataContext.coupons_master.Where(x => x.couponid == no).FirstOrDefault();
                    merchantconsumercoupondetail shareddetails = dataContext.merchantconsumercoupondetails.Where(x => x.Id == sharedcouponno).FirstOrDefault();
                    if (shareddetails != null)
                    {
                        couponDetails.ValidFrom = shareddetails.ValidFrom;
                        couponDetails.ValidTill = shareddetails.ValidTill;
                    }
                    var jsonResult = Json(couponDetails, JsonRequestBehavior.AllowGet);
                    jsonResult.ContentType = "application/json";
                    jsonResult.MaxJsonLength = Int32.MaxValue;

                    return jsonResult;

                    //return Json(couponDetails, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                //call error view here
                //log the error
                EventLog.LogErrorData("Error occurred Consumer/GetCouponDetails." + ex.Message, true);
                return Json(Global.Consumer.GetCouponDetails, JsonRequestBehavior.AllowGet);
            }
        }

        [AdminConsumerFilter]
        public JsonResult GetEventConditions(string couponid)
        {
            try
            {
                using (consumerEntities dataContext = new consumerEntities())
                {
                    int no = Convert.ToInt32(couponid);

                    eventcoupondetail couponDetails = dataContext.eventcoupondetails.Where(x => x.CouponId == no).FirstOrDefault();
                    if (couponDetails != null)
                    {
                        List<eventcouponcondition> cond = dataContext.eventcouponconditions.ToList();
                        var jsonResult = Json(cond, JsonRequestBehavior.AllowGet);
                        jsonResult.ContentType = "application/json";
                        jsonResult.MaxJsonLength = Int32.MaxValue;

                        return jsonResult;
                    }
                    else
                    {
                        List<eventcouponcondition> cond = new List<eventcouponcondition>();
                        var jsonResult = Json(cond, JsonRequestBehavior.AllowGet);
                        jsonResult.ContentType = "application/json";
                        jsonResult.MaxJsonLength = Int32.MaxValue;

                        return jsonResult;
                    }

                    //return Json(couponDetails, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                //call error view here
                //log the error
                EventLog.LogErrorData("Error occurred Consumer/GetEventConditions." + ex.Message, true);
                return Json(Global.Consumer.GetEventCondtiongsException, JsonRequestBehavior.AllowGet);
            }
        }

        [AdminConsumerFilter]
        public JsonResult getCouponConditions(string couponid)
        {
            try
            {
                using (consumerEntities dataContext = new consumerEntities())
                {
                    int no = Convert.ToInt32(couponid);

                    List<couponcondition> cond = dataContext.couponconditions.Where(x => x.couponid == no).ToList();
                    var jsonResult = Json(cond, JsonRequestBehavior.AllowGet);
                    jsonResult.ContentType = "application/json";
                    jsonResult.MaxJsonLength = Int32.MaxValue;

                    return jsonResult;
                    //return Json(couponDetails, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                //call error view here
                //log the error
                EventLog.LogErrorData("Error occurred Consumer/getCouponConditions." + ex.Message, true);
                return Json(Global.Consumer.GetCouponCondtiongsException, JsonRequestBehavior.AllowGet);
            }
        }

        //Create Consumer My Profile -PA
        [AdminConsumerFilter]
        public ActionResult MyProfile()
        {
            using (instadelight_consumerEntities dataContext = new instadelight_consumerEntities())
            {
                string userid = Session["UserId"].ToString();

                consumermaster master = dataContext.consumermasters.Where(x => x.UserId == userid).FirstOrDefault();
                if (master != null)
                {
                    ViewBag.id = master.id;

                    return View();
                }
                else
                {
                    return RedirectToAction("Login", "Account");
                }

            }
        }

        public ActionResult NewLogin(string username)
        {
            if (string.IsNullOrEmpty(username) == false)
            {
                IAuthenticationManager authManager = HttpContext.GetOwinContext().Authentication;
                ApplicationSignInManager signinmanager = HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
                ApplicationUserManager UserManager = HttpContext.GetOwinContext().Get<ApplicationUserManager>();

                if (username.Contains('@') == false)
                {
                    string[] PhonewithCode = username.Split(' ');
                    if (PhonewithCode.Length > 0)
                    {
                        if (PhonewithCode.Length == 3)
                        {
                            username = "+" + PhonewithCode[1] + " " + PhonewithCode[2].ToString();
                        }
                    }
                }

                var result = signinmanager.PasswordSignIn(username, "123456", false, shouldLockout: false);
                switch (result)
                {
                    case SignInStatus.Success:
                        {
                            var userId = signinmanager.AuthenticationManager.AuthenticationResponseGrant.Identity.GetUserId();
                            instadelight_consumerEntities dataContext = new instadelight_consumerEntities();
                            user currentuser = dataContext.users.Where(x => x.Id == userId).FirstOrDefault();

                            //deleted user is present in database but has allow logon = false
                            if (currentuser != null)
                            {
                                //add user id, role and name in session variables
                                Session["UserId"] = currentuser.Id;

                                Session["UserName"] = currentuser.FirstName + " " + currentuser.LastName;

                                if (currentuser.ChangePassword == 1)
                                {
                                    string code = UserManager.GeneratePasswordResetToken(userId);
                                    return RedirectToAction("ResetPassword", "Account", new { code = code, userid = userId });
                                }
                                consumermaster consumer = dataContext.consumermasters.Where(x => x.UserId == userId).FirstOrDefault();

                                if (Session["UserName"].ToString() == " ")
                                {

                                    if (consumer != null)
                                    {
                                        Session["UserName"] = consumer.consumername;
                                    }
                                }

                                if (Session["UserName"] == null)
                                {
                                    Session["UserName"] = username;
                                }

                                if (consumer.LanguageId != null)
                                {
                                    if (consumer.LanguageId != 0)
                                    {
                                        HttpCookie cookie = new HttpCookie("LanguageSelected");
                                        string lang = string.Empty;
                                        int languageid = Convert.ToInt32(consumer.LanguageId);

                                        language_master langmaster = dataContext.language_master.Where(x => x.LanguageiId == languageid).FirstOrDefault();

                                        cookie.Value = langmaster.LanguageCode;
                                        Response.SetCookie(cookie);
                                    }
                                }

                            }
                            return RedirectToAction("Index");//RedirectToLocal(returnUrl);
                        }
                    case SignInStatus.LockedOut:
                        return RedirectToAction("Login", "Account");
                    case SignInStatus.RequiresVerification:
                        return RedirectToAction("Login", "Account");
                    case SignInStatus.Failure:
                    default:
                        return RedirectToAction("Login", "Account");
                }
            }
            else
            {
                return RedirectToAction("Login");
            }
        }

        //Update Consumer -PA
        [AdminConsumerFilter]
        public string UpdateConsumer(consumermaster ch)
        {
            try
            {
                if (ch != null)
                {
                    using (instadelight_consumerEntities dataContext = new instadelight_consumerEntities())
                    {
                        int no = Convert.ToInt32(ch.id);
                        var consumer = dataContext.consumermasters.Where(x => x.id == no).FirstOrDefault();
                        consumer.consumername = ch.consumername;
                        consumer.Gender = ch.Gender;
                        consumer.DOA = ch.DOA;
                        consumer.DOB = ch.DOB;
                        consumer.BuildingName = ch.BuildingName;
                        consumer.SocietyName = ch.SocietyName;
                        consumer.Street = ch.Street;
                        consumer.Location = ch.Location;
                        consumer.City = ch.City;
                        consumer.Phone1 = ch.Phone1;
                        consumer.Phone2 = ch.Phone2;
                        consumer.Phone3 = ch.Phone3;
                        consumer.PinCode = ch.PinCode;

                        consumer.Country = ch.Country;
                        consumer.State = ch.State;
                        consumer.consumerlogo = ch.consumerlogo;
                        consumer.Email = ch.Email;
                        dataContext.SaveChanges();

                        Session["UserName"] = consumer.consumername;

                        return Global.Consumer.ConsumerUpdated;
                    }
                }
                else
                {
                    return Global.Consumer.InvalidConsumer;
                }
            }
            catch (Exception ex)
            {
                //call error view here
                //log the error
                EventLog.LogErrorData("Error occurred Consumer/UpdateConsumer." + ex.Message, true);
                return Global.Consumer.UpdateConsumerException;
            }
        }

        //Send OTP via mail
        [AdminConsumerFilter]
        public string sendOTPtoEmail(string EmailId)
        {
            //string email = "";            
            try
            {
                string userid = Session["UserId"].ToString();

                using (instadelight_consumerEntities dataContext = new instadelight_consumerEntities())
                {
                    // email = dataContext.consumermasters.Where(x => x.UserId == userid).Select(x => x.Email).FirstOrDefault();

                    if (EmailId != "")
                    {
                        //Generate OTP

                        string newotp = "";
                        consumer_otp_details currentotpdetails = dataContext.consumer_otp_details.Where(x => x.ConsumerId == userid).FirstOrDefault();
                        if (currentotpdetails != null)
                        {
                            if (currentotpdetails.EmailOTP != null)
                            {
                                newotp = currentotpdetails.EmailOTP;
                            }
                            else
                            {
                                newotp = RandomNumber.GenerateRandomOTP(6);
                            }
                        }
                        else
                        {
                            newotp = RandomNumber.GenerateRandomOTP(6);
                        }


                        if (string.IsNullOrEmpty(newotp) == false)
                        {
                            //Send email
                            EmailModel model = new EmailModel();
                            model.To = EmailId;
                            model.Email = "welcome@offertraker.com";
                            model.Subject = "OTP for email confirmation";
                            string body = "Welcome to Offertraker, <br /><br /><p>You have added this email-id as an additional identity for receiving DECs from businesses. <br>Your OTP is <b>" + newotp.ToString() + " </b> <br /></p>";
                            body += "<p> Please enter this OTP in the Offertraker app to confirm addition of this email-id in your profile.</p><br />";
                            body += "<p> In case you did not initiate this action in the Offertraker app, then please DO NOT share the above OTP with anyone.</p><br />";
                            body += "Thanks,<br /><br /> Offertraker Team";
                            model.Body = body;
                            SendEmail eml = new SendEmail();
                            string smsresult = eml.SendEmailToConsumer(model);

                            if (smsresult.Contains("Email sent"))
                            {

                                //Store in database
                                consumer_otp_details otpdetails = dataContext.consumer_otp_details.Where(x => x.ConsumerId == userid).FirstOrDefault();
                                if (otpdetails == null)
                                {
                                    otpdetails = new consumer_otp_details();
                                    otpdetails.ConsumerId = userid;
                                    otpdetails.EmailOTP = newotp;
                                    dataContext.consumer_otp_details.Add(otpdetails);
                                    dataContext.SaveChanges();
                                }
                                else
                                {
                                    otpdetails.EmailOTP = newotp;
                                    dataContext.SaveChanges();
                                }
                                return Global.Consumer.SendOTPMessage;
                            }
                            else
                            {
                                return Global.Consumer.OTPErrorMessage1.Replace("xxx", EmailId);
                            }
                        }
                        else
                        {
                            return Global.Consumer.OTPErrorMessage2;
                        }

                    }
                    else
                    {
                        return Global.Consumer.InvalidEmail;
                    }
                }
            }
            catch (Exception ex)
            {
                //call error view here
                //log the error
                EventLog.LogErrorData("Error occurred Consumer/sendOTPtoEmail." + ex.Message, true);
                return Global.Consumer.SendOTPException;
            }
        }

        //Verify OTP 
        [AdminConsumerFilter]
        public string VerifyEmailOTP(string OTP)
        {
            try
            {
                string userid = Session["UserId"].ToString();
                using (instadelight_consumerEntities dataContext = new instadelight_consumerEntities())
                {

                    if (string.IsNullOrEmpty(OTP) == false)
                    {
                        consumer_otp_details otpdetail = dataContext.consumer_otp_details.Where(x => x.ConsumerId == userid && x.EmailOTP == OTP).FirstOrDefault();
                        if (otpdetail != null)
                        {
                            //Valid OTP
                            otpdetail.EmailOTP = "";
                            dataContext.SaveChanges();
                            consumermaster consumer = dataContext.consumermasters.Where(x => x.UserId == userid).FirstOrDefault();
                            consumer.EmailVerified = true;
                            dataContext.SaveChanges();
                            return Global.Consumer.OTPVerified;
                        }
                        else
                        {
                            return Global.Consumer.InvalidOTP;
                        }
                    }
                    else
                    {
                        return Global.Consumer.InvalidOTP;
                    }
                }
            }
            catch (Exception ex)
            {
                //call error view here
                //log the error
                EventLog.LogErrorData("Error occurred Consumer/VerifyEmailOTP." + ex.Message, true);
                return Global.Consumer.VerifyOTPException;
            }
        }

        //Send OTP via mail
        [AdminConsumerFilter]
        public string sendOTPtoPhone2(string PhoneNumber)
        {
            try
            {
                string userid = Session["UserId"].ToString();
                // string phone2 = "";
                using (instadelight_consumerEntities dataContext = new instadelight_consumerEntities())
                {
                    // phone2 = dataContext.consumermasters.Where(x => x.UserId == userid).Select(x => x.Phone2).FirstOrDefault();

                    if (PhoneNumber != "")
                    {
                        //Generate OTP
                        string[] PhonewithCode = PhoneNumber.Split(' ');
                        if (PhonewithCode.Length > 0)
                        {
                            if (PhonewithCode.Length == 2)
                            {
                                PhoneNumber = PhonewithCode[1].ToString();
                            }

                        }

                        string newotp = "";
                        consumer_otp_details currentotpdetails = dataContext.consumer_otp_details.Where(x => x.ConsumerId == userid).FirstOrDefault();
                        if (currentotpdetails != null)
                        {
                            if (currentotpdetails.Phone2OTP != null)
                            {
                                newotp = currentotpdetails.Phone2OTP;
                            }
                            else
                            {
                                newotp = RandomNumber.GenerateRandomOTP(6);
                            }
                        }
                        else
                        {
                            newotp = RandomNumber.GenerateRandomOTP(6);
                        }

                        if (string.IsNullOrEmpty(newotp) == false)
                        {
                            //Send sms
                            SMSUtility sms = new SMSUtility();
                            string smsresult = sms.sendMessage(PhoneNumber, "Dear Customer, you have added a new cell number in your Offertraker profile. Please enter the OTP " + newotp + " in the app to confirm addition of this number in your profile.");

                            if (smsresult.Contains("SMS sent successfully"))
                            {
                                //Store in database
                                consumer_otp_details otpdetails = dataContext.consumer_otp_details.Where(x => x.ConsumerId == userid).FirstOrDefault();
                                if (otpdetails == null)
                                {
                                    otpdetails = new consumer_otp_details();
                                    otpdetails.ConsumerId = userid;
                                    otpdetails.Phone2OTP = newotp;
                                    dataContext.consumer_otp_details.Add(otpdetails);
                                    dataContext.SaveChanges();
                                }
                                else
                                {
                                    otpdetails.Phone2OTP = newotp;
                                    dataContext.SaveChanges();
                                }
                                return Global.Consumer.SendOTPMessage;
                            }
                            else
                            {
                                return Global.Consumer.OTPErrorMessage3.Replace("nnn", PhoneNumber);
                            }
                        }
                        else
                        {
                            return Global.Consumer.OTPErrorMessage2;
                        }

                    }
                    else
                    {
                        return Global.Consumer.InvalidPhoneNumber;
                    }
                }
            }
            catch (Exception ex)
            {
                //call error view here
                //log the error
                EventLog.LogErrorData("Error occurred Consumer/sendOTPtoPhone2." + ex.Message, true);
                return Global.Consumer.SendOTPException1;
            }
        }

        //Verify OTP
        [AdminConsumerFilter]
        public string Verifyphone2otp(string OTP)
        {
            try
            {
                string userid = Session["UserId"].ToString();
                using (instadelight_consumerEntities dataContext = new instadelight_consumerEntities())
                {

                    if (string.IsNullOrEmpty(OTP) == false)
                    {
                        consumer_otp_details otpdetail = dataContext.consumer_otp_details.Where(x => x.ConsumerId == userid && x.Phone2OTP == OTP).FirstOrDefault();
                        if (otpdetail != null)
                        {
                            //Valid OTP
                            otpdetail.Phone2OTP = "";
                            dataContext.SaveChanges();
                            consumermaster consumer = dataContext.consumermasters.Where(x => x.UserId == userid).FirstOrDefault();
                            consumer.Phone2Verified = true;
                            dataContext.SaveChanges();
                            return Global.Consumer.OTPVerified;
                        }
                        else
                        {
                            return Global.Consumer.InvalidOTP;
                        }
                    }
                    else
                    {
                        return Global.Consumer.InvalidOTP;
                    }
                }
            }
            catch (Exception ex)
            {
                //call error view here
                //log the error
                EventLog.LogErrorData("Error occurred Consumer/Verifyphone2otp." + ex.Message, true);
                return Global.Consumer.VerifyOTPException1;
            }
        }

        //Send OTP via mail
        [AdminConsumerFilter]
        public string sendOTPtoPhone3(string PhoneNumber)
        {
            try
            {
                string userid = Session["UserId"].ToString();
                //string phone3 = "";
                using (instadelight_consumerEntities dataContext = new instadelight_consumerEntities())
                {
                    //phone3 = dataContext.consumermasters.Where(x => x.UserId == userid).Select(x => x.Phone3).FirstOrDefault();

                    if (PhoneNumber != "")
                    {
                        //Generate OTP
                        string[] PhonewithCode = PhoneNumber.Split(' ');
                        if (PhonewithCode.Length > 0)
                        {
                            if (PhonewithCode.Length == 2)
                            {
                                PhoneNumber = PhonewithCode[1].ToString();
                            }
                        }


                        string newotp = "";
                        consumer_otp_details currentotpdetails = dataContext.consumer_otp_details.Where(x => x.ConsumerId == userid).FirstOrDefault();
                        if (currentotpdetails != null)
                        {
                            if (currentotpdetails.Phone3OTP != null)
                            {
                                newotp = currentotpdetails.Phone3OTP;
                            }
                            else
                            {
                                newotp = RandomNumber.GenerateRandomOTP(6);
                            }
                        }
                        else
                        {
                            newotp = RandomNumber.GenerateRandomOTP(6);
                        }

                        if (string.IsNullOrEmpty(newotp) == false)
                        {
                            //Send sms
                            SMSUtility sms = new SMSUtility();
                            string smsresult = sms.sendMessage(PhoneNumber, "Dear Customer, you have added a new cell number in your Offertraker profile. Please enter the OTP " + newotp + " in the app to confirm addition of this number in your profile.");
                            if (smsresult.Contains("SMS sent successfully"))
                            {
                                //Store in database
                                consumer_otp_details otpdetails = dataContext.consumer_otp_details.Where(x => x.ConsumerId == userid).FirstOrDefault();
                                if (otpdetails == null)
                                {
                                    otpdetails = new consumer_otp_details();
                                    otpdetails.ConsumerId = userid;
                                    otpdetails.Phone3OTP = newotp;
                                    dataContext.consumer_otp_details.Add(otpdetails);
                                    dataContext.SaveChanges();
                                }
                                else
                                {
                                    otpdetails.Phone3OTP = newotp;
                                    dataContext.SaveChanges();
                                }
                                return Global.Consumer.SendOTPMessage;
                            }
                            else
                            {
                                return Global.Consumer.OTPErrorMessage3.Replace("nnn", PhoneNumber);
                            }
                        }
                        else
                        {
                            return Global.Consumer.OTPErrorMessage2;
                        }

                    }
                    else
                    {
                        return Global.Consumer.InvalidPhoneNumber;
                    }
                }
            }
            catch (Exception ex)
            {
                //call error view here
                //log the error
                EventLog.LogErrorData("Error occurred Consumer/sendOTPtoPhone2." + ex.Message, true);
                return Global.Consumer.SendOTPException2;
            }
        }

        //Verify OTP
        [AdminConsumerFilter]
        public string Verifyphone3otp(string OTP)
        {
            try
            {
                string userid = Session["UserId"].ToString();
                using (instadelight_consumerEntities dataContext = new instadelight_consumerEntities())
                {

                    if (string.IsNullOrEmpty(OTP) == false)
                    {
                        consumer_otp_details otpdetail = dataContext.consumer_otp_details.Where(x => x.ConsumerId == userid && x.Phone3OTP == OTP).FirstOrDefault();
                        if (otpdetail != null)
                        {
                            //Valid OTP
                            otpdetail.Phone3OTP = "";
                            dataContext.SaveChanges();
                            consumermaster consumer = dataContext.consumermasters.Where(x => x.UserId == userid).FirstOrDefault();
                            consumer.Phone3Verified = true;
                            dataContext.SaveChanges();
                            return Global.Consumer.OTPVerified;
                        }
                        else
                        {
                            return Global.Consumer.InvalidOTP;
                        }
                    }
                    else
                    {
                        return Global.Consumer.InvalidOTP;
                    }
                }
            }
            catch (Exception ex)
            {
                //call error view here
                //log the error
                EventLog.LogErrorData("Error occurred Consumer/Verifyphone3otp." + ex.Message, true);
                return Global.Consumer.VerifyOTPException2;
            }
        }

        //Get Consumer by Id -PA
        [AdminConsumerFilter]
        public JsonResult GetConsumerById(string consumerid)
        {
            try
            {
                using (instadelight_consumerEntities dataContext = new instadelight_consumerEntities())
                {
                    int no = Convert.ToInt32(consumerid);
                    var consumer = dataContext.consumermasters.Find(no);
                    var jsonResult = Json(consumer, JsonRequestBehavior.AllowGet);
                    jsonResult.ContentType = "application/json";
                    jsonResult.MaxJsonLength = Int32.MaxValue;

                    return jsonResult;

                    //return Json(consumer, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                //call error view here
                //log the error
                EventLog.LogErrorData("Error occurred Consumer/GetConsumerById." + ex.Message, true);
                return Json(Global.Consumer.GetConsumerByIdException, JsonRequestBehavior.AllowGet);
            }
        }

        [AdminConsumerFilter]
        public JsonResult getConsumerLogo()
        {
            try
            {

                using (instadelight_consumerEntities dataContext = new instadelight_consumerEntities())
                {
                    var UserId = User.Identity.GetUserId();
                    var consumer = dataContext.consumermasters.Where(x => x.UserId == UserId).FirstOrDefault();
                    var jsonResult = Json(consumer, JsonRequestBehavior.AllowGet);
                    jsonResult.ContentType = "application/json";
                    jsonResult.MaxJsonLength = Int32.MaxValue;

                    return jsonResult;

                    //return Json(consumer, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                //call error view here
                //log the error
                EventLog.LogErrorData("Error occurred Consumer/GetConsumerLogo." + ex.Message, true);
                return Json(Global.Consumer.GetConsumerLogoException, JsonRequestBehavior.AllowGet);
            }
        }

        [AdminConsumerFilter]
        public JsonResult getConsumerPoints(string merchantid)
        {
            try
            {
                int no = Convert.ToInt32(merchantid);
                merchant_master merchant = new merchant_master();
                consumerEntities dataContext = new consumerEntities();

                merchant = dataContext.merchant_master.Where(u => u.merchantid == no).FirstOrDefault();


                using (instadelight_consumerEntities consumerdataContext = new instadelight_consumerEntities())
                {
                    if (Session["UserId"] != null)
                    {
                        var UserId = User.Identity.GetUserId();

                        int availablepoints = 0;
                        int redeemedpoints = 0;
                        int NoOfPoints = 0;
                        bool iscashback = false;
                        if (merchant.RunRewardProgram == true)
                        {
                            if (merchant.RedeemProgram == "Cashback")
                            {
                                availablepoints = Convert.ToInt32(consumerdataContext.consumercashbackdetails.Where(x => x.ConsumerId == UserId && x.MerchantId == merchant.UserId && x.ExpiryDate > DateTime.Now).Select(x => x.Cashback).Sum());
                                redeemedpoints = Convert.ToInt32(consumerdataContext.consumerredeemdetails.Where(x => x.ConsumerId == UserId && x.MerchantId == merchant.UserId).Select(x => x.PointsRedeemed).Sum());

                                NoOfPoints = availablepoints - redeemedpoints;
                                iscashback = true;

                            }
                            else
                            {
                                availablepoints = Convert.ToInt32(consumerdataContext.consumerrewardpoints.Where(x => x.ConsumerId == UserId && x.MerchantId == merchant.UserId && x.ExpiryDate > DateTime.Now).Select(x => x.Points).Sum());
                                redeemedpoints = Convert.ToInt32(consumerdataContext.consumerredeemdetails.Where(x => x.ConsumerId == UserId && x.MerchantId == merchant.UserId).Select(x => x.PointsRedeemed).Sum());

                                NoOfPoints = availablepoints - redeemedpoints;
                                iscashback = false;
                            }
                        }

                        //var points = consumerdataContext.consumerrewardpoints.Where(x => x.ConsumerId == UserId && x.MerchantId == merchant.UserId && x.ExpiryDate > DateTime.Now).FirstOrDefault();
                        consumerrewardpoint points = new consumerrewardpoint();
                        points.ConsumerId = UserId;
                        points.MerchantId = merchant.UserId;
                        points.Points = NoOfPoints;
                        points.iscashback = iscashback;
                        var jsonResult = Json(points, JsonRequestBehavior.AllowGet);
                        jsonResult.ContentType = "application/json";
                        jsonResult.MaxJsonLength = Int32.MaxValue;

                        return jsonResult;
                    }
                    else
                    {
                        return Json(Global.Consumer.InvalidConsumer, JsonRequestBehavior.AllowGet);
                    }
                    //return Json(consumer, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                //call error view here
                //log the error
                if (ex.InnerException != null)
                {
                    EventLog.LogErrorData("Error occurred Consumer/getConsumerPoints." + ex.InnerException.Message, true);
                    return Json(Global.Consumer.GetConsumerPointsException, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    EventLog.LogErrorData("Error occurred Consumer/getConsumerPoints." + ex.Message, true);
                    return Json(Global.Consumer.GetConsumerPointsException, JsonRequestBehavior.AllowGet);
                }
            }
        }

        [AdminConsumerFilter]
        public string SaveChangedCountry(string countryid, string langid)
        {
            try
            {
                using (instadelight_consumerEntities consumerdataContext = new instadelight_consumerEntities())
                {

                    var UserId = User.Identity.GetUserId();
                    var consumer = consumerdataContext.consumermasters.Where(x => x.UserId == UserId).FirstOrDefault();
                    consumer.Country = Convert.ToInt32(countryid);
                    consumer.LanguageId = Convert.ToInt32(langid);
                    consumerdataContext.SaveChanges();

                    HttpCookie cookie = new HttpCookie("LanguageSelected");
                    string lang = string.Empty;
                    int languageid = Convert.ToInt32(consumer.LanguageId);

                    language_master langmaster = consumerdataContext.language_master.Where(x => x.LanguageiId == languageid).FirstOrDefault();

                    cookie.Value = langmaster.LanguageCode;
                    Response.SetCookie(cookie);

                    return Global.Consumer.CountryUpdated;
                }
            }
            catch (Exception ex)
            {
                //call error view here
                //log the error
                EventLog.LogErrorData("Error occurred Consumer/SaveChangedCountry." + ex.Message, true);
                return Global.Consumer.UpdateCountryException;
            }
        }

        [AdminConsumerFilter]
        public JsonResult getLanguage(string countryid)
        {
            try
            {
                using (consumerEntities dataContext = new consumerEntities())
                {
                    int no = Convert.ToInt32(countryid);
                    var Language = dataContext.language_master.Where(x => x.CountryId == no).OrderBy(x => x.LanguageiId).ToList();

                    return Json(Language, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                //call error view here
                //log the error
                EventLog.LogErrorData("Error occurred Consumer/getLanguage." + ex.Message, true);
                return Json(Global.Consumer.GetLanguageException, JsonRequestBehavior.AllowGet);
            }
        }

        [AdminConsumerFilter]
        public JsonResult GetAllReviews()
        {
            try
            {
                using (consumerEntities dataContext = new consumerEntities())
                {

                    string userid = Session["UserId"].ToString();

                    MySqlParameter param1 = new MySqlParameter();
                    param1.Value = userid;
                    param1.Direction = System.Data.ParameterDirection.Input;
                    param1.ParameterName = "@CId";
                    param1.DbType = System.Data.DbType.String;

                    List<merchantwithreviews> merchantList = dataContext.Database.SqlQuery<merchantwithreviews>("CALL GetMerchantListForPendingReviews(@CId)", param1)
                     .Select(x => new merchantwithreviews
                     {
                         merchantid = x.merchantid,
                         DECName = x.DECName,
                         MerchantLogo = x.MerchantLogo
                     }).ToList();


                    var jsonResult = Json(merchantList, JsonRequestBehavior.AllowGet);
                    jsonResult.ContentType = "application/json";
                    jsonResult.MaxJsonLength = Int32.MaxValue;

                    return jsonResult;
                }
            }
            catch (Exception ex)
            {
                //call error view here
                //log the error
                EventLog.LogErrorData("Error occurred Consumer/GetAllReviews." + ex.Message, true);
                return Json(Global.Consumer.GetAllReviewsException, JsonRequestBehavior.AllowGet);
            }
        }

        [AdminConsumerFilter]
        public JsonResult GetPendingReviews()
        {
            try
            {

                //string merchantid = TempData["TempMerchantId"].ToString();
                using (consumerEntities dataContext = new consumerEntities())
                {
                    string userid = Session["UserId"].ToString();
                    int noofreviews = dataContext.merchantconsumerreviewdetails.Where(x => x.ConsumerId == userid && x.Status == "Shared" && x.ValidTill > DateTime.Now).Select(x => x.ReviewId).Distinct().Count();
                    var jsonResult = Json(noofreviews, JsonRequestBehavior.AllowGet);
                    jsonResult.ContentType = "application/json";
                    jsonResult.MaxJsonLength = Int32.MaxValue;

                    return jsonResult;


                }
            }
            catch (Exception ex)
            {
                //call error view here
                //log the error
                EventLog.LogErrorData("Error occurred Consumer/GetPendingReviews." + ex.Message, true);
                return Json(Global.Consumer.GetPendingReviewsException, JsonRequestBehavior.AllowGet);
            }
        }

        [AdminConsumerFilter]
        public JsonResult GetPendingReviewForMerchant(string merchantid)
        {
            try
            {

                //string merchantid = TempData["TempMerchantId"].ToString();
                using (consumerEntities dataContext = new consumerEntities())
                {
                    string userid = Session["UserId"].ToString();
                    int mid = Convert.ToInt32(merchantid);
                    merchant_master master = dataContext.merchant_master.Where(x => x.merchantid == mid).FirstOrDefault();

                    MySqlParameter param1 = new MySqlParameter();
                    param1.Value = master.UserId;
                    param1.Direction = System.Data.ParameterDirection.Input;
                    param1.ParameterName = "@MId";
                    param1.DbType = System.Data.DbType.String;

                    MySqlParameter param2 = new MySqlParameter();
                    param2.Value = userid;
                    param2.Direction = System.Data.ParameterDirection.Input;
                    param2.ParameterName = "@CId";
                    param2.DbType = System.Data.DbType.String;

                    List<reviewmaster> review = dataContext.Database.SqlQuery<reviewmaster>("CALL GetReviewDetailsByMerchantId(@MId,@CId)", param1, param2)
                     .Select(x => x).ToList();

                    //var review = dataContext.merchantconsumerreviewdetails.Where(x => x.ConsumerId == userid && x.MerchantId == master.UserId && x.Status == "Shared" && x.ValidTill > DateTime.Now).ToList();

                    var jsonResult = Json(review, JsonRequestBehavior.AllowGet);
                    jsonResult.ContentType = "application/json";
                    jsonResult.MaxJsonLength = Int32.MaxValue;

                    return jsonResult;
                }
            }
            catch (Exception ex)
            {
                //call error view here
                //log the error
                EventLog.LogErrorData("Error occurred Consumer/GetPendingReviewForMerchant." + ex.Message, true);
                return Json(Global.Consumer.GetPendingReviewsException, JsonRequestBehavior.AllowGet);
            }
        }

        //Save Consumer Review-PA
        [AdminConsumerFilter]
        public string SaveConsumerReview(clsReview_submitdetails ch)
        {
            try
            {
                var consumerId = User.Identity.GetUserId();
                //var merchantid = TempData["TempMerchantId"].ToString();
                if (ch != null)
                {
                    using (consumerEntities dataContext = new consumerEntities())
                    {
                        review_submit_details mappingcosumerreview = new review_submit_details();
                        mappingcosumerreview.reviewid = ch.reviewid;
                        mappingcosumerreview.consumerid = consumerId;
                        mappingcosumerreview.Question1Rating = ch.Question1Rating;
                        mappingcosumerreview.Question2Rating = ch.Question2Rating;
                        mappingcosumerreview.Question3Rating = ch.Question3Rating;
                        mappingcosumerreview.Question4Rating = ch.Question4Rating;
                        mappingcosumerreview.IsSharedDECWithFriends = ch.IsSharedDECWithFriends;
                        mappingcosumerreview.MerchantId = ch.merchantId;
                        mappingcosumerreview.Review_Submit_date = DateTime.Now;
                        mappingcosumerreview.Comment = ch.Comment;
                        dataContext.review_submit_details.Add(mappingcosumerreview);
                        dataContext.SaveChanges();

                        var master = dataContext.merchant_master.Where(m => m.merchantid == ch.merchantId).FirstOrDefault();

                        var merchantUserId = dataContext.merchant_master.Where(m => m.merchantid == ch.merchantId).Select(m => m.UserId).FirstOrDefault();


                        instadelight_consumerEntities consumerContext = new instadelight_consumerEntities();
                        var consumerphone = consumerContext.users.Where(c => c.Id == consumerId).Select(m => m.UserName).FirstOrDefault();

                        List<merchantconsumerreviewdetail> revdet = dataContext.merchantconsumerreviewdetails.Where(x => x.ReviewId == ch.reviewid && x.ConsumerId == consumerId && x.MerchantId == merchantUserId && x.Status == "Shared").ToList();

                        if (revdet != null)
                        {
                            if (revdet.Count > 0)
                            {
                                for (int i = 0; i < revdet.Count; i++)
                                {
                                    revdet[i].Status = "Submitted";
                                    dataContext.SaveChanges();
                                }
                            }
                        }

                        //check if merchant has set event coupon for submitting review
                        eventcoupondetail eventcoupon = dataContext.eventcoupondetails.Where(x => x.EventId == 3 && x.MerchantId == ch.merchantId).FirstOrDefault();
                        if (eventcoupon != null)
                        {
                            //Assign this coupon to this consumer
                            merchantconsumercoupondetail sendcoupon = new merchantconsumercoupondetail();
                            sendcoupon.ConsumerId = consumerId;
                            sendcoupon.ConsumerPhone = consumerphone;
                            sendcoupon.MerchantId = merchantUserId;
                            sendcoupon.CouponId = eventcoupon.CouponId;
                            sendcoupon.ValidFrom = DateTime.Now;
                            sendcoupon.ValidTill = DateTime.Now.AddMonths(6);
                            dataContext.merchantconsumercoupondetails.Add(sendcoupon);
                            dataContext.SaveChanges();

                            if (consumerphone.Contains("@") == false)
                            {
                                SMSUtility sms = new SMSUtility();

                                string smsresult = sms.sendMessage(consumerphone, "Dear Customer, Thank you for your valuable feedback. As a token of appreciation we have sent you a coupon! " + master.MerchantName);
                            }
                            else
                            {
                                EmailModel model = new EmailModel();
                                model.To = consumerphone;
                                model.Email = "Thank_you@offertraker.com";
                                model.Subject = "Thank you for submitting review";

                                model.Body = "Dear Customer,<br /><br /> Thank you for your valuable feedback. As a token of appreciation we have sent you a coupon! <br /><br />" + master.MerchantName;
                                SendEmail email = new SendEmail();
                                email.SendEmailToConsumer(model);
                            }
                        }

                        return merchantUserId;

                    }
                }
                else
                {
                    return Global.Consumer.InvalidConsumerreview;
                }
            }
            catch (Exception ex)
            {
                //call error view here
                //log the error
                EventLog.LogErrorData("Error occurred Consumer/SaveConsumerReview." + ex.Message, true);
                return Global.Consumer.SaveReviewException;
            }
        }

        [AdminConsumerFilter]
        public ActionResult MerchantGiftCards(string id)
        {
            TempData["TempMerchantId"] = id;
            ViewData["MerchantId"] = id;
            int merchantid = Convert.ToInt32(id);
            using (consumerEntities dataContext = new consumerEntities())
            {
                merchant_master MerchantUserId = dataContext.merchant_master.Where(u => u.merchantid == merchantid).FirstOrDefault();
                if (MerchantUserId != null)
                {
                    if (MerchantUserId.GiftCardDiscount != null)
                    {
                        if (Convert.ToInt32(MerchantUserId.GiftCardDiscount) > 0)
                            ViewBag.GiftCardDiscount = MerchantUserId.GiftCardDiscount;
                        else
                            ViewBag.GiftCardDiscount = 5;
                    }
                    else
                        ViewBag.GiftCardDiscount = 5;

                    if (MerchantUserId.Taxes != null)
                    {
                        if (Convert.ToInt32(MerchantUserId.Taxes) > 0)
                            ViewBag.Taxes = MerchantUserId.Taxes;
                        else
                            ViewBag.Taxes = 18;
                    }
                    else
                        ViewBag.Taxes = 18;
                }
            }
            return View();
        }

        [AdminConsumerFilter]
        public ActionResult MerchantGiftCardsIndex(string id)
        {
            TempData["TempMerchantId"] = id;


            ViewBag.MerchantId = id;
            return View();
        }

        [AdminConsumerFilter]
        public ActionResult ViewGiftCards(string id)
        {
            TempData["TempMerchantId"] = id;
            ViewBag.MerchantId = id;
            return View();
        }

        // Get Merchant Gift Card Denomination
        [AdminConsumerFilter]
        public JsonResult GetGiftCardDenomination()
        {
            try
            {
                int merchantid = Convert.ToInt32(TempData["TempMerchantId"].ToString());
                using (consumerEntities dataContext = new consumerEntities())
                {
                    var MerchantUserId = dataContext.merchant_master.Where(u => u.merchantid == merchantid).Select(x => x.UserId).FirstOrDefault();
                    var giftDenominations = (from x in dataContext.giftcardmasters.Select(x => x).ToList()
                                             where (x.MerchantUserId == MerchantUserId)
                                             orderby x.SetDate descending
                                             select x).Take(1);


                    var jsonResult = Json(giftDenominations, JsonRequestBehavior.AllowGet);
                    jsonResult.ContentType = "application/json";
                    jsonResult.MaxJsonLength = Int32.MaxValue;

                    return jsonResult;

                }
            }
            catch (Exception ex)
            {
                //call error view here
                //log the error
                EventLog.LogErrorData("Error occurred Consumer/GetGiftcardDenominations." + ex.Message, true);
                return Json(Global.Consumer.GetGiftCardDenominationException, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        [AdminConsumerFilter]
        public ActionResult PaymentResponse()
        {
            try
            {
                string merchantid = Session["merchantid"].ToString();
                string giftCardId = Session["giftCardId"].ToString();
                string Denomination = Session["Denomination"].ToString();
                string qty = Session["qty"].ToString();


                int quantity = Convert.ToInt32(qty);
                string secret_key = "32908f0fd900b405c3bbcf15041cbcbba3f735a7";
                string data = "";
                string txnId = Request["TxId"];
                string txnStatus = Request["TxStatus"];
                string amount = Request["amount"];
                string pgTxnId = Request["pgTxnNo"];
                string issuerRefNo = Request["issuerRefNo"];
                string authIdCode = Request["authIdCode"];
                string firstName = Request["firstName"];
                string lastName = Request["lastName"];
                string pgRespCode = Request["pgRespCode"];
                string zipCode = Request["addressZip"];
                string resSignature = Request["signature"];
                bool flag = true;
                if (txnId != null)
                {
                    data += txnId;
                }
                if (txnStatus != null)
                {
                    data += txnStatus;
                }
                if (amount != null)
                {
                    data += amount;
                }
                if (pgTxnId != null)
                {
                    data += pgTxnId;
                }
                if (issuerRefNo != null)
                {
                    data += issuerRefNo;
                }
                if (authIdCode != null)
                {
                    data += authIdCode;
                }
                if (firstName != null)
                {
                    data += firstName;
                }
                if (lastName != null)
                {
                    data += lastName;
                }
                if (pgRespCode != null)
                {
                    data += pgRespCode;
                }
                if (zipCode != null)
                {
                    data += zipCode;
                }
                System.Security.Cryptography.HMACSHA1 myhmacsha1 = new System.Security.Cryptography.HMACSHA1(Encoding.ASCII.GetBytes(secret_key));
                System.IO.MemoryStream stream = new System.IO.MemoryStream(Encoding.ASCII.GetBytes(data));
                string signature = BitConverter.ToString(myhmacsha1.ComputeHash(stream)).Replace("-", "").ToLower();

                if (resSignature != null && !signature.Equals(resSignature))
                {
                    flag = false;
                }
                if (flag)
                {
                    using (consumerEntities dataContext = new consumerEntities())
                    {
                        int merchantno = Convert.ToInt32(merchantid);
                        merchant_master master = dataContext.merchant_master.Where(x => x.merchantid == merchantno).FirstOrDefault();

                        using (instadelight_consumerEntities consumerDatacontext = new instadelight_consumerEntities())
                        {

                            string userid = Session["UserId"].ToString();
                            for (int i = 1; i <= quantity; i++)
                            {
                                consumergiftcarddetail gifts = new consumergiftcarddetail();
                                gifts.ConsumerId = userid;
                                gifts.MerchantId = master.UserId;
                                gifts.GiftCardId = Convert.ToInt32(giftCardId);
                                gifts.DenominationRs = Convert.ToInt32(Denomination);
                                gifts.Status = 0;
                                gifts.DateOfPurchase = DateTime.Now;
                                gifts.ValidTill = DateTime.Now.AddDays(364);
                                consumerDatacontext.consumergiftcarddetails.Add(gifts);
                                consumerDatacontext.SaveChanges();
                            }

                            //Send SMS/Email to consumer for a new gift card
                            user consumer = consumerDatacontext.users.Where(x => x.Id == userid).FirstOrDefault();
                            consumermaster consumermaster = consumerDatacontext.consumermasters.Where(x => x.UserId == userid).FirstOrDefault();

                            if (consumer.UserName.Contains('@') == false)
                            {
                                SMSUtility sms = new SMSUtility();
                                //string smsresult = sms.sendMessage(param.mobileno, "Dear Customer, your " + Session["UserName"] + " Digital Card is updated. Download the app for Android at goo.gl/r5rxjj and for Apple at goo.gl/dAq4er. Your cell number is your login and your password is 123456 unless you have reset it.");
                                string smsresult = sms.sendMessage(consumer.UserName, "Dear Customer, thank you for buying our Gift card. You can view the gift card in the \"purchased\" section of Gift cards on our DEC. Best wishes, " + master.DECName);

                                consumersmsdetail smsdetails = new consumersmsdetail();
                                smsdetails.ConsumerId = userid;
                                smsdetails.MerchantId = master.UserId;
                                smsdetails.SMSEmailStatus = smsresult;
                                smsdetails.UserName = consumer.UserName;
                                smsdetails.SentDate = DateTime.Now;
                                consumerDatacontext.consumersmsdetails.Add(smsdetails);
                                consumerDatacontext.SaveChanges();
                            }
                            else
                            {
                                EmailModel model = new EmailModel();
                                model.To = consumer.UserName;
                                model.Email = "welcome@offertraker.com";
                                model.Subject = "Thank you for buying a gift card !";
                                //gp@instadelight.com
                                model.bcc = "gp@instadelight.com";
                                string consumername = consumer.UserName;
                                if (consumermaster.consumername != null)
                                {
                                    consumername = consumermaster.consumername;
                                }


                                model.Body = "Dear " + consumername + ", <br />Thank you for buying a gift card from " + master.DECName + ". <br />It's so easy to send this gift card to someone. Go ahead and enjoy the card. Please check the terms and conditions and the validity of the gift card.<br /> Best wishes,<br /> Offertraker team.";

                                SendEmail email = new SendEmail();
                                string result = email.SendEmailToConsumer(model);
                                consumersmsdetail smsdetails = new consumersmsdetail();
                                smsdetails.ConsumerId = userid;
                                smsdetails.MerchantId = master.UserId;
                                smsdetails.SMSEmailStatus = result;
                                smsdetails.UserName = consumer.UserName;
                                smsdetails.SentDate = DateTime.Now;
                                consumerDatacontext.consumersmsdetails.Add(smsdetails);
                                consumerDatacontext.SaveChanges();
                            }
                        }
                    }
                    ViewBag.Result = "Your Unique Transaction/Order Id : " + txnId;
                    ViewBag.Status = "Transaction Status : " + txnStatus;

                    EventLog.LogData("Your Unique Transaction/Order Id : " + txnId, true);
                    EventLog.LogData("Transaction Status : " + txnStatus, true);
                }
                else
                {
                    ViewBag.Result = "";
                    ViewBag.Status = "Citrus Response Signature and Our (Merchant) Signature Mis-Mactch";
                    EventLog.LogErrorData("Citrus Response Signature and Our (Merchant) Signature Mis-Mactch", true);
                }

                Session.Remove("merchantid");
                Session.Remove("giftCardId");
                Session.Remove("Denomination");
                Session.Remove("qty");

                return View();
            }
            catch (Exception ex)
            {
                //call error view here
                //log the error
                EventLog.LogErrorData("Error occurred Consumer/PaymentResponse." + ex.Message, true);
                ViewBag.Result = "";
                ViewBag.Status = "Error occured while getting payment response";

                Session.Remove("merchantid");
                Session.Remove("giftCardId");
                Session.Remove("Denomination");
                Session.Remove("qty");

                return View();
            }
        }


        [HttpPost]
        [AdminConsumerFilter]
        public ActionResult PayForGiftCards()
        {
            try
            {
                //TempData["TempMerchantId"] = id;
                //ViewData["MerchantId"] = id;

                //Need to replace the last part of URL("your-vanityUrlPart") with your Testing/Live URL
                string formPostUrl = "https://www.citruspay.com/viklkipxp3";
                //Need to change with your Secret Key
                string secret_key = "32908f0fd900b405c3bbcf15041cbcbba3f735a7";
                //Need to change with your Vanity URL Key from the citrus panel
                string vanityUrl = "viklkipxp3";
                //Should be unique for every transaction
                string merchantTxnId = System.DateTime.Now.ToString("yyyyMMddHHmmssffff");
                //Need to change with your Order Amount
                string orderAmount = Request["GrandTotal"];// "1.00";
                string currency = "INR";
                string data = vanityUrl + orderAmount + merchantTxnId + currency;

                System.Security.Cryptography.HMACSHA1 myhmacsha1 = new System.Security.Cryptography.HMACSHA1(Encoding.ASCII.GetBytes(secret_key));
                System.IO.MemoryStream stream = new System.IO.MemoryStream(Encoding.ASCII.GetBytes(data));
                string securitySignature = BitConverter.ToString(myhmacsha1.ComputeHash(stream)).Replace("-", "").ToLower();

                Session["merchantid"] = Request["merchantid"];
                Session["giftCardId"] = Request["giftCardId"];
                Session["Denomination"] = Request["Denomination"];
                Session["qty"] = Request["qty"];
                instadelight_consumerEntities dataContext = new instadelight_consumerEntities();
                string userid = Session["UserId"].ToString();
                user consumer = dataContext.users.Where(x => x.Id == userid).FirstOrDefault();

                Citrus cs = new Citrus();
                cs.formPostUrl = formPostUrl;
                cs.currency = "INR";
                if (consumer.UserName.Contains("@") == false)
                {
                    cs.phoneNumber = consumer.UserName;
                    cs.email = "";
                }
                else
                {
                    cs.phoneNumber = "";
                    cs.email = consumer.UserName;
                }


                cs.merchantTxnId = merchantTxnId;
                cs.notifyUrl = "";
                cs.returnUrl = "https://www.offertraker.com:8089/Consumer/PaymentResponse";


                cs.orderAmount = orderAmount;
                cs.secSignature = securitySignature;
                EventLog.LogData("Sending request to citrus pay for amount Rs " + cs.orderAmount + " for consumer " + Session["UserName"], true);
                EventLog.LogData(securitySignature, true);

                return View(cs);
            }
            catch (Exception ex)
            {
                //call error view here
                //log the error
                EventLog.LogErrorData("Error occurred Consumer/PayForGiftCards." + ex.Message, true);
                return Json("Error occurred while getting payment gateway details", JsonRequestBehavior.AllowGet);
            }
        }

        public class clsGiftCard
        {
            public int giftcardid { get; set; }
            public int Denomination { get; set; }
            public int Quantity { get; set; }
            public int GrandTotal { get; set; }
            public string merchantId { get; set; }
        }


        //Save Consumer Gift Card




        //Save Consumer Gift Card
        [AdminConsumerFilter]
        public JsonResult GetConsumerGiftCards(string merchantid)
        {
            try
            {
                var consumerId = User.Identity.GetUserId();
                //var merchantUserId = dataContext.merchant_master.Where(m => m.merchantid == ch.merchantId).Select(m => m.UserId).FirstOrDefault();
                int no = Convert.ToInt32(merchantid);
                merchant_master merchant = new merchant_master();
                using (consumerEntities dataContext = new consumerEntities())
                {
                    merchant = dataContext.merchant_master.Where(u => u.merchantid == no).FirstOrDefault();
                }

                List<clsConsumerGiftCard> consgfcrd = new List<clsConsumerGiftCard>();
                consumerEntities merchantDataContext = new consumerEntities();

                using (instadelight_consumerEntities dataContext = new instadelight_consumerEntities())
                {
                    consumermaster master = dataContext.consumermasters.Where(x => x.UserId == consumerId).FirstOrDefault();
                    country_master country = dataContext.country_master.Where(x => x.countryid == merchant.Country).FirstOrDefault();
                    List<consumergiftcarddetail> purchasedGiftCards = dataContext.consumergiftcarddetails.Where(x => x.ConsumerId == consumerId && x.MerchantId == merchant.UserId && x.Status == 0 && x.ValidTill >= DateTime.Now).OrderBy(x => x.MerchantId).ThenBy(x => x.ValidTill).ToList();
                    if (purchasedGiftCards != null)
                    {
                        if (purchasedGiftCards.Count > 0)
                        {
                            foreach (consumergiftcarddetail gcard in purchasedGiftCards)
                            {
                                clsConsumerGiftCard temp = new clsConsumerGiftCard();
                                temp.consumerid = consumerId;
                                temp.Id = gcard.Id;
                                temp.giftcardid = Convert.ToInt32(gcard.GiftCardId);
                                temp.Denomination = Convert.ToInt32(gcard.DenominationRs);
                                temp.status = Convert.ToInt32(gcard.Status);
                                temp.merchantId = gcard.MerchantId;
                                temp.MerchantName = merchant.MerchantName;
                                temp.MerchantLogo = merchant.MerchantLogo;
                                temp.DECColor = merchant.DECColor;
                                temp.ConsumerLogo = master.consumerlogo;
                                temp.currency = country.currency;
                                temp.ValidTill = Convert.ToDateTime(gcard.ValidTill);


                                giftcardmaster giftcard = merchantDataContext.giftcardmasters.Where(x => x.GiftCardId == gcard.GiftCardId).FirstOrDefault();
                                if (giftcard != null)
                                {
                                    if (gcard.DenominationRs == giftcard.Denomination1 && giftcard.Denom1DEC != null)
                                    {
                                        temp.MerchantDEC = giftcard.Denom1DEC;
                                    }
                                    else if (gcard.DenominationRs == giftcard.Denomination2 && giftcard.Denom2DEC != null)
                                    {
                                        temp.MerchantDEC = giftcard.Denom2DEC;
                                    }
                                    else if (gcard.DenominationRs == giftcard.Denomination3 && giftcard.Denom3DEC != null)
                                    {
                                        temp.MerchantDEC = giftcard.Denom3DEC;
                                    }
                                    else if (gcard.DenominationRs == giftcard.Denomination4 && giftcard.Denom4DEC != null)
                                    {
                                        temp.MerchantDEC = giftcard.Denom4DEC;
                                    }
                                    else if (merchant.MerchantDEC != null)
                                    {
                                        temp.MerchantDEC = merchant.MerchantDEC;
                                    }
                                    else if (merchant.merchantDecFromLibrary != null)
                                    {
                                        temp.MerchantDEC = null;
                                        temp.merchantDecFromLibrary = merchant.merchantDecFromLibrary;
                                    }
                                }
                                else if (merchant.MerchantDEC != null)
                                {
                                    temp.MerchantDEC = merchant.MerchantDEC;
                                }
                                else if (merchant.merchantDecFromLibrary != null)
                                {
                                    temp.MerchantDEC = null;
                                    temp.merchantDecFromLibrary = merchant.merchantDecFromLibrary;
                                }

                                consgfcrd.Add(temp);

                            }
                        }
                    }
                }
                var jsonResult = Json(consgfcrd, JsonRequestBehavior.AllowGet);
                jsonResult.ContentType = "application/json";
                jsonResult.MaxJsonLength = Int32.MaxValue;

                return jsonResult;
            }
            catch (Exception ex)
            {
                //call error view here
                //log the error
                EventLog.LogErrorData("Error occurred Consumer/GetConsumerGiftCards." + ex.Message, true);
                return Json(Global.Consumer.GetGiftCardDenominationException, JsonRequestBehavior.AllowGet);
            }
        }

        [AdminConsumerFilter]
        public ActionResult SendGiftCard(string Id, string MerchantId)
        {
            TempData["Id"] = Id;
            @ViewBag.CardId = Id;
            @ViewBag.MerchantId = MerchantId;

            string userid = Session["UserId"].ToString();
            using (instadelight_consumerEntities dataContext = new instadelight_consumerEntities())
            {
                consumermaster consumer = dataContext.consumermasters.Where(x => x.UserId == userid).FirstOrDefault();
                ViewBag.countryid = consumer.Country;
            }

            return View();
        }

        [AdminConsumerFilter]
        public string SendGCToConsumer(string mobileno, string Id, string MerchantId)
        {
            try
            {
                SMSUtility sms = new SMSUtility();

                if (mobileno != null)
                {
                    var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(Consumercontext));
                    var UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(Consumercontext));

                    UserManager.UserValidator = new UserValidator<ApplicationUser>(UserManager)
                    {
                        AllowOnlyAlphanumericUserNames = false,
                        RequireUniqueEmail = true
                    };

                    consumerEntities dataContext = new consumerEntities();
                    string userid = Session["UserId"].ToString();
                    int merchantno = Convert.ToInt32(MerchantId);
                    int cid = Convert.ToInt32(Id);
                    merchant_master master = dataContext.merchant_master.Where(x => x.merchantid == merchantno).FirstOrDefault();
                    string merchantuserid = master.UserId;

                    string result = function.SendGCToConsumer(mobileno, Id, MerchantId, userid, Session["UserName"].ToString());
                }
                else
                {
                    return "Invalid Consumer Details";
                }
                return "Gift card sent successfully.";
            }
            catch (Exception ex)
            {
                //call error view here
                //log the error
                EventLog.LogErrorData("Error occurred Consumer/SendGCToConsumer." + ex.Message, true);
                return "Error occurred while sending Gift Card to consumer";
            }
        }

        [AdminConsumerFilter]
        public ActionResult SendDecToFriends(string Mobileno, string MerchantId)
        {
            string result = "";
            SMSUtility sms = new SMSUtility();

            consumerEntities dataContext = new consumerEntities();

            if (Mobileno != null)
            {
                string consumerid = Session["UserId"].ToString();
                merchant_master master = dataContext.merchant_master.Where(x => x.UserId == MerchantId).FirstOrDefault();
                if (master != null)
                {
                    string userid = master.UserId;

                    DateTime visitdate = DateTime.Now;
                    string[] cellnos = Mobileno.Split(',');
                    if (cellnos.Length > 0)
                    {
                        for (int i = 0; i < cellnos.Length; i++)
                        {
                            string cellno = cellnos[i];
                            if (cellno.Contains('@') == false)
                            {
                                string[] PhonewithCode = cellno.Split(' ');
                                if (PhonewithCode.Length > 0)
                                {
                                    if (PhonewithCode.Length == 3)
                                    {
                                        cellno = "+" + PhonewithCode[1] + " " + PhonewithCode[2].ToString();
                                    }
                                }
                            }

                            result = function.AddNewDECConsumer(cellno, MerchantId, consumerid, Session["UserName"].ToString());
                        }

                        result = Global.Consumer.SendDECToFriends;
                        ViewBag.result = result;
                        return View();
                    }
                    else
                    {
                        result = Global.Consumer.MobileNumberPrompt;
                        ViewBag.result = result;
                        return View();
                    }
                }
                else
                {
                    result = "Merchant does not exist.";
                    ViewBag.result = result;
                    return View();
                }
            }
            else
            {
                result = Global.Consumer.MobileNumberPrompt;
                ViewBag.result = result;
                return View();
            }
        }

        [AdminConsumerFilter]
        public ActionResult SendCouponToFriends(string Mobileno, string MerchantId, string CouponId, string SharedCouponId)
        {
            string result = "";
            if (Mobileno != null)
            {
                var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(Consumercontext));
                var UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(Consumercontext));
                UserManager.UserValidator = new UserValidator<ApplicationUser>(UserManager)
                {
                    AllowOnlyAlphanumericUserNames = false,
                    RequireUniqueEmail = true
                };
                consumerEntities dataContext = new consumerEntities();
                string consumerid = Session["UserId"].ToString();
                int merchantno = Convert.ToInt32(MerchantId);
                merchant_master master = dataContext.merchant_master.Where(x => x.merchantid == merchantno).FirstOrDefault();
                DateTime visitdate = DateTime.Now;
                int couponno = Convert.ToInt32(CouponId);

                string[] cellnos = Mobileno.Split(',');
                if (cellnos.Length > 0)
                {

                    for (int i = 0; i < cellnos.Length; i++)
                    {
                        string cellno = cellnos[i];
                        if (cellno.Contains('@') == false)
                        {
                            string[] PhonewithCode = cellno.Split(' ');
                            if (PhonewithCode.Length > 0)
                            {
                                if (PhonewithCode.Length == 3)
                                {
                                    cellno = "+" + PhonewithCode[1] + " " + PhonewithCode[2].ToString();
                                }
                            }
                        }

                        result = function.AddCheckCouponConsumer(cellno, master.UserId, CouponId, SharedCouponId, Session["UserName"].ToString());
                    }
                    result = Global.Consumer.SendCouponToFriends;
                    ViewBag.result = result;
                    return View();
                }
                else
                {
                    result = Global.Consumer.MobileNumberPrompt;
                    ViewBag.result = result;
                    return View();
                }
            }
            else
            {
                result = Global.Consumer.MobileNumberPrompt;
                ViewBag.result = result;
                return View();
            }

        }

        [AdminConsumerFilter]
        public JsonResult getConsumerLanguage()
        {
            try
            {
                if (Session["UserId"] != null)
                {
                    using (instadelight_consumerEntities dataContext = new instadelight_consumerEntities())
                    {
                        string userid = Session["UserId"].ToString();
                        consumermaster master = dataContext.consumermasters.Where(x => x.UserId == userid).FirstOrDefault();
                        if (master != null)
                        {
                            language_master Language = dataContext.language_master.Where(x => x.LanguageiId == master.LanguageId).FirstOrDefault();
                            string languagecode = Language.LanguageCode;
                            return Json(languagecode, JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            return Json("en");
                        }
                    }
                }
                else
                {
                    return Json("en");
                }
            }

            catch (Exception ex)
            {
                //call error view here
                //log the error
                EventLog.LogErrorData("Error occurred Consumer/getConsumerLanguage." + ex.Message, true);
                return Json("en");
            }
        }



        [HttpPost]
        public ActionResult BuyGiftCards()
        {
            try
            {
                //TempData["TempMerchantId"] = id;
                //ViewData["MerchantId"] = id;
                Citrus cs = new Citrus();
                instadelight_consumerEntities dataContext = new instadelight_consumerEntities();
                consumergiftcardtransactiondetail logtrans = new consumergiftcardtransactiondetail();
                logtrans.RequestString = "https://www.offertraker.com:8089/Consumer/BuyGiftCards?consumerid=" + Request["consumerid"] + "&merchantid=" + Request["merchantid"] + "&giftCardId=" + Request["giftCardId"] + "&Denomination=" + Request["Denomination"] + "&qty=" + Request["qty"] + "&GrandTotal=" + Request["GrandTotal"];
                logtrans.RequestDateTime = DateTime.Now;

                if (Request["consumerid"] == null)
                {
                    ViewBag.flag = "Consumer id is not provided";
                    EventLog.LogErrorData("Consumer id is not provided while buying gift card from native app", true);
                    logtrans.ResponseStatus = "Consumer id is not provided";
                    dataContext.consumergiftcardtransactiondetails.Add(logtrans);
                    dataContext.SaveChanges();

                    return View(cs);
                }

                if (Request["merchantid"] == null)
                {
                    ViewBag.flag = "Merchant id is not provided";
                    EventLog.LogErrorData("Merchant id is not provided while buying gift card from native app", true);

                    logtrans.ResponseStatus = "Merchant id is not provided";
                    dataContext.consumergiftcardtransactiondetails.Add(logtrans);
                    dataContext.SaveChanges();

                    return View(cs);
                }

                if (Request["giftCardId"] == null)
                {
                    ViewBag.flag = "Gift Card id is not provided";
                    EventLog.LogErrorData("Gift Card id is not provided while buying gift card from native app", true);

                    logtrans.ResponseStatus = "Gift Card id is not provided";
                    dataContext.consumergiftcardtransactiondetails.Add(logtrans);
                    dataContext.SaveChanges();

                    return View(cs);
                }

                if (Request["Denomination"] == null)
                {
                    ViewBag.flag = "Denomination is not provided";
                    EventLog.LogErrorData("Denomination is not provided while buying gift card from native app", true);

                    logtrans.ResponseStatus = "Denomination is not provided";
                    dataContext.consumergiftcardtransactiondetails.Add(logtrans);
                    dataContext.SaveChanges();

                    return View(cs);
                }

                if (Request["qty"] == null)
                {
                    ViewBag.flag = "Quantity is not provided";
                    EventLog.LogErrorData("Quantity is not provided while buying gift card from native app", true);

                    logtrans.ResponseStatus = "Quantity is not provided";
                    dataContext.consumergiftcardtransactiondetails.Add(logtrans);
                    dataContext.SaveChanges();

                    return View(cs);
                }

                if (Request["GrandTotal"] == null)
                {
                    ViewBag.flag = "Grand Total is not provided";
                    EventLog.LogErrorData("Grand Total is not provided while buying gift card from native app", true);

                    logtrans.ResponseStatus = "Grand Total is not provided";
                    dataContext.consumergiftcardtransactiondetails.Add(logtrans);
                    dataContext.SaveChanges();

                    return View(cs);
                }


                consumermaster consumer = dataContext.consumermasters.Where(x => x.UserId == Request["consumerid"]).FirstOrDefault();
                if (consumer == null)
                {
                    ViewBag.flag = "Consumer with id " + Request["consumerid"] + " does not exist";
                    EventLog.LogErrorData("Consumer with id " + Request["consumerid"] + " passed from native app does not exist", true);

                    logtrans.ResponseStatus = "Consumer with id " + Request["consumerid"] + " does not exist";
                    dataContext.consumergiftcardtransactiondetails.Add(logtrans);
                    dataContext.SaveChanges();

                    return View(cs);
                }


                int merchantno = Convert.ToInt32(Request["merchantid"]);

                consumerEntities merchantDataContext = new consumerEntities();

                merchant_master merchant = merchantDataContext.merchant_master.Where(x => x.merchantid == merchantno).FirstOrDefault();
                if (merchant == null)
                {
                    ViewBag.flag = "Merchant with id " + Request["merchantid"] + " does not exist";
                    EventLog.LogErrorData("Merchant with id " + Request["merchantid"] + " passed from native app does not exist", true);

                    logtrans.ResponseStatus = "Merchant with id " + Request["merchantid"] + " does not exist";
                    dataContext.consumergiftcardtransactiondetails.Add(logtrans);
                    dataContext.SaveChanges();

                    return View(cs);
                }

                int giftcardno = Convert.ToInt32(Request["giftCardId"]);
                giftcardmaster giftmaster = merchantDataContext.giftcardmasters.Where(x => x.GiftCardId == giftcardno && x.MerchantUserId == merchant.UserId).FirstOrDefault();
                if (giftmaster == null)
                {
                    ViewBag.flag = "Gift card is not associated with the merchant id passed";
                    EventLog.LogErrorData("Gift card is not associated with the merchant id passed from native app does not exist", true);

                    logtrans.ResponseStatus = "Gift card is not associated with the merchant id passed";
                    dataContext.consumergiftcardtransactiondetails.Add(logtrans);
                    dataContext.SaveChanges();

                    return View(cs);
                }

                merchantconsumerdetail merchant_consumer = merchantDataContext.merchantconsumerdetails.Where(x => x.ConsumerId == Request["consumerid"] && x.MerchantId == merchant.UserId).FirstOrDefault();
                if (giftmaster == null)
                {
                    ViewBag.flag = "Merchant has not shared DEC with consumer.";
                    EventLog.LogErrorData("Merchant has not shared DEC with consumer.", true);

                    logtrans.ResponseStatus = "Merchant has not shared DEC with consumer.";
                    dataContext.consumergiftcardtransactiondetails.Add(logtrans);
                    dataContext.SaveChanges();

                    return View(cs);
                }


                logtrans.MerchantId = merchant.UserId;
                logtrans.ConsumerId = Request["consumerid"];
                logtrans.GiftCardId = Convert.ToInt32(Request["giftCardId"]);
                logtrans.TransactionAmount = Convert.ToInt32(Request["GrandTotal"]);

                //Need to replace the last part of URL("your-vanityUrlPart") with your Testing/Live URL
                string formPostUrl = "https://www.citruspay.com/viklkipxp3";
                //Need to change with your Secret Key
                string secret_key = "32908f0fd900b405c3bbcf15041cbcbba3f735a7";
                //Need to change with your Vanity URL Key from the citrus panel
                string vanityUrl = "viklkipxp3";
                //Should be unique for every transaction
                string merchantTxnId = System.DateTime.Now.ToString("yyyyMMddHHmmssffff");
                //Need to change with your Order Amount
                string orderAmount = Request["GrandTotal"];// "1.00";
                string currency = "INR";
                string data = vanityUrl + orderAmount + merchantTxnId + currency;
                logtrans.SentRequestDetails = data;

                System.Security.Cryptography.HMACSHA1 myhmacsha1 = new System.Security.Cryptography.HMACSHA1(Encoding.ASCII.GetBytes(secret_key));
                System.IO.MemoryStream stream = new System.IO.MemoryStream(Encoding.ASCII.GetBytes(data));
                string securitySignature = BitConverter.ToString(myhmacsha1.ComputeHash(stream)).Replace("-", "").ToLower();
                logtrans.securitySignature = securitySignature;

                dataContext.consumergiftcardtransactiondetails.Add(logtrans);
                dataContext.SaveChanges();

                Session["consumerid"] = Request["consumerid"];
                Session["merchantid"] = Request["merchantid"];
                Session["giftCardId"] = Request["giftCardId"];
                Session["Denomination"] = Request["Denomination"];
                Session["qty"] = Request["qty"];

                string userid = Session["consumerid"].ToString();
                user con = dataContext.users.Where(x => x.Id == userid).FirstOrDefault();

                cs.formPostUrl = formPostUrl;
                cs.currency = "INR";
                cs.phoneNumber = con.UserName;
                cs.email = con.Email;
                cs.formPostUrl = formPostUrl;
                cs.currency = "INR";
                cs.email = "harshada@vaspsolutions.com";
                cs.phoneNumber = "9011085421";
                cs.merchantTxnId = merchantTxnId;
                cs.notifyUrl = "";

                cs.returnUrl = "https://www.offertraker.com:9999/Consumer/PaymentResponseForNativeApp";
                cs.orderAmount = orderAmount;
                cs.secSignature = securitySignature;


                EventLog.LogData("Sending request to citrus pay for amount Rs " + cs.orderAmount + " for consumer " + Session["UserName"], true);
                EventLog.LogData(securitySignature, true);
                ViewBag.flag = "show Pay Button";

                return View(cs);
            }
            catch (Exception ex)
            {
                //call error view here
                //log the error
                EventLog.LogErrorData("Error occurred Consumer/PayForGiftCards." + ex.Message, true);
                return Json("Error occurred while getting payment gateway details", JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult PaymentResponseForNativeApp()
        {
            try
            {
                string consumerid = Session["consumerid"].ToString();
                string merchantid = Session["merchantid"].ToString();
                string giftCardId = Session["giftCardId"].ToString();
                string Denomination = Session["Denomination"].ToString();
                string qty = Session["qty"].ToString();


                int quantity = Convert.ToInt32(qty);
                string secret_key = "32908f0fd900b405c3bbcf15041cbcbba3f735a7";
                string data = "";
                string txnId = Request["TxId"];
                string txnStatus = Request["TxStatus"];
                string amount = Request["amount"];
                string pgTxnId = Request["pgTxnNo"];
                string issuerRefNo = Request["issuerRefNo"];
                string authIdCode = Request["authIdCode"];
                string firstName = Request["firstName"];
                string lastName = Request["lastName"];
                string pgRespCode = Request["pgRespCode"];
                string zipCode = Request["addressZip"];
                string resSignature = Request["signature"];
                bool flag = true;
                if (txnId != null)
                {
                    data += txnId;
                }
                if (txnStatus != null)
                {
                    data += txnStatus;
                }
                if (amount != null)
                {
                    data += amount;
                }
                if (pgTxnId != null)
                {
                    data += pgTxnId;
                }
                if (issuerRefNo != null)
                {
                    data += issuerRefNo;
                }
                if (authIdCode != null)
                {
                    data += authIdCode;
                }
                if (firstName != null)
                {
                    data += firstName;
                }
                if (lastName != null)
                {
                    data += lastName;
                }
                if (pgRespCode != null)
                {
                    data += pgRespCode;
                }
                if (zipCode != null)
                {
                    data += zipCode;
                }
                System.Security.Cryptography.HMACSHA1 myhmacsha1 = new System.Security.Cryptography.HMACSHA1(Encoding.ASCII.GetBytes(secret_key));
                System.IO.MemoryStream stream = new System.IO.MemoryStream(Encoding.ASCII.GetBytes(data));
                string signature = BitConverter.ToString(myhmacsha1.ComputeHash(stream)).Replace("-", "").ToLower();

                if (resSignature != null && !signature.Equals(resSignature))
                {
                    flag = false;
                }
                if (flag)
                {
                    using (consumerEntities dataContext = new consumerEntities())
                    {

                        int merchantno = Convert.ToInt32(merchantid);
                        merchant_master master = dataContext.merchant_master.Where(x => x.merchantid == merchantno).FirstOrDefault();

                        using (instadelight_consumerEntities consumerDatacontext = new instadelight_consumerEntities())
                        {

                            for (int i = 1; i <= quantity; i++)
                            {
                                consumergiftcarddetail gifts = new consumergiftcarddetail();
                                gifts.ConsumerId = consumerid;
                                gifts.MerchantId = master.UserId;
                                gifts.GiftCardId = Convert.ToInt32(giftCardId);
                                gifts.DenominationRs = Convert.ToInt32(Denomination);
                                gifts.Status = 0;
                                gifts.DateOfPurchase = DateTime.Now;
                                gifts.ValidTill = DateTime.Now.AddDays(364);
                                consumerDatacontext.consumergiftcarddetails.Add(gifts);
                                consumerDatacontext.SaveChanges();
                            }

                            //Send SMS/Email to consumer for a new gift card
                            user consumer = consumerDatacontext.users.Where(x => x.Id == consumerid).FirstOrDefault();
                            consumermaster consumermaster = consumerDatacontext.consumermasters.Where(x => x.UserId == consumerid).FirstOrDefault();

                            if (consumer.UserName.Contains('@') == false)
                            {
                                SMSUtility sms = new SMSUtility();
                                //string smsresult = sms.sendMessage(param.mobileno, "Dear Customer, your " + Session["UserName"] + " Digital Card is updated. Download the app for Android at goo.gl/r5rxjj and for Apple at goo.gl/dAq4er. Your cell number is your login and your password is 123456 unless you have reset it.");
                                string smsresult = sms.sendMessage(consumer.UserName, "Dear Customer, thank you for buying our Gift card. You can view the gift card in the \"purchased\" section of Gift cards on our DEC. Best wishes, " + master.DECName);

                                consumersmsdetail smsdetails = new consumersmsdetail();
                                smsdetails.ConsumerId = consumerid;
                                smsdetails.MerchantId = master.UserId;
                                smsdetails.SMSEmailStatus = smsresult;
                                smsdetails.UserName = consumer.UserName;
                                smsdetails.SentDate = DateTime.Now;
                                consumerDatacontext.consumersmsdetails.Add(smsdetails);
                                consumerDatacontext.SaveChanges();
                            }
                            else
                            {
                                EmailModel model = new EmailModel();
                                model.To = consumer.UserName;
                                model.Email = "welcome@offertraker.com";
                                model.Subject = "Thank you for buying a gift card !";
                                //gp@instadelight.com
                                model.bcc = "gp@instadelight.com";
                                string consumername = consumer.UserName;
                                if (consumermaster.consumername != null)
                                {
                                    consumername = consumermaster.consumername;
                                }


                                model.Body = "Dear " + consumername + ", <br />Thank you for buying a gift card from " + master.DECName + ". <br />It's so easy to send this gift card to someone. Go ahead and enjoy the card. Please check the terms and conditions and the validity of the gift card.<br /> Best wishes,<br /> Offertraker team.";

                                SendEmail email = new SendEmail();
                                string result = email.SendEmailToConsumer(model);
                                consumersmsdetail smsdetails = new consumersmsdetail();
                                smsdetails.ConsumerId = consumerid;
                                smsdetails.MerchantId = master.UserId;
                                smsdetails.SMSEmailStatus = result;
                                smsdetails.UserName = consumer.UserName;
                                smsdetails.SentDate = DateTime.Now;
                                consumerDatacontext.consumersmsdetails.Add(smsdetails);
                                consumerDatacontext.SaveChanges();
                            }


                            ViewBag.Result = "Your Unique Transaction/Order Id : " + txnId;
                            ViewBag.Status = "Transaction Status : " + txnStatus;

                            EventLog.LogData("Your Unique Transaction/Order Id : " + txnId, true);
                            EventLog.LogData("Transaction Status : " + txnStatus, true);

                            consumergiftcardtransactiondetail logtrans = consumerDatacontext.consumergiftcardtransactiondetails.Where(x => x.MerchantId == master.UserId && x.ConsumerId == consumerid).OrderByDescending(x => x.RequestDateTime).FirstOrDefault();
                            if (logtrans != null)
                            {
                                logtrans.ResponseTransactionId = txnId;
                                logtrans.ResponseStatus = txnStatus;
                                logtrans.ResponseDateTime = DateTime.Now;

                                consumerDatacontext.SaveChanges();
                            }
                        }

                    }

                }
                else
                {
                    ViewBag.Result = "";
                    ViewBag.Status = "Citrus Response Signature and Our (Merchant) Signature Mis-Mactch";
                    EventLog.LogErrorData("Citrus Response Signature and Our (Merchant) Signature Mis-Mactch", true);
                }

                Session.Remove("consumerid");
                Session.Remove("merchantid");
                Session.Remove("giftCardId");
                Session.Remove("Denomination");
                Session.Remove("qty");

                return View();
            }
            catch (Exception ex)
            {
                //call error view here
                //log the error

                Session.Remove("consumerid");
                Session.Remove("merchantid");
                Session.Remove("giftCardId");
                Session.Remove("Denomination");
                Session.Remove("qty");

                EventLog.LogErrorData("Error occurred Consumer/PaymentResponse." + ex.Message, true);
                ViewBag.Result = "";
                ViewBag.Status = "Error occured while getting payment response";

                return View();
            }
        }
    }
}