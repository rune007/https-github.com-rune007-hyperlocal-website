using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Globalization;
using System.Web.Mvc;

namespace HLWebRole.Models
{
    public class UserModel
    {
        public int UserID { get; set; }

        // In the Edit view, we want to allow the user to save their unchanged email address back into the system. The remote validation 
        // method DisallowDuplicateEmailEditView compares the existing email (email) with the email which is entered in the Edit view (emailEditView). 
        // If they are the same we know it's OK, because it's the users own email. If they are not the same, we check to see if the email exists in 
        // the system already, and if it does, we will not allow that it is entered into the system.
        [Required]
        [DataType(DataType.EmailAddress)]
        [RegularExpression(".+\\@.+\\..+", ErrorMessage = "Please enter a valid email address")]
        [StringLength(150, ErrorMessage = "Email address may not be longer than 150 characters")]
        [Remote("DisallowDuplicateEmailEditView", "User", AdditionalFields = "Email", HttpMethod = "POST")]
        [Display(Name = "Email address")]
        public string EmailEditView { get; set; }

        [Required]
        [DataType(DataType.EmailAddress)]
        [RegularExpression(".+\\@.+\\..+", ErrorMessage = "Please enter a valid email address")]
        [StringLength(150, ErrorMessage = "Email address may not be longer than 150 characters")]
        [Remote("DisallowDuplicateEmail", "User", HttpMethod = "POST")]
        [Display(Name = "Email address")]
        public string Email { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        [Required]
        [DataType(DataType.Text)]
        [StringLength(30, ErrorMessage = "First name may not be longer than 30 characters")]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required]
        [DataType(DataType.Text)]
        [StringLength(30, ErrorMessage = "Last name may not be longer than 30 characters")]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Required]
        [DataType(DataType.Text)]
        [StringLength(300, ErrorMessage = "Bio may not be longer than 300 characters")]
        [Display(Name = "Bio")]
        public string Bio { get; set; }

        [Required]
        [DataType(DataType.Text)]
        [RegularExpression("^[0-9]+$", ErrorMessage = "Please enter a number only")]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; }

        [Required]
        [DataType(DataType.Text)]
        [StringLength(150, ErrorMessage = "Address may not be longer than 150 characters")]
        [Display(Name = "Address - (Street #, Postal Code, City, Country)")]
        public string Address { get; set; }

        public string AddressPositionPointWkt { get; set; }

        [DataType(DataType.DateTime)]
        [DisplayName("Became Member")]
        public DateTime CreateDate { get; set; }

        public string LastLoginPositionPointWkt { get; set; }

        [Required]
        [DataType(DataType.Text)]
        [Display(Name = "LastLoginLatitude")]
        public double LastLoginLatitude { get; set; }

        [Required]
        [DataType(DataType.Text)]
        [Display(Name = "LastLoginLongitude")]
        public double LastLoginLongitude { get; set; }

        [Required]
        [DataType(DataType.Text)]
        [DisplayName("Latitude")]
        [Remote("IsLatLongWithinDenmark", "SpatialQuery", AdditionalFields = "Longitude", HttpMethod = "POST")]
        public double Latitude { get; set; }

        [Required]
        [DataType(DataType.Text)]
        [DisplayName("Longitude")]
        public double Longitude { get; set; }

        public string ImageBlobUri { get; set; }

        public string MediumSizeBlobUri { get; set; }

        public string ThumbnailBlobUri { get; set; }

        // A bool which indicates whether two users share their contact info with each other.
        public bool AreUsersSharingContactInfo { get; set; }

        // A bool which indicates whether there is a ContactInfoRequest pending inbetween two users.
        public bool IsContactInfoRequestPending { get; set; }
    }
}