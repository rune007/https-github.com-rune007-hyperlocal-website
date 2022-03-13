using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace HLWebRole.Models
{
    public class PollModel
    {
        public int PollID { get; set; }

        public int AddedByUserID { get; set; }

        /// <summary>
        /// The system supports 5 types of Polls: 
        /// 1. Country, 2. Region, 3. Municipality, 4. Postal Code, 5. Community
        /// </summary>
        public int PollTypeID { get; set; }

        public DateTime CreateUpdateDate { get; set; }

        public string AreaIdentifier { get; set; }

        [Required(ErrorMessage = "The Poll Question field is required.")]
        [DataType(DataType.Text)]
        [StringLength(50, ErrorMessage = "Question may not be longer than 90 characters")]
        [DisplayName("Poll Question:")]
        public string QuestionText { get; set; }

        [DisplayName("Make Current")]
        public bool IsCurrent { get; set; }

        [DisplayName("Send To Archive")]
        public bool IsArchived { get; set; }

        public DateTime ArchivedDate { get; set; }

        public List<PollOptionModel> PollOptions { get; set; }

        /// <summary>
        /// A fragment of a message shown in the Poll/Edit view
        /// Things like: "Poll for Denmark", "municipality Hillerød", "postal code 2700 Brønshøj", etc.
        /// </summary>
        public string IdentityMessage { get; set; }
    }
}