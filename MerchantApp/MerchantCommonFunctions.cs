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

namespace MerchantApp
{
    public class MerchantCommonFunctions
    {
        ApplicationDbContext Consumercontext = new ApplicationDbContext("DefaultConsumerConnection");

        public void InsertBillAmount(string consumerid, string MerchantId, decimal bill, DateTime visitdate)
        {
            MySqlParameter param1 = new MySqlParameter();
            param1.Value = consumerid;
            param1.Direction = System.Data.ParameterDirection.Input;
            param1.ParameterName = "@ConsumerId";
            param1.DbType = System.Data.DbType.String;
            MySqlParameter param2 = new MySqlParameter();
            param2.Value = MerchantId;
            param2.Direction = System.Data.ParameterDirection.Input;
            param2.ParameterName = "@MerchantId";
            param2.DbType = System.Data.DbType.String;
            MySqlParameter param3 = new MySqlParameter();
            param3.Value = bill;
            param3.Direction = System.Data.ParameterDirection.Input;
            param3.ParameterName = "@BillAmount";
            param3.DbType = System.Data.DbType.Decimal;

            MySqlParameter param4 = new MySqlParameter();
            param4.Value = visitdate;
            param4.Direction = System.Data.ParameterDirection.Input;
            param4.ParameterName = "@VisitDate";
            param4.DbType = System.Data.DbType.DateTime;

            using (instadelight_consumerEntities ConsumerdataContext = new instadelight_consumerEntities())
            {
                ConsumerdataContext.Database.ExecuteSqlCommand("CALL InsertConsumerBillDetails(@ConsumerId, @MerchantId,@BillAmount,@VisitDate)", param1, param2, param3, param4);
            }
        }

        public void MapMerchantToConsumer(string MerchantId, string consumerid, string phoneno)
        {
            MySqlParameter param1 = new MySqlParameter();
            param1.Value = MerchantId;
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
            using (MerchantEntities dataContext = new MerchantEntities())
            {
                dataContext.Database.ExecuteSqlCommand("CALL InsertMerchantConsumerDetails(@MerchantId, @ConsumerId,@ConsumerPhone)", param1, param2, param3);
            }
        }

