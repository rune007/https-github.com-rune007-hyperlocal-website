using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace HLWebRole.Models
{
    public class NewsItemVideoModel
    {
        public int MediaID { get; set; }

        public int NewsItemID { get; set; }

        public string Title { get; set; }

        public string BlobUri { get; set; }
    }
}