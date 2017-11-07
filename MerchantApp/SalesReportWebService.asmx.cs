using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;

using System.Web.Mvc;
using MerchantApp.Models;
using Microsoft.AspNet.Identity;
using AspNet.Identity.MySQL;
using MySql.Data.MySqlClient;
using System.Web.Script.Serialization;
using System.IO;
using MerchantApp.Controllers;
using System.Web.Security;
using Microsoft.Owin.Security;

using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity.Owin;
using Newtonsoft.Json;

namespace MerchantApp
{
    /// <summary>
    /// Summary description for SalesReportWebService
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    [System.Web.Script.Services.ScriptService]
    public class SalesReportWebService : System.Web.Services.WebService
    {
        MerchantCommonFunctions function = new MerchantCommonFunctions();
        

        [WebMethod]
        public string Authenticate(string username, string password)
        {
            try
            {
                HttpContext context = HttpContext.Current;

                IAuthenticationManager authManager = context.GetOwinContext().Authentication;
                ApplicationSignInManager signinmanager = context.GetOwinContext().Get<ApplicationSignInManager>();


                var result = signinmanager.PasswordSignIn(username, password, false, shouldLockout: false);
                if (result == SignInStatus.Success)
                {
                    var userId = signinmanager.AuthenticationManager.AuthenticationResponseGrant.Identity.GetUserId();
                    MerchantEntities dataContext = new MerchantEntities();
                    user currentuser = dataContext.users.Where(x => x.Id == userId).FirstOrDefault();

                    //deleted user is present in database but has allow logon = false
                    if (currentuser != null)
                    {
                        return currentuser.Id;
                    }

                    else
                    {
                        return "Error: Please check username/password";
                    }
                }
                else
                {
                    return "Error: Please check username/password";
                }
            }
            catch (Exception ex)
            {
                EventLog.LogErrorData("Error occured SalesReportWebService/Authenticate." + ex.Message, true);

                return "Error: " + ex.InnerException.Message;
            }
        }

        [WebMethod]
        public string MerchantConfigured(string userid)
        {
            try
            {
                MerchantEntities dataContext = new MerchantEntities();
                merchantsalesutilitymaster rpt = dataContext.merchantsalesutilitymasters.Where(x => x.MerchantId == userid).FirstOrDefault();
                if (rpt == null)
                {
                    rpt = new merchantsalesutilitymaster();
                    rpt.MerchantId = userid;
                    rpt.DateConfigured = DateTime.Now;
                    rpt.DateUpdated = DateTime.Now;
                    dataContext.merchantsalesutilitymasters.Add(rpt);
                    dataContext.SaveChanges();
                }
                else
                {
                    rpt.DateUpdated = DateTime.Now;
                    dataContext.SaveChanges();
                }

                return "Merchant Configured Successfully.";
            }
            catch (Exception ex)
            {
                EventLog.LogData("Error occurred while configuring merchant for sales report utility " + ex.Message, true);
                return ex.Message;
            }
        }

        [WebMethod]
        public string AddNewDECConsumer(string userid, string mobileno, string BillAmt)
        {
            try
            {
                 DateTime visitdate = DateTime.Now;
                ApplicationDbContext Consumercontext = new ApplicationDbContext("DefaultConsumerConnection");
                ApplicationDbContext Merchantcontext = new ApplicationDbContext("DefaultConnection");
                if (mobileno != null)
                {
                    var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(Consumercontext));
                    var UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(Consumercontext));

                    var MerchantUserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(Merchantcontext));

                    UserManager.UserValidator = new UserValidator<ApplicationUser>(UserManager)
                    {
                        AllowOnlyAlphanumericUserNames = false,
                        RequireUniqueEmail = true
                    };

                    MerchantEntities dataContext = new MerchantEntities();
                    if (MerchantUserManager.IsInRole(userid, "Staff"))
                    {
                        merchant_master staffmaster = dataContext.merchant_master.Where(x => x.UserId == userid).FirstOrDefault();
                        staff_master stfmgr = dataContext.staff_master.Where(x => x.StaffMasterId == staffmaster.merchantid).FirstOrDefault();
                        merchant_master owner = dataContext.merchant_master.Where(x => x.merchantid == stfmgr.MerchantId).FirstOrDefault();
                        userid = owner.UserId;
                    }
                    else if (MerchantUserManager.IsInRole(userid, "LocationManager"))
                    {
                        merchant_master branchmaster = dataContext.merchant_master.Where(x => x.UserId == userid).FirstOrDefault();
                        branch_master branch = dataContext.branch_master.Where(x => x.BranchManagerId == branchmaster.merchantid).FirstOrDefault();
                        merchant_master owner = dataContext.merchant_master.Where(x => x.merchantid == branch.MerchantId).FirstOrDefault();
                        userid = owner.UserId;
                    }

