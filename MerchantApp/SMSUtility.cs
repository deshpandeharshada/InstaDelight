using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Net.Http;
using System.Web.Mvc;
using System.Threading.Tasks;
using System.Globalization;
using System.Security.Claims;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using MerchantApp.Models;


namespace MerchantApp
{
    public class SMSUtility : Controller
    {
        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        public string sendMessage(string phoneNo, string message)
        {
            try
            {
                //
                string url = "http://enterprise.smsgupshup.com/GatewayAPI/rest?method=SendMessage";
                string textResult = "";
                //message = HttpUtility.UrlPathEncode(message);
                message = message.Replace("&", "%26");
                string[] PhonewithCode = phoneNo.Split(' ');
                if (PhonewithCode.Length > 0)
                {
                    if (PhonewithCode.Length == 2)
                    {
                        phoneNo = PhonewithCode[1].ToString();
                    }
                }

                string strPost = "&send_to=" + phoneNo + "&userid=2000170305&password=O1czbaj9f&msg=" + message + "&v=1.1&msg_type=TEXT&mask=OFRTKR&auth_scheme=PLAIN";
                using (var client = new HttpClient())
                {
                    var uri = new Uri(url + strPost);

                    var response = client.GetAsync(uri).Result;

                    textResult = response.Content.ReadAsStringAsync().Result;

                    EventLog.LogData("SMSText=" + uri.ToString(), true);
                }

                EventLog.LogData("SMS sent successfully to " + phoneNo + ". Status: " + textResult, true);

                return "SMS sent successfully to " + phoneNo + ". Status: " + textResult;
            }
            catch (Exception ex)
            {
                EventLog.LogErrorData("Error occured in sendMessage:" + ex.Message, true);
                return "SMS Send Failed. Reason: " + ex.Message;
            }
        }
        //Note: For multiple numbers followed by comma separated, Ex: Mobile no1,mobile no2, ... (Dont Prefix 91 to Mobile number)
    }
}