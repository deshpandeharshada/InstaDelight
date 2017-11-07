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
using InstaDelight.Models;

namespace InstaDelight
{
    public class SMSUtility : Controller
    {
        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public string sendMessage(string phoneNo, string message)
        {
            try
            {
                //
                //string url = "http://124.124.75.93:9090/SmsGatewayWeb/api/srvt/sms";
                //string url = "http://202.154.164.183:9090/SmsGatewayWeb/api/srvt/sms";
                string url = "http://enterprise.smsgupshup.com/GatewayAPI/rest?method=SendMessage";

                string textResult = "";
                message = message.Replace("&", "%26");
                //message = HttpUtility.UrlPathEncode(message);
                //String strPost = "?user=" + HttpUtility.UrlPathEncode("KUMARPROPERTIES") + "&password=" + HttpUtility.UrlPathEncode("7304940259") + "&sender=" + HttpUtility.UrlPathEncode("KUMARS") + "&mobile=" + HttpUtility.UrlPathEncode(phoneNo) + "&type=" + HttpUtility.UrlPathEncode("3") + "&message=" + message;
                //string strPost = "?user=PravinSutar&password=Insta123#&msisdn=" + phoneNo + "&sid=INSDLT&msg=" + message + "&fl=0&gwid=2";
                //string strPost = "?mobileNo=" + phoneNo + "&smsText=" + message + "&clientID=inde";
                string strPost = "&send_to=" + phoneNo + "&userid=2000170305&password=O1czbaj9f&msg=" + message + "&v=1.1&msg_type=TEXT&mask=OFRTKR&auth_scheme=PLAIN";

                using (var client = new HttpClient())
                {
                    var uri = new Uri(url + strPost);

                    var response = client.GetAsync(uri).Result;

                    textResult = response.Content.ReadAsStringAsync().Result;
                    EventLog.LogData("SMS Text=" + uri, true);
                }
                EventLog.LogData("SMS sent successfully to " + phoneNo + ". Status: " + textResult, true);

                return "SMS sent successfully.";
            }
            catch (Exception ex)
            {
                EventLog.LogErrorData("Error occured in sendMessage:" + ex.Message, true);
                return "SMS Send Failed";
            }
        }




        //Note: For multiple numbers followed by comma separated, Ex: Mobile no1,mobile no2, ... (Dont Prefix 91 to Mobile number)
    }
}