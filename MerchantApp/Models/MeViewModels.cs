using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MerchantApp.Models
{
    // Models returned by MeController actions.
    public class GetViewModel
    {
        public string Phone { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}