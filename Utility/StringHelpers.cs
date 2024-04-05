using System;
using System.Text;
using System.Text.Json;
using System.Collections.Generic;

/// <summary>
/// Parsing helpers for working with String parsin
/// </summary>
static class StringHelpers
{

    /// <summary>
    /// Get rid of NULLs and cannonocalize blank strings
    /// </summary>
    /// <param name="stringIn"></param>
    /// <param name="defaultValue"></param>
    /// <returns></returns>
    public static string CannonicalizeBlankString(string stringIn, string defaultValue = "")
    {
        if(string.IsNullOrWhiteSpace(stringIn))
        {
            return defaultValue;
        }

        return stringIn;
    }
}
