using System;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.Google;
using Owin;
using AspNet.Identity.MySQL;
using MerchantApp.Models;

[assembly: OwinStartup(typeof(MerchantApp.Startup))]

namespace MerchantApp
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
            createRolesandUsers();   
        }

        private void createRolesandUsers()
        {
            ApplicationDbContext context = new ApplicationDbContext("DefaultConnection");

            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));
            var UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));
            UserManager.UserValidator = new UserValidator<ApplicationUser>(UserManager)
            {
                AllowOnlyAlphanumericUserNames = false,
                RequireUniqueEmail = true
            };

            if (!roleManager.RoleExists("Merchant"))
            {
                // first we create Admin rool   
                var role = new AspNet.Identity.MySQL.IdentityRole();
                role.Name = "Merchant";
                roleManager.Create(role);

                //Here we create a Admin super user who will maintain the website                  

                var user = new ApplicationUser();
                user.UserName = "9011085421";
                user.Email = "consumer@abc.com";
                string userPWD = "123456";
                user.FirstName = "Merchant";
                user.LastName = "Merchant";

                var chkUser = UserManager.Create(user, userPWD);

                //Add default User to Role Admin   
                if (chkUser.Succeeded)
                {
                    UserManager.AddToRole(user.Id, "Merchant");
                }
            }

            if (!roleManager.RoleExists("BrandManager"))
            {
                // first we create Admin rool   
                var role = new AspNet.Identity.MySQL.IdentityRole();
                role.Name = "BrandManager";
                roleManager.Create(role);
            }


            if (!roleManager.RoleExists("LocationManager"))
            {
                // first we create Admin rool   
                var role = new AspNet.Identity.MySQL.IdentityRole();
                role.Name = "LocationManager";
                roleManager.Create(role);
            }


            if (!roleManager.RoleExists("Staff"))
            {
                // first we create Admin rool   
                var role = new AspNet.Identity.MySQL.IdentityRole();
                role.Name = "Staff";
                roleManager.Create(role);
            }


        }
    }
}
