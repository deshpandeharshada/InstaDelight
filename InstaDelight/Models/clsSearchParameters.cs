using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace InstaDelight.Models
{
    public class clsSearchParameters
    {
        public string country { get; set; }
        public string state { get; set; }
        public string city { get; set; }
        public string pin { get; set; }
        public string mobile { get; set; }
        public string business { get; set; }
        public string VAR { get; set; }
        public string Srep { get; set; }
        public DateTime validfrom { get; set; }
        public DateTime validtill { get; set; }
    }
}