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
    
    public partial class consumerredeemdetail
    {
        public int Id { get; set; }
        public string ConsumerId { get; set; }
        public string MerchantId { get; set; }
        public Nullable<int> PointsRedeemed { get; set; }
        public Nullable<System.DateTime> RedeemDate { get; set; }
    }
}