                    merchant_master master = dataContext.merchant_master.Where(x => x.UserId == userid).FirstOrDefault();
                    decimal bill = Convert.ToDecimal(BillAmt);
                    reviewmaster rvw = dataContext.reviewmasters.Where(x => x.MerchantId == master.merchantid).OrderByDescending(x => x.CreationDate).FirstOrDefault();

                    using (instadelight_consumerEntities ConsumerdataContext = new instadelight_consumerEntities())
                    {
                        var isUser = ConsumerdataContext.consumerusers.Where(u => u.UserName == mobileno).FirstOrDefault();
                        if (isUser != null)
                        {
                            function.InsertBillAmount(isUser.Id, master.UserId, bill, visitdate);


                            //Check if Mapping Exist else insert Mapping of current merchant.
                            using (MerchantEntities merchantcontext = new MerchantEntities())
                            {
                                var isConsumerMapped = merchantcontext.merchantconsumerdetails.Where(u => u.ConsumerId == isUser.Id && u.MerchantId == userid).FirstOrDefault();
                                if (isConsumerMapped == null)
                                {
                                    function.MapMerchantToConsumer(userid, isUser.Id, mobileno);
                                    consumervisitdetail visit = new consumervisitdetail();
                                    visit.ConsumerId = isUser.Id;
                                    visit.MerchantId = userid;
                                    visit.VisitDate = visitdate;
                                    ConsumerdataContext.consumervisitdetails.Add(visit);
                                    ConsumerdataContext.SaveChanges();

                                    merchantjoiningbonu bonus = dataContext.merchantjoiningbonus.Where(x => x.MerchantId == userid).OrderByDescending(x => x.DateCreated).FirstOrDefault();
                                    if (bonus != null)
                                    {
                                        using (instadelight_consumerEntities dataConsumerContext = new instadelight_consumerEntities())
                                        {
                                            if (master.RunRewardProgram == true)
                                            {
                                                if (master.RedeemProgram == "Cashback")
                                                {
                                                    consumercashbackdetail detail = new consumercashbackdetail();
                                                    detail.ConsumerId = isUser.Id;
                                                    detail.MerchantId = userid;
                                                    detail.Cashback = bonus.JoiningBonus;
                                                    detail.CashbackDate = visitdate;
                                                    dataConsumerContext.consumercashbackdetails.Add(detail);
                                                    dataConsumerContext.SaveChanges();
                                                }
                                                else
                                                {
                                                    consumerrewardpoint pts = new consumerrewardpoint();
                                                    pts.ConsumerId = isUser.Id;
                                                    pts.MerchantId = userid;
                                                    pts.Points = bonus.JoiningBonus;
                                                    pts.VisitDate = visitdate;
                                                    dataConsumerContext.consumerrewardpoints.Add(pts);
                                                    dataConsumerContext.SaveChanges();
                                                }
                                            }
                                            else
                                            {
                                                consumerrewardpoint pts = new consumerrewardpoint();
                                                pts.ConsumerId = isUser.Id;
                                                pts.MerchantId = userid;
                                                pts.Points = bonus.JoiningBonus;
                                                pts.VisitDate = visitdate;
                                                dataConsumerContext.consumerrewardpoints.Add(pts);
                                                dataConsumerContext.SaveChanges();
                                            }
                                        }
                                    }

                                    //Add cashback if applicable
                                    if (master.RunRewardProgram == true)
                                    {
                                        if (master.RedeemProgram == "Cashback")
                                        {
                                            cashbackdetail cashback = dataContext.cashbackdetails.Where(x => x.MerchantId == userid).OrderByDescending(x => x.CreationDate).FirstOrDefault();
                                            if (cashback != null)
                                            {
                                                if (cashback.IsCashBackPerTransaction != null)
                                                {
                                                    if (cashback.IsCashBackPerTransaction == true)
                                                    {
                                                        if (cashback.FixedCashBack != null)
                                                        {
                                                            if (Convert.ToInt32(cashback.FixedCashBack) > 0)
                                                            {
                                                                using (instadelight_consumerEntities dataConsumerContext = new instadelight_consumerEntities())
                                                                {
                                                                    consumercashbackdetail rs = new consumercashbackdetail();
                                                                    rs.ConsumerId = isUser.Id;
                                                                    rs.MerchantId = userid;
                                                                    rs.Cashback = Convert.ToInt32(cashback.FixedCashBack);
                                                                    rs.CashbackDate = visitdate;
                                                                    dataConsumerContext.consumercashbackdetails.Add(rs);
                                                                    dataConsumerContext.SaveChanges();
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }

                                    //Add points on bill amount
                                    rewardmaster rws = dataContext.rewardmasters.Where(x => x.MerchantId == userid).OrderByDescending(x => x.CreationDate).FirstOrDefault();
                                    if (rws != null)
                                    {
                                        if (rws.RewardRs != null && rws.RewardPoints != null)
                                        {
                                            if (bill > 0)
                                            {
                                                int points = (Convert.ToInt32(rws.RewardPoints) * Convert.ToInt32(bill)) / Convert.ToInt32(rws.RewardRs);
                                                using (instadelight_consumerEntities dataConsumerContext = new instadelight_consumerEntities())
                                                {
                                                    consumerrewardpoint pts = new consumerrewardpoint();
                                                    pts.ConsumerId = isUser.Id;
                                                    pts.MerchantId = userid;
                                                    pts.Points = points;
                                                    pts.VisitDate = visitdate;
                                                    dataConsumerContext.consumerrewardpoints.Add(pts);
                                                    dataConsumerContext.SaveChanges();
                                                }
                                            }
                                        }
                                    }

                                    //Send shared coupons.
                                    function.SendSharedCoupons(master.merchantid, userid, isUser.Id, mobileno);

                                    //Send review question                                            
                                    if (rvw != null)
                                    {
                                        //SendReviewsToDECs(rvw);
                                        function.MapMerchantReviewToConsumer(rvw.MerchantUserId, isUser.Id, rvw.reviewid);
                                    }
                                    //Send SMS to consumer for a new DEC. 'Navigation://OpenQRCodeScanner'
                                    if (mobileno.Contains('@') == false)
                                    {
                                        SMSUtility sms = new SMSUtility();
                                        string smsresult = sms.sendMessage(mobileno, "Congrats!! Welcome to " + master.MerchantName + " and thanks for registering with us. Download the app for Android at goo.gl/r5rxjj and for Apple at goo.gl/dAq4er. Your cell number is your login and your password is 123456.");
                                        consumersmsdetail smsdetails = new consumersmsdetail();
                                        smsdetails.ConsumerId = isUser.Id;
                                        smsdetails.MerchantId = userid;
                                        smsdetails.SMSEmailStatus = smsresult;
                                        smsdetails.UserName = mobileno;
                                        smsdetails.SentDate = visitdate;
                                        ConsumerdataContext.consumersmsdetails.Add(smsdetails);
                                        ConsumerdataContext.SaveChanges();


                                        master.NoOfSMS = master.NoOfSMS - 1;
                                        dataContext.SaveChanges();
                                        merchantsmsdetail smsemailcnt = dataContext.merchantsmsdetails.Where(x => x.MerchantId == userid).FirstOrDefault();
                                        if (smsemailcnt != null)
                                        {
                                            smsemailcnt.SMSCount = Convert.ToInt32(smsemailcnt.SMSCount) + 1;
                                            dataContext.SaveChanges();
                                        }
                                        else
                                        {
                                            smsemailcnt = new merchantsmsdetail();
                                            smsemailcnt.MerchantId = userid;
                                            smsemailcnt.SMSCount = 1;
                                            smsemailcnt.EmailCount = 0;
                                            dataContext.merchantsmsdetails.Add(smsemailcnt);
                                            dataContext.SaveChanges();
                                        }
                                    }
                                    else
                                    {
                                        EmailModel model = new EmailModel();
                                        model.To = mobileno;
                                        model.Email = "welcome@offertraker.com";
                                        model.Subject = "Your Digital Card is here !";

                                        model.Body = "Dear " + mobileno + ", <br /><br />" + master.MerchantName + " is happy to share its digital card with you. The digital card carries exclusive coupons and offers for you. You can also place orders through the digital card. You can share the digital card with your friends and help them enjoy exclusive offers from our business. <br />Don't forget to add your picture in your profile once you have downloaded the app! It shows up on the digital card, so that you may flaunt it!<br /> The Offertraker app is a great way to keep digital cards from various merchants in one place, so that all your offers ,coupons or points can be easily redeemed! Best of all, you can also place orders directly from the digital card.  <br />Please download the Offertraker app for Android here :goo.gl/r5rxjj and for Apple here :goo.gl/dAq4er.Your login is your email-id and your password is 123456. Enjoy the offers and stay connected! <br /><br />" + master.MerchantName + ".";

                                        SendEmail email = new SendEmail();
                                        string result = email.SendEmailToConsumer(model);
                                        consumersmsdetail smsdetails = new consumersmsdetail();
                                        smsdetails.ConsumerId = isUser.Id;
                                        smsdetails.MerchantId = userid;
                                        smsdetails.SMSEmailStatus = result;
                                        smsdetails.UserName = mobileno;
                                        smsdetails.SentDate = visitdate;
                                        ConsumerdataContext.consumersmsdetails.Add(smsdetails);
                                        ConsumerdataContext.SaveChanges();


                                        merchantsmsdetail smsemailcnt = dataContext.merchantsmsdetails.Where(x => x.MerchantId == userid).FirstOrDefault();
                                        if (smsemailcnt != null)
                                        {
                                            smsemailcnt.EmailCount = Convert.ToInt32(smsemailcnt.EmailCount) + 1;
                                            dataContext.SaveChanges();
                                        }
                                        else
                                        {
                                            smsemailcnt = new merchantsmsdetail();
                                            smsemailcnt.MerchantId = userid;
                                            smsemailcnt.SMSCount = 0;
                                            smsemailcnt.EmailCount = 1;
                                            dataContext.merchantsmsdetails.Add(smsemailcnt);
                                            dataContext.SaveChanges();
                                        }

                                    }

                                }
                                else
                                {
                                    //Increase no of visits
                                    int noofvisits = Convert.ToInt32(isConsumerMapped.NoOfVisits);
                                    isConsumerMapped.NoOfVisits = noofvisits + 1;
                                    merchantcontext.SaveChanges();


                                    consumervisitdetail visit = new consumervisitdetail();
                                    visit.ConsumerId = isUser.Id;
                                    visit.MerchantId = userid;
                                    visit.VisitDate = visitdate;
                                    ConsumerdataContext.consumervisitdetails.Add(visit);
                                    ConsumerdataContext.SaveChanges();

                                    //Add cashback if applicable
                                    if (master.RunRewardProgram == true)
                                    {
                                        if (master.RedeemProgram == "Cashback")
                                        {
                                            cashbackdetail cashback = dataContext.cashbackdetails.Where(x => x.MerchantId == userid).OrderByDescending(x => x.CreationDate).FirstOrDefault();
                                            if (cashback != null)
                                            {
                                                if (cashback.IsCashBackPerTransaction != null)
                                                {
                                                    if (cashback.IsCashBackPerTransaction == true)
                                                    {
                                                        if (cashback.FixedCashBack != null)
                                                        {
                                                            if (Convert.ToInt32(cashback.FixedCashBack) > 0)
                                                            {
                                                                using (instadelight_consumerEntities dataConsumerContext = new instadelight_consumerEntities())
                                                                {
                                                                    consumercashbackdetail rs = new consumercashbackdetail();
                                                                    rs.ConsumerId = isUser.Id;
                                                                    rs.MerchantId = userid;
                                                                    rs.Cashback = Convert.ToInt32(cashback.FixedCashBack);
                                                                    rs.CashbackDate = visitdate;
                                                                    dataConsumerContext.consumercashbackdetails.Add(rs);
                                                                    dataConsumerContext.SaveChanges();
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }

                                    //Add points on bill amount
                                    rewardmaster rws = dataContext.rewardmasters.Where(x => x.MerchantId == userid).OrderByDescending(x => x.CreationDate).FirstOrDefault();
                                    if (rws != null)
                                    {
                                        if (rws.RewardRs != null && rws.RewardPoints != null)
                                        {
                                            if (bill > 0)
                                            {
                                                int points = (Convert.ToInt32(rws.RewardPoints) * Convert.ToInt32(bill)) / Convert.ToInt32(rws.RewardRs);
                                                using (instadelight_consumerEntities dataConsumerContext = new instadelight_consumerEntities())
                                                {
                                                    consumerrewardpoint pts = new consumerrewardpoint();
                                                    pts.ConsumerId = isUser.Id;
                                                    pts.MerchantId = userid;
                                                    pts.Points = points;
                                                    pts.VisitDate = visitdate;
                                                    dataConsumerContext.consumerrewardpoints.Add(pts);
                                                    dataConsumerContext.SaveChanges();
                                                }
                                            }
                                        }
                                    }

                                    //Send shared coupons.
                                    function.SendSharedCoupons(master.merchantid, userid, isUser.Id, mobileno);

                                    //Send review question                                            
                                    if (rvw != null)
                                    {
                                        //SendReviewsToDECs(rvw);

                                        function.MapMerchantReviewToConsumer(rvw.MerchantUserId, isUser.Id, rvw.reviewid);

                                    }
                                    //Do nothing
                                    if (mobileno.Contains('@') == false)
                                    {
                                        SMSUtility sms = new SMSUtility();
                                        string smsresult = sms.sendMessage(mobileno, "Dear Customer, your " + master.MerchantName + " Digital Card is updated. Download the app for Android at goo.gl/r5rxjj and for Apple at goo.gl/dAq4er. Your cell number is your login and your password is 123456 unless you have reset it.");
                                        consumersmsdetail smsdetails = new consumersmsdetail();
                                        smsdetails.ConsumerId = isUser.Id;
                                        smsdetails.MerchantId = userid;
                                        smsdetails.SMSEmailStatus = smsresult;
                                        smsdetails.UserName = mobileno;
                                        smsdetails.SentDate = visitdate;
                                        ConsumerdataContext.consumersmsdetails.Add(smsdetails);
                                        ConsumerdataContext.SaveChanges();


                                        master.NoOfSMS = master.NoOfSMS - 1;
                                        dataContext.SaveChanges();
                                        merchantsmsdetail smsemailcnt = dataContext.merchantsmsdetails.Where(x => x.MerchantId == userid).FirstOrDefault();
                                        if (smsemailcnt != null)
                                        {
                                            smsemailcnt.SMSCount = Convert.ToInt32(smsemailcnt.SMSCount) + 1;
                                            dataContext.SaveChanges();
                                        }
                                        else
                                        {
                                            smsemailcnt = new merchantsmsdetail();
                                            smsemailcnt.MerchantId = userid;
                                            smsemailcnt.SMSCount = 1;
                                            smsemailcnt.EmailCount = 0;
                                            dataContext.merchantsmsdetails.Add(smsemailcnt);
                                            dataContext.SaveChanges();
                                        }
                                    }
                                    else
                                    {
                                        EmailModel model = new EmailModel();
                                        model.To = mobileno;
                                        model.Email = "welcome@offertraker.com";
                                        model.Subject = "Your Digital Card is here !";

                                        model.Body = "Dear " + mobileno + ", <br /><br />" + master.MerchantName + " is happy to share its digital card with you. The digital card carries exclusive coupons and offers for you. You can also place orders through the digital card. You can share the digital card with your friends and help them enjoy exclusive offers from our business. <br />Don't forget to add your picture in your profile once you have downloaded the app! It shows up on the digital card, so that you may flaunt it!<br /> The Offertraker app is a great way to keep digital cards from various merchants in one place, so that all your offers ,coupons or points can be easily redeemed! Best of all, you can also place orders directly from the digital card.  <br />Please download the Offertraker app for Android here :goo.gl/r5rxjj and for Apple here :goo.gl/dAq4er. Your login is your email-id and your password is 123456. Enjoy the offers and stay connected! <br /><br />" + master.MerchantName + ".";
                                        SendEmail email = new SendEmail();
                                        string result = email.SendEmailToConsumer(model);
                                        consumersmsdetail smsdetails = new consumersmsdetail();
                                        smsdetails.ConsumerId = isUser.Id;
                                        smsdetails.MerchantId = userid;
                                        smsdetails.SMSEmailStatus = result;
                                        smsdetails.UserName = mobileno;
                                        smsdetails.SentDate =visitdate;
                                        ConsumerdataContext.consumersmsdetails.Add(smsdetails);
                                        ConsumerdataContext.SaveChanges();


                                        merchantsmsdetail smsemailcnt = dataContext.merchantsmsdetails.Where(x => x.MerchantId == userid).FirstOrDefault();
                                        if (smsemailcnt != null)
                                        {
                                            smsemailcnt.EmailCount = Convert.ToInt32(smsemailcnt.EmailCount) + 1;
                                            dataContext.SaveChanges();
                                        }
                                        else
                                        {
                                            smsemailcnt = new merchantsmsdetail();
                                            smsemailcnt.MerchantId = userid;
                                            smsemailcnt.SMSCount = 0;
                                            smsemailcnt.EmailCount = 1;
                                            dataContext.merchantsmsdetails.Add(smsemailcnt);
                                            dataContext.SaveChanges();
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            //Create consumer and corresponding mapping in merchantentities.
                            var user = new ApplicationUser();
                            user.UserName = mobileno;
                            string userPWD = "123456";
                            user.FirstName = "C";
                            user.LastName = "";
                            if (mobileno.Contains('@') == false)
                            {
                                user.Phone = mobileno;
                                user.PhoneNumber = mobileno;
                                user.Email = "test@test.com";
                            }
                            else
                            {
                                user.Email = mobileno;
                            }
                            var chkUser = UserManager.Create(user, userPWD);

                            ////Add default User to Role Admin   
                            if (chkUser.Succeeded)
                            {
                                var rolesForUser = UserManager.GetRoles(user.Id);
                                if (!rolesForUser.Contains("Consumer"))
                                {
                                    UserManager.AddToRole(user.Id, "Consumer");
                                }

                                //Insert bill amount in bill details
                                function.InsertBillAmount(user.Id, master.UserId, bill, visitdate);


                                //Added new code block for consumermaster
                                using (instadelight_consumerEntities consumerContext = new instadelight_consumerEntities())
                                {
                                    function.InsertNewConsumer(user.Id, mobileno);
                                    merchantjoiningbonu bonus = dataContext.merchantjoiningbonus.Where(x => x.MerchantId == userid).OrderByDescending(x => x.DateCreated).FirstOrDefault();
                                    if (bonus != null)
                                    {
                                        using (instadelight_consumerEntities dataConsumerContext = new instadelight_consumerEntities())
                                        {
                                            if (master.RunRewardProgram == true)
                                            {
                                                if (master.RedeemProgram == "Cashback")
                                                {
                                                    consumercashbackdetail detail = new consumercashbackdetail();
                                                    detail.ConsumerId = isUser.Id;
                                                    detail.MerchantId = userid;
                                                    detail.Cashback = bonus.JoiningBonus;
                                                    detail.CashbackDate = visitdate;
                                                    dataConsumerContext.consumercashbackdetails.Add(detail);
                                                    dataConsumerContext.SaveChanges();
                                                }
                                                else
                                                {
                                                    consumerrewardpoint pts = new consumerrewardpoint();
                                                    pts.ConsumerId = isUser.Id;
                                                    pts.MerchantId = userid;
                                                    pts.Points = bonus.JoiningBonus;
                                                    pts.VisitDate = visitdate;
                                                    dataConsumerContext.consumerrewardpoints.Add(pts);
                                                    dataConsumerContext.SaveChanges();
                                                }
                                            }
                                            else
                                            {
                                                consumerrewardpoint pts = new consumerrewardpoint();
                                                pts.ConsumerId = isUser.Id;
                                                pts.MerchantId = userid;
                                                pts.Points = bonus.JoiningBonus;
                                                pts.VisitDate = visitdate;
                                                dataConsumerContext.consumerrewardpoints.Add(pts);
                                                dataConsumerContext.SaveChanges();
                                            }
                                        }
                                    }
                                }

                                using (MerchantEntities merchantcontext = new MerchantEntities())
                                {
                                    var isConsumerMapped = merchantcontext.merchantconsumerdetails.Where(u => u.ConsumerId == user.Id && u.MerchantId == userid).FirstOrDefault();
                                    if (isConsumerMapped == null)
                                    {
                                        function.MapMerchantToConsumer(userid, user.Id, mobileno);

                                        consumervisitdetail visit = new consumervisitdetail();
                                        visit.ConsumerId = user.Id;
                                        visit.MerchantId = userid;
                                        visit.VisitDate = visitdate;
                                        ConsumerdataContext.consumervisitdetails.Add(visit);
                                        ConsumerdataContext.SaveChanges();

                                        //Add cashback if applicable
                                        if (master.RunRewardProgram == true)
                                        {
                                            if (master.RedeemProgram == "Cashback")
                                            {
                                                cashbackdetail cashback = dataContext.cashbackdetails.Where(x => x.MerchantId == userid).OrderByDescending(x => x.CreationDate).FirstOrDefault();
                                                if (cashback != null)
                                                {
                                                    if (cashback.IsCashBackPerTransaction != null)
                                                    {
                                                        if (cashback.IsCashBackPerTransaction == true)
                                                        {
                                                            if (cashback.FixedCashBack != null)
                                                            {
                                                                if (Convert.ToInt32(cashback.FixedCashBack) > 0)
                                                                {
                                                                    using (instadelight_consumerEntities dataConsumerContext = new instadelight_consumerEntities())
                                                                    {
                                                                        consumercashbackdetail rs = new consumercashbackdetail();
                                                                        rs.ConsumerId = user.Id;
                                                                        rs.MerchantId = userid;
                                                                        rs.Cashback = Convert.ToInt32(cashback.FixedCashBack);
                                                                        rs.CashbackDate = visitdate;
                                                                        dataConsumerContext.consumercashbackdetails.Add(rs);
                                                                        dataConsumerContext.SaveChanges();
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }

                                        //Add points on bill amount
                                        rewardmaster rws = dataContext.rewardmasters.Where(x => x.MerchantId == userid).OrderByDescending(x => x.CreationDate).FirstOrDefault();
                                        if (rws != null)
                                        {
                                            if (rws.RewardRs != null && rws.RewardPoints != null)
                                            {
                                                if (bill > 0)
                                                {
                                                    int points = (Convert.ToInt32(rws.RewardPoints) * Convert.ToInt32(bill)) / Convert.ToInt32(rws.RewardRs);
                                                    using (instadelight_consumerEntities dataConsumerContext = new instadelight_consumerEntities())
                                                    {
                                                        consumerrewardpoint pts = new consumerrewardpoint();
                                                        pts.ConsumerId = user.Id;
                                                        pts.MerchantId = userid;
                                                        pts.Points = points;
                                                        pts.VisitDate = visitdate;
                                                        dataConsumerContext.consumerrewardpoints.Add(pts);
                                                        dataConsumerContext.SaveChanges();
                                                    }
                                                }
                                            }
                                        }

                                        //Send shared coupons.
                                        function.SendSharedCoupons(master.merchantid, userid, user.Id, mobileno);

                                        //Send review question                                            
                                        if (rvw != null)
                                        {
                                            // SendReviewsToDECs(rvw);
                                            function.MapMerchantReviewToConsumer(rvw.MerchantUserId, user.Id, rvw.reviewid);
                                        }

                                        if (mobileno.Contains('@') == false)
                                        {
                                            SMSUtility sms = new SMSUtility();
                                            string smsresult = sms.sendMessage(mobileno, "Congrats!! Welcome to " + master.MerchantName + " and thanks for registering with us. Download the app for Android at goo.gl/r5rxjj and for Apple at goo.gl/dAq4er. Your cell number is your login and your password is 123456.");
                                            consumersmsdetail smsdetails = new consumersmsdetail();
                                            smsdetails.ConsumerId = user.Id;
                                            smsdetails.MerchantId = userid;
                                            smsdetails.SMSEmailStatus = smsresult;
                                            smsdetails.UserName = mobileno;
                                            smsdetails.SentDate = visitdate;
                                            ConsumerdataContext.consumersmsdetails.Add(smsdetails);
                                            ConsumerdataContext.SaveChanges();


                                            master.NoOfSMS = master.NoOfSMS - 1;
                                            dataContext.SaveChanges();
                                            merchantsmsdetail smsemailcnt = dataContext.merchantsmsdetails.Where(x => x.MerchantId == userid).FirstOrDefault();
                                            if (smsemailcnt != null)
                                            {
                                                smsemailcnt.SMSCount = Convert.ToInt32(smsemailcnt.SMSCount) + 1;
                                                dataContext.SaveChanges();
                                            }
                                            else
                                            {
                                                smsemailcnt = new merchantsmsdetail();
                                                smsemailcnt.MerchantId = userid;
                                                smsemailcnt.SMSCount = 1;
                                                smsemailcnt.EmailCount = 0;
                                                dataContext.merchantsmsdetails.Add(smsemailcnt);
                                                dataContext.SaveChanges();
                                            }
                                        }
                                        else
                                        {
                                            EmailModel model = new EmailModel();
                                            model.To = mobileno;
                                            model.Email = "welcome@offertraker.com";
                                            model.Subject = "Your Digital Card is here !";

                                            model.Body = "Dear " + mobileno + ", <br /><br />" + master.MerchantName + " is happy to share its digital card with you. The digital card carries exclusive coupons and offers for you. You can also place orders through the digital card. You can share the digital card with your friends and help them enjoy exclusive offers from our business. <br />Don't forget to add your picture in your profile once you have downloaded the app! It shows up on the digital card, so that you may flaunt it!<br /> The Offertraker app is a great way to keep digital cards from various merchants in one place, so that all your offers ,coupons or points can be easily redeemed! Best of all, you can also place orders directly from the digital card.  <br />Please download the Offertraker app for Android here :goo.gl/r5rxjj and for Apple here :goo.gl/dAq4er. Your login is your email-id and your password is 123456. Enjoy the offers and stay connected! <br /><br />" + master.MerchantName + ".";
                                            SendEmail email = new SendEmail();
                                            string result = email.SendEmailToConsumer(model);
                                            consumersmsdetail smsdetails = new consumersmsdetail();
                                            smsdetails.ConsumerId = user.Id;
                                            smsdetails.MerchantId = userid;
                                            smsdetails.SMSEmailStatus = result;
                                            smsdetails.UserName = mobileno;
                                            smsdetails.SentDate = visitdate;
                                            ConsumerdataContext.consumersmsdetails.Add(smsdetails);
                                            ConsumerdataContext.SaveChanges();


                                            merchantsmsdetail smsemailcnt = dataContext.merchantsmsdetails.Where(x => x.MerchantId == userid).FirstOrDefault();
                                            if (smsemailcnt != null)
                                            {
                                                smsemailcnt.EmailCount = Convert.ToInt32(smsemailcnt.EmailCount) + 1;
                                                dataContext.SaveChanges();
                                            }
                                            else
                                            {
                                                smsemailcnt = new merchantsmsdetail();
                                                smsemailcnt.MerchantId = userid;
                                                smsemailcnt.SMSCount = 0;
                                                smsemailcnt.EmailCount = 1;
                                                dataContext.merchantsmsdetails.Add(smsemailcnt);
                                                dataContext.SaveChanges();
                                            }
                                        }
                                    }
                                    else
                                    {
                                        int noofvisits = Convert.ToInt32(isConsumerMapped.NoOfVisits);
                                        isConsumerMapped.NoOfVisits = noofvisits + 1;
                                        merchantcontext.SaveChanges();


                                        consumervisitdetail visit = new consumervisitdetail();
                                        visit.ConsumerId = user.Id;
                                        visit.MerchantId = userid;
                                        visit.VisitDate = visitdate;
                                        ConsumerdataContext.consumervisitdetails.Add(visit);
                                        ConsumerdataContext.SaveChanges();

                                        //Add cashback if applicable
                                        if (master.RunRewardProgram == true)
                                        {
                                            if (master.RedeemProgram == "Cashback")
                                            {
                                                cashbackdetail cashback = dataContext.cashbackdetails.Where(x => x.MerchantId == userid).OrderByDescending(x => x.CreationDate).FirstOrDefault();
                                                if (cashback != null)
                                                {
                                                    if (cashback.IsCashBackPerTransaction != null)
                                                    {
                                                        if (cashback.IsCashBackPerTransaction == true)
                                                        {
                                                            if (cashback.FixedCashBack != null)
                                                            {
                                                                if (Convert.ToInt32(cashback.FixedCashBack) > 0)
                                                                {
                                                                    using (instadelight_consumerEntities dataConsumerContext = new instadelight_consumerEntities())
                                                                    {
                                                                        consumercashbackdetail rs = new consumercashbackdetail();
                                                                        rs.ConsumerId = user.Id;
                                                                        rs.MerchantId = userid;
                                                                        rs.Cashback = Convert.ToInt32(cashback.FixedCashBack);
                                                                        rs.CashbackDate = visitdate;
                                                                        dataConsumerContext.consumercashbackdetails.Add(rs);
                                                                        dataConsumerContext.SaveChanges();
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }

                                        //Add points on bill amount
                                        rewardmaster rws = dataContext.rewardmasters.Where(x => x.MerchantId == userid).OrderByDescending(x => x.CreationDate).FirstOrDefault();
                                        if (rws != null)
                                        {
                                            if (rws.RewardRs != null && rws.RewardPoints != null)
                                            {
                                                if (bill > 0)
                                                {
                                                    int points = (Convert.ToInt32(rws.RewardPoints) * Convert.ToInt32(bill)) / Convert.ToInt32(rws.RewardRs);
                                                    using (instadelight_consumerEntities dataConsumerContext = new instadelight_consumerEntities())
                                                    {
                                                        consumerrewardpoint pts = new consumerrewardpoint();
                                                        pts.ConsumerId = user.Id;
                                                        pts.MerchantId = userid;
                                                        pts.Points = points;
                                                        pts.VisitDate = visitdate;
                                                        dataConsumerContext.consumerrewardpoints.Add(pts);
                                                        dataConsumerContext.SaveChanges();
                                                    }
                                                }
                                            }
                                        }
                                        //Send shared coupons.
                                        function.SendSharedCoupons(master.merchantid, userid, user.Id, mobileno);

                                        //Send review question                                            
                                        if (rvw != null)
                                        {
                                            //SendReviewsToDECs(rvw);
                                            function.MapMerchantReviewToConsumer(rvw.MerchantUserId, user.Id, rvw.reviewid);
                                        }
                                        //Do nothing
                                    }
                                }
                            }
                            else
                            {
                                return "Not able to register Consumer";
                            }
                        }
                    }
                }
                else
                {
                    return "Invalid Consumer Details";
                }
                return "Consumer Added Successfully.";
            }
            catch (Exception ex)
            {
                //call error view here
                //log the error
                EventLog.LogErrorData("Error occured SalesReportWebService/AddNewDECConsumer." + ex.Message, true);
                return "Error occured while sending DEC to consumer";
            }
        }

        [WebMethod]
        public string AddSalesReportLog(string userid, string filename)
        {
            try
            {
                MerchantEntities dataContext = new MerchantEntities();
                merchantsalesreportlog log = new merchantsalesreportlog();
                log.MerchantId = userid;
                log.FileName = filename;
                log.TimeStamp = DateTime.Now;

                dataContext.merchantsalesreportlogs.Add(log);
                dataContext.SaveChanges();
                return "File Logged Successfully.";

            }
            catch (Exception ex)
            {
                EventLog.LogData("Error occurred while adding log file in sales report log " + ex.Message, true);
                return ex.Message;
            }

        }
    }

}
