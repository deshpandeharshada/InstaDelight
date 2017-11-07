using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MerchantApp.Models
{
    public class CustomerSummaryViewModel
    {
        public int serialno { get; set; }
        public DateTime Date { get; set; }
        public string UserName { get; set; }
        public int VisitNumber { get; set; }
        public double AmountSpent { get; set; }
        public int TotalPoints { get; set; }
        public int PointsRedeemed { get; set; }
        public int PointsEarned { get; set; }
        public string smsemailstatus { get; set; }
        public double TotalAmountSpent { get; set; }
        public DateTime Birthday { get; set; }
        public DateTime Anniversary { get; set; }

    }
}