using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MerchantApp.Models
{
    public class SubmittedReviewDetails
    {
        public double? percentage { get; set; }
        public int queNo { get; set; }
        public int? star { get; set; }
        public string que { get; set; }
        public int reviewAttempted { get; set; }
        public int totalSubmittedReviews { get; set; }
    }
}