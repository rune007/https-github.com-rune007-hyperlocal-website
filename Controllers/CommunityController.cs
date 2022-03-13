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
    public class CommunityController : Controller
    {
        // **************************************
        //// GET: /Community/Details/5/2
        // **************************************
        public ActionResult Details(int id, int? pageNumber)
        {
            var communityDto = new CommunityDto();

            var pageNumberInt = HlUtility.ConvertNullableIntToPositiveInt(pageNumber);

            ViewBag.MapSize = "ExtraSmall";

            try
            {
                using (var WS = new HLServiceClient())
                {
                    communityDto = WS.GetCommunity(id);

                    // In case of no material is returned from the request, we render a "Not Found" view.
                    if (communityDto == null)
                    {
                        @ViewBag.Message = "Not Found";
                        TempData["ErrorMessage"] = "Sorry the requested material cannot be found :-(";
                        return View("Message");
                    }
                    else
                    {
                        /* Community */
                        @ViewBag.CommunityDto = communityDto;

                        /* Checking whether an authenticated User is following the Community. */
                        if (Request.IsAuthenticated)
                        {
                            var userId = (int)Session["userId"];
                            @ViewBag.IsUserFollowingCommunity = WS.IsUserFollowingCommunity(userId, id);
                        }

                        /* NewsItems */
                        var newsItemDtos = WS.GetNewestNewsItemsFromCommunity(id, 12, pageNumberInt);

                        /* Displaying breaking news if there is any news breaking. */
                        GetBreakingNewsFromCommunity(id);

                        /* Poll */
                        GetCurrentCommunityPoll(communityDto.CommunityID);

                        /* These settings are used by the _Pager partial view. */
                        if (newsItemDtos.Length > 0)
                        {
                            ViewBag.ItemsToDisplay = true;
                            ViewBag.PagerType = "TwoParametersActionMethod";
                            ViewBag.ListObjectName = "News";
                            ViewBag.AreaIdentifier = id;
                            ViewBag.CurrentPage = pageNumberInt;
                            ViewBag.HasNextPageOfData = newsItemDtos[0].HasNextPageOfData;
                            ViewBag.Controller = "Community";
                            ViewBag.Action = "Details";
                        }
                        /* Only used when there are no items to display, just get's the 
                        * center of an area, in order to display an appropriate map*/
                        else
                        {
                            ViewBag.ItemsToDisplay = false;
                            ViewBag.MapZoomLevel = 12;
                            ViewBag.AreaCenterLatitude = communityDto.PolygonCenterLatitude;
                            ViewBag.AreaCenterLongitude = communityDto.PolygonCenterLongitude;
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
        // GET: /Community/Users/5/2
        // **************************************
        public ActionResult Users(int id, int? pageNumber)
        {
            var communityDto = new CommunityDto();

            var pageNumberInt = HlUtility.ConvertNullableIntToPositiveInt(pageNumber);

            ViewBag.MapSize = "ExtraSmall";

            try
            {
                using (var WS = new HLServiceClient())
                {
                    communityDto = WS.GetCommunity(id);

                    // In case of no material is returned from the request, we render a "Not Found" view.
                    if (communityDto == null)
                    {
                        @ViewBag.Message = "Not Found";
                        TempData["ErrorMessage"] = "Sorry the requested material cannot be found :-(";
                        return View("Message");
                    }
                    else
                    {
                        @ViewBag.CommunityDto = communityDto;

                        /* Checking whether an authenticated User is following the Community. */
                        if (Request.IsAuthenticated)
                        {
                            var userId = (int)Session["userId"];
                            @ViewBag.IsUserFollowingCommunity = WS.IsUserFollowingCommunity(userId, id);
                        }

                        var userDtos = WS.GetLatestActiveUsersFromCommunity(id, 12, pageNumberInt);

                        /* Displaying breaking news if there is any news breaking. */
                        GetBreakingNewsFromCommunity(id);

                        /* Poll */
                        GetCurrentCommunityPoll(communityDto.CommunityID);

                        /* These settings are used by the _Pager partial view. */
                        if (userDtos.Length > 0)
                        {
                            ViewBag.ItemsToDisplay = true;
                            ViewBag.PagerType = "TwoParametersActionMethod";
                            ViewBag.ListObjectName = "Users";
                            ViewBag.AreaIdentifier = id;
                            ViewBag.CurrentPage = pageNumberInt;
                            ViewBag.HasNextPageOfData = userDtos[0].HasNextPageOfData;
                            ViewBag.Controller = "Community";
                            ViewBag.Action = "Users";
                        }
                        /* Only used when there are no items to display, just get's the 
                        * center of an area, in order to display an appropriate map*/
                        else
                        {
                            ViewBag.ItemsToDisplay = false;
                            ViewBag.MapZoomLevel = 12;
                            ViewBag.AreaCenterLatitude = communityDto.PolygonCenterLatitude;
                            ViewBag.AreaCenterLongitude = communityDto.PolygonCenterLongitude;
                        }
                        return View(userDtos);
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
        // GET: /Community/Area/5
        // **************************************
        public ActionResult Area(int id)
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
                        TempData["ErrorMessage"] = "Sorry the requested material cannot be found :-(";
                        return View("Message");
                    }
                    else
                    {
                        /* Community */
                        /* The partial view _PollingForLatestBreakingNewsFromCommunityJavaScript expects CommunityDto via ViewBag. */
                        ViewBag.CommunityDto = communityDto;

                        /* Poll */
                        GetCurrentCommunityPoll(communityDto.CommunityID);

                        /* Displaying breaking news if there is any news breaking. */
                        GetBreakingNewsFromCommunity(id);

                        /* Checking whether an authenticated User is following the Community. */
                        if (Request.IsAuthenticated)
                        {
                            /* The Follow Community Button needs the CommunityID, here passed via @ViewBag.CommunityDto. */
                            @ViewBag.CommunityDto = communityDto;
                            var userId = (int)Session["userId"];
                            @ViewBag.IsUserFollowingCommunity = WS.IsUserFollowingCommunity(userId, id);
                        }
                        return View(communityDto);
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
        /// Checks for whether there is a current CommunityPoll to display and makes sure that only Users 
        /// living in the Community area or the creater of the Poll, has a chance to see the poll and vote.
        /// </summary>
        private void GetCurrentCommunityPoll(int communityId)
        {
            try
            {
                using (var WS = new HLServiceClient())
                {
                    /* Only logged in Users stand a chance of perhaps participating in the CommunityPoll. */
                    if (Request.IsAuthenticated)
                    {
                        /* The UserID of the logged in User. */
                        var userId = (int)Session["userId"];

                        /* Making sure that we only display the CommunityPoll to Users who live in the Community area,
                         * or to the creater of the Community. */
                        if (WS.IsUserLivingInCommunityArea(userId, communityId) || WS.IsCommunityCreatedByUser(userId, communityId))
                        {
                            /* Getting the current CommunityPoll, if there is such a one. */
                            var pollDto = WS.GetCurrentCommunityPoll(communityId, userId);
                            ViewBag.CommunityPoll = pollDto;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
        }


        // **************************************
        // GET: /Community/Create
        // **************************************
        [Authorize]
        public ActionResult Create()
        {
            var model = new CommunityModel();
            return View(model);
        }


        // **************************************
        // POST AJAX: /Community/Create
        // **************************************
        [HttpPost]
        [Authorize]
        public JsonResult Create(CommunityModel model)
        {
            var communityDto = new HLServiceReference.CommunityDto();

            communityDto.AddedByUserID = (int)Session["userId"];
            communityDto.Name = model.Name;
            communityDto.Description = model.Description;
            communityDto.PolygonWkt = model.PolygonWkt;

            try
            {
                using (var WS = new HLServiceClient())
                {
                    var currentCommunityId = WS.CreateCommunity(communityDto);

                    if (currentCommunityId > 0)
                    {
                        return Json
                        (
                            new CommunityModel()
                            {
                                CommunityID = currentCommunityId
                            }
                        );
                    }
                    else
                    {
                        return Json
                        (
                            new SystemMessageModel()
                            {
                                SystemMessage = "Sorry the Community was not created :-("
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
        // GET: /Community/Edit/5
        // **************************************
        [Authorize]
        public ActionResult Edit(int id)
        {
            var communityDto = new CommunityDto();

            try
            {
                using (var WS = new HLServiceReference.HLServiceClient())
                {
                    communityDto = WS.GetCommunity(id);

                    // In case of no material is returned from the request, we render a "Not Found" view.
                    if (communityDto == null)
                    {
                        @ViewBag.Message = "Not Found";
                        TempData["ErrorMessage"] = "Sorry the requested material cannot be found :-(";
                        return View("Message");
                    }

                    // Checking whether a user which is not the owner is trying to edit information which they don't own.
                    if (communityDto.AddedByUserID != Convert.ToInt32(Session["userId"]))
                    {
                        @ViewBag.Message = "Invalid Owner";
                        TempData["ErrorMessage"] = "Oops! You are trying to edit information which you don't own!";
                        return View("Message");
                    }

                    var communityModel = new CommunityModel()
                    {
                        CommunityID = communityDto.CommunityID,
                        AddedByUserID = communityDto.AddedByUserID,
                        Name = communityDto.Name,
                        Description = communityDto.Description,
                        PolygonWkt = communityDto.PolygonWkt,
                        PolygonWktB = communityDto.PolygonWkt,
                        PolygonWktC = communityDto.PolygonWkt,
                        ImageBlobUri = communityDto.ImageBlobUri
                    };

                    return View(communityModel);
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return View("Message");
            }
        }


        // **************************************
        // POST AJAX: /Community/Edit
        // **************************************
        [HttpPost]
        [Authorize]
        public JsonResult Edit(CommunityModel model)
        {
            var communityDto = new HLServiceReference.CommunityDto();
            var currentCommunityId = model.CommunityID;
            communityDto.CommunityID = model.CommunityID;
            communityDto.AddedByUserID = model.AddedByUserID;
            communityDto.Name = model.Name;
            communityDto.Description = model.Description;
            communityDto.PolygonWkt = model.PolygonWkt;

            try
            {
                using (var WS = new HLServiceClient())
                {
                    bool wasUpdated = WS.UpdateCommunity(communityDto);

                    if (wasUpdated)
                    {
                        return Json
                        (
                            new CommunityModel()
                            {
                                CommunityID = currentCommunityId
                            }
                        );
                    }
                    else
                    {
                        return Json
                        (
                            new SystemMessageModel()
                            {
                                SystemMessage = "Sorry the Community was not created :-("
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
        // POST AJAX: /Community/Delete
        // **************************************
        [HttpPost]
        [Authorize]
        public JsonResult Delete(int communityId)
        {
            try
            {
                using (var WS = new HLServiceClient())
                {
                    bool status = WS.DeleteCommunity(communityId);

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
                                SystemMessage = "Sorry the Community was not deleted :-("
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
        // POST AJAX: /Community/Upload
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
                    var sasUri = WS.GetSasUriForBlobWrite(HLServiceReference.BusinessLogicMediaUsage.Community, fileName);
                    var writeBlob = new CloudBlob(sasUri);
                    writeBlob.UploadFromStream(fileData.InputStream);

                    var uri = HlUtility.GetUriWithoutSas(sasUri);
                    WS.SaveMedia(hostItemId, uri, HLServiceReference.BusinessLogicMediaUsage.Community);
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            return "ok";
        }


        // **************************************
        // GET: /Community/MyCommunities/2
        // **************************************
        [Authorize]
        public ActionResult MyCommunities(int? id)
        {
            var userId = Convert.ToInt32(Session["userId"]);

            var idInt = HlUtility.ConvertNullableIntToPositiveInt(id);

            try
            {
                using (var WS = new HLServiceClient())
                {
                    var communityDtos = WS.GetCommunitiesCreatedByUser(userId, 12, idInt);

                    /* These settings are used by the _Pager partial view. */
                    if (communityDtos.Length > 0)
                    {
                        ViewBag.PagerType = "OneParameterActionMethod";
                        ViewBag.ListObjectName = "Communities";
                        ViewBag.CurrentPage = idInt;
                        ViewBag.HasNextPageOfData = communityDtos[0].HasNextPageOfData;
                        ViewBag.Controller = "Community";
                        ViewBag.Action = "MyCommunities";
                    }
                    return View(communityDtos);
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
        // GET: /Community/Followed/2
        // **************************************
        [Authorize]
        public ActionResult Followed(int? id)
        {
            var userId = Convert.ToInt32(Session["userId"]);

            var idInt = HlUtility.ConvertNullableIntToPositiveInt(id);

            try
            {
                using (var WS = new HLServiceClient())
                {
                    var communityDtos = WS.GetCommunitiesFollowedByUser(userId, 12, idInt);

                    /* These settings are used by the _Pager partial view. */
                    if (communityDtos.Length > 0)
                    {
                        ViewBag.PagerType = "OneParameterActionMethod";
                        ViewBag.ListObjectName = "Communities";
                        ViewBag.CurrentPage = idInt;
                        ViewBag.HasNextPageOfData = communityDtos[0].HasNextPageOfData;
                        ViewBag.Controller = "Community";
                        ViewBag.Action = "Followed";
                    }
                    return View(communityDtos);
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
        // POST AJAX: /Community/FollowCommunity
        // **************************************
        [HttpPost]
        [Authorize]
        public JsonResult FollowCommunity(int communityId)
        {
            var userId = (int)Session["userId"];
            var communityFollowed = false;

            try
            {
                using (var WS = new HLServiceReference.HLServiceClient())
                {
                    communityFollowed = WS.UserFollowCommunity(userId, communityId);
                }
                if (communityFollowed)
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
                            SystemMessage = "Sorry we didn't manage to make you follow Community :-("
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
        // POST AJAX: /Community/UnfollowCommunity
        // **************************************
        [HttpPost]
        [Authorize]
        public JsonResult UnfollowCommunity(int communityId)
        {
            var userId = (int)Session["userId"];
            var communityUnfollowed = false;

            try
            {
                using (var WS = new HLServiceReference.HLServiceClient())
                {
                    communityUnfollowed = WS.UserUnfollowCommunity(userId, communityId);
                }
                if (communityUnfollowed)
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
                            SystemMessage = "Sorry we didn't manage to make you unfollow Community :-("
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


        // ****************************************************************
        // POST AJAX: /Community/PollingForLatestBreakingNewsFromCommunity
        // ****************************************************************
        /// <summary>
        /// Polls asynchronously the server for latest breaking news from a Community and updates the UI.
        /// </summary>
        [HttpPost]
        public JsonResult PollingForLatestBreakingNewsFromCommunity(int newsItemId, int communityId)
        {
            try
            {
                using (var WS = new HLServiceClient())
                {
                    var dto = WS.PollingForLatestBreakingNewsFromCommunity(newsItemId, communityId, 8);

                    if (dto != null)
                    {
                        return Json
                        (
                            new NewsItemModel()
                            {
                                NewsItemID = dto.NewsItemID,
                                Title = dto.Title,
                                CoverPhotoLarge = dto.CoverPhotoLarge
                            }
                        );
                    }
                    else
                    {
                        return Json
                        (
                            null
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


        #region GeoNavigationMenu Methods

        // **************************************
        // GET: /Community/Index/2
        // **************************************
        public ActionResult Index(int? id)
        {
            /* 1 line below: Used by GeoNavigationMenu*/
            Session["controller"] = "Community";

            var idInt = HlUtility.ConvertNullableIntToPositiveInt(id);

            try
            {
                using (var WS = new HLServiceClient())
                {
                    /* Gets the latest trending NewsItem from within the area of the whole of Denmark and updates the UI. */
                    GetTrendingNewsFromDkCountry();

                    /* Getting the current Poll for the area, if there is such a one. */
                    var pollDto = WS.GetCurrentAnonymousPoll("Country");
                    ViewBag.Poll = pollDto;

                    var communityDtos = WS.GetLatestActiveCommunitiesFromDkCountry(12, idInt);

                    /* These settings are used by the _Pager partial view. */
                    if (communityDtos.Length > 0)
                    {
                        ViewBag.ItemsToDisplay = true;
                        ViewBag.PagerType = "OneParameterActionMethod";
                        ViewBag.ListObjectName = "Communities";
                        ViewBag.CurrentPage = idInt;
                        ViewBag.HasNextPageOfData = communityDtos[0].HasNextPageOfData;
                        ViewBag.Controller = "Community";
                        ViewBag.Action = "Index";
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
                    return View(communityDtos);
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return View("Message");
            }
        }


        // **************************************
        // GET: /Community/Region/Nordjylland/2
        // **************************************
        public ActionResult Region(string id, int? pageNumber)
        {
            /* 3 lines below: Used by GeoNavigationMenu*/
            Session["controller"] = "Community";
            ViewBag.Region = id;
            ViewBag.REGIONNAVN = Session[id];

            var pageNumberInt = HlUtility.ConvertNullableIntToPositiveInt(pageNumber);

            try
            {
                using (var WS = new HLServiceClient())
                {
                    /* Gets the latest trending NewsItem from within the area of the Region and updates the UI. */
                    GetTrendingNewsFromRegion(id);

                    /* Getting the current Poll for the area, if there is such a one. */
                    var pollDto = WS.GetCurrentAnonymousPoll(id);
                    ViewBag.Poll = pollDto;

                    var communityDtos = WS.GetLatestActiveCommunitiesFromRegion(id, 12, pageNumberInt);

                    /* These settings are used by the _Pager partial view. */
                    if (communityDtos.Length > 0)
                    {
                        ViewBag.ItemsToDisplay = true;
                        ViewBag.PagerType = "TwoParametersActionMethod";
                        ViewBag.ListObjectName = "Communities";
                        ViewBag.AreaIdentifier = id;
                        ViewBag.CurrentPage = pageNumberInt;
                        ViewBag.HasNextPageOfData = communityDtos[0].HasNextPageOfData;
                        ViewBag.Controller = "Community";
                        ViewBag.Action = "Region";
                    }
                    /* Only used when there are no items to display, just get's the 
                    * center of an area, in order to display an appropriate map*/
                    else
                    {
                        ViewBag.ItemsToDisplay = false;
                        var pointDto = WS.GetCenterPointOfRegion(id);
                        ViewBag.MapZoomLevel = 7;
                        ViewBag.AreaCenterLatitude = pointDto.Latitude;
                        ViewBag.AreaCenterLongitude = pointDto.Longitude;
                    }
                    return View(communityDtos);
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return View("Message");
            }
        }


        // ***************************************
        // GET: /Community/Municipality/Gentofte/2
        // ***************************************
        public ActionResult Municipality(string id, int? pageNumber)
        {
            /* 3 lines below: Used by GeoNavigationMenu*/
            Session["controller"] = "Community";
            ViewBag.Municipality = id;
            ViewBag.KOMNAVN = Session[id];

            var pageNumberInt = HlUtility.ConvertNullableIntToPositiveInt(pageNumber);

            try
            {
                using (var WS = new HLServiceClient())
                {
                    /* Gets the latest trending NewsItem from within the area of the Municipality and updates the UI. */
                    GetTrendingNewsFromMunicipality(id);

                    /* Getting the current Poll for the area, if there is such a one. */
                    var pollDto = WS.GetCurrentAnonymousPoll(id);
                    ViewBag.Poll = pollDto; ;

                    var communityDtos = WS.GetLatestActiveCommunitiesFromMunicipality(id, 12, pageNumberInt);

                    /* These settings are used by the _Pager partial view. */
                    if (communityDtos.Length > 0)
                    {
                        ViewBag.ItemsToDisplay = true;
                        ViewBag.PagerType = "TwoParametersActionMethod";
                        ViewBag.ListObjectName = "Communities";
                        ViewBag.AreaIdentifier = id;
                        ViewBag.CurrentPage = pageNumberInt;
                        ViewBag.HasNextPageOfData = communityDtos[0].HasNextPageOfData;
                        ViewBag.Controller = "Community";
                        ViewBag.Action = "Municipality";
                    }
                    /* Only used when there are no items to display, just get's the 
                    * center of an area, in order to display an appropriate map*/
                    else
                    {
                        ViewBag.ItemsToDisplay = false;
                        var pointDto = WS.GetCenterPointOfMunicipality(id);
                        ViewBag.MapZoomLevel = 10;
                        ViewBag.AreaCenterLatitude = pointDto.Latitude;
                        ViewBag.AreaCenterLongitude = pointDto.Longitude;
                    }
                    return View(communityDtos);
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return View("Message");
            }
        }


        // **************************************
        // GET: /Community/PostalCode/2900/2
        // **************************************
        public ActionResult PostalCode(string id, int? pageNumber)
        {
            /* 3 lines below: Used by GeoNavigationMenu*/
            Session["controller"] = "Community";
            ViewBag.POSTNR_TXT = id;
            ViewBag.POSTNR_TXT_POSTBYNAVN = Session[id];

            var pageNumberInt = HlUtility.ConvertNullableIntToPositiveInt(pageNumber);

            try
            {
                using (var WS = new HLServiceClient())
                {
                    /* Get and display eventual breaking news.*/
                    GetBreakingNewsFromPostalCode(id);

                    /* Getting the current Poll for the area, if there is such a one. */
                    var pollDto = WS.GetCurrentAnonymousPoll(id);
                    ViewBag.Poll = pollDto;

                    var communityDtos = WS.GetLatestActiveCommunitiesFromPostalCode(id, 12, pageNumberInt);

                    /* Setting the size of the map.*/
                    ViewBag.MapSize = "Medium";

                    /* These settings are used by the _Pager partial view. */
                    if (communityDtos.Length > 0)
                    {
                        ViewBag.ItemsToDisplay = true;
                        ViewBag.PagerType = "TwoParametersActionMethod";
                        ViewBag.ListObjectName = "Communities";
                        ViewBag.AreaIdentifier = id;
                        ViewBag.CurrentPage = pageNumberInt;
                        ViewBag.HasNextPageOfData = communityDtos[0].HasNextPageOfData;
                        ViewBag.Controller = "Community";
                        ViewBag.Action = "PostalCode";
                    }
                    /* Only used when there are no items to display, just get's the 
                    * center of an area, in order to display an appropriate map*/
                    else
                    {
                        ViewBag.ItemsToDisplay = false;
                        var pointDto = WS.GetCenterPointOfPostalCode(id);
                        ViewBag.MapZoomLevel = 11;
                        ViewBag.AreaCenterLatitude = pointDto.Latitude;
                        ViewBag.AreaCenterLongitude = pointDto.Longitude;
                    }
                    return View(communityDtos);
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return View("Message");
            }
        }

        #endregion


        /// <summary>
        /// Gets the latest breaking NewsItem (Marked as IsLocalBreakingNews) from within the area of a
        /// particular Community and which is not older than hoursToGoBack subtracted from the current time.
        /// (The variable hoursToGoBack is set within the method.)
        /// The eventual breaking NewsItem is displayed by the partial view _SystemMessage wich is residing
        /// in the layout page _LayoutBaseWrapper, located in the Views/Shared folder.
        /// </summary>
        private void GetBreakingNewsFromCommunity(int communityId)
        {
            try
            {
                using (var WS = new HLServiceClient())
                {
                    var dto = WS.GetBreakingNewsFromCommunity(communityId, 8);

                    if (dto != null)
                    {
                        TempData["NewsItemID"] = dto.NewsItemID;
                        TempData["BreakingNews"] = "   BREAKING NEWS - " + dto.Title;
                        if (dto.HasPhoto)
                            TempData["PhotoUri"] = dto.CoverPhotoLarge;               
                        else
                            TempData["PhotoUri"] = "../../Content/images/noPhotoAvailable.jpg";
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError("CommunityController.GetBreakingNewsFromCommunity(): " + ex.ToString());
            }
        }


        /// <summary>
        /// Gets the latest breaking NewsItem (Marked as IsLocalBreakingNews) from within the area of a
        /// particular PostalCode and which is not older than hoursToGoBack subtracted from the current time.
        /// (The variable hoursToGoBack is set within the method.)
        /// The eventual breaking NewsItem is displayed by the partial view _SystemMessage wich is residing
        /// in the layout page _LayoutBaseWrapper, located in the Views/Shared folder.
        /// </summary>
        private void GetBreakingNewsFromPostalCode(string POSTNR_TXT)
        {
            try
            {
                using (var WS = new HLServiceClient())
                {
                    var dto = WS.GetBreakingNewsFromPostalCode(POSTNR_TXT, 8);

                    if (dto != null)
                    {
                        TempData["NewsItemID"] = dto.NewsItemID;
                        TempData["BreakingNews"] = "   BREAKING NEWS - " + dto.Title;
                        if (dto.HasPhoto)
                            TempData["PhotoUri"] = dto.CoverPhotoLarge;
                        else
                            TempData["PhotoUri"] = "../../Content/images/noPhotoAvailable.jpg";
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError("CommunityController.GetBreakingNewsFromPostalCode(): " + ex.ToString());
            }
        }


        /// <summary>
        /// Gets the NewsItem which have generated the most User interaction (in the way of NumberOfComments and NumberOfShares
        /// - social media sharings) from within the area of Denmark, in the time frame defined by hoursToGoBack.
        /// (The variable hoursToGoBack is set within the method.)
        /// The eventual trending NewsItem is displayed by the partial view _SystemMessage wich is residing
        /// in the layout page _LayoutBaseWrapper, located in the Views/Shared folder.
        /// </summary>
        private void GetTrendingNewsFromDkCountry()
        {
            try
            {
                using (var WS = new HLServiceClient())
                {
                    var dto = WS.GetTrendingNewsFromDkCountry(8);

                    if (dto != null)
                    {
                        TempData["NewsItemID"] = dto.NewsItemID;
                        TempData["TrendingNews"] = "   TRENDING NEWS - " + dto.Title;
                        if (dto.HasPhoto)
                            TempData["PhotoUri"] = dto.CoverPhotoLarge;
                        else
                            TempData["PhotoUri"] = "../../Content/images/noPhotoAvailable.jpg";
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError("CommunityController.GetTrendingNewsFromDkCountry(): " + ex.ToString());
            }
        }


        /// <summary>
        /// Gets the NewsItem which have generated the most User interaction (in the way of NumberOfComments and NumberOfShares
        /// - social media sharings) from within the area of a Region, in the time frame defined by hoursToGoBack.
        /// (The variable hoursToGoBack is set within the method.)
        /// The eventual trending NewsItem is displayed by the partial view _SystemMessage wich is residing
        /// in the layout page _LayoutBaseWrapper, located in the Views/Shared folder.
        /// </summary>
        private void GetTrendingNewsFromRegion(string urlRegionName)
        {
            try
            {
                using (var WS = new HLServiceClient())
                {
                    var dto = WS.GetTrendingNewsFromRegion(urlRegionName, 8);

                    if (dto != null)
                    {
                        TempData["NewsItemID"] = dto.NewsItemID;
                        TempData["TrendingNews"] = "   TRENDING NEWS - " + dto.Title;
                        if (dto.HasPhoto)
                            TempData["PhotoUri"] = dto.CoverPhotoLarge;
                        else
                            TempData["PhotoUri"] = "../../Content/images/noPhotoAvailable.jpg";
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError("CommunityController.GetTrendingNewsFromRegion(): " + ex.ToString());
            }
        }


        /// <summary>
        /// Gets the NewsItem which have generated the most User interaction (in the way of NumberOfComments and NumberOfShares
        /// - social media sharings) from within the area of a Municipality, in the time frame defined by hoursToGoBack.
        /// (The variable hoursToGoBack is set within the method.)
        /// The eventual trending NewsItem is displayed by the partial view _SystemMessage wich is residing
        /// in the layout page _LayoutBaseWrapper, located in the Views/Shared folder.
        /// </summary>
        private void GetTrendingNewsFromMunicipality(string urlMunicipalityName)
        {
            try
            {
                using (var WS = new HLServiceClient())
                {
                    var dto = WS.GetTrendingNewsFromMunicipality(urlMunicipalityName, 8);

                    if (dto != null)
                    {
                        TempData["NewsItemID"] = dto.NewsItemID;
                        TempData["TrendingNews"] = "   TRENDING NEWS - " + dto.Title;
                        if (dto.HasPhoto)
                            TempData["PhotoUri"] = dto.CoverPhotoLarge;
                        else
                            TempData["PhotoUri"] = "../../Content/images/noPhotoAvailable.jpg";
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError("CommunityController.GetTrendingNewsFromMunicipality(): " + ex.ToString());
            }
        }
    }
}
