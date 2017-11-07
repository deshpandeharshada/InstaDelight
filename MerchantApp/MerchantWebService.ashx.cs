using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
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
using System.Data;

namespace MerchantApp
{
    /// <summary>
    /// Summary description for MerchantWebService
    /// </summary>
    public class MerchantWebService : IHttpHandler
    {
        ApplicationDbContext Consumercontext = new ApplicationDbContext("DefaultConsumerConnection");
        ApplicationDbContext Merchantcontext = new ApplicationDbContext("DefaultConnection");

        MerchantCommonFunctions function = new MerchantCommonFunctions();

        public void ProcessRequest(HttpContext context)
        {
            string action = "";
            if (context.Request["Action"] != null)
            {
                action = context.Request["Action"].ToString();
            }
            if (action == "Login")
            {
                try
                {
                    IAuthenticationManager authManager = context.GetOwinContext().Authentication;
                    ApplicationSignInManager signinmanager = context.GetOwinContext().Get<ApplicationSignInManager>();
                    //string username = Convert.ToString(context.Request["username"]);
                    //string pwd = Convert.ToString(context.Request["password"]);

                    string authorizationHeader = context.Request.Headers["Authorization"];
                    string[] credentials = authorizationHeader.Split(':');
                    if (credentials == null)
                    {
                        JavaScriptSerializer js = new JavaScriptSerializer();
                        js.MaxJsonLength = Int32.MaxValue;

                        string str = js.Serialize("Please pass user credentials in header.");
                        context.Response.Write(str);
                    }
                    else if (credentials.Length != 2)
                    {
                        JavaScriptSerializer js = new JavaScriptSerializer();
                        js.MaxJsonLength = Int32.MaxValue;

                        string str = js.Serialize("Please pass user credentials in header.");
                        context.Response.Write(str);
                    }
                    else
                    {
                        string username = Convert.ToString(credentials[0]);
                        string pwd = Convert.ToString(credentials[1]);
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


                        string loginstatus = "";
                        var result = signinmanager.PasswordSignIn(username, pwd, false, shouldLockout: false);
                        if (result == SignInStatus.Success)
                        {
                            var userId = signinmanager.AuthenticationManager.AuthenticationResponseGrant.Identity.GetUserId();
                            MerchantEntities dataContext = new MerchantEntities();
                            user currentuser = dataContext.users.Where(x => x.Id == userId).FirstOrDefault();

                            //deleted user is present in database but has allow logon = false
                            if (currentuser != null)
                            {
                                //add user id, role and name in session variables
                                if (currentuser.ChangePassword == 1)
                                {
                                    loginstatus = "ResetPassword";
                                    JavaScriptSerializer js = new JavaScriptSerializer();
                                    js.MaxJsonLength = Int32.MaxValue;

                                    string str = js.Serialize(loginstatus);
                                    context.Response.Write(str);
                                }
                                else
                                {
                                    merchant_master merchant = dataContext.merchant_master.Where(x => x.UserId == userId).FirstOrDefault();
                                    loginstatus = merchant.UserId;
                                    JavaScriptSerializer js = new JavaScriptSerializer();
                                    js.MaxJsonLength = Int32.MaxValue;

                                    string str = js.Serialize(loginstatus);
                                    context.Response.Write(str);
                                }
                            }
                            else
                            {
                                JavaScriptSerializer js = new JavaScriptSerializer();
                                js.MaxJsonLength = Int32.MaxValue;

                                string str = js.Serialize("Unauthorized Access.");
                                context.Response.Write(str);
                            }
                        }
                        else
                        {
                            JavaScriptSerializer js = new JavaScriptSerializer();
                            js.MaxJsonLength = Int32.MaxValue;

                            string str = js.Serialize("Unauthorized Access.");
                            context.Response.Write(str);
                        }
                    }
                }
                catch (Exception ex)
                {
                    EventLog.LogErrorData("Error occured MerchantWebService/Login." + ex.Message, true);
                    JavaScriptSerializer js = new JavaScriptSerializer();
                    js.MaxJsonLength = Int32.MaxValue;

                    string str = js.Serialize("Error occured while logging in from web service.");
                    context.Response.Write(str);
                }
            }
            else if (action == "ResetPassword")
            {
                try
                {
                    string phonenumber = Convert.ToString(context.Request["phonenumber"]);
                    string newpwd = Convert.ToString(context.Request["newpassword"]);
                    if (phonenumber.Contains('@') == false)
                    {
                        string[] PhonewithCode = phonenumber.Split(' ');
                        if (PhonewithCode.Length > 0)
                        {
                            if (PhonewithCode.Length == 3)
                            {
                                phonenumber = "+" + PhonewithCode[1] + " " + PhonewithCode[2].ToString();
                            }
                        }
                    }

                    ApplicationUserManager UserManager = context.GetOwinContext().Get<ApplicationUserManager>();
                    var user = UserManager.FindByName(phonenumber);
                    if (user == null)
                    {
                        // Don't reveal that the user does not exist or is not confirmed
                        EventLog.LogErrorData("Error occurred MerchantWebService/ResetPassword. User does not exist.", true);
                        JavaScriptSerializer js = new JavaScriptSerializer();
                        js.MaxJsonLength = Int32.MaxValue;

                        string str = js.Serialize("User " + phonenumber + " does not exist.");
                        context.Response.Write(str);
                    }
                    string code = UserManager.GeneratePasswordResetToken(user.Id);
                    var result = UserManager.ResetPassword(user.Id, code, newpwd);
                    if (result.Succeeded)
                    {
                        MerchantEntities dataContext = new MerchantEntities();
                        user currentuser = dataContext.users.Where(x => x.Id == user.Id).FirstOrDefault();

                        //deleted user is present in database but has allow logon = false
                        if (currentuser != null)
                        {
                            if (currentuser.ChangePassword == 1)
                            {
                                currentuser.ChangePassword = 0;
                                dataContext.SaveChanges();
                            }
                        }

                        JavaScriptSerializer js = new JavaScriptSerializer();
                        js.MaxJsonLength = Int32.MaxValue;

                        string str = js.Serialize(Global.Merchant.ResetMessage);
                        context.Response.Write(str);
                    }
                }
                catch (Exception ex)
                {
                    EventLog.LogErrorData("Error occurred MerchantWebService/ResetPassword." + ex.Message, true);
                    JavaScriptSerializer js = new JavaScriptSerializer();
                    js.MaxJsonLength = Int32.MaxValue;

                    string str = js.Serialize("Error occurred while resetting from web service.");
                    context.Response.Write(str);
                }
            }
            else if (action == "ForgotPassword")
            {
                try
                {
                    string phonenumber = Convert.ToString(context.Request["phonenumber"]);
                    if (phonenumber.Contains('@') == false)
                    {
                        string[] PhonewithCode = phonenumber.Split(' ');
                        if (PhonewithCode.Length > 0)
                        {
                            if (PhonewithCode.Length == 3)
                            {
                                phonenumber = "+" + PhonewithCode[1] + " " + PhonewithCode[2].ToString();
                            }
                        }
                    }


                    ForgotPasswordViewModel model = new ForgotPasswordViewModel();
                    model.Phone = phonenumber;

                    ApplicationUserManager UserManager = context.GetOwinContext().Get<ApplicationUserManager>();
                    var user = UserManager.FindByName(model.Phone);
                    if (user == null)
                    {
                        // Don't reveal that the user does not exist or is not confirmed
                        EventLog.LogErrorData("Error occured MerchantWebService/ForgotPassword. User does not exist.", true);
                        JavaScriptSerializer js = new JavaScriptSerializer();
                        js.MaxJsonLength = Int32.MaxValue;

                        string str = js.Serialize("User " + phonenumber + " does not exist.");
                        context.Response.Write(str);
                    }

                    string code = UserManager.GeneratePasswordResetToken(user.Id);

                    string newpwd = RandomNumber.GenerateRandomOTP(6);

                    var result = UserManager.ResetPassword(user.Id, code, newpwd);
                    if (result.Succeeded)
                    {
                        MerchantEntities dataContext = new MerchantEntities();
                        user currentuser = dataContext.users.Where(x => x.Id == user.Id).FirstOrDefault();

                        //deleted user is present in database but has allow logon = false
                        if (currentuser != null)
                        {
                            if (currentuser.ChangePassword == 0)
                            {
                                currentuser.ChangePassword = 1;
                                dataContext.SaveChanges();
                            }
                        }

                        if (model.Phone.Contains('@') == false)
                        {
                            SMSUtility sms = new SMSUtility();
                            string smsresult = sms.sendMessage(model.Phone, "Dear Customer, your password to access Offertraker Business App is " + newpwd);

                            EventLog.LogData("New password " + newpwd, true);

                            if (smsresult.Contains("SMS sent successfully"))
                            {
                                JavaScriptSerializer js = new JavaScriptSerializer();
                                js.MaxJsonLength = Int32.MaxValue;

                                string str = js.Serialize(@Global.Merchant.ForgotPasswordMessage);
                                context.Response.Write(str);
                            }
                        }
                        else
                        {
                            EmailModel mailmodel = new EmailModel();
                            mailmodel.To = model.Phone;
                            mailmodel.Email = "no-reply@offertraker.com";
                            mailmodel.Subject = "Your password has been reset";
                            mailmodel.Body = "Dear " + model.Phone + ",<br /><br /> Your password has been reset successfully. Your new login credentials are : <br /> User Name : " + user.UserName + " <br /> Password: " + newpwd + " <br /> Regards, <br /><br />Offertraker team";
                            SendEmail email = new SendEmail();
                            string mailresult = email.SendEmailToConsumer(mailmodel);
                            if (mailresult.Contains("Email sent"))
                            {
                                JavaScriptSerializer js = new JavaScriptSerializer();
                                js.MaxJsonLength = Int32.MaxValue;

                                string str = js.Serialize(@Global.Merchant.ForgotPasswordMessage);
                                context.Response.Write(str);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    EventLog.LogErrorData("Error occured MerchantWebService/ForgotPassword." + ex.Message, true);
                    JavaScriptSerializer js = new JavaScriptSerializer();
                    js.MaxJsonLength = Int32.MaxValue;

                    string str = js.Serialize("Error occured while setting new password from web service.");
                    context.Response.Write(str);
                }
            }
            else if (action == "GetCategories")
            {
                try
                {
                    using (MerchantEntities dataContext = new MerchantEntities())
                    {
                        var categoryList = dataContext.business_category_master.OrderBy(x => x.CategoryName).ToList();

                        JavaScriptSerializer js = new JavaScriptSerializer();
                        js.MaxJsonLength = Int32.MaxValue;

                        string str = js.Serialize(categoryList);
                        context.Response.Write(str);
                    }
                }
                catch (Exception ex)
                {
                    EventLog.LogErrorData("Error occured MerchantWebService/GetCategories." + ex.Message, true);
                    JavaScriptSerializer js = new JavaScriptSerializer();
                    js.MaxJsonLength = Int32.MaxValue;

                    string str = js.Serialize("Error occured while geting category list from web service.");
                    context.Response.Write(str);
                }
            }
            else if (action == "getCurrency")
            {
                try
                {
                    using (MerchantEntities dataContext = new MerchantEntities())
                    {
                        country_master ctry = dataContext.country_master.FirstOrDefault();

                        JavaScriptSerializer js = new JavaScriptSerializer();
                        js.MaxJsonLength = Int32.MaxValue;

                        string str = js.Serialize(ctry);
                        context.Response.Write(str);
                    }
                }
                catch (Exception ex)
                {
                    //call error view here
                    //log the error
                    EventLog.LogErrorData("Error occured MerchantWebService/GetCurrency." + ex.Message, true);
                    JavaScriptSerializer js = new JavaScriptSerializer();
                    js.MaxJsonLength = Int32.MaxValue;

                    string str = js.Serialize("Error occured while geting currency from web service.");
                    context.Response.Write(str);
                }
            }
            else if (action == "GetMerchantById")
            {
                string MerchantId = Convert.ToString(context.Request["MerchantId"]);
                try
                {
                    using (MerchantEntities dataContext = new MerchantEntities())
                    {
                        int no = Convert.ToInt32(MerchantId);
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

                        }

                        JavaScriptSerializer js = new JavaScriptSerializer();
                        js.MaxJsonLength = Int32.MaxValue;

                        string str = js.Serialize(merchant);
                        context.Response.Write(str);
                    }
                }
                catch (Exception ex)
                {
                    //call error view here
                    //log the error
                    EventLog.LogErrorData("Error occured MerchantWebService/GetMerchantById." + ex.Message, true);
                    JavaScriptSerializer js = new JavaScriptSerializer();
                    js.MaxJsonLength = Int32.MaxValue;

                    string str = js.Serialize("Error occured while geting merchant details by id from web service.");
                    context.Response.Write(str);
                }
            }
            else if (action == "getMerchantRewards")
            {
                string MerchantId = Convert.ToString(context.Request["MerchantId"]);
                try
                {
                    using (MerchantEntities dataContext = new MerchantEntities())
                    {
                        var merchantRewards = dataContext.rewardmasters.Where(x => x.MerchantId == MerchantId).OrderByDescending(x => x.CreationDate).FirstOrDefault();
                        JavaScriptSerializer js = new JavaScriptSerializer();
                        js.MaxJsonLength = Int32.MaxValue;

                        string str = js.Serialize(merchantRewards);
                        context.Response.Write(str);
                    }
                }
                catch (Exception ex)
                {
                    //call error view here
                    //log the error
                    EventLog.LogErrorData("Error occured MerchantWebService/getMerchantRewards." + ex.Message, true);
                    JavaScriptSerializer js = new JavaScriptSerializer();
                    js.MaxJsonLength = Int32.MaxValue;

                    string str = js.Serialize("Error occured while geting merchant's rewards from web service.");
                    context.Response.Write(str);
                }
            }
            else if (action == "getMerchantRedeems")
            {
                string MerchantId = Convert.ToString(context.Request["MerchantId"]);
                try
                {
                    using (MerchantEntities dataContext = new MerchantEntities())
                    {
                        var merchantRedeem = dataContext.redeemmasters.Where(x => x.MerchantId == MerchantId).OrderByDescending(x => x.CreationDate).FirstOrDefault();
                        JavaScriptSerializer js = new JavaScriptSerializer();
                        js.MaxJsonLength = Int32.MaxValue;

                        string str = js.Serialize(merchantRedeem);
                        context.Response.Write(str);
                    }
                }
                catch (Exception ex)
                {
                    //call error view here
                    //log the error
                    EventLog.LogErrorData("Error occured MerchantWebService/getMerchantRedeems." + ex.Message, true);
                    JavaScriptSerializer js = new JavaScriptSerializer();
                    js.MaxJsonLength = Int32.MaxValue;

                    string str = js.Serialize("Error occured while geting merchant's redeem from web service.");
                    context.Response.Write(str);
                }
            }
            else if (action == "GetCountrycode")
            {
                try
                {
                    using (MerchantEntities dataContext = new MerchantEntities())
                    {
                        int countrycode = 0;
                        country_master country = dataContext.country_master.FirstOrDefault();

                        countrycode = Convert.ToInt32(country.CountryCode);
                        JavaScriptSerializer js = new JavaScriptSerializer();
                        js.MaxJsonLength = Int32.MaxValue;

                        string str = js.Serialize(countrycode);
                        context.Response.Write(str);
                    }
                }
                catch (Exception ex)
                {
                    //call error view here
                    //log the error
                    EventLog.LogErrorData("Error occured MerchantWebService/GetCountryCode." + ex.Message, true);
                    JavaScriptSerializer js = new JavaScriptSerializer();
                    js.MaxJsonLength = Int32.MaxValue;

                    string str = js.Serialize("Error occured while geting country code from web service.");
                    context.Response.Write(str);
                }
            }
            else if (action == "RedeemPoints")
            {
                try
                {
                    string userid = Convert.ToString(context.Request["userid"]);
                    string RedeemPoints = Convert.ToString(context.Request["RedeemPoints"]);
                    string CustPhoneNumber = Convert.ToString(context.Request["CustPhoneNumber"]);
                    ApplicationUserManager UserManager = context.GetOwinContext().Get<ApplicationUserManager>();


                    if (CustPhoneNumber.Contains('@') == false)
                    {
                        string[] PhonewithCode = CustPhoneNumber.Split(' ');
                        if (PhonewithCode.Length > 0)
                        {
                            if (PhonewithCode.Length == 3)
                            {
                                CustPhoneNumber = "+" + PhonewithCode[1] + " " + PhonewithCode[2].ToString();
                            }
                        }
                    }

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
                                EventLog.LogErrorData("Error occured MerchantWebService/RedeemPoints." + Global.Merchant.ConsumerDoesNotExistErrorMessage, true);
                                JavaScriptSerializer js = new JavaScriptSerializer();
                                js.MaxJsonLength = Int32.MaxValue;

                                string str = js.Serialize(Global.Merchant.ConsumerDoesNotExistErrorMessage);
                                context.Response.Write(str);
                            }
                        }

                        using (MerchantEntities dataContext = new MerchantEntities())
                        {
                            if (UserManager.IsInRole(userid, "Staff"))
                            {
                                merchant_master staffmaster = dataContext.merchant_master.Where(x => x.UserId == userid).FirstOrDefault();
                                staff_master stfmgr = dataContext.staff_master.Where(x => x.StaffMasterId == staffmaster.merchantid).FirstOrDefault();
                                merchant_master owner = dataContext.merchant_master.Where(x => x.merchantid == stfmgr.MerchantId).FirstOrDefault();
                                userid = owner.UserId;
                            }
                            else if (UserManager.IsInRole(userid, "LocationManager"))
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
                                JavaScriptSerializer js = new JavaScriptSerializer();
                                js.MaxJsonLength = Int32.MaxValue;
                                string str = js.Serialize(Global.Merchant.Consumer + CustPhoneNumber + Global.Merchant.NotRegisteredErrorMessage + master.MerchantName);
                                context.Response.Write(str);
                            }

                            if (Convert.ToBoolean(master.RunRewardProgram) == true)
                            {
                                if (master.RedeemProgram == "Points")
                                {
                                    redeemmaster red = dataContext.redeemmasters.Where(x => x.MerchantId == userid).FirstOrDefault();
                                    if (red == null)
                                    {
                                        JavaScriptSerializer js = new JavaScriptSerializer();
                                        js.MaxJsonLength = Int32.MaxValue;
                                        string str = js.Serialize(Global.Merchant.NoRedeemFormulaErrorMessage + master.MerchantName.ToString());
                                        context.Response.Write(str);
                                    }

                                    using (instadelight_consumerEntities ConsumerdataContext = new instadelight_consumerEntities())
                                    {
                                        int pts = Convert.ToInt32(ConsumerdataContext.consumerrewardpoints.Where(x => x.ConsumerId == consumeruserid && x.MerchantId == userid).Select(x => x.Points).Sum());
                                        int redeempts = Convert.ToInt32(ConsumerdataContext.consumerredeemdetails.Where(x => x.ConsumerId == consumeruserid && x.MerchantId == userid).Select(x => x.PointsRedeemed).Sum());
                                        int availablepts = pts - redeempts;


                                        if (availablepts == 0)
                                        {
                                            JavaScriptSerializer js = new JavaScriptSerializer();
                                            js.MaxJsonLength = Int32.MaxValue;
                                            string str = js.Serialize(Global.Merchant.Consumer + CustPhoneNumber + Global.Merchant.RewardFormulaErrorMessage + master.MerchantName.ToString());
                                            context.Response.Write(str);
                                        }
                                        else
                                        {
                                            if (availablepts < pointstoredeem)
                                            {
                                                JavaScriptSerializer js = new JavaScriptSerializer();
                                                js.MaxJsonLength = Int32.MaxValue;
                                                string str = js.Serialize(Global.Merchant.NoPointsAvailableErrorMessage.Replace("xyz", CustPhoneNumber).Replace("nnn", pts.ToString()) + " " + Global.Merchant.NoPointsAvailableErrorMessage1);
                                                context.Response.Write(str);
                                            }
                                        }

                                        int actualdiscount = (pointstoredeem * Convert.ToInt32(red.RedeemRs)) / Convert.ToInt32(red.RedeemPt);


                                        JavaScriptSerializer js1 = new JavaScriptSerializer();
                                        js1.MaxJsonLength = Int32.MaxValue;
                                        string str1 = js1.Serialize(Global.Merchant.RedeemPointsConfirmationMessage.Replace("nnn", RedeemPoints.ToString()).Replace("ppp", actualdiscount.ToString()).Replace("Currency", country.currency));
                                        context.Response.Write(str1);
                                    }
                                }
                                else if (master.RedeemProgram == "Options")
                                {
                                    redeemoption options = dataContext.redeemoptions.Where(x => x.MerchantId == userid).OrderByDescending(x => x.CreationDate).FirstOrDefault();
                                    if (options == null)
                                    {
                                        JavaScriptSerializer js = new JavaScriptSerializer();
                                        js.MaxJsonLength = Int32.MaxValue;

                                        string str = js.Serialize(Global.Merchant.NoPointsDefinedErrorMessage.Replace("xyz", master.MerchantName));
                                        context.Response.Write(str);
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

                                    JavaScriptSerializer js1 = new JavaScriptSerializer();
                                    js1.MaxJsonLength = Int32.MaxValue;

                                    string str1 = js1.Serialize(msg);
                                    context.Response.Write(str1);
                                }
                                else if (master.RedeemProgram == "Cashback")
                                {
                                    cashbackdetail red = dataContext.cashbackdetails.Where(x => x.MerchantId == userid).FirstOrDefault();
                                    if (red == null)
                                    {
                                        EventLog.LogErrorData("Error occurred MerchantWebService/RedeemPoints." + Global.Merchant.NoRedeemFormulaErrorMessage + master.MerchantName.ToString(), true);
                                        JavaScriptSerializer js = new JavaScriptSerializer();
                                        js.MaxJsonLength = Int32.MaxValue;

                                        string str = js.Serialize(Global.Merchant.NoRedeemFormulaErrorMessage + master.MerchantName.ToString());
                                        context.Response.Write(str);
                                    }

                                    using (instadelight_consumerEntities ConsumerdataContext = new instadelight_consumerEntities())
                                    {
                                        int pts = Convert.ToInt32(ConsumerdataContext.consumerrewardpoints.Where(x => x.ConsumerId == consumeruserid && x.MerchantId == userid).Select(x => x.Points).Sum());
                                        int redeempts = Convert.ToInt32(ConsumerdataContext.consumerredeemdetails.Where(x => x.ConsumerId == consumeruserid && x.MerchantId == userid).Select(x => x.PointsRedeemed).Sum());
                                        int availablepts = pts - redeempts;


                                        if (availablepts == 0)
                                        {
                                            EventLog.LogErrorData("Error occurred MerchantWebService/RedeemPoints." + Global.Merchant.Consumer + CustPhoneNumber + Global.Merchant.RewardFormulaErrorMessage + master.MerchantName.ToString(), true);
                                            JavaScriptSerializer js = new JavaScriptSerializer();
                                            js.MaxJsonLength = Int32.MaxValue;

                                            string str = js.Serialize(Global.Merchant.Consumer + CustPhoneNumber + Global.Merchant.RewardFormulaErrorMessage + master.MerchantName.ToString());
                                            context.Response.Write(str);
                                        }
                                        else
                                        {
                                            if (availablepts < pointstoredeem)
                                            {
                                                EventLog.LogErrorData("Error occurred MerchantWebService/RedeemPoints." + Global.Merchant.NoPointsAvailableErrorMessage.Replace("xyz", CustPhoneNumber).Replace("nnn", pts.ToString()) + " " + Global.Merchant.NoPointsAvailableErrorMessage1, true);
                                                JavaScriptSerializer js = new JavaScriptSerializer();
                                                js.MaxJsonLength = Int32.MaxValue;

                                                string str = js.Serialize(Global.Merchant.NoPointsAvailableErrorMessage.Replace("xyz", CustPhoneNumber).Replace("nnn", pts.ToString()) + " " + Global.Merchant.NoPointsAvailableErrorMessage1);
                                                context.Response.Write(str);
                                            }
                                        }

                                        int actualdiscount = (pointstoredeem * Convert.ToInt32(red.RedeemRs)) / Convert.ToInt32(red.RedeemPoint);


                                        string msg = Global.Merchant.RedeemPointsConfirmationMessage.Replace("nnn", RedeemPoints.ToString()).Replace("ppp", actualdiscount.ToString()).Replace("Currency", country.currency);

                                        JavaScriptSerializer js1 = new JavaScriptSerializer();
                                        js1.MaxJsonLength = Int32.MaxValue;
                                        string str1 = js1.Serialize(msg);
                                        context.Response.Write(str1);
                                    }
                                }
                                else
                                {
                                    EventLog.LogErrorData("Error occurred MerchantWebService/RedeemPoints." + "Merchant " + master.MerchantName.ToString() + " has not defined any reward program", true);
                                    JavaScriptSerializer js = new JavaScriptSerializer();
                                    js.MaxJsonLength = Int32.MaxValue;

                                    string str = js.Serialize("Merchant " + master.MerchantName.ToString() + " has not defined any reward program");
                                    context.Response.Write(str);
                                }
                            }
                            else
                            {
                                EventLog.LogErrorData("Error occurred MerchantWebService/RedeemPoints." + "Merchant " + master.MerchantName.ToString() + " has not defined any reward program", true);
                                JavaScriptSerializer js = new JavaScriptSerializer();
                                js.MaxJsonLength = Int32.MaxValue;

                                string str = js.Serialize("Merchant " + master.MerchantName.ToString() + " has not defined any reward program");
                                context.Response.Write(str);
                            }
                        }

                    }
                    else
                    {
                        EventLog.LogErrorData("Error occurred MerchantWebService/RedeemPoints." + Global.Merchant.InvalidpointsDetails, true);
                        JavaScriptSerializer js = new JavaScriptSerializer();
                        js.MaxJsonLength = Int32.MaxValue;

                        string str = js.Serialize(Global.Merchant.InvalidpointsDetails);
                        context.Response.Write(str);
                    }
                }
                catch (Exception ex)
                {
                    //call error view here
                    //log the error
                    EventLog.LogErrorData("Error occurred MerchantWebService/RedeemCoupon." + ex.Message, true);
                    JavaScriptSerializer js = new JavaScriptSerializer();
                    js.MaxJsonLength = Int32.MaxValue;

                    string str = js.Serialize(Global.Merchant.RedeemCouponException);
                    context.Response.Write(str);
                }
            }
            else if (action == "RedeemCoupon")
            {
                string couponcode = Convert.ToString(context.Request["couponcode"]);
                string billedamount = Convert.ToString(context.Request["billedamount"]);
                string userid = Convert.ToString(context.Request["userid"]);
                string CustPhoneNumber = Convert.ToString(context.Request["CustPhoneNumber"]);
                try
                {
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
                                JavaScriptSerializer js = new JavaScriptSerializer();
                                js.MaxJsonLength = Int32.MaxValue;

                                string str = js.Serialize("Consumer does not exist.");
                                context.Response.Write(str);
                            }
                        }

