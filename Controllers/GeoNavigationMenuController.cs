using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using HLWebRole.HLServiceReference;


namespace HLWebRole.Controllers
{
    public class GeoNavigationMenuController : Controller
    {
        [ChildActionOnly]
        public ActionResult _IndexMenu()
        {
            try
            {
                using (var WS = new HLServiceReference.HLServiceClient())
                {
                    var regions = WS.GetAllRegions();
                    return PartialView(regions);
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return PartialView("_SystemMessage");
                //return View();
            }
        }


        [ChildActionOnly]
        public ActionResult _RegionMenu(string region)
        {
            try
            {
                using (var WS = new HLServiceReference.HLServiceClient())
                {
                    var municipalities = WS.GetMunicipalitiesForRegion(region);
                    return PartialView(municipalities);
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return PartialView("_SystemMessage");
            }
        }


        [ChildActionOnly]
        public ActionResult _MunicipalityMenu(string municipality)
        {
            try
            {
                using (var WS = new HLServiceReference.HLServiceClient())
                {
                    var postalCodes = WS.GetPostalCodesForMunicipality(municipality);
                    return PartialView(postalCodes);
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return PartialView("_SystemMessage");
            }
        }


        public ActionResult PostalCodeMenu(string postalCode)
        {
            return View();
        }
    }
}
