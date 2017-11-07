using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Web.Mvc;
using ConsumerApp.Models;
using AspNet.Identity.MySQL;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.Google;
using Owin;
using System.IO;
using System.Drawing;
using System.Web.Script.Serialization;
using MySql.Data.MySqlClient;

namespace ConsumerApp
{
    public class ConsumerCommonFunctions
    {
        ApplicationDbContext Consumercontext = new ApplicationDbContext("DefaultConsumerConnection");
        public string AddNewDECConsumer(string mobileno, string MerchantId, string userid, string username)
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

                    DateTime visitdate = DateTime.Now;

                    consumerEntities dataContext = new consumerEntities();

                    merchant_master master = dataContext.merchant_master.Where(x => x.UserId == MerchantId).FirstOrDefault();

                    reviewmaster rvw = dataContext.reviewmasters.Where(x => x.MerchantId == master.merchantid).OrderByDescending(x => x.CreationDate).FirstOrDefault();

                    DateTime expirydate = expdate(master.UserId, visitdate);

                    bool isNewConsumer = false;
                    using (instadelight_consumerEntities ConsumerdataContext = new instadelight_consumerEntities())
                    {
                        using (consumerEntities merchantcontext = new consumerEntities())
                        {
                            var isUser = ConsumerdataContext.users.Where(u => u.UserName == mobileno).FirstOrDefault();
                            if (isUser == null)
                            {
                                //add new consumer
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
                                if (chkUser.Succeeded)
                                {
                                    var rolesForUser = UserManager.GetRoles(user.Id);
                                    if (!rolesForUser.Contains("Consumer"))
                                    {
                                        UserManager.AddToRole(user.Id, "Consumer");
                                    }

                                    //Added new code block for consumermaster

                                    InsertNewConsumer(user.Id, mobileno);
                                    isUser = ConsumerdataContext.users.Where(u => u.UserName == mobileno).FirstOrDefault();
                                    isNewConsumer = true;
                                }
                                else
                                {
                                    return Global.Consumer.RegisterConsumerError;
                                }
                            }

                            var isConsumerMapped = merchantcontext.merchantconsumerdetails.Where(u => u.ConsumerId == isUser.Id && u.MerchantId == master.UserId).FirstOrDefault();
                            if (isConsumerMapped == null)
                            {
                                MapMerchantToConsumer(master.UserId, isUser.Id, mobileno);
                                isNewConsumer = true;
                                consumervisitdetail visit = new consumervisitdetail();
                                visit.ConsumerId = isUser.Id;
                                visit.MerchantId = master.UserId;
                                visit.VisitDate = visitdate;
                                ConsumerdataContext.consumervisitdetails.Add(visit);
                                ConsumerdataContext.SaveChanges();

                                merchantjoiningbonu bonus = dataContext.merchantjoiningbonus.Where(x => x.MerchantId == master.UserId).OrderByDescending(x => x.DateCreated).FirstOrDefault();
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
                                                detail.MerchantId = master.UserId;
                                                detail.Cashback = bonus.JoiningBonus;
                                                detail.CashbackDate = visitdate;
                                                detail.ExpiryDate = expirydate;
                                                dataConsumerContext.consumercashbackdetails.Add(detail);
                                                dataConsumerContext.SaveChanges();
                                            }
                                            else
                                            {
                                                consumerrewardpoint pts = new consumerrewardpoint();
                                                pts.ConsumerId = isUser.Id;
                                                pts.MerchantId = master.UserId;
                                                pts.Points = bonus.JoiningBonus;
                                                pts.VisitDate = visitdate;
                                                pts.ExpiryDate = expirydate;
                                                dataConsumerContext.consumerrewardpoints.Add(pts);
                                                dataConsumerContext.SaveChanges();
                                            }
                                        }
                                        else
                                        {
                                            consumerrewardpoint pts = new consumerrewardpoint();
                                            pts.ConsumerId = isUser.Id;
                                            pts.MerchantId = master.UserId;
                                            pts.Points = bonus.JoiningBonus;
                                            pts.VisitDate = visitdate;
                                            pts.ExpiryDate = expirydate;
                                            dataConsumerContext.consumerrewardpoints.Add(pts);
                                            dataConsumerContext.SaveChanges();
                                        }
                                    }
                                }

                            }

                            //Send shared coupons.
                            SendSharedCoupons(master.merchantid, master.UserId, isUser.Id, mobileno);
                            //Add cashback if applicable
                            if (master.RunRewardProgram == true)
                            {
                                if (master.RedeemProgram == "Cashback")
                                {
                                    cashbackdetail cashback = dataContext.cashbackdetails.Where(x => x.MerchantId == master.UserId).OrderByDescending(x => x.CreationDate).FirstOrDefault();
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
                                                            rs.MerchantId = master.UserId;
                                                            rs.Cashback = Convert.ToInt32(cashback.FixedCashBack);
                                                            rs.CashbackDate = visitdate;
                                                            rs.ExpiryDate = expirydate;
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

                            //Send review question                                            
                            if (rvw != null)
                            {
                                //SendReviewsToDECs(rvw);
                                MapMerchantReviewToConsumer(rvw.MerchantUserId, isUser.Id, rvw.reviewid);
                            }

                            if (userid != "" && username != "")
                            {
                                //Send SMS to consumer for a new DEC. 'Navigation://OpenQRCodeScanner'
                                if (mobileno.Contains('@') == false)
                                {
                                    SMSUtility sms = new SMSUtility();
                                    string smsresult = sms.sendMessage(mobileno, username + " has sent a great offer from " + master.DECName + " to you. Download the app for Android at goo.gl/r5rxjj and for Apple at goo.gl/dAq4er. Your cell number is your login and your password is 123456 unless you have reset it.");
                                    consumersmsdetail smsdetails = new consumersmsdetail();
                                    smsdetails.ConsumerId = isUser.Id;
                                    smsdetails.MerchantId = master.UserId;
                                    smsdetails.SMSEmailStatus = smsresult;
                                    smsdetails.UserName = mobileno;
                                    smsdetails.SentDate = visitdate;
                                    ConsumerdataContext.consumersmsdetails.Add(smsdetails);
                                    ConsumerdataContext.SaveChanges();
                                }
                                else
                                {
                                    EmailModel model = new EmailModel();
                                    model.To = mobileno;
                                    model.Email = "You_Rock@offertraker.com";
                                    model.Subject = "Your friend has shared a Digital Card with you";
                                    model.Body = "Dear " + mobileno + ",<br /><br /> Your friend " + username + " has sent you the digital card of " + master.DECName + ". <br /> The digital card is a great way to keep a track of special offers for you from various merchants, without having to search for offers in emails, smss or online. <br />So, go ahead and download the offertraker app for Android here:goo.gl/r5rxjj and for Apple here: goo.gl/dAq4er. Your login is your email-id and your password is 123456.<br /><br /> Best wishes, <br /><br />Offertraker team.";
                                    SendEmail email = new SendEmail();
                                    string result = email.SendEmailToConsumer(model);
                                    consumersmsdetail smsdetails = new consumersmsdetail();
                                    smsdetails.ConsumerId = isUser.Id;
                                    smsdetails.MerchantId = master.UserId;
                                    smsdetails.SMSEmailStatus = result;
                                    smsdetails.UserName = mobileno;
                                    smsdetails.SentDate = visitdate;
                                    ConsumerdataContext.consumersmsdetails.Add(smsdetails);
                                    ConsumerdataContext.SaveChanges();

                                    //SendSMS(param.mobileno,Session["UserName"] + " has shared offers with you. Download the app for Android at goo.gl/r5rxjj, use your mobile no. as login and 123456 as password and enjoy the benefits!")
                                }
                            }
                            else
                            {
                                if (mobileno.Contains('@') == false)
                                {
                                    SMSUtility sms = new SMSUtility();
                                    string smsresult = "";
                                    if (isNewConsumer)
                                        smsresult = sms.sendMessage(mobileno, "Congrats!! Welcome to " + master.DECName + " and thanks for registering with us. Download the app for Android at goo.gl/r5rxjj and for Apple at goo.gl/dAq4er. Your cell number is your login and your password is 123456.");
                                    else
                                        smsresult = sms.sendMessage(mobileno, "Dear Customer, your " + master.DECName + " Digital Card is updated. Download the app for Android at goo.gl/r5rxjj and for Apple at goo.gl/dAq4er. Your cell number is your login and your password is 123456 unless you have reset it.");

                                    consumersmsdetail smsdetails = new consumersmsdetail();
                                    smsdetails.ConsumerId = isUser.Id;
                                    smsdetails.MerchantId = master.UserId;
                                    smsdetails.SMSEmailStatus = smsresult;
                                    smsdetails.UserName = mobileno;
                                    smsdetails.SentDate = visitdate;
                                    ConsumerdataContext.consumersmsdetails.Add(smsdetails);
                                    ConsumerdataContext.SaveChanges();

                                    master.NoOfSMS = master.NoOfSMS - 1;
                                    dataContext.SaveChanges();
                                    merchantsmsdetail smsemailcnt = dataContext.merchantsmsdetails.Where(x => x.MerchantId == master.UserId).FirstOrDefault();
                                    if (smsemailcnt != null)
                                    {
                                        smsemailcnt.SMSCount = Convert.ToInt32(smsemailcnt.SMSCount) + 1;
                                        dataContext.SaveChanges();
                                    }
                                    else
                                    {
                                        smsemailcnt = new merchantsmsdetail();
                                        smsemailcnt.MerchantId = master.UserId;
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

                                    model.Body = "Dear " + mobileno + ", <br /><br />" + master.DECName + " is happy to share its digital card with you. The digital card carries exclusive coupons and offers for you. You can also place orders through the digital card. You can share the digital card with your friends and help them enjoy exclusive offers from our business. <br />Don't forget to add your picture in your profile once you have downloaded the app! It shows up on the digital card, so that you may flaunt it!<br /> The Offertraker app is a great way to keep digital cards from various merchants in one place, so that all your offers ,coupons or points can be easily redeemed! Best of all, you can also place orders directly from the digital card.  <br />Please download the Offertraker app for Android here :goo.gl/r5rxjj and for Apple here :goo.gl/dAq4er.Your login is your email-id and your password is 123456. Enjoy the offers and stay connected! <br /><br />" + master.DECName + ".";

                                    SendEmail email = new SendEmail();
                                    string result = email.SendEmailToConsumer(model);
                                    consumersmsdetail smsdetails = new consumersmsdetail();
                                    smsdetails.ConsumerId = isUser.Id;
                                    smsdetails.MerchantId = master.UserId;
                                    smsdetails.SMSEmailStatus = result;
                                    smsdetails.UserName = mobileno;
                                    smsdetails.SentDate = visitdate;
                                    ConsumerdataContext.consumersmsdetails.Add(smsdetails);
                                    ConsumerdataContext.SaveChanges();

                                    merchantsmsdetail smsemailcnt = dataContext.merchantsmsdetails.Where(x => x.MerchantId == master.UserId).FirstOrDefault();
                                    if (smsemailcnt != null)
                                    {
                                        smsemailcnt.EmailCount = Convert.ToInt32(smsemailcnt.EmailCount) + 1;
                                        dataContext.SaveChanges();
                                    }
                                    else
                                    {
                                        smsemailcnt = new merchantsmsdetail();
                                        smsemailcnt.MerchantId = master.UserId;
                                        smsemailcnt.SMSCount = 0;
                                        smsemailcnt.EmailCount = 1;
                                        dataContext.merchantsmsdetails.Add(smsemailcnt);
                                        dataContext.SaveChanges();
                                    }
                                }
                            }

                            //check if user has selected submit review with share with friends
                            if (userid != "")
                            {
                                review_submit_details rev = dataContext.review_submit_details.Where(x => x.consumerid == userid && x.IsSharedDECWithFriends == 1).FirstOrDefault();
                                if (rev != null)
                                {
                                    if (Convert.ToDateTime(rev.Review_Submit_date).Date == DateTime.Now.Date)
                                    {
                                        //check if merchant has set coupon for sharing dec with friends
                                        eventcoupondetail eventcoupon = dataContext.eventcoupondetails.Where(x => x.EventId == 4 && x.MerchantId == master.merchantid).FirstOrDefault();
                                        if (eventcoupon != null)
                                        {
                                            //Assign this coupon to this consumer
                                            instadelight_consumerEntities consumercontext = new instadelight_consumerEntities();
                                            user consumeruser = consumercontext.users.Where(x => x.Id == userid).FirstOrDefault();
                                            var consumerphone = consumeruser.UserName;

                                            merchantconsumercoupondetail sendcoupon = new merchantconsumercoupondetail();
                                            sendcoupon.ConsumerId = userid;
                                            sendcoupon.ConsumerPhone = consumerphone;
                                            sendcoupon.MerchantId = master.UserId;
                                            sendcoupon.CouponId = eventcoupon.CouponId;
                                            sendcoupon.ValidFrom = DateTime.Now;
                                            sendcoupon.ValidTill = DateTime.Now.AddMonths(6);
                                            dataContext.merchantconsumercoupondetails.Add(sendcoupon);
                                            dataContext.SaveChanges();

                                            rev.IsSharedDECWithFriends = 0;
                                            dataContext.SaveChanges();

                                            if (consumerphone.Contains("@") == false)
                                            {
                                                SMSUtility sms = new SMSUtility();
                                                string smsresult = sms.sendMessage(consumerphone, "Dear Customer, Thanks for sharing our DEC. As a token of appreciation we have sent you a coupon! " + master.MerchantName);
                                                consumersmsdetail smsdetails = new consumersmsdetail();
                                                smsdetails.ConsumerId = isUser.Id;
                                                smsdetails.MerchantId = userid;
                                                smsdetails.SMSEmailStatus = smsresult;
                                                smsdetails.UserName = consumerphone;
                                                smsdetails.SentDate = visitdate;
                                                ConsumerdataContext.consumersmsdetails.Add(smsdetails);
                                                ConsumerdataContext.SaveChanges();

                                            }
                                            else
                                            {
                                                EmailModel model = new EmailModel();
                                                model.To = consumerphone;
                                                model.Email = "Thank_you@offertraker.com";
                                                model.Subject = "Thank you for submitting review";

                                                model.Body = "Dear Customer,<br /><br /> Thanks for sharing our DEC. As a token of appreciation we have sent you a coupon!  <br /><br />" + master.MerchantName;
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
                                            }

                                        }
                                    }
                                }
                            }
                        }
                    }
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
                EventLog.LogErrorData("Error occurred ConsumerCommonFunction/AddNewDECConsumer." + ex.Message, true);
                return Global.Consumer.ShareDECException;
            }

        }

        public string AddCheckCouponConsumer(string mobileno, string MerchantId, string CouponId, string SharedCouponId, string username)
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

                    DateTime visitdate = DateTime.Now;

                    consumerEntities dataContext = new consumerEntities();

                    merchant_master master = dataContext.merchant_master.Where(x => x.UserId == MerchantId).FirstOrDefault();

                    reviewmaster rvw = dataContext.reviewmasters.Where(x => x.MerchantId == master.merchantid).OrderByDescending(x => x.CreationDate).FirstOrDefault();

                    DateTime expirydate = expdate(master.UserId, visitdate);
                    int couponno = Convert.ToInt32(CouponId);
                    int sharedcouponno = Convert.ToInt32(SharedCouponId);

                    using (instadelight_consumerEntities ConsumerdataContext = new instadelight_consumerEntities())
                    {
                        using (consumerEntities merchantcontext = new consumerEntities())
                        {
                            var isUser = ConsumerdataContext.users.Where(u => u.UserName == mobileno).FirstOrDefault();
                            if (isUser == null)
                            {
                                //add new consumer
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
                                if (chkUser.Succeeded)
                                {
                                    var rolesForUser = UserManager.GetRoles(user.Id);
                                    if (!rolesForUser.Contains("Consumer"))
                                    {
                                        UserManager.AddToRole(user.Id, "Consumer");
                                    }

                                    //Added new code block for consumermaster

                                    InsertNewConsumer(user.Id, mobileno);
                                    isUser = ConsumerdataContext.users.Where(u => u.UserName == mobileno).FirstOrDefault();

                                }
                                else
                                {
                                    return Global.Consumer.RegisterConsumerError;
                                }
                            }

                            var isConsumerMapped = merchantcontext.merchantconsumerdetails.Where(u => u.ConsumerId == isUser.Id && u.MerchantId == master.UserId).FirstOrDefault();
                            if (isConsumerMapped == null)
                            {
                                MapMerchantToConsumer(master.UserId, isUser.Id, mobileno);

                                consumervisitdetail visit = new consumervisitdetail();
                                visit.ConsumerId = isUser.Id;
                                visit.MerchantId = master.UserId;
                                visit.VisitDate = visitdate;
                                ConsumerdataContext.consumervisitdetails.Add(visit);
                                ConsumerdataContext.SaveChanges();

                                merchantjoiningbonu bonus = dataContext.merchantjoiningbonus.Where(x => x.MerchantId == master.UserId).OrderByDescending(x => x.DateCreated).FirstOrDefault();
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
                                                detail.MerchantId = master.UserId;
                                                detail.Cashback = bonus.JoiningBonus;
                                                detail.CashbackDate = visitdate;
                                                detail.ExpiryDate = expirydate;
                                                dataConsumerContext.consumercashbackdetails.Add(detail);
                                                dataConsumerContext.SaveChanges();
                                            }
                                            else
                                            {
                                                consumerrewardpoint pts = new consumerrewardpoint();
                                                pts.ConsumerId = isUser.Id;
                                                pts.MerchantId = master.UserId;
                                                pts.Points = bonus.JoiningBonus;
                                                pts.VisitDate = visitdate;
                                                pts.ExpiryDate = expirydate;
                                                dataConsumerContext.consumerrewardpoints.Add(pts);
                                                dataConsumerContext.SaveChanges();
                                            }
                                        }
                                        else
                                        {
                                            consumerrewardpoint pts = new consumerrewardpoint();
                                            pts.ConsumerId = isUser.Id;
                                            pts.MerchantId = master.UserId;
                                            pts.Points = bonus.JoiningBonus;
                                            pts.VisitDate = visitdate;
                                            pts.ExpiryDate = expirydate;
                                            dataConsumerContext.consumerrewardpoints.Add(pts);
                                            dataConsumerContext.SaveChanges();
                                        }
                                    }
                                }
                            }

                            coupons_master cpnmaster = dataContext.coupons_master.Where(x => x.couponid == couponno).FirstOrDefault();

                            MapMerchantCouponToConsumer(master.UserId, isUser.Id, couponno, mobileno, cpnmaster.ValidFrom, cpnmaster.ValidTill);

                            //Delete this coupon for this counsumer as he has shared it
                            //merchantconsumercoupondetail sharedcoupon = merchantcontext.merchantconsumercoupondetails.Where(u => u.MerchantId == master.UserId && u.ConsumerId == userid && u.CouponId == couponno).FirstOrDefault();
                            merchantconsumercoupondetail sharedcoupon = merchantcontext.merchantconsumercoupondetails.Where(u => u.Id == sharedcouponno).FirstOrDefault();
                            if (sharedcoupon != null)
                            {
                                merchantcontext.merchantconsumercoupondetails.Remove(sharedcoupon);
                                merchantcontext.SaveChanges();
                            }

                            //Add cashback if applicable
                            if (master.RunRewardProgram == true)
                            {
                                if (master.RedeemProgram == "Cashback")
                                {
                                    cashbackdetail cashback = dataContext.cashbackdetails.Where(x => x.MerchantId == master.UserId).OrderByDescending(x => x.CreationDate).FirstOrDefault();
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
                                                            rs.MerchantId = master.UserId;
                                                            rs.Cashback = Convert.ToInt32(cashback.FixedCashBack);
                                                            rs.CashbackDate = visitdate;
                                                            rs.ExpiryDate = expirydate;
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

                            if (mobileno.Contains("@") == false)
                            {
                                string smsresult = sms.sendMessage(mobileno, username + " has sent a great offer from " + master.DECName + " to you. Download the app for Android at goo.gl/r5rxjj and for Apple at goo.gl/dAq4er. Your cell number is your login and your password is 123456 unless you have reset it.");
                                consumersmsdetail smsdetails = new consumersmsdetail();
                                smsdetails.ConsumerId = isUser.Id;
                                smsdetails.MerchantId = master.UserId;
                                smsdetails.SMSEmailStatus = smsresult;
                                smsdetails.UserName = mobileno;
                                smsdetails.SentDate = visitdate;
                                ConsumerdataContext.consumersmsdetails.Add(smsdetails);
                                ConsumerdataContext.SaveChanges();

                                return Global.Consumer.ShareCouponMessage;
                            }
                            else
                            {
                                EmailModel model = new EmailModel();
                                model.To = mobileno;
                                model.Email = "You_Rock@offertraker.com";
                                model.Subject = "Your friend has shared a Digital Card with you";
                                model.Body = "Dear " + mobileno + ",<br /><br /> Your friend " + username + " has sent you the digital card of " + master.DECName + ". <br /> The digital card is a great way to keep a track of special offers for you from various merchants, without having to search for offers in emails, smss or online. <br />So, go ahead and download the offertraker app for Android here:goo.gl/r5rxjj and for Apple here: goo.gl/dAq4er. Your login is your email-id and your password is 123456.<br /><br /> Best wishes, <br /><br />Offertraker team.";
                                SendEmail email = new SendEmail();
                                string result = email.SendEmailToConsumer(model);
                                consumersmsdetail smsdetails = new consumersmsdetail();
                                smsdetails.ConsumerId = isUser.Id;
                                smsdetails.MerchantId = master.UserId;
                                smsdetails.SMSEmailStatus = result;
                                smsdetails.UserName = mobileno;
                                smsdetails.SentDate = visitdate;
                                ConsumerdataContext.consumersmsdetails.Add(smsdetails);
                                ConsumerdataContext.SaveChanges();

                                return Global.Consumer.ShareCouponMessage;
                            }
                        }
                    }
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
                EventLog.LogErrorData("Error occurred ConsumerCommonFunction/AddCheckCouponConsumer." + ex.Message, true);
                return Global.Consumer.ShareDECException;
            }

        }

        public string SendGCToConsumer(string mobileno, string Id, string MerchantId, string userid, string username)
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
                    int merchantno = Convert.ToInt32(MerchantId);
                    int cid = Convert.ToInt32(Id);
                    merchant_master master = dataContext.merchant_master.Where(x => x.merchantid == merchantno).FirstOrDefault();
                    string merchantuserid = master.UserId;
                    DateTime visitdate = DateTime.Now;

                    using (instadelight_consumerEntities ConsumerdataContext = new instadelight_consumerEntities())
                    {
                        var isUser = ConsumerdataContext.users.Where(u => u.UserName == mobileno).FirstOrDefault();
                        consumermaster sender = ConsumerdataContext.consumermasters.Where(x => x.UserId == userid).FirstOrDefault();

                        consumergiftcarddetail giftcard = ConsumerdataContext.consumergiftcarddetails.Where(x => x.Id == cid).FirstOrDefault();
                        if (isUser != null)
                        {
                            //Check if Mapping Exist else insert Mapping of current merchant.
                            using (consumerEntities merchantcontext = new consumerEntities())
                            {

                                var isConsumerMapped = merchantcontext.merchantconsumerdetails.Where(u => u.ConsumerPhone == mobileno && u.MerchantId == merchantuserid).FirstOrDefault();
                                if (isConsumerMapped == null)
                                {
                                    MapMerchantToConsumer(master.UserId, isUser.Id, mobileno);

                                    merchantjoiningbonu bonus = dataContext.merchantjoiningbonus.Where(x => x.MerchantId == master.UserId).OrderByDescending(x => x.DateCreated).FirstOrDefault();
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
                                                    detail.MerchantId = master.UserId;
                                                    detail.Cashback = bonus.JoiningBonus;
                                                    detail.CashbackDate = visitdate;
                                                    dataConsumerContext.consumercashbackdetails.Add(detail);
                                                    dataConsumerContext.SaveChanges();
                                                }
                                                else
                                                {
                                                    consumerrewardpoint pts = new consumerrewardpoint();
                                                    pts.ConsumerId = isUser.Id;
                                                    pts.MerchantId = master.UserId;
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
                                                pts.MerchantId = master.UserId;
                                                pts.Points = bonus.JoiningBonus;
                                                pts.VisitDate = visitdate;
                                                dataConsumerContext.consumerrewardpoints.Add(pts);
                                                dataConsumerContext.SaveChanges();
                                            }
                                        }
                                    }
                                }
                                //Add giftcard to consumer
                                consumergiftcarddetail gcd = new consumergiftcarddetail();
                                gcd.GiftCardId = giftcard.GiftCardId;
                                gcd.ConsumerId = isUser.Id;
                                gcd.MerchantId = master.UserId;
                                gcd.DenominationRs = giftcard.DenominationRs;
                                gcd.Status = 0;
                                gcd.DateOfPurchase = visitdate;
                                gcd.ValidTill = visitdate.AddDays(364);
                                ConsumerdataContext.consumergiftcarddetails.Add(gcd);
                                ConsumerdataContext.SaveChanges();

                                giftcard.Status = 1;
                                ConsumerdataContext.SaveChanges();

                                //Send SMS/Email to consumer for a new gift card                             

                                if (isUser.UserName.Contains('@') == false)
                                {
                                    string smsresult = sms.sendMessage(isUser.UserName, "Dear " + isUser.UserName + ", " + username + " has sent you a Gift Card. To avail the Gift card, please download the Offertraker app for Android at goo.gl/r5rxjj and for Apple at goo.gl/dAq4er. Your cell number is your login and your password is 123456. Offertraker team");

                                    consumersmsdetail smsdetails = new consumersmsdetail();
                                    smsdetails.ConsumerId = isUser.Id;
                                    smsdetails.MerchantId = master.UserId;
                                    smsdetails.SMSEmailStatus = smsresult;
                                    smsdetails.UserName = isUser.UserName;
                                    smsdetails.SentDate = DateTime.Now;
                                    ConsumerdataContext.consumersmsdetails.Add(smsdetails);
                                    ConsumerdataContext.SaveChanges();
                                }
                                else
                                {
                                    EmailModel model = new EmailModel();
                                    model.To = isUser.UserName;
                                    model.Email = "welcome@offertraker.com";
                                    model.Subject = "Congrates! Your friend has sent you a gift card!";
                                    if (string.IsNullOrEmpty(sender.Email) == false)
                                        model.cc = sender.Email;

                                    model.Body = "Dear " + isUser.UserName + ",<br /> Your friend " + username + " has sent you a gift card of " + master.DECName + ". <br />To avail the gift card, please go ahead and download the Offertraker app for Android here:goo.gl/r5rxjj and for Apple here: goo.gl/dAq4er. Your login is your email-id and your password is 123456.<br /> Best wishes, <br />Offertraker team.";

                                    SendEmail email = new SendEmail();
                                    string result = email.SendEmailToConsumer(model);
                                    consumersmsdetail smsdetails = new consumersmsdetail();
                                    smsdetails.ConsumerId = isUser.Id;
                                    smsdetails.MerchantId = master.UserId;
                                    smsdetails.SMSEmailStatus = result;
                                    smsdetails.UserName = isUser.UserName;
                                    smsdetails.SentDate = DateTime.Now;
                                    ConsumerdataContext.consumersmsdetails.Add(smsdetails);
                                    ConsumerdataContext.SaveChanges();
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

                                //Added new code block for consumermaster
                                using (instadelight_consumerEntities consumerContext = new instadelight_consumerEntities())
                                {
                                    InsertNewConsumer(user.Id, mobileno);
                                    merchantjoiningbonu bonus = dataContext.merchantjoiningbonus.Where(x => x.MerchantId == master.UserId).OrderByDescending(x => x.DateCreated).FirstOrDefault();
                                    if (bonus != null)
                                    {
                                        using (instadelight_consumerEntities dataConsumerContext = new instadelight_consumerEntities())
                                        {
                                            if (master.RunRewardProgram == true)
                                            {
                                                if (master.RedeemProgram == "Cashback")
                                                {
                                                    consumercashbackdetail detail = new consumercashbackdetail();
                                                    detail.ConsumerId = user.Id;
                                                    detail.MerchantId = master.UserId;
                                                    detail.Cashback = bonus.JoiningBonus;
                                                    detail.CashbackDate = visitdate;
                                                    dataConsumerContext.consumercashbackdetails.Add(detail);
                                                    dataConsumerContext.SaveChanges();
                                                }
                                                else
                                                {
                                                    consumerrewardpoint pts = new consumerrewardpoint();
                                                    pts.ConsumerId = user.Id;
                                                    pts.MerchantId = master.UserId;
                                                    pts.Points = bonus.JoiningBonus;
                                                    pts.VisitDate = visitdate;
                                                    dataConsumerContext.consumerrewardpoints.Add(pts);
                                                    dataConsumerContext.SaveChanges();
                                                }
                                            }
                                            else
                                            {
                                                consumerrewardpoint pts = new consumerrewardpoint();
                                                pts.ConsumerId = user.Id;
                                                pts.MerchantId = master.UserId;
                                                pts.Points = bonus.JoiningBonus;
                                                pts.VisitDate = visitdate;
                                                dataConsumerContext.consumerrewardpoints.Add(pts);
                                                dataConsumerContext.SaveChanges();
                                            }
                                        }
                                    }
                                }

                                using (consumerEntities merchantcontext = new consumerEntities())
                                {
                                    MapMerchantToConsumer(master.UserId, user.Id, mobileno);

                                    //Add giftcard to consumer
                                    consumergiftcarddetail gcd = new consumergiftcarddetail();
                                    gcd.GiftCardId = giftcard.GiftCardId;
                                    gcd.ConsumerId = user.Id;
                                    gcd.MerchantId = master.UserId;
                                    gcd.DenominationRs = giftcard.DenominationRs;
                                    gcd.Status = 0;
                                    gcd.DateOfPurchase = visitdate;
                                    gcd.ValidTill = visitdate.AddDays(364);
                                    ConsumerdataContext.consumergiftcarddetails.Add(gcd);
                                    ConsumerdataContext.SaveChanges();

                                    giftcard.Status = 1;
                                    ConsumerdataContext.SaveChanges();

                                    if (isUser.UserName.Contains('@') == false)
                                    {
                                        string smsresult = sms.sendMessage(user.UserName, "Dear " + user.UserName + ", " + username + " has sent you a Gift Card. To avail the Gift card, please download the Offertraker app for Android at goo.gl/r5rxjj and for Apple at goo.gl/dAq4er. Your cell number is your login and your password is 123456. Offertraker team");

                                        consumersmsdetail smsdetails = new consumersmsdetail();
                                        smsdetails.ConsumerId = user.Id;
                                        smsdetails.MerchantId = master.UserId;
                                        smsdetails.SMSEmailStatus = smsresult;
                                        smsdetails.UserName = user.UserName;
                                        smsdetails.SentDate = DateTime.Now;
                                        ConsumerdataContext.consumersmsdetails.Add(smsdetails);
                                        ConsumerdataContext.SaveChanges();
                                    }
                                    else
                                    {
                                        EmailModel model = new EmailModel();
                                        model.To = user.UserName;
                                        model.Email = "welcome@offertraker.com";
                                        model.Subject = "Congrates! Your friend has sent you a gift card!";
                                        if (string.IsNullOrEmpty(sender.Email) == false)
                                            model.cc = sender.Email;

                                        model.Body = "Dear " + user.UserName + ",<br /> Your friend " + username + " has sent you a gift card of " + master.DECName + ". <br />To avail the gift card, please go ahead and download the Offertraker app for Android here:goo.gl/r5rxjj and for Apple here: goo.gl/dAq4er. Your login is your email-id and your password is 123456.<br /> Best wishes, <br />Offertraker team.";

                                        SendEmail email = new SendEmail();
                                        string result = email.SendEmailToConsumer(model);
                                        consumersmsdetail smsdetails = new consumersmsdetail();
                                        smsdetails.ConsumerId = user.Id;
                                        smsdetails.MerchantId = master.UserId;
                                        smsdetails.SMSEmailStatus = result;
                                        smsdetails.UserName = user.UserName;
                                        smsdetails.SentDate = DateTime.Now;
                                        ConsumerdataContext.consumersmsdetails.Add(smsdetails);
                                        ConsumerdataContext.SaveChanges();
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

        public DateTime expdate(string UserId, DateTime visitdate)
        {
            DateTime expirydate = DateTime.Now;
            consumerEntities dataContext = new consumerEntities();
            pointscashbackexpiry exp = dataContext.pointscashbackexpiries.Where(x => x.MerchantId == UserId).OrderByDescending(x => x.CreationDate).FirstOrDefault();
            if (exp != null)
            {
                expiryperiodmaster expmaster = dataContext.expiryperiodmasters.Where(x => x.Id == exp.ExpPeriodId).FirstOrDefault();
                if (expmaster != null)
                {
                    switch (expmaster.expunit)
                    {
                        case "Days":
                            expirydate = visitdate.AddDays(Convert.ToDouble(expmaster.expperiod));
                            break;

                        case "Month":
                            expirydate = visitdate.AddMonths(Convert.ToInt32(expmaster.expperiod));
                            break;

                        case "Year":
                            expirydate = visitdate.AddYears(Convert.ToInt32(expmaster.expperiod));
                            break;
                    }
                }
            }
            return expirydate;
        }

        public void InsertNewConsumer(string UserId, string MobileNo)
        {
            string cellno = "";
            string email = "";
            if (MobileNo.Contains('@') == true)
                email = MobileNo;
            else
                cellno = MobileNo;

            MySqlParameter param1 = new MySqlParameter();
            param1.Value = UserId;
            param1.Direction = System.Data.ParameterDirection.Input;
            param1.ParameterName = "@UserId";
            param1.DbType = System.Data.DbType.String;

            MySqlParameter param2 = new MySqlParameter();
            param2.Value = cellno;
            param2.Direction = System.Data.ParameterDirection.Input;
            param2.ParameterName = "@Phone1";
            param2.DbType = System.Data.DbType.String;

            MySqlParameter param3 = new MySqlParameter();
            param3.Value = email;
            param3.Direction = System.Data.ParameterDirection.Input;
            param3.ParameterName = "@Email";
            param3.DbType = System.Data.DbType.String;
            using (instadelight_consumerEntities ConsumerdataContext = new instadelight_consumerEntities())
            {
                ConsumerdataContext.Database.ExecuteSqlCommand("CALL InsertConsumer(@UserId, @Phone1,@Email)", param1, param2, param3);
            }
        }

        public void MapMerchantReviewToConsumer(string merchantid, string consumerid, int reviewid)
        {
            MySqlParameter param1 = new MySqlParameter();
            param1.Value = merchantid;
            param1.Direction = System.Data.ParameterDirection.Input;
            param1.ParameterName = "@MerchantId";
            param1.DbType = System.Data.DbType.String;

            MySqlParameter param2 = new MySqlParameter();
            param2.Value = consumerid;
            param2.Direction = System.Data.ParameterDirection.Input;
            param2.ParameterName = "@ConsumerId";
            param2.DbType = System.Data.DbType.String;

            MySqlParameter param3 = new MySqlParameter();
            param3.Value = reviewid;
            param3.Direction = System.Data.ParameterDirection.Input;
            param3.ParameterName = "@ReviewId";
            param3.DbType = System.Data.DbType.Int32;

            MySqlParameter param4 = new MySqlParameter();
            param4.Value = "Shared";
            param4.Direction = System.Data.ParameterDirection.Input;
            param4.ParameterName = "@Status";
            param4.DbType = System.Data.DbType.String;

            MySqlParameter param5 = new MySqlParameter();
            param5.Value = DateTime.Now;
            param5.Direction = System.Data.ParameterDirection.Input;
            param5.ParameterName = "@SharedDate";
            param5.DbType = System.Data.DbType.DateTime;

            DateTime comparedate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 12, 0, 0);
            DateTime validtill = new DateTime();

            if (DateTime.Now < comparedate)
            {
                DateTime newvalidtill = DateTime.Now.AddDays(1);

                validtill = new DateTime(newvalidtill.Year, newvalidtill.Month, newvalidtill.Day, 12, 0, 0);
            }
            else
            {
                DateTime newvalidtill = DateTime.Now.AddDays(1);
                validtill = new DateTime(newvalidtill.Year, newvalidtill.Month, newvalidtill.Day, 23, 59, 0);
            }

            MySqlParameter param6 = new MySqlParameter();
            param6.Value = validtill;
            param6.Direction = System.Data.ParameterDirection.Input;
            param6.ParameterName = "@ValidTill";
            param6.DbType = System.Data.DbType.DateTime;

            using (consumerEntities dataContext = new consumerEntities())
            {
                dataContext.Database.ExecuteSqlCommand("CALL InsertMerchantReviewDetails(@MerchantId, @ConsumerId,@ReviewId,@Status,@SharedDate,@ValidTill)", param1, param2, param3, param4, param5, param6);
            }
        }

        public void SendSharedCoupons(int merchantid, string merchantuserid, string consumerId, string consumerPhone)
        {
            using (consumerEntities merchantcontext = new consumerEntities())
            {
                var sharedcoupons = merchantcontext.coupons_master.Where(x => x.MerchantId == merchantid && x.ShareWithAll == 1).ToList();
                if (sharedcoupons != null)
                {
                    foreach (var coupon in sharedcoupons)
                    {
                        var sentcoupon = merchantcontext.merchantconsumercoupondetails.Where(x => x.MerchantId == merchantuserid && x.ConsumerId == consumerId && x.CouponId == coupon.couponid).FirstOrDefault();
                        if (sentcoupon == null)
                        {
                            MapMerchantCouponToConsumer(merchantuserid, consumerId, coupon.couponid, consumerPhone, coupon.ValidFrom, coupon.ValidTill);
                        }
                    }
                }
            }
        }

        public void MapMerchantCouponToConsumer(string merchantuserid, string consumerId, int couponid, string consumerPhone, DateTime? ValidFrom, DateTime? ValidTo)
        {
            MySqlParameter param1 = new MySqlParameter();
            param1.Value = merchantuserid;
            param1.Direction = System.Data.ParameterDirection.Input;
            param1.ParameterName = "@MerchantId";
            param1.DbType = System.Data.DbType.String;

            MySqlParameter param2 = new MySqlParameter();
            param2.Value = couponid;
            param2.Direction = System.Data.ParameterDirection.Input;
            param2.ParameterName = "@CouponId";
            param2.DbType = System.Data.DbType.Int32;

            MySqlParameter param3 = new MySqlParameter();
            param3.Value = consumerId;
            param3.Direction = System.Data.ParameterDirection.Input;
            param3.ParameterName = "@ConsumerId";
            param3.DbType = System.Data.DbType.String;

            MySqlParameter param4 = new MySqlParameter();
            param4.Value = consumerPhone;
            param4.Direction = System.Data.ParameterDirection.Input;
            param4.ParameterName = "@ConsumerPhone";
            param4.DbType = System.Data.DbType.String;

            MySqlParameter param5 = new MySqlParameter();
            param5.Value = ValidFrom;
            param5.Direction = System.Data.ParameterDirection.Input;
            param5.ParameterName = "@ValidFrom";
            param5.DbType = System.Data.DbType.DateTime;

            MySqlParameter param6 = new MySqlParameter();
            param6.Value = ValidTo;
            param6.Direction = System.Data.ParameterDirection.Input;
            param6.ParameterName = "@ValidTo";
            param6.DbType = System.Data.DbType.DateTime;

            using (consumerEntities dataContext = new consumerEntities())
            {
                dataContext.Database.ExecuteSqlCommand("CALL InsertConsumerCouponDetails(@MerchantId, @CouponId,@ConsumerId,@ConsumerPhone,@ValidFrom,@ValidTo)", param1, param2, param3, param4, param5, param6);
            }
        }

        public void MapMerchantToConsumer(string merchantid, string consumerid, string phoneno)
        {
            MySqlParameter param1 = new MySqlParameter();
            param1.Value = merchantid;
            param1.Direction = System.Data.ParameterDirection.Input;
            param1.ParameterName = "@MerchantId";
            param1.DbType = System.Data.DbType.String;
            MySqlParameter param2 = new MySqlParameter();
            param2.Value = consumerid;
            param2.Direction = System.Data.ParameterDirection.Input;
            param2.ParameterName = "@ConsumerId";
            param2.DbType = System.Data.DbType.String;
            MySqlParameter param3 = new MySqlParameter();
            param3.Value = phoneno;
            param3.Direction = System.Data.ParameterDirection.Input;
            param3.ParameterName = "@ConsumerPhone";
            param3.DbType = System.Data.DbType.String;
            using (consumerEntities dataContext = new consumerEntities())
            {
                dataContext.Database.ExecuteSqlCommand("CALL InsertMerchantConsumerDetails(@MerchantId, @ConsumerId,@ConsumerPhone)", param1, param2, param3);
            }
        }
    }
}