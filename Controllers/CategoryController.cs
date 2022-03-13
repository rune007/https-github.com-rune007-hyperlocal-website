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
    public class CategoryController : Controller
    {

        // **************************************
        // GET: /Category/Index
        // **************************************
        public ActionResult Index()
        {
            try
            {
                using (var WS = new HLServiceClient())
                {
                    var categoryDtos = WS.GetNewsItemCategoriesIndex();

                    return View(categoryDtos);
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return View("Message");
            }
        }


        public ActionResult Details(int id, int? pageNumber)
        {
            var categoryDto = new NewsItemCategoryIndexDto();

            var pageNumberInt = HlUtility.ConvertNullableIntToPositiveInt(pageNumber);

            ViewBag.MapSize = "Slim";

            try
            {
                using (var WS = new HLServiceClient())
                {
                    categoryDto = WS.GetNewsItemCategory(id);

                    // In case of no material is returned from the request, we render a "Not Found" view.
                    if (categoryDto == null)
                    {
                        @ViewBag.Message = "Not Found";
                        TempData["ErrorMessage"] = "Sorry the requested material cannot be found :-(";
                        return View("Message");
                    }
                    else
                    {
                        /* NewsItemCategory */
                        ViewBag.CategoryDto = categoryDto;

                        /* NewsItems */
                        var newsItemDtos = WS.GetNewestNewsItemsInCategory(id, 12, pageNumberInt);

                        /* These settings are used by the _Pager partial view. */
                        if (newsItemDtos.Length > 0)
                        {
                            ViewBag.ItemsToDisplay = true;
                            ViewBag.PagerType = "TwoParametersActionMethod";
                            ViewBag.ListObjectName = "News";
                            ViewBag.AreaIdentifier = id;
                            ViewBag.CurrentPage = pageNumberInt;
                            ViewBag.HasNextPageOfData = newsItemDtos[0].HasNextPageOfData;
                            ViewBag.Controller = "Category";
                            ViewBag.Action = "Details";
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
    }
}
