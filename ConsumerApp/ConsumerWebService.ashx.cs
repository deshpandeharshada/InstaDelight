using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ConsumerApp.Models;
using Microsoft.AspNet.Identity;
using AspNet.Identity.MySQL;
using MySql.Data.MySqlClient;
using System.Web.Script.Serialization;
using System.IO;
using ConsumerApp.Controllers;
using System.Web.Security;
using Microsoft.Owin.Security;

using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity.Owin;



using Newtonsoft.Json;


namespace ConsumerApp
{
    /// <summary>
    /// Summary description for ConsumerWebService
    /// </summary>
    /// 

    public class ConsumerWebService : IHttpHandler
    {
        ConsumerCommonFunctions function = new ConsumerCommonFunctions();
        public void ProcessRequest(HttpContext context)
        {
            context.Response.AddHeader("Access-Control-Allow-Headers", "*");
            context.Response.AddHeader("Access-Control-Allow-Origin", "*");
            context.Response.AddHeader("Access-Control-Allow-Methods", "*");


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
                            instadelight_consumerEntities dataContext = new instadelight_consumerEntities();
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
                                    consumermaster consumer = dataContext.consumermasters.Where(x => x.UserId == userId).FirstOrDefault();
                                    loginstatus = consumer.UserId;
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
                    EventLog.LogErrorData("Error occurred ConsumerWebService/Login." + ex.Message, true);
                    JavaScriptSerializer js = new JavaScriptSerializer();
                    js.MaxJsonLength = Int32.MaxValue;

                    string str = js.Serialize("Error occurred while logging in from web service.");
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
                        EventLog.LogErrorData("Error occurred ConsumerWebService/ResetPassword. User does not exist.", true);
                        JavaScriptSerializer js = new JavaScriptSerializer();
                        js.MaxJsonLength = Int32.MaxValue;

                        string str = js.Serialize("User with this phone number does not exist.");
                        context.Response.Write(str);
                    }
                    string code = UserManager.GeneratePasswordResetToken(user.Id);
                    var result = UserManager.ResetPassword(user.Id, code, newpwd);
                    if (result.Succeeded)
                    {
                        instadelight_consumerEntities dataContext = new instadelight_consumerEntities();
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

                        string str = js.Serialize(Global.Consumer.ResetMessage);
                        context.Response.Write(str);
                    }
                }
                catch (Exception ex)
                {
                    EventLog.LogErrorData("Error occurred ConsumerWebService/ResetPassword." + ex.Message, true);
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
                        EventLog.LogErrorData("Error occurred ConsumerWebService/ForgotPassword. User does not exist.", true);
                        JavaScriptSerializer js = new JavaScriptSerializer();
                        js.MaxJsonLength = Int32.MaxValue;

                        string str = js.Serialize("User with this phone number does not exist.");
                        context.Response.Write(str);
                    }

                    string code = UserManager.GeneratePasswordResetToken(user.Id);

                    string newpwd = RandomNumber.GenerateRandomOTP(6);

