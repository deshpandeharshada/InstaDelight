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
    
    public partial class consumermaster
    {
        public int id { get; set; }
        public string consumername { get; set; }
        public byte[] consumerlogo { get; set; }
        public string Email { get; set; }
        public string Phone1 { get; set; }
        public string Phone2 { get; set; }
        public string Phone3 { get; set; }
        public Nullable<System.DateTime> DOA { get; set; }
        public Nullable<System.DateTime> DOB { get; set; }
        public string Gender { get; set; }
        public string BuildingName { get; set; }
        public string SocietyName { get; set; }
        public string Street { get; set; }
        public string Location { get; set; }
        public string City { get; set; }
        public Nullable<int> State { get; set; }
        public Nullable<int> Country { get; set; }
        public string UserId { get; set; }
        public Nullable<int> PinCode { get; set; }
        public Nullable<int> LanguageId { get; set; }
        public Nullable<bool> EmailVerified { get; set; }
        public Nullable<bool> Phone2Verified { get; set; }
        public Nullable<bool> Phone3Verified { get; set; }
    }
}
