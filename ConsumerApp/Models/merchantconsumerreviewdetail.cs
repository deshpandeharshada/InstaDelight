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
    
    public partial class merchantconsumerreviewdetail
    {
        public int Id { get; set; }
        public string MerchantId { get; set; }
        public string ConsumerId { get; set; }
        public Nullable<int> ReviewId { get; set; }
        public Nullable<System.DateTime> SharedDate { get; set; }
        public string Status { get; set; }
        public Nullable<System.DateTime> ValidTill { get; set; }
    }
}
