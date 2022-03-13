using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using HLWebRole.HLServiceReference;
using System.Diagnostics;

namespace HLWebRole.Controllers
{
    public class AreaController : Controller
    {
        #region GeoNavigationMenu Methods

        // **************************************
        // GET: /Area/Index
        // **************************************
        public ActionResult Index()
        {
            /* Setting used for GeoNavigationMenu. */
            Session["controller"] = "Area";

            try
            {
                using (var WS = new HLServiceClient())
                {
                    /* Gets the latest trending NewsItem from within the area of the whole of Denmark and updates the UI. */
                    GetTrendingNewsFromDkCountry();

                    /* Getting the current Poll for the area, if there is such a one. */
                    var pollDto = WS.GetCurrentAnonymousPoll("Country");
                    ViewBag.Poll = pollDto;

                    var polygonDtos = WS.GetDKCountryPolygons();
                    return View(polygonDtos);
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return View("Message");
            }
        }


        // **************************************
        // GET: /Area/Region/Nordjylland
        // **************************************
        public ActionResult Region(string id)
        {
            /* Settings used for GeoNavigationMenu. */
            Session["controller"] = "Area";
            ViewBag.Region = id;
            ViewBag.REGIONNAVN = Session[id];

            try
            {
                using (var WS = new HLServiceClient())
                {
                    /* Gets the latest trending NewsItem from within the area of the Region and updates the UI. */
                    GetTrendingNewsFromRegion(id);

                    /* Getting the current Poll for the area, if there is such a one. */
                    var pollDto = WS.GetCurrentAnonymousPoll(id);
                    ViewBag.Poll = pollDto;

                    var polygonDtos = WS.GetRegionPolygons(id);
                    return View(polygonDtos);
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return View("Message");
            }
        }


        // **************************************
        // GET: /Area/Municipality/Gentofte
        // **************************************
        public ActionResult Municipality(string id)
        {
            /* Settings used for GeoNavigationMenu. */
            Session["controller"] = "Area";
            ViewBag.Municipality = id;
            ViewBag.KOMNAVN = Session[id];

            try
            {
                using (var WS = new HLServiceClient())
                {
                    /* Gets the latest trending NewsItem from within the area of the Municipality and updates the UI. */
                    GetTrendingNewsFromMunicipality(id);

                    /* Getting the current Poll for the area, if there is such a one. */
                    var pollDto = WS.GetCurrentAnonymousPoll(id);
                    ViewBag.Poll = pollDto;

                    var polygonDtos = WS.GetMunicipalityPolygons(id);
                    return View(polygonDtos);
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return View("Message");
            }
        }


        // **************************************
        // GET: /Area/PostalCode/2900
        // **************************************
        public ActionResult PostalCode(string id)
        {
            /* Settings used for GeoNavigationMenu. */
            Session["controller"] = "Area";
            ViewBag.POSTNR_TXT = id;
            ViewBag.POSTNR_TXT_POSTBYNAVN = Session[id];

            ViewBag.MapSize = "Medium";

            try
            {
                using (var WS = new HLServiceClient())
                {
                    /* Get and display eventual breaking news.*/
                    GetBreakingNewsFromPostalCode(id);

                    /* Getting the current Poll for the area, if there is such a one. */
                    var pollDto = WS.GetCurrentAnonymousPoll(id);
                    ViewBag.Poll = pollDto;

                    var polygonDtos = WS.GetPostalCodePolygons(id);

                    return View(polygonDtos);
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
                Trace.TraceError("AreaController.GetBreakingNewsFromPostalCode(): " + ex.ToString());
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
                Trace.TraceError("AreaController.GetTrendingNewsFromDkCountry(): " + ex.ToString());
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
                Trace.TraceError("AreaController.GetTrendingNewsFromRegion(): " + ex.ToString());
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
                Trace.TraceError("AreaController.GetTrendingNewsFromMunicipality(): " + ex.ToString());
            }
        }
    }
}
