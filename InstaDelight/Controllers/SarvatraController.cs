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
using MySql.Data.MySqlClient;

namespace InstaDelight.Controllers
{
    public class SarvatraController : Controller
    {
        ApplicationDbContext context = new ApplicationDbContext("DefaultConnection");
        // GET: Sarvatra
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

        public ActionResult IndexUsers()
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

        public ActionResult CustomerSupport()
        {
            if (Request.IsAuthenticated)
            {
                if (Session["AdminUserId"] != null)
                {
                    ViewBag.FromDate = DateTime.Now.AddMonths(-1).ToString("yyyy-MM-dd");
                    ViewBag.ToDate = DateTime.Now.ToString("yyyy-MM-dd");

                    ViewBag.Images = Directory.EnumerateFiles(Server.MapPath("~/Images/BackgroundImages")).Select(fn => "~/Images/BackgroundImages/" + Path.GetFileName(fn));
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

        public ActionResult GiftCardSettings()
        {
            if (Request.IsAuthenticated)
            {
                if (Session["AdminUserId"] != null)
                {
                    ViewBag.FromDate = DateTime.Now.AddMonths(-1).ToString("yyyy-MM-dd");
                    ViewBag.ToDate = DateTime.Now.ToString("yyyy-MM-dd");
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


        public ActionResult BankSettings()
        {
            if (Request.IsAuthenticated)
            {
                if (Session["AdminUserId"] != null)
                {
                    ViewBag.FromDate = DateTime.Now.AddMonths(-1).ToString("yyyy-MM-dd");
                    ViewBag.ToDate = DateTime.Now.ToString("yyyy-MM-dd");
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

        public ActionResult CreateVAR()
        {
            if (Request.IsAuthenticated)
            {
                if (Session["AdminUserId"] != null)
                {
                    ViewBag.Id = "";
                    string userid = Session["AdminUserId"].ToString();
                    using (instadelightEntities dataContext = new instadelightEntities())
                    {
                        user master = dataContext.users.Where(x => x.Id == userid).FirstOrDefault();
                        if (master.VARCode == "INSTADELIGHT")
                        {
                            ViewBag.countryid = 1;
                        }
                        else if (master.VARCode.Contains("IND"))
                        {
                            ViewBag.countryid = 1;
                        }
                        else if (master.VARCode.Contains("UAE"))
                        {
                            ViewBag.countryid = 3;
                        }
                        else if (master.VARCode.Contains("USA"))
                        {
                            ViewBag.countryid = 2;
                        }

                        else if (master.VARCode.Contains("UK"))
                        {
                            ViewBag.countryid = 4;
                        }
                        else
                        {
                            ViewBag.countryid = 1;
                        }
                    }

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

        public ActionResult CreateUser()
        {
            if (Request.IsAuthenticated)
            {
                if (Session["AdminUserId"] != null)
                {
                    ViewBag.Id = "";
                    string userid = Session["AdminUserId"].ToString();
                    using (instadelightEntities dataContext = new instadelightEntities())
                    {
                        user master = dataContext.users.Where(x => x.Id == userid).FirstOrDefault();
                        if (master.VARCode == "INSTADELIGHT")
                        {
                            ViewBag.countryid = 1;
                        }
                        else if (master.VARCode.Contains("IND"))
                        {
                            ViewBag.countryid = 1;
                        }
                        else if (master.VARCode.Contains("UAE"))
                        {
                            ViewBag.countryid = 3;
                        }
                        else if (master.VARCode.Contains("USA"))
                        {
                            ViewBag.countryid = 2;
                        }

                        else if (master.VARCode.Contains("UK"))
                        {
                            ViewBag.countryid = 4;
                        }
                        else
                        {
                            ViewBag.countryid = 1;
                        }
                    }

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

        public void EditMerchant(string merchantid)
        {
            using (instadelightEntities dataContext = new instadelightEntities())
            {
                if (Request.IsAuthenticated)
                {
                    if (Session["AdminUserId"] != null)
                    {

                        Response.Redirect("http://localhost:49989/Account/Login/");
                    }
                }
            }
        }

        public JsonResult getVARCodes()
        {
            try
            {
                using (instadelightEntities dataContext = new instadelightEntities())
                {
                    List<varcode> varList = dataContext.varcodes.ToList();
                    return Json(varList, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                //call error view here
                //log the error
                EventLog.LogErrorData("Error occurred Sarvatra/getVARCodes." + ex.Message, true);
                return Json("Error occurred while retrieving VAR code list", JsonRequestBehavior.AllowGet);
            }
        }

        public string AddNewVAR(merchant_master mch)
        {
            try
            {
                if (mch != null)
                {
                    using (instadelightEntities dataContext = new instadelightEntities())
                    {
                        varcode existingvar = dataContext.varcodes.Where(x => x.VARCode1.ToUpper() == mch.VARCode.ToUpper()).FirstOrDefault();
                        if (existingvar == null)
                        {
                            varcode v1 = new varcode();
                            v1.VARCode1 = mch.VARCode.ToUpper();
                            dataContext.varcodes.Add(v1);
                            dataContext.SaveChanges();
                        }
                        else
                        {
                            EventLog.LogErrorData("VARCode " + mch.VARCode.ToUpper() + " already exist.", true);
                            return "VARCode " + mch.VARCode.ToUpper() + " already exist. Please enter other code and try again.";
                        }


                        var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));
                        var UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));
                        UserManager.UserValidator = new UserValidator<ApplicationUser>(UserManager)
                        {
                            AllowOnlyAlphanumericUserNames = false,
                            RequireUniqueEmail = true
                        };

                        var user = new ApplicationUser();
                        if (mch.PrimeryIDLogin == "cell")
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
                            string userid = Session["AdminUserId"].ToString();
                            user loggedinuser = dataContext.users.Where(x => x.Id == userid).FirstOrDefault();

                            var rolesForUser = UserManager.GetRoles(user.Id);
                            if (!rolesForUser.Contains("SupportAdmin"))
                            {
                                UserManager.AddToRole(user.Id, "SupportAdmin");
                            }
                            user currentuser = dataContext.users.Where(x => x.Id == user.Id).FirstOrDefault();
                            if (currentuser != null)
                            {
                                currentuser.FirstName = mch.MerchantName;
                                currentuser.OwnerName = mch.DECName;
                                currentuser.LastName = "";
                                currentuser.VARCode = mch.VARCode.ToUpper();
                                currentuser.CreatedBy = loggedinuser.VARCode;

                                dataContext.SaveChanges();

                                //Also insert record in admin table which allows login to sales portal designed by pravin
                                admin adminuser = new admin();
                                adminuser.user_name = currentuser.FirstName;
                                adminuser.last_name = currentuser.LastName;
                                adminuser.user_type = "1"; //admin user
                                adminuser.user_email = currentuser.Email;
                                adminuser.rm_email = currentuser.Email;
                                adminuser.rname = currentuser.FirstName;
                                adminuser.city = "";
                                adminuser.country = "";
                                adminuser.state = "";
                                adminuser.zip = "";
                                adminuser.password = "123456";
                                adminuser.aproval = "1";
                                adminuser.creation_date = DateTime.Now;
                                adminuser.user_contact = currentuser.UserName;
                                dataContext.admins.Add(adminuser);
                                dataContext.SaveChanges();
                            }


                        }
                        else
                        {
                            EventLog.LogErrorData("Error occurred while creating new VAR." + chkUser.Errors.FirstOrDefault(), true);
                            return "Error occurred while creating new VAR";
                        }
                    }
                }
                return "New VAR created successfully.";
            }

            catch (Exception ex)
            {
                //call error view here
                //log the error
                EventLog.LogErrorData("Error occured Support/AddNewVAR." + ex.Message, true);
                return "Error occurred while creating new VAR";
            }
        }

        public string AddNewVARStaff(merchant_master mch)
        {
            try
            {
                if (mch != null)
                {
                    using (instadelightEntities dataContext = new instadelightEntities())
                    {

                        var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));
                        var UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));
                        UserManager.UserValidator = new UserValidator<ApplicationUser>(UserManager)
                        {
                            AllowOnlyAlphanumericUserNames = false,
                            RequireUniqueEmail = true
                        };

                        var user = new ApplicationUser();
                        if (mch.PrimeryIDLogin == "cell")
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
                            string userid = Session["AdminUserId"].ToString();
                            user loggedinuser = dataContext.users.Where(x => x.Id == userid).FirstOrDefault();

                            var rolesForUser = UserManager.GetRoles(user.Id);
                            if (!rolesForUser.Contains("SupportUsers"))
                            {
                                UserManager.AddToRole(user.Id, "SupportUsers");
                            }
                            user currentuser = dataContext.users.Where(x => x.Id == user.Id).FirstOrDefault();
                            if (currentuser != null)
                            {
                                currentuser.FirstName = mch.MerchantName;
                                currentuser.LastName = "";
                                currentuser.OwnerName = loggedinuser.OwnerName;
                                currentuser.VARCode = loggedinuser.VARCode;
                                currentuser.CreatedBy = loggedinuser.VARCode;
                                dataContext.SaveChanges();

                                //Also insert record in admin table which allows login to sales portal designed by pravin
                                admin adminuser = new admin();
                                adminuser.user_name = currentuser.FirstName;
                                adminuser.last_name = currentuser.LastName;
                                adminuser.user_type = "0"; //user
                                adminuser.user_email = currentuser.Email;
                                adminuser.rm_email = currentuser.Email;
                                adminuser.rname = currentuser.FirstName;
                                adminuser.city = "";
                                adminuser.country = "";
                                adminuser.state = "";
                                adminuser.zip = "";
                                adminuser.password = "123456";
                                adminuser.aproval = "1";
                                adminuser.creation_date = DateTime.Now;
                                adminuser.user_contact = currentuser.UserName;
                                dataContext.admins.Add(adminuser);
                                dataContext.SaveChanges();
                            }
                        }
                        else
                        {
                            EventLog.LogErrorData("Error occurred while creating new User." + chkUser.Errors.FirstOrDefault(), true);
                            return "Error occurred while creating new User";
                        }
                    }
                }
                return "New User created successfully.";
            }

            catch (Exception ex)
            {
                //call error view here
                //log the error
                EventLog.LogErrorData("Error occured Support/AddNewVARStaff." + ex.Message, true);
                return "Error occurred while creating new User";
            }
        }



        public string EditVAR(merchant_master mch)
        {
            try
            {
                if (mch != null)
                {
                    using (instadelightEntities dataContext = new instadelightEntities())
                    {

                        user currentuser = dataContext.users.Where(x => x.Id == mch.UserId).FirstOrDefault();
                        if (currentuser != null)
                        {
                            currentuser.FirstName = mch.MerchantName;
                            currentuser.LastName = "";
                            currentuser.OwnerName = mch.DECName;
                            currentuser.PhoneNumber = mch.PhoneNumber;
                            currentuser.Email = mch.Email;


                            dataContext.SaveChanges();
                        }
                        else
                        {
                            EventLog.LogErrorData("Error occurred while editing VAR. User does not exist", true);
                            return "Error occurred while editing VAR";
                        }
                    }
                }
                else
                {
                    EventLog.LogErrorData("Error occurred while editing VAR. Please enter all details.", true);
                    return "Error occurred while editing VAR";
                }
                return "VAR edited successfully.";
            }

            catch (Exception ex)
            {
                //call error view here
                //log the error
                EventLog.LogErrorData("Error occured Support/EditVAP." + ex.Message, true);
                return "Error occurred while updating existing VAR";
            }
        }

        public string EditVARUser(merchant_master mch)
        {
            try
            {
                if (mch != null)
                {
                    using (instadelightEntities dataContext = new instadelightEntities())
                    {

                        user currentuser = dataContext.users.Where(x => x.Id == mch.UserId).FirstOrDefault();
                        if (currentuser != null)
                        {
                            currentuser.FirstName = mch.MerchantName;
                            currentuser.LastName = "";
                            currentuser.OwnerName = mch.DECName;
                            currentuser.PhoneNumber = mch.PhoneNumber;
                            currentuser.Email = mch.Email;


                            dataContext.SaveChanges();
                        }
                        else
                        {
                            EventLog.LogErrorData("Error occurred while editing User. User does not exist", true);
                            return "Error occurred while editing VAR";
                        }
                    }
                }
                else
                {
                    EventLog.LogErrorData("Error occurred while editing User. Please enter all details.", true);
                    return "Error occurred while editing VAR";
                }
                return "User edited successfully.";
            }

            catch (Exception ex)
            {
                //call error view here
                //log the error
                EventLog.LogErrorData("Error occured Support/EditVARUser." + ex.Message, true);
                return "Error occurred while updating existing User";
            }
        }

        public JsonResult GetVARUsers()
        {
            try
            {
                using (instadelightEntities dataContext = new instadelightEntities())
                {
                    string varcode = "";

                    if (User.IsInRole("SupportAdmin"))
                    {
                        string userid = Session["AdminUserId"].ToString();
                        user loggedinuser = dataContext.users.Where(x => x.Id == userid).FirstOrDefault();
                        varcode = loggedinuser.VARCode;
                    }

                    MySqlParameter param1 = new MySqlParameter();
                    param1.Value = varcode;
                    param1.Direction = System.Data.ParameterDirection.Input;
                    param1.ParameterName = "@VARCodeName";
                    param1.DbType = System.Data.DbType.String;

                    MySqlParameter param2 = new MySqlParameter();
                    param2.Value = "SupportAdmin";
                    param2.Direction = System.Data.ParameterDirection.Input;
                    param2.ParameterName = "@RoleName";
                    param2.DbType = System.Data.DbType.String;

                    List<user> VARList = dataContext.Database.SqlQuery<user>("CALL getSupportMembers(@VARCodeName,@RoleName)", param1, param2).ToList();
                    var jsonResult = Json(VARList, JsonRequestBehavior.AllowGet);
                    jsonResult.ContentType = "application/json";
                    jsonResult.MaxJsonLength = Int32.MaxValue;

                    return jsonResult;
                }
            }
            catch (Exception ex)
            {
                //call error view here
                //log the error
                EventLog.LogErrorData("Error occurred Sarvatra/GetVARUsers." + ex.Message, true);
                return Json("Error occurred while retrieving support staff", JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult GetSupportUsers()
        {
            try
            {
                using (instadelightEntities dataContext = new instadelightEntities())
                {
                    string varcode = "";


                    if (User.IsInRole("SupportAdmin"))
                    {
                        string userid = Session["AdminUserId"].ToString();
                        user loggedinuser = dataContext.users.Where(x => x.Id == userid).FirstOrDefault();
                        varcode = loggedinuser.VARCode;
                    }

                    List<user> VARList = new List<user>();
                    if (varcode == "")
                    {
                        VARList = GetVARUserList(varcode);
                    }
                    else
                    {
                        VARList.AddRange(GetVARUserList(varcode));

                        var innerJoinQuery =
    from u in dataContext.users
    join r in dataContext.userroles on u.Id equals r.UserId
    join rols in dataContext.roles on r.RoleId equals rols.Id
    where rols.Name == "SupportAdmin" && u.CreatedBy == varcode
    select new { VARCode = u.VARCode };

                        if (innerJoinQuery != null)
                        {
                            if (innerJoinQuery.Count() > 0)
                            {
                                foreach (var s in innerJoinQuery)
                                {
                                    VARList.AddRange(GetVARUserList(s.VARCode));
                                }
                            }
                        }
                    }


                    var jsonResult = Json(VARList, JsonRequestBehavior.AllowGet);
                    jsonResult.ContentType = "application/json";
                    jsonResult.MaxJsonLength = Int32.MaxValue;

                    return jsonResult;
                }
            }
            catch (Exception ex)
            {
                //call error view here
                //log the error
                EventLog.LogErrorData("Error occurred Sarvatra/GetVARUsers." + ex.Message, true);
                return Json("Error occurred while retrieving support staff", JsonRequestBehavior.AllowGet);
            }
        }

        public List<user> GetVARUserList(string VARCode)
        {
            using (instadelightEntities dataContext = new instadelightEntities())
            {
                MySqlParameter param1 = new MySqlParameter();
                param1.Value = VARCode;
                param1.Direction = System.Data.ParameterDirection.Input;
                param1.ParameterName = "@VARCodeName";
                param1.DbType = System.Data.DbType.String;

                MySqlParameter param2 = new MySqlParameter();
                param2.Value = "SupportUsers";
                param2.Direction = System.Data.ParameterDirection.Input;
                param2.ParameterName = "@RoleName";
                param2.DbType = System.Data.DbType.String;

                List<user> VARList = dataContext.Database.SqlQuery<user>("CALL getSupportMembers(@VARCodeName,@RoleName)", param1, param2).ToList();
                return VARList;
            }
        }

        public string ChangeVARStatus(string userid, string action)
        {
            try
            {
                if (string.IsNullOrEmpty(userid) == false && string.IsNullOrEmpty(action) == false)
                {
                    using (instadelightEntities dataContext = new instadelightEntities())
                    {
                        user currentuser = dataContext.users.Where(x => x.Id == userid).FirstOrDefault();
                        if (currentuser != null)
                        {
                            if (action == "Activate")
                            {
                                currentuser.IsActive = 1;
                            }
                            else if (action == "Deactivate")
                            {
                                currentuser.IsActive = 0;
                            }


                            dataContext.SaveChanges();
                        }
                        else
                        {
                            EventLog.LogErrorData("Error occurred while changing VAR status. User does not exist", true);
                            return "Error occurred while changing VAR status";
                        }
                    }
                }
                else
                {
                    EventLog.LogErrorData("Error occurred while changing VAR status. Please enter all details.", true);
                    return "Error occurred while changing VAR status";
                }
                if (action == "Activate")
                {
                    return "VAR activated successfully.";
                }
                else
                {
                    return "VAR suspended successfully.";
                }
            }

            catch (Exception ex)
            {
                //call error view here
                //log the error
                EventLog.LogErrorData("Error occured Support/EditVAP." + ex.Message, true);
                return "Error occurred while updating existing VAR";
            }
        }

    }
}