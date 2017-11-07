using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MerchantApp.Models;

namespace MerchantApp.Models
{
    public class GiftCardModel
    {
        public int Id { get; set; }        
        public string currency { get; set; }
        public double DenominationRs { get; set; }
        public DateTime validtill { get; set; }
        public string MerchantName { get; set; }
        public byte[] GiftCardDEC { get; set; }
        public string merchantDecFromLibrary { get; set; }
    }
}