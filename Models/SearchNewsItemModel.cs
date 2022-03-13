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
    public class SearchNewsItemModel
    {
        [DisplayName("Type Location to Center Geo Search (You can drag Pushpin):")]
        [Required(ErrorMessage = "A Search Center is required for Geo Search!")]
        public string Address { get; set; }

        [DisplayName("Search Radius")]
        public int? SearchRadius { get; set; }

        [DisplayName("Select Category:")]
        public int? CategoryID { get; set; }

        public string CategoryName { get; set; }

        public IEnumerable<SelectListItem> CategorySelectList { get; set; }

        [DisplayName("Select Assignment:")]
        public int? AssignmentID { get; set; }

        public string AssignmentTitle { get; set; }

        public IEnumerable<SelectListItem> AssignmentSelectList { get; set; }

        [DataType(DataType.Text)]
        [DisplayName("Type part of Title:")]
        public string Title { get; set; }

        [DataType(DataType.Text)]
        [DisplayName("Type part of News Story:")]
        public string Story { get; set; }

        [DataType(DataType.Date)]
        [DisplayName("Date")]
        [Required]
        // Regular Expression matching date in format (MM/DD/YYYY)
        [RegularExpression(@"^(((0?[1-9]|1[012])/(0?[1-9]|1\d|2[0-8])|(0?[13456789]|1[012])/(29|30)|(0?[13578]|1[02])/31)/(19|[2-9]\d)\d{2}|0?2/29/((19|[2-9]\d)(0[48]|[2468][048]|[13579][26])|(([2468][048]|[3579][26])00)))$", ErrorMessage = "Please enter a valid date (MM/DD/YYYY)")] 
        public DateTime? CreateUpdateDate { get; set; }

        [Required]
        public double Latitude { get; set; }

        [Required]
        public double Longitude { get; set; }

        // The number of search results a particular search is yielding.
        public int NumberOfSearchResults { get; set; }
    }
}