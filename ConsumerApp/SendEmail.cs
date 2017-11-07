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
using ConsumerApp.Models;
using System.Net.Mail;

namespace ConsumerApp
{
    public class SendEmail : Controller
    {
        [HttpPost]
        public string SendEmailToConsumer(EmailModel model)
        {
            try
            {
                using (MailMessage mm = new MailMessage(model.Email, model.To))               
                {
                    mm.Subject = model.Subject;
                    mm.Body = model.Body;
                    
                    
                    //if (model.Attachment.ContentLength > 0)
                    //{
                    //    string fileName = Path.GetFileName(model.Attachment.FileName);
                    //    mm.Attachments.Add(new Attachment(model.Attachment.InputStream, fileName));
                    //}
                    mm.IsBodyHtml = true;
                    using (SmtpClient smtp = new SmtpClient())
                    {

                        smtp.Host = "webmail.offertraker.com";
                        //smtp.EnableSsl = true;

                        //smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                        NetworkCredential NetworkCred = new NetworkCredential("enquiry@offertraker.com", "info.1234");
                        //smtp.UseDefaultCredentials = true;
                        smtp.Credentials = NetworkCred;
                        smtp.Port = 25;

                        smtp.Send(mm);
                        EventLog.LogData("Email sent successfully to " + model.To, true);
                        return "Email sent successfully to " + model.To;
                    }
                }
            }
            catch (Exception ex)
            {
                EventLog.LogErrorData("Error occurred while sending mail." + ex.Message, true);
                return ex.Message;
            }
        }
    }
}