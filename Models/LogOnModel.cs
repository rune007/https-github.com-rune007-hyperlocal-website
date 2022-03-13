using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Web.Mvc;
using System.Web.Security;

namespace HLWebRole.Models
{
    public class LogOnModel
    {
        [Required]
        [DataType(DataType.EmailAddress)]
        // NEEDS TO BE UNCOMMENTED, DURING DEVELOPMENT IT'S JUST EASIER WITHOUT THIS VALIDATION.
        //[RegularExpression(".+\\@.+\\..+", ErrorMessage = "Please enter a valid email address")]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }

        [DataType(DataType.Text)]
        [Display(Name = "LastLoginLatitude")]
        public double LastLoginLatitude { get; set; }

        [DataType(DataType.Text)]
        [Display(Name = "LastLoginLongitude")]
        public double LastLoginLongitude { get; set; }
    }
}