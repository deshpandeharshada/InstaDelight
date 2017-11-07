using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ConsumerApp.Models
{
    // Models returned by MeController actions.
    public class GetViewModel
    {
       // public string Hometown { get; set; }
        public string Phone { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}