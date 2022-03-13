using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Web.Mvc;

namespace HLWebRole.Models
{
    // This model is used mostly to carry the search parameters in relation to search on NewsItems.
    public class SearchCommunityModel
    {
        [DisplayName("Type Location to Center Geo Search (You can drag Pushpin):")]
        [Required(ErrorMessage = "A Search Center is required for Geo Search!")]
        public string Address { get; set; }

        [DisplayName("Search Radius")]
        public int? SearchRadius { get; set; }

        [DataType(DataType.Text)]
        [DisplayName("Type part of Name:")]
        public string Name { get; set; }

        [DataType(DataType.Text)]
        [DisplayName("Type part of Description:")]
        public string Description { get; set; }

        [Required]
        public double Latitude { get; set; }

        [Required]
        public double Longitude { get; set; }

        // The number of search results a particular search is yielding.
        public int NumberOfSearchResults { get; set; }
    }
}