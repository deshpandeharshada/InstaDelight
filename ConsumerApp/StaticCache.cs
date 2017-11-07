using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ConsumerApp.Models;
using Microsoft.AspNet.Identity;
using AspNet.Identity.MySQL;
using MySql.Data.MySqlClient;
using System.ComponentModel;

namespace ConsumerApp
{
    [System.ComponentModel.DataObject]
    public class StaticCache
    {
        public static void LoadStaticCache()
        {
            try
            {
                using (consumerEntities dataContext = new consumerEntities())
                {
                    HttpContext.Current.Application["banklist"] = dataContext.bank_master.ToList();
                    HttpContext.Current.Application["merchantlist"] = dataContext.merchant_master.ToList();
                }
            }
            catch (Exception ex)
            {
                //call error view here
                //log the error
                EventLog.LogErrorData("Error occurred Consumer/LoadStaticCache." + ex.Message, true);
            }
        }

        
        
        public static List<bank_master> GetBanks()
        {
            return HttpContext.Current.Application["banklist"] as List<bank_master> ;
        }

        public static List<merchant_master> GetMerchants()
        {
            return HttpContext.Current.Application["merchantlist"] as List<merchant_master>;
        }
    }
}