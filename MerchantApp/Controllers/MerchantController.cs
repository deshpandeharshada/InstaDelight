using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Web.Mvc;
using MerchantApp.Models;
using AspNet.Identity.MySQL;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.Google;
using Owin;
using QRCoder;
using System.IO;
using System.Drawing;
using System.Web.Script.Serialization;
using MySql.Data.MySqlClient;
using MerchantApp.Filters;
using System.Security.Claims;


namespace MerchantApp.Controllers
{
    [AdminMerchantFilter]
    public class MerchantController : Controller
    {

        public class ValidLocations
        {
            public int id { get; set; }
        }

        MerchantCommonFunctions function = new MerchantCommonFunctions();

        ApplicationDbContext context = new ApplicationDbContext("DefaultConnection");
        ApplicationDbContext Consumercontext = new ApplicationDbContext("DefaultConsumerConnection");


        public ActionResult ViewReport()
        {
            return View();
        }


        public ActionResult VerifyAndRedeemCoupon(string CouponId, string SharedCouponId)
        {
            ViewBag.CouponId = CouponId;
            ViewBag.SharedCouponId = SharedCouponId;
            //ViewBag.result = "";
            return View();
        }

        public ActionResult VerifyAndRedeemGiftCard(string Id)
        {
            ViewBag.Id = Id;
            using (instadelight_consumerEntities datacontext = new instadelight_consumerEntities())
            {
                int no = Convert.ToInt32(Id);
                try
                {
                    string userid = Session["UserId"].ToString();


                    consumergiftcarddetail giftcard = datacontext.consumergiftcarddetails.Where(x => x.Id == no).FirstOrDefault();
                    consumeruser consumer = datacontext.consumerusers.Where(x => x.Id == giftcard.ConsumerId).FirstOrDefault();

                    if (consumer != null)
                    {
                        //Generate OTP

                        string newotp = "";
                        consumer_giftcard_otp currentotpdetails = datacontext.consumer_giftcard_otp.Where(x => x.GiftCardId == no).FirstOrDefault();
                        if (currentotpdetails != null)
                        {
                            if (string.IsNullOrEmpty(currentotpdetails.OTP) == false)
                            {
                                newotp = currentotpdetails.OTP;
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
                            if (consumer.UserName.Contains('@'))
                            {
                                //Send Email
                                EmailModel model = new EmailModel();
                                model.To = consumer.UserName;
                                model.Email = "verify@offertraker.com";
                                model.Subject = "OTP for Gift Card redemption";
                                string body = "Dear Customer, <br />Please share this OTP " + newotp.ToString() + " with the business, only if you wish to redeem your gift card. Gift card that is once redeemed can not be reverted back in any circumstances. <br /> Best wishes, <br /> Offertraker team.";
                                model.Body = body;
                                SendEmail eml = new SendEmail();
                                string smsresult = eml.SendEmailToConsumer(model);
                                if (smsresult.Contains("Email sent"))
                                {
                                    //Store in database
                                    consumer_giftcard_otp otpdetails = datacontext.consumer_giftcard_otp.Where(x => x.GiftCardId == no).FirstOrDefault();
                                    if (otpdetails == null)
                                    {
                                        otpdetails = new consumer_giftcard_otp();
                                        otpdetails.GiftCardId = no;
                                        otpdetails.OTP = newotp;
                                        datacontext.consumer_giftcard_otp.Add(otpdetails);
                                        datacontext.SaveChanges();
                                    }
                                    else
                                    {
                                        otpdetails.OTP = newotp;
                                        datacontext.SaveChanges();
                                    }

                                    ViewBag.Result = "OTP Sent Successfully.";
                                }
                                else
                                {
                                    ViewBag.Result = "Error occurred while sending OTP to email id " + consumer.UserName;
                                }
                            }
                            else
                            {
                                //Send SMS
                                SMSUtility sms = new SMSUtility();
                                string smsresult = sms.sendMessage(consumer.UserName, "Dear consumer,  if you wish to redeem your Gift Card, then please share the OTP " + newotp + " with the Business staff now. Please do not share this OTP if you do not wish to redeem. Offertraker team");

                                if (smsresult.Contains("SMS sent successfully") && smsresult.Contains("error") == false)
                                {
                                    //Store in database                                        
                                    consumer_giftcard_otp otpdetails = datacontext.consumer_giftcard_otp.Where(x => x.GiftCardId == no).FirstOrDefault();
                                    if (otpdetails == null)
                                    {
                                        otpdetails = new consumer_giftcard_otp();
                                        otpdetails.GiftCardId = no;
                                        otpdetails.OTP = newotp;
                                        datacontext.consumer_giftcard_otp.Add(otpdetails);
                                        datacontext.SaveChanges();
                                    }
                                    else
                                    {
                                        otpdetails.OTP = newotp;
                                        datacontext.SaveChanges();
                                    }

                                    ViewBag.Result = "OTP Sent Successfully.";
                                }
                                else
                                {
                                    ViewBag.Result = "Error occurred while sending OTP to " + consumer.UserName;
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    //call error view here
                    //log the error
                    EventLog.LogErrorData("Error occurred Consumer/VerifyAndRedeemGiftCard." + ex.Message, true);
                    ViewBag.Result = "Error occured while sending OTP to consumer";
                }
            }
            return View();
        }

        public string VerifyGiftcardOTP(string OTP, string GiftCardId)
        {
            try
            {
                string userid = Session["UserId"].ToString();
                int no = Convert.ToInt32(GiftCardId);

                using (instadelight_consumerEntities dataContext = new instadelight_consumerEntities())
                {
                    if (string.IsNullOrEmpty(OTP) == false)
                    {
                        consumer_giftcard_otp otpdetail = dataContext.consumer_giftcard_otp.Where(x => x.GiftCardId == no && x.OTP == OTP).FirstOrDefault();
                        if (otpdetail != null)
                        {
                            //Valid OTP
                            otpdetail.OTP = "";
                            dataContext.SaveChanges();
                            return Global.Merchant.OTPVerified;
                        }
                        else
                        {
                            return Global.Merchant.InvalidOTP;
                        }
                    }
                    else
                    {
                        return Global.Merchant.InvalidOTP;
                    }
                }
            }
            catch (Exception ex)
            {
                //call error view here
                //log the error
                EventLog.LogErrorData("Error occurred Merchant/VerifyGiftcardOTP." + ex.Message, true);
                return Global.Merchant.VerifyOTPException;
            }
        }

        public string VerifyCouponOTP(string OTP, string SharedCouponId)
        {
            try
            {
                string userid = Session["UserId"].ToString();
                int no = Convert.ToInt32(SharedCouponId);

                using (instadelight_consumerEntities dataContext = new instadelight_consumerEntities())
                {
                    if (string.IsNullOrEmpty(OTP) == false)
                    {
                        consumer_coupon_otp otpdetail = dataContext.consumer_coupon_otp.Where(x => x.SharedCouponId == no && x.OTP == OTP).FirstOrDefault();
                        if (otpdetail != null)
                        {
                            //Valid OTP
                            otpdetail.OTP = "";
                            dataContext.SaveChanges();
                            return Global.Merchant.OTPVerified;
                        }
                        else
                        {
                            return Global.Merchant.InvalidOTP;
                        }
                    }
                    else
                    {
                        return Global.Merchant.InvalidOTP;
                    }
                }
            }
            catch (Exception ex)
            {
                //call error view here
                //log the error
                EventLog.LogErrorData("Error occurred Merchant/VerifyCouponOTP." + ex.Message, true);
                return Global.Merchant.VerifyOTPException;
            }
        }

        public string SendCouponOTP(string sharedcouponid)
        {
            using (MerchantEntities datacontext = new MerchantEntities())
            {
                int no = Convert.ToInt32(sharedcouponid);
                try
                {
                    merchantconsumercoupondetail coupon = datacontext.merchantconsumercoupondetails.Where(x => x.Id == no).FirstOrDefault();

                    if (coupon != null)
                    {
                        //Generate OTP
                        using (instadelight_consumerEntities consumerDataContext = new instadelight_consumerEntities())
                        {
                            consumeruser consumer = consumerDataContext.consumerusers.Where(x => x.Id == coupon.ConsumerId).FirstOrDefault();
                            if (consumer != null)
                            {
                                string newotp = "";
                                consumer_coupon_otp currentotpdetails = consumerDataContext.consumer_coupon_otp.Where(x => x.SharedCouponId == no).FirstOrDefault();
                                if (currentotpdetails != null)
                                {
                                    if (string.IsNullOrEmpty(currentotpdetails.OTP) == false)
                                    {
                                        newotp = currentotpdetails.OTP;
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
                                    if (consumer.UserName.Contains('@'))
                                    {
                                        //Send Email
                                        EmailModel model = new EmailModel();
                                        model.To = consumer.UserName;
                                        model.Email = "verify@offertraker.com";
                                        model.Subject = "OTP for coupon redemption";
                                        string body = "Dear Customer, <br />Please share this OTP " + newotp.ToString() + " with the business, only if you wish to redeem your coupon. Coupon that is once redeemed can not be reverted back in any circumstances. <br />Best wishes, <br />Offertraker team.";
                                        model.Body = body;
                                        SendEmail eml = new SendEmail();
                                        string smsresult = eml.SendEmailToConsumer(model);
                                        if (smsresult.Contains("Email sent"))
                                        {
                                            //Store in database
                                            consumer_coupon_otp otpdetails = consumerDataContext.consumer_coupon_otp.Where(x => x.SharedCouponId == no).FirstOrDefault();
                                            if (otpdetails == null)
                                            {
                                                otpdetails = new consumer_coupon_otp();
                                                otpdetails.SharedCouponId = no;
                                                otpdetails.OTP = newotp;
                                                consumerDataContext.consumer_coupon_otp.Add(otpdetails);
                                                consumerDataContext.SaveChanges();
                                            }
                                            else
                                            {
                                                otpdetails.OTP = newotp;
                                                consumerDataContext.SaveChanges();
                                            }

                                            return "OTP Sent Successfully.";
                                        }
                                        else
                                        {
                                            return "Error occurred while sending OTP to email id " + consumer.UserName;
                                        }
                                    }
                                    else
                                    {
                                        //Send SMS
                                        SMSUtility sms = new SMSUtility();
                                        string smsresult = sms.sendMessage(consumer.UserName, "Dear consumer,  if you wish to redeem your coupon, then please share the OTP " + newotp + " with the Business staff now. Please do not share this OTP if you do not wish to redeem. Offertraker team.");

                                        //if (smsresult.Contains("SMS sent successfully") && smsresult.Contains("error") == false)
                                        if (smsresult.Contains("SMS sent successfully"))
                                        {
                                            //Store in database                                        
                                            consumer_coupon_otp otpdetails = consumerDataContext.consumer_coupon_otp.Where(x => x.SharedCouponId == no).FirstOrDefault();
                                            if (otpdetails == null)
                                            {
                                                otpdetails = new consumer_coupon_otp();
                                                otpdetails.SharedCouponId = no;
                                                otpdetails.OTP = newotp;
                                                consumerDataContext.consumer_coupon_otp.Add(otpdetails);
                                                consumerDataContext.SaveChanges();
                                            }
                                            else
                                            {
                                                otpdetails.OTP = newotp;
                                                consumerDataContext.SaveChanges();
                                            }

                                            return "OTP Sent Successfully.";
                                        }
                                        else
                                        {
                                            return "Error occurred while sending OTP to " + consumer.UserName;
                                        }
                                    }
                                }
                                else
                                {
                                    EventLog.LogErrorData("Error occurred Merchant/SendCouponOTP. New OTP cant be generated", true);
                                    return "New OTP cant be generated";
                                }
                            }
                            else
                            {
                                EventLog.LogErrorData("Error occurred Merchant/SendCouponOTP. Consumer does not exist", true);
                                return "Consumer does not exist";
                            }
                        }
                    }
                    else
                    {
                        EventLog.LogErrorData("Error occurred Merchant/SendCouponOTP. Shared coupon does not exist", true);
                        return "Shared coupon does not exist";
                    }
                }
                catch (Exception ex)
                {
                    //call error view here
                    //log the error
                    EventLog.LogErrorData("Error occurred Merchant/SendCouponOTP." + ex.Message, true);
                    return "Error occured while sending OTP to consumer";
                }
            }
        }

        public ActionResult ManageLicenses()
        {
            string userid = Session["UserId"].ToString();

            var roles = ((ClaimsIdentity)User.Identity).Claims
              .Where(c => c.Type == ClaimTypes.Role).FirstOrDefault();

            string isUserCreationAllowed = "true";

            if (roles.Value == "Staff")
            {
                isUserCreationAllowed = "false";

            }
            else if (roles.Value == "LocationManager")
            {
                using (MerchantEntities dataContext = new MerchantEntities())
                {

                    merchant_master master = dataContext.merchant_master.Where(x => x.UserId == userid).FirstOrDefault();
                    if (master != null)
                    {
                        branch_master locmgr = dataContext.branch_master.Where(x => x.BranchManagerId == master.merchantid).FirstOrDefault();
                        if (locmgr != null)
                        {
                            if (locmgr.IsAddUserAllowed == false)
                            {
                                isUserCreationAllowed = "false";
                            }
                        }
                    }
                }
            }

            ViewBag.isUserCreationAllowed = isUserCreationAllowed;
            using (MerchantEntities dataContext = new MerchantEntities())
            {
                merchant_master master = dataContext.merchant_master.Where(x => x.UserId == userid).FirstOrDefault();
                ViewBag.countryid = master.Country;
            }


            return View();
        }

        public ActionResult CustomerList()
        {
            return View();
        }

        public ActionResult CustomerSummaryReport(string ConsumerId)
        {
            try
            {
                string userid = Session["UserId"].ToString();
                List<CustomerSummaryViewModel> summary = new List<CustomerSummaryViewModel>();

                using (MerchantEntities dataContext = new MerchantEntities())
                {
                    if (string.IsNullOrEmpty(ConsumerId) == false)
                    {
                        using (instadelight_consumerEntities consumerDataContext = new instadelight_consumerEntities())
                        {
                            consumeruser user = consumerDataContext.consumerusers.Where(x => x.Id == ConsumerId).FirstOrDefault();
                            if (user != null)
                            {
                                int srno = 1;
                                List<consumervisitdetail> visits = consumerDataContext.consumervisitdetails.Where(x => x.ConsumerId == user.Id && x.MerchantId == userid).OrderByDescending(x => x.VisitDate).ToList();
                                foreach (consumervisitdetail v in visits)
                                {
                                    CustomerSummaryViewModel s = new CustomerSummaryViewModel();
                                    s.serialno = srno;
                                    s.Date = Convert.ToDateTime(v.VisitDate);
                                    s.UserName = user.UserName;
                                    s.VisitNumber = srno;

                                    DateTime visitdate = Convert.ToDateTime(v.VisitDate);

                                    double amountspent = Convert.ToDouble(consumerDataContext.consumerbilldetails.Where(x => x.ConsumerId == user.Id && x.MerchantId == userid && x.BillDate == visitdate).Select(x => x.BillAmount).Sum());
                                    s.AmountSpent = amountspent;

                                    double totalamountspent = Convert.ToDouble(consumerDataContext.consumerbilldetails.Where(x => x.ConsumerId == user.Id && x.MerchantId == userid).Select(x => x.BillAmount).Sum());
                                    s.TotalAmountSpent = totalamountspent;

                                    int visitpoints = Convert.ToInt32(consumerDataContext.consumerrewardpoints.Where(x => x.ConsumerId == user.Id && x.MerchantId == userid && x.VisitDate == visitdate).Select(x => x.Points).Sum());
                                    s.PointsEarned = visitpoints;

                                    int totalpoints = Convert.ToInt32(consumerDataContext.consumerrewardpoints.Where(x => x.ConsumerId == user.Id && x.MerchantId == userid).Select(x => x.Points).Sum());
                                    s.TotalPoints = totalpoints;

                                    int pointsredeemed = Convert.ToInt32(consumerDataContext.consumerredeemdetails.Where(x => x.ConsumerId == user.Id && x.MerchantId == userid).Select(x => x.PointsRedeemed).Sum());
                                    s.PointsRedeemed = pointsredeemed;

                                    consumersmsdetail sms = consumerDataContext.consumersmsdetails.Where(x => x.ConsumerId == user.Id && x.MerchantId == userid).OrderByDescending(x => x.SentDate).FirstOrDefault();
                                    if (sms != null)
                                    {
                                        s.smsemailstatus = sms.SMSEmailStatus;
                                    }

                                    consumermaster master = consumerDataContext.consumermasters.Where(x => x.UserId == user.Id).FirstOrDefault();
                                    s.Birthday = Convert.ToDateTime(master.DOB);
                                    s.Anniversary = Convert.ToDateTime(master.DOA);

                                    summary.Add(s);
                                    srno++;
                                }
                            }
                        }
                    }


                    return View(summary);
                }

            }
            catch (Exception ex)
            {
                if (ex.InnerException != null)
                    EventLog.LogErrorData("Error occurred in Merchant/CustomerSummaryReport: " + ex.InnerException.Message, true);
                else
                    EventLog.LogErrorData("Error occurred in Merchant/CustomerSummaryReport: " + ex.Message, true);

                return RedirectToAction("Login", "Account");
            }
        }

        public ActionResult KeyMetricsReport()
        {
            try
            {
                ViewBag.Result = Global.Merchant.KeyBusinessMetrics;

                using (MerchantEntities dataContext = new MerchantEntities())
                {
                    string userid = Session["UserId"].ToString();
                    if (User.IsInRole("Staff"))
                    {
                        merchant_master staffmaster = dataContext.merchant_master.Where(x => x.UserId == userid).FirstOrDefault();
                        staff_master stfmgr = dataContext.staff_master.Where(x => x.StaffMasterId == staffmaster.merchantid).FirstOrDefault();
                        merchant_master owner = dataContext.merchant_master.Where(x => x.merchantid == stfmgr.MerchantId).FirstOrDefault();
                        userid = owner.UserId;
                    }
                    else if (User.IsInRole("LocationManager"))
                    {
                        merchant_master branchmaster = dataContext.merchant_master.Where(x => x.UserId == userid).FirstOrDefault();
                        branch_master branch = dataContext.branch_master.Where(x => x.BranchManagerId == branchmaster.merchantid).FirstOrDefault();
                        merchant_master owner = dataContext.merchant_master.Where(x => x.merchantid == branch.MerchantId).FirstOrDefault();
                        userid = owner.UserId;
                    }

                    merchant_master master = dataContext.merchant_master.Where(x => x.UserId == userid).FirstOrDefault();

                    //Getting consumer count here
                    var consumercount = dataContext.merchantconsumerdetails.Where(x => x.MerchantId == userid).Select(x => x.ConsumerId).Distinct().Count();
                    ViewBag.ConsumerCount = consumercount;

                    var smscount = dataContext.merchantsmsdetails.Where(x => x.MerchantId == userid).FirstOrDefault();
                    if (smscount != null)
                    {
                        ViewBag.SMSCount = smscount.SMSCount;
                        ViewBag.EmailCount = smscount.EmailCount;
                        if (master.NoOfSMS != null)
                            ViewBag.NoOfSMS = master.NoOfSMS;
                        else
                            ViewBag.NoOfSMS = 6000;
                    }
                }

                return View();
            }
            catch (Exception ex)
            {
                EventLog.LogErrorData("Error occurred in Merchant/KeyMetricsReport: " + ex.Message, true);
                return RedirectToAction("Login", "Account");
            }
        }

        public ActionResult ReviewReport()
        {
            try
            {
                ViewBag.Result = Global.Merchant.ReviewReport;
                //get review details and send it via model
                using (MerchantEntities dataContext = new MerchantEntities())
                {
                    string userid = Session["UserId"].ToString();
                    if (User.IsInRole("Staff"))
                    {
                        merchant_master staffmaster = dataContext.merchant_master.Where(x => x.UserId == userid).FirstOrDefault();
                        staff_master stfmgr = dataContext.staff_master.Where(x => x.StaffMasterId == staffmaster.merchantid).FirstOrDefault();
                        merchant_master owner = dataContext.merchant_master.Where(x => x.merchantid == stfmgr.MerchantId).FirstOrDefault();
                        userid = owner.UserId;
                    }
                    else if (User.IsInRole("LocationManager"))
                    {
                        merchant_master branchmaster = dataContext.merchant_master.Where(x => x.UserId == userid).FirstOrDefault();
                        branch_master branch = dataContext.branch_master.Where(x => x.BranchManagerId == branchmaster.merchantid).FirstOrDefault();
                        merchant_master owner = dataContext.merchant_master.Where(x => x.merchantid == branch.MerchantId).FirstOrDefault();
                        userid = owner.UserId;
                    }

                    merchant_master master = dataContext.merchant_master.Where(x => x.UserId == userid).FirstOrDefault();

                    reviewmaster review = dataContext.reviewmasters.Where(x => x.MerchantId == master.merchantid).OrderByDescending(x => x.CreationDate).FirstOrDefault();
                    if (review != null)
                    {
                        if (dataContext.review_submit_details.Any(x => x.reviewid == review.reviewid && x.MerchantId == master.merchantid))
                        {
                            MySqlParameter param1 = new MySqlParameter();
                            param1.Value = review.reviewid;
                            param1.Direction = System.Data.ParameterDirection.Input;
                            param1.ParameterName = "@rid";
                            param1.DbType = System.Data.DbType.Int32;
                            MySqlParameter param2 = new MySqlParameter();
                            param2.Value = master.merchantid;
                            param2.Direction = System.Data.ParameterDirection.Input;
                            param2.ParameterName = "@mid";
                            param2.DbType = System.Data.DbType.Int32;

                            List<SubmittedReviewDetails> submittedReviewDetails = dataContext.Database.SqlQuery<SubmittedReviewDetails>("CALL GetSubmittedReviewDetails(@rid, @mid)", param1, param2)
                             .Select(x => new SubmittedReviewDetails
                             {
                                 percentage = x.percentage == null ? 0 : Convert.ToDouble(x.percentage),
                                 que = x.que == null ? "" : x.que.ToString(),
                                 queNo = x.queNo,
                                 star = x.star,
                                 totalSubmittedReviews = x.totalSubmittedReviews,
                                 reviewAttempted = x.reviewAttempted
                             }).ToList();
                            if (submittedReviewDetails.Count > 0)
                            {
                                ReviewReportViewModel reviewDetails = new ReviewReportViewModel()
                                {
                                    Question1 = submittedReviewDetails[0].que,
                                    Question1Percentage1 = submittedReviewDetails[0].percentage,
                                    Question1Percentage2 = submittedReviewDetails[1].percentage,
                                    Question1Percentage3 = submittedReviewDetails[2].percentage,
                                    Question1Percentage4 = submittedReviewDetails[3].percentage,
                                    Question1Percentage5 = submittedReviewDetails[4].percentage,

                                    Question1TotalSubmittedReviews1 = submittedReviewDetails[0].totalSubmittedReviews,
                                    Question1TotalSubmittedReviews2 = submittedReviewDetails[1].totalSubmittedReviews,
                                    Question1TotalSubmittedReviews3 = submittedReviewDetails[2].totalSubmittedReviews,
                                    Question1TotalSubmittedReviews4 = submittedReviewDetails[3].totalSubmittedReviews,
                                    Question1TotalSubmittedReviews5 = submittedReviewDetails[4].totalSubmittedReviews,

                                    Question1ReviewAttempted1 = submittedReviewDetails[0].reviewAttempted,
                                    Question1ReviewAttempted2 = submittedReviewDetails[1].reviewAttempted,
                                    Question1ReviewAttempted3 = submittedReviewDetails[2].reviewAttempted,
                                    Question1ReviewAttempted4 = submittedReviewDetails[3].reviewAttempted,
                                    Question1ReviewAttempted5 = submittedReviewDetails[4].reviewAttempted,


                                    Question2 = submittedReviewDetails[5].que,
                                    Question2Percentage1 = submittedReviewDetails[5].percentage,
                                    Question2Percentage2 = submittedReviewDetails[6].percentage,
                                    Question2Percentage3 = submittedReviewDetails[7].percentage,
                                    Question2Percentage4 = submittedReviewDetails[8].percentage,
                                    Question2Percentage5 = submittedReviewDetails[9].percentage,

                                    Question2TotalSubmittedReviews1 = submittedReviewDetails[5].totalSubmittedReviews,
                                    Question2TotalSubmittedReviews2 = submittedReviewDetails[6].totalSubmittedReviews,
                                    Question2TotalSubmittedReviews3 = submittedReviewDetails[7].totalSubmittedReviews,
                                    Question2TotalSubmittedReviews4 = submittedReviewDetails[8].totalSubmittedReviews,
                                    Question2TotalSubmittedReviews5 = submittedReviewDetails[9].totalSubmittedReviews,

                                    Question2ReviewAttempted1 = submittedReviewDetails[5].reviewAttempted,
                                    Question2ReviewAttempted2 = submittedReviewDetails[6].reviewAttempted,
                                    Question2ReviewAttempted3 = submittedReviewDetails[7].reviewAttempted,
                                    Question2ReviewAttempted4 = submittedReviewDetails[8].reviewAttempted,
                                    Question2ReviewAttempted5 = submittedReviewDetails[9].reviewAttempted,

                                    Question3 = submittedReviewDetails[10].que,
                                    Question3Percentage1 = submittedReviewDetails[10].percentage,
                                    Question3Percentage2 = submittedReviewDetails[11].percentage,
                                    Question3Percentage3 = submittedReviewDetails[12].percentage,
                                    Question3Percentage4 = submittedReviewDetails[13].percentage,
                                    Question3Percentage5 = submittedReviewDetails[14].percentage,

                                    Question3TotalSubmittedReviews1 = submittedReviewDetails[10].totalSubmittedReviews,
                                    Question3TotalSubmittedReviews2 = submittedReviewDetails[11].totalSubmittedReviews,
                                    Question3TotalSubmittedReviews3 = submittedReviewDetails[12].totalSubmittedReviews,
                                    Question3TotalSubmittedReviews4 = submittedReviewDetails[13].totalSubmittedReviews,
                                    Question3TotalSubmittedReviews5 = submittedReviewDetails[14].totalSubmittedReviews,

                                    Question3ReviewAttempted1 = submittedReviewDetails[10].reviewAttempted,
                                    Question3ReviewAttempted2 = submittedReviewDetails[11].reviewAttempted,
                                    Question3ReviewAttempted3 = submittedReviewDetails[12].reviewAttempted,
                                    Question3ReviewAttempted4 = submittedReviewDetails[13].reviewAttempted,
                                    Question3ReviewAttempted5 = submittedReviewDetails[14].reviewAttempted,

                                    Question4 = submittedReviewDetails[15].que,
                                    Question4Percentage1 = submittedReviewDetails[15].percentage,
                                    Question4Percentage2 = submittedReviewDetails[16].percentage,
                                    Question4Percentage3 = submittedReviewDetails[17].percentage,
                                    Question4Percentage4 = submittedReviewDetails[18].percentage,
                                    Question4Percentage5 = submittedReviewDetails[19].percentage,

                                    Question4TotalSubmittedReviews1 = submittedReviewDetails[15].totalSubmittedReviews,
                                    Question4TotalSubmittedReviews2 = submittedReviewDetails[16].totalSubmittedReviews,
                                    Question4TotalSubmittedReviews3 = submittedReviewDetails[17].totalSubmittedReviews,
                                    Question4TotalSubmittedReviews4 = submittedReviewDetails[18].totalSubmittedReviews,
                                    Question4TotalSubmittedReviews5 = submittedReviewDetails[19].totalSubmittedReviews,

                                    Question4ReviewAttempted1 = submittedReviewDetails[15].reviewAttempted,
                                    Question4ReviewAttempted2 = submittedReviewDetails[16].reviewAttempted,
                                    Question4ReviewAttempted3 = submittedReviewDetails[17].reviewAttempted,
                                    Question4ReviewAttempted4 = submittedReviewDetails[18].reviewAttempted,
                                    Question4ReviewAttempted5 = submittedReviewDetails[19].reviewAttempted
                                };
                                reviewDetails.TotalReviews = dataContext.review_submit_details.Where(x => x.reviewid == review.reviewid && x.MerchantId == master.merchantid).ToList().Count;
                                return View(reviewDetails);
                            }
                            else
                            {
                                ViewBag.Result = "No reviews exist";
                            }
                        }
                        else
                        {
                            ViewBag.Result = "No reviews submitted yet by consumers.";
                        }
                    }
                    else
                    {
                        //no review details exists for given reviewId and MerchantId
                        ViewBag.Result = "No reviews exist";
                    }
                }
                return View();
            }
            catch (Exception ex)
            {
                EventLog.LogErrorData("Error occurred in Merchant/ViewReport: " + ex.Message, true);
                return RedirectToAction("Login", "Account");
            }

        }

        public ActionResult Upload()
        {
            //ViewBag.result = "";
            return View();

        }



        [HttpPost]
        public ActionResult Upload(HttpPostedFileBase file)
        {
            try
            {
                using (MerchantEntities dataContext = new MerchantEntities())
                {
                    string userid = Session["UserId"].ToString();
                    merchant_master master = dataContext.merchant_master.Where(x => x.UserId == userid).FirstOrDefault();
                    if (User.IsInRole("LocationManager"))
                    {
                        branch_master branch = dataContext.branch_master.Where(x => x.BranchManagerId == master.merchantid).FirstOrDefault();
                        master = dataContext.merchant_master.Where(x => x.merchantid == branch.MerchantId).FirstOrDefault();
                    }

                    if (master != null)
                    {
                        // Verify that the user selected a file
                        if (file != null && file.ContentLength > 0)
                        {
                            var fileName = Path.GetFileName(file.FileName);

                            StreamReader csvReader = new StreamReader(file.InputStream);
                            int cnt = 0;
                            while (!csvReader.EndOfStream)
                            {
                                var line = csvReader.ReadLine();
                                string[] values = line.Split(',');
                                if (cnt == 0)
                                {
                                    //check headers.
                                    if (master.Category == 5)
                                    {
                                        if (values.Length != 5)
                                        {
                                            ViewBag.result = "File uploaded is not in correct format.";
                                            return View();
                                        }
                                        if (values[0] != "PackageName" || values[1] != "Price" || values[2] != "Discount" || values[3] != "Valid" || values[4] != "Available")
                                        {
                                            ViewBag.result = "File uploaded is not in correct format.";
                                            return View();
                                        }
                                    }
                                    else
                                    {
                                        if (values.Length != 9)
                                        {
                                            ViewBag.result = "File uploaded is not in correct format.";
                                            return View();
                                        }
                                        if (values[0] != "Category" || values[1] != "Subcategory" || values[2] != "ItemCode" || values[3] != "ItemName" || values[4] != "ItemPrice" || values[5] != "ItemDiscription" || values[6] != "GST" || values[7] != "PackagingCharges" || values[8] != "DeliveryCharge")
                                        {
                                            ViewBag.result = "File uploaded is not in correct format.";
                                            return View();
                                        }
                                    }
                                    cnt++;
                                }
                                else
                                {
                                    //insert values.
                                    if (master.Category == 5)
                                    {
                                        package pkg = new package();

                                        pkg.MerchantId = master.merchantid;
                                        pkg.BranchId = 0;
                                        if (string.IsNullOrEmpty(values[0]) == false)
                                            pkg.PackageName = values[0].ToString();

                                        if (string.IsNullOrEmpty(values[1]) == false)
                                            pkg.Price = Convert.ToDouble(values[1]);

                                        if (string.IsNullOrEmpty(values[2]) == false)
                                            pkg.Discount = Convert.ToDouble(values[2]);

                                        if (string.IsNullOrEmpty(values[3]) == false)
                                            if (function.CheckDate(values[3]) == true)
                                                pkg.Valid = Convert.ToDateTime(values[3]);

                                        if (string.IsNullOrEmpty(values[4]) == false)
                                            pkg.Available = Convert.ToInt32(values[4]);

                                        pkg.CreationDate = DateTime.Now;
                                        pkg.LastUpdate = DateTime.Now;

                                        dataContext.packages.Add(pkg);
                                        dataContext.SaveChanges();
                                    }
                                    else
                                    {
                                        menu mnu = new menu();

                                        mnu.MerchantId = master.merchantid;
                                        mnu.BranchId = 0;
                                        if (string.IsNullOrEmpty(values[0]) == false)
                                            mnu.Category = values[0].ToString();

                                        if (string.IsNullOrEmpty(values[1]) == false)
                                            mnu.Subcategory = values[1].ToString();

                                        if (string.IsNullOrEmpty(values[2]) == false)
                                            mnu.ItemCode = values[2].ToString();

                                        if (string.IsNullOrEmpty(values[3]) == false)
                                            mnu.ItemName = values[3];

                                        if (string.IsNullOrEmpty(values[4]) == false)
                                            mnu.ItemPrice = Convert.ToDouble(values[4]);

                                        if (string.IsNullOrEmpty(values[5]) == false)
                                            mnu.ItemDiscription = values[5].ToString();

                                        if (string.IsNullOrEmpty(values[6]) == false)
                                            mnu.GST = Convert.ToDouble(values[6]);

                                        if (string.IsNullOrEmpty(values[7]) == false)
                                            mnu.PackagingCharges = Convert.ToDouble(values[7]);

                                        if (string.IsNullOrEmpty(values[8]) == false)
                                            mnu.DeliveryCharge = Convert.ToDouble(values[8]);

                                        mnu.CreationDate = DateTime.Now;
                                        mnu.LastUpdate = DateTime.Now;

                                        dataContext.menus.Add(mnu);
                                        dataContext.SaveChanges();
                                    }

                                    cnt++;
                                }
                            }

                            MemoryStream target = new MemoryStream();
                            file.InputStream.CopyTo(target);
                            byte[] data = target.ToArray();

                            master.MenuCsvFile = data;
                            dataContext.SaveChanges();
                            ViewBag.result = "Menu uploaded successfully.";

                            return View();

                        }
                        else
                        {
                            EventLog.LogErrorData("file is null of file.ContentLength = 0", true);
                            ViewBag.result = "File is not uploaded";
                            return View();
                        }
                    }
                    else
                    {
                        EventLog.LogErrorData("merchant does not exist.", true);
                        ViewBag.result = "merchant does not exist.";
                        return View();
                    }
                }
            }
            catch (Exception ex)
            {
                EventLog.LogErrorData("Error occurred in Merchant/Upload: " + ex.Message, true);
                ViewBag.result = "Error occurred in Merchant/Upload: " + ex.Message;
                return View();
            }
        }

        public ActionResult SelectGroups(string CouponId)
        {
            ViewBag.CouponId = CouponId;
            return View();
        }

        public ActionResult UploadConsumerFile(string CouponId)
        {
            ViewBag.CouponId = CouponId;
            //ViewBag.result = "";
            return View();
        }

        public string sendCouponToGroups(string groupnames, string couponid)
        {
            try
            {
                if (groupnames != "")
                {
                    groupnames = groupnames.Remove(groupnames.LastIndexOf(","));

                    string[] groups = groupnames.Split(',');
                    if (groups.Count() > 0)
                    {
                        using (MerchantEntities dataContext = new MerchantEntities())
                        {
                            string userid = Session["UserId"].ToString();
                            for (int i = 0; i < groups.Count(); i++)
                            {
                                string name = groups[i];
                                List<consumergroup> grp = dataContext.consumergroups.Where(x => x.MerchantId == userid && x.GroupName == name).ToList();
                                foreach (consumergroup g in grp)
                                {
                                    if (couponid != "0")
                                    {
                                        string result = AddNewCouponConsumer(g.ConsumerUserId, couponid);
                                    }
                                    else
                                    {
                                        ShareDECParameters param = new ShareDECParameters();
                                        param.mobileno = g.ConsumerUserId;
                                        param.BillAmt = "0";
                                        string result = AddNewDECConsumer(param);
                                    }
                                    if (couponid != "0")
                                    {
                                        ViewBag.result = Global.Merchant.CouponSharedToGroupMessage;
                                    }
                                    else
                                    {
                                        ViewBag.result = Global.Merchant.DECSharedToGroupMessage;
                                    }
                                }
                            }

                            return ViewBag.result;
                        }
                    }
                    else
                    {
                        return Global.Merchant.NoGroupErrorMessage;
                    }
                }
                else
                {
                    return Global.Merchant.PleaseselectgroupErrorMessage;
                }
            }
            catch (Exception ex)
            {
                EventLog.LogErrorData("Error occurred in Merchant/sendCouponToGroups: " + ex.Message, true);
                return Global.Merchant.SendToGroupException;
            }
        }

        [HttpPost]
        public ActionResult UploadConsumerFile(HttpPostedFileBase file, string hdnCouponId, string group)
        {
            // Verify that the user selected a file
            try
            {
                if (file != null && file.ContentLength > 0)
                {
                    DataTable dt = new DataTable();
                    using (StreamReader sr = new StreamReader(file.InputStream))
                    {
                        string[] headers = sr.ReadLine().Split(',');
                        if (headers.Count() > 1)
                        {
                            ViewBag.result = Global.Merchant.CSVErrorMessage;
                            EventLog.LogErrorData("Error occurred in Merchant/UploadConsumerFile. CSV file is not in correct format. Please download sample file and try again.", true);

                            return View();
                        }
                        foreach (string header in headers)
                        {
                            if (header != "PhoneNumber")
                            {
                                ViewBag.result = Global.Merchant.CSVErrorMessage;
                                EventLog.LogErrorData("Error occurred in Merchant/UploadConsumerFile. CSV file is not in correct format. Please download sample file and try again.", true);

                                return View();
                            }

                            dt.Columns.Add(header);
                        }

                        while (!sr.EndOfStream)
                        {
                            string[] rows = sr.ReadLine().Split(',');
                            if (rows.Length > 0)
                            {
                                DataRow dr = dt.NewRow();
                                for (int i = 0; i < headers.Length; i++)
                                {
                                    dr[i] = rows[i].Trim();
                                }
                                dt.Rows.Add(dr);
                            }
                        }

                    }



                    using (MerchantEntities dataContext = new MerchantEntities())
                    {
                        if (Request.IsAuthenticated)
                        {
                            if (Session["UserId"] != null)
                            {
                                if (dt != null)
                                {
                                    if (dt.Rows.Count > 0)
                                    {
                                        string userid = Session["UserId"].ToString();
                                        for (int i = 0; i < dt.Rows.Count; i++)
                                        {
                                            if (hdnCouponId != "0")
                                            {
                                                string result = AddNewCouponConsumer(dt.Rows[i][0].ToString(), hdnCouponId);
                                            }
                                            else
                                            {
                                                ShareDECParameters param = new ShareDECParameters();
                                                param.mobileno = dt.Rows[i][0].ToString();
                                                param.BillAmt = "0";
                                                string result = AddNewDECConsumer(param);
                                            }
                                            if (string.IsNullOrEmpty(group) == false)
                                            {
                                                string username = dt.Rows[i][0].ToString();

                                                consumergroup grp = dataContext.consumergroups.Where(x => x.GroupName == group && x.MerchantId == userid && x.ConsumerUserId == username).FirstOrDefault();
                                                if (grp == null)
                                                {
                                                    grp = new consumergroup();
                                                    grp.GroupName = group;
                                                    grp.MerchantId = userid;
                                                    grp.ConsumerUserId = username;
                                                    dataContext.consumergroups.Add(grp);
                                                    dataContext.SaveChanges();
                                                }
                                            }
                                        }
                                        if (hdnCouponId != "0")
                                        {
                                            ViewBag.result = Global.Merchant.CouponSharedToCSVMessage;
                                        }
                                        else
                                        {
                                            ViewBag.result = Global.Merchant.DECSharedToCSVMessage;
                                        }

                                        return View();
                                    }
                                    else
                                    {
                                        return RedirectToAction("Login", "Account");
                                    }
                                }
                                else
                                {
                                    return RedirectToAction("Login", "Account");
                                }
                            }
                            else
                            {

                                return RedirectToAction("Login", "Account");
                            }
                        }
                        else
                        {

                            return RedirectToAction("Login", "Account");
                        }
                    }
                }
                else
                {

                    return RedirectToAction("Login", "Account");
                }
            }
            catch (Exception ex)
            {
                EventLog.LogErrorData("Error occurred in Merchant/UploadConsumerFile: " + ex.Message, true);
                return RedirectToAction("Login", "Account");
            }

        }

        public ActionResult Download()
        {
            try
            {
                using (MerchantEntities dataContext = new MerchantEntities())
                {
                    string userid = Session["UserId"].ToString();
                    merchant_master master = dataContext.merchant_master.Where(x => x.UserId == userid).FirstOrDefault();
                    string templatefor = "Menu";
                    if (master != null)
                    {
                        if (master.Category == 5)
                        {
                            templatefor = "Spa";
                        }
                    }
                    menu_csv_template template = dataContext.menu_csv_template.Where(x => x.templatefor == templatefor).FirstOrDefault();
                    if (template != null)
                    {
                        downloadCSV(template);
                        ViewBag.Result = "Menu template downloaded successfully.";
                        return RedirectToAction("Download", "Merchant");
                    }
                    else
                    {
                        return RedirectToAction("Login", "Account");
                    }
                }
            }
            catch (Exception ex)
            {
                EventLog.LogErrorData("Error occurred in Merchant/Download: " + ex.Message, true);
                return RedirectToAction("Login", "Account");
            }

        }

        public ActionResult DownloadConsumerSample()
        {
            try
            {
                using (MerchantEntities dataContext = new MerchantEntities())
                {
                    menu_csv_template template = dataContext.menu_csv_template.Where(x => x.templatefor == "Consumer").FirstOrDefault();
                    if (template != null)
                    {
                        downloadCSV(template);
                        ViewBag.Result = Global.Merchant.DownloadCSVMessage;
                        return RedirectToAction("DownloadConsumerSample", "Merchant");
                    }
                    else
                    {
                        return RedirectToAction("Login", "Account");
                    }
                }
            }
            catch (Exception ex)
            {
                EventLog.LogErrorData("Error occurred in Merchant/DownloadConsumerSample: " + ex.Message, true);
                return RedirectToAction("Login", "Account");
            }

        }

        private void downloadCSV(menu_csv_template template)
        {
            Byte[] bytes = (Byte[])template.csv_template;
            Response.Buffer = true;
            Response.BufferOutput = true;
            Response.Charset = "";
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.ContentType = template.ContentType;
            Response.AddHeader("content-disposition", "attachment;filename=" + template.FileName);
            Response.BinaryWrite(bytes);
            Response.Flush();
            Response.End();
        }

        public ActionResult UploadMenu()
        {
            return View();
        }

        public ActionResult UploadConsumers(int CouponId = 0)
        {
            ViewBag.CouponId = CouponId;
            return View();
        }

        // GET: Merchant
        public ActionResult Index()
        {
            var roles = ((ClaimsIdentity)User.Identity).Claims
              .Where(c => c.Type == ClaimTypes.Role).FirstOrDefault();

            string isCouponCreationAllowed = "true";
            string isEventCouponAllowed = "true";

            if (roles.Value == "Staff")
            {
                isCouponCreationAllowed = "false";
                isEventCouponAllowed = "false";
            }
            else if (roles.Value == "LocationManager")
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
                            if (locmgr.IsCouponAllowed == false)
                            {
                                isCouponCreationAllowed = "false";
                            }
                            if (locmgr.IsEventCouponsAllowed == false)
                            {
                                isEventCouponAllowed = "false";
                            }
                        }
                    }
                }
            }

            ViewBag.isCouponCreationAllowed = isCouponCreationAllowed;
            ViewBag.isEventCouponAllowed = isEventCouponAllowed;

            return View();
        }

        public ActionResult CreateReview()
        {
            return View();
        }

        public ActionResult ChangeCountry()
        {
            string userid = Session["UserId"].ToString();
            MerchantEntities dataContext = new MerchantEntities();
            merchant_master master = dataContext.merchant_master.Where(x => x.UserId == userid).FirstOrDefault();
            ViewBag.countryid = master.Country;

            ViewBag.languageid = master.LanguageId;

            return View();
        }

        public ActionResult SendDec()
        {
            string userid = Session["UserId"].ToString();
            using (MerchantEntities dataContext = new MerchantEntities())
            {
                merchant_master master = dataContext.merchant_master.Where(x => x.UserId == userid).FirstOrDefault();
                ViewBag.countryid = master.Country;
            }

            return View();
        }

        public ActionResult SendCoupon(string CouponId)
        {
            TempData["Id"] = CouponId;
            ViewBag.CouponId = CouponId;
            string userid = Session["UserId"].ToString();
            using (MerchantEntities dataContext = new MerchantEntities())
            {
                merchant_master master = dataContext.merchant_master.Where(x => x.UserId == userid).FirstOrDefault();
                ViewBag.countryid = master.Country;
            }

            return View();
        }

        public ActionResult EventCoupons()
        {
            return View();
        }

        public ActionResult CouponList()
        {
            var roles = ((ClaimsIdentity)User.Identity).Claims
                .Where(c => c.Type == ClaimTypes.Role).FirstOrDefault();

            string isCouponSharingAllowed = "";
            if (roles.Value == "Staff")
            {
                using (MerchantEntities dataContext = new MerchantEntities())
                {
                    string userid = Session["UserId"].ToString();
                    merchant_master master = dataContext.merchant_master.Where(x => x.UserId == userid).FirstOrDefault();
                    if (master != null)
                    {
                        staff_master staff = dataContext.staff_master.Where(x => x.StaffMasterId == master.merchantid).FirstOrDefault();
                        if (staff != null)
                        {
                            if (staff.IsCouponSendAllowed == false)
                            {
                                isCouponSharingAllowed = "false";
                            }
                        }
                    }
                }
            }
            ViewBag.isCouponSharingAllowed = isCouponSharingAllowed;

            return View();
        }

        public ActionResult ScanQRCode()
        {
            string userid = Session["UserId"].ToString();
            using (MerchantEntities dataContext = new MerchantEntities())
            {
                merchant_master master = dataContext.merchant_master.Where(x => x.UserId == userid).FirstOrDefault();
                ViewBag.countryid = master.Country;
            }

            return View();

        }

        public ActionResult MyProfile(string flag)
        {
            using (MerchantEntities dataContext = new MerchantEntities())
            {
                string userid = Session["UserId"].ToString();
                merchant_master master = dataContext.merchant_master.Where(x => x.UserId == userid).FirstOrDefault();
                if (master != null)
                {
                    ViewBag.MerchantId = master.merchantid;
                    ViewBag.Images = Directory.EnumerateFiles(Server.MapPath("~/Images/BackgroundImages")).Select(fn => "~/Images/BackgroundImages/" + Path.GetFileName(fn));

                    ViewBag.Flag = flag;
                    return View();
                }
                else
                {
                    return RedirectToAction("Login", "Account");
                }

            }

        }

        public JsonResult getBrands()
        {
            try
            {
                using (MerchantEntities dataContext = new MerchantEntities())
                {
                    string userid = Session["UserId"].ToString();
                    if (User.IsInRole("LocationManager"))
                    {
                        merchant_master locmaster = dataContext.merchant_master.Where(x => x.UserId == userid).FirstOrDefault();
                        branch_master bnchmgr = dataContext.branch_master.Where(x => x.BranchManagerId == locmaster.merchantid).FirstOrDefault();
                        merchant_master master = dataContext.merchant_master.Where(x => x.merchantid == bnchmgr.MerchantId).FirstOrDefault();
                        brand_master brand = dataContext.brand_master.Where(x => x.BrandId == bnchmgr.BrandId).FirstOrDefault();
                        if (brand != null && bnchmgr != null)
                        {
                            List<brand_master> brandList = new List<brand_master>();
                            brand.branches = new List<branch_master>();

                            brand.branches.Add(bnchmgr);

                            brandList.Add(brand);

                            return Json(brandList, JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            return Json(Global.Merchant.MerchantErrorMessage, JsonRequestBehavior.AllowGet);
                        }
                    }
                    else if (User.IsInRole("BrandManager") && !User.IsInRole("Merchant"))
                    {
                        merchant_master locmaster = dataContext.merchant_master.Where(x => x.UserId == userid).FirstOrDefault();
                        brand_master brand = dataContext.brand_master.Where(x => x.BrandManagerId == locmaster.merchantid).FirstOrDefault();

                        if (brand != null)
                        {
                            List<brand_master> brandList = new List<brand_master>();

                            brand.branches = dataContext.branch_master.Where(x => x.BrandId == brand.BrandId).ToList();


                            brandList.Add(brand);

                            return Json(brandList, JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            return Json(Global.Merchant.MerchantErrorMessage, JsonRequestBehavior.AllowGet);
                        }
                    }
                    else
                    {
                        merchant_master master = dataContext.merchant_master.Where(x => x.UserId == userid).FirstOrDefault();
                        if (master != null)
                        {
                            List<brand_master> brandList = dataContext.brand_master.Where(x => x.MerchantId == master.merchantid).OrderBy(x => x.BrandName).ToList();
                            foreach (brand_master brand in brandList)
                            {
                                List<branch_master> branches = dataContext.branch_master.Where(x => x.MerchantId == master.merchantid && x.BrandId == brand.BrandId).ToList();

                                brand.branches = branches;
                            }


                            return Json(brandList, JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            return Json(Global.Merchant.MerchantErrorMessage, JsonRequestBehavior.AllowGet);
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                //call error view here
                //log the error
                EventLog.LogErrorData("Error occurred Merchant/getBrands." + ex.Message, true);
                return Json("Error occurred while retrieving brand list", JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult GetCategories()
        {
            try
            {
                using (MerchantEntities dataContext = new MerchantEntities())
                {
                    var categoryList = dataContext.business_category_master.OrderBy(x => x.CategoryName).ToList();

                    return Json(categoryList, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                //call error view here
                //log the error
                EventLog.LogErrorData("Error occurred Merchant/GetCategories." + ex.Message, true);
                return Json(Global.Merchant.CategoryListErrorMessage, JsonRequestBehavior.AllowGet);
            }
        }

        public string getCurrency()
        {
            try
            {
                using (MerchantEntities dataContext = new MerchantEntities())
                {
                    string userid = Session["UserId"].ToString();
                    merchant_master master = dataContext.merchant_master.Where(x => x.UserId == userid).FirstOrDefault();
                    if (master != null)
                    {
                        country_master ctry = dataContext.country_master.Where(x => x.countryid == master.Country).FirstOrDefault();
                        if (ctry != null)
                        {
                            return ctry.currency;
                        }
                        else
                        {
                            EventLog.LogErrorData("There is some mismatch between country set for merchant and country master. Setting Rs as currency instead.", true);
                            country_master ctry1 = dataContext.country_master.FirstOrDefault();
                            return ctry1.currency;
                        }
                    }
                    else
                    {
                        EventLog.LogErrorData("Merchant login details not found. Setting Rs as currency instead.", true);
                        country_master ctry1 = dataContext.country_master.FirstOrDefault();
                        return ctry1.currency;
                    }
                }
            }
            catch (Exception ex)
            {
                //call error view here
                //log the error
                EventLog.LogErrorData("Error occurred Merchant/GetCurrency." + ex.Message, true);
                return Global.Merchant.CurrencyErrorMessage;
            }
        }

        public class BrandDetails
        {
            public int NoOfLicenses { get; set; }
            public int AvailableLicenses { get; set; }
            public List<brand_master> brands { get; set; }
        }
        public JsonResult GetLicenses()
        {
            try
            {
                using (MerchantEntities dataContext = new MerchantEntities())
                {
                    string userid = Session["UserId"].ToString();
                    brand_master brand = new brand_master();

                    if (User.IsInRole("LocationManager") && !User.IsInRole("BrandManager"))
                    {
                        merchant_master branchmaster = dataContext.merchant_master.Where(x => x.UserId == userid).FirstOrDefault();
                        branch_master branch = dataContext.branch_master.Where(x => x.BranchManagerId == branchmaster.merchantid).FirstOrDefault();
                        branch.staffs = dataContext.staff_master.Where(x => x.BranchId == branch.BranchId).ToList();

                        merchant_master owner = dataContext.merchant_master.Where(x => x.merchantid == branch.MerchantId).FirstOrDefault();
                        userid = owner.UserId;
                        brand = dataContext.brand_master.Where(x => x.BrandId == branch.BrandId).FirstOrDefault();
                        brand.branches = new List<branch_master>();

                        brand.branches.Add(branch);
                        BrandDetails bnd = new BrandDetails();
                        bnd.NoOfLicenses = 0;
                        bnd.AvailableLicenses = 0;
                        bnd.brands = new List<brand_master>();
                        bnd.brands.Add(brand);

                        var jsonResult = Json(bnd, JsonRequestBehavior.AllowGet);
                        jsonResult.MaxJsonLength = Int32.MaxValue;

                        return jsonResult;

                    }
                    else if (User.IsInRole("BrandManager") && !User.IsInRole("Merchant"))
                    {
                        merchant_master brandmaster = dataContext.merchant_master.Where(x => x.UserId == userid).FirstOrDefault();
                        brand = dataContext.brand_master.Where(x => x.BrandManagerId == brandmaster.merchantid).FirstOrDefault();
                        merchant_master owner = dataContext.merchant_master.Where(x => x.merchantid == brand.MerchantId).FirstOrDefault();
                        userid = owner.UserId;
                    }

                    merchant_master master = dataContext.merchant_master.Where(x => x.UserId == userid).FirstOrDefault();
                    if (master != null)
                    {
                        BrandDetails bnd = new BrandDetails();
                        bnd.NoOfLicenses = Convert.ToInt32(master.NoOfLocations);
                        if (User.IsInRole("BrandManager") && !User.IsInRole("Merchant"))
                        {
                            brand.branches = dataContext.branch_master.Where(x => x.MerchantId == brand.BrandManagerId && x.BrandId == brand.BrandId).ToList();

                            bnd.brands = new List<brand_master>();
                            bnd.brands.Add(brand);

                            List<brand_master> brands = dataContext.brand_master.Where(x => x.MerchantId == master.merchantid).ToList();
                            int usedlicenses = 0;
                            foreach (brand_master b in brands)
                            {
                                List<branch_master> Allbranches = dataContext.branch_master.Where(x => x.BrandId == b.BrandId).ToList();

                                if (Allbranches.Count() > 0)
                                {
                                    usedlicenses += Allbranches.Count;
                                }

                                // b.branches = branches;
                            }
                            bnd.AvailableLicenses = bnd.NoOfLicenses - usedlicenses;
                        }
                        else
                        {
                            List<brand_master> brands = dataContext.brand_master.Where(x => x.MerchantId == master.merchantid).ToList();
                            int usedlicenses = 0;
                            foreach (brand_master b in brands)
                            {
                                List<branch_master> branches = dataContext.branch_master.Where(x => x.BrandId == b.BrandId).ToList();

                                if (branches.Count() > 0)
                                {
                                    usedlicenses += branches.Count;
                                }
                                b.branches = branches;
                            }

                            bnd.AvailableLicenses = bnd.NoOfLicenses - usedlicenses;
                            bnd.brands = brands;
                        }



                        var jsonResult = Json(bnd, JsonRequestBehavior.AllowGet);
                        jsonResult.MaxJsonLength = Int32.MaxValue;

                        return jsonResult;
                    }
                    else
                    {
                        return Json(Global.Merchant.MerchantErrorMessage, JsonRequestBehavior.AllowGet);
                    }
                }
            }
            catch (Exception ex)
            {
                EventLog.LogErrorData("Error occurred Merchant/GetLicenses." + ex.InnerException.Message, true);
                return Json(Global.Merchant.MerchantLicenseException, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult GetMerchantById(string MerchantId)
        {

            try
            {
                using (MerchantEntities dataContext = new MerchantEntities())
                {
                    int no = Convert.ToInt32(MerchantId);
                    var merchant = dataContext.merchant_master.Find(no);
                    // user muser = dataContext.users.Where(x => x.Id == merchant.UserId).FirstOrDefault();

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

                    if (merchant.RunRewardProgram == true)
                    {
                        rewardmaster rwd = dataContext.rewardmasters.Where(x => x.MerchantId == merchant.UserId).OrderByDescending(x => x.CreationDate).FirstOrDefault();
                        if (rwd != null)
                        {
                            merchant.RewardPoints = rwd.RewardPoints;
                            merchant.RewardRs = rwd.RewardRs;
                            merchant.RewardName = rwd.RewardName;
                        }

                        if (merchant.RedeemProgram == "Options")
                        {
                            redeemoption opt = dataContext.redeemoptions.Where(x => x.MerchantId == merchant.UserId).OrderByDescending(x => x.CreationDate).FirstOrDefault();
                            merchant.redeemoptions = opt;
                        }
                        else if (merchant.RedeemProgram == "Points")
                        {
                            redeemmaster rdm = dataContext.redeemmasters.Where(x => x.MerchantId == merchant.UserId).OrderByDescending(x => x.CreationDate).FirstOrDefault();
                            if (rdm != null)
                            {
                                merchant.RedeemPt = rdm.RedeemPt;
                                merchant.RedeemRs = rdm.RedeemRs;
                            }
                        }
                        else if (merchant.RedeemProgram == "Cashback")
                        {
                            cashbackdetail cash = dataContext.cashbackdetails.Where(x => x.MerchantId == merchant.UserId).OrderByDescending(x => x.CreationDate).FirstOrDefault();
                            merchant.cashbackdetails = cash;
                        }

                        merchant_benefits ben = dataContext.merchant_benefits.Where(x => x.MerchantId == merchant.UserId).OrderByDescending(x => x.CreationDate).FirstOrDefault();
                        if (ben != null)
                        {
                            merchant.benefits = ben;
                        }

                        pointscashbackexpiry exp = dataContext.pointscashbackexpiries.Where(x => x.MerchantId == merchant.UserId).OrderByDescending(x => x.CreationDate).FirstOrDefault();
                        if (exp != null)
                        {
                            merchant.exp = exp;
                        }

                    }

                    var jsonResult = Json(merchant, JsonRequestBehavior.AllowGet);
                    jsonResult.MaxJsonLength = Int32.MaxValue;

                    return jsonResult;
                    //return Json(merchant, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                //call error view here
                //log the error
                EventLog.LogErrorData("Error occurred Merchant/GetMerchantById." + ex.Message, true);
                return Json(Global.Merchant.MerchantErrorMessage, JsonRequestBehavior.AllowGet);
            }

        }

        public JsonResult getMerchantRewards(string MerchantId)
        {

            try
            {
                using (MerchantEntities dataContext = new MerchantEntities())
                {
                    var merchantRewards = dataContext.rewardmasters.Where(x => x.MerchantId == MerchantId).OrderByDescending(x => x.CreationDate).FirstOrDefault();
                    return Json(merchantRewards, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                //call error view here
                //log the error
                EventLog.LogErrorData("Error occurred Merchant/getMerchantRewards." + ex.Message, true);
                return Json(Global.Merchant.MerchantRewardErrorMessage, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult getMerchantRedeems(string MerchantId)
        {
            try
            {
                using (MerchantEntities dataContext = new MerchantEntities())
                {
                    var merchantRedeem = dataContext.redeemmasters.Where(x => x.MerchantId == MerchantId).OrderByDescending(x => x.CreationDate).FirstOrDefault();
                    return Json(merchantRedeem, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                //call error view here
                //log the error
                EventLog.LogErrorData("Error occurred Merchant/getMerchantRedeems." + ex.Message, true);
                return Json(Global.Merchant.MerchantRedeemErrorMessage, JsonRequestBehavior.AllowGet);
            }
        }


        public ActionResult GetCouponFromCouponCode(string couponcode)
        {
            ViewBag.couponcode = couponcode;
            return View();
        }

        public JsonResult GetCountrycode()
        {
            try
            {
                using (MerchantEntities dataContext = new MerchantEntities())
                {
                    var UserId = User.Identity.GetUserId();
                    var UsercountryId = dataContext.merchant_master.Where(x => x.UserId == UserId).Select(x => x.Country).FirstOrDefault();

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
                EventLog.LogErrorData("Error occurred Merchant/GetCountryCode." + ex.Message, true);
                return Json(Global.Merchant.CountryCodeErrorMessage, JsonRequestBehavior.AllowGet);
            }
        }

        public string RedeemPoints(string RedeemPoints, string CustPhoneNumber)
        {
            try
            {
                string userid = Session["UserId"].ToString();
                int pointstoredeem = Convert.ToInt32(RedeemPoints);

                if (string.IsNullOrEmpty(RedeemPoints) == false && string.IsNullOrEmpty(CustPhoneNumber) == false)
                {
                    string consumeruserid = "";

                    using (instadelight_consumerEntities ConsumerdataContext = new instadelight_consumerEntities())
                    {
                        var isUser = ConsumerdataContext.consumerusers.Where(u => u.UserName == CustPhoneNumber).FirstOrDefault();
                        if (isUser != null)
                        {
                            consumeruserid = isUser.Id;
                        }
                        else
                        {
                            return Global.Merchant.ConsumerDoesNotExistErrorMessage;
                        }
                    }

                    using (MerchantEntities dataContext = new MerchantEntities())
                    {
                        if (User.IsInRole("Staff"))
                        {
                            merchant_master staffmaster = dataContext.merchant_master.Where(x => x.UserId == userid).FirstOrDefault();
                            staff_master stfmgr = dataContext.staff_master.Where(x => x.StaffMasterId == staffmaster.merchantid).FirstOrDefault();
                            merchant_master owner = dataContext.merchant_master.Where(x => x.merchantid == stfmgr.MerchantId).FirstOrDefault();
                            userid = owner.UserId;
                        }
                        else if (User.IsInRole("LocationManager"))
                        {
                            merchant_master branchmaster = dataContext.merchant_master.Where(x => x.UserId == userid).FirstOrDefault();
                            branch_master branch = dataContext.branch_master.Where(x => x.BranchManagerId == branchmaster.merchantid).FirstOrDefault();
                            merchant_master owner = dataContext.merchant_master.Where(x => x.merchantid == branch.MerchantId).FirstOrDefault();
                            userid = owner.UserId;
                        }


                        merchant_master master = dataContext.merchant_master.Where(x => x.UserId == userid).FirstOrDefault();
                        country_master country = dataContext.country_master.Where(x => x.countryid == master.Country).FirstOrDefault();

                        merchantconsumerdetail det = dataContext.merchantconsumerdetails.Where(x => x.ConsumerId == consumeruserid).FirstOrDefault();
                        if (det == null)
                        {
                            return Global.Merchant.Consumer + CustPhoneNumber + Global.Merchant.NotRegisteredErrorMessage + Session["UserName"].ToString();
                        }

                        if (Convert.ToBoolean(master.RunRewardProgram) == true)
                        {
                            if (master.RedeemProgram == "Points")
                            {
                                redeemmaster red = dataContext.redeemmasters.Where(x => x.MerchantId == userid).FirstOrDefault();
                                if (red == null)
                                {
                                    return Global.Merchant.NoRedeemFormulaErrorMessage + Session["UserName"].ToString();
                                }

                                using (instadelight_consumerEntities ConsumerdataContext = new instadelight_consumerEntities())
                                {
                                    int pts = Convert.ToInt32(ConsumerdataContext.consumerrewardpoints.Where(x => x.ConsumerId == consumeruserid && x.MerchantId == userid).Select(x => x.Points).Sum());
                                    int redeempts = Convert.ToInt32(ConsumerdataContext.consumerredeemdetails.Where(x => x.ConsumerId == consumeruserid && x.MerchantId == userid).Select(x => x.PointsRedeemed).Sum());
                                    int availablepts = pts - redeempts;


                                    if (availablepts == 0)
                                    {
                                        return Global.Merchant.Consumer + CustPhoneNumber + Global.Merchant.RewardFormulaErrorMessage + Session["UserName"].ToString();
                                    }
                                    else
                                    {
                                        if (availablepts < pointstoredeem)
                                        {
                                            return Global.Merchant.NoPointsAvailableErrorMessage.Replace("xyz", CustPhoneNumber).Replace("nnn", pts.ToString()) + " " + Global.Merchant.NoPointsAvailableErrorMessage1;
                                        }
                                    }

                                    int actualdiscount = (pointstoredeem * Convert.ToInt32(red.RedeemRs)) / Convert.ToInt32(red.RedeemPt);


                                    string msg = Global.Merchant.RedeemPointsConfirmationMessage.Replace("nnn", RedeemPoints.ToString()).Replace("ppp", actualdiscount.ToString()).Replace("Currency", country.currency);
                                    return msg;
                                }
                            }
                            else if (master.RedeemProgram == "Options")
                            {
                                redeemoption options = dataContext.redeemoptions.Where(x => x.MerchantId == userid).OrderByDescending(x => x.CreationDate).FirstOrDefault();
                                if (options == null)
                                {
                                    return Global.Merchant.NoPointsDefinedErrorMessage.Replace("xyz", Session["UserName"].ToString());
                                }

                                string msg = Global.Merchant.RedeemOptionsMessage.Replace("nnn", RedeemPoints.ToString());
                                if (options.Option1 != null)
                                {
                                    msg = msg + " " + options.Option1;
                                }
                                if (options.Option2 != null)
                                {
                                    msg = msg + " " + options.Option2;
                                }
                                if (options.Option3 != null)
                                {
                                    msg = msg + " " + options.Option3;
                                }
                                if (options.Option4 != null)
                                {
                                    msg = msg + " " + options.Option4;
                                }
                                if (options.Option5 != null)
                                {
                                    msg = msg + " " + options.Option5;
                                }
                                return msg;
                            }
                            else if (master.RedeemProgram == "Cashback")
                            {
                                cashbackdetail red = dataContext.cashbackdetails.Where(x => x.MerchantId == userid).FirstOrDefault();
                                if (red == null)
                                {
                                    return Global.Merchant.NoRedeemFormulaErrorMessage + Session["UserName"].ToString();
                                }

                                using (instadelight_consumerEntities ConsumerdataContext = new instadelight_consumerEntities())
                                {
                                    int pts = Convert.ToInt32(ConsumerdataContext.consumerrewardpoints.Where(x => x.ConsumerId == consumeruserid && x.MerchantId == userid).Select(x => x.Points).Sum());
                                    int redeempts = Convert.ToInt32(ConsumerdataContext.consumerredeemdetails.Where(x => x.ConsumerId == consumeruserid && x.MerchantId == userid).Select(x => x.PointsRedeemed).Sum());
                                    int availablepts = pts - redeempts;


                                    if (availablepts == 0)
                                    {
                                        return Global.Merchant.Consumer + CustPhoneNumber + Global.Merchant.RewardFormulaErrorMessage + Session["UserName"].ToString();
                                    }
                                    else
                                    {
                                        if (availablepts < pointstoredeem)
                                        {
                                            return Global.Merchant.NoPointsAvailableErrorMessage.Replace("xyz", CustPhoneNumber).Replace("nnn", pts.ToString()) + " " + Global.Merchant.NoPointsAvailableErrorMessage1;
                                        }
                                    }

                                    int actualdiscount = (pointstoredeem * Convert.ToInt32(red.RedeemRs)) / Convert.ToInt32(red.RedeemPoint);


                                    string msg = Global.Merchant.RedeemPointsConfirmationMessage.Replace("nnn", RedeemPoints.ToString()).Replace("ppp", actualdiscount.ToString()).Replace("Currency", country.currency);
                                    return msg;
                                }
                            }
                            else
                            {
                                return "Merchant " + Session["UserName"].ToString() + " has not defined any reward program";
                            }
                        }
                        else
                        {
                            return "Merchant " + Session["UserName"].ToString() + " has not defined any reward program";
                        }
                    }

                }
                else
                {
                    return Global.Merchant.InvalidpointsDetails;
                }
            }
            catch (Exception ex)
            {
                //call error view here
                //log the error
                EventLog.LogErrorData("Error occurred Merchant/RedeemCoupon." + ex.Message, true);
                return Global.Merchant.RedeemCouponException;
            }
        }

        public string RedeemCoupon(string couponcode, string billedamount, string CustPhoneNumber)
        {
            try
            {
                string userid = Session["UserId"].ToString();

                if (string.IsNullOrEmpty(couponcode) == false && string.IsNullOrEmpty(billedamount) == false && string.IsNullOrEmpty(CustPhoneNumber) == false)
                {
                    string consumeruserid = "";

                    using (instadelight_consumerEntities ConsumerdataContext = new instadelight_consumerEntities())
                    {
                        var isUser = ConsumerdataContext.consumerusers.Where(u => u.UserName == CustPhoneNumber).FirstOrDefault();
                        if (isUser != null)
                        {
                            consumeruserid = isUser.Id;
                        }
                        else
                        {
                            return Global.Merchant.ConsumerDoesNotExistErrorMessage;
                        }
                    }

                    using (MerchantEntities dataContext = new MerchantEntities())
                    {
                        merchantconsumerdetail det = dataContext.merchantconsumerdetails.Where(x => x.ConsumerId == consumeruserid).FirstOrDefault();
                        if (det == null)
                        {
                            return Global.Merchant.Consumer + CustPhoneNumber + Global.Merchant.NotRegisteredErrorMessage + Session["UserName"].ToString();
                        }

                        coupons_master cpn = dataContext.coupons_master.Where(x => x.CouponCode == couponcode).FirstOrDefault();
                        if (cpn != null)
                        {
                            merchantconsumercoupondetail sharedcoupon = dataContext.merchantconsumercoupondetails.Where(s => s.MerchantId == userid && s.ConsumerId == consumeruserid && s.CouponId == cpn.couponid).FirstOrDefault();
                            if (sharedcoupon == null)
                            {
                                return Global.Merchant.InvalidConsumerCouponErrorMessage.Replace("xyx", CustPhoneNumber).Replace("pqr", Session["UserName"].ToString());
                            }


                            decimal billedamt = Convert.ToDecimal(billedamount);
                            //coupon_redeem_details redeem = new coupon_redeem_details();
                            //redeem.couponid = cpn.couponid;
                            //redeem.couponcode = cpn.CouponCode;
                            //redeem.redeemedon = DateTime.Now;
                            //redeem.MerchantId = cpn.MerchantId;



                            var mer = dataContext.merchant_master.Where(x => x.merchantid == cpn.MerchantId && x.UserId == userid).ToList();

                            if (mer == null)
                            {
                                return Global.Merchant.InvalidMerchantCouponErrorMessage.Replace("xyz", Session["UserName"].ToString());
                            }
                            else if (mer.Count == 0)
                            {
                                return Global.Merchant.InvalidMerchantCouponErrorMessage.Replace("xyz", Session["UserName"].ToString());
                            }


                            if (cpn.ValidFrom != null)
                            {
                                if (cpn.ValidFrom < DateTime.Now)
                                {
                                    if (cpn.ValidTill != null)
                                    {
                                        if (cpn.ValidTill < DateTime.Now)
                                        {
                                            return Global.Merchant.CouponExpiredErrorMessage;
                                        }
                                    }
                                }
                            }

                            if (cpn.ValidTill != null)
                            {
                                if (cpn.ValidTill < DateTime.Now)
                                {
                                    return Global.Merchant.CouponExpiredErrorMessage;
                                }
                            }




                            coupon_redeem_details checkredeem = dataContext.coupon_redeem_details.Where(x => x.couponcode == cpn.CouponCode && x.merchantid == cpn.MerchantId && x.ConsumerId == consumeruserid).FirstOrDefault();
                            if (checkredeem != null)
                            {
                                return Global.Merchant.AlreadyRedeemedErrorMessage.Replace("xyz", couponcode).Replace("pqr", CustPhoneNumber).Replace("dt", Convert.ToDateTime(checkredeem.redeemedon).ToString("dd-MMM-yyyy"));
                            }

                            if (cpn.AboveAmount > billedamt)
                            {
                                return Global.Merchant.MaxAmountErrorMessage.Replace("nnn", cpn.AboveAmount.ToString());
                            }
                            if (cpn.PercentageOff != null)
                            {
                                if (cpn.PercentageOff != 0)
                                {
                                    decimal actualdiscount = (billedamt * Convert.ToDecimal(cpn.PercentageOff)) / 100;

                                    if (cpn.MaxDiscount != null)
                                    {
                                        if (cpn.MaxDiscount != 0)
                                        {
                                            if (actualdiscount > cpn.MaxDiscount)
                                            {
                                                return Global.Merchant.ValidCouponMessage1.Replace("nnn", cpn.MaxDiscount.ToString());
                                            }
                                        }
                                    }

                                }
                            }
                            country_master ctry = dataContext.country_master.FirstOrDefault();

                            //dataContext.coupon_redeem_details.Add(redeem);
                            //dataContext.SaveChanges();

                            string msg = Global.Merchant.CouponIsValid;
                            if (cpn.Discount != null)
                            {
                                if (cpn.Discount != 0)
                                {
                                    msg += Global.Merchant.ValidCouponMessage2.Replace("Currency", ctry.currency).Replace("nnn", cpn.Discount.ToString());
                                }

                            }
                            if (cpn.PercentageOff != null)
                            {
                                if (cpn.PercentageOff != 0)
                                {
                                    msg += Global.Merchant.ValidCouponMessage3.Replace("nnn", cpn.PercentageOff.ToString());
                                }
                            }
                            return msg;
                        }
                        else
                        {
                            return Global.Merchant.InvalidCouponDetails;
                        }
                    }

                }
                else
                {
                    return Global.Merchant.InvalidCouponDetails;
                }
            }
            catch (Exception ex)
            {
                //call error view here
                //log the error
                EventLog.LogErrorData("Error occurred Merchant/RedeemCoupon." + ex.Message, true);
                return Global.Merchant.RedeemCouponException;
            }
        }

        public string RedeemCouponFromCode(string couponcode)
        {
            try
            {
                if (string.IsNullOrEmpty(couponcode) == false)
                {
                    using (MerchantEntities dataContext = new MerchantEntities())
                    {
                        coupons_master cpn = dataContext.coupons_master.Where(x => x.CouponCode == couponcode).FirstOrDefault();
                        if (cpn != null)
                        {
                            string userid = Session["UserId"].ToString();

                            var mer = dataContext.merchant_master.Where(x => x.merchantid == cpn.MerchantId && x.UserId == userid).ToList();

                            if (mer != null)
                            {
                                if (mer.Count > 0)
                                {

                                }
                                else
                                {
                                    return Global.Merchant.InvalidMerchantCouponErrorMessage.Replace("xyz", Session["UserName"].ToString());
                                }
                            }
                            else
                            {
                                return Global.Merchant.InvalidMerchantCouponErrorMessage.Replace("xyz", Session["UserName"].ToString());
                            }



                            if (cpn.ValidFrom != null)
                            {
                                if (cpn.ValidFrom < DateTime.Now)
                                {
                                    if (cpn.ValidTill != null)
                                    {
                                        if (cpn.ValidTill < DateTime.Now)
                                        {
                                            return Global.Merchant.CouponExpiredErrorMessage;
                                        }
                                    }
                                }
                            }

                            if (cpn.ValidTill != null)
                            {
                                if (cpn.ValidTill < DateTime.Now)
                                {
                                    return Global.Merchant.CouponExpiredErrorMessage;
                                }
                            }

                            return Global.Merchant.CouponIsValid;
                        }
                        else
                        {
                            return Global.Merchant.InvalidCouponDetails;
                        }
                    }

                }
                else
                {
                    return Global.Merchant.InvalidCouponDetails;
                }
            }
            catch (Exception ex)
            {
                //call error view here
                //log the error
                EventLog.LogErrorData("Error occurred Merchant/RedeemCouponFromCode." + ex.Message, true);
                return Global.Merchant.RedeemCouponException;
            }
        }


        public ActionResult AddCoupon(string Flag, int couponid)
        {
            ViewBag.Flag = Flag;
            ViewBag.BankId = couponid;
            return View("AddCoupon");
        }

        public JsonResult GetAllGroups()
        {
            try
            {
                using (MerchantEntities dataContext = new MerchantEntities())
                {
                    string userid = Session["UserId"].ToString();
                    merchant_master master = dataContext.merchant_master.Where(x => x.UserId == userid).FirstOrDefault();
                    if (master != null)
                    {
                        var grouplist = dataContext.consumergroups.Where(x => x.MerchantId == userid).Select(x => new { GroupName = x.GroupName }).Distinct().ToList();


                        var jsonResult = Json(grouplist, JsonRequestBehavior.AllowGet);
                        jsonResult.ContentType = "application/json";
                        jsonResult.MaxJsonLength = Int32.MaxValue;

                        return jsonResult;
                    }
                    else
                    {
                        return Json(Global.Merchant.InvalidMerchantDetails, JsonRequestBehavior.AllowGet);
                    }
                }
            }
            catch (Exception ex)
            {
                //call error view here
                //log the error
                EventLog.LogErrorData("Error occurred Merchant/GetAllGroups." + ex.Message, true);
                return Json(Global.Merchant.GetAllGroupsException, JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult GetAllCoupons()
        {
            try
            {
                using (MerchantEntities dataContext = new MerchantEntities())
                {
                    string userid = Session["UserId"].ToString();
                    merchant_master master = dataContext.merchant_master.Where(x => x.UserId == userid).FirstOrDefault();
                    if (User.IsInRole("LocationManager"))
                    {
                        branch_master branch = dataContext.branch_master.Where(x => x.BranchManagerId == master.merchantid).FirstOrDefault();
                        master = dataContext.merchant_master.Where(x => x.merchantid == branch.MerchantId).FirstOrDefault();
                    }
                    else if (User.IsInRole("Staff"))
                    {
                        staff_master staff = dataContext.staff_master.Where(x => x.StaffMasterId == master.merchantid).FirstOrDefault();
                        master = dataContext.merchant_master.Where(x => x.merchantid == staff.MerchantId).FirstOrDefault();
                    }
                    if (master != null)
                    {
                        var couponList = dataContext.coupons_master.Where(x => x.MerchantId == master.merchantid).ToList();

                        // return Json(couponList, JsonRequestBehavior.AllowGet);
                        var jsonResult = Json(couponList, JsonRequestBehavior.AllowGet);
                        jsonResult.ContentType = "application/json";
                        jsonResult.MaxJsonLength = Int32.MaxValue;

                        return jsonResult;
                    }
                    else
                    {
                        return Json(Global.Merchant.InvalidMerchantDetails, JsonRequestBehavior.AllowGet);
                    }
                }
            }
            catch (Exception ex)
            {
                //call error view here
                //log the error
                EventLog.LogErrorData("Error occurred Merchant/GetAllCoupons." + ex.Message, true);
                return Json(Global.Merchant.GetAllCouponsException, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult getNoOfVisits(string ConsumerPhone)
        {
            try
            {
                //string MerchantId = TempData["TempMerchantId"].ToString();
                using (MerchantEntities dataContext = new MerchantEntities())
                {
                    string userid = Session["UserId"].ToString();
                    string ownerid = "";
                    string staffloc = "";
                    if (User.IsInRole("Staff"))
                    {
                        merchant_master master = dataContext.merchant_master.Where(x => x.UserId == userid).FirstOrDefault();
                        staff_master stfmgr = dataContext.staff_master.Where(x => x.StaffMasterId == master.merchantid).FirstOrDefault();
                        merchant_master owner = dataContext.merchant_master.Where(x => x.merchantid == stfmgr.MerchantId).FirstOrDefault();
                        ownerid = owner.UserId;
                        branch_master branch = dataContext.branch_master.Where(x => x.BranchId == stfmgr.BranchId).FirstOrDefault();
                        staffloc = branch.BranchLocation;

                    }
                    else if (User.IsInRole("LocationManager"))
                    {
                        merchant_master master = dataContext.merchant_master.Where(x => x.UserId == userid).FirstOrDefault();
                        branch_master branch = dataContext.branch_master.Where(x => x.BranchManagerId == master.merchantid).FirstOrDefault();
                        merchant_master owner = dataContext.merchant_master.Where(x => x.merchantid == branch.MerchantId).FirstOrDefault();
                        ownerid = owner.UserId;
                        staffloc = branch.BranchLocation;
                    }

                    int availablepoints = 0;
                    int redeemedpoints = 0;

                    using (instadelight_consumerEntities consumerdataContext = new instadelight_consumerEntities())
                    {
                        consumeruser con = consumerdataContext.consumerusers.Where(x => x.UserName == ConsumerPhone).FirstOrDefault();

                        if (con != null)
                        {
                            consumermaster conmaster = consumerdataContext.consumermasters.Where(x => x.UserId == con.Id).FirstOrDefault();

                            ConsumerProfile profile = new ConsumerProfile();
                            if (String.IsNullOrEmpty(conmaster.consumername) == false)
                                profile.ConsumerName = conmaster.consumername;

                            if (conmaster.DOA != null)
                                profile.DOA = Convert.ToDateTime(conmaster.DOA);

                            if (conmaster.DOB != null)
                                profile.DOB = Convert.ToDateTime(conmaster.DOB);

                            if (User.IsInRole("Staff") || User.IsInRole("LocationManager"))
                            {
                                List<consumervisitdetail> detail = consumerdataContext.consumervisitdetails.Where(x => x.ConsumerId == con.Id && x.MerchantId == ownerid).ToList();

                                if (detail != null)
                                {
                                    profile.NoOfVisits = detail.Count;
                                }

                                merchant_master merchant = dataContext.merchant_master.Where(x => x.UserId == userid).FirstOrDefault();
                                if (merchant.RunRewardProgram == true)
                                {
                                    if (merchant.RedeemProgram == "Cashback")
                                    {
                                        availablepoints = Convert.ToInt32(consumerdataContext.consumercashbackdetails.Where(x => x.ConsumerId == con.Id && x.MerchantId == ownerid && x.ExpiryDate > DateTime.Now).Select(x => x.Cashback).Sum());
                                        redeemedpoints = Convert.ToInt32(consumerdataContext.consumerredeemdetails.Where(x => x.ConsumerId == con.Id && x.MerchantId == ownerid).Select(x => x.PointsRedeemed).Sum());

                                        profile.NoOfPoints = availablepoints - redeemedpoints;
                                        profile.iscashback = true;
                                    }
                                    else
                                    {
                                        availablepoints = Convert.ToInt32(consumerdataContext.consumerrewardpoints.Where(x => x.ConsumerId == con.Id && x.MerchantId == ownerid && x.ExpiryDate > DateTime.Now).Select(x => x.Points).Sum());
                                        redeemedpoints = Convert.ToInt32(consumerdataContext.consumerredeemdetails.Where(x => x.ConsumerId == con.Id && x.MerchantId == ownerid).Select(x => x.PointsRedeemed).Sum());

                                        profile.NoOfPoints = availablepoints - redeemedpoints;
                                        profile.iscashback = false;
                                    }
                                }
                                else
                                {
                                    availablepoints = Convert.ToInt32(consumerdataContext.consumerrewardpoints.Where(x => x.ConsumerId == con.Id && x.MerchantId == ownerid && x.ExpiryDate > DateTime.Now).Select(x => x.Points).Sum());
                                    redeemedpoints = Convert.ToInt32(consumerdataContext.consumerredeemdetails.Where(x => x.ConsumerId == con.Id && x.MerchantId == ownerid).Select(x => x.PointsRedeemed).Sum());

                                    profile.NoOfPoints = availablepoints - redeemedpoints;
                                    profile.iscashback = false;
                                }
                            }
                            else
                            {
                                List<consumervisitdetail> detail = consumerdataContext.consumervisitdetails.Where(x => x.ConsumerId == con.Id && x.MerchantId == userid).ToList();
                                if (detail != null)
                                {
                                    profile.NoOfVisits = detail.Count;
                                }

                                merchant_master merchant = dataContext.merchant_master.Where(x => x.UserId == userid).FirstOrDefault();
                                if (merchant.RunRewardProgram == true)
                                {
                                    if (merchant.RedeemProgram == "Cashback")
                                    {
                                        availablepoints = Convert.ToInt32(consumerdataContext.consumercashbackdetails.Where(x => x.ConsumerId == con.Id && x.MerchantId == userid && x.ExpiryDate > DateTime.Now).Select(x => x.Cashback).Sum());
                                        redeemedpoints = Convert.ToInt32(consumerdataContext.consumerredeemdetails.Where(x => x.ConsumerId == con.Id && x.MerchantId == userid).Select(x => x.PointsRedeemed).Sum());

                                        profile.NoOfPoints = availablepoints - redeemedpoints;
                                        profile.iscashback = true;

                                    }
                                    else
                                    {
                                        availablepoints = Convert.ToInt32(consumerdataContext.consumerrewardpoints.Where(x => x.ConsumerId == con.Id && x.MerchantId == userid && x.ExpiryDate > DateTime.Now).Select(x => x.Points).Sum());
                                        redeemedpoints = Convert.ToInt32(consumerdataContext.consumerredeemdetails.Where(x => x.ConsumerId == con.Id && x.MerchantId == userid).Select(x => x.PointsRedeemed).Sum());

                                        profile.NoOfPoints = availablepoints - redeemedpoints;
                                        profile.iscashback = false;
                                    }
                                }
                                else
                                {
                                    availablepoints = Convert.ToInt32(consumerdataContext.consumerrewardpoints.Where(x => x.ConsumerId == con.Id && x.MerchantId == userid && x.ExpiryDate > DateTime.Now).Select(x => x.Points).Sum());
                                    redeemedpoints = Convert.ToInt32(consumerdataContext.consumerredeemdetails.Where(x => x.ConsumerId == con.Id && x.MerchantId == userid).Select(x => x.PointsRedeemed).Sum());

                                    profile.NoOfPoints = availablepoints - redeemedpoints;
                                    profile.iscashback = false;
                                }

                            }
                            using (MerchantEntities merchantContext = new MerchantEntities())
                            {
                                if (User.IsInRole("Staff") || User.IsInRole("LocationManager"))
                                {
                                    MySqlParameter param1 = new MySqlParameter();
                                    param1.Value = ownerid;
                                    param1.Direction = System.Data.ParameterDirection.Input;
                                    param1.ParameterName = "@MId";
                                    param1.DbType = System.Data.DbType.String;

                                    MySqlParameter param2 = new MySqlParameter();
                                    param2.Value = con.Id;
                                    param2.Direction = System.Data.ParameterDirection.Input;
                                    param2.ParameterName = "@CId";
                                    param2.DbType = System.Data.DbType.String;

                                    MySqlParameter param3 = new MySqlParameter();
                                    param3.Value = userid;
                                    param3.Direction = System.Data.ParameterDirection.Input;
                                    param3.ParameterName = "@SId";
                                    param3.DbType = System.Data.DbType.String;

                                    MySqlParameter param4 = new MySqlParameter();
                                    param4.Value = staffloc;
                                    param4.Direction = System.Data.ParameterDirection.Input;
                                    param4.ParameterName = "@SLoc";
                                    param4.DbType = System.Data.DbType.String;

                                    List<CouponList> CouponList = merchantContext.Database.SqlQuery<CouponList>("CALL GetCouponListForStaff(@MId,@CId,@SId,@SLoc)", param1, param2, param3, param4)
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
                                                        QRCode = x.QRCode,
                                                        MaxDiscount = x.MaxDiscount,
                                                        MaxCoupons = x.MaxCoupons,
                                                        ShareWithAll = x.ShareWithAll,
                                                        DateCreated = x.DateCreated
                                                    }).ToList();



                                    profile.CouponList = CouponList;
                                }
                                else
                                {
                                    MySqlParameter param1 = new MySqlParameter();
                                    param1.Value = userid;
                                    param1.Direction = System.Data.ParameterDirection.Input;
                                    param1.ParameterName = "@MId";
                                    param1.DbType = System.Data.DbType.String;
                                    MySqlParameter param2 = new MySqlParameter();
                                    param2.Value = con.Id;
                                    param2.Direction = System.Data.ParameterDirection.Input;
                                    param2.ParameterName = "@CId";
                                    param2.DbType = System.Data.DbType.String;

                                    List<CouponList> CouponList = merchantContext.Database.SqlQuery<CouponList>("CALL GetCouponListForMerchant(@MId,@CId)", param1, param2)
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
                                                        QRCode = x.QRCode,
                                                        MaxDiscount = x.MaxDiscount,
                                                        MaxCoupons = x.MaxCoupons,
                                                        ShareWithAll = x.ShareWithAll,
                                                        DateCreated = x.DateCreated
                                                    }).ToList();



                                    profile.CouponList = CouponList;
                                }

                            }

                            profile.GiftCards = consumerdataContext.consumergiftcarddetails.Where(x => x.ConsumerId == con.Id && x.MerchantId == userid && x.Status == 0 && x.ValidTill >= DateTime.Now).ToList();
                            var jsonResult = Json(profile, JsonRequestBehavior.AllowGet);
                            jsonResult.ContentType = "application/json";
                            jsonResult.MaxJsonLength = Int32.MaxValue;

                            return jsonResult;
                        }
                        else
                        {
                            EventLog.LogErrorData(Global.Merchant.Unauthorizedaccess, true);
                            return Json("0", JsonRequestBehavior.AllowGet);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                //call error view here
                //log the error
                EventLog.LogErrorData("Error occurred Merchant/getNoOfVisits." + ex.Message, true);
                return Json(Global.Merchant.NoOfVisitsException, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult GetAllEventsCoupons()
        {
            try
            {
                using (MerchantEntities dataContext = new MerchantEntities())
                {
                    string userid = Session["UserId"].ToString();
                    merchant_master master = dataContext.merchant_master.Where(x => x.UserId == userid).FirstOrDefault();
                    if (User.IsInRole("LocationManager"))
                    {
                        branch_master branch = dataContext.branch_master.Where(x => x.BranchManagerId == master.merchantid).FirstOrDefault();
                        master = dataContext.merchant_master.Where(x => x.merchantid == branch.MerchantId).FirstOrDefault();
                    }
                    if (master != null)
                    {
                        var couponList = dataContext.eventcoupondetails.Where(x => x.MerchantId == master.merchantid).ToList();


                        // return Json(couponList, JsonRequestBehavior.AllowGet);
                        var jsonResult = Json(couponList, JsonRequestBehavior.AllowGet);
                        jsonResult.ContentType = "application/json";
                        jsonResult.MaxJsonLength = Int32.MaxValue;

                        return jsonResult;
                    }
                    else
                    {
                        return Json(Global.Merchant.InvalidMerchantDetails, JsonRequestBehavior.AllowGet);
                    }
                }
            }
            catch (Exception ex)
            {
                //call error view here
                //log the error
                EventLog.LogErrorData("Error occurred Merchant/GetAllEventCoupons." + ex.Message, true);
                return Json(Global.Merchant.GetEventCouponListException, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult getLocationsForCoupons()
        {
            try
            {
                using (MerchantEntities dataContext = new MerchantEntities())
                {
                    string userid = Session["UserId"].ToString();
                    //Change this back afterwords
                    //HD
                    //7 Jun 2017

                    //var loclist = (from m in dataContext.merchant_master
                    //               join c in dataContext.city_master on m.City equals c.cityid
                    //               join l in dataContext.location_master on m.Location equals l.LocationId
                    //               where m.UserId == userid
                    //               orderby m.Location
                    //               select new
                    //               {
                    //                   label = l.Location + "-" + c.City,
                    //                   id = l.LocationId
                    //               }).Distinct().ToList();

                    var loclist = (from m in dataContext.merchant_master
                                   where m.UserId == userid
                                   orderby m.Location
                                   select new
                                   {
                                       label = m.Location + "-" + m.City,
                                       id = m.Location
                                   }).Distinct().ToList();


                    return Json(loclist, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                //call error view here
                //log the error
                EventLog.LogErrorData("Error occurred Merchant/getLocationsForCoupons." + ex.Message, true);
                return Json(Global.Merchant.GetLocationForCouponException, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult GetLocations(string cityid)
        {
            try
            {
                using (MerchantEntities dataContext = new MerchantEntities())
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
                EventLog.LogErrorData("Error occurred Merchant/GetLocations." + ex.Message, true);
                return Json(Global.Merchant.GetLocationsException, JsonRequestBehavior.AllowGet);
            }
        }

        public string UpdateBrand(brand_master brand)
        {
            try
            {
                if (brand != null)
                {
                    using (MerchantEntities dataContext = new MerchantEntities())
                    {
                        string userid = Session["UserId"].ToString();
                        merchant_master master = dataContext.merchant_master.Where(x => x.UserId == userid).FirstOrDefault();

                        int no = Convert.ToInt32(brand.BrandId);
                        var brnd = dataContext.brand_master.Where(x => x.BrandId == no).FirstOrDefault();
                        if (brnd != null)
                        {
                            brnd.BrandName = brand.BrandName;
                            brnd.BrandManagerName = brand.BrandManagerName;
                            brnd.Category = brand.Category;
                            brnd.NoOfLocations = brand.NoOfLocations;
                            dataContext.SaveChanges();
                        }

                        return Global.Merchant.brandUpdated;
                    }
                }
                else
                {
                    return Global.Merchant.InvalidBrandDetails;
                }
            }
            catch (Exception ex)
            {
                //call error view here
                //log the error
                EventLog.LogErrorData("Error occurred Merchant/UpdateBrand." + ex.Message, true);
                return Global.Merchant.UpdateBrandException;
            }
        }

        public string UpdateBranch(branch_master branch)
        {
            try
            {
                if (branch != null)
                {
                    using (MerchantEntities dataContext = new MerchantEntities())
                    {
                        string userid = Session["UserId"].ToString();
                        merchant_master master = dataContext.merchant_master.Where(x => x.UserId == userid).FirstOrDefault();

                        int no = Convert.ToInt32(branch.BranchId);
                        var brh = dataContext.branch_master.Where(x => x.BranchId == no).FirstOrDefault();
                        if (brh != null)
                        {
                            brh.BranchLocation = branch.BranchLocation;
                            brh.BranchManagerName = branch.BranchManagerName;
                            brh.IsMenuAllowed = branch.IsMenuAllowed;
                            brh.IsCouponAllowed = branch.IsCouponAllowed;
                            brh.IsAddUserAllowed = branch.IsAddUserAllowed;
                            brh.IsEventCouponsAllowed = branch.IsEventCouponsAllowed;
                            dataContext.SaveChanges();
                        }

                        return Global.Merchant.branchUpdated;
                    }
                }
                else
                {
                    return Global.Merchant.InvalidBranchDetails;
                }
            }
            catch (Exception ex)
            {
                //call error view here
                //log the error
                EventLog.LogErrorData("Error occurred Merchant/UpdateBranch." + ex.Message, true);
                return Global.Merchant.UpdateBranchException;
            }
        }

        public string UpdateStaff(staff_master staff)
        {
            try
            {
                if (staff != null)
                {
                    using (MerchantEntities dataContext = new MerchantEntities())
                    {
                        string userid = Session["UserId"].ToString();
                        merchant_master master = dataContext.merchant_master.Where(x => x.UserId == userid).FirstOrDefault();

                        int no = Convert.ToInt32(staff.StaffId);
                        var stf = dataContext.staff_master.Where(x => x.StaffId == no).FirstOrDefault();
                        if (stf != null)
                        {
                            stf.BranchId = staff.BranchId;
                            stf.BrandId = staff.BrandId;
                            stf.StaffName = staff.StaffName;
                            stf.IsCouponSendAllowed = staff.IsCouponSendAllowed;
                            dataContext.SaveChanges();
                        }

                        return "Staff details updated successfully";
                    }
                }
                else
                {
                    return "Staff member does not exist";
                }
            }
            catch (Exception ex)
            {
                //call error view here
                //log the error
                EventLog.LogErrorData("Error occurred Merchant/UpdateBranch." + ex.Message, true);
                return "Error occurred while updating staff member";
            }
        }

        [AdminMerchantFilter]
        public JsonResult GetCountryFromcode(string countrycode)
        {
            try
            {
                using (MerchantEntities dataContext = new MerchantEntities())
                {
                    var country = dataContext.country_master.Where(c => c.CountryCode == countrycode).FirstOrDefault();
                    return Json(country, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                //call error view here
                //log the error
                EventLog.LogErrorData("Error occurred Merchant/GetCountryFromcode." + ex.Message, true);
                return Json("Error occurred while retrieving country from code", JsonRequestBehavior.AllowGet);
            }
        }

        public string AddNewStaff(merchant_master mch)
        {
            try
            {
                GenerateCouponCode cpncode = new GenerateCouponCode();
                if (mch != null && mch.staff != null)
                {
                    //Create staff first. then create staff master and add his id to already created staff
                    staff_master staff = new staff_master();

                    string userid = Session["UserId"].ToString();
                    using (MerchantEntities dataContext = new MerchantEntities())
                    {
                        brand_master brand = dataContext.brand_master.Where(x => x.BrandId == mch.staff.BrandId).FirstOrDefault();
                        merchant_master master = dataContext.merchant_master.Where(x => x.merchantid == brand.BrandManagerId).FirstOrDefault();
                        if (master != null)
                        {
                            //Insert staff
                            string username = "";
                            if (mch.staff.PrimaryId == "cell")
                            {
                                username = mch.PhoneNumber;
                            }
                            else
                            {
                                username = mch.Email;
                            }



                            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));
                            var UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));

                            UserManager.UserValidator = new UserValidator<ApplicationUser>(UserManager)
                            {
                                AllowOnlyAlphanumericUserNames = false,
                                RequireUniqueEmail = true
                            };

                            user curruser = dataContext.users.Where(x => x.UserName == username).FirstOrDefault();

                            if (curruser == null)
                            {
                                var user = new ApplicationUser();
                                user.UserName = username;

                                string userPWD = "123456";
                                user.FirstName = mch.MerchantName;
                                user.LastName = "";
                                user.Phone = mch.PhoneNumber;
                                user.PhoneNumber = mch.PhoneNumber;
                                user.Email = mch.Email;
                                var chkUser = UserManager.Create(user, userPWD);
                                if (chkUser.Succeeded)
                                {
                                    var rolesForUser = UserManager.GetRoles(user.Id);
                                    if (!rolesForUser.Contains("Staff"))
                                    {
                                        UserManager.AddToRole(user.Id, "Staff");
                                    }
                                    user currentuser = dataContext.users.Where(x => x.Id == user.Id).FirstOrDefault();
                                    if (currentuser != null)
                                    {
                                        currentuser.FirstName = mch.MerchantName;
                                        currentuser.LastName = "";
                                        dataContext.SaveChanges();
                                    }

                                    merchant_master branchmerchant = new merchant_master();

                                    branchmerchant.MerchantName = master.MerchantName;
                                    branchmerchant.BuildingName = master.BuildingName;
                                    branchmerchant.SocietyName = master.SocietyName;
                                    branchmerchant.Street = master.Street;
                                    branchmerchant.Location = master.Location;
                                    branchmerchant.City = master.City;
                                    branchmerchant.State = master.State;
                                    branchmerchant.Country = master.Country;
                                    branchmerchant.PinCode = master.PinCode;
                                    branchmerchant.MerchantLogo = mch.MerchantLogo;
                                    branchmerchant.Category = brand.Category;
                                    branchmerchant.UserId = user.Id;
                                    branchmerchant.PhoneNumber = mch.PhoneNumber;
                                    branchmerchant.Email = mch.Email;
                                    branchmerchant.DECName = master.DECName;
                                    branchmerchant.MerchantDEC = master.MerchantDEC;
                                    branchmerchant.LanguageId = master.LanguageId;
                                    branchmerchant.creation_date = DateTime.Now;
                                    branchmerchant.user_contact = master.PhoneNumber;
                                    branchmerchant.merchantDecFromLibrary = master.merchantDecFromLibrary;
                                    branchmerchant.DECColor = master.DECColor;
                                    branchmerchant.MenuCsvFile = master.MenuCsvFile;
                                    branchmerchant.button1_text = master.button1_text;
                                    branchmerchant.button1_url = master.button1_url;
                                    branchmerchant.button2_text = master.button2_text;
                                    branchmerchant.button2_url = master.button2_url;
                                    branchmerchant.button3_text = master.button3_text;
                                    branchmerchant.button3_url = master.button3_url;
                                    branchmerchant.button4_text = master.button4_text;
                                    branchmerchant.button4_url = master.button4_url;
                                    branchmerchant.NoOfLocations = brand.NoOfLocations;
                                    branchmerchant.IsSuperAdmin = false;

                                    dataContext.merchant_master.Add(branchmerchant);
                                    dataContext.SaveChanges();
                                    int newmerchantid = branchmerchant.merchantid;

                                    staff.StaffMasterId = newmerchantid;
                                    staff.MerchantId = master.merchantid;
                                    staff.BrandId = Convert.ToInt32(mch.staff.BrandId);
                                    staff.BranchId = mch.staff.BranchId;
                                    staff.PrimaryId = mch.staff.PrimaryId;
                                    staff.StaffName = mch.MerchantName;
                                    staff.IsCouponSendAllowed = mch.staff.IsCouponSendAllowed;
                                    dataContext.staff_master.Add(staff);
                                    dataContext.SaveChanges();

                                }
                                else
                                {
                                    EventLog.LogErrorData("Error occurred while creating staff member." + chkUser.Errors.FirstOrDefault(), true);
                                    return Global.Merchant.StaffManagerException;
                                }
                            }
                            else
                            {
                                merchant_master staffmaster = dataContext.merchant_master.Where(x => x.UserId == curruser.Id).FirstOrDefault();
                                if (staffmaster.merchantid != master.merchantid)
                                {
                                    EventLog.LogErrorData("Error occurred while creating staff. Staff " + username + " already registered with some other merchant.", true);
                                    return "Staff " + username + " already registered with some other merchant.";
                                }
                                var rolesForUser = UserManager.GetRoles(curruser.Id);
                                if (!rolesForUser.Contains("Staff"))
                                {
                                    UserManager.AddToRole(curruser.Id, "Staff");
                                }

                                staff.StaffMasterId = staffmaster.merchantid;
                                staff.MerchantId = master.merchantid;
                                staff.BrandId = Convert.ToInt32(mch.staff.BrandId);
                                staff.BranchId = mch.staff.BranchId;
                                staff.PrimaryId = mch.staff.PrimaryId;
                                staff.StaffName = mch.MerchantName;
                                staff.IsCouponSendAllowed = mch.staff.IsCouponSendAllowed;
                                dataContext.staff_master.Add(staff);
                                dataContext.SaveChanges();
                            }
                        }
                        else
                        {
                            return Global.Merchant.InvalidMerchantDetails;
                        }
                    }
                    return Global.Merchant.NewStaffSuccess;
                }
                else
                {
                    return Global.Merchant.EnterStaffDetails;
                }

            }
            catch (Exception ex)
            {
                //call error view here
                //log the error
                EventLog.LogErrorData("Error occured Merchant/AddNewStaff." + ex.Message, true);
                return Global.Merchant.StaffManagerException;
            }
        }

        public string AddNewBranch(merchant_master mch)
        {
            try
            {
                if (mch != null && mch.branch != null)
                {
                    //Create brand first. then create brand manager and add his id to already created brand
                    branch_master branch = new branch_master();
                    string userid = Session["UserId"].ToString();
                    using (MerchantEntities dataContext = new MerchantEntities())
                    {
                        brand_master brand = dataContext.brand_master.Where(x => x.BrandId == mch.branch.BrandId).FirstOrDefault();

                        merchant_master master = dataContext.merchant_master.Where(x => x.merchantid == brand.BrandManagerId).FirstOrDefault();
                        if (master != null)
                        {
                            //Insert brand manager
                            string username = "";
                            if (mch.branch.PrimaryId == "cell")
                            {
                                username = mch.PhoneNumber;
                            }
                            else
                            {
                                username = mch.Email;
                            }

                            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));
                            var UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));
                            UserManager.UserValidator = new UserValidator<ApplicationUser>(UserManager)
                            {
                                AllowOnlyAlphanumericUserNames = false,
                                RequireUniqueEmail = true
                            };

                            user curruser = dataContext.users.Where(x => x.UserName == username).FirstOrDefault();

                            if (curruser == null)
                            {
                                var user = new ApplicationUser();
                                if (mch.branch.PrimaryId == "cell")
                                {
                                    user.UserName = mch.PhoneNumber;
                                }
                                else
                                {
                                    user.UserName = mch.Email;
                                }

                                string userPWD = "123456";
                                user.FirstName = mch.MerchantName;
                                user.LastName = "";
                                user.Phone = mch.PhoneNumber;
                                user.PhoneNumber = mch.PhoneNumber;
                                user.Email = mch.Email;
                                var chkUser = UserManager.Create(user, userPWD);
                                if (chkUser.Succeeded)
                                {
                                    var rolesForUser = UserManager.GetRoles(user.Id);
                                    if (!rolesForUser.Contains("LocationManager"))
                                    {
                                        UserManager.AddToRole(user.Id, "LocationManager");
                                    }
                                    user currentuser = dataContext.users.Where(x => x.Id == user.Id).FirstOrDefault();
                                    if (currentuser != null)
                                    {
                                        currentuser.FirstName = mch.MerchantName;
                                        currentuser.LastName = "";
                                        dataContext.SaveChanges();
                                    }

                                    merchant_master branchmerchant = new merchant_master();

                                    branchmerchant.MerchantName = master.MerchantName;
                                    branchmerchant.BuildingName = master.BuildingName;
                                    branchmerchant.SocietyName = master.SocietyName;
                                    branchmerchant.Street = master.Street;
                                    branchmerchant.Location = master.Location;
                                    branchmerchant.City = master.City;
                                    branchmerchant.State = master.State;
                                    branchmerchant.Country = master.Country;
                                    branchmerchant.PinCode = master.PinCode;
                                    branchmerchant.MerchantLogo = mch.MerchantLogo;
                                    branchmerchant.Category = brand.Category;
                                    branchmerchant.UserId = user.Id;
                                    branchmerchant.PhoneNumber = mch.PhoneNumber;
                                    branchmerchant.Email = mch.Email;
                                    branchmerchant.DECName = master.DECName;
                                    branchmerchant.MerchantDEC = master.MerchantDEC;
                                    branchmerchant.LanguageId = master.LanguageId;
                                    branchmerchant.creation_date = DateTime.Now;
                                    branchmerchant.user_contact = master.PhoneNumber;
                                    branchmerchant.merchantDecFromLibrary = master.merchantDecFromLibrary;
                                    branchmerchant.DECColor = master.DECColor;
                                    branchmerchant.MenuCsvFile = master.MenuCsvFile;
                                    branchmerchant.button1_text = master.button1_text;
                                    branchmerchant.button1_url = master.button1_url;
                                    branchmerchant.button2_text = master.button2_text;
                                    branchmerchant.button2_url = master.button2_url;
                                    branchmerchant.button3_text = master.button3_text;
                                    branchmerchant.button3_url = master.button3_url;
                                    branchmerchant.button4_text = master.button4_text;
                                    branchmerchant.button4_url = master.button4_url;
                                    branchmerchant.NoOfLocations = brand.NoOfLocations;
                                    branchmerchant.IsSuperAdmin = false;

                                    dataContext.merchant_master.Add(branchmerchant);
                                    dataContext.SaveChanges();
                                    int newmerchantid = branchmerchant.merchantid;

                                    branch.MerchantId = master.merchantid;
                                    branch.BrandId = Convert.ToInt32(mch.branch.BrandId);
                                    branch.BranchLocation = mch.branch.BranchLocation;
                                    branch.BranchManagerName = mch.branch.BranchManagerName;
                                    branch.PrimaryId = mch.branch.PrimaryId;
                                    branch.IsMenuAllowed = mch.branch.IsMenuAllowed;
                                    branch.IsCouponAllowed = mch.branch.IsCouponAllowed;
                                    branch.IsAddUserAllowed = mch.branch.IsAddUserAllowed;
                                    branch.IsEventCouponsAllowed = mch.branch.IsEventCouponsAllowed;
                                    branch.BranchManagerId = newmerchantid;
                                    dataContext.branch_master.Add(branch);
                                    dataContext.SaveChanges();

                                }
                                else
                                {
                                    EventLog.LogErrorData("Error occurred while creating location manager." + chkUser.Errors.FirstOrDefault(), true);
                                    return Global.Merchant.LocationManagerException;
                                }
                            }
                            else
                            {
                                merchant_master branchmaster = dataContext.merchant_master.Where(x => x.UserId == curruser.Id).FirstOrDefault();
                                if (branchmaster.merchantid != master.merchantid)
                                {
                                    EventLog.LogErrorData("Error occurred while creating location manager. Location Manager " + username + " already registered with some other merchant.", true);
                                    return "Location Manager " + username + " already registered with some other merchant.";
                                }
                                var rolesForUser = UserManager.GetRoles(curruser.Id);
                                if (!rolesForUser.Contains("LocationManager"))
                                {
                                    UserManager.AddToRole(curruser.Id, "LocationManager");
                                }

                                branch.MerchantId = master.merchantid;
                                branch.BrandId = Convert.ToInt32(mch.branch.BrandId);
                                branch.BranchLocation = mch.branch.BranchLocation;
                                branch.BranchManagerName = mch.branch.BranchManagerName;
                                branch.PrimaryId = mch.branch.PrimaryId;
                                branch.IsMenuAllowed = mch.branch.IsMenuAllowed;
                                branch.IsCouponAllowed = mch.branch.IsCouponAllowed;
                                branch.IsAddUserAllowed = mch.branch.IsAddUserAllowed;
                                branch.IsEventCouponsAllowed = mch.branch.IsEventCouponsAllowed;
                                branch.BranchManagerId = branchmaster.merchantid;
                                dataContext.branch_master.Add(branch);
                                dataContext.SaveChanges();
                            }
                        }
                        else
                        {
                            return Global.Merchant.InvalidMerchantDetails;
                        }
                    }
                    return Global.Merchant.NewLocationSuccess;
                }
                else
                {
                    return Global.Merchant.EnterLocationDetails;
                }

            }
            catch (Exception ex)
            {
                //call error view here
                //log the error
                EventLog.LogErrorData("Error occured Merchant/AddNewBranch." + ex.Message, true);
                return Global.Merchant.AddLocationException;
            }
        }

        public string AddNewBrand(merchant_master mch)
        {
            try
            {
                GenerateCouponCode cpncode = new GenerateCouponCode();
                if (mch != null && mch.brand != null)
                {
                    //Create brand first. then create brand manager and add his id to already created brand
                    brand_master brand = new brand_master();
                    string userid = Session["UserId"].ToString();
                    using (MerchantEntities dataContext = new MerchantEntities())
                    {
                        merchant_master master = dataContext.merchant_master.Where(x => x.UserId == userid).FirstOrDefault();
                        if (master != null)
                        {
                            //Insert brand manager
                            string username = "";
                            if (mch.brand.PrimaryId == "cell")
                            {
                                username = mch.PhoneNumber;
                            }
                            else
                            {
                                username = mch.Email;
                            }

                            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));
                            var UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));

                            UserManager.UserValidator = new UserValidator<ApplicationUser>(UserManager)
                            {
                                AllowOnlyAlphanumericUserNames = false,
                                RequireUniqueEmail = true
                            };

                            user curruser = dataContext.users.Where(x => x.UserName == username).FirstOrDefault();

                            if (curruser == null)
                            {
                                var user = new ApplicationUser();

                                user.UserName = username;
                                string userPWD = "123456";
                                user.FirstName = mch.MerchantName;
                                user.LastName = "";
                                user.Phone = mch.PhoneNumber;
                                user.PhoneNumber = mch.PhoneNumber;
                                user.Email = mch.Email;
                                var chkUser = UserManager.Create(user, userPWD);
                                if (chkUser.Succeeded)
                                {
                                    var rolesForUser = UserManager.GetRoles(user.Id);
                                    if (!rolesForUser.Contains("BrandManager"))
                                    {
                                        UserManager.AddToRole(user.Id, "BrandManager");
                                    }
                                    user currentuser = dataContext.users.Where(x => x.Id == user.Id).FirstOrDefault();
                                    if (currentuser != null)
                                    {
                                        currentuser.FirstName = mch.MerchantName;
                                        currentuser.LastName = "";
                                        dataContext.SaveChanges();
                                    }

                                    merchant_master brandmerchant = new merchant_master();

                                    brandmerchant.MerchantName = mch.brand.BrandName;
                                    brandmerchant.BuildingName = master.BuildingName;
                                    brandmerchant.SocietyName = master.SocietyName;
                                    brandmerchant.Street = master.Street;
                                    brandmerchant.Location = master.Location;
                                    brandmerchant.City = master.City;
                                    brandmerchant.State = master.State;
                                    brandmerchant.Country = master.Country;
                                    brandmerchant.PinCode = master.PinCode;
                                    brandmerchant.MerchantLogo = mch.MerchantLogo;
                                    brandmerchant.Category = mch.brand.Category;
                                    brandmerchant.UserId = user.Id;
                                    brandmerchant.PhoneNumber = mch.PhoneNumber;
                                    brandmerchant.Email = mch.Email;
                                    brandmerchant.DECName = mch.brand.BrandName;
                                    brandmerchant.MerchantDEC = master.MerchantDEC;
                                    brandmerchant.LanguageId = master.LanguageId;
                                    brandmerchant.creation_date = DateTime.Now;
                                    brandmerchant.user_contact = master.PhoneNumber;
                                    brandmerchant.merchantDecFromLibrary = master.merchantDecFromLibrary;
                                    brandmerchant.DECColor = master.DECColor;
                                    brandmerchant.MenuCsvFile = master.MenuCsvFile;
                                    brandmerchant.button1_text = master.button1_text;
                                    brandmerchant.button1_url = master.button1_url;
                                    brandmerchant.button2_text = master.button2_text;
                                    brandmerchant.button2_url = master.button2_url;
                                    brandmerchant.button3_text = master.button3_text;
                                    brandmerchant.button3_url = master.button3_url;
                                    brandmerchant.button4_text = master.button4_text;
                                    brandmerchant.button4_url = master.button4_url;
                                    brandmerchant.NoOfLocations = mch.brand.NoOfLocations;
                                    brandmerchant.IsSuperAdmin = false;

                                    dataContext.merchant_master.Add(brandmerchant);
                                    dataContext.SaveChanges();
                                    int newmerchantid = brandmerchant.merchantid;

                                    //Reward options
                                    if (master.RunRewardProgram == false)
                                    {
                                        redeemoption optoriginal = dataContext.redeemoptions.Where(x => x.MerchantId == master.UserId).OrderByDescending(x => x.CreationDate).FirstOrDefault();
                                        if (optoriginal != null)
                                        {
                                            redeemoption opt = new redeemoption();
                                            opt.MerchantId = brandmerchant.UserId;
                                            opt.Option1 = optoriginal.Option1;
                                            opt.Option2 = optoriginal.Option2;
                                            opt.Option3 = optoriginal.Option3;
                                            opt.Option4 = optoriginal.Option4;
                                            opt.Option5 = optoriginal.Option5;
                                            opt.CreationDate = DateTime.Now;
                                            dataContext.redeemoptions.Add(opt);
                                            dataContext.SaveChanges();
                                        }
                                    }

                                    merchantjoiningbonu originalbonus = dataContext.merchantjoiningbonus.Where(x => x.MerchantId == master.UserId).OrderByDescending(x => x.DateCreated).FirstOrDefault();

                                    if (originalbonus != null)
                                    {
                                        merchantjoiningbonu bonus = new merchantjoiningbonu();
                                        bonus.MerchantId = brandmerchant.UserId;
                                        bonus.JoiningBonus = Convert.ToInt32(originalbonus.JoiningBonus);
                                        bonus.DateCreated = DateTime.Now;
                                        dataContext.merchantjoiningbonus.Add(bonus);
                                        dataContext.SaveChanges();
                                    }

                                    rewardmaster originalreward = dataContext.rewardmasters.Where(x => x.MerchantId == master.UserId).OrderByDescending(x => x.CreationDate).FirstOrDefault();
                                    if (originalreward != null)
                                    {
                                        rewardmaster rdm = new rewardmaster();
                                        rdm.RewardName = "Reward Points";
                                        rdm.MerchantId = brandmerchant.UserId;
                                        rdm.RewardRs = originalreward.RewardRs;
                                        rdm.RewardPoints = originalreward.RewardPoints;
                                        rdm.CreationDate = DateTime.Now;
                                        dataContext.rewardmasters.Add(rdm);
                                        dataContext.SaveChanges();
                                    }

                                    redeemmaster originalredeem = dataContext.redeemmasters.Where(x => x.MerchantId == master.UserId).OrderByDescending(x => x.CreationDate).FirstOrDefault();
                                    if (originalredeem != null)
                                    {
                                        redeemmaster redm = new redeemmaster();
                                        redm.MerchantId = brandmerchant.UserId;
                                        redm.RedeemRs = originalredeem.RedeemRs;
                                        redm.RedeemPt = originalredeem.RedeemPt;
                                        redm.CreationDate = DateTime.Now;
                                        dataContext.redeemmasters.Add(redm);
                                        dataContext.SaveChanges();
                                    }

                                    //Add default review questions
                                    reviewtemplate rtemp = dataContext.reviewtemplates.Where(x => x.CategoryId == brand.Category).FirstOrDefault();
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
                                        rev.MerchantId = brandmerchant.merchantid;
                                        rev.MerchantUserId = brandmerchant.UserId;
                                        rev.CreationDate = DateTime.Now;

                                        dataContext.reviewmasters.Add(rev);
                                        dataContext.SaveChanges();
                                    }


                                    //Add default coupons
                                    List<coupontemplate> ctemp = dataContext.coupontemplates.Where(x => x.CategoryId == brand.Category).ToList();
                                    if (ctemp != null)
                                    {
                                        if (ctemp.Count > 0)
                                        {
                                            for (int i = 0; i < ctemp.Count; i++)
                                            {
                                                coupons_master cpn = new coupons_master();

                                                cpn.CouponTitle = ctemp[i].CouponTitle;


                                                cpn.CouponCode = cpncode.CreateCouponCode(brandmerchant.merchantid);

                                                cpn.CouponDetails = ctemp[i].CouponDetails;

                                                cpn.MerchantId = brandmerchant.merchantid;

                                                cpn.ValidFrom = DateTime.Now;

                                                cpn.ValidTill = DateTime.Now.AddYears(10);

                                                cpn.categoryid = ctemp[i].CategoryId;
                                                cpn.PercentageOff = ctemp[i].PercentageOff;
                                                cpn.Discount = ctemp[i].Discount;
                                                cpn.AboveAmount = ctemp[i].AboveAmount;

                                                cpn.ValidAtLocation = "All";
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

                                    List<eventcoupontemplate> etemp = dataContext.eventcoupontemplates.Where(x => x.CategoryId == brand.Category).ToList();
                                    if (etemp != null)
                                    {
                                        if (etemp.Count > 0)
                                        {
                                            for (int i = 0; i < etemp.Count; i++)
                                            {
                                                coupons_master cpn = new coupons_master();

                                                cpn.CouponTitle = etemp[i].CouponTitle;

                                                cpn.CouponCode = cpncode.CreateCouponCode(brandmerchant.merchantid);

                                                cpn.CouponDetails = etemp[i].CouponDetails;

                                                cpn.MerchantId = brandmerchant.merchantid;

                                                cpn.ValidFrom = DateTime.Now;

                                                //cpn.ValidTill = DateTime.Now.AddMonths(6);

                                                cpn.categoryid = etemp[i].CategoryId;
                                                cpn.PercentageOff = etemp[i].PercentageOff;
                                                cpn.Discount = etemp[i].Discount;
                                                cpn.AboveAmount = etemp[i].AboveAmount;

                                                cpn.ValidAtLocation = "All";
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
                                                eventcoupon.MerchantId = brandmerchant.merchantid;
                                                dataContext.eventcoupondetails.Add(eventcoupon);
                                                dataContext.SaveChanges();
                                            }
                                        }
                                    }

                                    brand.MerchantId = master.merchantid;
                                    brand.BrandName = mch.brand.BrandName;
                                    brand.BrandManagerName = mch.brand.BrandManagerName;
                                    brand.Category = mch.brand.Category;
                                    brand.NoOfLocations = mch.brand.NoOfLocations;
                                    brand.PrimaryId = mch.brand.PrimaryId;

                                    dataContext.brand_master.Add(brand);
                                    dataContext.SaveChanges();

                                    int brandid = brand.BrandId;

                                    brand.BrandManagerId = newmerchantid;
                                    dataContext.SaveChanges();
                                }
                                else
                                {
                                    EventLog.LogErrorData("Error occurred while creating brand manager." + chkUser.Errors.FirstOrDefault(), true);
                                    return Global.Merchant.BrandManagerException;
                                }
                            }
                            else
                            {
                                merchant_master brandmaster = dataContext.merchant_master.Where(x => x.UserId == curruser.Id).FirstOrDefault();
                                if (brandmaster.merchantid != master.merchantid)
                                {
                                    EventLog.LogErrorData("Error occurred while creating brand manager. Brand Manager " + username + " already registered with some other merchant.", true);
                                    return "Brand Manager " + username + " already registered with some other merchant.";
                                }
                                var rolesForUser = UserManager.GetRoles(curruser.Id);
                                if (!rolesForUser.Contains("BrandManager"))
                                {
                                    UserManager.AddToRole(curruser.Id, "BrandManager");
                                }

                                brand.MerchantId = master.merchantid;
                                brand.BrandName = mch.brand.BrandName;
                                brand.BrandManagerName = mch.brand.BrandManagerName;
                                brand.Category = mch.brand.Category;
                                brand.NoOfLocations = mch.brand.NoOfLocations;
                                brand.PrimaryId = mch.brand.PrimaryId;

                                dataContext.brand_master.Add(brand);
                                dataContext.SaveChanges();

                                int brandid = brand.BrandId;

                                brand.BrandManagerId = brandmaster.merchantid;
                                dataContext.SaveChanges();



                                //Add default review questions
                                reviewmaster originalreview = dataContext.reviewmasters.Where(x => x.MerchantId == brandmaster.merchantid).OrderByDescending(x => x.CreationDate).FirstOrDefault();
                                if (originalreview == null)
                                {
                                    reviewtemplate rtemp = dataContext.reviewtemplates.Where(x => x.CategoryId == brand.Category).FirstOrDefault();
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
                                        rev.MerchantId = brandmaster.merchantid;
                                        rev.MerchantUserId = brandmaster.UserId;
                                        rev.CreationDate = DateTime.Now;

                                        dataContext.reviewmasters.Add(rev);
                                        dataContext.SaveChanges();
                                    }
                                }


                                //Add default coupons
                                List<coupons_master> coupons = dataContext.coupons_master.Where(x => x.MerchantId == brandmaster.merchantid).ToList();
                                if (coupons != null)
                                {
                                    if (coupons.Count == 0)
                                    {

                                        List<coupontemplate> ctemp = dataContext.coupontemplates.Where(x => x.CategoryId == brand.Category).ToList();
                                        if (ctemp != null)
                                        {
                                            if (ctemp.Count > 0)
                                            {
                                                for (int i = 0; i < ctemp.Count; i++)
                                                {
                                                    coupons_master cpn = new coupons_master();

                                                    cpn.CouponTitle = ctemp[i].CouponTitle;


                                                    cpn.CouponCode = cpncode.CreateCouponCode(brandmaster.merchantid);

                                                    cpn.CouponDetails = ctemp[i].CouponDetails;

                                                    cpn.MerchantId = brandmaster.merchantid;

                                                    cpn.ValidFrom = DateTime.Now;

                                                    cpn.ValidTill = DateTime.Now.AddYears(10);

                                                    cpn.categoryid = ctemp[i].CategoryId;
                                                    cpn.PercentageOff = ctemp[i].PercentageOff;
                                                    cpn.Discount = ctemp[i].Discount;
                                                    cpn.AboveAmount = ctemp[i].AboveAmount;

                                                    cpn.ValidAtLocation = "All";
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
                                }

                                List<eventcoupondetail> ecoupons = dataContext.eventcoupondetails.Where(x => x.MerchantId == brandmaster.merchantid).ToList();
                                if (ecoupons != null)
                                {
                                    if (ecoupons.Count == 0)
                                    {

                                        List<eventcoupontemplate> etemp = dataContext.eventcoupontemplates.Where(x => x.CategoryId == brand.Category).ToList();
                                        if (etemp != null)
                                        {
                                            if (etemp.Count > 0)
                                            {
                                                for (int i = 0; i < etemp.Count; i++)
                                                {
                                                    coupons_master cpn = new coupons_master();

                                                    cpn.CouponTitle = etemp[i].CouponTitle;

                                                    cpn.CouponCode = cpncode.CreateCouponCode(brandmaster.merchantid);

                                                    cpn.CouponDetails = etemp[i].CouponDetails;

                                                    cpn.MerchantId = brandmaster.merchantid;

                                                    cpn.ValidFrom = DateTime.Now;

                                                    //cpn.ValidTill = DateTime.Now.AddMonths(6);

                                                    cpn.categoryid = etemp[i].CategoryId;
                                                    cpn.PercentageOff = etemp[i].PercentageOff;
                                                    cpn.Discount = etemp[i].Discount;
                                                    cpn.AboveAmount = etemp[i].AboveAmount;

                                                    cpn.ValidAtLocation = "All";
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
                                                    eventcoupon.MerchantId = brandmaster.merchantid;
                                                    dataContext.eventcoupondetails.Add(eventcoupon);
                                                    dataContext.SaveChanges();
                                                }
                                            }
                                        }
                                    }
                                }

                            }
                        }
                        else
                        {
                            return Global.Merchant.InvalidMerchantDetails;
                        }
                    }
                    return Global.Merchant.NewBrandSuccess;
                }
                else
                {
                    return Global.Merchant.EnterBrandDetails;
                }
            }
            catch (Exception ex)
            {
                //call error view here
                //log the error
                EventLog.LogErrorData("Error occured Merchant/AddNewBrand." + ex.Message, true);
                return Global.Merchant.AddBrandException;
            }
        }

        public string AddNewCoupon(coupons_master cpn)
        {
            GenerateCouponCode cpncode = new GenerateCouponCode();
            try
            {
                if (cpn != null)
                {

                    using (MerchantEntities dataContext = new MerchantEntities())
                    {
                        string userid = Session["UserId"].ToString();
                        merchant_master master = dataContext.merchant_master.Where(x => x.UserId == userid).FirstOrDefault();
                        branch_master branch = new branch_master();

                        if (User.IsInRole("LocationManager"))
                        {
                            merchant_master branchmaster = dataContext.merchant_master.Where(x => x.UserId == userid).FirstOrDefault();
                            branch = dataContext.branch_master.Where(x => x.BranchManagerId == branchmaster.merchantid).FirstOrDefault();
                            master = dataContext.merchant_master.Where(x => x.merchantid == branch.MerchantId).FirstOrDefault();
                        }

                        if (master != null)
                        {
                            cpn.MerchantId = master.merchantid;
                            cpn.categoryid = master.Category;
                            cpn.CouponCode = cpncode.CreateCouponCode(master.merchantid);
                            if (User.IsInRole("LocationManager"))
                            {
                                cpn.ValidAtLocation = branch.BranchLocation;
                            }
                            else
                            {
                                cpn.ValidAtLocation = cpn.ValidAtLocation;
                            }


                            if (cpn.ValidFrom == null)
                            {
                                cpn.ValidFrom = DateTime.Now;
                            }

                            cpn.DEC = cpn.DEC;

                            cpn.DateCreated = DateTime.Now;


                            //List<ValidLocations> myDeserializedObjList = (List<ValidLocations>)Newtonsoft.Json.JsonConvert.DeserializeObject(cpn.ValidAtLocation, typeof(List<ValidLocations>));
                            //string validatlocations = "";
                            //if (myDeserializedObjList != null)
                            //{
                            //    foreach (ValidLocations loc in myDeserializedObjList)
                            //    {
                            //        validatlocations += loc.id.ToString() + ",";
                            //    }
                            //}
                            //if (validatlocations != "")
                            //{
                            //    validatlocations = validatlocations.Remove(validatlocations.LastIndexOf(","));
                            //}
                            //cpn.ValidAtLocation = validatlocations;

                            //uncomment to generate QR Code
                            //byte[] byteimage = GenerateQRCode(cpn.CouponCode);
                            //cpn.QRCode = byteimage;


                            dataContext.coupons_master.Add(cpn);
                            dataContext.SaveChanges();

                            //if (cpn.ShareWithAll == 1)
                            //{
                            //    //Share coupon with all consumers who have this merchant's dec.
                            //    //Get all mobile numbers registered with this merchant
                            //    var consumerphones = dataContext.merchantconsumerdetails.Where(x => x.MerchantId == userid).Select(x => x.ConsumerPhone).Distinct().ToList();
                            //    foreach (string ph in consumerphones)
                            //    {
                            //        string result = AddNewCouponConsumer(ph, cpn.couponid.ToString());
                            //    }

                            //}

                            if (cpn.conditions != null)
                            {
                                foreach (couponcondition cond in cpn.conditions)
                                {
                                    if (string.IsNullOrEmpty(cond.Condition) == false)
                                    {
                                        couponcondition condition = new couponcondition();
                                        condition.couponid = cpn.couponid;
                                        condition.Condition = cond.Condition;
                                        dataContext.couponconditions.Add(condition);
                                        dataContext.SaveChanges();
                                    }
                                }
                            }

                            return cpn.couponid.ToString();// "Coupon Added Successfully";
                        }
                        else
                        {
                            return "-1";// "Invalid Merchant Details";
                        }

                    }
                }
                else
                {
                    return "-1";// "Invalid Merchant Details";
                }
            }

            catch (Exception ex)
            {
                //call error view here
                //log the error
                EventLog.LogErrorData("Error occurred Merchant/AddNewCoupon." + ex.Message, true);
                return Global.Merchant.AddNewCouponException;
            }
        }


        public string AddNewReview(reviewmaster rvw)
        {
            try
            {
                if (rvw != null)
                {

                    using (MerchantEntities dataContext = new MerchantEntities())
                    {
                        string userid = Session["UserId"].ToString();
                        merchant_master master = dataContext.merchant_master.Where(x => x.UserId == userid).FirstOrDefault();
                        if (master != null)
                        {
                            rvw.MerchantId = master.merchantid;
                            rvw.MerchantUserId = master.UserId;
                        }
                        rvw.CreationDate = DateTime.Now;

                        //Insert review
                        //int revid = InsertReviewQuestions(rvw);
                        dataContext.reviewmasters.Add(rvw);
                        dataContext.SaveChanges();


                        //Send review to all DECs
                        function.SendReviewsToDECs(rvw);


                        return Global.Merchant.ReviewCreatedMessage;
                    }
                }
                else
                {
                    return Global.Merchant.InvalidLoginDetails;
                }
            }

            catch (Exception ex)
            {
                //call error view here
                //log the error
                EventLog.LogErrorData("Error occurred Merchant/AddNewReview." + ex.Message, true);
                return Global.Merchant.AddNewReviewException;
            }
        }

        public string SetEventCoupon(string birthdaycoupon, string anncoupon, string reviewcoupon, string sharecoupon)
        {
            try
            {
                using (MerchantEntities dataContext = new MerchantEntities())
                {
                    string userid = Session["UserId"].ToString();
                    merchant_master master = dataContext.merchant_master.Where(x => x.UserId == userid).FirstOrDefault();
                    if (User.IsInRole("LocationManager"))
                    {
                        branch_master branch = dataContext.branch_master.Where(x => x.BranchManagerId == master.merchantid).FirstOrDefault();
                        master = dataContext.merchant_master.Where(x => x.merchantid == branch.MerchantId).FirstOrDefault();
                    }

                    if (master != null)
                    {
                        if (birthdaycoupon != "")
                        {
                            int no = Convert.ToInt32(birthdaycoupon);
                            if (no != 0)
                            {
                                eventcoupondetail ev = new eventcoupondetail();
                                ev.EventId = 1;
                                ev.CouponId = no;
                                ev.MerchantId = master.merchantid;

                                dataContext.eventcoupondetails.Add(ev);
                                dataContext.SaveChanges();
                            }
                        }

                        if (anncoupon != "")
                        {
                            int no = Convert.ToInt32(anncoupon);
                            if (no != 0)
                            {
                                eventcoupondetail ev = new eventcoupondetail();
                                ev.EventId = 2;
                                ev.CouponId = no;
                                ev.MerchantId = master.merchantid;

                                dataContext.eventcoupondetails.Add(ev);
                                dataContext.SaveChanges();
                            }
                        }

                        if (reviewcoupon != "")
                        {
                            int no = Convert.ToInt32(reviewcoupon);
                            if (no != 0)
                            {
                                eventcoupondetail ev = new eventcoupondetail();
                                ev.EventId = 3;
                                ev.CouponId = no;
                                ev.MerchantId = master.merchantid;

                                dataContext.eventcoupondetails.Add(ev);
                                dataContext.SaveChanges();
                            }
                        }

                        if (sharecoupon != "")
                        {
                            int no = Convert.ToInt32(sharecoupon);
                            if (no != 0)
                            {
                                eventcoupondetail ev = new eventcoupondetail();
                                ev.EventId = 4;
                                ev.CouponId = no;
                                ev.MerchantId = master.merchantid;

                                dataContext.eventcoupondetails.Add(ev);
                                dataContext.SaveChanges();
                            }
                        }

                        return Global.Merchant.AddEventCouponMessage;
                    }
                    else
                    {
                        return Global.Merchant.InvalidLoginDetails;
                    }
                }
            }
            catch (Exception ex)
            {
                //call error view here
                //log the error
                EventLog.LogErrorData("Error occurred Merchant/SetEventCoupon." + ex.Message, true);
                return Global.Merchant.SetEventCouponsException;
            }
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
                EventLog.LogErrorData("Error occurred in Merchant/GenerateQRCode: " + ex.Message, true);
                return byteImage;
            }
        }

        public string UpdateCoupon(coupons_master cpn)
        {
            try
            {
                if (cpn != null)
                {
                    using (MerchantEntities dataContext = new MerchantEntities())
                    {
                        string userid = Session["UserId"].ToString();

                        int no = Convert.ToInt32(cpn.couponid);
                        var coupon = dataContext.coupons_master.Where(x => x.couponid == no).FirstOrDefault();
                        coupon.CouponCode = cpn.CouponCode;
                        coupon.CouponTitle = cpn.CouponTitle;
                        coupon.CouponDetails = cpn.CouponDetails;
                        coupon.MerchantId = cpn.MerchantId;
                        coupon.ValidFrom = cpn.ValidFrom;

                        coupon.ValidTill = cpn.ValidTill;
                        coupon.categoryid = cpn.categoryid;
                        coupon.PercentageOff = cpn.PercentageOff;
                        coupon.Discount = cpn.Discount;
                        coupon.MaxDiscount = cpn.MaxDiscount;
                        coupon.AboveAmount = cpn.AboveAmount;
                        coupon.MaxCoupons = cpn.MaxCoupons;
                        coupon.ShareWithAll = cpn.ShareWithAll;
                        coupon.ValidAtLocation = cpn.ValidAtLocation;

                        //List<ValidLocations> myDeserializedObjList = (List<ValidLocations>)Newtonsoft.Json.JsonConvert.DeserializeObject(cpn.ValidAtLocation, typeof(List<ValidLocations>));
                        //string validatlocations = "";
                        //if (myDeserializedObjList != null)
                        //{
                        //    foreach (ValidLocations loc in myDeserializedObjList)
                        //    {
                        //        validatlocations += loc.id.ToString() + ",";
                        //    }
                        //}

                        //validatlocations = validatlocations.Remove(validatlocations.LastIndexOf(","));
                        //coupon.ValidAtLocation = validatlocations;

                        coupon.DEC = cpn.DEC;
                        //uncomment to generate QR Code
                        //if (cpn.QRCode == null)
                        //{
                        //    byte[] byteimage = GenerateQRCode(cpn.CouponCode);
                        //    coupon.QRCode = byteimage;
                        //}
                        //else
                        //{
                        //    coupon.QRCode = cpn.QRCode;
                        //}

                        dataContext.SaveChanges();

                        //if (cpn.ShareWithAll == 1)
                        //{
                        //    //Share coupon with all consumers who have this merchant's dec.
                        //    //Get all mobile numbers registered with this merchant
                        //    var consumerphones = dataContext.merchantconsumerdetails.Where(x => x.MerchantId == userid).Select(x => x.ConsumerPhone).Distinct().ToList();
                        //    foreach (string ph in consumerphones)
                        //    {
                        //        string result = AddNewCouponConsumer(ph, cpn.couponid.ToString());
                        //    }

                        //}

                        List<couponcondition> conditions = dataContext.couponconditions.Where(x => x.couponid == cpn.couponid).ToList();
                        if (conditions != null)
                        {
                            if (conditions.Count > 0)
                            {
                                dataContext.couponconditions.RemoveRange(conditions);
                                dataContext.SaveChanges();
                            }
                        }
                        if (cpn.conditions != null)
                        {
                            foreach (couponcondition cond in cpn.conditions)
                            {
                                if (string.IsNullOrEmpty(cond.Condition) == false)
                                {
                                    couponcondition condition = new couponcondition();
                                    condition.couponid = cpn.couponid;
                                    condition.Condition = cond.Condition;
                                    dataContext.couponconditions.Add(condition);
                                    dataContext.SaveChanges();
                                }
                            }
                        }


                        return Global.Merchant.couponUpdated;
                    }
                }
                else
                {
                    return Global.Merchant.InvalidCouponDetails;
                }
            }
            catch (Exception ex)
            {
                //call error view here
                //log the error
                EventLog.LogErrorData("Error occurred Merchant/UpdateCoupon." + ex.Message, true);
                return Global.Merchant.UpdateCouponException;
            }
        }

        public JsonResult GetBrandById(string BrandId)
        {
            try
            {
                using (MerchantEntities dataContext = new MerchantEntities())
                {
                    int no = Convert.ToInt32(BrandId);
                    brand_master brand = dataContext.brand_master.Where(x => x.BrandId == no).FirstOrDefault();

                    var jsonResult = Json(brand, JsonRequestBehavior.AllowGet);
                    jsonResult.MaxJsonLength = Int32.MaxValue;

                    return jsonResult;
                }
            }
            catch (Exception ex)
            {
                //call error view here
                //log the error
                EventLog.LogErrorData("Error occurred Merchant/GetBrandById." + ex.Message, true);
                return Json(Global.Merchant.GetBrandByIdException, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult GetBranchById(string BranchId)
        {
            try
            {
                using (MerchantEntities dataContext = new MerchantEntities())
                {
                    int no = Convert.ToInt32(BranchId);
                    branch_master branch = dataContext.branch_master.Where(x => x.BranchId == no).FirstOrDefault();
                    List<staff_master> staffs = dataContext.staff_master.Where(x => x.BrandId == branch.BrandId && x.BranchId == branch.BranchId).ToList();
                    branch.staffs = staffs;

                    var jsonResult = Json(branch, JsonRequestBehavior.AllowGet);
                    jsonResult.MaxJsonLength = Int32.MaxValue;

                    return jsonResult;
                }
            }
            catch (Exception ex)
            {
                //call error view here
                //log the error
                EventLog.LogErrorData("Error occurred Merchant/GetBranchById." + ex.Message, true);
                return Json(Global.Merchant.GetBranchByIdException, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult GetStaffById(string StaffId)
        {
            try
            {
                using (MerchantEntities dataContext = new MerchantEntities())
                {
                    int no = Convert.ToInt32(StaffId);
                    staff_master staff = dataContext.staff_master.Where(x => x.StaffId == no).FirstOrDefault();

                    var jsonResult = Json(staff, JsonRequestBehavior.AllowGet);
                    jsonResult.MaxJsonLength = Int32.MaxValue;

                    return jsonResult;
                }
            }
            catch (Exception ex)
            {
                //call error view here
                //log the error
                EventLog.LogErrorData("Error occurred Merchant/GetStaffById." + ex.Message, true);
                return Json("Error occurred while retrieving staff details", JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult GetBranches(string BrandId)
        {
            try
            {
                using (MerchantEntities dataContext = new MerchantEntities())
                {
                    string userid = Session["UserId"].ToString();

                    merchant_master master = dataContext.merchant_master.Where(x => x.UserId == userid).FirstOrDefault();
                    if (User.IsInRole("LocationManager"))
                    {
                        branch_master branch = dataContext.branch_master.Where(x => x.BranchManagerId == master.merchantid).FirstOrDefault();
                        master = dataContext.merchant_master.Where(x => x.merchantid == branch.MerchantId).FirstOrDefault();
                    }
                    int brand = Convert.ToInt32(BrandId);

                    List<branch_master> branches = dataContext.branch_master.Where(x => x.BrandId == brand && x.MerchantId == master.merchantid).ToList();

                    var jsonResult = Json(branches, JsonRequestBehavior.AllowGet);
                    jsonResult.MaxJsonLength = Int32.MaxValue;

                    return jsonResult;
                }
            }
            catch (Exception ex)
            {
                //call error view here
                //log the error
                EventLog.LogErrorData("Error occurred Merchant/GetStaffById." + ex.Message, true);
                return Json("Error occurred while retrieving staff details", JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult GeCouponById(string couponid)
        {
            try
            {
                using (MerchantEntities dataContext = new MerchantEntities())
                {
                    int no = Convert.ToInt32(couponid);
                    var coupon = dataContext.coupons_master.Find(no);
                    List<couponcondition> conditions = dataContext.couponconditions.Where(c => c.couponid == no).ToList();
                    coupon.conditions = conditions;
                    // return Json(coupon, JsonRequestBehavior.AllowGet);

                    var jsonResult = Json(coupon, JsonRequestBehavior.AllowGet);
                    jsonResult.MaxJsonLength = Int32.MaxValue;

                    return jsonResult;
                }
            }
            catch (Exception ex)
            {
                //call error view here
                //log the error
                EventLog.LogErrorData("Error occurred Merchant/GetCouponById." + ex.Message, true);
                return Json(Global.Merchant.GetCouponByIdException, JsonRequestBehavior.AllowGet);
            }
        }



        public JsonResult GetEventConditions(string couponid)
        {
            try
            {
                using (MerchantEntities dataContext = new MerchantEntities())
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
                EventLog.LogErrorData("Error occurred Merchant/GetEventConditions." + ex.Message, true);
                return Json(Global.Merchant.GetEventConditionsExcepton, JsonRequestBehavior.AllowGet);
            }
        }

        public string UpdateMerchant(merchant_master mch)
        {
            try
            {
                if (mch != null)
                {
                    using (MerchantEntities dataContext = new MerchantEntities())
                    {
                        int no = Convert.ToInt32(mch.merchantid);
                        var merchant = dataContext.merchant_master.Where(x => x.merchantid == no).FirstOrDefault();
                        merchant.MerchantName = mch.MerchantName;
                        merchant.DECName = mch.DECName;
                        merchant.BuildingName = mch.BuildingName;
                        merchant.SocietyName = mch.SocietyName;
                        merchant.Street = mch.Street;
                        merchant.Location = mch.Location;
                        merchant.City = mch.City;
                        merchant.State = mch.State;
                        merchant.Country = mch.Country;

                        merchant.button1_text = mch.button1_text;
                        merchant.button1_url = mch.button1_url;
                        merchant.button2_text = mch.button2_text;
                        merchant.button2_url = mch.button2_url;
                        merchant.button3_text = mch.button3_text;
                        merchant.button3_url = mch.button3_url;
                        merchant.button4_text = mch.button4_text;
                        merchant.button4_url = mch.button4_url;

                        merchant.PinCode = mch.PinCode;
                        merchant.MerchantLogo = mch.MerchantLogo;

                        merchant.MerchantDEC = mch.MerchantDEC;


                        merchant.merchantDecFromLibrary = mch.merchantDecFromLibrary;
                        merchant.DECColor = mch.DECColor;
                        merchant.Category = mch.Category;

                        merchant.Email = mch.Email;

                        merchant.RunRewardProgram = mch.RunRewardProgram;
                        merchant.RedeemProgram = mch.RedeemProgram;

                        dataContext.SaveChanges();

                        if (Convert.ToBoolean(merchant.RunRewardProgram) == true)
                        {
                            if (mch.RewardRs != null && mch.RewardPoints != null)
                            {
                                if (Convert.ToInt32(mch.RewardRs) != 0 && Convert.ToInt32(mch.RewardPoints) != 0)
                                {
                                    //Adding reward entries
                                    rewardmaster rdm = new rewardmaster();
                                    rdm.RewardName = "Reward Points";
                                    rdm.MerchantId = mch.UserId;
                                    rdm.RewardName = mch.RewardName;
                                    rdm.RewardRs = mch.RewardRs;
                                    rdm.RewardPoints = mch.RewardPoints;
                                    rdm.CreationDate = DateTime.Now;
                                    dataContext.rewardmasters.Add(rdm);
                                    dataContext.SaveChanges();
                                }
                            }
                        }

                        if (Convert.ToBoolean(merchant.RunRewardProgram) == true && mch.RedeemProgram == "Options")
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

                        if (mch.benefits != null)
                        {
                            merchant_benefits benefits = new merchant_benefits();
                            benefits.MerchantId = mch.UserId;
                            benefits.Benefit1 = mch.benefits.Benefit1;
                            benefits.Benefit2 = mch.benefits.Benefit2;
                            benefits.Benefit3 = mch.benefits.Benefit3;
                            benefits.Benefit4 = mch.benefits.Benefit4;
                            benefits.Benefit5 = mch.benefits.Benefit5;
                            benefits.CreationDate = DateTime.Now;
                            dataContext.merchant_benefits.Add(benefits);
                            dataContext.SaveChanges();
                        }

                        if (mch.exp != null)
                        {
                            pointscashbackexpiry expiry = new pointscashbackexpiry();
                            expiry.MerchantId = mch.UserId;
                            expiry.ExpPeriodId = mch.exp.ExpPeriodId;
                            expiry.CreationDate = DateTime.Now;
                            dataContext.pointscashbackexpiries.Add(expiry);
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

                        if (Convert.ToBoolean(merchant.RunRewardProgram) == true && mch.RedeemProgram == "Points")
                        {
                            if (mch.RedeemPt != null && mch.RedeemRs != null)
                            {
                                if (Convert.ToInt32(mch.RedeemPt) != 0 && Convert.ToInt32(mch.RedeemRs) != 0)
                                {
                                    ////Adding Redeem entries
                                    redeemmaster redm = new redeemmaster();
                                    redm.MerchantId = mch.UserId;

                                    redm.RedeemRs = mch.RedeemRs;
                                    redm.RedeemPt = mch.RedeemPt;
                                    redm.CreationDate = DateTime.Now;
                                    dataContext.redeemmasters.Add(redm);
                                    dataContext.SaveChanges();
                                }
                            }
                        }

                        if (Convert.ToBoolean(merchant.RunRewardProgram) == true && mch.RedeemProgram == "Cashback")
                        {
                            cashbackdetail cash = new cashbackdetail();
                            cash.MerchantId = mch.UserId;
                            cash.RedeemPoint = mch.cashbackdetails.RedeemPoint;
                            cash.RedeemRs = mch.cashbackdetails.RedeemRs;
                            cash.FixedCashBack = mch.cashbackdetails.FixedCashBack;
                            cash.IsCashBackPerTransaction = mch.cashbackdetails.IsCashBackPerTransaction;
                            cash.CreationDate = DateTime.Now;
                            dataContext.cashbackdetails.Add(cash);
                            dataContext.SaveChanges();

                            rewardmaster reward = dataContext.rewardmasters.Where(x => x.MerchantId == mch.UserId).OrderByDescending(x => x.CreationDate).FirstOrDefault();
                            if (reward != null)
                            {
                                reward.RewardName = "Cashback";
                                dataContext.SaveChanges();
                            }
                        }

                        return Global.Merchant.ProfileUpdated;
                    }
                }
                else
                {
                    return Global.Merchant.InvalidMerchantDetails;
                }
            }
            catch (Exception ex)
            {
                //call error view here
                //log the error
                EventLog.LogErrorData("Error occurred Merchant/UpdateMerchant." + ex.Message, true);
                return Global.Merchant.UpdateMerchantException;
            }
        }

        public string FinalGiftcardRedeem(string Id)
        {
            try
            {
                int no = Convert.ToInt32(Id);
                DateTime visitdate = DateTime.Now;

                if (string.IsNullOrEmpty(Id) == false)
                {
                    using (instadelight_consumerEntities consumerDataContext = new instadelight_consumerEntities())
                    {
                        consumergiftcarddetail giftcard = consumerDataContext.consumergiftcarddetails.Where(x => x.Id == no).FirstOrDefault();

                        if (giftcard != null)
                        {
                            giftcard.Status = 1;
                            consumerDataContext.SaveChanges();
                            return @Global.Merchant.GiftcardRedeemedSuccessfully;
                        }
                        else
                        {
                            return @Global.Merchant.GiftcardRedeemError;
                        }
                    }
                }
                else
                {
                    return @Global.Merchant.GiftcardRedeemError1;
                }
            }
            catch (Exception ex)
            {
                //call error view here
                //log the error
                EventLog.LogErrorData("Error occurred Merchant/FinalGiftcardRedeem." + ex.Message, true);
                return Global.Merchant.FinalRedeemGiftcardException;
            }
        }

        public string FinalRedeem(string CouponId, string SharedCouponId)
        {
            try
            {
                string consumeruserid = "";
                int couponno = Convert.ToInt32(CouponId);
                int sharedcouponno = Convert.ToInt32(SharedCouponId);
                DateTime visitdate = DateTime.Now;

                if (string.IsNullOrEmpty(CouponId) == false && string.IsNullOrEmpty(SharedCouponId) == false)
                {
                    using (MerchantEntities dataContext = new MerchantEntities())
                    {
                        string userid = Session["UserId"].ToString();

                        string ownerid = "";
                        string staffloc = "";
                        if (User.IsInRole("Staff"))
                        {
                            merchant_master master = dataContext.merchant_master.Where(x => x.UserId == userid).FirstOrDefault();
                            staff_master stfmgr = dataContext.staff_master.Where(x => x.StaffMasterId == master.merchantid).FirstOrDefault();
                            merchant_master owner = dataContext.merchant_master.Where(x => x.merchantid == stfmgr.MerchantId).FirstOrDefault();
                            ownerid = owner.UserId;
                            branch_master branch = dataContext.branch_master.Where(x => x.BranchId == stfmgr.BranchId).FirstOrDefault();
                            staffloc = branch.BranchLocation;
                        }
                        else if (User.IsInRole("LocationManager"))
                        {
                            merchant_master master = dataContext.merchant_master.Where(x => x.UserId == userid).FirstOrDefault();
                            branch_master branch = dataContext.branch_master.Where(x => x.BranchManagerId == master.merchantid).FirstOrDefault();
                            merchant_master owner = dataContext.merchant_master.Where(x => x.merchantid == branch.MerchantId).FirstOrDefault();
                            ownerid = owner.UserId;
                            staffloc = branch.BranchLocation;
                        }

                        coupons_master cpn = dataContext.coupons_master.Where(x => x.couponid == couponno).FirstOrDefault();
                        merchantconsumercoupondetail sharedcoupon = new merchantconsumercoupondetail();

                        if (cpn != null)
                        {
                            if (User.IsInRole("Staff") || User.IsInRole("LocationManager"))
                            {
                                sharedcoupon = dataContext.merchantconsumercoupondetails.Where(s => s.MerchantId == ownerid && s.CouponId == cpn.couponid && s.Id == sharedcouponno).FirstOrDefault();
                                if (sharedcoupon == null)
                                {
                                    return Global.Merchant.MerchantShareCouponErrorMessage.Replace("xyz", Session["UserName"].ToString());
                                }
                                consumeruserid = sharedcoupon.ConsumerId;
                            }
                            else
                            {
                                sharedcoupon = dataContext.merchantconsumercoupondetails.Where(s => s.MerchantId == userid && s.CouponId == cpn.couponid && s.Id == sharedcouponno).FirstOrDefault();
                                if (sharedcoupon == null)
                                {
                                    return Global.Merchant.MerchantShareCouponErrorMessage.Replace("xyz", Session["UserName"].ToString());
                                }
                                consumeruserid = sharedcoupon.ConsumerId;
                            }
                            using (instadelight_consumerEntities consumerDataContext = new instadelight_consumerEntities())
                            {
                                consumermaster consumer = consumerDataContext.consumermasters.Where(x => x.UserId == consumeruserid).FirstOrDefault();
                                //coupon_redeem_details checkredeem = dataContext.coupon_redeem_details.Where(x => x.couponcode  == cpn.CouponCode && x.merchantid == cpn.MerchantId && x.ConsumerId == consumeruserid).FirstOrDefault();
                                coupon_redeem_details checkredeem = dataContext.coupon_redeem_details.Where(x => x.couponid == sharedcouponno && x.merchantid == cpn.MerchantId && x.ConsumerId == consumeruserid).FirstOrDefault();
                                if (checkredeem != null)
                                {
                                    return Global.Merchant.AlreadyRedeemedErrorMessage.Replace("xyz", cpn.CouponCode).Replace("pqr", consumer.UserId).Replace("dt", Convert.ToDateTime(checkredeem.redeemedon).ToString("dd-MMM-yyyy"));
                                }


                                coupon_redeem_details redeem = new coupon_redeem_details();
                                redeem.couponid = sharedcouponno;
                                redeem.couponcode = cpn.CouponCode;
                                redeem.redeemedon = visitdate;
                                redeem.merchantid = cpn.MerchantId;

                                if (User.IsInRole("Staff") || User.IsInRole("LocationManager"))
                                {
                                    var mer = dataContext.merchant_master.Where(x => x.merchantid == cpn.MerchantId && x.UserId == ownerid).ToList();

                                    if (mer == null)
                                    {
                                        return Global.Merchant.InvalidMerchantCouponErrorMessage.Replace("xyz", Session["UserName"].ToString());
                                    }
                                    else if (mer.Count == 0)
                                    {
                                        return Global.Merchant.InvalidMerchantCouponErrorMessage.Replace("xyz", Session["UserName"].ToString());
                                    }

                                    redeem.merchantid = mer[0].merchantid;
                                    redeem.city = mer[0].City;
                                    redeem.location = mer[0].Location;
                                }
                                else
                                {
                                    var mer = dataContext.merchant_master.Where(x => x.merchantid == cpn.MerchantId && x.UserId == userid).ToList();

                                    if (mer == null)
                                    {
                                        return Global.Merchant.InvalidMerchantCouponErrorMessage.Replace("xyz", Session["UserName"].ToString());
                                    }
                                    else if (mer.Count == 0)
                                    {
                                        return Global.Merchant.InvalidMerchantCouponErrorMessage.Replace("xyz", Session["UserName"].ToString());
                                    }

                                    redeem.merchantid = mer[0].merchantid;
                                    redeem.city = mer[0].City;
                                    redeem.location = mer[0].Location;
                                }

                                redeem.ConsumerId = consumeruserid;
                                redeem.ConsumerPhone = consumer.UserId;
                                dataContext.coupon_redeem_details.Add(redeem);
                                dataContext.SaveChanges();

                                //Delete coupon from shared list
                                if (sharedcoupon != null)
                                {
                                    dataContext.merchantconsumercoupondetails.Remove(sharedcoupon);
                                    dataContext.SaveChanges();
                                }

                                consumervisitdetail visit = new consumervisitdetail();
                                visit.ConsumerId = consumeruserid;
                                visit.MerchantId = userid;
                                visit.VisitDate = visitdate;
                                consumerDataContext.consumervisitdetails.Add(visit);
                                consumerDataContext.SaveChanges();

                                return Global.Merchant.CouponRedeemedSuccessfully;
                            }
                        }
                        else
                        {
                            return Global.Merchant.InvalidCouponDetails;
                        }
                    }

                }
                else
                {
                    return Global.Merchant.InvalidCouponDetails;
                }
            }
            catch (Exception ex)
            {
                //call error view here
                //log the error
                EventLog.LogErrorData("Error occurred Merchant/FinalRedeem." + ex.Message, true);
                return Global.Merchant.FinalRedeemException;
            }
        }

        public string FinalRedeemPoints(string RedeemPoints, string CustPhoneNumber)
        {
            try
            {
                DateTime visitdate = DateTime.Now;
                string consumeruserid = "";
                string userid = Session["UserId"].ToString();
                MerchantEntities dataContext = new MerchantEntities();
                if (User.IsInRole("Staff"))
                {
                    merchant_master staffmaster = dataContext.merchant_master.Where(x => x.UserId == userid).FirstOrDefault();
                    staff_master stfmgr = dataContext.staff_master.Where(x => x.StaffMasterId == staffmaster.merchantid).FirstOrDefault();
                    merchant_master owner = dataContext.merchant_master.Where(x => x.merchantid == stfmgr.MerchantId).FirstOrDefault();
                    userid = owner.UserId;
                }
                else if (User.IsInRole("LocationManager"))
                {
                    merchant_master master = dataContext.merchant_master.Where(x => x.UserId == userid).FirstOrDefault();
                    branch_master branch = dataContext.branch_master.Where(x => x.BranchManagerId == master.merchantid).FirstOrDefault();
                    merchant_master owner = dataContext.merchant_master.Where(x => x.merchantid == branch.MerchantId).FirstOrDefault();
                    userid = owner.UserId;
                }

                if (string.IsNullOrEmpty(RedeemPoints) == false && string.IsNullOrEmpty(CustPhoneNumber) == false)
                {
                    int pointstoredeem = Convert.ToInt32(RedeemPoints);
                    using (instadelight_consumerEntities ConsumerdataContext = new instadelight_consumerEntities())
                    {
                        var isUser = ConsumerdataContext.consumerusers.Where(u => u.UserName == CustPhoneNumber).FirstOrDefault();
                        if (isUser != null)
                        {
                            consumeruserid = isUser.Id;
                        }
                        else
                        {
                            return Global.Merchant.ConsumerDoesNotExistErrorMessage;
                        }


                        consumerredeemdetail rdm = new consumerredeemdetail();
                        rdm.ConsumerId = consumeruserid;
                        rdm.MerchantId = userid;
                        rdm.PointsRedeemed = pointstoredeem;
                        rdm.RedeemDate = visitdate;
                        ConsumerdataContext.consumerredeemdetails.Add(rdm);
                        ConsumerdataContext.SaveChanges();

                        consumervisitdetail visit = new consumervisitdetail();
                        visit.ConsumerId = consumeruserid;
                        visit.MerchantId = userid;
                        visit.VisitDate = visitdate;
                        ConsumerdataContext.consumervisitdetails.Add(visit);
                        ConsumerdataContext.SaveChanges();


                        //consumerrewardpoint rwd = ConsumerdataContext.consumerrewardpoints.Where(x => x.ConsumerId == consumeruserid && x.MerchantId == userid).FirstOrDefault();
                        //if (rwd != null)
                        //{
                        //    rwd.Points = rwd.Points - pointstoredeem;
                        //    ConsumerdataContext.SaveChanges();
                        //}
                    }

                    return Global.Merchant.PointsRedeemedSuccessfully;
                }
                else
                {
                    return Global.Merchant.InvalidpointsDetails;
                }
            }

            catch (Exception ex)
            {
                //call error view here
                //log the error
                EventLog.LogErrorData("Error occurred Merchant/FinalRedeem." + ex.Message, true);
                return Global.Merchant.FinalRedeemPointsException;
            }
        }

        #region Send DEC and coupons

        public int InsertReviewQuestions(reviewmaster rev)
        {
            MySqlParameter param1 = new MySqlParameter();
            param1.Value = rev.MerchantId;
            param1.Direction = System.Data.ParameterDirection.Input;
            param1.ParameterName = "@MerchantId";
            param1.DbType = System.Data.DbType.Int32;

            MySqlParameter param2 = new MySqlParameter();
            param2.Value = rev.Question1;
            param2.Direction = System.Data.ParameterDirection.Input;
            param2.ParameterName = "@Question1";
            param2.DbType = System.Data.DbType.String;

            MySqlParameter param3 = new MySqlParameter();
            param3.Value = rev.Question1Type;
            param3.Direction = System.Data.ParameterDirection.Input;
            param3.ParameterName = "@Question1Type";
            param3.DbType = System.Data.DbType.String;

            MySqlParameter param4 = new MySqlParameter();
            param4.Value = rev.Question2;
            param4.Direction = System.Data.ParameterDirection.Input;
            param4.ParameterName = "@Question2";
            param4.DbType = System.Data.DbType.String;

            MySqlParameter param5 = new MySqlParameter();
            param5.Value = rev.Question2Type;
            param5.Direction = System.Data.ParameterDirection.Input;
            param5.ParameterName = "@Question2Type";
            param5.DbType = System.Data.DbType.String;

            MySqlParameter param6 = new MySqlParameter();
            param6.Value = rev.Question3;
            param6.Direction = System.Data.ParameterDirection.Input;
            param6.ParameterName = "@Question3";
            param6.DbType = System.Data.DbType.String;

            MySqlParameter param7 = new MySqlParameter();
            param7.Value = rev.Question3Type;
            param7.Direction = System.Data.ParameterDirection.Input;
            param7.ParameterName = "@Question3Type";
            param7.DbType = System.Data.DbType.String;

            MySqlParameter param8 = new MySqlParameter();
            param8.Value = rev.Question4;
            param8.Direction = System.Data.ParameterDirection.Input;
            param8.ParameterName = "@Question4";
            param8.DbType = System.Data.DbType.String;

            MySqlParameter param9 = new MySqlParameter();
            param9.Value = rev.Question4Type;
            param9.Direction = System.Data.ParameterDirection.Input;
            param9.ParameterName = "@Question4Type";
            param9.DbType = System.Data.DbType.String;

            MySqlParameter param10 = new MySqlParameter();
            param10.Value = rev.DefaultQuestion;
            param10.Direction = System.Data.ParameterDirection.Input;
            param10.ParameterName = "@DefaultQuestion";
            param10.DbType = System.Data.DbType.String;

            MySqlParameter param11 = new MySqlParameter();
            param11.Value = rev.DefaultType;
            param11.Direction = System.Data.ParameterDirection.Input;
            param11.ParameterName = "@DefaultType";
            param11.DbType = System.Data.DbType.String;

            MySqlParameter param12 = new MySqlParameter();
            param12.Value = rev.MerchantUserId;
            param12.Direction = System.Data.ParameterDirection.Input;
            param12.ParameterName = "@MerchantUserId";
            param12.DbType = System.Data.DbType.String;

            MySqlParameter param13 = new MySqlParameter();
            param13.Direction = System.Data.ParameterDirection.Output;
            param13.ParameterName = "@ID";


            int revid = 0;
            using (MerchantEntities dataContext = new MerchantEntities())
            {
                dataContext.Database.ExecuteSqlCommand("CALL InsertReviewQuestions(@MerchantId,@Question1, @Question1Type ,@Question2 ,@Question2Type ,@Question3 , @Question3Type ,@Question4 ,@Question4Type ,@DefaultQuestion ,@DefaultType , @MerchantUserId ,@ID)", param1, param2, param3, param4, param5, param6, param7, param8, param9, param10, param11, param12, param13);
            }
            revid = Convert.ToInt32(param13.Value);

            return revid;
        }

        public string AddNewDECConsumer(ShareDECParameters param)
        {
            try
            {
                DateTime visitdate = DateTime.Now;

                if (param.mobileno != null)
                {
                    var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(Consumercontext));
                    var UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(Consumercontext));
                    UserManager.UserValidator = new UserValidator<ApplicationUser>(UserManager)
                    {
                        AllowOnlyAlphanumericUserNames = false,
                        RequireUniqueEmail = true
                    };

                    MerchantEntities dataContext = new MerchantEntities();
                    string userid = Session["UserId"].ToString();
                    if (User.IsInRole("Staff"))
                    {
                        merchant_master staffmaster = dataContext.merchant_master.Where(x => x.UserId == userid).FirstOrDefault();
                        staff_master stfmgr = dataContext.staff_master.Where(x => x.StaffMasterId == staffmaster.merchantid).FirstOrDefault();
                        merchant_master owner = dataContext.merchant_master.Where(x => x.merchantid == stfmgr.MerchantId).FirstOrDefault();
                        userid = owner.UserId;
                    }
                    else if (User.IsInRole("LocationManager"))
                    {
                        merchant_master branchmaster = dataContext.merchant_master.Where(x => x.UserId == userid).FirstOrDefault();
                        branch_master branch = dataContext.branch_master.Where(x => x.BranchManagerId == branchmaster.merchantid).FirstOrDefault();
                        merchant_master owner = dataContext.merchant_master.Where(x => x.merchantid == branch.MerchantId).FirstOrDefault();
                        userid = owner.UserId;
                    }

                    string result = function.AddNewDECConsumer(param, userid);
                    return result;
                }
                else
                {
                    EventLog.LogErrorData("Error occurred Merchant/AddNewDECConsumer. Mobile No not specified", true);
                    return Global.Merchant.MobileNoPrompt;
                }
            }
            catch (Exception ex)
            {
                //call error view here
                //log the error
                if (ex.InnerException != null)
                {
                    EventLog.LogErrorData("Error occurred Merchant/AddNewDECConsumer." + ex.InnerException.Message, true);
                }
                else
                    EventLog.LogErrorData("Error occurred Merchant/AddNewDECConsumer." + ex.Message, true);
                return Global.Merchant.SendDECException;
            }
        }

        public string SendCouponToAll(string CouponId)
        {
            try
            {
                //Share coupon with all consumers who have this merchant's dec.
                //Get all mobile numbers registered with this merchant
                string userid = Session["UserId"].ToString();


                MerchantEntities dataContext = new MerchantEntities();
                if (User.IsInRole("Staff"))
                {
                    merchant_master staffmaster = dataContext.merchant_master.Where(x => x.UserId == userid).FirstOrDefault();
                    staff_master stfmgr = dataContext.staff_master.Where(x => x.StaffMasterId == staffmaster.merchantid).FirstOrDefault();
                    merchant_master owner = dataContext.merchant_master.Where(x => x.merchantid == stfmgr.MerchantId).FirstOrDefault();
                    userid = owner.UserId;
                }
                else if (User.IsInRole("LocationManager"))
                {
                    merchant_master branchmaster = dataContext.merchant_master.Where(x => x.UserId == userid).FirstOrDefault();
                    branch_master branch = dataContext.branch_master.Where(x => x.BranchManagerId == branchmaster.merchantid).FirstOrDefault();
                    merchant_master owner = dataContext.merchant_master.Where(x => x.merchantid == branch.MerchantId).FirstOrDefault();
                    userid = owner.UserId;
                }

                var consumerphones = dataContext.merchantconsumerdetails.Where(x => x.MerchantId == userid).Select(x => x.ConsumerPhone).Distinct().ToList();
                string result = "";
                foreach (string ph in consumerphones)
                {
                    result = AddNewCouponConsumer(ph, CouponId);
                }
                return result;
            }
            catch (Exception ex)
            {
                //call error view here
                //log the error
                EventLog.LogErrorData("Error occurred Merchant/SendCouponToAll." + ex.Message, true);
                return Global.Merchant.SendDECToAllException;
            }
        }

        public string AddNewCouponConsumer(string mobileno, string CouponId)
        {
            try
            {
                if (mobileno != null)
                {
                    var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(Consumercontext));
                    var UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(Consumercontext));
                    UserManager.UserValidator = new UserValidator<ApplicationUser>(UserManager)
                    {
                        AllowOnlyAlphanumericUserNames = false,
                        RequireUniqueEmail = true
                    };

                    MerchantEntities dataContext = new MerchantEntities();
                    string userid = Session["UserId"].ToString();

                    if (User.IsInRole("Staff"))
                    {
                        merchant_master staffmaster = dataContext.merchant_master.Where(x => x.UserId == userid).FirstOrDefault();
                        staff_master stfmgr = dataContext.staff_master.Where(x => x.StaffMasterId == staffmaster.merchantid).FirstOrDefault();
                        merchant_master owner = dataContext.merchant_master.Where(x => x.merchantid == stfmgr.MerchantId).FirstOrDefault();
                        userid = owner.UserId;
                    }
                    else if (User.IsInRole("LocationManager"))
                    {
                        merchant_master branchmaster = dataContext.merchant_master.Where(x => x.UserId == userid).FirstOrDefault();
                        branch_master branch = dataContext.branch_master.Where(x => x.BranchManagerId == branchmaster.merchantid).FirstOrDefault();
                        merchant_master owner = dataContext.merchant_master.Where(x => x.merchantid == branch.MerchantId).FirstOrDefault();
                        userid = owner.UserId;
                    }

                    string result = function.AddNewCouponConsumer(mobileno, CouponId, userid);
                    return result;
                }
                else
                {
                    return Global.Merchant.InvalidConsumerDetails;
                }
            }
            catch (Exception ex)
            {
                //call error view here
                //log the error
                EventLog.LogErrorData("Error occurred Merchant/AddNewCouponConsumer." + ex.Message, true);

                return Global.Merchant.SendCouponException;
            }
        }

        public ActionResult SendDecToFriends(string Mobileno)
        {
            string result = "";
            DateTime visitdate = DateTime.Now;

            if (Mobileno != null)
            {
                string[] cellnos = Mobileno.Split(',');
                if (cellnos.Length > 0)
                {
                    var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(Consumercontext));
                    var UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(Consumercontext));
                    UserManager.UserValidator = new UserValidator<ApplicationUser>(UserManager)
                    {
                        AllowOnlyAlphanumericUserNames = false,
                        RequireUniqueEmail = true
                    };

                    MerchantEntities dataContext = new MerchantEntities();
                    string userid = Session["UserId"].ToString();
                    if (User.IsInRole("Staff"))
                    {
                        merchant_master staffmaster = dataContext.merchant_master.Where(x => x.UserId == userid).FirstOrDefault();
                        staff_master stfmgr = dataContext.staff_master.Where(x => x.StaffMasterId == staffmaster.merchantid).FirstOrDefault();
                        merchant_master owner = dataContext.merchant_master.Where(x => x.merchantid == stfmgr.MerchantId).FirstOrDefault();
                        userid = owner.UserId;
                    }
                    else if (User.IsInRole("LocationManager"))
                    {
                        merchant_master branchmaster = dataContext.merchant_master.Where(x => x.UserId == userid).FirstOrDefault();
                        branch_master branch = dataContext.branch_master.Where(x => x.BranchManagerId == branchmaster.merchantid).FirstOrDefault();
                        merchant_master owner = dataContext.merchant_master.Where(x => x.merchantid == branch.MerchantId).FirstOrDefault();
                        userid = owner.UserId;
                    }


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

                        ShareDECParameters param = new ShareDECParameters();
                        param.mobileno = cellno;
                        param.BillAmt = "0";
                        result = function.AddNewDECConsumer(param, userid);
                    }
                    ViewBag.result = result;
                    return View();
                }
                else
                {
                    result = Global.Merchant.MobileNoPrompt;
                    ViewBag.result = result;
                    return View();
                }
            }
            else
            {
                result = Global.Merchant.MobileNoPrompt;
                ViewBag.result = result;
                return View();
            }
        }


        public ActionResult SendCouponsToFriends(string Mobileno, string CouponId)
        {
            string result = "";
            if (Mobileno != null)
            {
                string[] cellnos = Mobileno.Split(',');
                if (cellnos.Length > 0)
                {
                    var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(Consumercontext));
                    var UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(Consumercontext));
                    UserManager.UserValidator = new UserValidator<ApplicationUser>(UserManager)
                    {
                        AllowOnlyAlphanumericUserNames = false,
                        RequireUniqueEmail = true
                    };

                    MerchantEntities dataContext = new MerchantEntities();
                    string userid = Session["UserId"].ToString();

                    if (User.IsInRole("Staff"))
                    {
                        merchant_master staffmaster = dataContext.merchant_master.Where(x => x.UserId == userid).FirstOrDefault();
                        staff_master stfmgr = dataContext.staff_master.Where(x => x.StaffMasterId == staffmaster.merchantid).FirstOrDefault();
                        merchant_master owner = dataContext.merchant_master.Where(x => x.merchantid == stfmgr.MerchantId).FirstOrDefault();
                        userid = owner.UserId;
                    }
                    else if (User.IsInRole("LocationManager"))
                    {
                        merchant_master branchmaster = dataContext.merchant_master.Where(x => x.UserId == userid).FirstOrDefault();
                        branch_master branch = dataContext.branch_master.Where(x => x.BranchManagerId == branchmaster.merchantid).FirstOrDefault();
                        merchant_master owner = dataContext.merchant_master.Where(x => x.merchantid == branch.MerchantId).FirstOrDefault();
                        userid = owner.UserId;
                    }

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
                        result = function.AddNewCouponConsumer(cellno, CouponId, userid);
                    }
                    result = Global.Merchant.SendCouponToAll;
                    ViewBag.result = result;
                    return View();
                }
                else
                {
                    result = Global.Merchant.MobileNoPrompt;
                    ViewBag.result = result;
                    return View();
                }
            }
            else
            {
                result = Global.Merchant.MobileNoPrompt;
                ViewBag.result = result;
                return View();
            }
        }
        #endregion

        public JsonResult GetExpPeriods()
        {
            try
            {
                using (MerchantEntities dataContext = new MerchantEntities())
                {
                    var expperiodlist = dataContext.expiryperiodmasters.OrderBy(x => x.Id).ToList();

                    return Json(expperiodlist, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                //call error view here
                //log the error
                EventLog.LogErrorData("Error occurred Merchant/GetExpPeriods." + ex.Message, true);
                return Json("Error occurred while retrieving exp period.", JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult GetCountries()
        {
            try
            {
                using (MerchantEntities dataContext = new MerchantEntities())
                {
                    var countryList = dataContext.country_master.OrderBy(x => x.countryname).ToList();

                    return Json(countryList, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                //call error view here
                //log the error
                EventLog.LogErrorData("Error occurred Merchant/GetCountries." + ex.Message, true);
                return Json(Global.Merchant.GetCountryListException, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult GetCustomers()
        {
            {
                try
                {
                    using (MerchantEntities dataContext = new MerchantEntities())
                    {
                        string userid = Session["UserId"].ToString();

                        var consumerphones = dataContext.merchantconsumerdetails.Where(x => x.MerchantId == userid).Select(x => new { x.ConsumerPhone, x.ConsumerId }).Distinct().ToList();


                        return Json(consumerphones, JsonRequestBehavior.AllowGet);
                    }
                }
                catch (Exception ex)
                {
                    //call error view here
                    //log the error
                    EventLog.LogErrorData("Error occurred Merchant/GetCountries." + ex.Message, true);
                    return Json(Global.Merchant.GetCountryListException, JsonRequestBehavior.AllowGet);
                }
            }
        }

        public JsonResult GetReview()
        {
            try
            {
                using (MerchantEntities dataContext = new MerchantEntities())
                {
                    string userid = Session["UserId"].ToString();
                    merchant_master master = dataContext.merchant_master.Where(x => x.UserId == userid).FirstOrDefault();
                    int MerchantId = master.merchantid;
                    var review = dataContext.reviewmasters.Where(x => x.MerchantId == MerchantId).OrderByDescending(x => x.reviewid).FirstOrDefault();

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
                EventLog.LogErrorData("Error occurred Merchant/GetReview." + ex.Message, true);
                return Json(Global.Merchant.GetReviewException, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult GetGiftCard()
        {
            try
            {
                using (MerchantEntities dataContext = new MerchantEntities())
                {
                    string userid = Session["UserId"].ToString();
                    var giftcard = dataContext.giftcardmasters.Where(x => x.MerchantUserId == userid).OrderByDescending(x => x.SetDate).FirstOrDefault();
                    return Json(giftcard, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                //call error view here
                //log the error
                EventLog.LogErrorData("Error occurred Merchant/GetGiftCard." + ex.Message, true);
                return Json(Global.Merchant.GetGiftCardsException, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult GetGiftConditions(string GiftCardId)
        {
            try
            {
                using (MerchantEntities dataContext = new MerchantEntities())
                {
                    int no = Convert.ToInt32(GiftCardId);
                    var conditions = dataContext.giftcardconditionsmappings.Where(x => x.GiftCardId == no).OrderBy(x => x.id).ToList();

                    return Json(conditions, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                //call error view here
                //log the error
                EventLog.LogErrorData("Error occurred Merchant/GetGiftConditions." + ex.Message, true);
                return Json(Global.Merchant.GetGiftConditionsException, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult getMerchantLanguage()
        {
            try
            {
                using (MerchantEntities dataContext = new MerchantEntities())
                {
                    string userid = Session["UserId"].ToString();
                    merchant_master master = dataContext.merchant_master.Where(x => x.UserId == userid).FirstOrDefault();
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
            catch (Exception ex)
            {
                //call error view here
                //log the error
                EventLog.LogErrorData("Error occurred Merchant/getMerchantLanguage." + ex.Message, true);
                return Json("en");
            }
        }

        public JsonResult getLanguage(string countryid)
        {
            try
            {
                using (MerchantEntities dataContext = new MerchantEntities())
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
                EventLog.LogErrorData("Error occurred Merchant/GetLanguage." + ex.Message, true);
                return Json(Global.Merchant.GetLanguageException, JsonRequestBehavior.AllowGet);
            }
        }

        public string SaveChangedCountry(string countryid, string langid)
        {
            try
            {
                using (MerchantEntities dataContext = new MerchantEntities())
                {
                    var UserId = User.Identity.GetUserId();
                    var merchant = dataContext.merchant_master.Where(x => x.UserId == UserId).FirstOrDefault();
                    merchant.Country = Convert.ToInt32(countryid);
                    merchant.LanguageId = Convert.ToInt32(langid);
                    dataContext.SaveChanges();

                    HttpCookie cookie = new HttpCookie("LanguageSelected");
                    string lang = string.Empty;
                    int languageid = Convert.ToInt32(merchant.LanguageId);

                    language_master langmaster = dataContext.language_master.Where(x => x.LanguageiId == languageid).FirstOrDefault();

                    cookie.Value = langmaster.LanguageCode;
                    Response.SetCookie(cookie);

                    return Global.Merchant.CountryUpdated;
                }
            }
            catch (Exception ex)
            {
                //call error view here
                //log the error
                EventLog.LogErrorData("Error occurred Merchant/SaveChangedCountry." + ex.Message, true);
                return Global.Merchant.RegionalSettingsException;
            }
        }

        public ActionResult SendGiftCard()
        {
            using (MerchantEntities dataContext = new MerchantEntities())
            {
                string userid = Session["UserId"].ToString();
                merchant_master master = dataContext.merchant_master.Where(x => x.UserId == userid).FirstOrDefault();
                if (master != null)
                {
                    ViewBag.MerchantId = master.merchantid;

                    return View();
                }
                else
                {
                    return RedirectToAction("Login", "Account");
                }

            }
        }

        public string SaveMerchantGiftCard(giftcardmaster gfc, string lastgiftcardid)
        {
            try
            {
                var lastinsertedId = "";
                var UserId = User.Identity.GetUserId();
                if (gfc != null)
                {
                    using (MerchantEntities dataContext = new MerchantEntities())
                    {
                        //Set Gift Card
                        giftcardmaster rdm = new giftcardmaster();
                        rdm.MerchantUserId = UserId;
                        rdm.Denomination1 = gfc.Denomination1;
                        rdm.Denom1DEC = gfc.Denom1DEC;
                        rdm.Denomination2 = gfc.Denomination2;
                        rdm.Denom2DEC = gfc.Denom2DEC;
                        rdm.Denomination3 = gfc.Denomination3;
                        rdm.Denom3DEC = gfc.Denom3DEC;
                        rdm.Denomination4 = gfc.Denomination4;
                        rdm.Denom4DEC = gfc.Denom4DEC;
                        rdm.Email = gfc.Email;
                        rdm.TermsCondition = gfc.TermsCondition;
                        rdm.SetDate = DateTime.Now;
                        dataContext.giftcardmasters.Add(rdm);
                        dataContext.SaveChanges();
                        lastinsertedId = rdm.GiftCardId.ToString();
                        Session["lastinsertedGiftId"] = lastinsertedId;
                    }

                }
                return lastinsertedId;
            }
            catch (Exception ex)
            {
                //call error view here
                //log the error
                EventLog.LogErrorData("Error occurred Merchant/SaveMerchantGiftCards." + ex.Message, true);
                return Global.Merchant.SaveGiftCardException;
            }
        }



        public class clsGiftcardConditions
        {
            public int id { get; set; }
            public string condition { get; set; }
        }
        //Gift card Conditions
        // public string SaveGiftCardConditions(clsGiftcardConditions[] conditionelemnt, string lastgiftcardid)
        public string SaveGiftCardConditions(clsGiftcardConditions[] gfc)
        {
            try
            {
                var lastinsertedGiftId = Session["lastinsertedGiftId"].ToString();
                var UserId = User.Identity.GetUserId();
                if (gfc != null)
                {
                    try
                    {
                        using (MerchantEntities dataContext = new MerchantEntities())
                        {
                            foreach (var item in gfc)
                            {
                                //Save Gift Card Condition
                                giftcardconditionsmapping objgiftcardmapping = new giftcardconditionsmapping();
                                objgiftcardmapping.MerchantUserId = UserId;
                                objgiftcardmapping.GiftCardId = Convert.ToInt32(lastinsertedGiftId);
                                objgiftcardmapping.TermsCondition = item.condition;
                                dataContext.giftcardconditionsmappings.Add(objgiftcardmapping);
                                dataContext.SaveChanges();
                            }
                        }
                    }
                    catch (Exception ex)
                    {

                        EventLog.LogErrorData(ex.Message, true);

                    }
                }
                return Global.Merchant.SaveGiftCardMessage;
            }
            catch (Exception ex)
            {
                //call error view here
                //log the error
                EventLog.LogErrorData("Error occurred Merchant/SaveMerchantGiftCardsConditions." + ex.Message, true);
                return Global.Merchant.GiftCardConditionException;
            }
        }


        public JsonResult GetStates(string countryid)
        {
            try
            {
                using (MerchantEntities dataContext = new MerchantEntities())
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
                EventLog.LogErrorData("Error occurred Merchant/GetStates." + ex.Message, true);
                return Json(Global.Merchant.GetStateException, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult GetCities(string stateid)
        {
            try
            {
                using (MerchantEntities dataContext = new MerchantEntities())
                {
                    int no = Convert.ToInt32(stateid);
                    var cityList = dataContext.city_master.Where(x => x.stateid == no).OrderBy(x => x.City).ToList();

                    return Json(cityList, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                //call error view here
                //log the error
                EventLog.LogErrorData("Error occurred Merchant/GetCities." + ex.Message, true);
                return Json(Global.Merchant.GetCityException, JsonRequestBehavior.AllowGet);
            }
        }
    }
}