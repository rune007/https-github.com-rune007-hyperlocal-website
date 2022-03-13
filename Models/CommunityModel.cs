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
    public class CommunityModel
    {
        public int CommunityID { get; set; }

        public int AddedByUserID { get; set; }

        [Required]
        [DataType(DataType.Text)]
        [StringLength(60, ErrorMessage = "Name may not be longer than 60 characters")]
        [Display(Name = "Name")]
        public string Name { get; set; }

        [Required]
        [DataType(DataType.Text)]
        [StringLength(400, ErrorMessage = "Description may not be longer than 400 characters")]
        [Display(Name = "Description")]
        public string Description { get; set; }

        // Actually the three fields below: "PolygonWkt", "PolygonWktB" and "PolygonWktC" are all the same field.
        // But in order to use the three [Remote] validation attributes:
        // [Remote("IsPolygonWithinDenmark", "SpatialQuery", HttpMethod = "POST")]
        // [Remote("IsPolygonValid", "SpatialQuery", HttpMethod = "POST")]
        // [Remote("IsPolygonAreaTooBig", "SpatialQuery", HttpMethod = "POST")]
        // I needed to call the same field, "PolygonWkt", something different. Originally I had decorated the one field "PolygonWkt" 
        // with all the three [Remote] validation attributes, but this was seen as an error by Visual Studio.
        // Therefore I had to split up the three [Remote] attributes on three fields.
        [Required( ErrorMessage= "You need to have a polygon for your Community!")]
        [Remote("IsPolygonWithinDenmark", "SpatialQuery", HttpMethod = "POST")]
        public string PolygonWkt { get; set; }

        [Remote("IsPolygonValid", "SpatialQuery", HttpMethod = "POST")]
        public string PolygonWktB { get; set; }

        [Remote("IsPolygonAreaTooBig", "SpatialQuery", HttpMethod = "POST")]
        public string PolygonWktC { get; set; }

        public DateTime CreateUpdateDate { get; set; }

        public string ImageBlobUri { get; set; }

        public string MediumSizeBlobUri { get; set; }

        public string ThumbnailBlobUri { get; set; }

        public int NumberOfUsersInCommunity { get; set; }
    }
}