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
    
    public partial class branch_master
    {
        public int BranchId { get; set; }
        public Nullable<int> MerchantId { get; set; }
        public Nullable<int> BrandId { get; set; }
        public string BranchLocation { get; set; }
        public string PrimaryId { get; set; }
        public Nullable<bool> IsMenuAllowed { get; set; }
        public Nullable<bool> IsCouponAllowed { get; set; }
        public Nullable<bool> IsAddUserAllowed { get; set; }
        public Nullable<int> BranchManagerId { get; set; }
        public Nullable<bool> IsEventCouponsAllowed { get; set; }
        public string BranchManagerName { get; set; }
    }
}