//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace MerchantApp.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class review_submit_details
    {
        public int id { get; set; }
        public Nullable<int> reviewid { get; set; }
        public string consumerid { get; set; }
        public Nullable<int> Question1Rating { get; set; }
        public Nullable<int> Question2Rating { get; set; }
        public Nullable<int> Question3Rating { get; set; }
        public Nullable<int> Question4Rating { get; set; }
        public Nullable<sbyte> IsSharedDECWithFriends { get; set; }
        public Nullable<int> MerchantId { get; set; }
        public Nullable<System.DateTime> Review_Submit_date { get; set; }
        public string Comment { get; set; }
    }
}
