using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Web.Mvc;

namespace HLWebRole.Models
{
    // This model is used mostly to carry the search parameters in relation to search on Users.
    public class SearchUserModel
    {
        [DisplayName("Type Location to Center Geo Search (You can drag Pushpin):")]
        [Required(ErrorMessage = "A Search Center is required for Geo Search!")]
        public string Address { get; set; }

        [DisplayName("Search Radius")]       
        public int? SearchRadius { get; set; }

        [DataType(DataType.Text)]
        [DisplayName("Type part of First Name:")]
        public string FirstName { get; set; }

        [DataType(DataType.Text)]
        [DisplayName("Type part of Last Name:")]
        public string LastName { get; set; }

        [DataType(DataType.Text)]
        [DisplayName("Type part of Bio:")]
        public string Bio { get; set; }

        [DataType(DataType.Text)]
        [DisplayName("Type part of Email:")]
        public string Email { get; set; }

        [DataType(DataType.Text)]
        [DisplayName("Type part of Address:")]
        public string UserAddress { get; set; }

        [DataType(DataType.Text)]
        [DisplayName("Type part of Phone:")]
        public string Phone { get; set; }

        [Required]
        public double Latitude { get; set; }

        [Required]
        public double Longitude { get; set; }

        // The number of search results a particular search is yielding.
        public int NumberOfSearchResults { get; set; }
    }
}