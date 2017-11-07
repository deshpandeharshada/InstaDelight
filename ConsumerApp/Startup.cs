using System;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.Google;
using Owin;
using AspNet.Identity.MySQL;
using ConsumerApp.Models;

[assembly: OwinStartup(typeof(ConsumerApp.Startup))]

namespace ConsumerApp
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
            ApplicationDbContext context = new ApplicationDbContext("DefaultConsumerConnection");

            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));
            var UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));
            if (!roleManager.RoleExists("Consumer"))
            {
                // first we create Admin rool   
                var role = new AspNet.Identity.MySQL.IdentityRole();
                role.Name = "Consumer";
                roleManager.Create(role);

                //Here we create a Admin super user who will maintain the website                  

                var user = new ApplicationUser();
                user.UserName = "9766638590";
                user.Email = "consumer@abc.com";
                string userPWD = "123456";
                user.FirstName = "Consumer";
                user.LastName = "Consumer";

                var chkUser = UserManager.Create(user, userPWD);

                //Add default User to Role Admin   
                if (chkUser.Succeeded)
                {
                    UserManager.AddToRole(user.Id, "Consumer");
                }
            }

        }
    }
}
