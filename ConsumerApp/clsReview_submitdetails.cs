using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ConsumerApp
{
    //Class for Review Submit
    public class clsReview_submitdetails
    {
        public int reviewid { get; set; }
        public int Question1Rating { get; set; }
        public int Question2Rating { get; set; }
        public int Question3Rating { get; set; }
        public int Question4Rating { get; set; }
        public sbyte IsSharedDECWithFriends { get; set; }
        public int merchantId { get; set; }
        public string consumerId;
        public string Comment { get; set; }
    }
}