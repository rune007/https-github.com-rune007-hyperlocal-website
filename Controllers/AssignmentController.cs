using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;
using HLWebRole.Models;
using HLWebRole.HLServiceReference;
using HLWebRole.Utilities;
using System.Diagnostics;

namespace HLWebRole.Controllers
{
    public class AssignmentController : Controller
    {
        // **************************************
        // GET: /Assignment/Index/2
        // **************************************
        public ActionResult Index(int? id)
        {
            var idInt = HlUtility.ConvertNullableIntToPositiveInt(id);

            try
            {
                using (var WS = new HLServiceClient())
                {
                    var assignmentDtos = WS.GetActiveAssignments(12, idInt);

                    /* These settings are used by the _Pager partial view. */
                    if (assignmentDtos.Length > 0)
                    {
                        ViewBag.PagerType = "OneParameterActionMethod";
                        ViewBag.ListObjectName = "Assignments";
                        ViewBag.CurrentPage = idInt;
                        ViewBag.HasNextPageOfData = assignmentDtos[0].HasNextPageOfData;
                        ViewBag.Controller = "Assignment";
                        ViewBag.Action = "Index";
                    }
                    return View(assignmentDtos);
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return View("Message");
            }
        }


        // **************************************
        //// GET: /Assignment/Details/5/2
        // **************************************
        public ActionResult Details(int id, int? pageNumber)
        {
            var assignmentDto = new AssignmentDto();

            var pageNumberInt = HlUtility.ConvertNullableIntToPositiveInt(pageNumber);

            ViewBag.MapSize = "Broad";

            try
            {
                using (var WS = new HLServiceClient())
                {
                    assignmentDto = WS.GetAssignment(id);

                    // In case of no material is returned from the request, we render a "Not Found" view.
                    if (assignmentDto == null)
                    {
                        @ViewBag.Message = "Not Found";
                        TempData["ErrorMessage"] = "Sorry the requested material cannot be found :-(";
                        return View("Message");
                    }
                    else
                    {
                        /* Assignment */
                        @ViewBag.AssignmentDto = assignmentDto;

                        /* Transporting AssignmentAreaWkt to the map via ViewBag. */
                        ViewBag.AssignmentAreaWkt = assignmentDto.AreaPolygonWkt;

                        /* NewsItems */
                        var newsItemDtos = WS.GetNewestNewsItemsOnAssignment(id, 12, pageNumberInt);

                        if (newsItemDtos.Length > 0 || assignmentDto.AreaPolygonWkt != null)
                        {
                            /* These settings are used by the _Pager partial view. */
                            if (newsItemDtos.Length > 0)
                            {
                                ViewBag.ItemsToDisplay = true;
                                ViewBag.PagerType = "TwoParametersActionMethod";
                                ViewBag.ListObjectName = "News";
                                ViewBag.AreaIdentifier = id;
                                ViewBag.CurrentPage = pageNumberInt;
                                ViewBag.HasNextPageOfData = newsItemDtos[0].HasNextPageOfData;
                                ViewBag.Controller = "Assignment";
                                ViewBag.Action = "Details";
                            }
                            ViewBag.ItemsToDisplay = true;
                        }
                        /* Only used when there are no items to display, just get's the 
                        * center of an area, in order to display an appropriate map*/
                        else
                        {
                            ViewBag.ItemsToDisplay = false;
                            var pointDto = WS.GetCenterPointOfDkCountry();
                            ViewBag.MapZoomLevel = 6;
                            ViewBag.AreaCenterLatitude = pointDto.Latitude;
                            ViewBag.AreaCenterLongitude = pointDto.Longitude; ;
                        }
                        return View(newsItemDtos);
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
        // GET: /Assignment/Create
        // **************************************
        [Authorize(Roles = "Editor")]
        public ActionResult Create()
        {
            var model = new AssignmentModel();
            var now = DateTime.Now;
            var defaultExpiry = now.AddDays(31);
            model.ExpiryDate = defaultExpiry;
            return View(model);
        }


        // ******************************
        // POST AJAX: /Assignment/Create
        // ******************************
        [HttpPost]
        [Authorize(Roles = "Editor")]
        public JsonResult Create(AssignmentModel model)
        {
            var assignmentDto = new HLServiceReference.AssignmentDto();
            assignmentDto.AddedByUserID = (int)Session["userId"];
            assignmentDto.Title = model.Title;
            assignmentDto.Description = model.Description;
            assignmentDto.ExpiryDate = model.ExpiryDate;

            try
            {
                using (var WS = new HLServiceClient())
                {
                    var currentAssignmentId = WS.CreateAssignment(assignmentDto);

                    if (currentAssignmentId > 0)
                    {
                        return Json
                        (
                            new AssignmentModel()
                            {
                                AssignmentID = currentAssignmentId
                            }
                        );
                    }
                    else
                    {
                        return Json
                        (
                            new SystemMessageModel()
                            {
                                SystemMessage = "Sorry the Assignment creation was not successful :-("
                            }
                        );
                    }
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
        // GET: /Assignment/Edit/5
        // **************************************
        [Authorize(Roles = "Editor")]
        public ActionResult Edit(int id)
        {
            var assignmentDto = new AssignmentDto();

            try
            {
                using (var WS = new HLServiceReference.HLServiceClient())
                {
                    assignmentDto = WS.GetAssignment(id);

                    // In case of no material is returned from the request, we render a "Not Found" view.
                    if (assignmentDto == null)
                    {
                        @ViewBag.Message = "Not Found";
                        ViewBag.Layout = "_LayoutUserHome.cshtml";
                        TempData["ErrorMessage"] = "Sorry the requested material cannot be found :-(";
                        return View("Message");
                    }

                    // Checking whether a user which is not the owner is trying to edit information which they don't own.
                    if (assignmentDto.AddedByUserID != Convert.ToInt32(Session["userId"]))
                    {
                        @ViewBag.Message = "Invalid Owner";
                        ViewBag.Layout = "_LayoutUserHome.cshtml";
                        TempData["ErrorMessage"] = "Oops! You are trying to edit information which you don't own!";
                        return View("Message");
                    }

                    var assignmentModel = new AssignmentModel()
                    {
                        AssignmentID = assignmentDto.AssignmentID,
                        AddedByUserID = assignmentDto.AddedByUserID,
                        Title = assignmentDto.Title,
                        Description = assignmentDto.Description,
                        ExpiryDate = assignmentDto.ExpiryDate,
                        ImageBlobUri = assignmentDto.ImageBlobUri
                    };
                    return View(assignmentModel);
                }
            }
            catch (Exception ex)
            {
                ViewBag.Layout = "_LayoutUserHome.cshtml";
                TempData["ErrorMessage"] = ex.Message;
                return View("Message");
            }
        }


        // **************************************
        // POST AJAX: /Assignment/Edit
        // **************************************
        [HttpPost]
        [Authorize(Roles = "Editor")]
        public JsonResult Edit(AssignmentModel model)
        {
            var assignmentDto = new HLServiceReference.AssignmentDto();
            var currentAssignmentId = model.AssignmentID;
            assignmentDto.AssignmentID = model.AssignmentID;
            assignmentDto.Title = model.Title;
            assignmentDto.Description = model.Description;
            assignmentDto.ExpiryDate = model.ExpiryDate;

            try
            {
                using (var WS = new HLServiceClient())
                {
                    bool wasUpdated = WS.UpdateAssignment(assignmentDto);

                    if (wasUpdated)
                    {
                        return Json
                        (
                            new AssignmentModel()
                            {
                                AssignmentID = currentAssignmentId
                            }
                        );
                    }
                    else
                    {
                        return Json
                        (
                            new SystemMessageModel()
                            {
                                SystemMessage = "Sorry the Assignment was not updated :-("
                            }
                        );
                    }
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
        // POST AJAX: /Assignment/Delete
        // **************************************
        [HttpPost]
        [Authorize(Roles = "Editor")]
        public JsonResult Delete(int assignmentId)
        {
            try
            {
                using (var WS = new HLServiceClient())
                {
                    bool wasDeleted = WS.DeleteAssignment(assignmentId);

                    if (wasDeleted)
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
                                SystemMessage = "Sorry the Assignment was not deleted :-("
                            }
                        );
                    }
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
        // POST AJAX: /Assignment/Upload
        // **************************************
        /// <summary>
        /// This method is used by the Uploadify jQuery plugin to upload media.
        /// </summary>
        /// <param name="fileData"></param>
        /// <param name="hostItemId">This is the id of the item which is connected to the photo, it can be a NewsItemID, UserID, CommunityID or AssignmentID,
        /// depending on whether the object hosting the photo(s) is a NewsItem, User, Community or Assignment. </param>
        /// <returns></returns>
        public string Upload(HttpPostedFileBase fileData, int hostItemId)
        {
            var fileName = fileData.FileName;

            try
            {
                using (var WS = new HLServiceClient())
                {
                    var sasUri = WS.GetSasUriForBlobWrite(HLServiceReference.BusinessLogicMediaUsage.Assignment, fileName);
                    var writeBlob = new CloudBlob(sasUri);
                    writeBlob.UploadFromStream(fileData.InputStream);

                    var uri = HlUtility.GetUriWithoutSas(sasUri);
                    WS.SaveMedia(hostItemId, uri, HLServiceReference.BusinessLogicMediaUsage.Assignment);
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            return "ok";
        }


        // **************************************
        // GET: /Assignment/MyAssignments/2
        // **************************************
        [Authorize(Roles = "Editor")]
        public ActionResult MyAssignments(int? id)
        {
            var userId = Convert.ToInt32(Session["userId"]);

            var idInt = HlUtility.ConvertNullableIntToPositiveInt(id);

            try
            {
                using (var WS = new HLServiceClient())
                {
                    var assignmentDtos = WS.GetAssignmentsCreatedByUser(userId, 12, idInt);

                    /* These settings are used by the _Pager partial view. */
                    if (assignmentDtos.Length > 0)
                    {
                        ViewBag.PagerType = "OneParameterActionMethod";
                        ViewBag.ListObjectName = "Assignments";
                        ViewBag.CurrentPage = idInt;
                        ViewBag.HasNextPageOfData = assignmentDtos[0].HasNextPageOfData;
                        ViewBag.Controller = "Assignment";
                        ViewBag.Action = "MyAssignments";
                    }
                    return View(assignmentDtos);
                }
            }
            catch (Exception ex)
            {
                ViewBag.Layout = "_LayoutUserHome.cshtml";
                TempData["ErrorMessage"] = ex.Message;
                return View("Message");
            }
        }




        // **************************************
        // GET: /Assignment/CreateGeoTemporal
        // **************************************
        [Authorize(Roles = "Editor")]
        public ActionResult CreateGeoTemporal()
        {
            var model = new CreateGeoTemporalAssignmentModel();
            var now = DateTime.Now;
            var defaultExpiry = now.AddDays(1);
            model.ExpiryDate = defaultExpiry;

            return View(model);
        }


        // ***********************************
        // POST AJAX: /Assignment/GeoTemporal
        // ***********************************
        [HttpPost]
        [Authorize(Roles = "Editor")]
        public JsonResult CreateGeoTemporal(CreateGeoTemporalAssignmentModel model)
        {
            try
            {
                var assignmentDto = new HLServiceReference.AssignmentDto();
                assignmentDto.AddedByUserID = (int)Session["userId"];
                assignmentDto.Title = model.Title;
                assignmentDto.Description = model.Description;
                assignmentDto.ExpiryDate = model.ExpiryDate;
                /* We make a reverse geocode request (Finds an address for a lat/long location)  
                 * at Bing Maps Geocode Service. In case the request does not return any address string
                 * we just keep the original Address string that the User typed in. */
                var assignmentCenterAddress = model.Address;
                var spatialQueryController = new SpatialQueryController();
                var reverseGeocodeAddress = spatialQueryController.MakeReverseGeocodeRequest(model.Latitude, model.Longitude);
                if (reverseGeocodeAddress != null)
                    assignmentCenterAddress = reverseGeocodeAddress;
                assignmentDto.AssignmentCenterAddress = assignmentCenterAddress;
                assignmentDto.AssignmentRadius = model.AssignmentRadius;
                assignmentDto.HoursToGoBack = model.HoursToGoBack;
                assignmentDto.AssignmentCenterLatitude = model.Latitude;
                assignmentDto.AssignmentCenterLongitude = model.Longitude;

                using (var WS = new HLServiceClient())
                {
                    var currentAssignmentId = WS.CreateGeoTemporalAssignment(assignmentDto);

                    if (currentAssignmentId > 0)
                    {
                        model.AssignmentID = currentAssignmentId;
                        TempData["SuccessMessage"] = "Your Geo Temporal Assignment has been created.";
                        return Json
                        (
                            new AssignmentModel()
                            {
                                AssignmentID = currentAssignmentId
                            }
                        );
                    }
                    else
                    {
                        return Json
                        (
                            new SystemMessageModel()
                            {
                                SystemMessage = "Sorry the Assignment creation was not successful :-("
                            }
                        );
                    }
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


        // ***********************************
        // POST AJAX: /Assignment/GeoTemporal
        // ***********************************
        [HttpPost]
        [Authorize(Roles = "Editor")]
        public ActionResult GeoTemporal(CreateGeoTemporalAssignmentModel model)
        {
            try
            {
                using (var WS = new HLServiceClient())
                {

                    var assignmentDto = WS.GetAssignment(model.AssignmentID);

                    if (assignmentDto != null)
                    {
                        assignmentDto.HoursToGoBack = model.HoursToGoBack;
                        TempData["AssignmentDto"] = assignmentDto;
                        return RedirectToAction("GeoTemporalResult");
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Sorry, something went wront the Geo Temporal Assignment was not created :-(";
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
        // GET: /Assignment/GeoTemporalResult/2
        // **************************************
        [Authorize(Roles = "Editor")]
        public ActionResult GeoTemporalResult(int? id)
        {
            var idInt = HlUtility.ConvertNullableIntToPositiveInt(id);

            /* The size of map we display. */
            ViewBag.MapSize = "Deep";

            AssignmentDto assignmentDto = (AssignmentDto)TempData["AssignmentDto"];
            TempData["AssignmentDto"] = assignmentDto;
            ViewBag.AssignmentDto = assignmentDto;

            // Putting the current page number into the DTO, used by server side paging.
            assignmentDto.PageNumber = idInt;
            assignmentDto.PageSize = 12;

            try
            {
                using (var WS = new HLServiceClient())
                {
                    var userDtos = WS.GetUsersWithinAreaAndTimeOfGeoTemporalAssignment(assignmentDto);
                    /* Transporting AssignmentAreaWkt to the map via ViewBag. */
                    ViewBag.AssignmentAreaWkt = assignmentDto.AreaPolygonWkt;

                    if (userDtos.Length > 0 || assignmentDto.AreaPolygonWkt != null)
                    {
                        if (userDtos.Length > 0)
                        {
                            /* Telling _UserList that we want to use "GeoTemporalAssignment" UserListType. */
                            ViewBag.UserListType = "GeoTemporalAssignment";
                            ViewBag.ItemsToDisplay = true;
                            /* Settings used by the _Pager patial view. */
                            ViewBag.PagerType = "OneParameterActionMethod";
                            ViewBag.ListObjectName = "Users";
                            ViewBag.CurrentPage = idInt;
                            ViewBag.HasNextPageOfData = userDtos[0].HasNextPageOfData;
                            ViewBag.Controller = "Assignment";
                            ViewBag.Action = "GeoTemporalResult";
                            /* Transfering the number of results which the search yields. */
                            ViewBag.NumberOfSearchResults = userDtos[0].NumberOfSearchResults;
                        }
                        ViewBag.ItemsToDisplay = true;
                    }
                    /* Only used when there are no items to display, just get's the 
                    * center of an area, in order to display an appropriate map*/
                    else
                    {
                        ViewBag.ItemsToDisplay = false;
                        var pointDto = WS.GetCenterPointOfDkCountry();
                        ViewBag.MapZoomLevel = 6;
                        ViewBag.AreaCenterLatitude = pointDto.Latitude;
                        ViewBag.AreaCenterLongitude = pointDto.Longitude;
                    }
                    return View(userDtos);
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return View("Message");
            }
        }
    }
}
