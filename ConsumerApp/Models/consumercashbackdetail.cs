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
    
    public partial class consumercashbackdetail
    {
        public int Id { get; set; }
        public string ConsumerId { get; set; }
        public string MerchantId { get; set; }
        public Nullable<int> Cashback { get; set; }
        public Nullable<System.DateTime> CashbackDate { get; set; }
        public Nullable<System.DateTime> ExpiryDate { get; set; }
    }
}
