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
    
    public partial class reviewmaster
    {
        public int reviewid { get; set; }
        public Nullable<int> MerchantId { get; set; }
        public string Question1 { get; set; }
        public string Question1Type { get; set; }
        public string Question2 { get; set; }
        public string Question2Type { get; set; }
        public string Question3 { get; set; }
        public string Question3Type { get; set; }
        public string Question4 { get; set; }
        public string Question4Type { get; set; }
        public string DefaultQuestion { get; set; }
        public string DefaultType { get; set; }
        public string MerchantUserId { get; set; }
        public Nullable<System.DateTime> CreationDate { get; set; }
    }
}
