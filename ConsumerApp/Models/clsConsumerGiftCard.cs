using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ConsumerApp.Models
{
    public class clsConsumerGiftCard
    {
        public string consumerid { get; set; }
        public int Id { get; set; }
        public int giftcardid { get; set; }
        public int Denomination { get; set; }
        public string merchantId { get; set; }
        public int status { get; set; }
        public string MerchantName { get; set; }
        public byte[] MerchantDEC { get; set; }
        public byte[] MerchantLogo { get; set; }
        public byte[] ConsumerLogo { get; set; }
        public DateTime ValidTill { get; set; }
        public string currency { get; set; }
        public string DECColor { get; set; }
        public string merchantDecFromLibrary { get; set; }
    }
}