        public void SendSharedCoupons(int MerchantId, string merchantuserid, string consumerId, string consumerPhone)
        {
            using (MerchantEntities merchantcontext = new MerchantEntities())
            {
                var sharedcoupons = merchantcontext.coupons_master.Where(x => x.MerchantId == MerchantId && x.ShareWithAll == 1).ToList();
                if (sharedcoupons != null)
                {
                    foreach (var coupon in sharedcoupons)
                    {
                        bool sharethiscoupon = true;
                        if (Convert.ToInt32(coupon.MaxCoupons) != 0)
                        {
                            int couponsshared = merchantcontext.merchantconsumercoupondetails.Where(x => x.CouponId == coupon.couponid).Count();
                            if (couponsshared == Convert.ToInt32(coupon.MaxCoupons))
                            {
                                sharethiscoupon = false;
                            }
                        }

                        if (sharethiscoupon)
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

            using (MerchantEntities dataContext = new MerchantEntities())
            {
                dataContext.Database.ExecuteSqlCommand("CALL InsertConsumerCouponDetails(@MerchantId, @CouponId,@ConsumerId,@ConsumerPhone,@ValidFrom,@ValidTo)", param1, param2, param3, param4, param5, param6);
            }
        }

        public void MapMerchantReviewToConsumer(string MerchantId, string consumerid, int reviewid)
        {
            MySqlParameter param1 = new MySqlParameter();
            param1.Value = MerchantId;
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

            using (MerchantEntities dataContext = new MerchantEntities())
            {
                dataContext.Database.ExecuteSqlCommand("CALL InsertMerchantReviewDetails(@MerchantId, @ConsumerId,@ReviewId,@Status,@SharedDate,@ValidTill)", param1, param2, param3, param4, param5, param6);
            }
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

        public void SendReviewsToDECs(reviewmaster rvw)
        {
            try
            {
                using (MerchantEntities dataContext = new MerchantEntities())
                {

                    MySqlParameter param1 = new MySqlParameter();
                    param1.Value = rvw.MerchantUserId;
                    param1.Direction = System.Data.ParameterDirection.Input;
                    param1.ParameterName = "@MerchantId";
                    param1.DbType = System.Data.DbType.String;


                    List<string> ConsumerList = dataContext.Database.SqlQuery<string>("CALL GetConsumersToShareReview(@MerchantId)", param1)
                                   .Select(x => x).ToList();

                    foreach (string c in ConsumerList)
                    {
                        MapMerchantReviewToConsumer(rvw.MerchantUserId, c, rvw.reviewid);
                    }

                }
            }
            catch (Exception ex)
            {
                //call error view here
                //log the error
                EventLog.LogErrorData("Error occured Consumer/GetCoupons." + ex.Message, true);
            }

        }



        public string AddNewCouponConsumer(string mobileno, string CouponId, string userid)
        {
            try
            {
                SMSUtility sms = new SMSUtility();
                if (mobileno != null)
                {
                    DateTime visitdate = DateTime.Now;
                    var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(Consumercontext));
                    var UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(Consumercontext));
                    UserManager.UserValidator = new UserValidator<ApplicationUser>(UserManager)
                    {
                        AllowOnlyAlphanumericUserNames = false,
                        RequireUniqueEmail = true
                    };

                    MerchantEntities dataContext = new MerchantEntities();
                    merchant_master master = dataContext.merchant_master.Where(x => x.UserId == userid).FirstOrDefault();
                    int couponid = Convert.ToInt32(CouponId);
                    bool isNewConsumer = false;
                    DateTime expirydate = expdate(master.UserId, visitdate);

                    using (instadelight_consumerEntities ConsumerdataContext = new instadelight_consumerEntities())
                    {
                        using (MerchantEntities merchantcontext = new MerchantEntities())
                        {
                            bool sharethiscoupon = true;
                            coupons_master cpn = merchantcontext.coupons_master.Where(x => x.couponid == couponid).FirstOrDefault();
                            if (Convert.ToInt32(cpn.MaxCoupons) != 0)
                            {
                                int couponsshared = merchantcontext.merchantconsumercoupondetails.Where(x => x.CouponId == couponid).Count();
                                if (couponsshared == Convert.ToInt32(cpn.MaxCoupons))
                                {
                                    sharethiscoupon = false;
                                }
                            }
                            if (sharethiscoupon)
                            {
                                //check if consumer exists. Create if does not exist.
                                var isUser = ConsumerdataContext.consumerusers.Where(u => u.UserName == mobileno).FirstOrDefault();
                                if (isUser == null)
                                {
                                    isNewConsumer = true;
                                    //Add consumer in database.
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
                                        //Add consumer in role "Consumer"
                                        var rolesForUser = UserManager.GetRoles(user.Id);
                                        if (!rolesForUser.Contains("Consumer"))
                                        {
                                            UserManager.AddToRole(user.Id, "Consumer");
                                        }
                                        //Insert consumer in consumer_master table
                                        InsertNewConsumer(user.Id, mobileno);
                                        isUser = ConsumerdataContext.consumerusers.Where(u => u.UserName == mobileno).FirstOrDefault();
                                    }
                                    else
                                    {
                                        foreach (string err in chkUser.Errors)
                                        {
                                            EventLog.LogErrorData("Error occurred MerchantCommonFunctions/AddNewCouponConsumer. " + err, true);
                                        }
                                        return "Error occurred while creating new consumer with username " + mobileno;
                                    }
                                }

                                //Map merchant and consumer if not already done.
                                var isConsumerMapped = merchantcontext.merchantconsumerdetails.Where(u => u.ConsumerId == isUser.Id && u.MerchantId == userid).FirstOrDefault();
                                if (isConsumerMapped == null)
                                {
                                    isNewConsumer = true;
                                    MapMerchantToConsumer(userid, isUser.Id, mobileno);

                                    //Add no of visits
                                    consumervisitdetail visit = new consumervisitdetail();
                                    visit.ConsumerId = isUser.Id;
                                    visit.MerchantId = userid;
                                    visit.VisitDate = visitdate;
                                    ConsumerdataContext.consumervisitdetails.Add(visit);
                                    ConsumerdataContext.SaveChanges();

                                    //Give joining bonus if applicable
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
                                                    detail.ExpiryDate = expirydate;
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
                                                    pts.ExpiryDate = expirydate;
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
                                                pts.ExpiryDate = expirydate;
                                                dataConsumerContext.consumerrewardpoints.Add(pts);
                                                dataConsumerContext.SaveChanges();
                                            }
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
                                }

                                coupons_master cpnmaster = dataContext.coupons_master.Where(x => x.couponid == couponid).FirstOrDefault();
                                MapMerchantCouponToConsumer(userid, isUser.Id, couponid, mobileno, cpnmaster.ValidFrom, cpnmaster.ValidTill);


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

                                if (mobileno.Contains("@") == false)
                                {
                                    string smsresult = "";
                                    if (isNewConsumer)
                                    {
                                        smsresult = sms.sendMessage(mobileno, "Congrats!! Welcome to " + master.MerchantName + " and thanks for registering with us. Download the app for Android at goo.gl/r5rxjj and for Apple at goo.gl/dAq4er. Your cell number is your login and your password is 123456.");
                                    }
                                    else
                                    {
                                        smsresult = sms.sendMessage(mobileno, "Dear Customer, your " + master.MerchantName + " Digital Card is updated. Download the app for Android at goo.gl/r5rxjj and for Apple at goo.gl/dAq4er. Your cell number is your login and your password is 123456 unless you have reset it.");
                                    }

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
                                    return Global.Merchant.CouponSharedSuccessfully;
                                }
                                else
                                {
                                    EmailModel model = new EmailModel();
                                    model.To = mobileno;
                                    model.Email = "welcome@offertraker.com";
                                    model.Subject = "Exclusive offer for you!";

                                    model.Body = "Dear " + mobileno + ", <br /><br />" + master.MerchantName + " has shared an exclusive coupon with you. It comes to you on a very exciting and personalized digital card. <br />Go ahead and enjoy the privilege. Please download the offertraker app for Android here :goo.gl/r5rxjj and for Apple here :goo.gl/dAq4er.  If you already have the app, the coupon is now available on the DEC of " + master.MerchantName + ", and you can enjoy it as per your convenience. Your login is your email-id and your password is 123456. <br /><br />Best wishes, <br /><br />" + master.MerchantName;
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
                                    return Global.Merchant.CouponSharedSuccessfully;
                                }
                            }
                            else
                            {
                                return Global.Merchant.MaxCouponMessage;
                            }
                        }
                    }
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
                EventLog.LogErrorData("Error occured MerchantWebService/AddNewCouponConsumer." + ex.Message, true);

                return "Error occured while sending coupon to consumer";
            }

        }

        public bool CheckDate(String date)
        {
            try
            {
                DateTime dt = DateTime.Parse(date);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public DateTime expdate(string UserId, DateTime visitdate)
        {
            DateTime expirydate = DateTime.Now;
            MerchantEntities dataContext = new MerchantEntities();
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

        public string AddNewDECConsumer(ShareDECParameters param, string userid)
        {
            try
            {
                DateTime visitdate = DateTime.Now;
                bool isNewConsumer = false;

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


                    merchant_master master = dataContext.merchant_master.Where(x => x.UserId == userid).FirstOrDefault();

                    decimal bill = 0;
                    if (string.IsNullOrEmpty(param.BillAmt) == false)
                        bill = Convert.ToDecimal(param.BillAmt);

                    reviewmaster rvw = dataContext.reviewmasters.Where(x => x.MerchantId == master.merchantid).OrderByDescending(x => x.CreationDate).FirstOrDefault();
                    DateTime expirydate = expdate(master.UserId, visitdate);

                    using (instadelight_consumerEntities ConsumerdataContext = new instadelight_consumerEntities())
                    {
                        using (MerchantEntities merchantcontext = new MerchantEntities())
                        {
                            //check if consumer exists. Create if does not exist.
                            var isUser = ConsumerdataContext.consumerusers.Where(u => u.UserName == param.mobileno).FirstOrDefault();
                            if (isUser == null)
                            {
                                isNewConsumer = true;
                                //Add consumer in database.
                                //Create consumer and corresponding mapping in merchantentities.
                                var user = new ApplicationUser();
                                user.UserName = param.mobileno;
                                string userPWD = "123456";
                                user.FirstName = "C";
                                user.LastName = "";
                                if (param.mobileno.Contains('@') == false)
                                {
                                    user.Phone = param.mobileno;
                                    user.PhoneNumber = param.mobileno;
                                    user.Email = "test@test.com";
                                }
                                else
                                {
                                    user.Email = param.mobileno;
                                }
                                var chkUser = UserManager.Create(user, userPWD);

                                if (chkUser.Succeeded)
                                {
                                    //Add consumer in role "Consumer"
                                    var rolesForUser = UserManager.GetRoles(user.Id);
                                    if (!rolesForUser.Contains("Consumer"))
                                    {
                                        UserManager.AddToRole(user.Id, "Consumer");
                                    }
                                    //Insert consumer in consumer_master table
                                    InsertNewConsumer(user.Id, param.mobileno);
                                    isUser = ConsumerdataContext.consumerusers.Where(u => u.UserName == param.mobileno).FirstOrDefault();
                                }
                                else
                                {
                                    foreach (string err in chkUser.Errors)
                                    {
                                        EventLog.LogErrorData("Error occurred MerchantCommonFunctions/AddNewDECConsumer. " + err, true);
                                    }
                                    return "Error occurred while creating new consumer with username " + param.mobileno;
                                }
                            }

                            //Map merchant and consumer if not already done.
                            var isConsumerMapped = merchantcontext.merchantconsumerdetails.Where(u => u.ConsumerId == isUser.Id && u.MerchantId == userid).FirstOrDefault();
                            if (isConsumerMapped == null)
                            {
                                isNewConsumer = true;
                                MapMerchantToConsumer(userid, isUser.Id, param.mobileno);

                                //Add no of visits
                                consumervisitdetail visit = new consumervisitdetail();
                                visit.ConsumerId = isUser.Id;
                                visit.MerchantId = userid;
                                visit.VisitDate = visitdate;
                                ConsumerdataContext.consumervisitdetails.Add(visit);
                                ConsumerdataContext.SaveChanges();

                                //Give joining bonus if applicable
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
                                                detail.ExpiryDate = expirydate;
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
                                                pts.ExpiryDate = expirydate;
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
                                            pts.ExpiryDate = expirydate;
                                            dataConsumerContext.consumerrewardpoints.Add(pts);
                                            dataConsumerContext.SaveChanges();
                                        }
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
                            }

                            //Insert bill amount in bill details
                            if (bill > 0)
                            {
                                InsertBillAmount(isUser.Id, userid, bill, visitdate);
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

                            //Add points on bill amount
                            if (bill > 0)
                            {
                                if (master.RunRewardProgram == true)
                                {
                                    rewardmaster rws = dataContext.rewardmasters.Where(x => x.MerchantId == userid).OrderByDescending(x => x.CreationDate).FirstOrDefault();
                                    if (rws != null)
                                    {
                                        if (rws.RewardRs != null && rws.RewardPoints != null)
                                        {
                                            int points = (Convert.ToInt32(rws.RewardPoints) * Convert.ToInt32(bill)) / Convert.ToInt32(rws.RewardRs);
                                            using (instadelight_consumerEntities dataConsumerContext = new instadelight_consumerEntities())
                                            {
                                                consumerrewardpoint pts = new consumerrewardpoint();
                                                pts.ConsumerId = isUser.Id;
                                                pts.MerchantId = userid;
                                                pts.Points = points;
                                                pts.VisitDate = visitdate;
                                                pts.ExpiryDate = expirydate;
                                                dataConsumerContext.consumerrewardpoints.Add(pts);
                                                dataConsumerContext.SaveChanges();
                                            }
                                        }
                                    }
                                }
                            }

                            //Send shared coupons.
                            SendSharedCoupons(master.merchantid, userid, isUser.Id, param.mobileno);

                            //Send review question                                            
                            if (rvw != null)
                            {
                                //SendReviewsToDECs(rvw);
                                MapMerchantReviewToConsumer(rvw.MerchantUserId, isUser.Id, rvw.reviewid);
                            }

                            //Send SMS to consumer for a new DEC. 'Navigation://OpenQRCodeScanner'
                            if (param.mobileno.Contains('@') == false)
                            {
                                SMSUtility sms = new SMSUtility();
                                //string smsresult = sms.sendMessage(param.mobileno, "Dear Customer, your " + Session["UserName"] + " Digital Card is updated. Download the app for Android at goo.gl/r5rxjj and for Apple at goo.gl/dAq4er. Your cell number is your login and your password is 123456 unless you have reset it.");
                                string smsresult = "";

                                if (isNewConsumer)
                                {
                                    smsresult = sms.sendMessage(param.mobileno, "Congrats!! Welcome to " + master.MerchantName + " and thanks for registering with us. Download the app for Android at goo.gl/r5rxjj and for Apple at goo.gl/dAq4er. Your cell number is your login and your password is 123456.");
                                }
                                else
                                {
                                    smsresult = sms.sendMessage(param.mobileno, "Dear Customer, your " + master.MerchantName + " Digital Card is updated. Download the app for Android at goo.gl/r5rxjj and for Apple at goo.gl/dAq4er. Your cell number is your login and your password is 123456 unless you have reset it.");
                                }
                                consumersmsdetail smsdetails = new consumersmsdetail();
                                smsdetails.ConsumerId = isUser.Id;
                                smsdetails.MerchantId = userid;
                                smsdetails.SMSEmailStatus = smsresult;
                                smsdetails.UserName = param.mobileno;
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
                                model.To = param.mobileno;
                                model.Email = "welcome@offertraker.com";
                                model.Subject = "Your Digital Card is here !";

                                model.Body = "Dear " + param.mobileno + ", <br /><br />" + master.MerchantName + " is happy to share its digital card with you. The digital card carries exclusive coupons and offers for you. You can also place orders through the digital card. You can share the digital card with your friends and help them enjoy exclusive offers from our business. <br />Don't forget to add your picture in your profile once you have downloaded the app! It shows up on the digital card, so that you may flaunt it!<br /> The Offertraker app is a great way to keep digital cards from various merchants in one place, so that all your offers ,coupons or points can be easily redeemed! Best of all, you can also place orders directly from the digital card.  <br />Please download the Offertraker app for Android here :goo.gl/r5rxjj and for Apple here :goo.gl/dAq4er.Your login is your email-id and your password is 123456. Enjoy the offers and stay connected! <br /><br />" + master.MerchantName + ".";

                                SendEmail email = new SendEmail();
                                string result = email.SendEmailToConsumer(model);
                                consumersmsdetail smsdetails = new consumersmsdetail();
                                smsdetails.ConsumerId = isUser.Id;
                                smsdetails.MerchantId = userid;
                                smsdetails.SMSEmailStatus = result;
                                smsdetails.UserName = param.mobileno;
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
                    }
                }
                else
                {
                    return "Mobile number not specified";
                }
                return Global.Merchant.ConsumerAddedSuccessfully;
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
    }

}