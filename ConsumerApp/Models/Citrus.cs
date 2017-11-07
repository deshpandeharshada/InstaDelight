using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ConsumerApp.Models
{
    public class Citrus
    {
        public string formPostUrl { get; set; }
        public string merchantTxnId { get; set; }
        public string orderAmount { get; set; }
        public string currency { get; set; }
        public string email { get; set; }
        public string phoneNumber { get; set; }
        public string returnUrl { get; set; }
        public string notifyUrl { get; set; }
        public string secSignature { get; set; }


    }
}