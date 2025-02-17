﻿/// This script is basically the same as map-find-location.js, the only difference is that it does not uses geo validation to ensure that a location is in Denmark,
/// this functionality is necessary in connection with User input, but it's not necessary in connection with search.
/// My Map implementation is based on modyfying code I found in the book:
/// Haack/Hanselman/Guthrie/Conery, "Professional ASP.NET MVC 1", Willey Publishing, USA 2009, p. 146 -  156.

/// <reference path="jquery-1.5.1.min.js" />

var map = null;
var points = [];
var shapes = [];
var center = null;
var radiusInMeters = 0;
var theCategoryId = 0;

function LoadMap(latitude, longitude, onMapLoaded) {
    map = new VEMap('theMap');
    options = new VEMapOptions();
    options.EnableBirdseye = false;

    // Makes the control bar less obtrusize.
    map.SetDashboardSize(VEDashboardSize.Small);

    if (onMapLoaded != null)
        map.onLoadMap = onMapLoaded;

    if (latitude != null && longitude != null) {
        center = new VELatLong(latitude, longitude);
    }

    map.LoadMap(center, null, null, null, null, null, null, options);
}

function LoadPin(LL, name, description) {
    var shape = new VEShape(VEShapeType.Pushpin, LL);

    //Make a nice Pushpin shape with a title and description
    shape.SetTitle("<span class=\"pinTitle\"> " + escape(name) + "</span>");
    if (description !== undefined) {
        shape.SetDescription("<p class=\"pinDetails\">" +
        escape(description) + "</p>");
    }

    // Making the Pushpin draggable.
    shape.Draggable = true;
    shape.onenddrag = EndDragHandler;


    map.AddShape(shape);
    points.push(LL);
    shapes.push(shape);
}

function FindAddressOnMap(where) {
    var numberOfResults = 20;
    var setBestMapView = true;
    var showResults = true;

    map.Find("", where, null, null, null,
           numberOfResults, showResults, true, true,
           setBestMapView, callbackForLocation);
}

function callbackForLocation(layer, resultsArray, places,
            hasMore, VEErrorMessage) {

    clearMap();

    if (places == null)
        return;

    //Make a pushpin for each place we find
    $.each(places, function (i, item) {
        var description = "";
        if (item.Description !== undefined) {
            description = item.Description;
        }
        var LL = new VELatLong(item.LatLong.Latitude,
                        item.LatLong.Longitude);

        LoadPin(LL, item.Name, description);
    });

    //Make sure all pushpins are visible
    if (points.length > 1) {
        map.SetMapView(points);
    }

    //If we've found exactly one place, that's our address.
    if (points.length === 1) {
        $("#Latitude").val(points[0].Latitude);
        $("#Longitude").val(points[0].Longitude);

        /* This validates our Latitude field (Remote validation checking whether Lat/Long are within Danish territory. */
        /*$('form').validate().element("#Latitude");*/
    }
}

function clearMap() {
    map.Clear();
    points = [];
    shapes = [];
}