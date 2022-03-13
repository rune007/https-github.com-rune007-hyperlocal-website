using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using HLWebRole.HLServiceReference;
using HLWebRole.Utilities;
using HLWebRole.Models;
using System.Diagnostics;

namespace HLWebRole.Controllers
{
    public class SearchController : Controller
    {
        ///// <summary>
        ///// Storing the current position latitude.
        ///// </summary>
        double currentPositionLatitude;
        ///// <summary>
        ///// Storing the current position longitude.
        ///// </summary>
        double currentPositionLongitude;


        // **************************************
        // GET: /Search/SearchAroundMe
        // **************************************
        public ActionResult SearchAroundMe()
        {
            return View();
        }


        // *************************************
        // POST: /Search/SearchAroundMe
        // *************************************
        [HttpPost]
        public ActionResult SearchAroundMe(double latitude, double longitude)
        {
            Session["currentPositionLatitude"] = latitude;
            Session["currentPositionLongitude"] = longitude;
            currentPositionLatitude = Convert.ToDouble(Session["currentPositionLatitude"]);
            currentPositionLongitude = Convert.ToDouble(Session["currentPositionLongitude"]);
            ViewBag.MapSize = "Large";
            

            try
            {
                using (var WS = new HLServiceClient())
                {
                    MakeReverseGeocodeRequest(currentPositionLatitude, currentPositionLongitude);

                    var newsItemDtos = WS.GetNewsItemsClosestToPosition(currentPositionLatitude, currentPositionLongitude, 14, 12, 1);

                    if (newsItemDtos.Length > 0)
                    {
                        ViewBag.ItemsToDisplay = true;
                        /* Settings used by the _Pager patial view. */
                        ViewBag.PagerType = "OneParameterActionMethod";
                        ViewBag.ListObjectName = "News";
                        ViewBag.CurrentPage = 1;
                        ViewBag.HasNextPageOfData = newsItemDtos[0].HasNextPageOfData;
                        ViewBag.Controller = "Search";
                        ViewBag.Action = "AroundMe";
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
                    return View("AroundMe", newsItemDtos);
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return View("Message");
            }
        }


        // **************************************
        // GET: /Search/AroundMe/2
        // **************************************
        public ActionResult AroundMe(int? id)
        {
            var idInt = HlUtility.ConvertNullableIntToPositiveInt(id);
            currentPositionLatitude = Convert.ToDouble(Session["currentPositionLatitude"]);
            currentPositionLongitude = Convert.ToDouble(Session["currentPositionLongitude"]);
            ViewBag.MapSize = "Large";

            try
            {
                using (var WS = new HLServiceClient())
                {
                    MakeReverseGeocodeRequest(currentPositionLatitude, currentPositionLongitude);

                    var newsItemDtos = WS.GetNewsItemsClosestToPosition(currentPositionLatitude, currentPositionLongitude, 14, 12, idInt);

                    if (newsItemDtos.Length > 0)
                    {
                        ViewBag.ItemsToDisplay = true;
                        /* Settings used by the _Pager patial view. */
                        ViewBag.PagerType = "OneParameterActionMethod";
                        ViewBag.ListObjectName = "News";
                        ViewBag.CurrentPage = idInt;
                        ViewBag.HasNextPageOfData = newsItemDtos[0].HasNextPageOfData;
                        ViewBag.Controller = "Search";
                        ViewBag.Action = "AroundMe";
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
        // GET: /Search/CommunityAroundMe/2
        // **************************************
        public ActionResult CommunityAroundMe(int? id)
        {
            var idInt = HlUtility.ConvertNullableIntToPositiveInt(id);
            currentPositionLatitude = Convert.ToDouble(Session["currentPositionLatitude"]);
            currentPositionLongitude = Convert.ToDouble(Session["currentPositionLongitude"]);
            ViewBag.MapSize = "Large";

            try
            {
                using (var WS = new HLServiceClient())
                {
                    MakeReverseGeocodeRequest(currentPositionLatitude, currentPositionLongitude);

                    var communityDtos = WS.GetCommunitiesClosestToPosition(currentPositionLatitude, currentPositionLongitude, 12, idInt);

                    /* These settings are used by the _Pager partial view. */
                    if (communityDtos.Length > 0)
                    {
                        ViewBag.ItemsToDisplay = true;
                        ViewBag.PagerType = "OneParameterActionMethod";
                        ViewBag.ListObjectName = "Communities";
                        ViewBag.CurrentPage = idInt;
                        ViewBag.HasNextPageOfData = communityDtos[0].HasNextPageOfData;
                        ViewBag.Controller = "Search";
                        ViewBag.Action = "CommunityAroundMe";
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
        // GET: /Search/UserAroundMe/2
        // **************************************
        public ActionResult UserAroundMe(int? id)
        {
            var idInt = HlUtility.ConvertNullableIntToPositiveInt(id);
            currentPositionLatitude = Convert.ToDouble(Session["currentPositionLatitude"]);
            currentPositionLongitude = Convert.ToDouble(Session["currentPositionLongitude"]);
            ViewBag.MapSize = "Large";

            try
            {
                using (var WS = new HLServiceClient())
                {
                    MakeReverseGeocodeRequest(currentPositionLatitude, currentPositionLongitude);

                    var userDtos = WS.GetUsersClosestToPosition(currentPositionLatitude, currentPositionLongitude, 12, idInt);

                    /* These settings are used by the _Pager partial view. */
                    if (userDtos.Length > 0)
                    {
                        ViewBag.ItemsToDisplay = true;
                        ViewBag.PagerType = "OneParameterActionMethod";
                        ViewBag.ListObjectName = "Users";
                        ViewBag.CurrentPage = idInt;
                        ViewBag.HasNextPageOfData = userDtos[0].HasNextPageOfData;
                        ViewBag.Controller = "Search";
                        ViewBag.Action = "UserAroundMe";
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


        // **************************************
        // GET: /Search/News
        // **************************************
        /// <summary>
        /// This is the initial view the user is presented with when they want to search NewsItems. It presents the user with an empty input form with search
        /// parameters. When the user presses the "Search" button the form is posted to the action method below: POST: News(), this action method redirects the 
        /// data to the action method GET ResultNews(). The redirection is done in order to be able to do paging in the search result. The same action method
        /// cannot both handle new searchs comming in via HTTP POST with a SearchNewsItemModel and handle paging which are done with HTTP GET.    
        /// </summary>
        public ActionResult News()
        {
            /* The size of map we display. */
            ViewBag.MapSize = "Broad";

            try
            {
                using (var WS = new HLServiceClient())
                {
                    /* The view has a map which is strongly typed to newsItemDtos, our search result will come in this form. 
                     Eventhough we don't transport any NewsItemDto in this action method, we need to transport an empty 
                     array, because the map view is strongly typed and expects a NewsItemDto[]. */
                    NewsItemDto[] newsItemDtos = new NewsItemDto[0];

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

                    /* The SearchNewsItemModel is used for controlling input information about parameters for searching NewsItems. Normally we would tranport this
                     * data via a strongly typed view, but in this case the view is already strongly typed to NewsItemDto[], which is the format we are receiving the 
                     * search result in. Because of this we transport the SearchNewsItemModel to the View via ViewBag.  */
                    var searchNewsItemModel = new SearchNewsItemModel();
                    var now = DateTime.Now;
                    var defaultDate = now.AddDays(-60);
                    searchNewsItemModel.CreateUpdateDate = defaultDate;
                    searchNewsItemModel.CategorySelectList = lstCategory;
                    searchNewsItemModel.AssignmentSelectList = lstAssignment;

                    ViewBag.SearchNewsItemModel = searchNewsItemModel;

                    /* Adjusting a nice start map. */
                    ViewBag.ItemsToDisplay = false;
                    var pointDto = WS.GetCenterPointOfDkCountry();
                    ViewBag.MapZoomLevel = 6;
                    ViewBag.AreaCenterLatitude = pointDto.Latitude;
                    ViewBag.AreaCenterLongitude = pointDto.Longitude;

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
        // POST: /Search/News
        // **************************************
        /// <summary>
        /// Every time the "Search" button is pressed, either in Search/News view (above) or in Search/ResultNews view (below), this is the action method the form is 
        /// posted to. But because this action method is designed to receive a SearchNewsItemModel via HTTP POST, we need to redirect to action ResultNews() in order
        /// to do paging in the search result. Because paging is done with HTTP GET requests like /Search/ResultNews/2.  Action ResultNews() is located just below.
        /// </summary>
        [HttpPost]
        public ActionResult News(SearchNewsItemModel model)
        {
            var searchNewsItemDto = new SearchNewsItemDto();

            TempData["SearchNewsItemModel"] = model;

            searchNewsItemDto.SearchRadius = model.SearchRadius;
            searchNewsItemDto.CategoryID = model.CategoryID;
            searchNewsItemDto.AssignmentID = model.AssignmentID;
            searchNewsItemDto.Title = model.Title;
            searchNewsItemDto.Story = model.Story;
            searchNewsItemDto.CreateUpdateDate = model.CreateUpdateDate;
            searchNewsItemDto.SearchCenterLatitude = model.Latitude;
            searchNewsItemDto.SearchCenterLongitude = model.Longitude;
            searchNewsItemDto.PageSize = 12;
            searchNewsItemDto.PageNumber = 1;

            TempData["SearchNewsItemDto"] = searchNewsItemDto;

            return RedirectToAction("ResultNews");
        }


        // **************************************
        // GET: /Search/ResultNews/2
        // **************************************
        /// <summary>
        /// This view displays the result of the NewsItem search, at the same time it's possible to adjust the search or make a new search from this view.
        /// </summary>
        public ActionResult ResultNews(int? id)
        {
            var idInt = HlUtility.ConvertNullableIntToPositiveInt(id);

            /* The size of map we display. */
            ViewBag.MapSize = "Broad";

            // SearchNewsItemModel is used by the view to take in search parameters.
            SearchNewsItemModel searchNewsItemModel = (SearchNewsItemModel)TempData["SearchNewsItemModel"];
            // We pass SearchNewsItemModel back to TempData because we want remember it when we page through the search result via the _Pager
            // partial view, with GET requests like: /Search/ResultNews/2.
            TempData["SearchNewsItemModel"] = searchNewsItemModel;
            ViewBag.SearchNewsItemModel = searchNewsItemModel;

            // SearchNewsItemDto is used to transport the search parameters back to the web service.
            SearchNewsItemDto searchNewsItemsDto = (SearchNewsItemDto)TempData["SearchNewsItemDto"];
            TempData["SearchNewsItemDto"] = searchNewsItemsDto;

            // Putting the current page number into searchNewsItemsDto, used by server side paging.
            searchNewsItemsDto.PageNumber = idInt;

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

                    searchNewsItemModel.CategorySelectList = lstCategory;
                    searchNewsItemModel.AssignmentSelectList = lstAssignment;

                    var newsItemDtos = WS.SearchNewsItems(searchNewsItemsDto);

                    if (newsItemDtos.Length > 0)
                    {
                        ViewBag.ItemsToDisplay = true;
                        /* Settings used by the _Pager patial view. */
                        ViewBag.PagerType = "OneParameterActionMethod";
                        ViewBag.ListObjectName = "News";
                        ViewBag.CurrentPage = idInt;
                        ViewBag.HasNextPageOfData = newsItemDtos[0].HasNextPageOfData;
                        ViewBag.Controller = "Search";
                        ViewBag.Action = "ResultNews";
                        /* Transfering the number of results which the search yields. */
                        searchNewsItemModel.NumberOfSearchResults = newsItemDtos[0].NumberOfSearchResults;
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
                    return View("ResultNews", newsItemDtos);
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return View("Message");
            }
        }


        // **************************************
        // GET: /Search/Community
        // **************************************
        /// <summary>
        /// This is the initial view the user is presented with when they want to search Communities. It presents the user with an empty input form with search
        /// parameters. When the user presses the "Search" button the form is posted to the action method below: POST: News(), this action method redirects the 
        /// data to the action method GET ResultCommunity(). The redirection is done in order to be able to do paging in the search result. The same action method
        /// cannot both handle new searchs comming in via HTTP POST with a SearchCommunityModel and handle paging which are done with HTTP GET.    
        /// </summary>
        public ActionResult Community()
        {
            /* The size of map we display. */
            ViewBag.MapSize = "Broad";

            try
            {
                using (var WS = new HLServiceClient())
                {
                    /* The view has a map which is strongly typed to requestDtos, our search result will come in this form. 
                     Eventhough we don't transport any CommunityDto in this action method, we need to transport an empty 
                     array, because the map view is strongly typed and expects a CommunityDto[]. */
                    CommunityDto[] communityDtos = new CommunityDto[0];

                    /* The SearchCommunityModel is used for controlling input information about parameters for searching Communities. Normally we would tranport this
                     * data via a strongly typed view, but in this case the view is already strongly typed to CommunityDto[], which is the format we are receiving the 
                     * search result in. Because of this we transport the SearchCommunityModel to the View via ViewBag.  */
                    var searchCommunityModel = new SearchCommunityModel();

                    ViewBag.SearchCommunityModel = searchCommunityModel;

                    /* Adjusting a nice start map. */
                    ViewBag.ItemsToDisplay = false;
                    var pointDto = WS.GetCenterPointOfDkCountry();
                    ViewBag.MapZoomLevel = 6;
                    ViewBag.AreaCenterLatitude = pointDto.Latitude;
                    ViewBag.AreaCenterLongitude = pointDto.Longitude;

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
        // POST: /Search/Community
        // **************************************
        /// <summary>
        /// Every time the "Search" button is pressed, either in Search/Community view (above) or in Search/ResultCommunity view (below), this is the action method the 
        /// form is posted to. But because this action method is designed to receive a SearchCommunityModel via HTTP POST, we need to redirect to action ResultCommunity() 
        /// in order to do paging in the search result. Because paging is done with HTTP GET requests like /Search/ResultCommunity/2. Action ResultCommunity() is located 
        /// just below.
        /// </summary>
        [HttpPost]
        public ActionResult Community(SearchCommunityModel model)
        {
            var searchCommunityDto = new SearchCommunityDto();

            TempData["SearchCommunityModel"] = model;

            searchCommunityDto.SearchRadius = model.SearchRadius;
            searchCommunityDto.Name = model.Name;
            searchCommunityDto.Description = model.Description;
            searchCommunityDto.SearchCenterLatitude = model.Latitude;
            searchCommunityDto.SearchCenterLongitude = model.Longitude;
            searchCommunityDto.PageSize = 12;
            searchCommunityDto.PageNumber = 1;

            TempData["SearchCommunityDto"] = searchCommunityDto;

            return RedirectToAction("ResultCommunity");
        }


        // **************************************
        // GET: /Search/ResultCommunity/2
        // **************************************
        /// <summary>
        /// This view displays the result of the Community search, at the same time it's possible to adjust the search or make a new search from this view.
        /// </summary>
        public ActionResult ResultCommunity(int? id)
        {
            var idInt = HlUtility.ConvertNullableIntToPositiveInt(id);

            /* The size of map we display. */
            ViewBag.MapSize = "Broad";

            // SearchCommunityModel is used by the view to take in search parameters.
            SearchCommunityModel searchCommunityModel = (SearchCommunityModel)TempData["SearchCommunityModel"];
            // We pass SearchCommunityModel back to TempData because we want remember it when we page through the search result via the _Pager
            // partial view, with GET requests like: /Search/ResultCommunity/2
            TempData["SearchCommunityModel"] = searchCommunityModel;
            ViewBag.SearchCommunityModel = searchCommunityModel;

            // SearchCommunityDto is used to transport the search parameters back to the web service.
            SearchCommunityDto searchCommunityDto = (SearchCommunityDto)TempData["SearchCommunityDto"];
            TempData["SearchCommunityDto"] = searchCommunityDto;

            // Putting the current page number into searchCommunityDto, used by server side paging.
            searchCommunityDto.PageNumber = idInt;

            try
            {
                using (var WS = new HLServiceClient())
                {
                    var communityDtos = WS.SearchCommunities(searchCommunityDto);

                    if (communityDtos.Length > 0)
                    {
                        ViewBag.ItemsToDisplay = true;
                        /* Settings used by the _Pager patial view. */
                        ViewBag.PagerType = "OneParameterActionMethod";
                        ViewBag.ListObjectName = "Communities";
                        ViewBag.CurrentPage = idInt;
                        ViewBag.HasNextPageOfData = communityDtos[0].HasNextPageOfData;
                        ViewBag.Controller = "Search";
                        ViewBag.Action = "ResultCommunity";
                        /* Transfering the number of results which the search yields. */
                        searchCommunityModel.NumberOfSearchResults = communityDtos[0].NumberOfSearchResults;
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
                    return View("ResultCommunity", communityDtos);
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return View("Message");
            }
        }




        // **************************************
        // GET: /Search/User
        // **************************************
        /// <summary>
        /// This is the initial view the user is presented with when they want to search Users. It presents the user with an empty input form with search
        /// parameters. When the user presses the "Search" button the form is posted to the action method below: POST: User(), this action method redirects the 
        /// data to the action method GET ResultUser(). The redirection is done in order to be able to do paging in the search result. The same action method
        /// cannot both handle new searchs comming in via HTTP POST with a SearchUserModel and handle paging which are done with HTTP GET.    
        /// </summary>
        public ActionResult User()
        {
            /* The size of map we display. */
            ViewBag.MapSize = "Broad";

            try
            {
                using (var WS = new HLServiceClient())
                {
                    /* The view has a map which is strongly typed to userDtos, our search result will come in this form. 
                     Eventhough we don't transport any UserDto in this action method, we need to transport an empty 
                     array, because the map view is strongly typed and expects a UserDto[]. */
                    UserDto[] userDtos = new UserDto[0];

                    /* The SearchUserModel is used for controlling input information about parameters for searching Users. Normally we would tranport this
                     * data via a strongly typed view, but in this case the view is already strongly typed to UserDto[], which is the format we are receiving the 
                     * search result in. Because of this we transport the SearchUserModel to the View via ViewBag.  */
                    var searchUserModel = new SearchUserModel();

                    ViewBag.SearchUserModel = searchUserModel;

                    /* Adjusting a nice start map. */
                    ViewBag.ItemsToDisplay = false;
                    var pointDto = WS.GetCenterPointOfDkCountry();
                    ViewBag.MapZoomLevel = 6;
                    ViewBag.AreaCenterLatitude = pointDto.Latitude;
                    ViewBag.AreaCenterLongitude = pointDto.Longitude;

                    return View(userDtos);
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return View("Message");
            }
        }


        // **************************************
        // POST: /Search/User
        // **************************************
        /// <summary>
        /// Every time the "Search" button is pressed, either in Search/User view (above) or in Search/ResultUser view (below), this is the action method the 
        /// form is posted to. But because this action method is designed to receive a SearchUserModel via HTTP POST, we need to redirect to action ResultUser() 
        /// in order to do paging in the search result. Because paging is done with HTTP GET requests like /Search/ResultUser/2. Action ResultUser() is located 
        /// just below.
        /// </summary>
        [HttpPost]
        public ActionResult User(SearchUserModel model)
        {
            var searchUserDto = new SearchUserDto();

            TempData["SearchUserModel"] = model;

            searchUserDto.SearchRadius = model.SearchRadius;
            searchUserDto.FirstName = model.FirstName;
            searchUserDto.LastName = model.LastName;
            searchUserDto.Bio = model.Bio;
            searchUserDto.Email = model.Email;
            searchUserDto.Address= model.UserAddress;
            searchUserDto.Phone = model.Phone;
            searchUserDto.SearchCenterLatitude = model.Latitude;
            searchUserDto.SearchCenterLongitude = model.Longitude;
            searchUserDto.PageSize = 12;
            searchUserDto.PageNumber = 1;

            TempData["SearchUserDto"] = searchUserDto;

            return RedirectToAction("ResultUser");
        }


        // **************************************
        // GET: /Search/ResultUser/2
        // **************************************
        /// <summary>
        /// This view displays the result of the User search, at the same time it's possible to adjust the search or make a new search from this view.
        /// </summary>
        public ActionResult ResultUser(int? id)
        {
            var idInt = HlUtility.ConvertNullableIntToPositiveInt(id);

            /* The size of map we display. */
            ViewBag.MapSize = "Broad";

            // SearchUserModel is used by the view to take in search parameters.
            SearchUserModel searchUserModel = (SearchUserModel)TempData["SearchUserModel"];
            // We pass SearchUserModel back to TempData because we want remember it when we page through the search result via the _Pager
            // partial view, with GET requests like: /Search/ResultUser/2.
            TempData["SearchUserModel"] = searchUserModel;
            ViewBag.SearchUserModel = searchUserModel;

            // SearchUserDto is used to transport the search parameters back to the web service.
            SearchUserDto searchUserDto = (SearchUserDto)TempData["SearchUserDto"];
            TempData["SearchUserDto"] = searchUserDto;

            // Putting the current page number into searchUserDto, used by server side paging.
            searchUserDto.PageNumber = idInt;

            try
            {
                using (var WS = new HLServiceClient())
                {
                    var userDtos = WS.SearchUsers(searchUserDto);

                    if (userDtos.Length > 0)
                    {
                        ViewBag.ItemsToDisplay = true;
                        /* Settings used by the _Pager patial view. */
                        ViewBag.PagerType = "OneParameterActionMethod";
                        ViewBag.ListObjectName = "Users";
                        ViewBag.CurrentPage = idInt;
                        ViewBag.HasNextPageOfData = userDtos[0].HasNextPageOfData;
                        ViewBag.Controller = "Search";
                        ViewBag.Action = "ResultUser";
                        /* Transfering the number of results which the search yields. */
                        searchUserModel.NumberOfSearchResults = userDtos[0].NumberOfSearchResults;
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
                    return View("ResultUser", userDtos);
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return View("Message");
            }
        }


        /// <summary>
        /// Makes a reverse geocode request (Finds an address for a lat/long location)  
        /// at Bing Maps SOAP Geocode Service
        /// http://dev.virtualearth.net/webservices/v1/geocodeservice/geocodeservice.svc 
        /// The method transport the result of the reverse geocode request to the view
        /// via ViewBag.ReverseGeocodeAddress.
        /// </summary>
        private void MakeReverseGeocodeRequest(double latitude, double longitude)
        {
            try
            {
                /* Calling SpatialQueryController to make a reverse geocode request (Getting an address string in reponse to a lat/long position) */
                using (var spatialQueryController = new SpatialQueryController())
                {
                    var reverseGeocodeAddress = spatialQueryController.MakeReverseGeocodeRequest(latitude, longitude);
                    ViewBag.ReverseGeocodeAddress = reverseGeocodeAddress;
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError("SearchController.MakeReverseGeocodeRequest(): " + ex.ToString());
            }
        }
    }
}
