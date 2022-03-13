using System;
using System.Text;
using System.Web.Mvc;
using HLWebRole.Models;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Web.Script.Serialization;
using System.Runtime.Serialization;

namespace HLWebRole.Utilities
{
    public static class HtmlHelpers
    {
        /// <summary>
        /// Is used to serialize a collection of model objects (Razor syntax in the view) into JavaScript
        /// so that we can iterate through the model (Razor syntax), which is passed to the view, in JavaScript.
        /// We use this in conjunction with rendering of spatial data on the Bing map. For example, the partial view
        /// _AreaMap is strongly typed and receives a collection of PolygonDtos with PolygonWkt, but in order to get this data
        /// to the JavaScript (which converts the PolygonWkt to VEShapes) we need to serialize the data to a format which
        /// JavaScript can deal with (it can deal directly with the MVC PolygonDto).
        /// </summary>
        /// <param name="helper"></param>
        /// <param name="value">IEnumerable of MVC models (Razor syntax)</param>
        /// <returns>JSON (JavaScript Object Notation) version of the IEnumerable of MVC models (Razor syntax)</returns>
        public static MvcHtmlString JSValue(this HtmlHelper helper, object value)
        {
            return MvcHtmlString.Create(serializeValue(value));
        }


        private static string serializeValue(object value)
        {
            var varValue = "";
            if (value == null) varValue = "null";
            else if (value is string) varValue = quoteString(value.ToString());
            else if (value is DateTime) value = quoteString(Convert.ToString((DateTime)value));
            else if (value is DateTime?) value = quoteString(Convert.ToString((DateTime?)value));
            else if (value is bool || value is bool?) varValue = value.ToString().ToLower();
            else if (value is int || value is int?
                    || value is byte || value is byte?
                    || value is long || value is long?
                    || value is float || value is float?
                    || value is double || value is double?
                    || value is decimal || value is decimal) varValue = value.ToString();
            else if (value is char || value is char?) varValue = quoteString(value.ToString());
            else
            {
                var sb = new StringBuilder();
                var serializer = new JavaScriptSerializer();
                serializer.Serialize(value, sb);
                varValue = sb.ToString();
            }
            return varValue;
        }


        private static string quoteString(string s)
        {
            return "'"
                + s
                    .Replace("\\", "\\\\")
                    .Replace("'", "\'")
                    .Replace("\n", "\\n")
                    .Replace("\r", "\\r")
                + "'";
        }


        /// <summary>
        /// Used for truncating text.
        /// </summary>
        public static string Truncate(this HtmlHelper helper, string input, int length)
        {
            if (input.Length <= length)
            {
                return input;
            }
            else
            {
                return input.Substring(0, length) + "...";
            }
        }
    }
}