                        using (MerchantEntities dataContext = new MerchantEntities())
                        {
                            merchant_master master = dataContext.merchant_master.Where(x => x.UserId == userid).FirstOrDefault();
                            merchantconsumerdetail det = dataContext.merchantconsumerdetails.Where(x => x.ConsumerId == consumeruserid).FirstOrDefault();
                            if (det == null)
                            {
                                JavaScriptSerializer js = new JavaScriptSerializer();
                                js.MaxJsonLength = Int32.MaxValue;

                                string str = js.Serialize("Consumer " + CustPhoneNumber + " is not registered with merchant " + master.MerchantName);
                                context.Response.Write(str);
                            }

                            coupons_master cpn = dataContext.coupons_master.Where(x => x.CouponCode == couponcode).FirstOrDefault();
                            if (cpn != null)
                            {
                                merchantconsumercoupondetail sharedcoupon = dataContext.merchantconsumercoupondetails.Where(s => s.MerchantId == userid && s.ConsumerId == consumeruserid && s.CouponId == cpn.couponid).FirstOrDefault();
                                if (sharedcoupon == null)
                                {
                                    JavaScriptSerializer js = new JavaScriptSerializer();
                                    js.MaxJsonLength = Int32.MaxValue;

                                    string str = js.Serialize("This coupon is not shared with consumer " + CustPhoneNumber + " by merchant " + master.MerchantName);
                                    context.Response.Write(str);
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
                                    JavaScriptSerializer js = new JavaScriptSerializer();
                                    js.MaxJsonLength = Int32.MaxValue;

                                    string str = js.Serialize("Coupon is not valid for merchant " + master.MerchantName);
                                    context.Response.Write(str);
                                }
                                else if (mer.Count == 0)
                                {
                                    JavaScriptSerializer js = new JavaScriptSerializer();
                                    js.MaxJsonLength = Int32.MaxValue;

                                    string str = js.Serialize("Coupon is not valid for merchant " + master.MerchantName);
                                    context.Response.Write(str);
                                }


                                if (cpn.ValidFrom != null)
                                {
                                    if (cpn.ValidFrom < DateTime.Now)
                                    {
                                        if (cpn.ValidTill != null)
                                        {
                                            if (cpn.ValidTill < DateTime.Now)
                                            {
                                                JavaScriptSerializer js = new JavaScriptSerializer();
                                                js.MaxJsonLength = Int32.MaxValue;

                                                string str = js.Serialize("Coupon is expired. You cannot redeem coupon now.");
                                                context.Response.Write(str);
                                            }
                                        }
                                    }
                                }

                                if (cpn.ValidTill != null)
                                {
                                    if (cpn.ValidTill < DateTime.Now)
                                    {
                                        JavaScriptSerializer js = new JavaScriptSerializer();
                                        js.MaxJsonLength = Int32.MaxValue;

                                        string str = js.Serialize("Coupon is expired. You cannot redeem coupon now.");
                                        context.Response.Write(str);
                                    }
                                }

                                coupon_redeem_details checkredeem = dataContext.coupon_redeem_details.Where(x => x.couponcode == cpn.CouponCode && x.merchantid == cpn.MerchantId && x.ConsumerId == consumeruserid).FirstOrDefault();
                                if (checkredeem != null)
                                {
                                    JavaScriptSerializer js = new JavaScriptSerializer();
                                    js.MaxJsonLength = Int32.MaxValue;

                                    string str = js.Serialize("Coupon " + couponcode + " is already redeemed by " + CustPhoneNumber + " on " + Convert.ToDateTime(checkredeem.redeemedon).ToString("dd-MMM-yyyy"));
                                    context.Response.Write(str);
                                }



                                if (cpn.AboveAmount > billedamt)
                                {
                                    JavaScriptSerializer js = new JavaScriptSerializer();
                                    js.MaxJsonLength = Int32.MaxValue;

                                    string str = js.Serialize("Billed amount must be greater than " + cpn.AboveAmount + " to redeem the coupon. You cannot redeem coupon now.");
                                    context.Response.Write(str);
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
                                                    JavaScriptSerializer js = new JavaScriptSerializer();
                                                    js.MaxJsonLength = Int32.MaxValue;

                                                    string str = js.Serialize("Coupon is Valid. Discounted amount is greater than max discount allowed of " + cpn.MaxDiscount + " . Redeem Coupon for " + cpn.MaxDiscount + ".");
                                                    context.Response.Write(str);
                                                }
                                            }
                                        }

                                    }
                                }
                                country_master ctry = dataContext.country_master.FirstOrDefault();

                                //dataContext.coupon_redeem_details.Add(redeem);
                                //dataContext.SaveChanges();

                                string msg = "Coupon is Valid. ";
                                if (cpn.Discount != null)
                                {
                                    if (cpn.Discount != 0)
                                    {
                                        msg += "Please give " + ctry.currency + " " + cpn.Discount + " discount on the bill amount";
                                    }

                                }
                                if (cpn.PercentageOff != null)
                                {
                                    if (cpn.PercentageOff != 0)
                                    {
                                        msg += "Please give " + cpn.PercentageOff + "% discount on the bill amount";
                                    }
                                }

                                JavaScriptSerializer js1 = new JavaScriptSerializer();
                                js1.MaxJsonLength = Int32.MaxValue;

                                string str1 = js1.Serialize(msg);
                                context.Response.Write(str1);

                            }
                            else
                            {
                                JavaScriptSerializer js = new JavaScriptSerializer();
                                js.MaxJsonLength = Int32.MaxValue;

                                string str = js.Serialize("Invalid Coupon Details");
                                context.Response.Write(str);
                            }
                        }

                    }
                    else
                    {
                        JavaScriptSerializer js = new JavaScriptSerializer();
                        js.MaxJsonLength = Int32.MaxValue;

                        string str = js.Serialize("Invalid Coupon Details");
                        context.Response.Write(str);
                    }
                }
                catch (Exception ex)
                {
                    //call error view here
                    //log the error
                    EventLog.LogErrorData("Error occured MerchantWebService/RedeemCoupon." + ex.Message, true);
                    JavaScriptSerializer js = new JavaScriptSerializer();
                    js.MaxJsonLength = Int32.MaxValue;

                    string str = js.Serialize("Error occured while redeem coupon");
                    context.Response.Write(str);
                }
            }
            else if (action == "RedeemCouponFromCode")
            {
                string couponcode = Convert.ToString(context.Request["couponcode"]);
                string userid = Convert.ToString(context.Request["userid"]);

                try
                {
                    if (string.IsNullOrEmpty(couponcode) == false)
                    {
                        using (MerchantEntities dataContext = new MerchantEntities())
                        {
                            coupons_master cpn = dataContext.coupons_master.Where(x => x.CouponCode == couponcode).FirstOrDefault();
                            if (cpn != null)
                            {
                                merchant_master mer = dataContext.merchant_master.Where(x => x.merchantid == cpn.MerchantId && x.UserId == userid).FirstOrDefault();

                                if (mer == null)
                                {
                                    JavaScriptSerializer js = new JavaScriptSerializer();
                                    js.MaxJsonLength = Int32.MaxValue;

                                    string str = js.Serialize("Coupon is not valid for merchant " + mer.MerchantName);
                                    context.Response.Write(str);

                                }



                                if (cpn.ValidFrom != null)
                                {
                                    if (cpn.ValidFrom < DateTime.Now)
                                    {
                                        if (cpn.ValidTill != null)
                                        {
                                            if (cpn.ValidTill < DateTime.Now)
                                            {
                                                JavaScriptSerializer js = new JavaScriptSerializer();
                                                js.MaxJsonLength = Int32.MaxValue;

                                                string str = js.Serialize("Coupon is expired. You cannot redeem coupon now.");
                                                context.Response.Write(str);
                                            }
                                        }
                                    }
                                }

                                if (cpn.ValidTill != null)
                                {
                                    if (cpn.ValidTill < DateTime.Now)
                                    {
                                        JavaScriptSerializer js = new JavaScriptSerializer();
                                        js.MaxJsonLength = Int32.MaxValue;

                                        string str = js.Serialize("Coupon is expired. You cannot redeem coupon now.");
                                        context.Response.Write(str);
                                    }
                                }

                                JavaScriptSerializer js1 = new JavaScriptSerializer();
                                js1.MaxJsonLength = Int32.MaxValue;

                                string str1 = js1.Serialize("Valid Coupon");
                                context.Response.Write(str1);
                            }
                            else
                            {
                                JavaScriptSerializer js = new JavaScriptSerializer();
                                js.MaxJsonLength = Int32.MaxValue;

                                string str = js.Serialize("Invalid Coupon Details");
                                context.Response.Write(str);
                            }
                        }

                    }
                    else
                    {
                        JavaScriptSerializer js = new JavaScriptSerializer();
                        js.MaxJsonLength = Int32.MaxValue;

                        string str = js.Serialize("Invalid Coupon Details");
                        context.Response.Write(str);
                    }
                }
                catch (Exception ex)
                {
                    //call error view here
                    //log the error
                    EventLog.LogErrorData("Error occured MerchantWebService/RedeemCouponFromCode." + ex.Message, true);
                    JavaScriptSerializer js = new JavaScriptSerializer();
                    js.MaxJsonLength = Int32.MaxValue;

                    string str = js.Serialize("Error occured while redeem coupon");
                    context.Response.Write(str);
                }

            }
            else if (action == "GetAllCoupons")
            {
                string userid = Convert.ToString(context.Request["userid"]);
                try
                {
                    using (MerchantEntities dataContext = new MerchantEntities())
                    {
                        merchant_master master = dataContext.merchant_master.Where(x => x.UserId == userid).FirstOrDefault();
                        if (master != null)
                        {
                            var couponList = dataContext.coupons_master.Where(x => x.MerchantId == master.merchantid).ToList();

                            JavaScriptSerializer js = new JavaScriptSerializer();
                            js.MaxJsonLength = Int32.MaxValue;

                            string str = js.Serialize(couponList);
                            context.Response.Write(str);
                        }
                        else
                        {
                            JavaScriptSerializer js = new JavaScriptSerializer();
                            js.MaxJsonLength = Int32.MaxValue;

                            string str = js.Serialize("Invalid Merchant Details");
                            context.Response.Write(str);
                        }
                    }
                }
                catch (Exception ex)
                {
                    //call error view here
                    //log the error
                    EventLog.LogErrorData("Error occured MerchantWebService/GetAllCoupons." + ex.Message, true);
                    JavaScriptSerializer js = new JavaScriptSerializer();
                    js.MaxJsonLength = Int32.MaxValue;

                    string str = js.Serialize("Error occured while retrieving all coupons for merchant");
                    context.Response.Write(str);
                }
            }
            else if (action == "getNoOfVisits")
            {
                string ConsumerPhone = Convert.ToString(context.Request["ConsumerPhone"]);
                string userid = Convert.ToString(context.Request["userid"]);
                ApplicationUserManager UserManager = context.GetOwinContext().Get<ApplicationUserManager>();
                if (ConsumerPhone.Contains('@') == false)
                {
                    string[] PhonewithCode = ConsumerPhone.Split(' ');
                    if (PhonewithCode.Length > 0)
                    {
                        if (PhonewithCode.Length == 3)
                        {
                            ConsumerPhone = "+" + PhonewithCode[1] + " " + PhonewithCode[2].ToString();
                        }
                    }
                }

                try
                {
                    //string MerchantId = TempData["TempMerchantId"].ToString();
                    using (MerchantEntities dataContext = new MerchantEntities())
                    {
                        string ownerid = "";
                        string staffloc = "";
                        if (UserManager.IsInRole(userid, "Staff"))
                        {
                            merchant_master master = dataContext.merchant_master.Where(x => x.UserId == userid).FirstOrDefault();
                            staff_master stfmgr = dataContext.staff_master.Where(x => x.StaffMasterId == master.merchantid).FirstOrDefault();
                            merchant_master owner = dataContext.merchant_master.Where(x => x.merchantid == stfmgr.MerchantId).FirstOrDefault();
                            ownerid = owner.UserId;
                            branch_master branch = dataContext.branch_master.Where(x => x.BranchId == stfmgr.BranchId).FirstOrDefault();
                            staffloc = branch.BranchLocation;

                        }
                        else if (UserManager.IsInRole(userid, "LocationManager"))
                        {
                            merchant_master master = dataContext.merchant_master.Where(x => x.UserId == userid).FirstOrDefault();
                            branch_master branch = dataContext.branch_master.Where(x => x.BranchManagerId == master.merchantid).FirstOrDefault();
                            merchant_master owner = dataContext.merchant_master.Where(x => x.merchantid == branch.MerchantId).FirstOrDefault();
                            ownerid = owner.UserId;
                            staffloc = branch.BranchLocation;
                        }

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

                                int availablepoints = 0;
                                int redeemedpoints = 0;
                                if (UserManager.IsInRole(userid, "Staff") || UserManager.IsInRole(userid, "LocationManager"))
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
                                    if (UserManager.IsInRole(userid, "Staff") || UserManager.IsInRole(userid, "LocationManager"))
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

                                JavaScriptSerializer js = new JavaScriptSerializer();
                                js.MaxJsonLength = Int32.MaxValue;

                                string str = js.Serialize(profile);
                                context.Response.Write(str);
                            }
                            else
                            {
                                EventLog.LogErrorData("Unauthorized access.", true);
                                JavaScriptSerializer js = new JavaScriptSerializer();
                                js.MaxJsonLength = Int32.MaxValue;

                                string str = js.Serialize("0");
                                context.Response.Write(str);

                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    //call error view here
                    //log the error
                    EventLog.LogErrorData("Error occured MerchantWebService/getNoOfVisits." + ex.Message, true);
                    JavaScriptSerializer js = new JavaScriptSerializer();
                    js.MaxJsonLength = Int32.MaxValue;

                    string str = js.Serialize("Error occured while retrieving no of visits");
                    context.Response.Write(str);
                }
            }
            else if (action == "GetAllEventsCoupons")
            {
                string userid = Convert.ToString(context.Request["userid"]);
                try
                {
                    using (MerchantEntities dataContext = new MerchantEntities())
                    {
                        merchant_master master = dataContext.merchant_master.Where(x => x.UserId == userid).FirstOrDefault();
                        if (master != null)
                        {
                            var couponList = dataContext.eventcoupondetails.Where(x => x.MerchantId == master.merchantid).ToList();

                            JavaScriptSerializer js = new JavaScriptSerializer();
                            js.MaxJsonLength = Int32.MaxValue;

                            string str = js.Serialize(couponList);
                            context.Response.Write(str);
                        }
                        else
                        {
                            JavaScriptSerializer js = new JavaScriptSerializer();
                            js.MaxJsonLength = Int32.MaxValue;

                            string str = js.Serialize("Invalid Merchant Details");
                            context.Response.Write(str);
                        }
                    }
                }
                catch (Exception ex)
                {
                    //call error view here
                    //log the error
                    EventLog.LogErrorData("Error occured MerchantWebService/GetAllEventCoupons." + ex.Message, true);
                    JavaScriptSerializer js = new JavaScriptSerializer();
                    js.MaxJsonLength = Int32.MaxValue;

                    string str = js.Serialize("Error occured while retrieving event coupon list");
                    context.Response.Write(str);
                }
            }
            else if (action == "getLocationsForCoupons")
            {
                string userid = Convert.ToString(context.Request["userid"]);
                try
                {
                    using (MerchantEntities dataContext = new MerchantEntities())
                    {
                        var loclist = (from m in dataContext.merchant_master
                                       where m.UserId == userid
                                       orderby m.Location
                                       select new
                                       {
                                           label = m.Location + "-" + m.City,
                                           id = m.Location
                                       }).Distinct().ToList();

                        JavaScriptSerializer js = new JavaScriptSerializer();
                        js.MaxJsonLength = Int32.MaxValue;

                        string str = js.Serialize(loclist);
                        context.Response.Write(str);
                    }
                }
                catch (Exception ex)
                {
                    //call error view here
                    //log the error
                    EventLog.LogErrorData("Error occured MerchantWebService/getLocationsForCoupons." + ex.Message, true);
                    JavaScriptSerializer js = new JavaScriptSerializer();
                    js.MaxJsonLength = Int32.MaxValue;

                    string str = js.Serialize("Error occured while retrieving coupons for locations");
                    context.Response.Write(str);
                }
            }
            else if (action == "GetLocations")
            {
                string userid = Convert.ToString(context.Request["userid"]);
                string cityid = Convert.ToString(context.Request["cityid"]);

                try
                {
                    using (MerchantEntities dataContext = new MerchantEntities())
                    {
                        int no = Convert.ToInt32(cityid);
                        var Locations = dataContext.location_master.Where(x => x.CityId == no).OrderBy(x => x.Location).ToList();
                        JavaScriptSerializer js = new JavaScriptSerializer();
                        js.MaxJsonLength = Int32.MaxValue;

                        string str = js.Serialize(Locations);
                        context.Response.Write(str);
                    }
                }
                catch (Exception ex)
                {
                    //call error view here
                    //log the error
                    EventLog.LogErrorData("Error occured MerchantWebService/GetLocations." + ex.Message, true);

                    JavaScriptSerializer js = new JavaScriptSerializer();
                    js.MaxJsonLength = Int32.MaxValue;

                    string str = js.Serialize("Error occured while retrieving location list");
                    context.Response.Write(str);
                }
            }
            else if (action == "AddNewCoupon")
            {
                try
                {
                    var jsonString = String.Empty;
                    string userid = Convert.ToString(context.Request["userid"]);

                    context.Request.InputStream.Position = 0;
                    using (var inputStream = new StreamReader(context.Request.InputStream))
                    {
                        jsonString = inputStream.ReadToEnd();
                    }

                    var jsonSettings = new JsonSerializerSettings
                    {
                        TypeNameHandling = TypeNameHandling.Objects,
                        TypeNameAssemblyFormat = System.Runtime.Serialization.Formatters.FormatterAssemblyStyle.Simple
                    };
                    coupons_master cpn = JsonConvert.DeserializeObject<coupons_master>(jsonString, jsonSettings);
                    if (cpn != null)
                    {
                        GenerateCouponCode cpncode = new GenerateCouponCode();

                        using (MerchantEntities dataContext = new MerchantEntities())
                        {
                            merchant_master master = dataContext.merchant_master.Where(x => x.UserId == userid).FirstOrDefault();
                            if (master != null)
                            {
                                cpn.MerchantId = master.merchantid;
                                cpn.categoryid = master.Category;
                                cpn.CouponCode = cpncode.CreateCouponCode(master.merchantid);
                                cpn.ValidAtLocation = cpn.ValidAtLocation;

                                if (cpn.ValidFrom == null)
                                {
                                    cpn.ValidFrom = DateTime.Now;
                                }

                                cpn.DateCreated = DateTime.Now;


                                dataContext.coupons_master.Add(cpn);
                                dataContext.SaveChanges();

                                if (cpn.ShareWithAll == 1)
                                {
                                    //Share coupon with all consumers who have this merchant's dec.
                                    //Get all mobile numbers registered with this merchant
                                    var consumerphones = dataContext.merchantconsumerdetails.Where(x => x.MerchantId == userid).Select(x => x.ConsumerPhone).Distinct().ToList();
                                    foreach (string ph in consumerphones)
                                    {
                                        string result = function.AddNewCouponConsumer(ph, cpn.couponid.ToString(), userid);
                                    }

                                }
                                JavaScriptSerializer js = new JavaScriptSerializer();
                                js.MaxJsonLength = Int32.MaxValue;

                                string str = js.Serialize("Coupon Added Successfully");
                                context.Response.Write(str);
                            }
                            else
                            {
                                JavaScriptSerializer js = new JavaScriptSerializer();
                                js.MaxJsonLength = Int32.MaxValue;

                                string str = js.Serialize("Invalid Merchant Details");
                                context.Response.Write(str);
                            }
                        }
                    }
                    else
                    {
                        JavaScriptSerializer js = new JavaScriptSerializer();
                        js.MaxJsonLength = Int32.MaxValue;

                        string str = js.Serialize("Invalid Coupon Details");
                        context.Response.Write(str);
                    }
                }

                catch (Exception ex)
                {
                    //call error view here
                    //log the error
                    EventLog.LogErrorData("Error occured MerchantWebService/AddNewCoupon." + ex.Message, true);

                    JavaScriptSerializer js = new JavaScriptSerializer();
                    js.MaxJsonLength = Int32.MaxValue;

                    string str = js.Serialize("Error occured while adding new coupon");
                    context.Response.Write(str);
                }
            }
            else if (action == "AddNewReview")
            {
                try
                {
                    var jsonString = String.Empty;
                    string userid = Convert.ToString(context.Request["userid"]);

                    context.Request.InputStream.Position = 0;
                    using (var inputStream = new StreamReader(context.Request.InputStream))
                    {
                        jsonString = inputStream.ReadToEnd();
                    }

                    var jsonSettings = new JsonSerializerSettings
                    {
                        TypeNameHandling = TypeNameHandling.Objects,
                        TypeNameAssemblyFormat = System.Runtime.Serialization.Formatters.FormatterAssemblyStyle.Simple
                    };
                    reviewmaster rvw = JsonConvert.DeserializeObject<reviewmaster>(jsonString, jsonSettings);

                    if (rvw != null)
                    {

                        using (MerchantEntities dataContext = new MerchantEntities())
                        {
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

                            JavaScriptSerializer js = new JavaScriptSerializer();
                            js.MaxJsonLength = Int32.MaxValue;

                            string str = js.Serialize("Review Created Successfully");
                            context.Response.Write(str);
                        }
                    }
                    else
                    {
                        JavaScriptSerializer js = new JavaScriptSerializer();
                        js.MaxJsonLength = Int32.MaxValue;

                        string str = js.Serialize("Invalid review details");
                        context.Response.Write(str);
                    }
                }

                catch (Exception ex)
                {
                    //call error view here
                    //log the error
                    EventLog.LogErrorData("Error occured MerchantWebService/AddNewReview." + ex.Message, true);
                    JavaScriptSerializer js = new JavaScriptSerializer();
                    js.MaxJsonLength = Int32.MaxValue;

                    string str = js.Serialize("Error occured while adding new review");
                    context.Response.Write(str);
                }
            }
            else if (action == "SetEventCoupon")
            {
                string userid = Convert.ToString(context.Request["userid"]);
                string birthdaycoupon = Convert.ToString(context.Request["birthdaycoupon"]);
                string anncoupon = Convert.ToString(context.Request["anncoupon"]);
                string reviewcoupon = Convert.ToString(context.Request["reviewcoupon"]);
                string sharecoupon = Convert.ToString(context.Request["sharecoupon"]);

                try
                {
                    using (MerchantEntities dataContext = new MerchantEntities())
                    {
                        merchant_master master = dataContext.merchant_master.Where(x => x.UserId == userid).FirstOrDefault();
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
                            JavaScriptSerializer js = new JavaScriptSerializer();
                            js.MaxJsonLength = Int32.MaxValue;

                            string str = js.Serialize("Event Coupons Set Successfully");
                            context.Response.Write(str);
                        }
                        else
                        {
                            JavaScriptSerializer js = new JavaScriptSerializer();
                            js.MaxJsonLength = Int32.MaxValue;

                            string str = js.Serialize("Invalid Merchant Details");
                            context.Response.Write(str);
                        }
                    }
                }
                catch (Exception ex)
                {
                    //call error view here
                    //log the error
                    EventLog.LogErrorData("Error occured MerchantWebService/SetEventCoupon." + ex.Message, true);
                    JavaScriptSerializer js = new JavaScriptSerializer();
                    js.MaxJsonLength = Int32.MaxValue;

                    string str = js.Serialize("Error occured while setting event coupons");
                    context.Response.Write(str);
                }
            }
            else if (action == "UpdateCoupon")
            {
                try
                {
                    var jsonString = String.Empty;
                    string userid = Convert.ToString(context.Request["userid"]);

                    context.Request.InputStream.Position = 0;
                    using (var inputStream = new StreamReader(context.Request.InputStream))
                    {
                        jsonString = inputStream.ReadToEnd();
                    }

                    var jsonSettings = new JsonSerializerSettings
                    {
                        TypeNameHandling = TypeNameHandling.Objects,
                        TypeNameAssemblyFormat = System.Runtime.Serialization.Formatters.FormatterAssemblyStyle.Simple
                    };
                    coupons_master cpn = JsonConvert.DeserializeObject<coupons_master>(jsonString, jsonSettings);
                    if (cpn != null)
                    {
                        using (MerchantEntities dataContext = new MerchantEntities())
                        {

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

                            coupon.DEC = cpn.DEC;

                            dataContext.SaveChanges();

                            if (cpn.ShareWithAll == 1)
                            {
                                //Share coupon with all consumers who have this merchant's dec.
                                //Get all mobile numbers registered with this merchant
                                var consumerphones = dataContext.merchantconsumerdetails.Where(x => x.MerchantId == userid).Select(x => x.ConsumerPhone).Distinct().ToList();
                                foreach (string ph in consumerphones)
                                {
                                    string result = function.AddNewCouponConsumer(ph, cpn.couponid.ToString(), userid);
                                }

                            }
                            JavaScriptSerializer js = new JavaScriptSerializer();
                            js.MaxJsonLength = Int32.MaxValue;

                            string str = js.Serialize("coupon Updated");
                            context.Response.Write(str);
                        }
                    }
                    else
                    {
                        JavaScriptSerializer js = new JavaScriptSerializer();
                        js.MaxJsonLength = Int32.MaxValue;

                        string str = js.Serialize("Invalid coupon");
                        context.Response.Write(str);
                    }
                }
                catch (Exception ex)
                {
                    //call error view here
                    //log the error
                    EventLog.LogErrorData("Error occured MerchantWebService/UpdateCoupon." + ex.Message, true);
                    JavaScriptSerializer js = new JavaScriptSerializer();
                    js.MaxJsonLength = Int32.MaxValue;

                    string str = js.Serialize("Error occured while updating coupon");
                    context.Response.Write(str);
                }
            }
            else if (action == "GeCouponById")
            {
                string couponid = Convert.ToString(context.Request["couponid"]);
                try
                {
                    using (MerchantEntities dataContext = new MerchantEntities())
                    {
                        int no = Convert.ToInt32(couponid);
                        var coupon = dataContext.coupons_master.Find(no);
                        // return Json(coupon, JsonRequestBehavior.AllowGet);
                        JavaScriptSerializer js = new JavaScriptSerializer();
                        js.MaxJsonLength = Int32.MaxValue;

                        string str = js.Serialize(coupon);
                        context.Response.Write(str);
                    }
                }
                catch (Exception ex)
                {
                    //call error view here
                    //log the error
                    EventLog.LogErrorData("Error occured MerchantWebService/GetCouponById." + ex.Message, true);

                    JavaScriptSerializer js = new JavaScriptSerializer();
                    js.MaxJsonLength = Int32.MaxValue;

                    string str = js.Serialize("Error occured while retrieving coupon by id");
                    context.Response.Write(str);
                }
            }
            else if (action == "GetEventConditions")
            {
                string couponid = Convert.ToString(context.Request["couponid"]);
                try
                {
                    using (MerchantEntities dataContext = new MerchantEntities())
                    {
                        int no = Convert.ToInt32(couponid);

                        eventcoupondetail couponDetails = dataContext.eventcoupondetails.Where(x => x.CouponId == no).FirstOrDefault();
                        if (couponDetails != null)
                        {
                            List<eventcouponcondition> cond = dataContext.eventcouponconditions.ToList();
                            JavaScriptSerializer js = new JavaScriptSerializer();
                            js.MaxJsonLength = Int32.MaxValue;

                            string str = js.Serialize(cond);
                            context.Response.Write(str);
                        }
                        else
                        {
                            List<eventcouponcondition> cond = new List<eventcouponcondition>();
                            JavaScriptSerializer js = new JavaScriptSerializer();
                            js.MaxJsonLength = Int32.MaxValue;

                            string str = js.Serialize(cond);
                            context.Response.Write(str);
                        }
                    }
                }
                catch (Exception ex)
                {
                    //call error view here
                    //log the error
                    EventLog.LogErrorData("Error occured MerchantWebService/GetEventConditions." + ex.Message, true);
                    JavaScriptSerializer js = new JavaScriptSerializer();
                    js.MaxJsonLength = Int32.MaxValue;

                    string str = js.Serialize("Error occured while retrieving event conditions for coupon");
                    context.Response.Write(str);
                }
            }
            else if (action == "UpdateMerchant")
            {
                try
                {
                    var jsonString = String.Empty;

                    context.Request.InputStream.Position = 0;
                    using (var inputStream = new StreamReader(context.Request.InputStream))
                    {
                        jsonString = inputStream.ReadToEnd();
                    }

                    var jsonSettings = new JsonSerializerSettings
                    {
                        TypeNameHandling = TypeNameHandling.Objects,
                        TypeNameAssemblyFormat = System.Runtime.Serialization.Formatters.FormatterAssemblyStyle.Simple
                    };
                    merchant_master mch = JsonConvert.DeserializeObject<merchant_master>(jsonString, jsonSettings);

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

                            dataContext.SaveChanges();

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
                                    rdm.RewardName = mch.RewardName;
                                    rdm.RewardRs = mch.RewardRs;
                                    rdm.RewardPoints = mch.RewardPoints;
                                    rdm.CreationDate = DateTime.Now;
                                    dataContext.rewardmasters.Add(rdm);
                                    dataContext.SaveChanges();
                                }
                            }

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
                            JavaScriptSerializer js = new JavaScriptSerializer();
                            js.MaxJsonLength = Int32.MaxValue;

                            string str = js.Serialize("Profile Updated");
                            context.Response.Write(str);
                        }
                    }
                    else
                    {
                        JavaScriptSerializer js = new JavaScriptSerializer();
                        js.MaxJsonLength = Int32.MaxValue;

                        string str = js.Serialize("Invalid Merchant");
                        context.Response.Write(str);
                    }
                }
                catch (Exception ex)
                {
                    //call error view here
                    //log the error
                    EventLog.LogErrorData("Error occured MerchantWebService/UpdateMerchant." + ex.Message, true);
                    JavaScriptSerializer js = new JavaScriptSerializer();
                    js.MaxJsonLength = Int32.MaxValue;

                    string str = js.Serialize("Error occured while updating merchant");
                    context.Response.Write(str);
                }
            }
            else if (action == "SendCouponOTP")
            {
                string SharedCouponId = Convert.ToString(context.Request["SharedCouponId"]);

                using (MerchantEntities datacontext = new MerchantEntities())
                {
                    int no = Convert.ToInt32(SharedCouponId);
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

                                                JavaScriptSerializer js = new JavaScriptSerializer();
                                                js.MaxJsonLength = Int32.MaxValue;

                                                string str = js.Serialize("OTP Sent Successfully.");
                                                context.Response.Write(str);
                                            }
                                            else
                                            {
                                                JavaScriptSerializer js = new JavaScriptSerializer();
                                                js.MaxJsonLength = Int32.MaxValue;

                                                string str = js.Serialize("Error occurred while sending OTP to email id " + consumer.UserName);
                                                context.Response.Write(str);
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
                                                JavaScriptSerializer js = new JavaScriptSerializer();
                                                js.MaxJsonLength = Int32.MaxValue;

                                                string str = js.Serialize("OTP Sent Successfully.");
                                                context.Response.Write(str);
                                            }
                                            else
                                            {
                                                JavaScriptSerializer js = new JavaScriptSerializer();
                                                js.MaxJsonLength = Int32.MaxValue;

                                                string str = js.Serialize("Error occurred while sending OTP to " + consumer.UserName);
                                                context.Response.Write(str);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        EventLog.LogErrorData("Error occurred MerchantWebService/SendCouponOTP. New OTP cant be generated", true);
                                        JavaScriptSerializer js = new JavaScriptSerializer();
                                        js.MaxJsonLength = Int32.MaxValue;

                                        string str = js.Serialize("New OTP cant be generated");
                                        context.Response.Write(str);
                                    }
                                }
                                else
                                {
                                    EventLog.LogErrorData("Error occurred MerchantWebService/SendCouponOTP. Consumer does not exist", true);

                                    JavaScriptSerializer js = new JavaScriptSerializer();
                                    js.MaxJsonLength = Int32.MaxValue;

                                    string str = js.Serialize("Consumer does not exist");
                                    context.Response.Write(str);
                                }
                            }
                        }
                        else
                        {
                            EventLog.LogErrorData("Error occurred MerchantWebService/SendCouponOTP. Shared coupon does not exist", true);
                            JavaScriptSerializer js = new JavaScriptSerializer();
                            js.MaxJsonLength = Int32.MaxValue;

                            string str = js.Serialize("Shared coupon does not exist");
                            context.Response.Write(str);
                        }
                    }
                    catch (Exception ex)
                    {
                        //call error view here
                        //log the error
                        EventLog.LogErrorData("Error occurred MerchantWebService/SendCouponOTP." + ex.Message, true);
                        JavaScriptSerializer js = new JavaScriptSerializer();
                        js.MaxJsonLength = Int32.MaxValue;

                        string str = js.Serialize("Error occured while sending coupon OTP");
                        context.Response.Write(str);
                    }
                }
            }
            else if (action == "VerifyCouponOTP")
            {
                try
                {
                    string userid = Convert.ToString(context.Request["UserId"]);

                    string OTP = Convert.ToString(context.Request["OTP"]);
                    string SharedCouponId = Convert.ToString(context.Request["SharedCouponId"]);
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
                                JavaScriptSerializer js = new JavaScriptSerializer();
                                js.MaxJsonLength = Int32.MaxValue;

                                string str = js.Serialize("OTP Verified");
                                context.Response.Write(str);

                            }
                            else
                            {
                                JavaScriptSerializer js = new JavaScriptSerializer();
                                js.MaxJsonLength = Int32.MaxValue;

                                string str = js.Serialize("OTP is invalid.");
                                context.Response.Write(str);
                            }
                        }
                        else
                        {
                            JavaScriptSerializer js = new JavaScriptSerializer();
                            js.MaxJsonLength = Int32.MaxValue;

                            string str = js.Serialize("OTP is invalid.");
                            context.Response.Write(str);
                        }
                    }
                }
                catch (Exception ex)
                {
                    //call error view here
                    //log the error
                    EventLog.LogErrorData("Error occurred MerchantWebService/VerifyCouponOTP." + ex.Message, true);
                    JavaScriptSerializer js = new JavaScriptSerializer();
                    js.MaxJsonLength = Int32.MaxValue;

                    string str = js.Serialize("Error occurred while verifying OTP.");
                    context.Response.Write(str);

                }
            }
            else if (action == "FinalRedeem")
            {
                string couponid = Convert.ToString(context.Request["couponid"]);
                string SharedCouponId = Convert.ToString(context.Request["SharedCouponId"]);
                string userid = Convert.ToString(context.Request["userid"]);
                DateTime visitdate = DateTime.Now;

                ApplicationUserManager UserManager = context.GetOwinContext().Get<ApplicationUserManager>();
                try
                {
                    string consumeruserid = "";
                    int couponno = Convert.ToInt32(couponid);
                    int sharedcouponno = Convert.ToInt32(SharedCouponId);

                    if (string.IsNullOrEmpty(couponid) == false && string.IsNullOrEmpty(SharedCouponId) == false)
                    {
                        using (MerchantEntities dataContext = new MerchantEntities())
                        {
                            string ownerid = "";
                            string staffloc = "";
                            if (UserManager.IsInRole(userid, "Staff"))
                            {
                                merchant_master staffmaster = dataContext.merchant_master.Where(x => x.UserId == userid).FirstOrDefault();
                                staff_master stfmgr = dataContext.staff_master.Where(x => x.StaffMasterId == staffmaster.merchantid).FirstOrDefault();
                                merchant_master owner = dataContext.merchant_master.Where(x => x.merchantid == stfmgr.MerchantId).FirstOrDefault();
                                ownerid = owner.UserId;
                                branch_master branch = dataContext.branch_master.Where(x => x.BranchId == stfmgr.BranchId).FirstOrDefault();
                                staffloc = branch.BranchLocation;
                            }
                            else if (UserManager.IsInRole(userid, "LocationManager"))
                            {
                                merchant_master locationmaster = dataContext.merchant_master.Where(x => x.UserId == userid).FirstOrDefault();
                                branch_master branch = dataContext.branch_master.Where(x => x.BranchManagerId == locationmaster.merchantid).FirstOrDefault();
                                merchant_master owner = dataContext.merchant_master.Where(x => x.merchantid == branch.MerchantId).FirstOrDefault();
                                ownerid = owner.UserId;
                                staffloc = branch.BranchLocation;
                            }

                            merchant_master master = dataContext.merchant_master.Where(x => x.UserId == userid).FirstOrDefault();

                            coupons_master cpn = dataContext.coupons_master.Where(x => x.couponid == couponno).FirstOrDefault();
                            merchantconsumercoupondetail sharedcoupon = new merchantconsumercoupondetail();

                            if (cpn != null)
                            {
                                if (UserManager.IsInRole(userid, "Staff") || UserManager.IsInRole(userid, "LocationManager"))
                                {
                                    sharedcoupon = dataContext.merchantconsumercoupondetails.Where(s => s.MerchantId == ownerid && s.CouponId == cpn.couponid && s.Id == sharedcouponno).FirstOrDefault();
                                    if (sharedcoupon == null)
                                    {
                                        JavaScriptSerializer js = new JavaScriptSerializer();
                                        js.MaxJsonLength = Int32.MaxValue;

                                        string str = js.Serialize("This coupon is not shared by merchant " + master.MerchantName);
                                        context.Response.Write(str);
                                    }
                                    consumeruserid = sharedcoupon.ConsumerId;
                                }
                                else
                                {
                                    sharedcoupon = dataContext.merchantconsumercoupondetails.Where(s => s.MerchantId == userid && s.CouponId == cpn.couponid && s.Id == sharedcouponno).FirstOrDefault();
                                    if (sharedcoupon == null)
                                    {
                                        JavaScriptSerializer js = new JavaScriptSerializer();
                                        js.MaxJsonLength = Int32.MaxValue;

                                        string str = js.Serialize("This coupon is not shared by merchant " + master.MerchantName);
                                        context.Response.Write(str);
                                    }
                                    consumeruserid = sharedcoupon.ConsumerId;
                                }

                                using (instadelight_consumerEntities consumerDataContext = new instadelight_consumerEntities())
                                {
                                    consumermaster consumer = consumerDataContext.consumermasters.Where(x => x.UserId == consumeruserid).FirstOrDefault();

                                    //coupon_redeem_details checkredeem = dataContext.coupon_redeem_details.Where(x => x.couponcode == cpn.CouponCode && x.merchantid == cpn.MerchantId && x.ConsumerId == consumeruserid).FirstOrDefault();
                                    coupon_redeem_details checkredeem = dataContext.coupon_redeem_details.Where(x => x.couponid == sharedcouponno && x.merchantid == cpn.MerchantId && x.ConsumerId == consumeruserid).FirstOrDefault();
                                    if (checkredeem != null)
                                    {
                                        JavaScriptSerializer js = new JavaScriptSerializer();
                                        js.MaxJsonLength = Int32.MaxValue;

                                        string str = js.Serialize("Coupon " + cpn.CouponCode + " is already redeemed by " + consumer.UserId + " on " + Convert.ToDateTime(checkredeem.redeemedon).ToString("dd-MMM-yyyy"));
                                        context.Response.Write(str);
                                    }

                                    coupon_redeem_details redeem = new coupon_redeem_details();
                                    redeem.couponid = sharedcouponno;
                                    redeem.couponcode = cpn.CouponCode;
                                    redeem.redeemedon = DateTime.Now;
                                    redeem.merchantid = cpn.MerchantId;

                                    if (UserManager.IsInRole(userid, "Staff") || UserManager.IsInRole(userid, "LocationManager"))
                                    {
                                        var mer = dataContext.merchant_master.Where(x => x.merchantid == cpn.MerchantId && x.UserId == ownerid).ToList();

                                        if (mer == null)
                                        {
                                            JavaScriptSerializer js = new JavaScriptSerializer();
                                            js.MaxJsonLength = Int32.MaxValue;

                                            string str = js.Serialize("Coupon is not valid for merchant " + master.MerchantName);
                                            context.Response.Write(str);
                                        }
                                        else if (mer.Count == 0)
                                        {
                                            JavaScriptSerializer js = new JavaScriptSerializer();
                                            js.MaxJsonLength = Int32.MaxValue;

                                            string str = js.Serialize("Coupon is not valid for merchant " + master.MerchantName);
                                            context.Response.Write(str);
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
                                            JavaScriptSerializer js = new JavaScriptSerializer();
                                            js.MaxJsonLength = Int32.MaxValue;

                                            string str = js.Serialize("Coupon is not valid for merchant " + master.MerchantName);
                                            context.Response.Write(str);

                                        }
                                        else if (mer.Count == 0)
                                        {
                                            JavaScriptSerializer js = new JavaScriptSerializer();
                                            js.MaxJsonLength = Int32.MaxValue;

                                            string str = js.Serialize("Coupon is not valid for merchant " + master.MerchantName);
                                            context.Response.Write(str);
                                        }

                                        redeem.merchantid = mer[0].merchantid;
                                        redeem.city = mer[0].City;
                                        redeem.location = mer[0].Location;
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


                                        JavaScriptSerializer js1 = new JavaScriptSerializer();
                                        js1.MaxJsonLength = Int32.MaxValue;

                                        string str1 = js1.Serialize("Coupon Redeemed Successfully.");
                                        context.Response.Write(str1);
                                    }
                                }
                            }
                            else
                            {
                                JavaScriptSerializer js = new JavaScriptSerializer();
                                js.MaxJsonLength = Int32.MaxValue;

                                string str = js.Serialize("Invalid Coupon Details");
                                context.Response.Write(str);
                            }
                        }

                    }
                    else
                    {
                        JavaScriptSerializer js = new JavaScriptSerializer();
                        js.MaxJsonLength = Int32.MaxValue;

                        string str = js.Serialize("Invalid Coupon Details");
                        context.Response.Write(str);
                    }
                }
                catch (Exception ex)
                {
                    //call error view here
                    //log the error
                    EventLog.LogErrorData("Error occured MerchantWebService/FinalRedeem." + ex.Message, true);

                    JavaScriptSerializer js = new JavaScriptSerializer();
                    js.MaxJsonLength = Int32.MaxValue;

                    string str = js.Serialize("Error occured while final redeem of coupon");
                    context.Response.Write(str);
                }
            }
            else if (action == "FinalRedeemPoints")
            {
                try
                {
                    string consumeruserid = "";
                    string userid = Convert.ToString(context.Request["userid"]);
                    string RedeemPoints = Convert.ToString(context.Request["RedeemPoints"]);
                    string CustPhoneNumber = Convert.ToString(context.Request["CustPhoneNumber"]);
                    DateTime visitdate = DateTime.Now;

                    if (CustPhoneNumber.Contains('@') == false)
                    {
                        string[] PhonewithCode = CustPhoneNumber.Split(' ');
                        if (PhonewithCode.Length > 0)
                        {
                            if (PhonewithCode.Length == 3)
                            {
                                CustPhoneNumber = "+" + PhonewithCode[1] + " " + PhonewithCode[2].ToString();
                            }
                        }
                    }

                    ApplicationUserManager UserManager = context.GetOwinContext().Get<ApplicationUserManager>();

                    MerchantEntities dataContext = new MerchantEntities();
                    if (UserManager.IsInRole(userid, "Staff"))
                    {
                        merchant_master staffmaster = dataContext.merchant_master.Where(x => x.UserId == userid).FirstOrDefault();
                        staff_master stfmgr = dataContext.staff_master.Where(x => x.StaffMasterId == staffmaster.merchantid).FirstOrDefault();
                        merchant_master owner = dataContext.merchant_master.Where(x => x.merchantid == stfmgr.MerchantId).FirstOrDefault();
                        userid = owner.UserId;
                    }
                    else if (UserManager.IsInRole(userid, "LocationManager"))
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

                                JavaScriptSerializer js = new JavaScriptSerializer();
                                js.MaxJsonLength = Int32.MaxValue;

                                string str = js.Serialize("Consumer does not exist.");
                                context.Response.Write(str);
                            }

                            int pts = Convert.ToInt32(ConsumerdataContext.consumerrewardpoints.Where(x => x.ConsumerId == consumeruserid && x.MerchantId == userid).Select(x => x.Points).Sum());
                            int redeempts = Convert.ToInt32(ConsumerdataContext.consumerredeemdetails.Where(x => x.ConsumerId == consumeruserid && x.MerchantId == userid).Select(x => x.PointsRedeemed).Sum());
                            int availablepts = pts - redeempts;


                            if (availablepts != 0)
                            {
                                if (availablepts > pointstoredeem)
                                {
                                    consumerredeemdetail rdm = new consumerredeemdetail();
                                    rdm.ConsumerId = consumeruserid;
                                    rdm.MerchantId = userid;
                                    rdm.PointsRedeemed = pointstoredeem;
                                    rdm.RedeemDate = DateTime.Now;
                                    ConsumerdataContext.consumerredeemdetails.Add(rdm);
                                    ConsumerdataContext.SaveChanges();

                                    consumervisitdetail visit = new consumervisitdetail();
                                    visit.ConsumerId = consumeruserid;
                                    visit.MerchantId = userid;
                                    visit.VisitDate = visitdate;
                                    ConsumerdataContext.consumervisitdetails.Add(visit);
                                    ConsumerdataContext.SaveChanges();

                                    JavaScriptSerializer js1 = new JavaScriptSerializer();
                                    js1.MaxJsonLength = Int32.MaxValue;

                                    string str1 = js1.Serialize("Points redeemed successfully.");
                                    context.Response.Write(str1);
                                }
                                else
                                {
                                    JavaScriptSerializer js = new JavaScriptSerializer();
                                    js.MaxJsonLength = Int32.MaxValue;

                                    string str = js.Serialize("Available points are less than points to be redeemed.");
                                    context.Response.Write(str);
                                }

                            }
                        }
                    }
                    else
                    {
                        JavaScriptSerializer js = new JavaScriptSerializer();
                        js.MaxJsonLength = Int32.MaxValue;

                        string str = js.Serialize("Invalid redeem points");
                        context.Response.Write(str);
                    }
                }

                catch (Exception ex)
                {
                    //call error view here
                    //log the error
                    EventLog.LogErrorData("Error occured MerchantWebService/FinalRedeemPoints." + ex.Message, true);
                    JavaScriptSerializer js = new JavaScriptSerializer();
                    js.MaxJsonLength = Int32.MaxValue;

                    string str = js.Serialize("Error occured while final redeem of coupon");
                    context.Response.Write(str);
                }
            }
            else if (action == "AddNewDECConsumer")
            {
                try
                {
                    var jsonString = String.Empty;
                    string userid = Convert.ToString(context.Request["userid"]);
                    string mobileno = Convert.ToString(context.Request["mobileno"]);
                    string BillAmt = Convert.ToString(context.Request["BillAmt"]);
                    DateTime visitdate = DateTime.Now;

                    if (mobileno != null)
                    {
                        if (mobileno.Contains('@') == false)
                        {
                            string[] PhonewithCode = mobileno.Split(' ');
                            if (PhonewithCode.Length > 0)
                            {
                                if (PhonewithCode.Length == 3)
                                {
                                    mobileno = "+" + PhonewithCode[1] + " " + PhonewithCode[2].ToString();
                                }
                            }
                        }

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

                        ShareDECParameters param = new ShareDECParameters();
                        param.BillAmt = BillAmt;
                        param.mobileno = mobileno;

                        string result = function.AddNewDECConsumer(param, userid);
                        JavaScriptSerializer js1 = new JavaScriptSerializer();
                        js1.MaxJsonLength = Int32.MaxValue;

                        string str1 = js1.Serialize("Consumer Added Successfully.");
                        context.Response.Write(str1);
                    }
                    else
                    {
                        JavaScriptSerializer js = new JavaScriptSerializer();
                        js.MaxJsonLength = Int32.MaxValue;

                        string str = js.Serialize("Invalid Consumer Details");
                        context.Response.Write(str);
                    }
                }
                catch (Exception ex)
                {
                    //call error view here
                    //log the error
                    EventLog.LogErrorData("Error occured MerchantWebService/AddNewDECConsumer." + ex.Message, true);

                    JavaScriptSerializer js = new JavaScriptSerializer();
                    js.MaxJsonLength = Int32.MaxValue;

                    string str = js.Serialize("Error occured while sending DEC to consumer");
                    context.Response.Write(str);
                }
            }
            else if (action == "AddNewCouponConsumer")
            {
                string mobileno = Convert.ToString(context.Request["mobileno"]);
                string CouponId = Convert.ToString(context.Request["CouponId"]);
                string userid = Convert.ToString(context.Request["userid"]);
                var MerchantUserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(Merchantcontext));
                MerchantEntities dataContext = new MerchantEntities();
                try
                {
                    SMSUtility sms = new SMSUtility();
                    if (mobileno != null)
                    {
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

                        string result = function.AddNewCouponConsumer(mobileno, CouponId, userid);
                        JavaScriptSerializer js1 = new JavaScriptSerializer();
                        js1.MaxJsonLength = Int32.MaxValue;

                        string str1 = js1.Serialize(result);
                        context.Response.Write(str1);
                    }
                    else
                    {
                        JavaScriptSerializer js = new JavaScriptSerializer();
                        js.MaxJsonLength = Int32.MaxValue;

                        string str = js.Serialize("Invalid Consumer Details");
                        context.Response.Write(str);
                    }
                }
                catch (Exception ex)
                {
                    //call error view here
                    //log the error
                    EventLog.LogErrorData("Error occured MerchantWebService/AddNewCouponConsumer." + ex.Message, true);

                    JavaScriptSerializer js = new JavaScriptSerializer();
                    js.MaxJsonLength = Int32.MaxValue;

                    string str = js.Serialize("Error occured while sending coupon to consumer");
                    context.Response.Write(str);
                }
            }
            else if (action == "GetCountries")
            {
                try
                {
                    using (MerchantEntities dataContext = new MerchantEntities())
                    {
                        var countryList = dataContext.country_master.OrderBy(x => x.countryname).ToList();
                        JavaScriptSerializer js = new JavaScriptSerializer();
                        js.MaxJsonLength = Int32.MaxValue;

                        string str = js.Serialize(countryList);
                        context.Response.Write(str);
                    }
                }
                catch (Exception ex)
                {
                    //call error view here
                    //log the error
                    EventLog.LogErrorData("Error occured MerchantWebService/GetCountries." + ex.Message, true);

                    JavaScriptSerializer js = new JavaScriptSerializer();
                    js.MaxJsonLength = Int32.MaxValue;

                    string str = js.Serialize("Error occured while retrieving country list");
                    context.Response.Write(str);
                }
            }
            else if (action == "GetReview")
            {
                try
                {
                    using (MerchantEntities dataContext = new MerchantEntities())
                    {
                        string userid = Convert.ToString(context.Request["userid"]);
                        merchant_master master = dataContext.merchant_master.Where(x => x.UserId == userid).FirstOrDefault();
                        int MerchantId = master.merchantid;
                        var review = dataContext.reviewmasters.Where(x => x.MerchantId == MerchantId).OrderByDescending(x => x.reviewid).FirstOrDefault();

                        JavaScriptSerializer js = new JavaScriptSerializer();
                        js.MaxJsonLength = Int32.MaxValue;

                        string str = js.Serialize(review);
                        context.Response.Write(str);

                    }
                }
                catch (Exception ex)
                {
                    //call error view here
                    //log the error
                    EventLog.LogErrorData("Error occured MerchantWebService/GetReview." + ex.Message, true);

                    JavaScriptSerializer js = new JavaScriptSerializer();
                    js.MaxJsonLength = Int32.MaxValue;

                    string str = js.Serialize("Error occured while retrieving review");
                    context.Response.Write(str);
                }
            }
            else if (action == "getLanguage")
            {
                try
                {
                    string countryid = Convert.ToString(context.Request["countryid"]);
                    using (MerchantEntities dataContext = new MerchantEntities())
                    {
                        int no = Convert.ToInt32(countryid);
                        var Language = dataContext.language_master.Where(x => x.CountryId == no).OrderBy(x => x.LanguageiId).ToList();

                        JavaScriptSerializer js = new JavaScriptSerializer();
                        js.MaxJsonLength = Int32.MaxValue;

                        string str = js.Serialize(Language);
                        context.Response.Write(str);
                    }
                }
                catch (Exception ex)
                {
                    //call error view here
                    //log the error
                    EventLog.LogErrorData("Error occured MerchantWebService/GetLanguage." + ex.Message, true);

                    JavaScriptSerializer js = new JavaScriptSerializer();
                    js.MaxJsonLength = Int32.MaxValue;

                    string str = js.Serialize("Error occured while retrieving language");
                    context.Response.Write(str);
                }
            }
            else if (action == "SaveChangedCountry")
            {
                try
                {
                    using (MerchantEntities dataContext = new MerchantEntities())
                    {
                        string UserId = Convert.ToString(context.Request["UserId"]);
                        string countryid = Convert.ToString(context.Request["countryid"]);
                        string langid = Convert.ToString(context.Request["langid"]);

                        var merchant = dataContext.merchant_master.Where(x => x.UserId == UserId).FirstOrDefault();
                        merchant.Country = Convert.ToInt32(countryid);
                        merchant.LanguageId = Convert.ToInt32(langid);
                        dataContext.SaveChanges();

                        JavaScriptSerializer js = new JavaScriptSerializer();
                        js.MaxJsonLength = Int32.MaxValue;

                        string str = js.Serialize("Country Updated");
                        context.Response.Write(str);

                    }
                    JavaScriptSerializer js1 = new JavaScriptSerializer();
                    js1.MaxJsonLength = Int32.MaxValue;

                    string str1 = js1.Serialize("Country Changed Successfully!");
                    context.Response.Write(str1);
                }
                catch (Exception ex)
                {
                    //call error view here
                    //log the error
                    EventLog.LogErrorData("Error occured MerchantWebService/SaveChangedCountry." + ex.Message, true);

                    JavaScriptSerializer js = new JavaScriptSerializer();
                    js.MaxJsonLength = Int32.MaxValue;

                    string str = js.Serialize("Error occured while saving regional settings");
                    context.Response.Write(str);
                }
            }
            else if (action == "GetStates")
            {
                try
                {
                    string countryid = Convert.ToString(context.Request["countryid"]);
                    using (MerchantEntities dataContext = new MerchantEntities())
                    {
                        int no = Convert.ToInt32(countryid);
                        var States = dataContext.state_master.Where(x => x.countryid == no).OrderBy(x => x.state).ToList();

                        JavaScriptSerializer js = new JavaScriptSerializer();
                        js.MaxJsonLength = Int32.MaxValue;

                        string str = js.Serialize(States);
                        context.Response.Write(str);
                    }
                }
                catch (Exception ex)
                {
                    //call error view here
                    //log the error
                    EventLog.LogErrorData("Error occured MerchantWebService/GetStates." + ex.Message, true);

                    JavaScriptSerializer js = new JavaScriptSerializer();
                    js.MaxJsonLength = Int32.MaxValue;

                    string str = js.Serialize("Error occured while retrieving states list from country");
                    context.Response.Write(str);
                }
            }
            else if (action == "GetCities")
            {
                try
                {
                    string stateid = Convert.ToString(context.Request["stateid"]);
                    using (MerchantEntities dataContext = new MerchantEntities())
                    {
                        int no = Convert.ToInt32(stateid);
                        var cityList = dataContext.city_master.Where(x => x.stateid == no).OrderBy(x => x.City).ToList();

                        JavaScriptSerializer js = new JavaScriptSerializer();
                        js.MaxJsonLength = Int32.MaxValue;

                        string str = js.Serialize(cityList);
                        context.Response.Write(str);
                    }
                }
                catch (Exception ex)
                {
                    //call error view here
                    //log the error
                    EventLog.LogErrorData("Error occured MerchantWebService/GetCities." + ex.Message, true);

                    JavaScriptSerializer js = new JavaScriptSerializer();
                    js.MaxJsonLength = Int32.MaxValue;

                    string str = js.Serialize("Error occured while retrieving city list from state");
                    context.Response.Write(str);
                }
            }
            else if (action == "DownloadConsumerSample")
            {
                try
                {
                    using (MerchantEntities dataContext = new MerchantEntities())
                    {
                        menu_csv_template template = dataContext.menu_csv_template.Where(x => x.templatefor == "Consumer").FirstOrDefault();
                        if (template != null)
                        {
                            Byte[] bytes = (Byte[])template.csv_template;
                            context.Response.Buffer = true;
                            context.Response.BufferOutput = true;
                            context.Response.Charset = "";
                            context.Response.Cache.SetCacheability(HttpCacheability.NoCache);
                            context.Response.ContentType = template.ContentType;
                            context.Response.AddHeader("content-disposition", "attachment;filename=" + template.FileName);
                            context.Response.BinaryWrite(bytes);
                            context.Response.Flush();
                            context.Response.End();

                        }
                        else
                        {

                        }
                    }
                }
                catch (Exception ex)
                {
                    if (ex.Message != "Thread was being aborted.")
                    {
                        EventLog.LogErrorData("Error occurred in MerchantWebService/DownloadConsumerSample: " + ex.Message, true);
                        JavaScriptSerializer js = new JavaScriptSerializer();
                        js.MaxJsonLength = Int32.MaxValue;

                        string str = js.Serialize("Error occurred in downloading sample csv file");
                        context.Response.Write(str);
                    }
                }
            }
            else if (action == "UploadConsumerFile")
            {
                try
                {
                    HttpPostedFile file = context.Request.Files["Filedata"];
                    string userid = Convert.ToString(context.Request["userid"]);
                    string hdnCouponId = Convert.ToString(context.Request["CouponId"]);
                    string group = Convert.ToString(context.Request["group"]);

                    ApplicationUserManager MerchantUserManager = context.GetOwinContext().Get<ApplicationUserManager>();


                    if (file != null && file.ContentLength > 0)
                    {
                        DataTable dt = new DataTable();
                        using (StreamReader sr = new StreamReader(file.InputStream))
                        {
                            string[] headers = sr.ReadLine().Split(',');
                            if (headers.Count() > 1)
                            {
                                EventLog.LogErrorData("Error occurred in MerchantWebService/UploadConsumerFile. CSV file is not in correct format. Please download sample file and try again.", true);

                                JavaScriptSerializer js = new JavaScriptSerializer();
                                js.MaxJsonLength = Int32.MaxValue;

                                string str = js.Serialize("CSV file is not in correct format. Please download sample file and try again.");
                                context.Response.Write(str);
                            }
                            else
                            {
                                foreach (string header in headers)
                                {
                                    if (header != "PhoneNumber")
                                    {
                                        EventLog.LogErrorData("Error occurred in Merchant/UploadConsumerFile. CSV file is not in correct format. Please download sample file and try again.", true);

                                        JavaScriptSerializer js = new JavaScriptSerializer();
                                        js.MaxJsonLength = Int32.MaxValue;

                                        string str = js.Serialize("CSV file is not in correct format. Please download sample file and try again.");
                                        context.Response.Write(str);
                                        break;
                                    }
                                    dt.Columns.Add(header);
                                }
                                if (dt.Columns.Count > 0)
                                {
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
                            }

                            using (MerchantEntities dataContext = new MerchantEntities())
                            {
                                if (userid != null)
                                {
                                    if (dt != null)
                                    {
                                        if (dt.Rows.Count > 0)
                                        {
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

                                            for (int i = 0; i < dt.Rows.Count; i++)
                                            {
                                                if (hdnCouponId != "0")
                                                {
                                                    string result = function.AddNewCouponConsumer(dt.Rows[i][0].ToString(), hdnCouponId, userid);
                                                }
                                                else
                                                {
                                                    ShareDECParameters param = new ShareDECParameters();
                                                    param.mobileno = dt.Rows[i][0].ToString();
                                                    param.BillAmt = "0";
                                                    string result = function.AddNewDECConsumer(param, userid);
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
                                                JavaScriptSerializer js = new JavaScriptSerializer();
                                                js.MaxJsonLength = Int32.MaxValue;

                                                string str = js.Serialize("Coupon shared successfully to list of consumers.");
                                                context.Response.Write(str);
                                            }
                                            else
                                            {
                                                JavaScriptSerializer js = new JavaScriptSerializer();
                                                js.MaxJsonLength = Int32.MaxValue;

                                                string str = js.Serialize("DEC shared successfully to list of consumers.");
                                                context.Response.Write(str);
                                            }
                                        }
                                        else
                                        {
                                            JavaScriptSerializer js = new JavaScriptSerializer();
                                            js.MaxJsonLength = Int32.MaxValue;

                                            string str = js.Serialize("CSV file is not in correct format. Please download sample file and try again.");
                                            context.Response.Write(str);
                                        }
                                    }
                                    else
                                    {
                                        JavaScriptSerializer js = new JavaScriptSerializer();
                                        js.MaxJsonLength = Int32.MaxValue;

                                        string str = js.Serialize("CSV file is not in correct format. Please download sample file and try again.");
                                        context.Response.Write(str);
                                    }
                                }
                                else
                                {

                                    JavaScriptSerializer js = new JavaScriptSerializer();
                                    js.MaxJsonLength = Int32.MaxValue;

                                    string str = js.Serialize("CSV file is not in correct format. Please download sample file and try again.");
                                    context.Response.Write(str);
                                }
                            }
                        }

                    }
                    else
                    {

                        JavaScriptSerializer js = new JavaScriptSerializer();
                        js.MaxJsonLength = Int32.MaxValue;

                        string str = js.Serialize("CSV file is not in correct format. Please download sample file and try again.");
                        context.Response.Write(str);
                    }
                }
                catch (Exception ex)
                {
                    EventLog.LogErrorData("Error occurred in MerchantWebService/UploadConsumerFile: " + ex.Message, true);
                    JavaScriptSerializer js = new JavaScriptSerializer();
                    js.MaxJsonLength = Int32.MaxValue;

                    string str = js.Serialize("Error occured while uploading file.");
                    context.Response.Write(str);
                }
            }
            else if (action == "SendDecToFriends")
            {
                string result = "";
                DateTime visitdate = DateTime.Now;
                string Mobileno = Convert.ToString(context.Request["Mobileno"]);
                string userid = Convert.ToString(context.Request["userid"]);
                try
                {
                    ApplicationUserManager MerchantUserManager = context.GetOwinContext().Get<ApplicationUserManager>();
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


                    if (Mobileno != null)
                    {
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
                                ShareDECParameters param = new ShareDECParameters();
                                param.mobileno = cellno;
                                param.BillAmt = "";

                                result = function.AddNewDECConsumer(param, userid);
                            }
                            JavaScriptSerializer js = new JavaScriptSerializer();
                            js.MaxJsonLength = Int32.MaxValue;

                            string str = js.Serialize(result);
                            context.Response.Write(str);
                        }
                        else
                        {
                            EventLog.LogErrorData("Error occurred in MerchantWebService/SendDecToFriends: Cell nos not given", true);
                            JavaScriptSerializer js = new JavaScriptSerializer();
                            js.MaxJsonLength = Int32.MaxValue;

                            string str = js.Serialize("Cell numbers not passed");
                            context.Response.Write(str);
                        }
                    }
                    else
                    {
                        EventLog.LogErrorData("Error occurred in MerchantWebService/SendDecToFriends: Cell nos not given", true);
                        JavaScriptSerializer js = new JavaScriptSerializer();
                        js.MaxJsonLength = Int32.MaxValue;

                        string str = js.Serialize("Cell numbers not passed");
                        context.Response.Write(str);
                    }
                }
                catch (Exception ex)
                {
                    EventLog.LogErrorData("Error occurred in MerchantWebService/SendDecToFriends: " + ex.Message, true);
                    JavaScriptSerializer js = new JavaScriptSerializer();
                    js.MaxJsonLength = Int32.MaxValue;

                    string str = js.Serialize("Error occured while sending DEC to friends.");
                    context.Response.Write(str);
                }
            }
            else if (action == "GetAllGroups")
            {
                try
                {
                    using (MerchantEntities dataContext = new MerchantEntities())
                    {
                        string userid = Convert.ToString(context.Request["userid"]);
                        merchant_master master = dataContext.merchant_master.Where(x => x.UserId == userid).FirstOrDefault();
                        if (master != null)
                        {
                            var grouplist = dataContext.consumergroups.Where(x => x.MerchantId == userid).Select(x => new { GroupName = x.GroupName }).Distinct().ToList();

                            JavaScriptSerializer js = new JavaScriptSerializer();
                            js.MaxJsonLength = Int32.MaxValue;

                            string str = js.Serialize(grouplist);
                            context.Response.Write(str);
                        }
                        else
                        {
                            EventLog.LogErrorData("Error occurred in MerchantWebService/SendDecToFriends: " + Global.Merchant.InvalidMerchantDetails, true);
                            JavaScriptSerializer js = new JavaScriptSerializer();
                            js.MaxJsonLength = Int32.MaxValue;

                            string str = js.Serialize("Invalid Merchant Details.");
                            context.Response.Write(str);
                        }
                    }
                }
                catch (Exception ex)
                {
                    //call error view here
                    //log the error
                    EventLog.LogErrorData("Error occurred MerchantWebService/GetAllGroups." + ex.Message, true);
                    JavaScriptSerializer js = new JavaScriptSerializer();
                    js.MaxJsonLength = Int32.MaxValue;

                    string str = js.Serialize("Error occured while getting group names for merchant.");
                    context.Response.Write(str);

                }
            }
            else if (action == "sendCouponToGroups")
            {
                try
                {
                    string groupnames = Convert.ToString(context.Request["groupnames"]);
                    string userid = Convert.ToString(context.Request["userid"]);
                    string couponid = Convert.ToString(context.Request["couponid"]);

                    if (groupnames != "")
                    {
                        if (groupnames.LastIndexOf(",") > 0)
                            groupnames = groupnames.Remove(groupnames.LastIndexOf(","));

                        string[] groups = groupnames.Split(',');
                        if (groups.Count() > 0)
                        {
                            using (MerchantEntities dataContext = new MerchantEntities())
                            {
                                string response = "";
                                for (int i = 0; i < groups.Count(); i++)
                                {
                                    string name = groups[i];
                                    List<consumergroup> grp = dataContext.consumergroups.Where(x => x.MerchantId == userid && x.GroupName == name).ToList();
                                    foreach (consumergroup g in grp)
                                    {
                                        if (couponid != "0")
                                        {
                                            string result = function.AddNewCouponConsumer(g.ConsumerUserId, couponid, userid);
                                        }
                                        else
                                        {
                                            ShareDECParameters param = new ShareDECParameters();
                                            param.mobileno = g.ConsumerUserId;
                                            param.BillAmt = "0";
                                            string result = function.AddNewDECConsumer(param, userid);
                                        }
                                        if (couponid != "0")
                                        {
                                            response = "Coupon shared successfully to group of consumers.";
                                        }
                                        else
                                        {
                                            response = "DEC shared successfully to group of consumers.";
                                        }
                                    }
                                }
                                JavaScriptSerializer js = new JavaScriptSerializer();
                                js.MaxJsonLength = Int32.MaxValue;

                                string str = js.Serialize(response);
                                context.Response.Write(str);
                            }
                        }
                        else
                        {
                            JavaScriptSerializer js = new JavaScriptSerializer();
                            js.MaxJsonLength = Int32.MaxValue;

                            string str = js.Serialize("No groups with selected names.");
                            context.Response.Write(str);
                        }
                    }
                    else
                    {
                        JavaScriptSerializer js = new JavaScriptSerializer();
                        js.MaxJsonLength = Int32.MaxValue;

                        string str = js.Serialize("No group names specified.");
                        context.Response.Write(str);
                    }
                }
                catch (Exception ex)
                {
                    EventLog.LogErrorData("Error occurred in MerchantWebService/sendCouponToGroups: " + ex.Message, true);
                    JavaScriptSerializer js = new JavaScriptSerializer();
                    js.MaxJsonLength = Int32.MaxValue;

                    string str = js.Serialize("Error occured while sending coupon/DEC to groups of consumers.");
                    context.Response.Write(str);

                }
            }
            else if (action == "sendGiftCardOTP")
            {
                using (instadelight_consumerEntities datacontext = new instadelight_consumerEntities())
                {
                    string Id = Convert.ToString(context.Request["Id"]);
                    string userid = Convert.ToString(context.Request["userid"]);

                    int no = Convert.ToInt32(Id);
                    try
                    {
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
                                        JavaScriptSerializer js = new JavaScriptSerializer();
                                        js.MaxJsonLength = Int32.MaxValue;

                                        string str = js.Serialize("OTP Sent Successfully.");
                                        context.Response.Write(str);
                                    }
                                    else
                                    {
                                        JavaScriptSerializer js = new JavaScriptSerializer();
                                        js.MaxJsonLength = Int32.MaxValue;

                                        string str = js.Serialize("Error occurred while sending OTP to email id " + consumer.UserName);
                                        context.Response.Write(str);
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

                                        JavaScriptSerializer js = new JavaScriptSerializer();
                                        js.MaxJsonLength = Int32.MaxValue;

                                        string str = js.Serialize("OTP Sent Successfully.");
                                        context.Response.Write(str);
                                    }
                                    else
                                    {
                                        JavaScriptSerializer js = new JavaScriptSerializer();
                                        js.MaxJsonLength = Int32.MaxValue;

                                        string str = js.Serialize("Error occurred while sending OTP to " + consumer.UserName);
                                        context.Response.Write(str);
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        //call error view here
                        //log the error
                        EventLog.LogErrorData("Error occurred MerchantWebService/VerifyAndRedeemGiftCard." + ex.Message, true);
                        JavaScriptSerializer js = new JavaScriptSerializer();
                        js.MaxJsonLength = Int32.MaxValue;

                        string str = js.Serialize("Error occurred while sending OTP to redeem gift card");
                        context.Response.Write(str);
                    }
                }
            }
            else if (action == "VerifyGiftcardOTP")
            {
                try
                {
                    string userid = Convert.ToString(context.Request["userid"]);
                    string GiftCardId = Convert.ToString(context.Request["GiftCardId"]);
                    string OTP = Convert.ToString(context.Request["OTP"]);

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
                                JavaScriptSerializer js = new JavaScriptSerializer();
                                js.MaxJsonLength = Int32.MaxValue;

                                string str = js.Serialize("OTP Verified");
                                context.Response.Write(str);
                            }
                            else
                            {
                                JavaScriptSerializer js = new JavaScriptSerializer();
                                js.MaxJsonLength = Int32.MaxValue;

                                string str = js.Serialize("Invalid OTP");
                                context.Response.Write(str);
                            }
                        }
                        else
                        {
                            JavaScriptSerializer js = new JavaScriptSerializer();
                            js.MaxJsonLength = Int32.MaxValue;

                            string str = js.Serialize("Invalid OTP");
                            context.Response.Write(str);
                        }
                    }
                }
                catch (Exception ex)
                {
                    //call error view here
                    //log the error
                    EventLog.LogErrorData("Error occurred MerchantWebService/VerifyGiftcardOTP." + ex.Message, true);
                    JavaScriptSerializer js = new JavaScriptSerializer();
                    js.MaxJsonLength = Int32.MaxValue;

                    string str = js.Serialize("Error occurred while verifying OTP");
                    context.Response.Write(str);
                }
            }
            else if (action == "Upload")
            {
                try
                {
                    using (MerchantEntities dataContext = new MerchantEntities())
                    {
                        string userid = Convert.ToString(context.Request["userid"]);
                        HttpPostedFile file = context.Request.Files["Filedata"];

                        ApplicationUserManager UserManager = context.GetOwinContext().Get<ApplicationUserManager>();

                        merchant_master master = dataContext.merchant_master.Where(x => x.UserId == userid).FirstOrDefault();
                        if (UserManager.IsInRole(userid, "LocationManager"))
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
                                                JavaScriptSerializer js = new JavaScriptSerializer();
                                                js.MaxJsonLength = Int32.MaxValue;

                                                string str = js.Serialize("File uploaded is not in correct format.");
                                                context.Response.Write(str);
                                            }
                                            if (values[0] != "PackageName" || values[1] != "Price" || values[2] != "Discount" || values[3] != "Valid" || values[4] != "Available")
                                            {
                                                JavaScriptSerializer js = new JavaScriptSerializer();
                                                js.MaxJsonLength = Int32.MaxValue;

                                                string str = js.Serialize("File uploaded is not in correct format.");
                                                context.Response.Write(str);
                                            }
                                        }
                                        else
                                        {
                                            if (values.Length != 9)
                                            {
                                                JavaScriptSerializer js = new JavaScriptSerializer();
                                                js.MaxJsonLength = Int32.MaxValue;

                                                string str = js.Serialize("File uploaded is not in correct format.");
                                                context.Response.Write(str);
                                            }
                                            if (values[0] != "Category" || values[1] != "Subcategory" || values[2] != "ItemCode" || values[3] != "ItemName" || values[4] != "ItemPrice" || values[5] != "ItemDiscription" || values[6] != "GST" || values[7] != "PackagingCharges" || values[8] != "DeliveryCharge")
                                            {
                                                JavaScriptSerializer js = new JavaScriptSerializer();
                                                js.MaxJsonLength = Int32.MaxValue;

                                                string str = js.Serialize("File uploaded is not in correct format.");
                                                context.Response.Write(str);
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

                                JavaScriptSerializer js1 = new JavaScriptSerializer();
                                js1.MaxJsonLength = Int32.MaxValue;

                                string str1 = js1.Serialize("Menu uploaded successfully.");
                                context.Response.Write(str1);

                            }
                            else
                            {
                                EventLog.LogErrorData("file is null of file.ContentLength = 0", true);
                                JavaScriptSerializer js1 = new JavaScriptSerializer();
                                js1.MaxJsonLength = Int32.MaxValue;

                                string str1 = js1.Serialize("File is null.");
                                context.Response.Write(str1);
                            }
                        }
                        else
                        {
                            EventLog.LogErrorData("Merchant does not exist.", true);
                            JavaScriptSerializer js1 = new JavaScriptSerializer();
                            js1.MaxJsonLength = Int32.MaxValue;

                            string str1 = js1.Serialize("Merchant does not exist.");
                            context.Response.Write(str1);
                        }
                    }
                }
                catch (Exception ex)
                {
                    EventLog.LogErrorData("Error occurred in MerchantWebService/Upload: " + ex.Message, true);
                    JavaScriptSerializer js1 = new JavaScriptSerializer();
                    js1.MaxJsonLength = Int32.MaxValue;

                    string str1 = js1.Serialize("Error occurred while uploading menu.");
                    context.Response.Write(str1);
                }
            }
            else if (action == "Download")
            {
                try
                {
                    string userid = Convert.ToString(context.Request["userid"]);
                    using (MerchantEntities dataContext = new MerchantEntities())
                    {
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
                            Byte[] bytes = (Byte[])template.csv_template;
                            context.Response.Buffer = true;
                            context.Response.BufferOutput = true;
                            context.Response.Charset = "";
                            context.Response.Cache.SetCacheability(HttpCacheability.NoCache);
                            context.Response.ContentType = template.ContentType;
                            context.Response.AddHeader("content-disposition", "attachment;filename=" + template.FileName);
                            context.Response.BinaryWrite(bytes);
                            context.Response.Flush();
                            context.Response.End();
                        }
                        else
                        {

                        }
                    }
                }
                catch (Exception ex)
                {
                    EventLog.LogErrorData("Error occurred in MerchantWebService/Download: " + ex.Message, true);
                    JavaScriptSerializer js = new JavaScriptSerializer();
                    js.MaxJsonLength = Int32.MaxValue;

                    string str = js.Serialize("Error occurred in downloading sample csv file");
                    context.Response.Write(str);
                }
            }
            else if (action == "getBrands")
            {
                try
                {
                    string userid = Convert.ToString(context.Request["userid"]);
                    ApplicationUserManager UserManager = context.GetOwinContext().Get<ApplicationUserManager>();

                    using (MerchantEntities dataContext = new MerchantEntities())
                    {

                        if (UserManager.IsInRole(userid, "LocationManager"))
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

                                JavaScriptSerializer js = new JavaScriptSerializer();
                                js.MaxJsonLength = Int32.MaxValue;

                                string str = js.Serialize(brand);
                                context.Response.Write(str);

                                return;
                            }
                            else
                            {
                                JavaScriptSerializer js = new JavaScriptSerializer();
                                js.MaxJsonLength = Int32.MaxValue;
                                context.Response.Write("Error occurred while retrieving merchant");

                                return;
                            }
                        }
                        else if (UserManager.IsInRole(userid, "BrandManager") && !UserManager.IsInRole(userid, "Merchant"))
                        {
                            merchant_master locmaster = dataContext.merchant_master.Where(x => x.UserId == userid).FirstOrDefault();
                            brand_master brand = dataContext.brand_master.Where(x => x.BrandManagerId == locmaster.merchantid).FirstOrDefault();

                            if (brand != null)
                            {
                                List<brand_master> brandList = new List<brand_master>();

                                brand.branches = dataContext.branch_master.Where(x => x.BrandId == brand.BrandId).ToList();


                                brandList.Add(brand);

                                JavaScriptSerializer js = new JavaScriptSerializer();
                                js.MaxJsonLength = Int32.MaxValue;

                                string str = js.Serialize(brand);
                                context.Response.Write("Error occurred while retrieving merchant");

                                return;
                            }
                            else
                            {
                                JavaScriptSerializer js = new JavaScriptSerializer();
                                js.MaxJsonLength = Int32.MaxValue;
                                context.Response.Write("Error occurred while retrieving merchant");

                                return;
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

                                JavaScriptSerializer js = new JavaScriptSerializer();
                                js.MaxJsonLength = Int32.MaxValue;

                                string str = js.Serialize(brandList);
                                context.Response.Write("Error occurred while retrieving merchant");

                                return;
                            }
                            else
                            {
                                JavaScriptSerializer js = new JavaScriptSerializer();
                                js.MaxJsonLength = Int32.MaxValue;

                                context.Response.Write("Error occurred while retrieving merchant");

                                return;
                            }
                        }

                    }
                }
                catch (Exception ex)
                {
                    //call error view here
                    //log the error
                    EventLog.LogErrorData("Error occurred MerchantWebService/getBrands." + ex.Message, true);
                    JavaScriptSerializer js = new JavaScriptSerializer();
                    js.MaxJsonLength = Int32.MaxValue;

                    context.Response.Write("Error occurred while retrieving brand list");
                }
            }
        }


        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}