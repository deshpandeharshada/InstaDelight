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
    
    public partial class bank_master
    {
        public int bankid { get; set; }
        public string bankname { get; set; }
        public string button1_text { get; set; }
        public string button1_url { get; set; }
        public string button2_text { get; set; }
        public string button2_url { get; set; }
        public string button3_text { get; set; }
        public string button3_url { get; set; }
        public string button4_text { get; set; }
        public string button4_url { get; set; }
        public Nullable<System.DateTime> CreateDate { get; set; }
        public Nullable<sbyte> button1_status { get; set; }
        public Nullable<sbyte> button2_status { get; set; }
        public Nullable<sbyte> button3_status { get; set; }
        public Nullable<sbyte> button4_status { get; set; }
        public byte[] bank_logo { get; set; }
    }
}
