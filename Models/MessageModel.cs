using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Globalization;
using System.Web.Mvc;
using System.Web.Security;

namespace HLWebRole.Models
{
    public class MessageModel
    {
        public string PartitionKey { get; set; }

        public string RowKey { get; set; }

        public int SenderUserID { get; set; }

        [Required(ErrorMessage = "The Subject line is required.")]
        [DataType(DataType.Text)]
        [StringLength(60, ErrorMessage = "Subject line may not be longer than 60 characters")]
        [DisplayName("Subject:")]
        public string Subject { get; set; }

        [Required(ErrorMessage = "A Message is required.")]
        [DataType(DataType.Text)]
        [StringLength(2000, ErrorMessage = "Message may not be longer than 2000 characters")]
        [DisplayName("Message:")]
        public string MessageBody { get; set; }

        public bool IsRead { get; set; }

        public bool DeletedBySender { get; set; }

        public bool DeletedByReceiver { get; set; }

        public DateTime CreateDate { get; set; }

        public string ReceiverUserName { get; set; }

        [DataType(DataType.ImageUrl)]
        public string ReceiverThumbnailBlobUri { get; set; }
    }
}