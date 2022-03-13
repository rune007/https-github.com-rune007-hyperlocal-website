using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Globalization;
using System.Web.Mvc;
using System.Web.Security;

namespace HLWebRole.Models
{
    public class CommentModel
    {
        public string PartitionKey { get; set; }

        public string RowKey { get; set; }

        public int PostedByUserID { get; set; }

        public string PostedByUserName { get; set; }

        [Required]
        [DataType(DataType.Text)]
        [StringLength(500, ErrorMessage = "Comment may not be longer than 500 characters")]
        [DisplayName("Comment")]
        public string CommentBody { get; set; }

        public string CreateDate { get; set; }

        public string MediumSizeBlobUri { get; set; }

        public string ThumbnailBlobUri { get; set; }
    }
}