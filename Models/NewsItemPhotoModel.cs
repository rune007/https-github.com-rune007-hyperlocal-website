using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace HLWebRole.Models
{
    public class NewsItemPhotoModel
    {
        public int MediaID { get; set; }

        public int NewsItemID { get; set; }

        public string Caption { get; set; }

        public string BlobUri { get; set; }

        public string MediumSizeBlobUri { get; set; }

        public string ThumbnailBlobUri { get; set; }
    }
}