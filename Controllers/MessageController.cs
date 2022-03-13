using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using HLWebRole.Models;
using HLWebRole.HLServiceReference;
using HLWebRole.Utilities;

namespace HLWebRole.Controllers
{
    public class MessageController : Controller
    {
        // **************************************
        // GET: /Message/Send
        // **************************************
        [Authorize]
        public ActionResult Send()
        {
            var model = new MessageModel();

            if (TempData["UserID"] != null)
            {
                model.SenderUserID = Convert.ToInt32(Session["userId"]);
                model.PartitionKey = Convert.ToString(TempData["UserID"]);
                model.ReceiverUserName = Convert.ToString(TempData["UserName"]);
                model.ReceiverThumbnailBlobUri = Convert.ToString(TempData["ThumbnailBlobUri"]);
                return View(model);
            }
            else
            {
                TempData["ErrorMessage"] = "Oops, we can't send message, because there wasn't chosen anybody to send the message to!";
                return View("Message");
            }
        }


        // **************************************
        // POST: /Message/Send
        // **************************************
        [HttpPost]
        [Authorize]
        public ActionResult Send(MessageModel model)
        {
            try
            {
                using (var WS = new HLServiceClient())
                {
                    WS.SendMessage(Convert.ToInt32(model.PartitionKey), model.SenderUserID, model.Subject, model.MessageBody);

                    ViewBag.Layout = "_LayoutUserHome.cshtml";
                    TempData["SuccessMessage"] = "Your Message to " + model.ReceiverUserName + " was sent :-)";
                    return View("Message");
                }
            }
            catch (Exception ex)
            {
                ViewBag.Layout = "_LayoutUserHome.cshtml";
                TempData["ErrorMessage"] = ex.Message;
                return View("Message");
            }
        }


