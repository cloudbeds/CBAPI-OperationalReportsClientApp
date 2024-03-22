using System;
using System.Text;
using System.Text.Json;
using System.Collections.Generic;

/// <summary>
/// Parsing helpers for working with JSON
/// </summary>
static class JsonParseHelpers
{
    /// <summary>
    /// Pull out an string valued attribute
    /// </summary>
    /// <param name="jsonElement"></param>
    /// <param name="attributeName"></param>
    /// <returns></returns>
    public static string FindJasonAttributeValue_String(JsonElement jsonElement, string attributeName)
    {
        var jsonPropertyOrNull = FindSubPropertyWithName(jsonElement, attributeName);
        if(jsonPropertyOrNull == null) { return null; }

        return jsonPropertyOrNull.Value.Value.ToString();
     }

    /// <summary>
    /// Pull out a boolean valued attribute
    /// </summary>
    /// <param name="jsonElement"></param>
    /// <param name="attributeName"></param>
    /// <returns></returns>
    public static bool? FindJasonAttributeValue_BoolOrNull(JsonElement jsonElement, string attributeName)
    {
        var jsonPropertyOrNull = FindSubPropertyWithName(jsonElement, attributeName);
        if (jsonPropertyOrNull == null) { return null; }

        var asString = jsonPropertyOrNull.Value.Value.ToString().ToLower();
        if(asString == "true") { return true; }
        if (asString == "false") { return false; }

        throw new Exception("1022-556: Not true/false: " + asString);

    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="dateText"></param>
    /// <returns></returns>
    public static DateTime ParseDateFromJsonText(string dateText)
    {

        return DateTime.ParseExact(dateText, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Pull out an integer valued attribute
    /// </summary>
    /// <param name="jsonElement"></param>
    /// <param name="attributeName"></param>
    /// <returns></returns>
    public static int? FindJasonAttributeValue_IntegerOrNull(JsonElement jsonElement, string attributeName)
    {
        var jsonPropertyOrNull = FindSubPropertyWithName(jsonElement, attributeName);
        if (jsonPropertyOrNull == null) { return null; }

        var asString = jsonPropertyOrNull.Value.Value.ToString();
        return System.Convert.ToInt32(asString);
    }

    /// <summary>
    /// Pull out an decimal valued attribute
    /// </summary>
    /// <param name="jsonElement"></param>
    /// <param name="attributeName"></param>
    /// <returns></returns>
    public static decimal? FindJasonAttributeValue_DecimalOrNull(JsonElement jsonElement, string attributeName)
    {
        var jsonPropertyOrNull = FindSubPropertyWithName(jsonElement, attributeName);
        if (jsonPropertyOrNull == null) { return null; }

        var asString = jsonPropertyOrNull.Value.Value.ToString();
        return System.Convert.ToDecimal(asString, System.Globalization.CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Finds the property if it exists
    /// </summary>
    /// <param name="jsonElement"></param>
    /// <param name="propertyName"></param>
    /// <returns></returns>
    public static JsonProperty? FindSubPropertyWithName(JsonElement jsonElement, string propertyName)
    {
        foreach (var thisAttribute in jsonElement.EnumerateObject())
        {
            if (thisAttribute.Name == propertyName)
            {
                return thisAttribute;
            }

        }

        return null;
    }

}
