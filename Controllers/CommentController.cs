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

namespace HLWebRole.Controllers
{
    public class CommentController : Controller
    {
        // **********************************************************
        // POST AJAX: /Comment/Create
        // **********************************************************
        [HttpPost]
        [Authorize]
        public JsonResult Create(int newsItemId, string commentBody)
        {
            var commentDto = new CommentDto();
            var userId = Convert.ToInt32(Session["userId"]);
            var commentModel = new CommentModel();

            try
            {
                using (var WS = new HLServiceReference.HLServiceClient())
                {
                    commentDto = WS.CreateComment(newsItemId, userId, commentBody);
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

            if (commentDto != null)
            {
                return Json
                (
                    new CommentModel()
                    {
                        RowKey = commentDto.RowKey,
                        PostedByUserID = commentDto.PostedByUserID,
                        PostedByUserName = commentDto.PostedByUserName,
                        CommentBody = commentDto.CommentBody,
                        CreateDate = Convert.ToString(commentDto.CreateDate),
                        ThumbnailBlobUri = commentDto.ThumbnailBlobUri
                    }
                );
            }
            else
            {
                return Json
                (
                    new SystemMessageModel()
                    {
                        SystemMessage = "Sorry the Comment was not added:-("
                    }
                );
            }
        }


        // **********************************************************
        // POST AJAX: /Comment/Delete
        // **********************************************************
        [HttpPost]
        public JsonResult Delete(int newsItemId, string rowKey)
        {
            var success = false;

            try
            {
                using (var WS = new HLServiceReference.HLServiceClient())
                {
                    success = WS.DeleteComment(newsItemId, rowKey);
                }
                if (success)
                {
                    return Json
                    (
                        new CommentModel()
                        {
                            RowKey = rowKey
                        }
                    );
                }
                else
                {
                    return Json
                    (
                        new SystemMessageModel()
                        {
                            SystemMessage = "Sorry the Comment was not Deleted:-("
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
    }
}