        // *****************************************************************************
        // GET: /Message/Read/12520676876004726959_8c2829ad-31e0-4432-9d45-ff11b6e1684f
        // *****************************************************************************
        /// <param name="id">
        /// The id parameter is the RowKey of the MessageEntityModel, it is a reversed DateTime.Now.Ticks
        /// with a GUI appended, separated with an underscore: [ReversedDateTimeNowTicks]_[GUI]
        /// </param>
        /// <returns></returns>
        [Authorize]
        public ActionResult Read(string id)
        {
            // We use a string version of the UserID as PartitionKey for the MessageEntityModel.
            var userId = Session["userId"].ToString();

            try
            {
                using (var WS = new HLServiceClient())
                {
                    var messageDto = WS.GetMessage(userId, id);

                    // In case of no material is returned from the request, we render a "Not Found" view.
                    if (messageDto == null)
                    {
                        @ViewBag.Message = "Not Found";
                        TempData["ErrorMessage"] = "Sorry the requested material cannot be found :-(";
                        return View("Message");
                    }

                    /* Marking the message as read. */
                    WS.MarkMessageAsRead(userId, id);

                    ViewBag.MessageDto = messageDto;

                    // A new MessageModel for an eventual reply message to the message. We put into it necessary information to be able to 
                    // send it. Namely PartionKey (The UserID of the receiver), SenderUserID, and ReceivedUserName
                    var messageModel = new MessageModel();
                    messageModel.PartitionKey = messageDto.SenderUserID.ToString();
                    messageModel.SenderUserID = Convert.ToInt32(userId);
                    messageModel.ReceiverUserName = messageDto.SenderUserName;

                    return View(messageModel);
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return View("Message");
            }
        }


        // ***********************************************************************************
        // GET: /Message/Details/12520676876004726959_8c2829ad-31e0-4432-9d45-ff11b6e1684f/11
        // ***********************************************************************************
        /// <summary>
        /// The route matching this action method is: /Message/Details/{RowKey}/{PartitionKey}
        /// </summary>
        /// <param name="id">
        /// The id parameter is the RowKey of the MessageEntityModel, it is a reversed DateTime.Now.Ticks
        /// with a GUI appended, separated with an underscore: {ReversedDateTimeNowTicks}_{GUI}
        /// </param>
        /// <param name="pageNumber">
        /// THE PAGENUMBER PARAMETER BELOW IS ACTUALLY NOT A PAGENUMBER PARAMETER! IT IS THE PARTITIONKEY OF THE AZURE TABLE ENTITY! 
        /// The reason for this confusion is that, early on in the development of the project, I mapped the route "DefaultPaging" in the 
        /// Global.asax.cs file, I mapped this route to support pagination. Later in the project I got the need to support this Details 
        /// view to Azure table storage entities, and the Azure table entity is identified by both a PartitionKey and a RowKey. The route 
        /// "DefaultPaging" was actually ok for me to use for the Azure table entity Details view, the only problem was the name of the parameter 
        /// "pageNumber". It was too cumbersome to change the name of the parameter at this later stage. Because I used it in a lot of action methods 
        /// and links around the application. I should have chosen a more general name than "pageNumber", e.g. "identifier", when I originally mapped 
        /// the route. Because the route is ok for my purpose, it' only the name of the parameter which is too specific.
        /// </param>
        [Authorize]
        public ActionResult Details(string id, int pageNumber)
        {
            /* The pageNumber is in reality the PartitionKey of the Azure table entity, for explanation look above. */
            var partitionKey = pageNumber.ToString();

            try
            {
                using (var WS = new HLServiceClient())
                {
                    var messageDto = WS.GetMessage(partitionKey, id);

                    // In case of no material is returned from the request, we render a "Not Found" view.
                    if (messageDto == null)
                    {
                        @ViewBag.Message = "Not Found";
                        TempData["ErrorMessage"] = "Sorry the requested material cannot be found :-(";
                        return View("Message");
                    }
                    return View(messageDto);
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return View("Message");
            }
        }


        // **************************************
        // GET: /Message/Inbox
        // **************************************
        [Authorize]
        public ActionResult Inbox(int? id)
        {
            var idInt = HlUtility.ConvertNullableIntToPositiveInt(id);
            var userId = Convert.ToInt32(Session["userId"]);
            if (userId > 0)
            {
                try
                {
                    using (var WS = new HLServiceClient())
                    {
                        var messages = WS.GetInboxContent(userId, 12, idInt);

                        var numberOfUnreadMessages = WS.GetNumberOfUnreadMessages(userId);
                        ViewBag.NumberOfUnreadMessages = numberOfUnreadMessages;
                        ViewBag.NumberOfUnreadMessagesString = Convert.ToString(numberOfUnreadMessages);

                        /* These settings are used by the _Pager partial view. */
                        if (messages.Length > 0)
                        {
                            ViewBag.PagerType = "OneParameterActionMethod";
                            ViewBag.ListObjectName = "Messages";
                            ViewBag.CurrentPage = idInt;
                            ViewBag.HasNextPageOfData = messages[0].HasNextPageOfData;
                            ViewBag.Controller = "Message";
                            ViewBag.Action = "Inbox";
                        }


                        return View(messages);
                    }
                }
                catch (Exception ex)
                {
                    ViewBag.Layout = "_LayoutUserHome.cshtml";
                    TempData["ErrorMessage"] = ex.Message;
                    return View("Message");
                }
            }
            else
            {
                TempData["ErrorMessage"] = "You need to be logged in to access your Inbox!";
                return View("Message");
            }
        }


        // **************************************
        // GET: /Message/Outbox
        // **************************************
        [Authorize]
        public ActionResult Outbox(int? id)
        {
            var idInt = HlUtility.ConvertNullableIntToPositiveInt(id);
            var userId = Convert.ToInt32(Session["userId"]);
            if (userId > 0)
            {
                try
                {
                    using (var WS = new HLServiceClient())
                    {
                        var messages = WS.GetOutboxContent(userId, 12, idInt);

                        /* These settings are used by the _Pager partial view. */
                        if (messages.Length > 0)
                        {
                            ViewBag.PagerType = "OneParameterActionMethod";
                            ViewBag.ListObjectName = "Messages";
                            ViewBag.CurrentPage = idInt;
                            ViewBag.HasNextPageOfData = messages[0].HasNextPageOfData;
                            ViewBag.Controller = "Message";
                            ViewBag.Action = "Outbox";
                        }
                        return View(messages);
                    }
                }
                catch (Exception ex)
                {
                    ViewBag.Layout = "_LayoutUserHome.cshtml";
                    TempData["ErrorMessage"] = ex.Message;
                    return View("Message");
                }
            }
            else
            {
                TempData["ErrorMessage"] = "You need to be logged in to access your Outbox!";
                return View("Message");
            }
        }


        // ***************************************
        // POST AJAX: /Message/DeleteMessageInbox
        // ***************************************
        [HttpPost]
        [Authorize]
        public JsonResult DeleteMessageInbox(string rowKey)
        {
            var deleted = false;
            var partitionKey = Convert.ToString(Session["userId"]);

            try
            {
                using (var WS = new HLServiceReference.HLServiceClient())
                {
                    deleted = WS.DeleteMessage(BusinessLogicMessageOwner.Receiver, partitionKey, rowKey);

                    if (deleted)
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
                                SystemMessage = "Sorry message was not deleted :-("
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


        // ****************************************
        // POST AJAX: /Message/DeleteMessageOutbox
        // ****************************************
        [HttpPost]
        [Authorize]
        public JsonResult DeleteMessageOutbox(string partitionKey, string rowKey)
        {
            var deleted = false;

            try
            {
                using (var WS = new HLServiceReference.HLServiceClient())
                {
                    deleted = WS.DeleteMessage(BusinessLogicMessageOwner.Sender, partitionKey, rowKey);

                    if (deleted)
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
                                SystemMessage = "Sorry message was not deleted :-("
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


        /// <summary>
        /// Gets the number of unread Messages in a Users inbox, this method is called from the layout page
        /// _LayoutUserHome, to put the number of unread Messages on the Inbox tab menu item.
        /// E.g. "Inbox (8)" indicating that there are 8 unread messages for the User to deal with.
        /// </summary>
        [Authorize]
        [ChildActionOnly]
        public string GetNumberOfUnreadMessages()
        {
            var userId = Convert.ToInt32(Session["userId"]);
            string numberOfUnreadMessagesString = "";

            try
            {
                using (var WS = new HLServiceClient())
                {
                    var numberOfUnreadMessages = WS.GetNumberOfUnreadMessages(userId);

                    if (numberOfUnreadMessages > 0)
                        numberOfUnreadMessagesString = " (" + numberOfUnreadMessages + ")";

                    return numberOfUnreadMessagesString;
                }
            }
            catch (Exception ex)
            {
                ViewBag.Layout = "_LayoutUserHome.cshtml";
                TempData["ErrorMessage"] = ex.Message;
                return numberOfUnreadMessagesString;
            }
        }
    }
}
