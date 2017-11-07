using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using InstaDelight.Models;
using Microsoft.AspNet.Identity;
using AspNet.Identity.MySQL;
using System.Data;
using System.IO;
using MySql.Data.MySqlClient;

namespace InstaDelight.Controllers
{
    public class BankController : Controller
    {

        ApplicationDbContext Consumercontext = new ApplicationDbContext("DefaultConsumerConnection");

        // GET: Bank
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

        public ActionResult BankConfiguration()
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

        public ActionResult AddBank(string Flag, int bankid)
        {
            if (Request.IsAuthenticated)
            {
                if (Session["AdminUserId"] != null)
                {
                    ViewBag.Flag = Flag;
                    ViewBag.BankId = bankid;
                    return View("AddBank");
                }
                else
                    return RedirectToAction("Login", "Account");

            }
            else
            {
                return RedirectToAction("Login", "Account");
            }

        }

        public ActionResult DownloadConsumerSample()
        {
            if (Request.IsAuthenticated)
            {
                if (Session["AdminUserId"] != null)
                {
                    using (instadelightEntities dataContext = new instadelightEntities())
                    {
                        menu_csv_template template = dataContext.menu_csv_template.Where(x => x.templatefor == "Consumer").FirstOrDefault();
                        if (template != null)
                        {
                            downloadCSV(template);
                            ViewBag.Result = "Consumer csv template downloaded successfully.";
                            return RedirectToAction("DownloadConsumerSample", "Bank");
                        }
                        else
                        {
                            return RedirectToAction("Login", "Account");
                        }
                    }
                }
                else
                    return RedirectToAction("Login", "Account");

            }
            else
            {
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


        public ActionResult UploadConsumers(int BankId)
        {

            ViewBag.BankId = BankId;
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

        public ActionResult UploadConsumerFile(string BankId)
        {
            ViewBag.BankId = BankId;
            if (Request.IsAuthenticated)
            {
                if (Session["AdminUserId"] != null)
                {
                    //ViewBag.result = "";
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


        [HttpPost]
        public ActionResult UploadConsumerFile(HttpPostedFileBase file, string hdnBankId)
        {
            // Verify that the user selected a file

            if (file != null && file.ContentLength > 0)
            {
                DataTable dt = new DataTable();
                using (StreamReader sr = new StreamReader(file.InputStream))
                {
                    string[] headers = sr.ReadLine().Split(',');
                    foreach (string header in headers)
                    {
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
                using (instadelightEntities dataContext = new instadelightEntities())
                {

                    if (Request.IsAuthenticated)
                    {
                        if (Session["AdminUserName"] != null)
                        {
                            if (dt != null)
                            {
                                if (dt.Rows.Count > 0)
                                {
                                    string userid = Session["AdminUserName"].ToString();
                                    for (int i = 0; i < dt.Rows.Count; i++)
                                    {
                                        string result = AddNewBankConsumer(dt.Rows[i][0].ToString(), hdnBankId);
                                    }
                                    ViewBag.result = "Bank DEC shared successfully.";

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
                            var bankList = dataContext.bank_master.ToList();
                            var jsonResult = Json(bankList, JsonRequestBehavior.AllowGet);
                            jsonResult.MaxJsonLength = Int32.MaxValue;

                            return jsonResult;
                        }
                    }
                    catch (Exception ex)
                    {
                        //call error view here
                        //log the error
                        EventLog.LogErrorData("Error occured Bank/GetAll." + ex.Message, true);
                        return Json("Error occured while retrieving all banks", JsonRequestBehavior.AllowGet);
                    }
                }
                else
                    return Json("Unauthorized access", JsonRequestBehavior.AllowGet);
            }
            else
                return Json("Unauthorized access", JsonRequestBehavior.AllowGet);
        }



        public JsonResult GetBankById(string BankId)
        {
            if (Request.IsAuthenticated)
            {
                if (Session["AdminUserId"] != null)
                {
                    try
                    {
                        using (instadelightEntities dataContext = new instadelightEntities())
                        {
                            int no = Convert.ToInt32(BankId);
                            var bankList = dataContext.bank_master.Find(no);
                            return Json(bankList, JsonRequestBehavior.AllowGet);
                        }
                    }
                    catch (Exception ex)
                    {
                        //call error view here
                        //log the error
                        EventLog.LogErrorData("Error occured Bank/GetBankById." + ex.Message, true);
                        return Json("Error occured while retrieving bank details by id", JsonRequestBehavior.AllowGet);
                    }
                }
                else
                    return Json("Unauthorized access", JsonRequestBehavior.AllowGet);
            }
            else
                return Json("Unauthorized access", JsonRequestBehavior.AllowGet);
        }

        public JsonResult getBankDEC(string bankid)
        {
            if (Request.IsAuthenticated)
            {
                if (Session["AdminUserId"] != null)
                {
                    try
                    {
                        using (instadelightEntities dataContext = new instadelightEntities())
                        {
                            int no = Convert.ToInt32(bankid);
                            var bankdec = dataContext.bank_dec_details.Where(x => x.bankid == no).FirstOrDefault();
                            var jsonResult = Json(bankdec, JsonRequestBehavior.AllowGet);
                            jsonResult.MaxJsonLength = Int32.MaxValue;

                            return jsonResult;
                        }
                    }
                    catch (Exception ex)
                    {
                        //call error view here
                        //log the error
                        EventLog.LogErrorData("Error occured Bank/getBankDEC." + ex.Message, true);
                        return Json("Error occured while retrieving bank dec by id", JsonRequestBehavior.AllowGet);
                    }
                }
                else
                    return Json("Unauthorized access", JsonRequestBehavior.AllowGet);
            }
            else
                return Json("Unauthorized access", JsonRequestBehavior.AllowGet);
        }


        public string AddNewBank(bank_master bnk)
        {
            if (Request.IsAuthenticated)
            {
                if (Session["AdminUserId"] != null)
                {
                    try
                    {
                        if (bnk != null)
                        {
                            using (instadelightEntities dataContext = new instadelightEntities())
                            {
                                bnk.CreateDate = DateTime.Now;
                                dataContext.bank_master.Add(bnk);
                                dataContext.SaveChanges();

                                if (bnk.bank_dec_details != null)
                                {
                                    if (bnk.bank_dec_details.decname != "" && bnk.bank_dec_details.decimage != null)
                                    {
                                        bnk.bank_dec_details.bankid = bnk.bankid;

                                        dataContext.bank_dec_details.Add(bnk.bank_dec_details);
                                        dataContext.SaveChanges();
                                    }
                                }


                                return "Bank Added Successfully";
                            }
                        }
                        else
                        {
                            return "Invalid Bank Details";
                        }
                    }
                    catch (Exception ex)
                    {
                        //call error view here
                        //log the error
                        EventLog.LogErrorData("Error occured Bank/AddNewBank." + ex.Message, true);
                        return "Error occured while adding new bank.";
                    }
                }
                else
                    return "Unauthorized access";
            }
            else
                return "Unauthorized access";
        }

        public string UpdateBank(bank_master bnk)
        {
            if (Request.IsAuthenticated)
            {
                if (Session["AdminUserId"] != null)
                {
                    try
                    {
                        if (bnk != null)
                        {
                            using (instadelightEntities dataContext = new instadelightEntities())
                            {
                                int no = Convert.ToInt32(bnk.bankid);
                                var bank = dataContext.bank_master.Where(x => x.bankid == no).FirstOrDefault();
                                bank.bankname = bnk.bankname;
                                bank.bank_logo = bnk.bank_logo;
                                bank.button1_text = bnk.button1_text;
                                bank.button1_url = bnk.button1_url;
                                bank.button2_text = bnk.button2_text;
                                bank.button2_url = bnk.button2_url;
                                bank.button3_text = bnk.button3_text;
                                bank.button3_url = bnk.button3_url;
                                bank.button4_text = bnk.button4_text;
                                bank.button4_url = bnk.button4_url;
                                dataContext.SaveChanges();

                                bank_dec_details decs = dataContext.bank_dec_details.Where(x => x.bankid == bank.bankid).FirstOrDefault();
                                if (decs != null)
                                {
                                    dataContext.bank_dec_details.Remove(decs);
                                    dataContext.SaveChanges();
                                }

                                //Adding dec
                                bank_dec_details dec = new bank_dec_details();
                                dec.bankid = bnk.bankid;
                                dec.decname = bnk.bank_dec_details.decname;
                                dec.decimage = bnk.bank_dec_details.decimage;
                                dataContext.bank_dec_details.Add(dec);
                                dataContext.SaveChanges();

                                return "Bank Updated";
                            }
                        }
                        else
                        {
                            return "Invalid Bank";
                        }
                    }
                    catch (Exception ex)
                    {
                        //call error view here
                        //log the error
                        EventLog.LogErrorData("Error occured Bank/UpdateBank." + ex.Message, true);
                        return "Error occured while updating bank.";
                    }
                }
                else
                    return "Unauthorized access";
            }
            else
                return "Unauthorized access";
        }

        //PA-13-07-2017
        public ActionResult LinkBankConsumer(string Id)
        {
            TempData["Id"] = Id;
            if (Request.IsAuthenticated)
            {
                if (Session["AdminUserId"] != null)
                {
                    ViewBag.BankId = Id;
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

        //PA-13-07-2017
        //Mapping Consumer & bank
        public string AddNewBankConsumer(string mobileno, string BankId)
        {
            if (Request.IsAuthenticated)
            {
                if (Session["AdminUserId"] != null)
                {
                    try
                    {
                        SMSUtility sms = new SMSUtility();
                        if (mobileno != null)
                        {
                            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(Consumercontext));
                            var UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(Consumercontext));

                            instadelightEntities dataContext = new instadelightEntities();
                            int bankno=Convert.ToInt32(BankId);
                            using (instadelight_consumerEntities ConsumerdataContext = new instadelight_consumerEntities())
                            {
                                var isUser = ConsumerdataContext.ConsumerUsers.Where(u => u.UserName == mobileno).FirstOrDefault();
                                var bankdetails=dataContext.bank_master.Where(x=>x.bankid==bankno).FirstOrDefault();

                                if (isUser != null)
                                {
                                    //Check if Mapping Exist else insert Mapping of current merchant.
                                    using (instadelightEntities bankcontext = new instadelightEntities())
                                    {
                                        var isConsumerMapped = bankcontext.bankconsumerdetails.Where(u => u.ConsumerId == isUser.Id && u.BankId == BankId).FirstOrDefault();
                                        if (isConsumerMapped == null)
                                        {   //Mapping Bank And Consumer
                                            bankconsumerdetail mappingcosumerContext = new bankconsumerdetail();
                                            mappingcosumerContext.ConsumerId = isUser.Id;
                                            mappingcosumerContext.ConsumerPhone = mobileno;
                                            mappingcosumerContext.BankId = BankId;
                                            bankcontext.bankconsumerdetails.Add(mappingcosumerContext);
                                            bankcontext.SaveChanges();

                                            //Send SMS to consumer for a new DEC. 'Navigation://OpenQRCodeScanner'
                                            if (mobileno.Contains('@') == false)
                                            {
                                                string smsresult = sms.sendMessage(mobileno, "Dear Customer, we are pleased to launch our Digital Card. You can avail exciting offers from thousands of Businesses and also use the bank's services from the card. " + bankdetails.bankname);
                                            }
                                            else
                                            {
                                                EmailModel model = new EmailModel();
                                                model.To = mobileno;
                                                model.Email = "welcome@offertraker.com";
                                                model.Subject = "Welcome to our Digital Card";
                                                model.Body = "Dear Customer, <br /><br />" + bankdetails.bankname + " is pleased to launch our own digital card. Our digital card is a great initiative to make it easy for you to avail offers and coupons from thousands of merchants and also stay connected with the bank right from your mobile phone. <br />Please download the Offertraker app for Android here :goo.gl/r5rxjj and for Apple here :goo.gl/dAq4er. Your login is your email-id and your password is 123456.<br /><br />Best wishes, <br /><br />" + bankdetails.bankname;
                                                
                                                SendEmail email = new SendEmail();
                                                email.SendEmailToConsumer(model);                                                
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
                                        //Added new code block for consumermaster
                                        using (instadelight_consumerEntities consumerContext = new instadelight_consumerEntities())
                                        {
                                            InsertNewConsumer(user.Id, mobileno);

                                            using (instadelightEntities bankcontext = new instadelightEntities())
                                            {
                                                var isConsumerMapped = bankcontext.bankconsumerdetails.Where(u => u.ConsumerId == mobileno && u.BankId == BankId).FirstOrDefault();
                                                if (isConsumerMapped == null)
                                                {   //Mapping Bank And Consumer
                                                    bankconsumerdetail mappingcosumerContext = new bankconsumerdetail();
                                                    mappingcosumerContext.ConsumerId = user.Id;
                                                    mappingcosumerContext.ConsumerPhone = mobileno;
                                                    mappingcosumerContext.BankId = BankId;
                                                    bankcontext.bankconsumerdetails.Add(mappingcosumerContext);
                                                    bankcontext.SaveChanges();
                                                }
                                            }

                                            if (mobileno.Contains('@') == false)
                                            {
                                                string smsresult = sms.sendMessage(mobileno, Session["AdminUserName"] + " has shared offers with you.Download the app for Android at goo.gl/r5rxjj, Use your Cell no. as login and 123456 as password and enjoy the benefits!");
                                            }
                                            else
                                            {
                                                EmailModel model = new EmailModel();
                                                model.To = mobileno;
                                                model.Email = "welcome@offertraker.com";
                                                model.Subject = "Welcome to our Digital Card";
                                                model.Body = "Dear Customer, <br /><br />" + bankdetails.bankname + " is pleased to launch our own digital card. Our digital card is a great initiative to make it easy for you to avail offers and coupons from thousands of merchants and also stay connected with the bank right from your mobile phone. <br />Please download the Offertraker app for Android here :goo.gl/r5rxjj and for Apple here :goo.gl/dAq4er. <br />Best wishes,<br /> <br />" + bankdetails.bankname;
                                                SendEmail email = new SendEmail();
                                                email.SendEmailToConsumer(model);                                                
                                            }
                                        }
                                    }
                                    else
                                    {
                                        return "Not able to register Consumer";
                                    }                                  
                                }
                                return "Consumer Mapped Successfully to Bank.";
                            }


                        }
                        else
                        {
                            return "Enter Mobile Number";
                        }
                    }
                    catch (Exception ex)
                    {
                        //call error view here
                        //log the error
                        EventLog.LogErrorData("Error occured Bank/AddNewBankConsumer." + ex.Message, true);
                        return "Error occured while adding consumer for bank";
                    }
                }
                else
                    return "Unauthorized access";
            }
            else
                return "Unauthorized access";
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

    }
}