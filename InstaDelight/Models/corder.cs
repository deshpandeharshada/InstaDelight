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
    
    public partial class corder
    {
        public int order_id { get; set; }
        public int token_id { get; set; }
        public int cbill { get; set; }
        public int item_code { get; set; }
        public string submenu { get; set; }
        public int active { get; set; }
        public string business_id { get; set; }
        public string branch_id { get; set; }
        public string type { get; set; }
        public int tabl { get; set; }
        public int payment { get; set; }
        public int cashier { get; set; }
        public int ready { get; set; }
        public int delivery { get; set; }
        public string item_name { get; set; }
        public string item_price { get; set; }
        public string quantity { get; set; }
        public int tprice { get; set; }
        public int total { get; set; }
        public System.DateTime creation_date { get; set; }
        public string customer_name { get; set; }
        public string customer_address { get; set; }
        public string customer_contact { get; set; }
    }
}
