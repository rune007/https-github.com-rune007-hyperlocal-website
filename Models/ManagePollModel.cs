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
    public class ManagePollModel
    {
        /// <summary>
        /// The three fields below together makes up the PollSelectList, this determines what kind of Poll will be created:
        /// Country, Region, Municipality or Postal Code.
        /// </summary>
        [Required(ErrorMessage = "Selection of Poll Type is required.")]
        [DisplayName("Please Select a Poll Type:")]
        public string TypeIDManage { get; set; }

        public string NameManage { get; set; }

        public IEnumerable<SelectListItem> PollSelectListManage { get; set; }

        /// <summary>
        /// The AreaIdentifier is used with the jQuery autocomplete textbox. Depending on what is chosen in the PollSelectList. It can be
        /// a Region, a Municipality or a PostalCode.
        /// </summary>
        [Required(ErrorMessage = "Except for Country Poll, an area identfier is required")]
        [DisplayName("Type in the Area (Region, Municipality or Postal Code):")]
        public string AreaIdentifierManage { get; set; }
    }
}