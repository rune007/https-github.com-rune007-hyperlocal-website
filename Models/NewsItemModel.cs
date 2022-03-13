using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Web.Mvc;

namespace HLWebRole.Models
{
    public class NewsItemModel
    {
        public int NewsItemID { get; set; }

        public int PostedByUserID { get; set; }

        public string PostedByUserName { get; set; }

        [Required(ErrorMessage = "Selection of Category is required.")]
        [DisplayName("Please Select a Category:")]
        public int CategoryID { get; set; }

        public string CategoryName { get; set; }

        public IEnumerable<SelectListItem> CategorySelectList { get; set; }

        [DisplayName("Associate your news with an Assignment (not required)")]
        public int? AssignmentID { get; set; }

        public string AssignmentTitle { get; set; }

        public IEnumerable<SelectListItem> AssignmentSelectList { get; set; }

        [Required(ErrorMessage = "The Title field is required.")]
        [DataType(DataType.Text)]
        [StringLength(50, ErrorMessage = "Title may not be longer than 50 characters")]
        [DisplayName("Title:")]
        public string Title { get; set; }

        [Required(ErrorMessage = "The Story field is required.")]
        [DataType(DataType.Text)]
        [StringLength(1400, ErrorMessage = "Story may not be longer than 1400 characters")]
        [DisplayName("Story:")]
        public string Story { get; set; }

        [DisplayName("Type Location to Plot Map (You can drag Pushpin):")]
        public string Address { get; set; }

        public string PositionPointWkt { get; set; }

        [Remote("IsLatLongWithinDenmark", "SpatialQuery", AdditionalFields = "Longitude", HttpMethod = "POST")]
        public double Latitude { get; set; }

        public double Longitude { get; set; }

        public DateTime CreateUpdateDate { get; set; }

        [DisplayName("Post the News as Breaking News:")]
        public bool IsLocalBreakingNews { get; set; }

        public int NumberOfViews { get; set; }

        public int NumberOfComments { get; set; }

        public int NumberOfShares { get; set; }

        public List<NewsItemPhotoModel> Photos { get; set; }

        public List<NewsItemVideoModel> Videos { get; set; }

        [DataType(DataType.ImageUrl)]
        public string CoverPhotoLarge { get; set; }
    }
}