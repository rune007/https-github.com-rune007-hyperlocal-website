using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using HLWebRole.HLServiceReference;
using HLWebRole.Models;
using System.Diagnostics;

namespace HLWebRole.Controllers
{
    public class SpatialQueryController : Controller
    {
        // *************************************************************
        // POST AJAX: /SpatialQuery/IsLatLongWithinDenmark
        // *************************************************************
        [HttpPost]
        public ActionResult IsLatLongWithinDenmark(double latitude, double longitude)
        {
            if (latitude != 0 && longitude != 0)
            {
                try
                {
                    using (var WS = new HLServiceClient())
                    {
                        if (WS.IsLatLongWithinDenmark(latitude, longitude))
                        {
                            return Json(true, JsonRequestBehavior.AllowGet);
                        }
                        return Json(string.Format("Oops! That location is not in Denmark! We only allow Danish input!"), JsonRequestBehavior.AllowGet);
                    }
                }
                catch (Exception ex)
                {
                    return Json("Oops! There was a problem: " + ex.Message, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                return Json(string.Format("Oops! That location is not in Denmark! We only allow Danish input!"), JsonRequestBehavior.AllowGet);
            }
        }


        // *************************************************************
        // POST AJAX: /SpatialQuery/IsPolygonWithinDenmark
        // *************************************************************
        [HttpPost]
        public ActionResult IsPolygonWithinDenmark(string polygonWkt)
        {
            try
            {
                using (var WS = new HLServiceClient())
                {
                    if (WS.IsGeographyInDenmark(polygonWkt))
                    {
                        return Json(true, JsonRequestBehavior.AllowGet);
                    }
                    return Json(string.Format("Polygon is not in Denmark, We only allow Danish content!"), JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json("Oops! There was a problem: " + ex.Message, JsonRequestBehavior.AllowGet);
            }
        }


        // ***************************************************
        // POST AJAX: /SpatialQuery/IsPolygonValid
        // ***************************************************
        [HttpPost]
        public ActionResult IsPolygonValid(string polygonWktB)
        {
            try
            {
                using (var WS = new HLServiceClient())
                {
                    if (WS.IsPolygonValid(polygonWktB))
                    {
                        return Json(true, JsonRequestBehavior.AllowGet);
                    }
                    return Json(string.Format("Sorry! We Can't Save that Polygon, it's Not Valid!"), JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json("Oops! There was a problem: " + ex.Message, JsonRequestBehavior.AllowGet);
            }
        }


        // **********************************************************
        // POST AJAX: /SpatialQuery/IsPolygonAreaTooBig
        // **********************************************************
        [HttpPost]
        public ActionResult IsPolygonAreaTooBig(string polygonWktC)
        {
            try
            {
                using (var WS = new HLServiceClient())
                {
                    if (!WS.IsPolygonTooBig(polygonWktC))
                    {
                        return Json(true, JsonRequestBehavior.AllowGet);
                    }
                    return Json(string.Format("That Area is Too Big! It exceeds the 15 SQ KM Limit!"), JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json("Oops! There was a problem: " + ex.Message, JsonRequestBehavior.AllowGet);
            }
        }


        /// <summary>
        /// Makes a reverse geocode request (Finds an address for a lat/long location)  
        /// at Bing Maps SOAP Geocode Service
        /// http://dev.virtualearth.net/webservices/v1/geocodeservice/geocodeservice.svc 
        /// </summary>
        public string MakeReverseGeocodeRequest(double latitude, double longitude)
        {
            string results = "";
            try
            {
                // Set a Bing Maps key before making a request
                string key = "Atx6C4-6W6IofeQA3RsyCWSbKeE0Z9QVrmFxNIMagT0rrxQF6_R25A1d-N_r0bXY";

                GeocodeService.ReverseGeocodeRequest reverseGeocodeRequest = new GeocodeService.ReverseGeocodeRequest();

                // Set the credentials using a valid Bing Maps key
                reverseGeocodeRequest.Credentials = new GeocodeService.Credentials();
                reverseGeocodeRequest.Credentials.ApplicationId = key;

                // Set the point to use to find a matching address
                GeocodeService.Location point = new GeocodeService.Location();
                point.Latitude = latitude;
                point.Longitude = longitude;

                reverseGeocodeRequest.Location = point;

                // Make the reverse geocode request
                GeocodeService.GeocodeServiceClient geocodeService =
                new GeocodeService.GeocodeServiceClient("BasicHttpBinding_IGeocodeService");
                GeocodeService.GeocodeResponse geocodeResponse = geocodeService.ReverseGeocode(reverseGeocodeRequest);

                if (geocodeResponse.Results.Length > 0)
                {
                    results = geocodeResponse.Results[0].DisplayName;
                }
                return results;
            }
            catch (Exception ex)
            {
                Trace.TraceError("SpatialQueryController.MakeReverseGeocodeRequest(): " + ex.ToString());
                return null;
            }
        }
    }
}
