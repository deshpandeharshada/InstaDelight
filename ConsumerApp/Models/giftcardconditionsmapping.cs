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
    
    public partial class giftcardconditionsmapping
    {
        public int id { get; set; }
        public Nullable<int> GiftCardId { get; set; }
        public string MerchantUserId { get; set; }
        public string TermsCondition { get; set; }
    }
}