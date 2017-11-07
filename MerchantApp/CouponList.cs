using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MerchantApp
{
    public class CouponList
    {
        public int Id { get; set; }
        public int couponid { get; set; }
        public string CouponTitle { get; set; }
        public string CouponCode { get; set; }
        public string CouponDetails { get; set; }
        public Nullable<int> MerchantId { get; set; }
        public Nullable<System.DateTime> ValidFrom { get; set; }
        public Nullable<System.DateTime> ValidTill { get; set; }
        public Nullable<int> categoryid { get; set; }
        public Nullable<decimal> PercentageOff { get; set; }
        public Nullable<decimal> Discount { get; set; }
        public Nullable<decimal> AboveAmount { get; set; }
        public string ValidAtLocation { get; set; }
        public byte[] DEC { get; set; }
        public byte[] QRCode { get; set; }
        public Nullable<decimal> MaxDiscount { get; set; }
        public Nullable<int> MaxCoupons { get; set; }
        public Nullable<sbyte> ShareWithAll { get; set; }
        public Nullable<System.DateTime> DateCreated { get; set; }
    }
}