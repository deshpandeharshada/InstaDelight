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
    
    public partial class customer
    {
        public int customer_id { get; set; }
        public int branch_id { get; set; }
        public string user_name { get; set; }
        public string last_name { get; set; }
        public string user_type { get; set; }
        public string user_email { get; set; }
        public string rm_email { get; set; }
        public string rname { get; set; }
        public string city { get; set; }
        public string state { get; set; }
        public string zip { get; set; }
        public string country { get; set; }
        public string password { get; set; }
        public string activation { get; set; }
        public string aproval { get; set; }
        public System.DateTime creation_date { get; set; }
        public string user_contact { get; set; }
    }
}
