//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace ConsumerApp.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class eventcoupontemplate
    {
        public int Id { get; set; }
        public Nullable<int> CategoryId { get; set; }
        public string CouponTitle { get; set; }
        public string CouponDetails { get; set; }
        public Nullable<decimal> PercentageOff { get; set; }
        public Nullable<decimal> Discount { get; set; }
        public string ValidAtLocation { get; set; }
        public Nullable<decimal> AboveAmount { get; set; }
        public Nullable<decimal> MaxDiscount { get; set; }
        public Nullable<int> EventId { get; set; }
    }
}
