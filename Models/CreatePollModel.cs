using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Web.Mvc;

namespace HLWebRole.Models
{
    /// <summary>
    /// This model is used by Users in the Editor role when they create AnonymousPolls (Polls where access to partake in the Poll is determined by a cookie
    /// which the User receives upon voting). The Editors can create, apart from CommunityPolls (Polls where only the poll creator or logged in Users living in
    /// the Community area can vote), the Editors can create Polls on Country level, Region level, Municipality level and PostalCode level.
    /// </summary>
    public class CreatePollModel
    {
        /// <summary>
        /// The three fields below together makes up the PollSelectList, this determines what kind of Poll will be created:
        /// Country, Region, Municipality or Postal Code.
        /// </summary>
        [Required(ErrorMessage = "Selection of Poll Type is required.")]
        [DisplayName("Please Select a Poll Type:")]
        public string TypeIDCreate { get; set; }

        public string NameCreate { get; set; }

        public IEnumerable<SelectListItem> PollSelectListCreate { get; set; }

        [Required(ErrorMessage = "The Poll Question field is required.")]
        [DataType(DataType.Text)]
        [StringLength(50, ErrorMessage = "Question may not be longer than 90 characters")]
        [DisplayName("Poll Question:")]
        public string QuestionText { get; set; }

        /// <summary>
        /// The AreaIdentifier is used with the jQuery autocomplete textbox. Depending on what is chosen in the PollSelectList. It can be
        /// a Region, a Municipality or a PostalCode.
        /// </summary>
        [Required(ErrorMessage = "Unless you create a Poll for Country, an area identfier is required")]
        [DisplayName("Type in the Area (Region, Municipality or Postal Code):")]
        public string AreaIdentifierCreate { get; set; }

        /// <summary>
        /// This carries the UrlName of the area identifier, its a version of the area identifier which does not disturb the browser.
        /// E.g. "Hillerød" becomes "Hilleroed", "Sjælland" becomes "Sjaelland" a.s.o.
        /// </summary>
        public string AreaIdentifier { get; set; }

        /// <summary>
        /// The system supports 5 types of Polls: 
        /// 1. Country, 2. Region, 3. Municipality, 4. Postal Code, 5. Community
        /// </summary>
        public int PolltypeID { get; set; }
    }
}