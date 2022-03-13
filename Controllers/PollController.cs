using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using HLWebRole.Models;
using HLWebRole.HLServiceReference;
using HLWebRole.Utilities;
using System.Diagnostics;
using System.Globalization;

namespace HLWebRole.Controllers
{
    public class PollController : Controller
    {
        // **************************************
        // GET: /Poll/Create/5
        // **************************************
        [Authorize]
        public ActionResult Create(int id)
        {
            var communityDto = new CommunityDto();
            try
            {
                using (var WS = new HLServiceClient())
                {
                    communityDto = WS.GetCommunity(id);

                    // In case of no material is returned from the request, we render a "Not Found" view.
                    if (communityDto == null)
                    {
                        @ViewBag.Message = "Not Found";
                        TempData["ErrorMessage"] = "Oops! You were trying to add a poll to a community which we couldn't find!";
                        return View("Message");
                    }

                    // Checking whether a user which is not the owner is trying to edit information which they don't own.
                    if (communityDto.AddedByUserID != Convert.ToInt32(Session["userId"]))
                    {
                        @ViewBag.Message = "Invalid Owner";
                        TempData["ErrorMessage"] = "Oops! You were trying to add a poll to a community which you don't own!";
                        return View("Message");
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return View("Message");
            }

            // Just passing some info about the Community that the User is editing a Poll for, just in case they forgot.
            ViewBag.CommunityName = communityDto.Name;
            ViewBag.CommunityPhoto = communityDto.MediumSizeBlobUri;

            var pollModel = new PollModel();
            /* Polls for a certain area are identified by a string, the AreaIdentifier. For the whole country, Regions, 
             * Municipalities, PostalCodes, it's strings like: "DKCountry", "Nordjylland", "Aalborg", "9000". For Polls
             * associated with a particular Community we use an AreaIdentifier where we convert the CommunityID int to string
             * and prepend it with "c_", like "c_4". The HlUtility.ConvertIdIntToAreaIdentifier() method, does this for us. */
            pollModel.AreaIdentifier = HlUtility.ConvertIdIntToAreaIdentifier(id);

            return View(pollModel);
        }


        // **************************************
        // POST: /Poll/Create
        // **************************************
        [HttpPost]
        [Authorize]
        public ActionResult Create(PollModel pollModel)
        {
            // This variable will capture the PollID of the newly created Poll.
            var pollId = 0;
            var userId = Convert.ToInt32(Session["userId"]);

            if (ModelState.IsValid)
            {
                try
                {
                    using (var WS = new HLServiceReference.HLServiceClient())
                    {
                        pollId = WS.CreatePoll
                        (
                            pollModel.QuestionText,
                            pollModel.AreaIdentifier,
                            string.Empty,
                            userId,
                            5
                        );
                    }
                    if (pollId > 0)
                    {
                        TempData["SuccessMessage"] = "Your poll has been created.";
                        return RedirectToAction("Edit", new { id = pollId });
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Sorry the Poll was not created";
                        return View(pollModel);
                    }
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = ex.Message;
                    return View(pollModel);
                }
            }
            else
            {
                return View(pollModel);
            }
        }


        // **************************************
        // GET: /Poll/Edit/5
        // **************************************
        [Authorize]
        public ActionResult Edit(int id)
        {
            var pollDto = new PollDto();
            var pollModel = new PollModel();
            var communityDto = new CommunityDto();

            try
            {
                using (var WS = new HLServiceReference.HLServiceClient())
                {
                    pollDto = WS.GetPoll(id);

                    // In case of no material is returned from the request, we render a "Not Found" view.
                    if (pollDto == null)
                    {
                        @ViewBag.Message = "Not Found";
                        TempData["ErrorMessage"] = "Sorry the requested material cannot be found :-(";
                        return View("Message");
                    }

                    // Checking whether a user which is not the owner is trying to edit information which they don't own.
                    if (pollDto.AddedByUserID != Convert.ToInt32(Session["userId"]))
                    {
                        @ViewBag.Message = "Invalid Owner";
                        TempData["ErrorMessage"] = "Oops! You are trying to edit information which you don't own!";
                        return View("Message");
                    }

                    /* The system supports 5 types of Polls: 1. Country, 2. Region, 3. Municipality, 4. Postal Code, 5. Community*/
                    switch (pollDto.PollTypeID)
                    {
                        case 1:
                            /* IdentityMessage is used in the Edit UI. */
                            pollModel.IdentityMessage = "Poll for Denmark";
                            break;
                        case 2:
                            pollModel.IdentityMessage = "region " + pollDto.UiAreaIdentifier;
                            break;
                        case 3:
                            pollModel.IdentityMessage = "municipality " + pollDto.UiAreaIdentifier;
                            break;
                        case 4:
                            pollModel.IdentityMessage = "postal code " + pollDto.UiAreaIdentifier;
                            break;
                        case 5:
                            communityDto = WS.GetCommunity(HlUtility.ExtractIdFromAreaIdentifier(pollDto.AreaIdentifier));
                            pollModel.IdentityMessage = "community " + communityDto.Name;
                            ViewBag.CommunityName = communityDto.Name;
                            ViewBag.CommunityPhoto = communityDto.MediumSizeBlobUri;
                            break;
                    }
                }

                var pollOptionModels = new List<PollOptionModel>();

                // Transfering the poll options from PollDto to PollModel, that is transfering
                // pollDto.PollOptions to PollOptionModels.
                if (pollDto.PollOptions != null)
                {
                    foreach (var p in pollDto.PollOptions)
                    {
                        pollOptionModels.Add
                        (
                            new PollOptionModel()
                            {
                                PollOptionID = p.PollOptionID,
                                PollID = p.PollID,
                                AddedByUserID = p.AddedByUserID,
                                CreateUpdateDate = p.CreateUpdateDate,
                                OptionText = p.OptionText,
                                Votes = p.Votes
                            }
                        );
                    }
                }
                pollModel.PollID = pollDto.PollID;
                pollModel.AddedByUserID = pollDto.AddedByUserID;
                pollModel.PollTypeID = pollDto.PollTypeID;
                pollModel.AreaIdentifier = pollDto.AreaIdentifier;
                pollModel.QuestionText = pollDto.QuestionText;
                pollModel.IsCurrent = pollDto.IsCurrent;
                pollModel.IsArchived = pollDto.IsArchived;
                pollModel.PollOptions = pollOptionModels;

                return View(pollModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return View();
            }
        }


        // **************************************
        // POST: /Poll/Edit
        // **************************************
        [HttpPost]
        [Authorize]
        public ActionResult Edit(PollModel pollModel)
        {
            var pollUpdated = false;

            if (ModelState.IsValid)
            {
                try
                {
                    using (var WS = new HLServiceReference.HLServiceClient())
                    {
                        pollUpdated = WS.UpdatePoll
                        (
                            pollModel.PollID,
                            pollModel.QuestionText,
                            pollModel.IsCurrent,
                            pollModel.IsArchived
                        );
                    }
                    TempData["SuccessMessage"] = "Your poll: \"" + pollModel.QuestionText + "\" has been saved.";

                    switch (pollModel.PollTypeID)
                    {
                        case 1:
                            return RedirectToAction("Danmark", "Poll");
                        case 2:
                            return RedirectToAction("Region", "Poll", new { id = pollModel.AreaIdentifier });
                        case 3:
                            return RedirectToAction("Municipality", "Poll", new { id = pollModel.AreaIdentifier });
                        case 4:
                            return RedirectToAction("PostalCode", "Poll", new { id = pollModel.AreaIdentifier });
                    }
                    return RedirectToAction("Community", "Poll", new { id = HlUtility.ExtractIdFromAreaIdentifier(pollModel.AreaIdentifier) });
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = ex.Message;
                    pollModel.PollOptions = GetPollOptions(pollModel.PollID);
                    return View(pollModel);
                }
            }
            else
            {
                pollModel.PollOptions = GetPollOptions(pollModel.PollID);
                return View(pollModel);
            }
        }


        // ************************************************
        // POST AJAX: /Poll/Delete
        // ************************************************
        [HttpPost]
        [Authorize]
        public JsonResult Delete(int pollId)
        {
            var status = false;

            try
            {
                using (var WS = new HLServiceReference.HLServiceClient())
                {
                    status = WS.DeletePoll(pollId);

                }
                if (status)
                {
                    return Json
                    (
                        status
                    );
                }
                else
                {
                    return Json
                    (
                        new SystemMessageModel()
                        {
                            SystemMessage = "Sorry the Poll option was Not Deleted :-("
                        }
                    );
                }
            }
            catch (Exception ex)
            {
                return Json
                (
                    new SystemMessageModel()
                    {
                        SystemMessage = ex.Message
                    }
                );
            }
        }


        // **********************************************************
        // POST AJAX: /Poll/CreatePollOption
        // **********************************************************
        [HttpPost]
        [Authorize]
        public JsonResult CreatePollOption(int pollId, string optionText)
        {
            var pollOptionId = 0;
            var userId = Convert.ToInt32(Session["userId"]);
            var pollOptionModel = new PollOptionModel();

            try
            {
                using (var WS = new HLServiceReference.HLServiceClient())
                {
                    pollOptionId = WS.CreatePollOption(pollId, userId, optionText);
                }
                if (pollOptionId > 0)
                {
                    return Json
                    (
                        new PollOptionModel()
                        {
                            PollOptionID = pollOptionId,
                            OptionText = optionText
                        }
                    );
                }
                else
                {
                    return Json
                    (
                        new SystemMessageModel()
                        {
                            SystemMessage = "Sorry the poll option was not added:-("
                        }
                    );
                }
            }
            catch (Exception ex)
            {
                return Json
                (
                    new SystemMessageModel()
                    {
                        SystemMessage = ex.Message
                    }
                );
            }
        }


        // **************************************************************
        // POST AJAX: /Poll/EditPollOption
        // **************************************************************
        [HttpPost]
        [Authorize]
        public JsonResult EditPollOption(int pollOptionId, string optionText)
        {
            var pollOptionUpdated = false;

            try
            {
                using (var WS = new HLServiceReference.HLServiceClient())
                {
                    pollOptionUpdated = WS.UpdatePollOption(pollOptionId, optionText);
                }

                if (pollOptionUpdated)
                {
                    return Json
                    (
                        new PollOptionModel()
                        {
                            PollOptionID = pollOptionId,
                            OptionText = optionText
                        }
                    );
                }
                else
                {
                    return Json
                    (
                        new SystemMessageModel()
                        {
                            SystemMessage = "Sorry the poll option was not updated :-("
                        }
                    );
                }
            }
            catch (Exception ex)
            {
                return Json
                (
                    new SystemMessageModel()
                    {
                        SystemMessage = ex.Message
                    }
                );
            }
        }


        // ************************************************
        // POST AJAX: /Poll/DeletePollOption
        // ************************************************
        [HttpPost]
        [Authorize]
        public JsonResult DeletePollOption(int pollOptionId)
        {
            var pollOptionDeleted = false;

            try
            {
                using (var WS = new HLServiceReference.HLServiceClient())
                {
                    pollOptionDeleted = WS.DeletePollOption(pollOptionId);

                }
                if (pollOptionDeleted)
                {
                    return Json
                    (
                        new PollOptionModel()
                        {
                            PollOptionID = pollOptionId,
                        }
                    );
                }
                else
                {
                    return Json
                    (
                        new SystemMessageModel()
                        {
                            SystemMessage = "Sorry the poll option was not deleted :-("
                        }
                    );
                }
            }
            catch (Exception ex)
            {
                return Json
                (
                    new SystemMessageModel()
                    {
                        SystemMessage = ex.Message
                    }
                );
            }
        }


        // **************************************
        // POST AJAX: /Poll/VoteCommunityPoll
        // **************************************
        [Authorize]
        [HttpPost]
        public JsonResult VoteCommunityPoll(int optionId)
        {
            var pollDto = new PollDto();

            try
            {
                using (var WS = new HLServiceReference.HLServiceClient())
                {
                    pollDto = WS.VoteCommunityPoll(optionId, Convert.ToInt32(Session["userId"]));
                }
                return Json(pollDto);
            }
            catch (Exception ex)
            {
                return Json
                (
                    new SystemMessageModel()
                    {
                        SystemMessage = ex.Message
                    }
                );
            }
        }


        // **************************************
        // POST AJAX: /Poll/VoteAnonymousPoll
        // **************************************
        [HttpPost]
        public JsonResult VoteAnonymousPoll(int optionId)
        {
            var pollDto = new PollDto();

            try
            {
                using (var WS = new HLServiceReference.HLServiceClient())
                {
                    pollDto = WS.VoteAnonymousPoll(optionId);
                }
                return Json(pollDto);
            }
            catch (Exception ex)
            {
                return Json
                (
                    new SystemMessageModel()
                    {
                        SystemMessage = ex.Message
                    }
                );
            }
        }


        // **************************************
        // GET: /Poll/Danmark/2
        // **************************************
        public ActionResult Danmark(int? id)
        {
            var idInt = HlUtility.ConvertNullableIntToPositiveInt(id);

            try
            {
                using (var WS = new HLServiceClient())
                {
                    var polls = WS.GetPollsForArea("Country", false, 4, idInt);

                    /* These settings are used by the _Pager partial view. */
                    if (polls.Length > 0)
                    {
                        ViewBag.PagerType = "OneParameterActionMethod";
                        ViewBag.ListObjectName = "Polls";
                        ViewBag.CurrentPage = idInt;
                        ViewBag.HasNextPageOfData = polls[0].HasNextPageOfData;
                        ViewBag.Controller = "Poll";
                        ViewBag.Action = "Danmark";
                    }
                    return View(polls);
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return View("Message");
            }
        }


        // **************************************
        // GET: /Poll/Region/Sjaelland/2
        // **************************************
        public ActionResult Region(string id, int? pageNumber)
        {
            var pageNumberInt = HlUtility.ConvertNullableIntToPositiveInt(pageNumber);

            try
            {
                using (var WS = new HLServiceClient())
                {
                    var polls = WS.GetPollsForArea(id, false, 4, pageNumberInt);

                    /* These settings are used by the _Pager partial view. */
                    ViewBag.AreaIdentifier = id;
                    if (polls.Length > 0)
                    {
                        ViewBag.PagerType = "TwoParametersActionMethod";
                        ViewBag.ListObjectName = "Polls";                       
                        ViewBag.CurrentPage = pageNumberInt;
                        ViewBag.HasNextPageOfData = polls[0].HasNextPageOfData;
                        ViewBag.UiAreaIdentifier = polls[0].UiAreaIdentifier;
                        ViewBag.Controller = "Poll";
                        ViewBag.Action = "Region";
                    }
                    return View(polls);
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return View("Message");
            }
        }


        // **************************************
        // GET: /Poll/Municipality/Koebenhavn/2
        // **************************************
        public ActionResult Municipality(string id, int? pageNumber)
        {
            var pageNumberInt = HlUtility.ConvertNullableIntToPositiveInt(pageNumber);

            try
            {
                using (var WS = new HLServiceClient())
                {
                    var polls = WS.GetPollsForArea(id, false, 4, pageNumberInt);

                    /* These settings are used by the _Pager partial view. */
                    ViewBag.AreaIdentifier = id;
                    if (polls.Length > 0)
                    {
                        ViewBag.PagerType = "TwoParametersActionMethod";
                        ViewBag.ListObjectName = "Polls";
                        ViewBag.CurrentPage = pageNumberInt;
                        ViewBag.HasNextPageOfData = polls[0].HasNextPageOfData;
                        ViewBag.UiAreaIdentifier = polls[0].UiAreaIdentifier;
                        ViewBag.Controller = "Poll";
                        ViewBag.Action = "Municipality";
                    }
                    return View(polls);
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return View("Message");
            }
        }


        // **************************************
        // GET: /Poll/PostalCode/2400/2
        // **************************************
        public ActionResult PostalCode(string id, int? pageNumber)
        {
            var pageNumberInt = HlUtility.ConvertNullableIntToPositiveInt(pageNumber);

            try
            {
                using (var WS = new HLServiceClient())
                {
                    var polls = WS.GetPollsForArea(id, false, 4, pageNumberInt);

                    /* These settings are used by the _Pager partial view. */
                    ViewBag.AreaIdentifier = id;
                    if (polls.Length > 0)
                    {
                        ViewBag.PagerType = "TwoParametersActionMethod";
                        ViewBag.ListObjectName = "Polls";
                        ViewBag.CurrentPage = pageNumberInt;
                        ViewBag.HasNextPageOfData = polls[0].HasNextPageOfData;
                        ViewBag.UiAreaIdentifier = polls[0].UiAreaIdentifier;
                        ViewBag.Controller = "Poll";
                        ViewBag.Action = "PostalCode";
                    }
                    return View(polls);
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return View("Message");
            }
        }


        // **************************************
        // GET: /Poll/Community/5/2
        // **************************************
        [Authorize]
        public ActionResult Community(int id, int? pageNumber)
        {
            var pageNumberInt = HlUtility.ConvertNullableIntToPositiveInt(pageNumber);

            try
            {
                using (var WS = new HLServiceClient())
                {
                    /* The UserID of the logged in User. */
                    var userId = (int)Session["userId"];

                    /* Making sure that we only display CommunityPoll to Users who live 
                     * in the Community area, or to the creater of the Community. */
                    if (WS.IsUserLivingInCommunityArea(userId, id) || WS.IsCommunityCreatedByUser(userId, id))
                    {
                        var polls = WS.GetPollsForCommunity(id, userId, false, 4, pageNumberInt);

                        /* These settings are used by the _Pager partial view. */
                        if (polls.Length > 0)
                        {
                            ViewBag.PagerType = "TwoParametersActionMethod";
                            ViewBag.ListObjectName = "Polls";
                            ViewBag.AreaIdentifier = id;
                            ViewBag.CurrentPage = pageNumberInt;
                            ViewBag.HasNextPageOfData = polls[0].HasNextPageOfData;
                            ViewBag.Controller = "Poll";
                            ViewBag.Action = "Community";
                        }

                        // Getting some info about the Community that the Polls belongs to.
                        var communityDto = WS.GetCommunity(id);
                        ViewBag.CommunityDto = communityDto;

                        return View(polls);
                    }
                    else
                    {
                        @ViewBag.Message = "Not Allowed";
                        TempData["ErrorMessage"] = "Sorry pal, you are not allowed to see this information";
                        return View("Message");
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return View("Message");
            }
        }


        /// <summary>
        /// This is a helper method being used by the action methods above. It returns the poll options for a poll.
        /// </summary>
        public List<PollOptionModel> GetPollOptions(int pollId)
        {
            var pollOptionDtos = new List<PollOptionDto>();

            try
            {
                using (var WS = new HLServiceReference.HLServiceClient())
                {
                    pollOptionDtos = WS.GetPollOptions(pollId).ToList();
                }
                var pollOptionModels = new List<PollOptionModel>();

                // Transfering the poll options from PollOptionDto to PollOptionModel.
                if (pollOptionDtos != null)
                {
                    foreach (var p in pollOptionDtos)
                    {
                        pollOptionModels.Add
                        (
                            new PollOptionModel()
                            {
                                PollOptionID = p.PollOptionID,
                                PollID = p.PollID,
                                AddedByUserID = p.AddedByUserID,
                                CreateUpdateDate = p.CreateUpdateDate,
                                OptionText = p.OptionText,
                                Votes = p.Votes
                            }
                        );
                    }
                }
                return pollOptionModels;
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return null;
            }
        }


        #region Editor view methods

        /// <summary>
        /// Contains data for the jQuery autocomplete textboxes in the "Editor" view.
        /// </summary>
        string[] autoCompleteTextBoxData = null;


        // **************************************
        // GET: /Poll/Editor
        // **************************************
        [Authorize(Roles = "Editor")]
        public ActionResult Editor()
        {
            try
            {
                /* Model for Poll creation */
                var createModel = new CreatePollModel();
                /* Model for going to manage Polls */
                var manageModel = new ManagePollModel();

                using (var WS = new HLServiceClient())
                {
                    /* Generate drop down list for the different Poll types: County, Region, Municipality and Postal Code. */
                    var pollTypeDtos = WS.GetAnonymousPollTypes();
                    IEnumerable<SelectListItem> lstPollType = pollTypeDtos.Select(p => new SelectListItem
                    {
                        Value = p.Name,
                        Text = p.Name
                    });
                    createModel.PollSelectListCreate = lstPollType;
                    manageModel.PollSelectListManage = lstPollType;

                    ViewBag.ManageModel = manageModel;
                }
                return View(createModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return View();
            }
        }


        // **************************************************************
        // POST AJAX: /Poll/PollType
        // **************************************************************      
        /// <summary>
        /// This method is used in conjunction with the autocomplete textbox in the Editor view, it sets the PollType
        /// which can be "Country", "Region", "Municipality" or "PostalCode".
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Editor")]
        public void PollType(string selectedValue)
        {
            Session["PollType"] = selectedValue;
        }


        // **************************************************************
        // GET: /Poll/AreaIdentifiers
        // **************************************************************
        /// <summary>
        /// This action method is used by the jQuery autocomplete textboxes in the Editor view.
        /// </summary>
        /// <param name="term">This is what is typed in into the textbox.</param>
        /// <returns>string[] autoCompleteTextBoxData, this can either be Regions, Municipalities or PostalCodes</returns>
        [Authorize(Roles = "Editor")]
        public ActionResult AreaIdentifiers(string term)
        {
            string pollType = (string)Session["PollType"];

            /* Depending on the Poll type do we go and fetch different data for the autocomplete textbox
             Regions, Municipalities or PostalCodes. */
            switch (pollType)
            {
                case "Region":
                    autoCompleteTextBoxData = GetAllRegions();
                    break;
                case "Municipality":
                    autoCompleteTextBoxData = GetAllMunicipalities();
                    break;
                case "Postal Code":
                    autoCompleteTextBoxData = GetAllPostalCodes();
                    break;
                default:
                    autoCompleteTextBoxData = new string[1] { "" };
                    break;
            }

            if (String.IsNullOrWhiteSpace(term))
                return Json(autoCompleteTextBoxData, JsonRequestBehavior.AllowGet);

            return Json(autoCompleteTextBoxData.Where(s => s.StartsWith(term, true, CultureInfo.CurrentCulture)),
                JsonRequestBehavior.AllowGet);
        }


        // **************************************
        // POST: /Poll/CountryCreate
        // **************************************
        [HttpPost]
        [Authorize(Roles = "Editor")]
        public ActionResult CountryCreate(CreatePollModel model)
        {
            model.PolltypeID = 1;
            model.AreaIdentifier = "Country";
            model.AreaIdentifierCreate = "Danmark";
            return RedirectToAction("CreateAnonymousPoll", model);
        }


        // **************************************
        // POST: /Poll/RegionCreate
        // **************************************
        [HttpPost]
        [Authorize(Roles = "Editor")]
        public ActionResult RegionCreate(CreatePollModel model)
        {
            try
            {
                using (var WS = new HLServiceReference.HLServiceClient())
                {
                    var regionDto = WS.GetUrlRegionName(model.AreaIdentifierCreate);

                    if (regionDto.UrlRegionName != null)
                    {
                        model.PolltypeID = 2;
                        model.AreaIdentifier = regionDto.UrlRegionName;
                        return RedirectToAction("CreateAnonymousPoll", model);
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Sorry, but we couldn't find that Region";
                        return View("Message");
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return View("Message");
            }
        }


        // **************************************
        // POST: /Poll/MunicipalityCreate
        // **************************************
        [HttpPost]
        [Authorize(Roles = "Editor")]
        public ActionResult MunicipalityCreate(CreatePollModel model)
        {
            try
            {
                using (var WS = new HLServiceReference.HLServiceClient())
                {
                    var municipalityDto = WS.GetUrlMunicipalityName(model.AreaIdentifierCreate);

                    if (municipalityDto.UrlMunicipalityName != null)
                    {
                        model.PolltypeID = 3;
                        model.AreaIdentifier = municipalityDto.UrlMunicipalityName;
                        return RedirectToAction("CreateAnonymousPoll", model);
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Sorry, but we couldn't find Municipality: " + model.AreaIdentifierCreate;
                        return View("Message");
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return View("Message");
            }
        }


        // **************************************
        // POST: /Poll/PostalCodeCreate
        // **************************************
        [HttpPost]
        [Authorize(Roles = "Editor")]
        public ActionResult PostalCodeCreate(CreatePollModel model)
        {
            /* PostalCode area identifier comes in the form of {POSTNR_TXT} {POSTBYNAVN}
            Like "2920 Charlottenlund", "3400 Hillerød" or "1000-1499 København K".
            In the code below we cut out the PostalCode which is used as URL
            area identifier, and get values like "2920", "3400" or "1000-1499". */
            var postalCodePostalTown = new string[2];
            postalCodePostalTown = model.AreaIdentifierCreate.Split(' ');
            var urlAreaIdentifier = postalCodePostalTown[0];

            model.AreaIdentifier = urlAreaIdentifier;
            model.PolltypeID = 4;
            return RedirectToAction("CreateAnonymousPoll", model);
        }


        // **************************************
        // POST: /Poll/CountryManage
        // **************************************
        [HttpPost]
        [Authorize(Roles = "Editor")]
        public ActionResult CountryManage(ManagePollModel manageModel)
        {
            return RedirectToAction("Danmark", "Poll");
        }


        // **************************************
        // POST: /Poll/RegionManage
        // **************************************
        [HttpPost]
        [Authorize(Roles = "Editor")]
        public ActionResult RegionManage(ManagePollModel manageModel)
        {
            try
            {
                using (var WS = new HLServiceReference.HLServiceClient())
                {
                    var regionDto = WS.GetUrlRegionName(manageModel.AreaIdentifierManage);

                    if (regionDto.UrlRegionName != null)
                    {
                        return RedirectToAction("Region", "Poll", new { id = regionDto.UrlRegionName });
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Sorry, but we couldn't find that Region";
                        return View("Message");
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return View("Message");
            }
        }


        // **************************************
        // POST: /Poll/MunicipalityManage
        // **************************************
        [HttpPost]
        [Authorize(Roles = "Editor")]
        public ActionResult MunicipalityManage(ManagePollModel manageModel)
        {
            try
            {
                using (var WS = new HLServiceReference.HLServiceClient())
                {
                    var municipalityDto = WS.GetUrlMunicipalityName(manageModel.AreaIdentifierManage);

                    if (municipalityDto.UrlMunicipalityName != null)
                    {
                        return RedirectToAction("Municipality", "Poll", new { id = municipalityDto.UrlMunicipalityName });
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Sorry, but we couldn't find that Municipality";
                        return View("Message");
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return View("Message");
            }
        }


        // **************************************
        // POST: /Poll/PostalCodeManage
        // **************************************
        [HttpPost]
        [Authorize(Roles = "Editor")]
        public ActionResult PostalCodeManage(ManagePollModel manageModel)
        {
            /* PostalCode area identifier comes in the form of {POSTNR_TXT} {POSTBYNAVN}
            Like "2920 Charlottenlund", "3400 Hillerød" or "1000-1499 København K".
            In the code below we cut out the PostalCode which is used as URL
            area identifier, and get values like "2920", "3400" or "1000-1499". */
            var postalCodePostalTown = new string[2];
            postalCodePostalTown = manageModel.AreaIdentifierManage.Split(' ');
            var urlAreaIdentifier = postalCodePostalTown[0];
            return RedirectToAction("PostalCode", "Poll", new { id = urlAreaIdentifier });
        }


        [Authorize(Roles = "Editor")]
        public ActionResult CreateAnonymousPoll(CreatePollModel model)
        {
            // This variable will capture the PollID of the newly created Poll.
            var pollId = 0;
            var userId = Convert.ToInt32(Session["userId"]);

            try
            {
                using (var WS = new HLServiceReference.HLServiceClient())
                {
                    pollId = WS.CreatePoll
                    (
                        model.QuestionText,
                        model.AreaIdentifier,
                        model.AreaIdentifierCreate,
                        userId,
                        model.PolltypeID
                    );
                }
                if (pollId > 0)
                {
                    TempData["SuccessMessage"] = "Your poll has been created.";
                    return RedirectToAction("Edit", new { id = pollId });
                }
                else
                {
                    TempData["ErrorMessage"] = "Sorry the Poll was not created";
                    return View("Message");
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return View("Message");
            }
        }


        /// <summary>
        /// Used by the autocomplete text box in the Editor view.
        /// </summary>
        private string[] GetAllRegions()
        {
            try
            {
                using (var WS = new HLServiceClient())
                {
                    var areas = WS.GetAllRegions();
                    var count = areas.Count();
                    string[] areaNames = new string[count];

                    int index = 0;
                    foreach (var a in areas)
                    {
                        areaNames[index] = a.REGIONNAVN;
                        index++;
                    }
                    return areaNames;
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError("Problem in Poll.GetAllRegions() " + ex.ToString());
                return null;
            }
        }


        /// <summary>
        /// Used by the autocomplete text box in the Editor view.
        /// </summary>
        private string[] GetAllMunicipalities()
        {
            try
            {
                using (var WS = new HLServiceClient())
                {
                    var areas = WS.GetAllMunicipalities();
                    var count = areas.Count();
                    string[] areaNames = new string[count];

                    int index = 0;
                    foreach (var a in areas)
                    {
                        areaNames[index] = a.KOMNAVN;
                        index++;
                    }
                    return areaNames;
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError("Problem in Poll.GetAllMunicipalities() " + ex.ToString());
                return null;
            }
        }


        /// <summary>
        /// Used by the autocomplete text box in the Editor view.
        /// </summary>
        private string[] GetAllPostalCodes()
        {
            try
            {
                using (var WS = new HLServiceClient())
                {
                    var areas = WS.GetAllPostalCodes();
                    var count = areas.Count();
                    string[] areaNames = new string[count];

                    int index = 0;
                    foreach (var a in areas)
                    {
                        areaNames[index] = a.POSTNR_TXT + " " + a.POSTBYNAVN;
                        index++;
                    }
                    return areaNames;
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError("Problem in Poll.GetAllPostalCodes(): " + ex.ToString());
                return null;
            }
        }

        #endregion
    }
}
