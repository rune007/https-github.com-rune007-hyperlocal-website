using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Web.Mvc;

namespace HLWebRole.Models
{
    public class AssignmentModel
    {
        public int AssignmentID { get; set; }

        public int AddedByUserID { get; set; }

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

        public DateTime CreateUpdateDate { get; set; }

        //[Required(ErrorMessage = "The Expiry Date field is required.")]
        [DataType(DataType.Date)]
        [DisplayName("Expiry Date:")]
        public DateTime ExpiryDate { get; set; }

        public string ImageBlobUri { get; set; }

        public string MediumSizeBlobUri { get; set; }

        public string ThumbnailBlobUri { get; set; }
    }
}