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
    public class NotificationController : Controller
    {
        // **************************************
        // GET: /Notification/Edit
        // **************************************
        [Authorize]
        public ActionResult Edit()
        {
            var userId = Convert.ToInt32(Session["userId"]);

            try
            {
                using (var WS = new HLServiceReference.HLServiceClient())
                {
                    var userAlertDto = WS.GetUserAlerts(userId);

                    if (userAlertDto != null)
                        return View(userAlertDto);
                    else
                    {
                        ViewBag.Layout = "_LayoutUserHome.cshtml";
                        TempData["ErrorMessage"] = "Sorry we Don't have any User Alert Notification Settings On You.";
                        return View("Message");
                    }

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
        // POST: /Notification/Edit
        // **************************************
        [Authorize]
        [HttpPost]
        public ActionResult Edit(UserAlertDto dto)
        {
            var userId = Convert.ToInt32(Session["userId"]);

            try
            {
                using (var WS = new HLServiceReference.HLServiceClient())
                {
                    var status = WS.UpdateUserAlerts(dto);

                    if (status)
                    {
                        ViewBag.Layout = "_LayoutUserHome.cshtml";
                        TempData["SuccessMessage"] = "Your Notification Settings have been Updated!";
                        return View("Message");
                    }
                    else
                    {
                        ViewBag.Layout = "_LayoutUserHome.cshtml";
                        TempData["ErrorMessage"] = "Oops something went wrong :-( Your Notifications Was Not Updated :-(";
                        return View("Message");
                    }
                }
            }
            catch (Exception ex)
            {
                ViewBag.Layout = "_LayoutUserHome.cshtml";
                TempData["ErrorMessage"] = ex.Message;
                return View("Message");
            }
        }
    }
}
