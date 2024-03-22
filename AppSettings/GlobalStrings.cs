//==================================================================================
// Copyright © 2018 Ivo Salmre.  ALL RIGHTS RESERVED.  NO WARRANTY OF ANY KIND.
// No part of this code may be used, copied or modified in any way without explicit
// written permission.
//==================================================================================
using System;
using System.Text;
using System.Collections.Generic;
using System.Configuration;

/// <summary>
/// Global settings for the application
/// </summary>
internal static class GlobalStrings
{
    /// <summary>
    /// This can be data driven based on customer applicaiton preferenaces
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static string FormatCurrency(decimal value)
    {
        return CurrencySymbol + value.ToString("0.00");
    }

    /// <summary>
    /// This can be data driven based on customer applicaiton preferenaces
    /// </summary>
    public static string CurrencySymbol
    {
        get
        {
            return "$";
        }
    }

}