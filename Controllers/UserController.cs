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
using System.Diagnostics;

namespace HLWebRole.Controllers
{
    public class UserController : Controller
    {
        // **************************************
        // GET: /User/5
        // **************************************
        public ActionResult Details(int id)
        {
            var userDto = new UserDto();

            try
            {
                using (var WS = new HLServiceClient())
                {
                    userDto = WS.GetUser(id);

                    // In case of no material is returned from the request, we render a "Not Found" view.
                    if (userDto == null)
                    {
                        @ViewBag.Message = "Not Found";
                        TempData["ErrorMessage"] = "Sorry the requested material cannot be found :-(";
                        return View("Message");
                    }
                    else
                    {
                        // The code lines below determines whether the viewing user and the user profile being viewed shares contact information.
                        // If they share contact information, that information will render in the view.
                        // If they don't share contact information, that information will not render in the view.
                        // It's determined by the bool areUsersSharingContactInfo, which is a field in the UserModel.
                        bool areUsersSharingContactInfo = false;
                        bool isContactInfoRequestPending = false;
                        if (Session["userId"] != null && Request.IsAuthenticated)
                        {
                            areUsersSharingContactInfo = WS.AreUsersSharingContactInfo(Convert.ToInt32(Session["userId"]), userDto.UserID);
                            isContactInfoRequestPending = WS.IsContactInfoRequestPending(Convert.ToInt32(Session["userId"]), userDto.UserID);
                        }

                        var userModel = new UserModel()
                        {
                            UserID = userDto.UserID,
                            Email = userDto.Email,
                            Password = userDto.Password,
                            FirstName = userDto.FirstName,
                            LastName = userDto.LastName,
                            Bio = userDto.Bio,
                            PhoneNumber = userDto.PhoneNumber,
                            Address = userDto.Address,
                            AddressPositionPointWkt = userDto.AddressPositionPointWkt,
                            CreateDate = userDto.CreateDate,
                            Latitude = userDto.Latitude,
                            Longitude = userDto.Longitude,
                            ImageBlobUri = userDto.ImageBlobUri,
                            MediumSizeBlobUri = userDto.MediumSizeBlobUri,
                            ThumbnailBlobUri = userDto.ThumbnailBlobUri,
                            AreUsersSharingContactInfo = areUsersSharingContactInfo,
                            IsContactInfoRequestPending = isContactInfoRequestPending
                        };

                        /* Presenting the latests NewsItems uploaded by the User. */
                        var newsItemDtos = WS.GetNewsItemsCreatedByUser(userDto.UserID, 12, 1);
                        var numberOfNewsItemsCreatedByUser = 0;
                        ViewBag.NewsItemDtos = newsItemDtos;
                        if (newsItemDtos.Length > 0)
                        {
                            numberOfNewsItemsCreatedByUser = newsItemDtos[0].NumberOfNewsItemsCreatedByUser;
                        }
                        ViewBag.NumberOfNewsItemsCreatedByUser = numberOfNewsItemsCreatedByUser.ToString();

                        /* The TempData below transport data to the Message/Send view when someone presses the Send Message button on
                         User/Details view. */
                        TempData["UserID"] = userDto.UserID;
                        TempData["UserName"] = userDto.FirstName + " " + userDto.LastName;
                        TempData["ThumbnailBlobUri"] = userDto.ThumbnailBlobUri;

                        return View(userModel);
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return View("Message");
            }
        }


        // **************************
        // GET: /User/Register
        // **************************
        public ActionResult Register()
        {
            var model = new UserModel();
            return View(model);
        }


        // **************************
        // POST AJAX: /User/Register
        // **************************
        [HttpPost]
        public JsonResult Register(UserModel model)
        {
            var userDto = new HLServiceReference.UserDto();
            userDto.Email = model.Email;
            userDto.Password = model.Password;
            userDto.FirstName = model.FirstName;
            userDto.LastName = model.LastName;
            userDto.Bio = model.Bio;
            userDto.PhoneNumber = model.PhoneNumber;
            userDto.Address = model.Address;
            userDto.RoleID = 1;
            userDto.Blocked = false;
            userDto.Latitude = model.Latitude;
            userDto.Longitude = model.Longitude;
            userDto.LastLoginLatitude = model.LastLoginLatitude;
            userDto.LastLoginLongitude = model.LastLoginLongitude;

            try
            {
                using (var WS = new HLServiceClient())
                {
                    var currentUserId = WS.CreateUser(userDto);

                    if (currentUserId > 0)
                    {
                        // We store the Users UserID in Session so we can use it around in the application.
                        Session["userId"] = currentUserId;
                        // Creates an FormsAuthentication authentication ticket for the user name (Email).
                        FormsAuthentication.SetAuthCookie(model.Email, false /* createPersistentCookie */);

                        return Json
                        (
                            new UserModel()
                            {
                                UserID = currentUserId
                            }
                        );
                    }
                    else
                    {
                        return Json
                        (
                            new SystemMessageModel()
                            {
                                SystemMessage = "Sorry the Registration was not successful :-("
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
        // GET: /User/Edit/5
        // **************************************
        public ActionResult Edit(int id)
        {
            var userDto = new UserDto();

            try
            {
                using (var WS = new HLServiceReference.HLServiceClient())
                {
                    userDto = WS.GetUser(id);

                    // In case of no material is returned from the request, we render a "Not Found" view.
                    if (userDto == null)
                    {
                        @ViewBag.Message = "Not Found";
                        TempData["ErrorMessage"] = "Sorry the requested material cannot be found :-(";
                        return View("Message");
                    }

                    // Checking whether a user which is not the owner is trying to edit information which they don't own.
                    if (userDto.UserID != Convert.ToInt32(Session["userId"]))
                    {
                        @ViewBag.Message = "Invalid Owner";
                        TempData["ErrorMessage"] = "Oops! You are trying to edit information which you don't own!";
                        return View("Message");
                    }

                    var userModel = new UserModel()
                    {
                        UserID = userDto.UserID,
                        Email = userDto.Email,
                        EmailEditView = userDto.Email,
                        Password = userDto.Password,
                        FirstName = userDto.FirstName,
                        LastName = userDto.LastName,
                        Bio = userDto.Bio,
                        PhoneNumber = userDto.PhoneNumber,
                        Address = userDto.Address,
                        AddressPositionPointWkt = userDto.AddressPositionPointWkt,
                        CreateDate = userDto.CreateDate,
                        Latitude = userDto.Latitude,
                        Longitude = userDto.Longitude,
                        ImageBlobUri = userDto.ImageBlobUri,
                    };

                    return View(userModel);
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return View("Message");
            }
        }


        // **************************
        // POST AJAX: /User/Edit
        // **************************
        [HttpPost]
        public JsonResult Edit(UserModel model)
        {
            var userDto = new HLServiceReference.UserDto();
            var currentUserId = model.UserID;
            userDto.UserID = model.UserID;
            userDto.Email = model.EmailEditView;
            userDto.FirstName = model.FirstName;
            userDto.LastName = model.LastName;
            userDto.Bio = model.Bio;
            userDto.PhoneNumber = model.PhoneNumber;
            userDto.Address = model.Address;
            userDto.Latitude = model.Latitude;
            userDto.Longitude = model.Longitude;

            try
            {
                using (var WS = new HLServiceClient())
                {
                    bool updateStatus = WS.UpdateUser(userDto);

                    if (updateStatus)
                    {
                        return Json
                        (
                            new UserModel()
                            {
                                UserID = currentUserId
                            }
                        );
                    }
                    else
                    {
                        return Json
                        (
                            new SystemMessageModel()
                            {
                                SystemMessage = "Sorry the update was not successful :-("
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
        // POST: /User/Delete
        // **************************************
        [HttpPost]
        [Authorize]
        public ActionResult Delete()
        {
            try
            {
                var userId = Convert.ToInt32(Session["userId"]);
                using (var WS = new HLServiceReference.HLServiceClient())
                {
                    var status = WS.DeleteUser(userId);

                    if (status)
                    {
                        FormsAuthentication.SignOut();
                        Session["userId"] = -1;
                        TempData["SuccessMessage"] = "Your Profile at hyperlocal was deleted. Goodbye! Hope to see you again another time ;-)";

                        return RedirectToAction("LogOn", "User");
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Sorry but something went wrong, we didn't manage to delete your User profile :-(";
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
        // POST AJAX: /User/Upload
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
                    var sasUri = WS.GetSasUriForBlobWrite(HLServiceReference.BusinessLogicMediaUsage.User, fileName);
                    var writeBlob = new CloudBlob(sasUri);
                    writeBlob.UploadFromStream(fileData.InputStream);

                    var uri = HlUtility.GetUriWithoutSas(sasUri);
                    WS.SaveMedia(hostItemId, uri, HLServiceReference.BusinessLogicMediaUsage.User);
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            return "ok";
        }


        // **************************************
        // GET: /User/LogOn
        // **************************************
        public ActionResult LogOn()
        {
            return View();
        }


        // **************************************
        // POST: /User/LogOn
        // **************************************
        [HttpPost]
        public ActionResult LogOn(LogOnModel model, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    using (var WS = new HLServiceReference.HLServiceClient())
                    {
                        // Checking whether the user has been blocked
                        if (WS.IsUserBlocked(model.Email, model.Password))
                            ModelState.AddModelError("", "You have been blocked! Contact Admin");

                        var userId = WS.UserLogin(model.Email, model.Password);

                        if (userId > 0)
                        {
                            /* Updates the latests login position. */
                            WS.UpdateLastLoginPosition(userId, model.LastLoginLatitude, model.LastLoginLongitude);

                            Membership.ValidateUser(model.Email, model.Password);
                            FormsAuthentication.SetAuthCookie(model.Email, model.RememberMe);
                            if (Url.IsLocalUrl(returnUrl) && returnUrl.Length > 1 && returnUrl.StartsWith("/")
                                && !returnUrl.StartsWith("//") && !returnUrl.StartsWith("/\\"))
                            {
                                return Redirect(returnUrl);
                            }
                            else
                            {
                                return RedirectToAction("Stream", "News");
                            }
                        }
                        else
                        {
                            ModelState.AddModelError("", "The user name or password provided is incorrect.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = ex.Message;
                }
            }

            // If we got this far, something failed, redisplay form.
            return View(model);
        }


        // **************************************
        // GET: /User/LogOff
        // **************************************
        public ActionResult LogOff()
        {
            FormsAuthentication.SignOut();
            Session["userId"] = -1;

            return RedirectToAction("LogOn", "User");
        }


        // **************************************
        // GET: /User/ChangePassword
        // **************************************
        [Authorize]
        public ActionResult ChangePassword()
        {
            return View();
        }


        // **************************************
        // POST: /User/ChangePassword
        // **************************************
        [Authorize]
        [HttpPost]
        public ActionResult ChangePassword(ChangePasswordModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    using (var WS = new HLServiceClient())
                    {
                        if (WS.ChangePassword(User.Identity.Name, model.OldPassword, model.NewPassword))
                        {
                            ViewBag.Layout = "_LayoutUserHome.cshtml";
                            TempData["SuccessMessage"] = "Your Password was changed :-)";
                            return View("Message");
                        }
                        else
                        {
                            ModelState.AddModelError("", "The current password is incorrect or the new password is invalid.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = ex.Message;
                }
            }
            // If we got this far, something failed, redisplay form
            return View(model);
        }


        // **************************************
        //// GET: /User/News/5/2
        // **************************************
        /// <summary>
        /// Presents the NewsItems created by the User.
        /// </summary>
        public ActionResult News(int id, int? pageNumber)
        {
            var userDto = new UserDto();

            var pageNumberInt = HlUtility.ConvertNullableIntToPositiveInt(pageNumber);

            ViewBag.MapSize = "Deep";

            try
            {
                using (var WS = new HLServiceClient())
                {
                    userDto = WS.GetUser(id);

                    // In case of no material is returned from the request, we render a "Not Found" view.
                    if (userDto == null)
                    {
                        @ViewBag.Message = "Not Found";
                        TempData["ErrorMessage"] = "Sorry the requested material cannot be found :-(";
                        return View("Message");
                    }
                    else
                    {
                        /* Transporting User data to the view via ViewBag. */
                        ViewBag.UserDto = userDto;

                        /* NewsItems */
                        var newsItemDtos = WS.GetNewsItemsCreatedByUser(id, 12, pageNumberInt);

                        /* These settings are used by the _Pager partial view. */
                        if (newsItemDtos.Length > 0)
                        {
                            ViewBag.ItemsToDisplay = true;
                            ViewBag.PagerType = "TwoParametersActionMethod";
                            ViewBag.ListObjectName = "News";
                            ViewBag.AreaIdentifier = id;
                            ViewBag.CurrentPage = pageNumberInt;
                            ViewBag.HasNextPageOfData = newsItemDtos[0].HasNextPageOfData;
                            ViewBag.Controller = "User";
                            ViewBag.Action = "News";
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
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return View("Message");
            }
        }


        // This action method is used by the remote validation attribute [Remote("DisallowDuplicateEmail", "User")],
        // which we have decorated the Email field with in our UserModel, it enforces client-side
        // validation that an email already existing in the database is not allowed into the system again.
        // *************************************************************
        // POST AJAX: /User/DisallowDuplicateEmail
        // *************************************************************
        [HttpPost]
        public ActionResult DisallowDuplicateEmail(string email)
        {
            try
            {
                using (var WS = new HLServiceClient())
                {
                    if (!WS.IsEmailInUse(email))
                    {
                        return Json(true, JsonRequestBehavior.AllowGet);
                    }
                    return Json(string.Format("Error! Email:  \"{0}\" is already registered in our database!", email), JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json("Oops! There was a problem: " + ex.Message, JsonRequestBehavior.AllowGet);
            }
        }


        // This action method is used by the remote validation attribute on EmailEditView field on the UserModel:
        // [Remote("DisallowDuplicateEmailEditView", "User", AdditionalFields = "Email", HttpMethod = "POST")]
        // In the Edit view, we want to allow the user to save their unchanged email address back into the system. This method compares the
        // existing email (email) with the email which is entered in the field in the Edit view (emailEditView). If they are the same we know it's OK, because
        // it's the users own email. If they are not the same, we check to see if the email (emailEditView) exists in the system already, and if it does, we
        // will not allow that it is entered into the system.
        // *************************************************************
        // POST AJAX: /User/DisallowDuplicateEmailEditView
        // *************************************************************
        [HttpPost]
        public ActionResult DisallowDuplicateEmailEditView(string emailEditView, string email)
        {
            if (email == emailEditView)
                return Json(true, JsonRequestBehavior.AllowGet);
            try
            {
                using (var WS = new HLServiceClient())
                {
                    if (!WS.IsEmailInUse(emailEditView))
                    {
                        return Json(true, JsonRequestBehavior.AllowGet);
                    }
                    return Json(string.Format("Error! Email:  \"{0}\" is already registered in our database!", emailEditView), JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json("Oops! There was a problem: " + ex.Message, JsonRequestBehavior.AllowGet);
            }
        }


        #region GeoNavigationMenu Methods

        // **************************************
        // GET: /User/Index/2
        // **************************************
        public ActionResult Index(int? id)
        {
            /* 1 line below: Used by GeoNavigationMenu*/
            Session["controller"] = "User";

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

                    var userDtos = WS.GetLatestActiveUsersFromDkCountry(12, idInt);

                    /* These settings are used by the _Pager partial view. */
                    if (userDtos.Length > 0)
                    {
                        ViewBag.ItemsToDisplay = true;
                        ViewBag.PagerType = "OneParameterActionMethod";
                        ViewBag.ListObjectName = "Users";
                        ViewBag.CurrentPage = idInt;
                        ViewBag.HasNextPageOfData = userDtos[0].HasNextPageOfData;
                        ViewBag.Controller = "User";
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
        // GET: /User/Region/Nordjylland/2
        // **************************************
        public ActionResult Region(string id, int? pageNumber)
        {
            /* 3 lines below: Used by GeoNavigationMenu*/
            Session["controller"] = "User";
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

                    var userDtos = WS.GetLatestActiveUsersFromRegion(id, 12, pageNumberInt);

                    /* These settings are used by the _Pager partial view. */
                    if (userDtos.Length > 0)
                    {
                        ViewBag.ItemsToDisplay = true;
                        ViewBag.PagerType = "TwoParametersActionMethod";
                        ViewBag.ListObjectName = "Users";
                        ViewBag.AreaIdentifier = id;
                        ViewBag.CurrentPage = pageNumberInt;
                        ViewBag.HasNextPageOfData = userDtos[0].HasNextPageOfData;
                        ViewBag.Controller = "User";
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
        // GET: /User/Municipality/Gentofte/2
        // **************************************
        public ActionResult Municipality(string id, int? pageNumber)
        {
            /* 3 lines below: Used by GeoNavigationMenu*/
            Session["controller"] = "User";
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

                    var userDtos = WS.GetLatestActiveUsersFromMunicipality(id, 12, pageNumberInt);

                    /* These settings are used by the _Pager partial view. */
                    if (userDtos.Length > 0)
                    {
                        ViewBag.ItemsToDisplay = true;
                        ViewBag.PagerType = "TwoParametersActionMethod";
                        ViewBag.ListObjectName = "Users";
                        ViewBag.AreaIdentifier = id;
                        ViewBag.CurrentPage = pageNumberInt;
                        ViewBag.HasNextPageOfData = userDtos[0].HasNextPageOfData;
                        ViewBag.Controller = "User";
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
        // GET: /User/PostalCode/2900/2
        // **************************************
        public ActionResult PostalCode(string id, int? pageNumber)
        {
            /* 3 lines below: Used by GeoNavigationMenu*/
            Session["controller"] = "User";
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

                    var userDtos = WS.GetLatestActiveUsersFromPostalCode(id, 12, pageNumberInt);

                    /* Setting the size of the map.*/
                    ViewBag.MapSize = "Medium";

                    /* These settings are used by the _Pager partial view. */
                    if (userDtos.Length > 0)
                    {
                        ViewBag.ItemsToDisplay = true;
                        ViewBag.PagerType = "TwoParametersActionMethod";
                        ViewBag.ListObjectName = "Users";
                        ViewBag.AreaIdentifier = id;
                        ViewBag.CurrentPage = pageNumberInt;
                        ViewBag.HasNextPageOfData = userDtos[0].HasNextPageOfData;
                        ViewBag.Controller = "User";
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
                    return View(userDtos);
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
        /// Gets the number of ContactInfoRequests + number of unread messages for the logged in User, this method is called from the layout page 
        /// _LayoutBase, to put the number of ContactInfoRequests + number of unread messages on the "My Stuff" tab menu item.
        /// E.g. "My Stuff (8)" indicating that there are 8 unread messages/ContactInfoRequests for the User to deal with.
        /// </summary>
        [Authorize]
        [ChildActionOnly]
        public string GetNumberOfRequestsAndUnreadMessagesToUser()
        {
            var userId = Convert.ToInt32(Session["userId"]);
            string numberOfRequestsAndUnreadMessagesString = "";

            try
            {
                using (var WS = new HLServiceClient())
                {
                    var numberOfRequestsAndUnreadMessages = WS.GetNumberOfRequestsAndUnreadMessagesToUser(userId);

                    if (numberOfRequestsAndUnreadMessages > 0)
                        numberOfRequestsAndUnreadMessagesString = " (" + numberOfRequestsAndUnreadMessages + ")";

                    return numberOfRequestsAndUnreadMessagesString;
                }
            }
            catch (Exception ex)
            {
                ViewBag.Layout = "_LayoutUserHome.cshtml";
                TempData["ErrorMessage"] = ex.Message;
                return numberOfRequestsAndUnreadMessagesString;
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
                Trace.TraceError("UserController.GetBreakingNewsFromPostalCode(): " + ex.ToString());
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
                Trace.TraceError("UserController.GetTrendingNewsFromDkCountry(): " + ex.ToString());
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
                Trace.TraceError("UserController.GetTrendingNewsFromRegion(): " + ex.ToString());
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
                Trace.TraceError("UserController.GetTrendingNewsFromMunicipality(): " + ex.ToString());
            }
        }
    }
}
