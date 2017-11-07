using System;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.Google;
using Owin;
using AspNet.Identity.MySQL;
using InstaDelight.Models;


[assembly: OwinStartup(typeof(InstaDelight.Startup))]

namespace InstaDelight
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


            // In Startup iam creating first Admin Role and creating a default Admin User    
            if (!roleManager.RoleExists("Admin"))
            {
                // first we create Admin rool   
                var role = new AspNet.Identity.MySQL.IdentityRole();
                role.Name = "Admin";
                roleManager.Create(role);

                //Here we create a Admin super user who will maintain the website                  

                var user = new ApplicationUser();
                user.UserName = "9011085421";
                user.Email = "admin@abc.com";
                string userPWD = "123456";
                user.FirstName = "Admin";
                user.LastName = "Admin";

                var chkUser = UserManager.Create(user, userPWD);

                ////Add default User to Role Admin   
                if (chkUser.Succeeded)
                {
                    var rolesForUser = UserManager.GetRoles(user.Id);
                    if (!rolesForUser.Contains("Admin"))
                    {
                        UserManager.AddToRole(user.Id, "Admin");
                    }
                }
            }
            if (!roleManager.RoleExists("SupportAdmin"))
            {
                // first we create Admin rool   
                var role = new AspNet.Identity.MySQL.IdentityRole();
                role.Name = "SupportAdmin";
                roleManager.Create(role);
            }
            if (!roleManager.RoleExists("SupportUsers"))
            {
                // first we create Admin rool   
                var role = new AspNet.Identity.MySQL.IdentityRole();
                role.Name = "SupportUsers";
                roleManager.Create(role);
            }
            //if (!roleManager.RoleExists("SarvatraAdmin"))
            //{
            //    // first we create Admin rool   
            //    var role = new AspNet.Identity.MySQL.IdentityRole();
            //    role.Name = "SarvatraAdmin";
            //    roleManager.Create(role);

            //    //Here we create a Admin super user who will maintain the website                  

            //    var user = new ApplicationUser();
            //    user.UserName = "sarvatrasupport";
            //    user.Email = "sarvatraadmin@abc.com";
            //    string userPWD = "123456";
            //    user.FirstName = "Sarvatra";
            //    user.LastName = "Admin";

            //    var chkUser = UserManager.Create(user, userPWD);

            //    ////Add default User to Role Admin   
            //    if (chkUser.Succeeded)
            //    {
            //        var rolesForUser = UserManager.GetRoles(user.Id);
            //        if (!rolesForUser.Contains("SarvatraAdmin"))
            //        {
            //            UserManager.AddToRole(user.Id, "SarvatraAdmin");
            //        }
            //    }               
            //}
            if (!roleManager.RoleExists("InstadelightAdmin"))
            {
                // first we create Admin rool   
                var role = new AspNet.Identity.MySQL.IdentityRole();
                role.Name = "InstadelightAdmin";
                roleManager.Create(role);

                //Here we create a Admin super user who will maintain the website                  

                var user = new ApplicationUser();
                user.UserName = "instadelightsupport";
                user.Email = "instadelightadmin@abc.com";
                string userPWD = "123456";
                user.FirstName = "Instadelight";
                user.LastName = "Admin";

                var chkUser = UserManager.Create(user, userPWD);

                ////Add default User to Role Admin   
                if (chkUser.Succeeded)
                {
                    var rolesForUser = UserManager.GetRoles(user.Id);
                    if (!rolesForUser.Contains("InstadelightAdmin"))
                    {
                        UserManager.AddToRole(user.Id, "InstadelightAdmin");
                    }
                }
            }
            //if (!roleManager.RoleExists("TECHBMSSAdmin"))
            //{
            //    // first we create Admin rool   
            //    var role = new AspNet.Identity.MySQL.IdentityRole();
            //    role.Name = "TECHBMSSAdmin";
            //    roleManager.Create(role);

            //    //Here we create a Admin super user who will maintain the website                  

            //    var user = new ApplicationUser();
            //    user.UserName = "techbmsssupport";
            //    user.Email = "techbmsssupportadmin@abc.com";
            //    string userPWD = "123456";
            //    user.FirstName = "techbmss";
            //    user.LastName = "Admin";                

            //    var chkUser = UserManager.Create(user, userPWD);

            //    ////Add default User to Role Admin   
            //    if (chkUser.Succeeded)
            //    {
            //        var rolesForUser = UserManager.GetRoles(user.Id);
            //        if (!rolesForUser.Contains("TECHBMSSAdmin"))
            //        {
            //            UserManager.AddToRole(user.Id, "TECHBMSSAdmin");
            //        }
            //    }
            //}
            //if (!roleManager.RoleExists("SarvatraUsers"))
            //{
            //    // first we create Admin rool   
            //    var role = new AspNet.Identity.MySQL.IdentityRole();
            //    role.Name = "SarvatraUsers";
            //    roleManager.Create(role);

            //    //Here we create a Admin super user who will maintain the website                  
            //    /*      Deepali Shinde (manager access)
            //            Dhiraj Desai -view only
            //            Harish Landge -view only
            //            Pravin Jadhav -view only
            //            Nikhih Shinde -view only
            //            Sachin Kalaskar --view only
            //            Baburao Nevase -view only
            //            Beena Rajput -view only
            //            Swati Swami -view only
            //            Vijayalaxmi Akrakeri -view only*/

            //    var user = new ApplicationUser();
            //    user.UserName = "Deepali.Shinde@sarvatra.in";
            //    user.Email = "Deepali.Shinde@sarvatra.in";
            //    string userPWD = "123456";
            //    user.FirstName = "Deepali";
            //    user.LastName = "Shinde";

            //    var chkUser = UserManager.Create(user, userPWD);

            //    ////Add default User to Role Admin   
            //    if (chkUser.Succeeded)
            //    {
            //        var rolesForUser = UserManager.GetRoles(user.Id);
            //        if (!rolesForUser.Contains("SarvatraUsers"))
            //        {
            //            UserManager.AddToRole(user.Id, "SarvatraUsers");
            //        }
            //    }

            //    user = new ApplicationUser();
            //    user.UserName = "Dhiraj.Desai@sarvatra.in";
            //    user.Email = "Dhiraj.Desai@sarvatra.in";

            //    user.FirstName = "Dhiraj";
            //    user.LastName = "Desai";

            //    chkUser = UserManager.Create(user, userPWD);

            //    ////Add default User to Role Admin   
            //    if (chkUser.Succeeded)
            //    {
            //        var rolesForUser = UserManager.GetRoles(user.Id);
            //        if (!rolesForUser.Contains("SarvatraUsers"))
            //        {
            //            UserManager.AddToRole(user.Id, "SarvatraUsers");
            //        }
            //    }

            //    user = new ApplicationUser();
            //    user.UserName = "Harish.Landge@sarvatra.in";
            //    user.Email = "Harish.Landge@sarvatra.in";

            //    user.FirstName = "Harish";
            //    user.LastName = "Landge";

            //    chkUser = UserManager.Create(user, userPWD);

            //    ////Add default User to Role Admin   
            //    if (chkUser.Succeeded)
            //    {
            //        var rolesForUser = UserManager.GetRoles(user.Id);
            //        if (!rolesForUser.Contains("SarvatraUsers"))
            //        {
            //            UserManager.AddToRole(user.Id, "SarvatraUsers");
            //        }
            //    }

            //    user = new ApplicationUser();
            //    user.UserName = "Pravin.Jadhav@sarvatra.in";
            //    user.Email = "Pravin.Jadhav@sarvatra.in";

            //    user.FirstName = "Pravin";
            //    user.LastName = "Jadhav";

            //    chkUser = UserManager.Create(user, userPWD);

            //    ////Add default User to Role Admin   
            //    if (chkUser.Succeeded)
            //    {
            //        var rolesForUser = UserManager.GetRoles(user.Id);
            //        if (!rolesForUser.Contains("SarvatraUsers"))
            //        {
            //            UserManager.AddToRole(user.Id, "SarvatraUsers");
            //        }
            //    }

            //    user = new ApplicationUser();
            //    user.UserName = "Nikhil.Shinde@sarvatra.in";
            //    user.Email = "Nikhil.Shinde@sarvatra.in";

            //    user.FirstName = "Nikhil";
            //    user.LastName = "Shinde";

            //    chkUser = UserManager.Create(user, userPWD);

            //    ////Add default User to Role Admin   
            //    if (chkUser.Succeeded)
            //    {
            //        var rolesForUser = UserManager.GetRoles(user.Id);
            //        if (!rolesForUser.Contains("SarvatraUsers"))
            //        {
            //            UserManager.AddToRole(user.Id, "SarvatraUsers");
            //        }
            //    }

            //    user = new ApplicationUser();
            //    user.UserName = "Sachin.Kalaskar@sarvatra.in";
            //    user.Email = "Sachin.Kalaskar@sarvatra.in";

            //    user.FirstName = "Sachin";
            //    user.LastName = "Kalaskar";

            //    chkUser = UserManager.Create(user, userPWD);

            //    ////Add default User to Role Admin   
            //    if (chkUser.Succeeded)
            //    {
            //        var rolesForUser = UserManager.GetRoles(user.Id);
            //        if (!rolesForUser.Contains("SarvatraUsers"))
            //        {
            //            UserManager.AddToRole(user.Id, "SarvatraUsers");
            //        }
            //    }

            //    user = new ApplicationUser();
            //    user.UserName = "Baburao.Nevase@sarvatra.in";
            //    user.Email = "Baburao.Nevase@sarvatra.in";

            //    user.FirstName = "Baburao";
            //    user.LastName = "Nevase";

            //    chkUser = UserManager.Create(user, userPWD);

            //    ////Add default User to Role Admin   
            //    if (chkUser.Succeeded)
            //    {
            //        var rolesForUser = UserManager.GetRoles(user.Id);
            //        if (!rolesForUser.Contains("SarvatraUsers"))
            //        {
            //            UserManager.AddToRole(user.Id, "SarvatraUsers");
            //        }
            //    }

            //    user = new ApplicationUser();
            //    user.UserName = "Beena.Rajput@sarvatra.in";
            //    user.Email = "Beena.Rajput@sarvatra.in";

            //    user.FirstName = "Beena";
            //    user.LastName = "Rajput";

            //    chkUser = UserManager.Create(user, userPWD);

            //    ////Add default User to Role Admin   
            //    if (chkUser.Succeeded)
            //    {
            //        var rolesForUser = UserManager.GetRoles(user.Id);
            //        if (!rolesForUser.Contains("SarvatraUsers"))
            //        {
            //            UserManager.AddToRole(user.Id, "SarvatraUsers");
            //        }
            //    }

            //    user = new ApplicationUser();
            //    user.UserName = "Swati.Swami@sarvatra.in";
            //    user.Email = "Swati.Swami@sarvatra.in";

            //    user.FirstName = "Swati";
            //    user.LastName = "Swami";

            //    chkUser = UserManager.Create(user, userPWD);

            //    ////Add default User to Role Admin   
            //    if (chkUser.Succeeded)
            //    {
            //        var rolesForUser = UserManager.GetRoles(user.Id);
            //        if (!rolesForUser.Contains("SarvatraUsers"))
            //        {
            //            UserManager.AddToRole(user.Id, "SarvatraUsers");
            //        }
            //    }

            //    user = new ApplicationUser();
            //    user.UserName = "Vijayalaxmi.Akrakeri@sarvatra.in";
            //    user.Email = "Vijayalaxmi.Akrakeri@sarvatra.in";

            //    user.FirstName = "Vijayalaxmi";
            //    user.LastName = "Akrakeri";

            //    chkUser = UserManager.Create(user, userPWD);

            //    ////Add default User to Role Admin   
            //    if (chkUser.Succeeded)
            //    {
            //        var rolesForUser = UserManager.GetRoles(user.Id);
            //        if (!rolesForUser.Contains("SarvatraUsers"))
            //        {
            //            UserManager.AddToRole(user.Id, "SarvatraUsers");
            //        }
            //    }
            //}

        }
    }
}
