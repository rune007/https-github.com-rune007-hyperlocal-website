using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Web.Mvc;

namespace HLWebRole.Models
{
    public class CreateGeoTemporalAssignmentModel
    {
        public int AssignmentID { get; set; }

        [Required(ErrorMessage = "The Title field is required.")]
        [DataType(DataType.Text)]
        [StringLength(75, ErrorMessage = "Title may not be longer than 50 characters")]
        [DisplayName("Title:")]
        public string Title { get; set; }

        [Required(ErrorMessage = "The Description field is required.")]
        [DataType(DataType.Text)]
        [StringLength(1400, ErrorMessage = "Description may not be longer than 1400 characters")]
        [DisplayName("Description:")]
        public string Description { get; set; }

        [Required(ErrorMessage = "The Expiry Date field is required.")]
        [DataType(DataType.Date)]
        [DisplayName("Expiry Date:")]
        // Regular Expression matching date in format (MM/DD/YYYY)
        [RegularExpression(@"^(((0?[1-9]|1[012])/(0?[1-9]|1\d|2[0-8])|(0?[13456789]|1[012])/(29|30)|(0?[13578]|1[02])/31)/(19|[2-9]\d)\d{2}|0?2/29/((19|[2-9]\d)(0[48]|[2468][048]|[13579][26])|(([2468][048]|[3579][26])00)))$", ErrorMessage = "Please enter a valid date (MM/DD/YYYY)")]
        public DateTime ExpiryDate { get; set; }

        [DisplayName("Type Location to Center Geo Assignment (You can drag Pushpin):")]
        [Required(ErrorMessage = "A Center Location is required for Geo Assignment!")]
        public string Address { get; set; }

        [DisplayName("Assignment Radius")]
        [RegularExpression(@"^[1-9]\d*$", ErrorMessage = "That is not a positive number!")]
        [Range(1, 40, ErrorMessage = "We don't allow Assignment Radius greater than 40 km.")]
        [Required(ErrorMessage = "An Assignment Radius is required for Geo Assignment!")]
        public int AssignmentRadius { get; set; }

        /// <summary>
        /// The number of hours to go back when looking for Users who have logged in within the 
        /// Assignment area.
        /// </summary>
        [DisplayName("# Hours to go back")]
        [RegularExpression(@"^[1-9]\d*$", ErrorMessage = "That is not a positive number!")]
        [Required(ErrorMessage = "# Hours to go back is required for Geo Assignment!")]
        public int HoursToGoBack { get; set; }

        /// <summary>
        /// Assignment center latitude
        /// </summary>
        [Required]
        [DataType(DataType.Text)]
        [DisplayName("Latitude")]
        [Remote("IsLatLongWithinDenmark", "SpatialQuery", AdditionalFields = "Longitude", HttpMethod = "POST")]
        public double Latitude { get; set; }

        /// <summary>
        /// Assignment center longitude
        /// </summary>
        [Required]
        [DataType(DataType.Text)]
        [DisplayName("Longitude")]
        public double Longitude { get; set; }
    }
}