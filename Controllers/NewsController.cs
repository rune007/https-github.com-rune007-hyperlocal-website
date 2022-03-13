using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;
using HLWebRole.HLServiceReference;
using HLWebRole.Models;
using HLWebRole.Utilities;
using Microsoft.Web.Helpers;
using System.Diagnostics;

namespace HLWebRole.Controllers
{
    public class NewsController : Controller
    {
        // **************************************
        // GET: /News/Details/5/2
        // **************************************
        public ActionResult Details(int id, int? pageNumber)
        {
            var pageNumberInt = HlUtility.ConvertNullableIntToPositiveInt(pageNumber);

            NewsItemDto newsItemDto = new NewsItemDto();

            try
            {
                using (var WS = new HLServiceReference.HLServiceClient())
                {
                    newsItemDto = WS.GetNewsItem(id);

                    // In case of no material is returned from the request, we render a "Not Found" view.
                    if (newsItemDto == null)
                    {
                        @ViewBag.Message = "Not Found";
                        TempData["ErrorMessage"] = "Sorry the requested material cannot be found :-(";
                        return View("Message");
                    }

                    var userId = 0;

                    /* Getting UserID if the Request.IsAuthenticated (If it's a logged in User which makes the request. */
                    if (Request.IsAuthenticated)
                    {
                        userId = Convert.ToInt32(Session["userId"]);
                    }

                    /* Getting the eventual Comments for the NewsItem. */
                    var commentDtos = WS.GetCommentsOnNewsItem(id, 24, pageNumberInt);
                    ViewBag.CommentDtos = commentDtos;

                    /* These settings are used by the _Pager partial view. */
                    if (commentDtos.Length > 0)
                    {
                        ViewBag.PagerType = "TwoParametersActionMethod";
                        ViewBag.ListObjectName = "Comments";
                        /* The name AreaIdentifier is misplaced in this context, what it covers is NewsItemID, we need to transport this to the _Pager in order
                         to get proper two parameter ActionLinks like: "/News/Details/5/2" which is: "/News/Details/{NewsItemID}/{PageNumber}" The reason it is
                         called AreaIdentifier is that I orginally wrote the _Pager to be able to page through the data in the different areas, like:
                         "/Community/Region/Nordjylland/2", which is: "/Community/Region/{UrlRegionName}/{PageNumber}". Later, like now, when I also use
                         the _Pager to page through other material, AreaIdentifier is not a good name. A name like "Identifier" would have been a better
                         choice. I have kept the name because it is too time consuming to revise the code and rename. */
                        ViewBag.AreaIdentifier = id;
                        ViewBag.CurrentPage = pageNumberInt;
                        ViewBag.HasNextPageOfData = commentDtos[0].HasNextPageOfData;
                        ViewBag.Controller = "News";
                        ViewBag.Action = "Details";
                    }

                    /* Getting CommentModel for new Comment */
                    var commentModel = new CommentModel();
                    ViewBag.CommentModel = commentModel;

                    /* Getting User who can add a new Comment. */
                    var userDto = WS.GetUser(userId);
                    ViewBag.UserDto = userDto;
                }

                var newsItemPhotoModels = new List<NewsItemPhotoModel>();

                // Adding the photos.
                if (newsItemDto.Photos != null)
                {
                    foreach (var p in newsItemDto.Photos)
                    {
                        newsItemPhotoModels.Add
                        (
                            new NewsItemPhotoModel()
                            {
                                MediaID = p.MediaID,
                                NewsItemID = p.NewsItemID,
                                Caption = p.Caption,
                                BlobUri = p.BlobUri,
                                MediumSizeBlobUri = p.MediumSizeBlobUri,
                                ThumbnailBlobUri = p.ThumbnailBlobUri
                            }
                        );
                    }
                }

                var newsItemVideoModels = new List<NewsItemVideoModel>();

                // Adding the videos.
                if (newsItemDto.Videos != null)
                {
                    foreach (var v in newsItemDto.Videos)
                    {
                        newsItemVideoModels.Add
                        (
                            new NewsItemVideoModel()
                            {
                                MediaID = v.MediaID,
                                NewsItemID = v.NewsItemID,
                                Title = v.Title,
                                BlobUri = v.BlobUri
                            }
                        );
                    }
                }

                var newsItemModel = new NewsItemModel()
                {
                    NewsItemID = newsItemDto.NewsItemID,
                    PostedByUserID = newsItemDto.PostedByUserID,
                    PostedByUserName = newsItemDto.PostedByUserName,
                    CategoryName = newsItemDto.CategoryName,
                    AssignmentID = newsItemDto.AssignmentID,
                    AssignmentTitle = newsItemDto.AssignmentTitle,
                    Title = newsItemDto.Title,
                    Story = newsItemDto.Story,
                    PositionPointWkt = newsItemDto.PositionPointWkt,
                    Latitude = newsItemDto.Latitude,
                    Longitude = newsItemDto.Longitude,
                    CreateUpdateDate = newsItemDto.CreateUpdateDate,
                    IsLocalBreakingNews = newsItemDto.IsLocalBreakingNews,
                    NumberOfViews = newsItemDto.NumberOfViews,
                    NumberOfComments = newsItemDto.NumberOfComments,
                    NumberOfShares = newsItemDto.NumberOfShares,
                    Photos = newsItemPhotoModels,
                    Videos = newsItemVideoModels
                };

                return View(newsItemModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return View("Message");
            }
        }


        // **************************************
        // GET: /News/AddNews
        // **************************************
        [Authorize]
        public ActionResult AddNews()
        {
            try
            {
                using (var WS = new HLServiceClient())
                {
                    // Populating the Category Dropdownlist.
                    var categoryDtos = WS.GetNewsItemCategories();
                    IEnumerable<SelectListItem> lstCategory = categoryDtos.Select(c => new SelectListItem
                    {
                        Value = Convert.ToString(c.CategoryID),
                        Text = c.CategoryName
                    });

                    // Populating the Assignment Dropdownlist.
                    var assignmentDtos = WS.GetAssignmentsForDropDownList();
                    IEnumerable<SelectListItem> lstAssignment = assignmentDtos.Select(a => new SelectListItem
                    {
                        Value = Convert.ToString(a.AssignmentID),
                        Text = a.Title
                    });

                    var newsItemModel = new NewsItemModel();
                    // Retrieving the UserID of the user which is going to add a NewsItem.                
                    newsItemModel.PostedByUserID = (int)Session["userId"];
                    newsItemModel.CategorySelectList = lstCategory;
                    newsItemModel.AssignmentSelectList = lstAssignment;

                    return View(newsItemModel);
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return View("Message");
            }
        }


        // **************************
        // POST AJAX: /News/AddNews
        // **************************
        [HttpPost]
        [Authorize]
        public JsonResult AddNews(NewsItemModel model)
        {
            var newsItemDto = new HLServiceReference.NewsItemDto();
            newsItemDto.PostedByUserID = model.PostedByUserID;
            newsItemDto.CategoryID = model.CategoryID;
            newsItemDto.AssignmentID = model.AssignmentID;
            newsItemDto.Title = model.Title;
            newsItemDto.Story = model.Story;
            newsItemDto.IsLocalBreakingNews = model.IsLocalBreakingNews;
            newsItemDto.Latitude = model.Latitude;
            newsItemDto.Longitude = model.Longitude;

            try
            {
                using (var WS = new HLServiceClient())
                {
                    var currentNewsItemId = WS.CreateNewsItem(newsItemDto);

                    if (currentNewsItemId > 0)
                    {
                        return Json
                        (
                            new NewsItemModel()
                            {
                                NewsItemID = currentNewsItemId
                            }
                        );
                    }
                    else
                    {
                        return Json
                        (
                            new SystemMessageModel()
                            {
                                SystemMessage = "Sorry the News was not posted :-("
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
        // GET: /News/Edit/5
        // **************************************
        [Authorize]
        public ActionResult Edit(int id)
        {
            NewsItemDto newsItemDto = new NewsItemDto();
            IEnumerable<SelectListItem> lstCategory;
            IEnumerable<SelectListItem> lstAssignment;

            try
            {
                using (var WS = new HLServiceReference.HLServiceClient())
                {
                    // Populating the Category Dropdownlist.
                    var categoryDtos = WS.GetNewsItemCategories();
                    lstCategory = categoryDtos.Select(c => new SelectListItem
                    {
                        Value = Convert.ToString(c.CategoryID),
                        Text = c.CategoryName
                    });

                    // Populating the Assignment Dropdownlist.
                    var assignmentDtos = WS.GetAssignmentsForDropDownList();
                    lstAssignment = assignmentDtos.Select(a => new SelectListItem
                    {
                        Value = Convert.ToString(a.AssignmentID),
                        Text = a.Title
                    });

                    newsItemDto = WS.GetNewsItem(id);

                    // In case of no material is returned from the request, we render a "Not Found" view.
                    if (newsItemDto == null)
                    {
                        @ViewBag.Message = "Not Found";
                        TempData["ErrorMessage"] = "Sorry the requested material cannot be found :-(";
                        return View("Message");
                    }

                    // Checking whether a user which is not the owner is trying to edit information which they don't own.
                    if (newsItemDto.PostedByUserID != Convert.ToInt32(Session["userId"]))
                    {
                        @ViewBag.Message = "Invalid Owner";
                        TempData["ErrorMessage"] = "Oops! You are trying to edit information which you don't own!";
                        return View("Message");
                    }
                }

                var newsItemPhotoModels = new List<NewsItemPhotoModel>();

                if (newsItemDto.Photos != null)
                {
                    foreach (var p in newsItemDto.Photos)
                    {
                        newsItemPhotoModels.Add
                        (
                            new NewsItemPhotoModel()
                            {
                                MediaID = p.MediaID,
                                NewsItemID = p.NewsItemID,
                                Caption = p.Caption,
                                BlobUri = p.BlobUri,
                                MediumSizeBlobUri = p.MediumSizeBlobUri,
                                ThumbnailBlobUri = p.ThumbnailBlobUri
                            }
                        );
                    }
                }

                var newsItemVideoModels = new List<NewsItemVideoModel>();

                if (newsItemDto.Videos != null)
                {
                    foreach (var v in newsItemDto.Videos)
                    {
                        newsItemVideoModels.Add
                        (
                            new NewsItemVideoModel()
                            {
                                MediaID = v.MediaID,
                                NewsItemID = v.NewsItemID,
                                Title = v.Title,
                                BlobUri = v.BlobUri
                            }
                        );
                    }
                }

                var newsItemModel = new NewsItemModel()
                {
                    NewsItemID = newsItemDto.NewsItemID,
                    PostedByUserID = newsItemDto.PostedByUserID,
                    PostedByUserName = newsItemDto.PostedByUserName,
                    CategoryID = newsItemDto.CategoryID,
                    CategoryName = newsItemDto.CategoryName,
                    AssignmentID = newsItemDto.AssignmentID,
                    AssignmentTitle = newsItemDto.AssignmentTitle,
                    CategorySelectList = lstCategory,
                    AssignmentSelectList = lstAssignment,
                    Title = newsItemDto.Title,
                    Story = newsItemDto.Story,
                    PositionPointWkt = newsItemDto.PositionPointWkt,
                    Latitude = newsItemDto.Latitude,
                    Longitude = newsItemDto.Longitude,
                    CreateUpdateDate = newsItemDto.CreateUpdateDate,
                    IsLocalBreakingNews = newsItemDto.IsLocalBreakingNews,
                    NumberOfViews = newsItemDto.NumberOfViews,
                    NumberOfComments = newsItemDto.NumberOfComments,
                    NumberOfShares = newsItemDto.NumberOfShares,
                    Photos = newsItemPhotoModels,
                    Videos = newsItemVideoModels
                };

                return View(newsItemModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return View("Message");
            }
        }


        // **************************
        // POST AJAX: /News/Edit
        // **************************
        [HttpPost]
        [Authorize]
        public JsonResult Edit(NewsItemModel model)
        {
            var newsItemDto = new HLServiceReference.NewsItemDto();

            /* Communicating the current NewsItemID to the Uploadify Upload() method. */
            var currentNewsItemId = model.NewsItemID;
            newsItemDto.NewsItemID = model.NewsItemID;
            newsItemDto.CategoryID = model.CategoryID;
            newsItemDto.AssignmentID = model.AssignmentID;
            newsItemDto.Title = model.Title;
            newsItemDto.Story = model.Story;
            newsItemDto.Latitude = model.Latitude;
            newsItemDto.Longitude = model.Longitude;
            newsItemDto.IsLocalBreakingNews = model.IsLocalBreakingNews;

            try
            {
                using (var WS = new HLServiceClient())
                {
                    var updateStatus = WS.UpdateNewsItem(newsItemDto);

                    if (updateStatus)
                    {
                        return Json
                        (
                            new NewsItemModel()
                            {
                                NewsItemID = currentNewsItemId
                            }
                        );
                    }
                    else
                    {
                        return Json
                        (
                            new SystemMessageModel()
                            {
                                SystemMessage = "Sorry the News post was not updated :-("
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
        // POST AJAX: /News/Delete
        // **************************************
        [HttpPost]
        [Authorize]
        public JsonResult Delete(int newsItemId)
        {
            try
            {
                using (var WS = new HLServiceClient())
                {
                    bool status = true;

                    status = WS.DeleteNewsItem(newsItemId);

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
        // POST AJAX: /News/Upload
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
                    var sasUri = WS.GetSasUriForBlobWrite(HLServiceReference.BusinessLogicMediaUsage.News, fileName);
                    var writeBlob = new CloudBlob(sasUri);
                    writeBlob.UploadFromStream(fileData.InputStream);

                    var uri = HlUtility.GetUriWithoutSas(sasUri);
                    WS.SaveMedia(hostItemId, uri, HLServiceReference.BusinessLogicMediaUsage.News);
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            return "ok";
        }


        // *********************************
        // POST AJAX: /News/EditPhotoCaption
        // *********************************
        [HttpPost]
        [Authorize]
        public JsonResult EditPhotoCaption(int mediaId, string photoCaptionText)
        {
            var photoCaptionUpdated = false;

            try
            {
                using (var WS = new HLServiceReference.HLServiceClient())
                {
                    photoCaptionUpdated = WS.UpdatePhotoCaption(mediaId, photoCaptionText);
                }
                // If the item was updated we return the updated values.
                if (photoCaptionUpdated)
                {
                    return Json
                    (
                        new NewsItemPhotoModel()
                        {
                            MediaID = mediaId,
                            Caption = photoCaptionText
                        }
                    );
                }
                // If the item was not updated we return an error message.
                else
                {
                    return Json
                    (
                        new SystemMessageModel()
                        {
                            SystemMessage = "Sorry the photo was not updated :-("
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


        // *******************************
        // POST AJAX: /News/EditVideoTitle
        // *******************************
        [HttpPost]
        [Authorize]
        public JsonResult EditVideoTitle(int mediaId, string videoTitleText)
        {
            var videoTitleUpdated = false;

            try
            {
                using (var WS = new HLServiceReference.HLServiceClient())
                {
                    videoTitleUpdated = WS.UpdateVideoTitle(mediaId, videoTitleText);
                }
                // If the item was updated we return the updated values.
                if (videoTitleUpdated)
                {
                    return Json
                    (
                        new NewsItemVideoModel()
                        {
                            MediaID = mediaId,
                            Title = videoTitleText
                        }
                    );
                }
                // If the item was not updated we return an error message.
                else
                {
                    return Json
                    (
                        new SystemMessageModel()
                        {
                            SystemMessage = "Sorry the video was not updated :-("
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
        // POST AJAX: /News/DeleteMedia
        // **************************************
        [HttpPost]
        [Authorize]
        public JsonResult DeleteMedia(int mediaId)
        {
            var photoDeleted = false;

            try
            {
                using (var WS = new HLServiceReference.HLServiceClient())
                {
                    photoDeleted = WS.DeleteMedia(mediaId, BusinessLogicMediaUsage.News);
                }
                if (photoDeleted)
                {
                    return Json
                    (
                        new NewsItemPhotoModel()
                        {
                            MediaID = mediaId
                        }
                    );
                }
                else
                {
                    return Json
                    (
                        new SystemMessageModel()
                        {
                            SystemMessage = "Sorry the media was not deleted :-("
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


        // *****************************************
        // POST AJAX: /News/IncrementNumberOfShares
        // *****************************************
        /// <summary>
        /// Increments the number of social media shares.
        /// </summary>
        [HttpPost]
        public JsonResult IncrementNumberOfShares(int newsItemId)
        {
            try
            {
                using (var WS = new HLServiceClient())
                {
                    var status = WS.IncrementNumberOfSharesOfNewsItem(newsItemId);

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
                                SystemMessage = "There was a problem incrementing #NumberOfShares for NewsItem."
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
        // GET: /News/Stream/5/2
        // **************************************
        [Authorize]
        public ActionResult Stream(int? id)
        {
            var idInt = HlUtility.ConvertNullableIntToPositiveInt(id);
            // Retrieving the UserID of the logged in User.                
            var userId = (int)Session["userId"];

            /* The size of map we display. */
            ViewBag.MapSize = "Slim";

            try
            {
                using (var WS = new HLServiceClient())
                {
                    /* Gets the breaking news from the News/Stream if there is any. */
                    GetBreakingNewsFromNewsStream(userId);

                    var newsItemDtos = WS.GetNewsStreamForUser(userId, 30, 12, idInt);

                    if (newsItemDtos.Length > 0)
                    {
                        ViewBag.ItemsToDisplay = true;
                        /* Settings used by the _Pager patial view. */
                        ViewBag.PagerType = "OneParameterActionMethod";
                        ViewBag.ListObjectName = "News";
                        ViewBag.CurrentPage = idInt;
                        ViewBag.HasNextPageOfData = newsItemDtos[0].HasNextPageOfData;
                        ViewBag.Controller = "News";
                        ViewBag.Action = "Stream";
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

                    return View(newsItemDtos);
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return View("Message");
            }
        }



        // **************************************
        // GET: /News/MyNews/5/2
        // **************************************
        [Authorize]
        public ActionResult MyNews(int? id)
        {
            var userId = Convert.ToInt32(Session["userId"]);

            var idInt = HlUtility.ConvertNullableIntToPositiveInt(id);

            try
            {
                using (var WS = new HLServiceClient())
                {
                    var newsItemDtos = WS.GetNewsItemsCreatedByUser(userId, 12, idInt);

                    /* These settings are used by the _Pager partial view. */
                    if (newsItemDtos.Length > 0)
                    {
                        ViewBag.PagerType = "OneParameterActionMethod";
                        ViewBag.ListObjectName = "News";
                        ViewBag.CurrentPage = idInt;
                        ViewBag.HasNextPageOfData = newsItemDtos[0].HasNextPageOfData;
                        ViewBag.Controller = "News";
                        ViewBag.Action = "MyNews";
                    }
                    return View(newsItemDtos);
                }
            }
            catch (Exception ex)
            {
                ViewBag.Layout = "_LayoutUserHome.cshtml";
                TempData["ErrorMessage"] = ex.Message;
                return View("Message");
            }
        }


        // *****************************************************************
        // POST AJAX: /News/PollingForLatestBreakingNewsFromPostalCode
        // *****************************************************************
        /// <summary>
        /// Polls asynchronously the server for latest breaking news from a PostalCode and updates the UI.
        /// </summary>
        [HttpPost]
        public JsonResult PollingForLatestBreakingNewsFromPostalCode(int newsItemId, string postalCode)
        {
            try
            {
                using (var WS = new HLServiceClient())
                {
                    var dto = WS.PollingForLatestBreakingNewsFromPostalCode(newsItemId, postalCode, 8);        

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


        // *****************************************************************
        // POST AJAX: /News/PollingForLatestBreakingNewsFromNewsStream
        // *****************************************************************
        /// <summary>
        /// Polls asynchronously the server for latest breaking news from a User's news stream and updates the UI.
        /// A Users news stream is the stream of news coming from the Communities that they follow.
        /// </summary>
        [HttpPost]
        [Authorize]
        public JsonResult PollingForLatestBreakingNewsFromNewsStream(int newsItemId)
        {
            try
            {
                var userId = Convert.ToInt32(Session["userId"]);

                using (var WS = new HLServiceClient())
                {
                    var dto = WS.PollingForLatestBreakingNewsFromNewsStream(newsItemId, userId, 8);

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
        // GET: /News/Index/2
        // **************************************
        public ActionResult Index(int? id)
        {
            /* 1 line below: Used by GeoNavigationMenu*/
            Session["controller"] = "News";

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

                    var newsItemDtos = WS.GetNewestNewsItemsFromDkCountry(12, idInt);

                    if (newsItemDtos.Length > 0)
                    {
                        ViewBag.ItemsToDisplay = true;
                        /* Settings used by the _Pager patial view. */
                        ViewBag.PagerType = "OneParameterActionMethod";
                        ViewBag.ListObjectName = "News";
                        ViewBag.CurrentPage = idInt;
                        ViewBag.HasNextPageOfData = newsItemDtos[0].HasNextPageOfData;
                        ViewBag.Controller = "News";
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
                    return View(newsItemDtos);
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return View("Message");
            }
        }


        // **************************************
        // GET: /News/Region/Nordjylland/2
        // **************************************
        public ActionResult Region(string id, int? pageNumber)
        {
            /* 3 lines below: Used by GeoNavigationMenu*/
            Session["controller"] = "News";
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

                    var newsItemDtos = WS.GetNewestNewsItemsFromRegion(id, 12, pageNumberInt);

                    if (newsItemDtos.Length > 0)
                    {
                        ViewBag.ItemsToDisplay = true;
                        /* Settings used by the _Pager patial view. */
                        ViewBag.PagerType = "TwoParametersActionMethod";
                        ViewBag.ListObjectName = "News";
                        ViewBag.AreaIdentifier = id;
                        ViewBag.CurrentPage = pageNumberInt;
                        ViewBag.HasNextPageOfData = newsItemDtos[0].HasNextPageOfData;
                        ViewBag.Controller = "News";
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
                    return View(newsItemDtos);
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return View("Message");
            }
        }


        // **************************************
        // GET: /News/Municipality/Gentofte/2
        // **************************************
        public ActionResult Municipality(string id, int? pageNumber)
        {
            /* 3 lines below: Used by GeoNavigationMenu*/
            Session["controller"] = "News";
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
                    ViewBag.Poll = pollDto;

                    var newsItemDtos = WS.GetNewestNewsItemsFromMunicipality(id, 12, pageNumberInt);

                    if (newsItemDtos.Length > 0)
                    {
                        ViewBag.ItemsToDisplay = true;
                        /* Settings used by the _Pager patial view. */
                        ViewBag.PagerType = "TwoParametersActionMethod";
                        ViewBag.ListObjectName = "News";
                        ViewBag.AreaIdentifier = id;
                        ViewBag.CurrentPage = pageNumberInt;
                        ViewBag.HasNextPageOfData = newsItemDtos[0].HasNextPageOfData;
                        ViewBag.Controller = "News";
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
                    return View(newsItemDtos);
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return View("Message");
            }
        }


        // **************************************
        // GET: /News/PostalCode/2900/2
        // **************************************
        public ActionResult PostalCode(string id, int? pageNumber)
        {
            /* 3 lines below: Used by GeoNavigationMenu*/
            Session["controller"] = "News";
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

                    var newsItemDtos = WS.GetNewestNewsItemsFromPostalCode(id, 12, pageNumberInt);

                    /* Setting the size of the map.*/
                    ViewBag.MapSize = "Medium";

                    if (newsItemDtos.Length > 0)
                    {
                        ViewBag.ItemsToDisplay = true;
                        /* Settings used by the _Pager patial view. */
                        ViewBag.PagerType = "TwoParametersActionMethod";
                        ViewBag.ListObjectName = "News";
                        ViewBag.AreaIdentifier = id;
                        ViewBag.CurrentPage = pageNumberInt;
                        ViewBag.HasNextPageOfData = newsItemDtos[0].HasNextPageOfData;
                        ViewBag.Controller = "News";
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
                    return View(newsItemDtos);
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
                Trace.TraceError("NewsController.GetBreakingNewsFromNewsStream(): " + ex.ToString());
            }
        }


        /// <summary>
        /// Gets the latest breaking NewsItem (Marked as IsLocalBreakingNews) from a User's news stream (From within the areas of 
        /// the Communities that a User is following) and which is not older than hoursToGoBack subtracted from the current time.
        /// (The variable hoursToGoBack is set within the method.)
        /// The eventual breaking NewsItem is displayed by the partial view _SystemMessage wich is residing
        /// in the layout page _LayoutBaseWrapper, located in the Views/Shared folder.
        /// </summary>
        private void GetBreakingNewsFromNewsStream(int userId)
        {
            try
            {
                using (var WS = new HLServiceClient())
                {
                    var dto = WS.GetBreakingNewsFromNewsStream(userId, 8);

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
                Trace.TraceError("NewsController.GetBreakingNewsFromPostalCode(): " + ex.ToString());
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
                Trace.TraceError("NewsController.GetTrendingNewsFromDkCountry(): " + ex.ToString());
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
                Trace.TraceError("NewsController.GetTrendingNewsFromRegion(): " + ex.ToString());
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
                Trace.TraceError("NewsController.GetTrendingNewsFromMunicipality(): " + ex.ToString());
            }
        }
    }
}
