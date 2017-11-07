using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MerchantApp.Models;

namespace MerchantApp
{
    public class ConsumerProfile
    {
        public int NoOfVisits { get; set; }
        public int NoOfPoints { get; set; }
        public List<CouponList> CouponList { get; set; }
        public List<consumergiftcarddetail> GiftCards { get; set; }
        public bool iscashback { get; set; }
        public string ConsumerName { get; set; }
        public DateTime? DOB { get; set; }
        public DateTime? DOA { get; set; }
    }
}