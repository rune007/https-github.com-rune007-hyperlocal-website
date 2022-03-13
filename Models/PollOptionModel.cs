using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace HLWebRole.Models
{
    public class PollOptionModel
    {
        public int PollOptionID { get; set; }

        public int PollID { get; set; }

        public int AddedByUserID { get; set; }

        public DateTime CreateUpdateDate { get; set; }

        [Required]
        public string OptionText { get; set; }

        public int Votes { get; set; }
    }
}