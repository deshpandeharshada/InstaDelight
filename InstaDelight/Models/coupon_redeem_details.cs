//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace InstaDelight.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class coupon_redeem_details
    {
        public int id { get; set; }
        public Nullable<int> couponid { get; set; }
        public string couponcode { get; set; }
        public Nullable<System.DateTime> redeemedon { get; set; }
        public Nullable<int> merchantid { get; set; }
        public string city { get; set; }
        public string location { get; set; }
        public string ConsumerId { get; set; }
        public string ConsumerPhone { get; set; }
    }
}
