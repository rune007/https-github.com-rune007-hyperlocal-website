using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HLWebRole.Utilities
{
    /// <summary>
    /// Contains different utility methods.
    /// </summary>
    public static class HlUtility
    {
        /// <summary>
        /// Takes an URI with SAS and returns an URI without SAS.
        /// </summary>
        /// <param name="uri">URI with SAS.</param>
        /// <returns>URI without SAS.</returns>
        public static string GetUriWithoutSas(string uri)
        {
            var uriParts = uri.Split('?');
            return uriParts[0];
        }


        /// <summary>
        /// Is used to convert a Poll AreaIdentifier (e.g. string: "c_4") to the CommunityID it represents (e.g. int: 4).
        /// </summary>
        /// <param name="areaIdentifier">Takes in a Poll AreaIdentifier string: "c_4".</param>
        /// <returns>Returns an int which is the CommunityID contained in the AreaIdentifier, e.g. 4.</returns>
        public static int ExtractIdFromAreaIdentifier(string areaIdentifier)
        {
            // Removes the "c_" from the "c_4" AreaIdentifier.
            var id = areaIdentifier.Remove(0, 2);

            // Converts the "4" string to it's equivalent int 4.
            var idInt = Convert.ToInt32(id);

            return idInt;
        }


        /// <summary>
        /// Takes in a CommunityID (e.g. int 4) and returns an AreaIdentifier string (e.g. "c_4"), used by Poll objects.
        /// </summary>
        /// <param name="idInt">CommunityID, like 4.</param>
        /// <returns>Poll AreaIdentifier, like "c_4".</returns>
        public static string ConvertIdIntToAreaIdentifier(int idInt)
        {
            // Converts the int to string.
            var id = idInt.ToString();

            // Preprend the ID with "c_".
            var areaIdentifier = "c_" + id;

            return areaIdentifier;
        }

        /// <summary>
        /// Takes a nullable int and returns a positive int
        /// </summary>
        public static int ConvertNullableIntToPositiveInt(int? intValue)
        {
            if (intValue == null)
                intValue = 1;
            if (intValue < 1)
                intValue = 1;
            return Convert.ToInt32(intValue);
        }
    }
}
