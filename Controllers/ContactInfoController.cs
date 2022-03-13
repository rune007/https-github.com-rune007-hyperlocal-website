using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;
using HLWebRole.Utilities;
using Microsoft.Web.Helpers;
using System.Web.Security;
using HLWebRole.Models;
using HLWebRole.HLServiceReference;

namespace HLWebRole.Controllers
{
    public class ContactInfoController : Controller
    {
        // *******************************************
        // POST AJAX: /ContactInfo/RequestContactInfo
        // *******************************************
        [HttpPost]
        [Authorize]
        public JsonResult RequestContactInfo(int targetUserId)
        {
            var userId = (int)Session["userId"];
            var status = false;

            try
            {
                using (var WS = new HLServiceReference.HLServiceClient())
                {
                    status = WS.RequestContactInformation(userId, targetUserId);
                }
                if (status)
                {
                    return Json
                    (
                        true
                    );
                }
                else
                {
                    return Json
                    (
                        new SystemMessageModel()
                        {
                            SystemMessage = "Sorry we didn't manage to send your Contact Info Request :-("
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
        // GET: /ContactInfo/Requests/2
        // **************************************
        [Authorize]
        public ActionResult Requests(int? id)
        {
            var userId = Convert.ToInt32(Session["userId"]);

            var idInt = HlUtility.ConvertNullableIntToPositiveInt(id);

            try
            {
                using (var WS = new HLServiceClient())
                {
                    var requestDtos = WS.GetContactInfoRequestsToUser(userId, 12, idInt);

                    /* These settings are used by the _Pager partial view. */
                    if (requestDtos.Length > 0)
                    {
                        ViewBag.PagerType = "OneParameterActionMethod";
                        ViewBag.ListObjectName = "Requests";
                        ViewBag.CurrentPage = idInt;
                        ViewBag.HasNextPageOfData = requestDtos[0].HasNextPageOfData;
                        ViewBag.Controller = "ContactInfo";
                        ViewBag.Action = "Requests";
                    }
                    return View(requestDtos);
                }
            }
            catch (Exception ex)
            {
                ViewBag.Layout = "_LayoutUserHome.cshtml";
                TempData["ErrorMessage"] = ex.Message;
                return View("Message");
            }
        }


        // *******************************************
        // POST AJAX: /ContactInfo/AcceptRequest
        // *******************************************
        [HttpPost]
        [Authorize]
        public JsonResult AcceptRequest(int contactInfoRequestId, int fromUserId)
        {
            var userId = (int)Session["userId"];
            var status = false;

            try
            {
                using (var WS = new HLServiceReference.HLServiceClient())
                {
                    status = WS.AcceptContactInfoRequest(contactInfoRequestId, fromUserId, userId);
                }
                if (status)
                {
                    return Json
                    (
                        true
                    );
                }
                else
                {
                    return Json
                    (
                        new SystemMessageModel()
                        {
                            SystemMessage = "Sorry we didn't manage to accept that Contact Info Request :-("
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


        // *******************************************
        // POST AJAX: /ContactInfo/RejectRequest
        // *******************************************
        [HttpPost]
        [Authorize]
        public JsonResult RejectRequest(int contactInfoRequestId)
        {
            var status = false;

            try
            {
                using (var WS = new HLServiceReference.HLServiceClient())
                {
                    status = WS.RejectContactInfoRequest(contactInfoRequestId);
                }
                if (status)
                {
                    return Json
                    (
                        true
                    );
                }
                else
                {
                    return Json
                    (
                        new SystemMessageModel()
                        {
                            SystemMessage = "Sorry we didn't manage reject that Contact Info Request :-("
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
        // GET: /ContactInfo/MyContacts/2
        // **************************************
        [Authorize]
        public ActionResult MyContacts(int? id)
        {
            var userId = Convert.ToInt32(Session["userId"]);

            var idInt = HlUtility.ConvertNullableIntToPositiveInt(id);

            try
            {
                using (var WS = new HLServiceClient())
                {
                    var userDtos = WS.GetUsersWhoAreSharingContactInfoWithUser(userId, 12, idInt);

                    /* These settings are used by the _Pager partial view. */
                    if (userDtos.Length > 0)
                    {
                        ViewBag.PagerType = "OneParameterActionMethod";
                        ViewBag.ListObjectName = "Users";
                        ViewBag.CurrentPage = idInt;
                        ViewBag.HasNextPageOfData = userDtos[0].HasNextPageOfData;
                        ViewBag.Controller = "ContactInfo";
                        ViewBag.Action = "MyContacts";
                    }
                    return View(userDtos);
                }
            }
            catch (Exception ex)
            {
                ViewBag.Layout = "_LayoutUserHome.cshtml";
                TempData["ErrorMessage"] = ex.Message;
                return View("Message");
            }
        }


        // *******************************************
        // POST AJAX: /ContactInfo/StopSharing
        // *******************************************
        /// <summary>
        /// Makes two Users stop sharing contact information.
        /// </summary>
        [HttpPost]
        [Authorize]
        public JsonResult StopSharing(int userId)
        {
            var loggedInUserId = Convert.ToInt32(Session["userId"]);

            var status = false;

            try
            {
                using (var WS = new HLServiceReference.HLServiceClient())
                {
                    status = WS.StopSharingContactInfo(loggedInUserId, userId);
                }
                if (status)
                {
                    return Json
                    (
                        true
                    );
                }
                else
                {
                    return Json
                    (
                        new SystemMessageModel()
                        {
                            SystemMessage = "Sorry we didn't manage reject that Contact Info Request :-("
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


        /// <summary>
        /// Gets the number of ContactInfoRequests for the logged in User, this method is called from the layout page 
        /// _LayoutUserHome, to put the number of ContactInfoRequests on the Request tab menu item.
        /// E.g. "Requests (8)" indicating that there are 8 ContactInfoRequests for the User to deal with.
        /// </summary>
        [Authorize]
        [ChildActionOnly]
        public string GetNumberOfContactInfoRequests()
        {
            var userId = Convert.ToInt32(Session["userId"]);
            string numberOfContactInfoRequestsString = "";

            try
            {
                using (var WS = new HLServiceClient())
                {
                    var numberOfContactInfoRequests = WS.GetNumberOfContactInfoRequestsToUser(userId);

                    if (numberOfContactInfoRequests > 0)
                        numberOfContactInfoRequestsString = " (" + numberOfContactInfoRequests + ")";

                    return numberOfContactInfoRequestsString;
                }
            }
            catch (Exception ex)
            {
                ViewBag.Layout = "_LayoutUserHome.cshtml";
                TempData["ErrorMessage"] = ex.Message;
                return numberOfContactInfoRequestsString;
            }
        }
    }
}