                    var result = UserManager.ResetPassword(user.Id, code, newpwd);
                    if (result.Succeeded)
                    {
                        instadelight_consumerEntities dataContext = new instadelight_consumerEntities();
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
                            string smsresult = sms.sendMessage(model.Phone, "Dear Customer, your password to access Offertraker App is " + newpwd.ToString());
                            EventLog.LogData("New password " + newpwd, true);

                            if (smsresult.Contains("SMS sent successfully"))
                            {
                                JavaScriptSerializer js = new JavaScriptSerializer();
                                js.MaxJsonLength = Int32.MaxValue;

                                string str = js.Serialize(@Global.Consumer.ForgotPasswordMessage);
                                context.Response.Write(str);
                            }
                        }
                        else
                        {
                            EmailModel mailmodel = new EmailModel();
                            mailmodel.To = model.Phone;
                            mailmodel.Email = "no-reply@offertraker.com";
                            mailmodel.Subject = "Your password has been reset";
                            mailmodel.Body = "Dear " + model.Phone + ", <br /><br />Your password has been reset successfully. Your new login credentials are : <br /> User Name : " + user.UserName + " <br /> Password: " + newpwd + " <br /> Enjoy the offers <br /><br />Offertraker team";
                            SendEmail email = new SendEmail();
                            string mailresult = email.SendEmailToConsumer(mailmodel);
                            if (mailresult.Contains("Email sent"))
                            {
                                JavaScriptSerializer js = new JavaScriptSerializer();
                                js.MaxJsonLength = Int32.MaxValue;

                                string str = js.Serialize(@Global.Consumer.ForgotPasswordMessage);
                                context.Response.Write(str);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    EventLog.LogErrorData("Error occurred ConsumerWebService/ForgotPassword." + ex.Message, true);
                    JavaScriptSerializer js = new JavaScriptSerializer();
                    js.MaxJsonLength = Int32.MaxValue;

                    string str = js.Serialize("Error occurred while setting new password from web service.");
                    context.Response.Write(str);
                }
            }
            else if (action == "Verifyphoneotp")
            {
                try
                {
                    string userid = Convert.ToString(context.Request["userid"]);
                    string OTP = Convert.ToString(context.Request["OTP"]);
                    string OptionalPhone = Convert.ToString(context.Request["OptionalPhone"]);

                    using (instadelight_consumerEntities dataContext = new instadelight_consumerEntities())
                    {

                        if (string.IsNullOrEmpty(OTP) == false)
                        {
                            if (OptionalPhone == "Phone2")
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
                                consumer_otp_details otpdetail = dataContext.consumer_otp_details.Where(x => x.ConsumerId == userid && x.Phone3OTP == OTP).FirstOrDefault();
                                if (otpdetail != null)
                                {
                                    //Valid OTP
                                    otpdetail.Phone2OTP = "";
                                    dataContext.SaveChanges();
                                    consumermaster consumer = dataContext.consumermasters.Where(x => x.UserId == userid).FirstOrDefault();
                                    consumer.Phone3Verified = true;
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
                    EventLog.LogErrorData("Error occurred ConsumerWebService/Verifyphoneotp." + ex.Message, true);

                    JavaScriptSerializer js = new JavaScriptSerializer();
                    js.MaxJsonLength = Int32.MaxValue;

                    string str = js.Serialize("Error occurred while verifying phone OTP");
                    context.Response.Write(str);
                }

            }
            else if (action == "sendOTPtoPhone")
            {
                try
                {
                    string userid = Convert.ToString(context.Request["userid"]);
                    string PhoneNumber = Convert.ToString(context.Request["PhoneNumber"]);
                    string OptionalPhone = Convert.ToString(context.Request["OptionalPhone"]);

                    // string phone2 = "";
                    using (instadelight_consumerEntities dataContext = new instadelight_consumerEntities())
                    {
                        // phone2 = dataContext.consumermasters.Where(x => x.UserId == userid).Select(x => x.Phone2).FirstOrDefault();

                        if (PhoneNumber != "")
                        {
                            //Generate OTP
                            string newotp = "";
                            consumer_otp_details currentotpdetails = dataContext.consumer_otp_details.Where(x => x.ConsumerId == userid).FirstOrDefault();
                            if (currentotpdetails != null)
                            {
                                if (OptionalPhone == "Phone2")
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
                                    if (currentotpdetails.Phone3OTP != null)
                                    {
                                        newotp = currentotpdetails.Phone3OTP;
                                    }
                                    else
                                    {
                                        newotp = RandomNumber.GenerateRandomOTP(6);
                                    }
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
                                        if (OptionalPhone == "Phone2")
                                        {
                                            otpdetails.Phone2OTP = newotp;
                                        }
                                        else
                                        {
                                            otpdetails.Phone3OTP = newotp;
                                        }
                                        dataContext.consumer_otp_details.Add(otpdetails);
                                        dataContext.SaveChanges();
                                    }
                                    else
                                    {
                                        otpdetails.Phone2OTP = newotp;
                                        dataContext.SaveChanges();
                                    }
                                    JavaScriptSerializer js = new JavaScriptSerializer();
                                    js.MaxJsonLength = Int32.MaxValue;

                                    string str = js.Serialize("OTP send successfully");
                                    context.Response.Write(str);
                                }
                                else
                                {
                                    JavaScriptSerializer js = new JavaScriptSerializer();
                                    js.MaxJsonLength = Int32.MaxValue;

                                    string str = js.Serialize("Not able to send OTP on cell number " + PhoneNumber + ". Please try again later.");
                                    context.Response.Write(str);
                                }
                            }
                            else
                            {
                                JavaScriptSerializer js = new JavaScriptSerializer();
                                js.MaxJsonLength = Int32.MaxValue;

                                string str = js.Serialize("Not able to generate OTP. Please try again later.");
                                context.Response.Write(str);
                            }

                        }
                        else
                        {
                            JavaScriptSerializer js = new JavaScriptSerializer();
                            js.MaxJsonLength = Int32.MaxValue;

                            string str = js.Serialize("Invalid phone number");
                            context.Response.Write(str);
                        }
                    }
                }
                catch (Exception ex)
                {
                    //call error view here
                    //log the error
                    EventLog.LogErrorData("Error occurred ConsumerWebService/sendOTPtoPhone2." + ex.Message, true);
                    JavaScriptSerializer js = new JavaScriptSerializer();
                    js.MaxJsonLength = Int32.MaxValue;

                    string str = js.Serialize("Error occurred while sending OTP to phone");
                    context.Response.Write(str);
                }
            }
            else if (action == "sendOTPtoEmail")
            {
                try
                {
                    string userid = Convert.ToString(context.Request["userid"]);
                    string EmailId = Convert.ToString(context.Request["EmailId"]);

                    using (instadelight_consumerEntities dataContext = new instadelight_consumerEntities())
                    {
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
                                    JavaScriptSerializer js = new JavaScriptSerializer();
                                    js.MaxJsonLength = Int32.MaxValue;

                                    string str = js.Serialize("OTP send successfully");
                                    context.Response.Write(str);
                                }
                                else
                                {
                                    JavaScriptSerializer js = new JavaScriptSerializer();
                                    js.MaxJsonLength = Int32.MaxValue;

                                    string str = js.Serialize("Not able to send OTP to email id " + EmailId + ". Please try again later.");
                                    context.Response.Write(str);
                                }
                            }
                            else
                            {
                                JavaScriptSerializer js = new JavaScriptSerializer();
                                js.MaxJsonLength = Int32.MaxValue;

                                string str = js.Serialize("Not able to generate OTP. Please try again later.");
                                context.Response.Write(str);
                            }

                        }
                        else
                        {
                            JavaScriptSerializer js = new JavaScriptSerializer();
                            js.MaxJsonLength = Int32.MaxValue;

                            string str = js.Serialize("Invalid Email");
                            context.Response.Write(str);
                        }
                    }
                }
                catch (Exception ex)
                {
                    //call error view here
                    //log the error
                    EventLog.LogErrorData("Error occurred ConsumerWebService/sendOTPtoEmail." + ex.Message, true);
                    JavaScriptSerializer js = new JavaScriptSerializer();
                    js.MaxJsonLength = Int32.MaxValue;

                    string str = js.Serialize("Error occurred while sending OTP over email");
                    context.Response.Write(str);
                }
            }
            else if (action == "VerifyEmailOTP")
            {
                string userid = Convert.ToString(context.Request["userid"]);
                string OTP = Convert.ToString(context.Request["OTP"]);

                try
                {
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
                    EventLog.LogErrorData("Error occurred ConsumerWebService/VerifyEmailOTP." + ex.Message, true);
                    JavaScriptSerializer js = new JavaScriptSerializer();
                    js.MaxJsonLength = Int32.MaxValue;

                    string str = js.Serialize("Error occurred while verifying email OTP");
                    context.Response.Write(str);
                }
            }
            else if (action == "GetPendingReviews")
            {
                try
                {
                    string userid = Convert.ToString(context.Request["userid"]);
                    using (consumerEntities dataContext = new consumerEntities())
                    {
                        int noofreviews = dataContext.merchantconsumerreviewdetails.Where(x => x.ConsumerId == userid && x.Status == "Shared" && x.ValidTill > DateTime.Now).Select(x => x.ReviewId).Distinct().Count();
                        JavaScriptSerializer js = new JavaScriptSerializer();
                        js.MaxJsonLength = Int32.MaxValue;

                        string str = js.Serialize(noofreviews);
                        context.Response.Write(str);
                    }
                }
                catch (Exception ex)
                {
                    EventLog.LogErrorData("Error occurred ConsumerWebService/getPendingReviews." + ex.Message, true);
                    JavaScriptSerializer js = new JavaScriptSerializer();
                    js.MaxJsonLength = Int32.MaxValue;

                    string str = js.Serialize("Error occurred while geting country list from web service.");
                    context.Response.Write(str);
                }
            }
            else if (action == "GetMerchantDEC")
            {
                try
                {
                    ApplicationDbContext Consumercontext = new ApplicationDbContext("DefaultConsumerConnection");
                    string mobileno = Convert.ToString(context.Request["mobileno"]);
                    string businessmobileno = Convert.ToString(context.Request["businessmobileno"]);

                    var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(Consumercontext));
                    var UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(Consumercontext));


                    DateTime visitdate = DateTime.Now;
                    UserManager.UserValidator = new UserValidator<ApplicationUser>(UserManager)
                    {
                        AllowOnlyAlphanumericUserNames = false,
                        RequireUniqueEmail = true
                    };

                    if (mobileno != null && businessmobileno != null)
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

                        if (businessmobileno.Contains('@') == false)
                        {
                            string[] MerchantPhonewithCode = businessmobileno.Split(' ');
                            if (MerchantPhonewithCode.Length > 0)
                            {
                                if (MerchantPhonewithCode.Length == 3)
                                {
                                    businessmobileno = "+" + MerchantPhonewithCode[1] + " " + MerchantPhonewithCode[2].ToString();
                                }
                            }
                        }

                        consumerEntities dataContext = new consumerEntities();

                        merchantuser isMerchant = dataContext.merchantusers.Where(x => x.UserName == businessmobileno).FirstOrDefault();
                        if (isMerchant == null)
                        {
                            EventLog.LogErrorData("Error occurred ConsumerWebService/GetMerchantDEC. Merchant with phone number " + businessmobileno + "does not exist", true);
                            JavaScriptSerializer js = new JavaScriptSerializer();
                            js.MaxJsonLength = Int32.MaxValue;

                            string str = js.Serialize("Error occurred while geting merchant dec from web service. Merchant with phone number " + businessmobileno + "does not exist");
                            context.Response.Write(str);
                        }

                        string result = function.AddNewDECConsumer(mobileno, isMerchant.Id, "", "");

                        JavaScriptSerializer js1 = new JavaScriptSerializer();
                        js1.MaxJsonLength = Int32.MaxValue;

                        string str1 = js1.Serialize(Global.Consumer.ShareDECMessage);
                        context.Response.Write(str1);
                    }
                    else
                    {
                        EventLog.LogErrorData("Error occurred ConsumerWebService/GetMerchantDEC. Phone number or business phone number not provided", true);
                        JavaScriptSerializer js = new JavaScriptSerializer();
                        js.MaxJsonLength = Int32.MaxValue;
                        string str = js.Serialize("Error occurred while geting merchant dec from web service. Phone number or business phone number not provided");
                        context.Response.Write(str);
                    }
                }

                catch (Exception ex)
                {
                    //call error view here
                    //log the error
                    EventLog.LogErrorData("Error occurred GetMerchantDEC/GetMerchantDEC." + ex.Message, true);
                    JavaScriptSerializer js = new JavaScriptSerializer();
                    js.MaxJsonLength = Int32.MaxValue;
                    string str = js.Serialize("Error occurred while geting merchant dec from web service.");
                    context.Response.Write(str);
                }
            }
            else if (action == "GetConsumerLogo")
            {
                string userid = Convert.ToString(context.Request["userid"]);
                try
                {
                    using (instadelight_consumerEntities dataContext = new instadelight_consumerEntities())
                    {
                        consumermaster master = dataContext.consumermasters.Where(x => x.UserId == userid).FirstOrDefault();
                        if (master != null)
                        {
                            if (master.consumerlogo != null)
                            {
                                context.Response.Clear();
                                context.Response.ClearContent();
                                context.Response.ClearHeaders();
                                context.Response.BufferOutput = false;
                                context.Response.ContentType = "application/octet-stream";
                                context.Response.BinaryWrite(master.consumerlogo);
                                context.Response.Flush();
                            }
                            else
                            {
                                JavaScriptSerializer js = new JavaScriptSerializer();
                                js.MaxJsonLength = Int32.MaxValue;

                                string str = js.Serialize("No image.");
                                context.Response.Write(str);
                            }
                        }
                        else
                        {
                            JavaScriptSerializer js = new JavaScriptSerializer();
                            js.MaxJsonLength = Int32.MaxValue;

                            string str = js.Serialize("Consumer does not exist.");
                            context.Response.Write(str);
                        }
                    }
                }
                catch (Exception ex)
                {

                    EventLog.LogErrorData("Error occurred ConsumerWebService/GetConsumerLogo." + ex.Message, true);
                    JavaScriptSerializer js = new JavaScriptSerializer();
                    js.MaxJsonLength = Int32.MaxValue;

                    string str = js.Serialize("Error occurred while getting consumer logo from web service.");
                    context.Response.Write(str);
                }
            }
            else if (action == "GetMerchantLogo")
            {
                string merchantid = Convert.ToString(context.Request["merchantid"]);
                int no = Convert.ToInt32(merchantid);
                try
                {
                    using (consumerEntities dataContext = new consumerEntities())
                    {
                        merchant_master master = dataContext.merchant_master.Where(x => x.merchantid == no).FirstOrDefault();
                        if (master != null)
                        {
                            if (master.MerchantLogo != null)
                            {
                                context.Response.Clear();
                                context.Response.ClearContent();
                                context.Response.ClearHeaders();
                                context.Response.BufferOutput = false;
                                context.Response.ContentType = "application/octet-stream";
                                context.Response.BinaryWrite(master.MerchantLogo);
                                context.Response.Flush();
                            }
                            else
                            {
                                JavaScriptSerializer js = new JavaScriptSerializer();
                                js.MaxJsonLength = Int32.MaxValue;

                                string str = js.Serialize("No image.");
                                context.Response.Write(str);
                            }
                        }
                        else
                        {
                            JavaScriptSerializer js = new JavaScriptSerializer();
                            js.MaxJsonLength = Int32.MaxValue;

                            string str = js.Serialize("Merchant does not exist.");
                            context.Response.Write(str);
                        }
                    }
                }
                catch (Exception ex)
                {

                    EventLog.LogErrorData("Error occurred ConsumerWebService/GetMerchantLogo." + ex.Message, true);
                    JavaScriptSerializer js = new JavaScriptSerializer();
                    js.MaxJsonLength = Int32.MaxValue;

                    string str = js.Serialize("Error occurred while getting merchant logo from web service.");
                    context.Response.Write(str);
                }
            }
            else if (action == "GetMerchantDEC")
            {
                string merchantid = Convert.ToString(context.Request["merchantid"]);
                int no = Convert.ToInt32(merchantid);
                try
                {
                    using (consumerEntities dataContext = new consumerEntities())
                    {
                        merchant_master master = dataContext.merchant_master.Where(x => x.merchantid == no).FirstOrDefault();
                        if (master != null)
                        {
                            if (master.MerchantDEC != null)
                            {
                                context.Response.Clear();
                                context.Response.ClearContent();
                                context.Response.ClearHeaders();
                                context.Response.BufferOutput = false;
                                context.Response.ContentType = "application/octet-stream";
                                context.Response.BinaryWrite(master.MerchantDEC);
                                context.Response.Flush();
                            }
                            else if (master.merchantDecFromLibrary != null)
                            {
                                JavaScriptSerializer js = new JavaScriptSerializer();
                                js.MaxJsonLength = Int32.MaxValue;
                                var request = HttpContext.Current.Request;
                                var appUrl = HttpRuntime.AppDomainAppVirtualPath;
                                if (appUrl != "/")
                                    appUrl = "/" + appUrl;

                                var baseUrl = string.Format("{0}://{1}{2}", request.Url.Scheme, request.Url.Authority, appUrl);


                                string str = js.Serialize(baseUrl + master.merchantDecFromLibrary);
                                context.Response.Write(str);
                            }
                            else
                            {
                                JavaScriptSerializer js = new JavaScriptSerializer();
                                js.MaxJsonLength = Int32.MaxValue;

                                string str = js.Serialize("No image.");
                                context.Response.Write(str);
                            }
                        }
                        else
                        {
                            JavaScriptSerializer js = new JavaScriptSerializer();
                            js.MaxJsonLength = Int32.MaxValue;

                            string str = js.Serialize("Merchant does not exist.");
                            context.Response.Write(str);
                        }
                    }
                }
                catch (Exception ex)
                {

                    EventLog.LogErrorData("Error occurred ConsumerWebService/GetMerchantDEC." + ex.Message, true);
                    JavaScriptSerializer js = new JavaScriptSerializer();
                    js.MaxJsonLength = Int32.MaxValue;

                    string str = js.Serialize("Error occurred while getting merchant DEC from web service.");
                    context.Response.Write(str);
                }
            }
            else if (action == "GetCouponDEC")
            {
                string couponid = Convert.ToString(context.Request["couponid"]);
                int no = Convert.ToInt32(couponid);
                try
                {
                    using (consumerEntities dataContext = new consumerEntities())
                    {
                        coupons_master master = dataContext.coupons_master.Where(x => x.couponid == no).FirstOrDefault();
                        if (master != null)
                        {
                            if (master.DEC != null)
                            {
                                context.Response.Clear();
                                context.Response.ClearContent();
                                context.Response.ClearHeaders();
                                context.Response.BufferOutput = false;
                                context.Response.ContentType = "application/octet-stream";
                                context.Response.BinaryWrite(master.DEC);
                                context.Response.Flush();
                            }
                            else
                            {
                                JavaScriptSerializer js = new JavaScriptSerializer();
                                js.MaxJsonLength = Int32.MaxValue;

                                string str = js.Serialize("No image.");
                                context.Response.Write(str);
                            }
                        }
                        else
                        {
                            JavaScriptSerializer js = new JavaScriptSerializer();
                            js.MaxJsonLength = Int32.MaxValue;

                            string str = js.Serialize("Coupon does not exist.");
                            context.Response.Write(str);
                        }
                    }
                }
                catch (Exception ex)
                {

                    EventLog.LogErrorData("Error occurred ConsumerWebService/GetCouponDEC." + ex.Message, true);
                    JavaScriptSerializer js = new JavaScriptSerializer();
                    js.MaxJsonLength = Int32.MaxValue;

                    string str = js.Serialize("Error occurred while getting coupon DEC from web service.");
                    context.Response.Write(str);
                }
            }
            else if (action == "GetAllReviews")
            {
                try
                {
                    string userid = Convert.ToString(context.Request["userid"]);
                    using (consumerEntities dataContext = new consumerEntities())
                    {
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


                        JavaScriptSerializer js = new JavaScriptSerializer();
                        js.MaxJsonLength = Int32.MaxValue;

                        string str = js.Serialize(merchantList);
                        context.Response.Write(str);
                    }
                }
                catch (Exception ex)
                {
                    EventLog.LogErrorData("Error occurred ConsumerWebService/GetAllReviews." + ex.Message, true);
                    JavaScriptSerializer js = new JavaScriptSerializer();
                    js.MaxJsonLength = Int32.MaxValue;

                    string str = js.Serialize("Error occurred while geting merchant list for pending review from web service.");
                    context.Response.Write(str);
                }
            }
            else if (action == "GetPendingReviewForMerchant")
            {
                try
                {
                    string userid = Convert.ToString(context.Request["userid"]);
                    string merchantid = Convert.ToString(context.Request["merchantid"]);
                    int mid = Convert.ToInt32(merchantid);

                    using (consumerEntities dataContext = new consumerEntities())
                    {
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

                        JavaScriptSerializer js = new JavaScriptSerializer();
                        js.MaxJsonLength = Int32.MaxValue;

                        string str = js.Serialize(review);
                        context.Response.Write(str);
                    }
                }
                catch (Exception ex)
                {
                    EventLog.LogErrorData("Error occurred ConsumerWebService/GetPendingReviewForMerchant." + ex.Message, true);
                    JavaScriptSerializer js = new JavaScriptSerializer();
                    js.MaxJsonLength = Int32.MaxValue;

                    string str = js.Serialize("Error occurred while geting pending review for merchant from web service.");
                    context.Response.Write(str);
                }
            }
            else if (action == "GetConsumerById")
            {
                try
                {
                    string consumerid = Convert.ToString(context.Request["consumerid"]);
                    using (instadelight_consumerEntities dataContext = new instadelight_consumerEntities())
                    {
                        // int no = Convert.ToInt32(consumerid);
                        var consumer = dataContext.consumermasters.Where(x => x.UserId == consumerid).FirstOrDefault();

                        JavaScriptSerializer js = new JavaScriptSerializer();
                        js.MaxJsonLength = Int32.MaxValue;

                        string str = js.Serialize(consumer);
                        context.Response.Write(str);
                    }
                }
                catch (Exception ex)
                {
                    EventLog.LogErrorData("Error occurred ConsumerWebService/GetConsumerById." + ex.Message, true);
                    JavaScriptSerializer js = new JavaScriptSerializer();
                    js.MaxJsonLength = Int32.MaxValue;

                    string str = js.Serialize("Error occurred while geting consumerdetails from web service.");
                    context.Response.Write(str);
                }
            }
            else if (action == "GetCountries")
            {
                try
                {
                    using (consumerEntities dataContext = new consumerEntities())
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
                    EventLog.LogErrorData("Error occurred ConsumerWebService/getCountry." + ex.Message, true);
                    JavaScriptSerializer js = new JavaScriptSerializer();
                    js.MaxJsonLength = Int32.MaxValue;

                    string str = js.Serialize("Error occurred while geting country list from web service.");
                    context.Response.Write(str);
                }
            }
            else if (action == "GetLanguages")
            {
                try
                {
                    using (consumerEntities dataContext = new consumerEntities())
                    {
                        var LanguageList = dataContext.language_master.OrderBy(x => x.LanguageiId).ToList();

                        JavaScriptSerializer js = new JavaScriptSerializer();
                        js.MaxJsonLength = Int32.MaxValue;

                        string str = js.Serialize(LanguageList);
                        context.Response.Write(str);
                    }
                }
                catch (Exception ex)
                {
                    EventLog.LogErrorData("Error occurred ConsumerWebService/GetLanguages." + ex.Message, true);
                    JavaScriptSerializer js = new JavaScriptSerializer();
                    js.MaxJsonLength = Int32.MaxValue;

                    string str = js.Serialize("Error occurred while geting language list from web service.");
                    context.Response.Write(str);
                }
            }
            else if (action == "getLanguage")
            {
                string countryid = Convert.ToString(context.Request["countryid"]);
                try
                {
                    using (consumerEntities dataContext = new consumerEntities())
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
                    EventLog.LogErrorData("Error occurred ConsumerWebService/getLanguage." + ex.Message, true);
                    JavaScriptSerializer js = new JavaScriptSerializer();
                    js.MaxJsonLength = Int32.MaxValue;

                    string str = js.Serialize("Error occurred while geting language from web service.");
                    context.Response.Write(str);
                }
            }
            else if (action == "SaveChangedCountry")
            {
                try
                {
                    using (instadelight_consumerEntities dataContext = new instadelight_consumerEntities())
                    {
                        string userid = Convert.ToString(context.Request["userid"]);
                        string countryid = Convert.ToString(context.Request["countryid"]);
                        string langid = Convert.ToString(context.Request["langid"]);

                        var consumer = dataContext.consumermasters.Where(x => x.UserId == userid).FirstOrDefault();
                        consumer.Country = Convert.ToInt32(countryid);
                        consumer.LanguageId = Convert.ToInt32(langid);
                        dataContext.SaveChanges();
                        JavaScriptSerializer js = new JavaScriptSerializer();
                        js.MaxJsonLength = Int32.MaxValue;

                        string str = js.Serialize("Country Updated");
                        context.Response.Write(str);
                    }
                }
                catch (Exception ex)
                {
                    EventLog.LogErrorData("Error occurred ConsumerWebService/SaveChangedCountry. " + ex.Message, true);
                    JavaScriptSerializer js = new JavaScriptSerializer();
                    js.MaxJsonLength = Int32.MaxValue;

                    string str = js.Serialize("Error occurred while saving consumer's country and language details from web service.");
                    context.Response.Write(str);
                }
            }
            else if (action == "SaveReviewComment")
            {
                try
                {
                    string ReviewId = Convert.ToString(context.Request["ReviewId"]);
                    string MerchantId = Convert.ToString(context.Request["MerchantId"]);
                    string Comment = Convert.ToString(context.Request["Comment"]);

                    string userid = Convert.ToString(context.Request["userid"]);

                    int merchantno = Convert.ToInt32(MerchantId);
                    int reviewId = Convert.ToInt32(ReviewId);
                    using (consumerEntities dataContext = new consumerEntities())
                    {
                        merchantreviewcomment mr = new merchantreviewcomment();
                        mr.MerchantId = MerchantId;
                        mr.ReviewId = reviewId;
                        mr.ConsumerUserId = userid;
                        mr.AddedDatetime = DateTime.Now;
                        mr.Comment = Comment;
                        dataContext.merchantreviewcomments.Add(mr);
                        dataContext.SaveChanges();

                        JavaScriptSerializer js = new JavaScriptSerializer();
                        js.MaxJsonLength = Int32.MaxValue;

                        string str = js.Serialize("Comment Saved.");
                        context.Response.Write(str);
                    }
                }

                catch (Exception ex)
                {
                    EventLog.LogErrorData("Error occurred ConsumerWebService/SaveReviewComment. " + ex.Message, true);
                    JavaScriptSerializer js = new JavaScriptSerializer();
                    js.MaxJsonLength = Int32.MaxValue;

                    string str = js.Serialize("Error occurred while saving consumer's comments from web service.");
                    context.Response.Write(str);
                }
            }
            else if (action == "GetAllBanks")
            {
                try
                {
                    string userid = Convert.ToString(context.Request["userid"]);
                    using (consumerEntities dataContext = new consumerEntities())
                    {
                        JavaScriptSerializer js = new JavaScriptSerializer();
                        js.MaxJsonLength = Int32.MaxValue;


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

                            string str = js.Serialize(bankList);
                            context.Response.Write(str);
                        }
                        else
                        {
                            string str = js.Serialize("No banks associated with consumer");
                            context.Response.Write(str);
                        }
                    }
                }
                catch (Exception ex)
                {
                    EventLog.LogErrorData("Error occurred ConsumerWebService/GetAllBanks." + ex.Message, true);
                    JavaScriptSerializer js = new JavaScriptSerializer();
                    js.MaxJsonLength = Int32.MaxValue;

                    string str = js.Serialize("Error occurred while geting bank list for consumers from web service.");
                    context.Response.Write(str);
                }
            }
            else if (action == "GetAllMerchants")
            {
                try
                {
                    string userid = Convert.ToString(context.Request["userid"]);
                    using (consumerEntities dataContext = new consumerEntities())
                    {
                        JavaScriptSerializer js = new JavaScriptSerializer();
                        js.MaxJsonLength = Int32.MaxValue;


                        var s = string.Join(",", dataContext.merchantconsumerdetails.Where(p => p.ConsumerId == userid)
                                                .Select(p => p.MerchantId.ToString()));
                        if (s != "")
                        {
                            var idlist = s.Split(',').Select(n => n).ToArray();

                            //var bankList = from x in dataContext.bank_master.Select(x => x).ToList()
                            //               where idlist.Contains(Convert.ToInt32(x.bankid))
                            //               select x;

                            var merchantList = (from x in dataContext.merchant_master
                                                where idlist.Contains(x.UserId) && x.activation != "Deactive"
                                                select new { merchantid = x.merchantid, MerchantName = x.MerchantName, DECName = x.DECName, MerchantLogo = x.MerchantLogo, UserId = x.UserId, button1_text = x.button1_text, button2_text = x.button2_text, button2_url = x.button2_url, button3_text = x.button3_text, button3_url = x.button3_url, button4_text = x.button4_text }).ToList();


                            string str = js.Serialize(merchantList);
                            context.Response.Write(str);
                        }
                        else
                        {
                            string str = js.Serialize("No merchants associated with consumer");
                            context.Response.Write(str);
                        }
                    }
                }
                catch (Exception ex)
                {
                    EventLog.LogErrorData("Error occurred ConsumerWebService/GetAllMerchants." + ex.Message, true);
                    JavaScriptSerializer js = new JavaScriptSerializer();
                    js.MaxJsonLength = Int32.MaxValue;

                    string str = js.Serialize("Error occurred while geting merchant list for consumers from web service.");
                    context.Response.Write(str);
                }
            }
            else if (action == "GetCountrycode")
            {
                try
                {
                    string userid = Convert.ToString(context.Request["userid"]);
                    using (instadelight_consumerEntities dataContext = new instadelight_consumerEntities())
                    {
                        var UsercountryId = dataContext.consumermasters.Where(x => x.UserId == userid).Select(x => x.Country).FirstOrDefault();

                        if (UsercountryId != 0)
                        {
                            var country = dataContext.country_master.Where(c => c.countryid == UsercountryId).FirstOrDefault();
                            JavaScriptSerializer js = new JavaScriptSerializer();
                            js.MaxJsonLength = Int32.MaxValue;

                            string str = js.Serialize(country);
                            context.Response.Write(str);
                        }
                        else
                        {
                            JavaScriptSerializer js = new JavaScriptSerializer();
                            js.MaxJsonLength = Int32.MaxValue;

                            string str = js.Serialize("Country not defined for consumer");
                            context.Response.Write(str);
                        }
                    }
                }
                catch (Exception ex)
                {
                    EventLog.LogErrorData("Error occurred ConsumerWebService/GetCountrycode." + ex.Message, true);
                    JavaScriptSerializer js = new JavaScriptSerializer();
                    js.MaxJsonLength = Int32.MaxValue;

                    string str = js.Serialize("Error occurred while geting consumer's country code from web service.");
                    context.Response.Write(str);
                }
            }
            else if (action == "GetDECDetails")
            {
                try
                {
                    string bankid = Convert.ToString(context.Request["bankid"]);
                    using (consumerEntities dataContext = new consumerEntities())
                    {
                        int no = Convert.ToInt32(bankid);
                        var bankDECList = dataContext.bank_dec_details.Where(x => x.bankid == no).FirstOrDefault();
                        JavaScriptSerializer js = new JavaScriptSerializer();
                        js.MaxJsonLength = Int32.MaxValue;

                        string str = js.Serialize(bankDECList);
                        context.Response.Write(str);
                    }
                }
                catch (Exception ex)
                {
                    EventLog.LogErrorData("Error occurred ConsumerWebService/GetDECDetails." + ex.Message, true);
                    JavaScriptSerializer js = new JavaScriptSerializer();
                    js.MaxJsonLength = Int32.MaxValue;

                    string str = js.Serialize("Error occurred while geting bank DEC details from web service.");
                    context.Response.Write(str);
                }
            }
            else if (action == "GetBenefits")
            {
                try
                {
                    string merchantid = Convert.ToString(context.Request["merchantid"]);
                    using (consumerEntities dataContext = new consumerEntities())
                    {
                        int no = Convert.ToInt32(merchantid);
                        consumerEntities datacontext = new consumerEntities();
                        merchant_master master = datacontext.merchant_master.Where(x => x.merchantid == no).FirstOrDefault();

                        redeemoption option = datacontext.redeemoptions.Where(x => x.MerchantId == master.UserId).OrderByDescending(x => x.CreationDate).FirstOrDefault();

                        JavaScriptSerializer js = new JavaScriptSerializer();
                        js.MaxJsonLength = Int32.MaxValue;

                        string str = js.Serialize(option);
                        context.Response.Write(str);
                    }
                }
                catch (Exception ex)
                {
                    EventLog.LogErrorData("Error occurred ConsumerWebService/GetBenefits." + ex.Message, true);
                    JavaScriptSerializer js = new JavaScriptSerializer();
                    js.MaxJsonLength = Int32.MaxValue;

                    string str = js.Serialize("Error occurred while geting merchant benefits details from web service.");
                    context.Response.Write(str);
                }
            }
            else if (action == "GetMerchantDECDetails")
            {
                try
                {
                    string merchantid = Convert.ToString(context.Request["merchantid"]);
                    using (consumerEntities dataContext = new consumerEntities())
                    {
                        int no = Convert.ToInt32(merchantid);
                        merchant_master merchantdetails = dataContext.merchant_master.Where(x => x.merchantid == no).FirstOrDefault();//StaticCache.GetMerchants().Where(x => x.merchantid == no).FirstOrDefault();
                        var merchantRewards = dataContext.rewardmasters.Where(x => x.MerchantId == merchantdetails.UserId).OrderByDescending(x => x.CreationDate).FirstOrDefault();
                        if (merchantRewards != null)
                        {
                            merchantdetails.RewardName = merchantRewards.RewardName;
                        }

                        JavaScriptSerializer js = new JavaScriptSerializer();
                        js.MaxJsonLength = Int32.MaxValue;

                        string str = js.Serialize(merchantdetails);
                        context.Response.Write(str);
                    }
                }
                catch (Exception ex)
                {
                    EventLog.LogErrorData("Error occurred ConsumerWebService/GetMerchantDECDetails." + ex.Message, true);
                    JavaScriptSerializer js = new JavaScriptSerializer();
                    js.MaxJsonLength = Int32.MaxValue;

                    string str = js.Serialize("Error occurred while geting merchant DEC details from web service.");
                    context.Response.Write(str);
                }
            }
            else if (action == "GetCities")
            {
                try
                {
                    using (consumerEntities dataContext = new consumerEntities())
                    {
                        var cityList = (from x in dataContext.merchant_master
                                        select new { City = x.City }).Distinct().ToList();

                        JavaScriptSerializer js = new JavaScriptSerializer();
                        js.MaxJsonLength = Int32.MaxValue;

                        string str = js.Serialize(cityList);
                        context.Response.Write(str);
                    }
                }
                catch (Exception ex)
                {
                    EventLog.LogErrorData("Error occurred ConsumerWebService/GetCities." + ex.Message, true);
                    JavaScriptSerializer js = new JavaScriptSerializer();
                    js.MaxJsonLength = Int32.MaxValue;

                    string str = js.Serialize("Error occurred while geting city list from web service.");
                    context.Response.Write(str);
                }
            }
            else if (action == "GetLocations")
            {
                string cityid = Convert.ToString(context.Request["cityid"]);
                try
                {
                    using (consumerEntities dataContext = new consumerEntities())
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
                    EventLog.LogErrorData("Error occurred ConsumerWebService/GetLocations." + ex.Message, true);
                    JavaScriptSerializer js = new JavaScriptSerializer();
                    js.MaxJsonLength = Int32.MaxValue;

                    string str = js.Serialize("Error occurred while geting location list from web service.");
                    context.Response.Write(str);
                }

            }
            else if (action == "GetStates")
            {
                string countryid = Convert.ToString(context.Request["countryid"]);

                try
                {
                    using (instadelight_consumerEntities dataContext = new instadelight_consumerEntities())
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
                    EventLog.LogErrorData("Error occurred ConsumerWebService/GetStates." + ex.Message, true);
                    JavaScriptSerializer js = new JavaScriptSerializer();
                    js.MaxJsonLength = Int32.MaxValue;

                    string str = js.Serialize("Error occurred while geting state list from web service.");
                    context.Response.Write(str);
                }
            }
            else if (action == "GetCategories")
            {
                try
                {
                    using (consumerEntities dataContext = new consumerEntities())
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
                    EventLog.LogErrorData("Error occurred ConsumerWebService/GetCategories." + ex.Message, true);
                    JavaScriptSerializer js = new JavaScriptSerializer();
                    js.MaxJsonLength = Int32.MaxValue;

                    string str = js.Serialize("Error occurred while geting category list from web service.");
                    context.Response.Write(str);
                }
            }
            else if (action == "GetCoupons")
            {
                try
                {
                    string city = Convert.ToString(context.Request["city"]);

                    string userid = Convert.ToString(context.Request["userid"]);

                    string categoryid = Convert.ToString(context.Request["categoryid"]);
                    int category = Convert.ToInt32(categoryid);

                    using (consumerEntities dataContext = new consumerEntities())
                    {
                        MySqlParameter param1 = new MySqlParameter();
                        param1.Value = city;
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
                                                                      QRCode = x.QRCode,
                                                                      MaxDiscount = x.MaxDiscount,
                                                                      MaxCoupons = x.MaxCoupons,
                                                                      ShareWithAll = x.ShareWithAll,
                                                                      DateCreated = x.DateCreated
                                                                  }).ToList();

                        JavaScriptSerializer js = new JavaScriptSerializer();
                        js.MaxJsonLength = Int32.MaxValue;

                        string str = js.Serialize(CouponList);
                        context.Response.Write(str);
                    }
                }
                catch (Exception ex)
                {
                    EventLog.LogErrorData("Error occurred ConsumerWebService/GetCoupons." + ex.Message, true);
                    JavaScriptSerializer js = new JavaScriptSerializer();
                    js.MaxJsonLength = Int32.MaxValue;

                    string str = js.Serialize("Error occurred while geting coupon list from web service.");
                    context.Response.Write(str);
                }
            }
            else if (action == "GetMerchantCoupons")
            {
                try
                {
                    string merchantid = Convert.ToString(context.Request["merchantid"]);
                    int merchantno = Convert.ToInt32(merchantid);


                    string userid = Convert.ToString(context.Request["userid"]);


                    using (consumerEntities dataContext = new consumerEntities())
                    {
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
                                            QRCode = x.QRCode,
                                            MaxDiscount = x.MaxDiscount,
                                            MaxCoupons = x.MaxCoupons,
                                            ShareWithAll = x.ShareWithAll,
                                            DateCreated = x.DateCreated
                                        }).ToList();

                        JavaScriptSerializer js = new JavaScriptSerializer();
                        js.MaxJsonLength = Int32.MaxValue;

                        string str = js.Serialize(CouponList);
                        context.Response.Write(str);
                    }
                }
                catch (Exception ex)
                {
                    EventLog.LogErrorData("Error occurred ConsumerWebService/GetMerchantCoupons." + ex.Message, true);
                    JavaScriptSerializer js = new JavaScriptSerializer();
                    js.MaxJsonLength = Int32.MaxValue;

                    string str = js.Serialize("Error occurred while geting merchant coupon list from web service.");
                    context.Response.Write(str);
                }
            }
            else if (action == "GetEventConditions")
            {
                try
                {
                    string couponid = Convert.ToString(context.Request["couponid"]);
                    int no = Convert.ToInt32(couponid);

                    string userid = Convert.ToString(context.Request["userid"]);


                    using (consumerEntities dataContext = new consumerEntities())
                    {
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
                    EventLog.LogErrorData("Error occurred ConsumerWebService/GetEventConditions." + ex.Message, true);
                    JavaScriptSerializer js = new JavaScriptSerializer();
                    js.MaxJsonLength = Int32.MaxValue;

                    string str = js.Serialize("Error occurred while geting event conditions for coupon from web service.");
                    context.Response.Write(str);
                }
            }
            else if (action == "GetMerchantDetails")
            {
                try
                {
                    string merchantid = Convert.ToString(context.Request["merchantid"]);
                    int no = Convert.ToInt32(merchantid);


                    using (consumerEntities dataContext = new consumerEntities())
                    {
                        var merchantDetails = dataContext.merchant_master.Where(x => x.merchantid == no).FirstOrDefault();

                        JavaScriptSerializer js = new JavaScriptSerializer();
                        js.MaxJsonLength = Int32.MaxValue;

                        string str = js.Serialize(merchantDetails);
                        context.Response.Write(str);

                    }
                }
                catch (Exception ex)
                {
                    EventLog.LogErrorData("Error occurred ConsumerWebService/GetMerchantDetails." + ex.Message, true);
                    JavaScriptSerializer js = new JavaScriptSerializer();
                    js.MaxJsonLength = Int32.MaxValue;

                    string str = js.Serialize("Error occurred while geting merchant details from web service.");
                    context.Response.Write(str);
                }
            }
            else if (action == "GetCouponDetails")
            {
                string couponid = Convert.ToString(context.Request["couponid"]);
                string SharedCouponId = Convert.ToString(context.Request["SharedCouponId"]);
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
                        JavaScriptSerializer js = new JavaScriptSerializer();
                        js.MaxJsonLength = Int32.MaxValue;

                        string str = js.Serialize(couponDetails);
                        context.Response.Write(str);
                    }
                }
                catch (Exception ex)
                {
                    //call error view here
                    //log the error
                    EventLog.LogErrorData("Error occurred ConsumerWebService/GetCouponDetails." + ex.Message, true);
                    JavaScriptSerializer js = new JavaScriptSerializer();
                    js.MaxJsonLength = Int32.MaxValue;

                    string str = js.Serialize("Error occurred while geting coupon details from web service.");
                    context.Response.Write(str);
                }


            }
            else if (action == "getConsumerLogo")
            {
                try
                {
                    string UserId = Convert.ToString(context.Request["userid"]);

                    using (instadelight_consumerEntities dataContext = new instadelight_consumerEntities())
                    {
                        var consumer = dataContext.consumermasters.Where(x => x.UserId == UserId).FirstOrDefault();

                        JavaScriptSerializer js = new JavaScriptSerializer();
                        js.MaxJsonLength = Int32.MaxValue;

                        string str = js.Serialize(consumer);
                        context.Response.Write(str);

                    }
                }
                catch (Exception ex)
                {
                    EventLog.LogErrorData("Error occurred ConsumerWebService/getConsumerLogo." + ex.Message, true);
                    JavaScriptSerializer js = new JavaScriptSerializer();
                    js.MaxJsonLength = Int32.MaxValue;

                    string str = js.Serialize("Error occurred while geting consumer details from web service.");
                    context.Response.Write(str);
                }
            }
            else if (action == "getConsumerPoints")
            {
                try
                {
                    string merchantid = Convert.ToString(context.Request["merchantid"]);
                    int no = Convert.ToInt32(merchantid);
                    merchant_master merchant = new merchant_master();
                    consumerEntities dataContext = new consumerEntities();

                    merchant = dataContext.merchant_master.Where(u => u.merchantid == no).FirstOrDefault();


                    string UserId = Convert.ToString(context.Request["userid"]);

                    using (instadelight_consumerEntities consumerdataContext = new instadelight_consumerEntities())
                    {

                        if (merchant.RunRewardProgram == true)
                        {
                            if (merchant.RedeemProgram == "Cashback")
                            {
                                int availablepoints = Convert.ToInt32(consumerdataContext.consumercashbackdetails.Where(x => x.ConsumerId == UserId && x.MerchantId == merchant.UserId).Select(x => x.Cashback).Sum());
                                int redeemedpoints = Convert.ToInt32(consumerdataContext.consumerredeemdetails.Where(x => x.ConsumerId == UserId && x.MerchantId == merchant.UserId).Select(x => x.PointsRedeemed).Sum());

                                consumerrewardpoint points = new consumerrewardpoint();
                                points.ConsumerId = UserId;
                                points.MerchantId = merchant.UserId;
                                points.Points = availablepoints - redeemedpoints;
                                points.iscashback = true;
                                consumerdataContext.consumerrewardpoints.Add(points);
                                consumerdataContext.SaveChanges();
                                JavaScriptSerializer js1 = new JavaScriptSerializer();
                                js1.MaxJsonLength = Int32.MaxValue;

                                string str1 = js1.Serialize(points);
                                context.Response.Write(str1);
                            }
                            else
                            {
                                int availablepoints = Convert.ToInt32(consumerdataContext.consumerrewardpoints.Where(x => x.ConsumerId == UserId && x.MerchantId == merchant.UserId && x.ExpiryDate > DateTime.Now).Select(x => x.Points).Sum());
                                int redeemedpoints = Convert.ToInt32(consumerdataContext.consumerredeemdetails.Where(x => x.ConsumerId == UserId && x.MerchantId == merchant.UserId).Select(x => x.PointsRedeemed).Sum());


                                consumerrewardpoint points = new consumerrewardpoint();
                                points.ConsumerId = UserId;
                                points.MerchantId = merchant.UserId;
                                points.Points = availablepoints - redeemedpoints;
                                points.iscashback = false;
                                consumerdataContext.consumerrewardpoints.Add(points);
                                consumerdataContext.SaveChanges();


                                JavaScriptSerializer js = new JavaScriptSerializer();
                                js.MaxJsonLength = Int32.MaxValue;

                                string str = js.Serialize(points);
                                context.Response.Write(str);
                            }
                        }
                        else
                        {
                            JavaScriptSerializer js = new JavaScriptSerializer();
                            js.MaxJsonLength = Int32.MaxValue;

                            string str = js.Serialize("Reward program not defined by this merchant");
                            context.Response.Write(str);
                        }
                    }
                }
                catch (Exception ex)
                {
                    EventLog.LogErrorData("Error occurred ConsumerWebService/getConsumerPoints." + ex.Message, true);
                    JavaScriptSerializer js = new JavaScriptSerializer();
                    js.MaxJsonLength = Int32.MaxValue;

                    string str = js.Serialize("Error occurred while geting consumer points for selected merchant from web service.");
                    context.Response.Write(str);
                }
            }
            else if (action == "AddNewDECConsumer")
            {
                try
                {

                    ApplicationDbContext Consumercontext = new ApplicationDbContext("DefaultConsumerConnection");

                    string merchantid = Convert.ToString(context.Request["merchantid"]);
                    int no = Convert.ToInt32(merchantid);

                    merchant_master master = new merchant_master();
                    using (consumerEntities dataContext = new consumerEntities())
                    {
                        master = dataContext.merchant_master.Where(u => u.merchantid == no).FirstOrDefault();
                    }

                    string MerchantId = master.UserId;

                    string userid = Convert.ToString(context.Request["userid"]);

                    string mobileno = Convert.ToString(context.Request["mobileno"]);


                    instadelight_consumerEntities consumerDataContext = new instadelight_consumerEntities();
                    string username = "";

                    consumermaster cons = consumerDataContext.consumermasters.Where(x => x.UserId == userid).FirstOrDefault();
                    if (cons != null)
                    {
                        if (string.IsNullOrEmpty(cons.consumername) == false)
                        {
                            username = cons.consumername;
                        }
                        else
                        {
                            user conuser = consumerDataContext.users.Where(x => x.Id == userid).FirstOrDefault();
                            if (conuser != null)
                            {
                                username = conuser.UserName;
                            }
                        }
                    }

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

                        string result = function.AddNewDECConsumer(mobileno, MerchantId, userid, username);
                        JavaScriptSerializer js1 = new JavaScriptSerializer();
                        js1.MaxJsonLength = Int32.MaxValue;

                        string str1 = js1.Serialize(result);
                        context.Response.Write(str1);

                    }
                    else
                    {
                        JavaScriptSerializer js = new JavaScriptSerializer();
                        js.MaxJsonLength = Int32.MaxValue;

                        string str = js.Serialize("Not a valid phone number");
                        context.Response.Write(str);
                    }
                }
                catch (Exception ex)
                {
                    EventLog.LogErrorData("Error occurred ConsumerWebService/AddNewDECConsumer." + ex.Message, true);
                    JavaScriptSerializer js = new JavaScriptSerializer();
                    js.MaxJsonLength = Int32.MaxValue;

                    string str = js.Serialize("Error occurred while sharing DEC with friend from web service.");
                    context.Response.Write(str);
                }
            }
            else if (action == "SendDecToFriends")
            {
                string merchantid = Convert.ToString(context.Request["merchantid"]);
                int no = Convert.ToInt32(merchantid);
                ApplicationDbContext Consumercontext = new ApplicationDbContext("DefaultConsumerConnection");

                merchant_master master = new merchant_master();

                consumerEntities dataContext = new consumerEntities();
                master = dataContext.merchant_master.Where(u => u.merchantid == no).FirstOrDefault();
                DateTime visitdate = DateTime.Now;

                string userid = master.UserId;

                string consumerid = Convert.ToString(context.Request["userid"]);

                string Mobileno = Convert.ToString(context.Request["mobileno"]);

                string result = "";
                SMSUtility sms = new SMSUtility();
                instadelight_consumerEntities consumerDataContext = new instadelight_consumerEntities();
                string username = "";

                consumermaster cons = consumerDataContext.consumermasters.Where(x => x.UserId == consumerid).FirstOrDefault();
                if (cons != null)
                {
                    if (string.IsNullOrEmpty(cons.consumername) == false)
                    {
                        username = cons.consumername;
                    }
                    else
                    {
                        user conuser = consumerDataContext.users.Where(x => x.Id == consumerid).FirstOrDefault();
                        if (conuser != null)
                        {
                            username = conuser.UserName;
                        }
                    }
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

                            result = function.AddNewDECConsumer(cellno, master.UserId, consumerid, username);
                        }

                        result = "DEC Sent Successfully to selected contacts.";
                        JavaScriptSerializer js1 = new JavaScriptSerializer();
                        js1.MaxJsonLength = Int32.MaxValue;

                        string str1 = js1.Serialize(result);
                        context.Response.Write(str1);
                    }
                    else
                    {
                        result = "Enter Mobile Number";
                        JavaScriptSerializer js1 = new JavaScriptSerializer();
                        js1.MaxJsonLength = Int32.MaxValue;

                        string str1 = js1.Serialize(result);
                        context.Response.Write(str1);
                    }
                }
                else
                {
                    result = "Enter Mobile Number";
                    JavaScriptSerializer js1 = new JavaScriptSerializer();
                    js1.MaxJsonLength = Int32.MaxValue;

                    string str1 = js1.Serialize(result);
                    context.Response.Write(str1);
                }
            }
            else if (action == "AddCheckCouponConsumer")
            {
                try
                {
                    string userid = Convert.ToString(context.Request["userid"]);
                    string mobileno = Convert.ToString(context.Request["mobileno"]);
                    string MerchantId = Convert.ToString(context.Request["MerchantId"]);
                    string CouponId = Convert.ToString(context.Request["CouponId"]);
                    string SharedCouponId = Convert.ToString(context.Request["SharedCouponId"]);

                    ApplicationDbContext Consumercontext = new ApplicationDbContext("DefaultConsumerConnection");

                    int no = Convert.ToInt32(MerchantId);

                    DateTime visitdate = DateTime.Now;
                    merchant_master master = new merchant_master();


                    SMSUtility sms = new SMSUtility();
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

                        using (consumerEntities dataContext = new consumerEntities())
                        {
                            master = dataContext.merchant_master.Where(u => u.merchantid == no).FirstOrDefault();
                        }

                        if (master != null)
                        {
                            string username = "";
                            instadelight_consumerEntities consumerDataContext = new instadelight_consumerEntities();
                            consumermaster cons = consumerDataContext.consumermasters.Where(x => x.UserId == userid).FirstOrDefault();
                            if (cons != null)
                            {
                                if (string.IsNullOrEmpty(cons.consumername) == false)
                                {
                                    username = cons.consumername;
                                }
                                else
                                {
                                    user conuser = consumerDataContext.users.Where(x => x.Id == userid).FirstOrDefault();
                                    if (conuser != null)
                                    {
                                        username = conuser.UserName;
                                    }
                                }
                            }


                            string result = function.AddCheckCouponConsumer(mobileno, master.UserId, CouponId, SharedCouponId, username);
                            JavaScriptSerializer js = new JavaScriptSerializer();
                            js.MaxJsonLength = Int32.MaxValue;

                            string str = js.Serialize("Coupon shared with consumer Successfully.");
                            context.Response.Write(str);
                        }
                        else
                        {
                            JavaScriptSerializer js = new JavaScriptSerializer();
                            js.MaxJsonLength = Int32.MaxValue;

                            string str = js.Serialize("Invalid merchant details");
                            context.Response.Write(str);
                        }
                    }
                    else
                    {

                        JavaScriptSerializer js = new JavaScriptSerializer();
                        js.MaxJsonLength = Int32.MaxValue;

                        string str = js.Serialize("Enter Mobile Number");
                        context.Response.Write(str);
                    }
                }
                catch (Exception ex)
                {
                    EventLog.LogErrorData("Error occurred ConsumerWebService/AddCheckCouponConsumer." + ex.Message, true);
                    JavaScriptSerializer js = new JavaScriptSerializer();
                    js.MaxJsonLength = Int32.MaxValue;

                    string str = js.Serialize("Error occurred while sharing coupon with friend from web service.");
                    context.Response.Write(str);
                }
            }
            else if (action == "SaveConsumerReview")
            {
                try
                {
                    string jsonString = String.Empty;
                    JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
                    var consumerId = Convert.ToString(context.Request["consumerId"]);
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
                    clsReview_submitdetails ch = JsonConvert.DeserializeObject<clsReview_submitdetails>(jsonString, jsonSettings);

                    if (ch != null)
                    {
                        using (consumerEntities dataContext = new consumerEntities())
                        {
                            review_submit_details mappingcosumerreview = new review_submit_details();
                            mappingcosumerreview.reviewid = ch.reviewid;
                            mappingcosumerreview.consumerid = ch.consumerId;
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
                            var consumerphone = consumerContext.users.Where(c => c.Id == ch.consumerId).Select(m => m.UserName).FirstOrDefault();

                            List<merchantconsumerreviewdetail> revdet = dataContext.merchantconsumerreviewdetails.Where(x => x.ReviewId == ch.reviewid && x.ConsumerId == ch.consumerId && x.MerchantId == merchantUserId && x.Status == "Shared").ToList();

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
                                sendcoupon.ConsumerId = ch.consumerId;
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

                            JavaScriptSerializer js = new JavaScriptSerializer();
                            js.MaxJsonLength = Int32.MaxValue;

                            string str = js.Serialize(merchantUserId);
                            context.Response.Write(str);

                        }
                    }
                    else
                    {
                        JavaScriptSerializer js = new JavaScriptSerializer();
                        js.MaxJsonLength = Int32.MaxValue;

                        string str = js.Serialize("Invalid Consumer review");
                        context.Response.Write(str);
                    }
                }
                catch (Exception ex)
                {
                    EventLog.LogErrorData("Error occurred ConsumerWebService/SaveConsumerReview." + ex.Message, true);
                    JavaScriptSerializer js = new JavaScriptSerializer();
                    js.MaxJsonLength = Int32.MaxValue;

                    string str = js.Serialize("Error occurred while saving consumer review from web service.");
                    context.Response.Write(str);
                }
            }
            else if (action == "UpdateConsumer")
            {
                try
                {
                    string jsonString = string.Empty;
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
                    consumermaster ch = JsonConvert.DeserializeObject<consumermaster>(jsonString, jsonSettings);

                    if (ch != null)
                    {
                        using (instadelight_consumerEntities dataContext = new instadelight_consumerEntities())
                        {
                            EventLog.LogData("Updating consumer", true);
                            EventLog.LogData("Username" + ch.id, true);
                            EventLog.LogData("Date of birth" + ch.DOB, true);

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

                            JavaScriptSerializer js = new JavaScriptSerializer();
                            js.MaxJsonLength = Int32.MaxValue;

                            string str = js.Serialize("Consumer Updated");
                            context.Response.Write(str);
                        }
                    }
                    else
                    {
                        JavaScriptSerializer js = new JavaScriptSerializer();
                        js.MaxJsonLength = Int32.MaxValue;

                        string str = js.Serialize("Invalid Consumer");
                        context.Response.Write(str);
                    }
                }
                catch (Exception ex)
                {
                    //call error view here
                    //log the error
                    EventLog.LogErrorData("Error occurred ConsumerWebService/UpdateConsumer." + ex.Message, true);
                    JavaScriptSerializer js = new JavaScriptSerializer();
                    js.MaxJsonLength = Int32.MaxValue;

                    string str = js.Serialize("Error occurred while updating consumer");
                    context.Response.Write(str);

                }
            }
            else if (action == "Benefits")
            {
                try
                {
                    string merchantid = Convert.ToString(context.Request["merchantid"]);

                    int no = Convert.ToInt32(merchantid);
                    consumerEntities datacontext = new consumerEntities();
                    merchant_master master = datacontext.merchant_master.Where(x => x.merchantid == no).FirstOrDefault();

                    merchant_benefits benefits = datacontext.merchant_benefits.Where(x => x.MerchantId == master.UserId).OrderByDescending(x => x.CreationDate).FirstOrDefault();

                    JavaScriptSerializer js = new JavaScriptSerializer();
                    js.MaxJsonLength = Int32.MaxValue;

                    string str = js.Serialize(benefits);
                    context.Response.Write(str);
                }
                catch (Exception ex)
                {
                    //call error view here
                    //log the error
                    EventLog.LogErrorData("Error occurred ConsumerWebService/Benefits." + ex.Message, true);
                    JavaScriptSerializer js = new JavaScriptSerializer();
                    js.MaxJsonLength = Int32.MaxValue;

                    string str = js.Serialize("Error occurred while getting benefits of merchant");
                    context.Response.Write(str);
                }
            }
            else if (action == "BankBenefits")
            {
                try
                {
                    string merchantid = Convert.ToString(context.Request["merchantid"]);

                    int no = Convert.ToInt32(merchantid);
                    consumerEntities datacontext = new consumerEntities();
                    merchant_master master = datacontext.merchant_master.Where(x => x.merchantid == no).FirstOrDefault();

                    List<bank_benefits> bbenefits = datacontext.bank_benefits.Where(x => x.MerchantId == master.UserId).ToList();

                    JavaScriptSerializer js = new JavaScriptSerializer();
                    js.MaxJsonLength = Int32.MaxValue;

                    string str = js.Serialize(bbenefits);
                    context.Response.Write(str);
                }
                catch (Exception ex)
                {
                    //call error view here
                    //log the error
                    EventLog.LogErrorData("Error occurred ConsumerWebService/BankBenefits." + ex.Message, true);
                    JavaScriptSerializer js = new JavaScriptSerializer();
                    js.MaxJsonLength = Int32.MaxValue;

                    string str = js.Serialize("Error occurred while getting bank benefits for merchant");
                    context.Response.Write(str);
                }
            }
            else if (action == "GetGiftCardDenomination")
            {
                try
                {
                    string merchantid = Convert.ToString(context.Request["merchantid"]);
                    int no = Convert.ToInt32(merchantid);

                    using (consumerEntities dataContext = new consumerEntities())
                    {
                        var MerchantUserId = dataContext.merchant_master.Where(u => u.merchantid == no).Select(x => x.UserId).FirstOrDefault();
                        var giftDenominations = (from x in dataContext.giftcardmasters.Select(x => x).ToList()
                                                 where (x.MerchantUserId == MerchantUserId)
                                                 orderby x.SetDate descending
                                                 select x).Take(1);


                        JavaScriptSerializer js = new JavaScriptSerializer();
                        js.MaxJsonLength = Int32.MaxValue;

                        string str = js.Serialize(giftDenominations);
                        context.Response.Write(str);
                    }
                }
                catch (Exception ex)
                {
                    //call error view here
                    //log the error
                    EventLog.LogErrorData("Error occurred ConsumerWebService/GetGiftCardDenomination." + ex.Message, true);
                    JavaScriptSerializer js = new JavaScriptSerializer();
                    js.MaxJsonLength = Int32.MaxValue;

                    string str = js.Serialize("Error occurred while getting gift card denomination of merchant");
                    context.Response.Write(str);
                }
            }
            else if (action == "GetConsumerGiftCards")
            {
                try
                {
                    string consumerId = Convert.ToString(context.Request["consumerId"]);
                    string merchantid = Convert.ToString(context.Request["merchantid"]);
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
                    JavaScriptSerializer js = new JavaScriptSerializer();
                    js.MaxJsonLength = Int32.MaxValue;

                    string str = js.Serialize(consgfcrd);
                    context.Response.Write(str);
                }
                catch (Exception ex)
                {
                    //call error view here
                    //log the error
                    EventLog.LogErrorData("Error occurred ConsumerWebService/GetConsumerGiftCards." + ex.Message, true);
                    JavaScriptSerializer js = new JavaScriptSerializer();
                    js.MaxJsonLength = Int32.MaxValue;

                    string str = js.Serialize("Error occurred while getting consumer gift cards");
                    context.Response.Write(str);
                }
            }
            else if (action == "SendGCToConsumer")
            {
                try
                {
                    SMSUtility sms = new SMSUtility();
                    string mobileno = Convert.ToString(context.Request["mobileno"]);

                    string merchantid = Convert.ToString(context.Request["merchantid"]);

                    string userid = Convert.ToString(context.Request["userid"]);
                    int merchantno = Convert.ToInt32(merchantid);
                    string giftcardid = Convert.ToString(context.Request["giftcardid"]);

                    int cid = Convert.ToInt32(giftcardid);
                    ApplicationDbContext Consumercontext = new ApplicationDbContext("DefaultConsumerConnection");

                    if (mobileno != null)
                    {
                        instadelight_consumerEntities consumerDataContext = new instadelight_consumerEntities();
                        string username = "";

                        consumermaster cons = consumerDataContext.consumermasters.Where(x => x.UserId == userid).FirstOrDefault();
                        if (cons != null)
                        {
                            if (string.IsNullOrEmpty(cons.consumername) == false)
                            {
                                username = cons.consumername;
                            }
                            else
                            {
                                user conuser = consumerDataContext.users.Where(x => x.Id == userid).FirstOrDefault();
                                if (conuser != null)
                                {
                                    username = conuser.UserName;
                                }
                            }
                        }

                        string result = function.SendGCToConsumer(mobileno, giftcardid, merchantid, userid, username);
                        JavaScriptSerializer js = new JavaScriptSerializer();
                        js.MaxJsonLength = Int32.MaxValue;

                        string str = js.Serialize(result);
                        context.Response.Write(str);
                    }
                    else
                    {
                        JavaScriptSerializer js = new JavaScriptSerializer();
                        js.MaxJsonLength = Int32.MaxValue;

                        string str = js.Serialize("Invalid Consumer Details");
                        context.Response.Write(str);

                    }
                    JavaScriptSerializer js1 = new JavaScriptSerializer();
                    js1.MaxJsonLength = Int32.MaxValue;

                    string str1 = js1.Serialize("Gift card sent successfully.");
                    context.Response.Write(str1);

                }
                catch (Exception ex)
                {
                    //call error view here
                    //log the error
                    EventLog.LogErrorData("Error occurred ConsumerWebService/SendGCToConsumer." + ex.Message, true);
                    JavaScriptSerializer js = new JavaScriptSerializer();
                    js.MaxJsonLength = Int32.MaxValue;

                    string str = js.Serialize("Error occurred while sending gift card to consumer");
                    context.Response.Write(str);
                }
            }
            else if (action == "BuyGiftCard")
            {
                try
                {
                    string qty = Convert.ToString(context.Request["qty"]);
                    string denomination = Convert.ToString(context.Request["denomination"]);
                    string merchantid = Convert.ToString(context.Request["merchantid"]);

                    string userid = Convert.ToString(context.Request["userid"]);
                    int merchantno = Convert.ToInt32(merchantid);
                    string giftcardid = Convert.ToString(context.Request["giftcardid"]);

                    int cid = Convert.ToInt32(giftcardid);

                    int quantity = Convert.ToInt32(qty);

                    using (consumerEntities dataContext = new consumerEntities())
                    {
                        merchant_master master = dataContext.merchant_master.Where(x => x.merchantid == merchantno).FirstOrDefault();

                        using (instadelight_consumerEntities consumerDatacontext = new instadelight_consumerEntities())
                        {

                            for (int i = 1; i <= quantity; i++)
                            {
                                consumergiftcarddetail gifts = new consumergiftcarddetail();
                                gifts.ConsumerId = userid;
                                gifts.MerchantId = master.UserId;
                                gifts.GiftCardId = cid;
                                gifts.DenominationRs = Convert.ToInt32(denomination);
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

                        JavaScriptSerializer js1 = new JavaScriptSerializer();
                        js1.MaxJsonLength = Int32.MaxValue;

                        string str1 = js1.Serialize("Gift card bought successfully.");
                        context.Response.Write(str1);
                    }
                }
                catch (Exception ex)
                {
                    //call error view here
                    //log the error

                    EventLog.LogErrorData("Error occurred ConsumerWebService/BuyGiftCard." + ex.Message, true);
                    JavaScriptSerializer js = new JavaScriptSerializer();
                    js.MaxJsonLength = Int32.MaxValue;

                    string str = js.Serialize("Error occurred while buying gift card");
                    context.Response.Write(str);
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