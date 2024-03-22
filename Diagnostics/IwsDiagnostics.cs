﻿//==================================================================================
// Copyright © 2018 Ivo Salmre.  ALL RIGHTS RESERVED.  NO WARRANTY OF ANY KIND.
// No part of this code may be used, copied or modified in any way without explicit
// written permission.
//==================================================================================
using System;
using System.Text;
using System.Collections.Generic;
using System.Diagnostics;
internal static class IwsDiagnostics
{


    /// <summary>
    /// GETS/SETS whether we show asserts as UI pop-ups
    /// </summary>
    public static bool ShowAssertsInUI
    {
        get
        {
            return s_showAssertsInUIDialog;
        }
        set
        {
            s_showAssertsInUIDialog = value;
        }
    }

    /// <summary>
    /// TRUE: Show the assert in the UI
    /// FALSE: Don't show the assert dialog
    /// </summary>
    private static bool s_showAssertsInUIDialog = true;

    /// <summary>
    /// This will serve as the debug-assert mechanism for this app
    /// </summary>
    /// <param name="condition"></param>
    /// <param name="text"></param>
    public static void Assert(bool condition, string text)
    {
        if (condition) return;


        //Write the standard output...
        System.Console.WriteLine("ASSERT FALSE:" + text);

        //System assert...
        if(s_showAssertsInUIDialog)
        {
            System.Diagnostics.Debug.Assert(false, text);
        }

        //UNDONE: (1) Write to debug file. (2) Add mechanism to return assert failures to client
        string assertFailure = text;

        try
        {
            AppLogging.LogAssertData(text);
        }
        catch(Exception e)
        {
            string eMessage = e.Message;
        }
    }


    /// <summary>
    /// Trims text to the maxLength
    /// </summary>
    /// <param name="textIn"></param>
    /// <param name="maxLength"></param>
    /// <returns></returns>
    private static string TrimTextToLength(string textIn, int maxLength)
    {
        if ((string.IsNullOrEmpty(textIn)) || (textIn.Length < maxLength))
        {
            return textIn;
        }

        return textIn.Substring(0, maxLength);
    }

    /// <summary>
    /// Writes diagnostic data
    /// </summary>
    /// <param name="text"></param>
    public static void WriteLine(string text)
    {
        var sb = new StringBuilder();
        sb.AppendLine("#iws#####################################################");
        sb.Append("iws - writeline: ");
        sb.AppendLine(DateTime.UtcNow.ToString());
        sb.AppendLine(text);

        var writeText = sb.ToString();
        Debug.WriteLine(writeText);
        //Log it whereever else we want to output it
        AppLogging.LogDebugOutput(writeText);
    }

}
