using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MerchantApp.Models
{
    // Models returned by AccountController actions.
    public class ExternalLoginConfirmationViewModel
    {
        [Display(Name = "Email",ResourceType=typeof(Global.Merchant))]
        public string Email { get; set; }

        [Display(Name = "Phone", ResourceType = typeof(Global.Merchant))]
        public string Phone { get; set; }
    }

    public class ExternalLoginListViewModel
    {
        public string ReturnUrl { get; set; }
    }

    public class SendCodeViewModel
    {
        public string SelectedProvider { get; set; }
        public ICollection<System.Web.Mvc.SelectListItem> Providers { get; set; }
        public string ReturnUrl { get; set; }
        public bool RememberMe { get; set; }
    }

    public class VerifyCodeViewModel
    {
        [Required]
        public string Provider { get; set; }

        [Required]
        [Display(Name = "Code")]
        public string Code { get; set; }
        public string ReturnUrl { get; set; }

        [Display(Name = "Remember this browser?")]
        public bool RememberBrowser { get; set; }

        public bool RememberMe { get; set; }
    }

    public class ForgotViewModel
    {
        [Required]
        [Display(Name = "Phone", ResourceType = typeof(Global.Merchant))]
        public string Phone { get; set; }
    }

    public class LoginViewModel
    {
        //[Required]
        [Display(Name = "UserName", ResourceType = typeof(Global.Merchant))]
        //[Phone]
        public string UserName { get; set; }

        //[Required]
        [Display(Name = "MobileNo", ResourceType = typeof(Global.Merchant))]
        [Phone]
        public string MobileNo { get; set; }

        //[Required]
        [Display(Name = "Email", ResourceType = typeof(Global.Merchant))]
        [EmailAddress]
        public string Email { get; set; }


        [Required]
        [Display(Name = "CountryCode", ResourceType = typeof(Global.Merchant))]
        //[Phone]
        public string CountryCode { get; set; }


        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password", ResourceType = typeof(Global.Merchant))]
        public string Password { get; set; }

        [Display(Name = "RememberMe", ResourceType = typeof(Global.Merchant))]
        public bool RememberMe { get; set; }
    }

    public class RegisterViewModel
    {
        
        [EmailAddress]
        [Display(Name = "Email", ResourceType = typeof(Global.Merchant))]
        public string Email { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 4)]
        [DataType(DataType.Password)]
        [Display(Name = "Password", ResourceType = typeof(Global.Merchant))]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirmpassword", ResourceType = typeof(Global.Merchant))]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        [Display(Name = "Phone", ResourceType = typeof(Global.Merchant))]
        public string Phone { get; set; }

        [Display(Name = "First Name", ResourceType = typeof(Global.Merchant))]
        public string FirstName { get; set; }

        [Display(Name = "Last Name", ResourceType = typeof(Global.Merchant))]
        public string LastName { get; set; }
    }

    public class ResetPasswordViewModel
    {
        [Required]
        [Display(Name = "UserName", ResourceType = typeof(Global.Merchant))]
       // [Phone]
        public string UserName { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 4)]
        [DataType(DataType.Password)]
        [Display(Name = "Password", ResourceType = typeof(Global.Merchant))]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirmpassword", ResourceType = typeof(Global.Merchant))]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        public string Code { get; set; }

        [Required]
        [Display(Name = "Select Language")]
        public int LanguageId { get; set; }

        [Required]
        [Display(Name = "Select Country")]
        public int CountryId { get; set; }
        //public string Country { get; set; }
    }

    public class ForgotPasswordViewModel
    {
        //[Required]
        [Display(Name = "Phone", ResourceType = typeof(Global.Merchant))]
        public string Phone { get; set; }

        //[Required]
        [Display(Name = "Email", ResourceType = typeof(Global.Merchant))]
        [EmailAddress]
        public string Email { get; set; }


        [Required]
        [Display(Name = "CountryCode", ResourceType = typeof(Global.Merchant))]
        //[Phone]
        public string CountryCode { get; set; }
    }
}
