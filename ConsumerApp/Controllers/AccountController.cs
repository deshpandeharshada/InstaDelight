using System;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using ConsumerApp.Models;
using System.Collections.Generic;
using System.Web.UI;

namespace ConsumerApp.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;

        public AccountController()
        {
        }

        public AccountController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set
            {
                _signInManager = value;
            }
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        // The Authorize Action is the end point which gets called when you access any
        // protected Web API. If the user is not logged in then they will be redirected to 
        // the Login page. After a successful login you can call a Web API.
        [HttpGet]
        public ActionResult Authorize()
        {
            var claims = new ClaimsPrincipal(User).Claims.ToArray();
            var identity = new ClaimsIdentity(claims, "Bearer");
            AuthenticationManager.SignIn(identity);
            return new EmptyResult();
        }

        //
        // GET: /Account/Login
        [AllowAnonymous]
        [OutputCache(Duration = 3600, VaryByParam = "none", Location = OutputCacheLocation.Client)]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;


            ViewBag.CountryList = GetCountryList();
            ViewBag.DefaultCountryCode = "+91";
            return View();
        }

        private List<country_master> GetCountryList()
        {
            instadelight_consumerEntities datacontext = new instadelight_consumerEntities();
            /*Getting data from database*/
            List<country_master> objcountrylist = (from data in datacontext.country_master
                                                   select data).ToList();

            return objcountrylist;
        }


        //
        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid)
            {
                instadelight_consumerEntities datacontext = new instadelight_consumerEntities();
                /*Getting data from database*/
                ViewBag.CountryList = GetCountryList();

                ViewBag.DefaultCountryCode = "+91";
                List<string> errmsgs = new List<string>();

                foreach (var val in ModelState.Values)
                {
                    if (val.Errors != null)
                    {
                        if (val.Errors.Count > 0)
                        {
                            for (int i = 0; i < val.Errors.Count; i++)
                            {
                                errmsgs.Add(val.Errors[i].ErrorMessage);
                            }
                        }
                    }
                }

                if (errmsgs.Count > 0)
                {
                    for (int i = 0; i < errmsgs.Count; i++)
                    {
                        ModelState.AddModelError("", errmsgs[i]);
                    }
                }

                return View(model);
            }

            // This doesn't count login failures towards account lockout
            // To enable password failures to trigger account lockout, change to shouldLockout: true
            //var result = await SignInManager.PasswordSignInAsync(model.UserName, model.Password, model.RememberMe, shouldLockout: false);
            //Include conuntry code

            if (string.IsNullOrEmpty(model.Email) == false && string.IsNullOrEmpty(model.MobileNo) == false)
            {
                instadelight_consumerEntities datacontext = new instadelight_consumerEntities();
                /*Getting data from database*/
                ViewBag.CountryList = GetCountryList();
                ViewBag.DefaultCountryCode = "+91";
                ModelState.AddModelError("", "Please enter either EMail or Phone Number to login");
                return View(model);
            }
            else if (string.IsNullOrEmpty(model.Email) == false)
            {
                model.UserName = model.Email;
            }
            else if (string.IsNullOrEmpty(model.MobileNo) == false)
            {
                model.UserName = model.CountryCode + " " + model.MobileNo;
            }
            else
            {
                instadelight_consumerEntities datacontext = new instadelight_consumerEntities();
                /*Getting data from database*/
                ViewBag.CountryList = GetCountryList();
                ViewBag.DefaultCountryCode = "+91";
                ModelState.AddModelError("", "Please enter either EMail or Phone Number to login");


                return View(model);
            }

            var result = await SignInManager.PasswordSignInAsync(model.UserName, model.Password, model.RememberMe, shouldLockout: false);
            switch (result)
            {
                case SignInStatus.Success:
                    {
                        var userId = SignInManager.AuthenticationManager.AuthenticationResponseGrant.Identity.GetUserId();
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
                                Session["UserName"] = model.UserName;
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
                        return View("SplashScreen");//RedirectToLocal(returnUrl);
                    }
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = model.RememberMe });
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Please enter the correct username or password.");
                    ViewBag.CountryList = GetCountryList();
                    ViewBag.DefaultCountryCode = "+91";

                    return View(model);
            }
        }

        public string ValidateUser(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return "Error:Invalid User Details";
            }

            // This doesn't count login failures towards account lockout
            // To enable password failures to trigger account lockout, change to shouldLockout: true
            var result = SignInManager.PasswordSignIn(model.UserName, model.Password, model.RememberMe, shouldLockout: false);

            switch (result)
            {
                case SignInStatus.Success:
                    var userId = SignInManager.AuthenticationManager.AuthenticationResponseGrant.Identity.GetUserId();
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
                            return "ResetPassword";
                        }
                        consumermaster consumer = dataContext.consumermasters.Where(x => x.UserId == userId).FirstOrDefault();
                    }
                    return userId;
                case SignInStatus.LockedOut:
                    return "Error:Consumer Locked Out";
                case SignInStatus.RequiresVerification:
                    return "Error:Consumer Requires Verification";
                case SignInStatus.Failure:
                default:
                    return "Error:Please enter the correct username or password.";
            }
        }
        //
        // GET: /Account/VerifyCode
        [AllowAnonymous]
        public async Task<ActionResult> VerifyCode(string provider, string returnUrl, bool rememberMe)
        {
            // Require that the user has already logged in via username/password or external login
            if (!await SignInManager.HasBeenVerifiedAsync())
            {
                return View("Error");
            }
            return View(new VerifyCodeViewModel { Provider = provider, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        //
        // POST: /Account/VerifyCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> VerifyCode(VerifyCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // The following code protects for brute force attacks against the two factor codes. 
            // If a user enters incorrect codes for a specified amount of time then the user account 
            // will be locked out for a specified amount of time. 
            // You can configure the account lockout settings in IdentityConfig
            var result = await SignInManager.TwoFactorSignInAsync(model.Provider, model.Code, isPersistent: model.RememberMe, rememberBrowser: model.RememberBrowser);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(model.ReturnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Invalid code.");
                    return View(model);
            }
        }

        //
        // GET: /Account/Register
        [AllowAnonymous]
        public ActionResult Register()
        {
            return View();
        }

        //
        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser { UserName = model.Phone, Email = model.Email, PhoneNumber = model.Phone, FirstName = model.FirstName, LastName = model.LastName };
                var result = await UserManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);

                    // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=320771
                    // Send an email with this link
                    // string code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
                    // var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
                    // await UserManager.SendEmailAsync(user.Id, "Confirm your account", "Please confirm your account by clicking <a href=\"" + callbackUrl + "\">here</a>");

                    return RedirectToAction("Index", "Home");
                }
                AddErrors(result);
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Account/ConfirmEmail
        [AllowAnonymous]
        public async Task<ActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return View("Error");
            }
            var result = await UserManager.ConfirmEmailAsync(userId, code);
            return View(result.Succeeded ? "ConfirmEmail" : "Error");
        }

        //
        // GET: /Account/ForgotPassword
        [AllowAnonymous]
        public ActionResult ForgotPassword()
        {
            ViewBag.CountryList = GetCountryList();
            ViewBag.DefaultCountryCode = "+91";

            return View();
        }

        //
        // POST: /Account/ForgotPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                string username = "";
                if (string.IsNullOrEmpty(model.Email) == false && string.IsNullOrEmpty(model.Phone) == false)
                {
                    ViewBag.CountryList = GetCountryList();
                    ViewBag.DefaultCountryCode = "+91";
                    ModelState.AddModelError("", "Please enter either EMail or Phone Number to login");
                    return View(model);
                }
                else if (string.IsNullOrEmpty(model.Email) == false)
                {
                    username = model.Email;
                }
                else if (string.IsNullOrEmpty(model.Phone) == false)
                {
                    username = model.CountryCode + " " + model.Phone;
                }
                else
                {
                    ViewBag.CountryList = GetCountryList();
                    ViewBag.DefaultCountryCode = "+91";
                    ModelState.AddModelError("", "Please enter either EMail or Phone Number");
                    return View(model);
                }

                //var user = await UserManager.FindByNameAsync(model.Phone);
                var user = await UserManager.FindByNameAsync(username);
                //if (user == null || !(await UserManager.IsPhoneNumberConfirmedAsync(user.Id)))
                if (user == null)
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    EventLog.LogErrorData("User " + username + " does not exist.", true);
                    ViewBag.CountryList = GetCountryList();
                    ViewBag.DefaultCountryCode = "+91";
                    ModelState.AddModelError("", "User " + username + " does not exist.");
                    return View(model);

                }
                string code = await UserManager.GeneratePasswordResetTokenAsync(user.Id);
                string newpwd = RandomNumber.GenerateRandomOTP(6);

                //var result = await UserManager.ResetPasswordAsync(user.Id, code, "123456");
                var result = await UserManager.ResetPasswordAsync(user.Id, code, newpwd);
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

                    if (string.IsNullOrEmpty(model.Phone) == false)
                    {
                        SMSUtility sms = new SMSUtility();
                        string smsresult = sms.sendMessage(model.Phone, "Dear Customer, your password to access Offertraker App is " + newpwd.ToString());
                        EventLog.LogData("New password " + newpwd, true);

                        if (smsresult.Contains("SMS sent successfully"))
                        {
                            return RedirectToAction("ForgotPasswordConfirmation", "Account");
                        }
                    }
                    else
                    {
                        EmailModel mailmodel = new EmailModel();
                        mailmodel.To = model.Email;
                        mailmodel.Email = "no-reply@offertraker.com";
                        mailmodel.Subject = "Your password has been reset";
                        mailmodel.Body = "Dear " + username + ", <br /><br />Your password has been reset successfully. Your new login credentials are : <br /> User Name : " + user.UserName + " <br /> Password: " + newpwd + " <br /> Enjoy the offers <br /><br />Offertraker team";
                        SendEmail email = new SendEmail();
                        string mailresult = email.SendEmailToConsumer(mailmodel);

                        if (mailresult.Contains("Email sent"))
                        {
                            return RedirectToAction("ForgotPasswordEmailConfirmation", "Account");
                        }
                        //SendSMS(param.mobileno,Session["UserName"] + " has shared offers with you. Download the app for Android at goo.gl/r5rxjj, use your mobile no. as login and 123456 as password and enjoy the benefits!")
                    }
                }
                else
                {
                    EventLog.LogErrorData(result.Errors.FirstOrDefault(), true);
                }


                // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=320771
                // Send an email with this link
                // 
                // var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);		
                // await UserManager.SendEmailAsync(user.Id, "Reset Password", "Please reset your password by clicking <a href=\"" + callbackUrl + "\">here</a>");
                // return RedirectToAction("ForgotPasswordConfirmation", "Account");
            }

            // If we got this far, something failed, redisplay form
            ViewBag.CountryList = GetCountryList();
            ViewBag.DefaultCountryCode = "+91";
            List<string> errmsgs = new List<string>();
            foreach (var val in ModelState.Values)
            {
                if (val.Errors != null)
                {
                    if (val.Errors.Count > 0)
                    {
                        for (int i = 0; i < val.Errors.Count; i++)
                        {
                            errmsgs.Add(val.Errors[i].ErrorMessage);
                        }
                    }
                }
            }

            if (errmsgs.Count > 0)
            {
                for (int i = 0; i < errmsgs.Count; i++)
                {
                    ModelState.AddModelError("", errmsgs[i]);
                }
            }


            return View(model);
        }

        //
        // GET: /Account/ForgotPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        [AllowAnonymous]
        public ActionResult ForgotPasswordEmailConfirmation()
        {
            return View();
        }

        //
        // GET: /Account/ResetPassword
        [AllowAnonymous]
        public ActionResult ResetPassword(string code, string userid)
        {
            /*Create instance of entity model*/
            instadelight_consumerEntities datacontext = new instadelight_consumerEntities();
            user curruser = datacontext.users.Where(x => x.Id == userid).FirstOrDefault();
            if (curruser != null)
            {
                ViewBag.UserName = curruser.UserName;
            }
            ///*Getting data from database*/
            //List<country_master> objcountrylist = (from data in datacontext.country_master
            //                                       select data).ToList();
            //List<SelectListItem> Country = new List<SelectListItem>();
            //foreach (var item in objcountrylist)
            //{
            //    Country.Add(new SelectListItem() { Text = item.countryname, Value = item.countryid.ToString() });
            //}

            //ViewBag.CountryList = Country;


            return code == null ? View("Error") : View();
        }

        public JsonResult getLanguage(int id)
        {
            consumerEntities datacontext = new consumerEntities();
            var countries = datacontext.language_master.Where(x => x.CountryId == id).ToList();
            List<SelectListItem> listates = new List<SelectListItem>();
            if (countries != null)
            {
                foreach (var x in countries)
                {
                    listates.Add(new SelectListItem { Text = x.Language, Value = x.LanguageiId.ToString() });

                }
            }
            return Json(new SelectList(listates, "Value", "Text", JsonRequestBehavior.AllowGet));
        }

        public JsonResult getLanguageid(int id)
        {
            consumerEntities datacontext = new consumerEntities();
            var data = datacontext.language_master.FirstOrDefault(p => p.LanguageiId == id).LanguageiId;

            return Json(data, JsonRequestBehavior.AllowGet);
        }



        //
        // POST: /Account/ResetPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                //consumerEntities datacontext = new consumerEntities();
                ///*Getting data from database*/
                //List<country_master> objcountrylist = (from data in datacontext.country_master
                //                                       select data).ToList();
                //List<SelectListItem> Country = new List<SelectListItem>();

                //foreach (var item in objcountrylist)
                //{
                //    Country.Add(new SelectListItem() { Text = item.countryname, Value = item.countryid.ToString() });
                //}


                //ViewBag.CountryList = Country;
                ViewBag.UserName = model.UserName;
                return View(model);
            }
            var user = await UserManager.FindByNameAsync(model.UserName);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                ViewBag.UserName = model.UserName;
                EventLog.LogErrorData("User " + model.UserName + " does not exist.", true);
                ModelState.AddModelError("", "User " + model.UserName + " does not exist.");
                return View(model);
            }
            var result = await UserManager.ResetPasswordAsync(user.Id, model.Code, model.Password);
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

                //Add Country and Language Id in consumemaster table
                consumermaster consumeruser = dataContext.consumermasters.Where(u => u.UserId == user.Id).FirstOrDefault();
                if (consumeruser != null)
                {
                    if (consumeruser.Country == null)
                    {
                        country_master country = dataContext.country_master.Where(x => x.countryname == "India").FirstOrDefault();

                        consumeruser.Country = country.countryid;

                        dataContext.SaveChanges();
                    }
                    if (consumeruser.Country == 0)
                    {
                        country_master country = dataContext.country_master.Where(x => x.countryname == "India").FirstOrDefault();

                        consumeruser.Country = country.countryid;

                        dataContext.SaveChanges();
                    }

                    if (consumeruser.LanguageId == null)
                    {
                        country_master country = dataContext.country_master.Where(x => x.countryname == "India").FirstOrDefault();
                        language_master language = dataContext.language_master.Where(x => x.CountryId == country.countryid && x.Language == "English").FirstOrDefault();
                        consumeruser.LanguageId = language.LanguageiId;
                        dataContext.SaveChanges();
                    }

                    if (consumeruser.LanguageId == 0)
                    {
                        country_master country = dataContext.country_master.Where(x => x.countryname == "India").FirstOrDefault();
                        language_master language = dataContext.language_master.Where(x => x.CountryId == country.countryid && x.Language == "English").FirstOrDefault();
                        consumeruser.LanguageId = language.LanguageiId;
                        dataContext.SaveChanges();
                    }

                    language_master consumerlanguage = dataContext.language_master.Where(x => x.LanguageiId == consumeruser.LanguageId).FirstOrDefault();

                    HttpCookie cookie = new HttpCookie("LanguageSelected");
                    string lang = string.Empty;

                    cookie.Value = consumerlanguage
                        .LanguageCode;
                    Response.SetCookie(cookie);

                }

                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            AddErrors(result);
            ViewBag.UserName = model.UserName;
            return View();
        }

        //
        // GET: /Account/ResetPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        //
        // POST: /Account/ExternalLogin
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            // Request a redirect to the external login provider
            return new ChallengeResult(provider, Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl }));
        }

        //
        // GET: /Account/SendCode
        [AllowAnonymous]
        public async Task<ActionResult> SendCode(string returnUrl, bool rememberMe)
        {
            var userId = await SignInManager.GetVerifiedUserIdAsync();
            if (userId == null)
            {
                return View("Error");
            }
            var userFactors = await UserManager.GetValidTwoFactorProvidersAsync(userId);
            var factorOptions = userFactors.Select(purpose => new SelectListItem { Text = purpose, Value = purpose }).ToList();
            return View(new SendCodeViewModel { Providers = factorOptions, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        //
        // POST: /Account/SendCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SendCode(SendCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            // Generate the token and send it
            if (!await SignInManager.SendTwoFactorCodeAsync(model.SelectedProvider))
            {
                return View("Error");
            }
            return RedirectToAction("VerifyCode", new { Provider = model.SelectedProvider, ReturnUrl = model.ReturnUrl, RememberMe = model.RememberMe });
        }

        //
        // GET: /Account/ExternalLoginCallback
        [AllowAnonymous]
        public async Task<ActionResult> ExternalLoginCallback(string returnUrl)
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync();
            if (loginInfo == null)
            {
                return RedirectToAction("Login");
            }

            // Sign in the user with this external login provider if the user already has a login
            var result = await SignInManager.ExternalSignInAsync(loginInfo, isPersistent: false);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(returnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = false });
                case SignInStatus.Failure:
                default:
                    // If the user does not have an account, then prompt the user to create an account
                    ViewBag.ReturnUrl = returnUrl;
                    ViewBag.LoginProvider = loginInfo.Login.LoginProvider;
                    return View("ExternalLoginConfirmation", new ExternalLoginConfirmationViewModel { Email = loginInfo.Email });
            }
        }

        //
        // POST: /Account/ExternalLoginConfirmation
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel model, string returnUrl)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Manage");
            }

            if (ModelState.IsValid)
            {
                // Get the information about the user from the external login provider
                var info = await AuthenticationManager.GetExternalLoginInfoAsync();
                if (info == null)
                {
                    return View("ExternalLoginFailure");
                }
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
                var result = await UserManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    result = await UserManager.AddLoginAsync(user.Id, info.Login);
                    if (result.Succeeded)
                    {
                        await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                        return RedirectToLocal(returnUrl);
                    }
                }
                AddErrors(result);
            }

            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        //
        // POST: /Account/LogOff
        [HttpPost]
        [ValidateAntiForgeryToken]

        public ActionResult LogOff()
        {
            Session.RemoveAll();
            Session.Clear();
            Session.Abandon();
            AuthenticationManager.SignOut();
            return RedirectToAction("Login");
        }

        public ActionResult LogOut()
        {
            Session.RemoveAll();
            Session.Clear();
            Session.Abandon();
            AuthenticationManager.SignOut();
            return RedirectToAction("Login");
        }

        //
        // GET: /Account/ExternalLoginFailure
        [AllowAnonymous]
        public ActionResult ExternalLoginFailure()
        {
            return View();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_userManager != null)
                {
                    _userManager.Dispose();
                    _userManager = null;
                }

                if (_signInManager != null)
                {
                    _signInManager.Dispose();
                    _signInManager = null;
                }
            }

            base.Dispose(disposing);
        }

        #region Helpers
        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }

        internal class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUri)
                : this(provider, redirectUri, null)
            {
            }

            public ChallengeResult(string provider, string redirectUri, string userId)
            {
                LoginProvider = provider;
                RedirectUri = redirectUri;
                UserId = userId;
            }

            public string LoginProvider { get; set; }
            public string RedirectUri { get; set; }
            public string UserId { get; set; }

            public override void ExecuteResult(ControllerContext context)
            {
                var properties = new AuthenticationProperties { RedirectUri = RedirectUri };
                if (UserId != null)
                {
                    properties.Dictionary[XsrfKey] = UserId;
                }
                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
            }
        }
        #endregion

        public ActionResult SplashScreen()
        {
            return View();
        }
    }
}
