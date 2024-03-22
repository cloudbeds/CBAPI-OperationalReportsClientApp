//==================================================================================
// Copyright © 2018 Ivo Salmre.  ALL RIGHTS RESERVED.  NO WARRANTY OF ANY KIND.
// No part of this code may be used, copied or modified in any way without explicit
// written permission.
//==================================================================================
using System;
using System.Text;
using System.Collections.Generic;
//using System.Web;
using System.Configuration;
using Microsoft.Win32;

/// <summary>
/// Global settings for the application
/// </summary>
internal static class AppSettings
{
    const string Registry_AppName = "CloudbedsApiUtility";
    const string Registry_AppKey = "HKEY_CURRENT_USER\\Software\\" + Registry_AppName;
    const string Registry_Preference_DefaultAppSecretsPath = "c:\\SomePath\\Cloudbeds_AppConfig.xml";
    const string Registry_Preference_UserAccessTokensPath = "c:\\SomePath\\Cloudbeds_UserAccessTokens.xml";
    const string Registry_Preference_DefaultFileProvisioningPath = "DefaultFileProvisioningConfigPath";



    /// <summary>
    /// TRUE: We want to use simulated data instead of loading live ata
    /// FALSE: Connect to Cloudbeds and get real data
    /// </summary>
    public static bool UseSimulatedGuestData
    {
        get
        {
            return s_useSimulatedGuestData;
        }
        set
        {
            s_useSimulatedGuestData = value;
        }
    }
    private static bool s_useSimulatedGuestData;

    /// <summary>
    /// Load the user's preferred path to their secrets file
    /// </summary>
    /// <param name="text"></param>
    internal static string LoadPreference_PathAppSecretsConfig()
    {
        //If there is an override, use it...
        if(!string.IsNullOrWhiteSpace(s_override_pathAppSecretsConfig))
        {
            return s_override_pathAppSecretsConfig;
        }

        return LoadRegistryString(Registry_Preference_DefaultAppSecretsPath);
    }
    /// <summary>
    /// Override if we don't want to use the system preference
    /// </summary>
    private static string? s_override_pathAppSecretsConfig = null;

    /// <summary>
    /// Override if we don't want to use the system preference
    /// </summary>
    /// <param name="filePath"></param>
    internal static void SetOverride_PathAppSecretsConfig(string filePath)
    {
        s_override_pathAppSecretsConfig = filePath;
    }



    /// <summary>
    /// Get the path that holds our session tokens
    /// </summary>
    /// <returns></returns>
    internal static string LoadPreference_PathUserAccessTokens()
    {
        //If there is an override, use it...
        if (!string.IsNullOrWhiteSpace(s_override_pathUserAccessTokens))
        {
            return s_override_pathUserAccessTokens;
        }

        return LoadRegistryString(Registry_Preference_UserAccessTokensPath);
    }
    /// <summary>
    /// Override if we don't want to use the system preference
    /// </summary>
    private static string? s_override_pathUserAccessTokens= null;

    /// <summary>
    /// Override if we don't want to use the system preference
    /// </summary>
    /// <param name="filePath"></param>
    internal static void SetOverride_UserAccessTokens(string filePath)
    {
        s_override_pathUserAccessTokens = filePath;
    }




    /// <summary>
    /// Save the user's preferred path to their secrets file
    /// </summary>
    /// <param name="text"></param>
    internal static void SavePreference_PathAppSecretsConfig(string text)
    {
        SaveRegistryValue(Registry_Preference_DefaultAppSecretsPath, text);
    }

    internal static void SavePreference_PathUserAccessTokens(string text)
    {
        SaveRegistryValue(Registry_Preference_UserAccessTokensPath, text);
    }

    /// <summary>
    /// Load the user's preferred path to the File provisioning file
    /// </summary>
    /// <param name="text"></param>
    internal static string LoadPreference_PathFileProvisioningConfig()
    {
        return LoadRegistryString(Registry_Preference_DefaultFileProvisioningPath);
    }



    /// <summary>
    /// Saves a value to the Windows registry (save a user preference)
    /// </summary>
    /// <param name="preferenceName"></param>
    /// <param name="value"></param>
    private static void SaveRegistryValue(string preferenceName, string value)
    {
        //Store the value
        Registry.SetValue(Registry_AppKey, preferenceName, value);
    }

    /// <summary>
    /// Saves a string from the Windows registry (load a user preference)
    /// </summary>
    /// <param name="preferenceName"></param>
    /// <param name="value"></param>
    private static string LoadRegistryString(string preferenceName)
    {
        try
        {
        //Get the value as a string
        return (string) Registry.GetValue(Registry_AppKey, preferenceName, "");
        }
        catch (Exception ex)
        {
            IwsDiagnostics.Assert(false, "819-1040: Error loading registry value, " + preferenceName + ", " + ex.Message);
            return "";
        }
    }

    /// <summary>
    /// Looks up an attribute by name and returns true/false
    /// </summary>
    /// <param name="attributeName"></param>
    /// <param name="defaultValue"></param>
    /// <returns></returns>
    private static bool GetAppSettingIntegerBoolean(string attributeName, bool defaultValue)
    {
        IwsDiagnostics.Assert(!string.IsNullOrWhiteSpace(attributeName), "160615-1959, missing attribute name");

        var textValue = ConfigurationManager.AppSettings[attributeName];

        if (string.IsNullOrWhiteSpace(textValue))
        {
            return defaultValue;
        }

        textValue = textValue.Trim().ToLower();
        if (textValue == "true") return true;
        if (textValue == "false") return false;

        //Abort
        IwsDiagnostics.Assert(
            false,
            "160615-1958, attribute value not true/false, " + attributeName + "/" + textValue);
        throw new ArgumentException("160615-1958, attribute value not true/false, " + attributeName + "/" + textValue);
    }

    /// <summary>
    /// Safe way to get a setting with a default value
    /// </summary>
    /// <param name="settingName"></param>
    /// <param name="defaultValue"></param>
    private static string GetAppSettingString(string settingName, string defaultValue = "")
    {
        if (string.IsNullOrWhiteSpace(settingName))
        {
            IwsDiagnostics.Assert(false, "151210-0601, missing setting key");
            throw new ArgumentException("151210-0601, missing setting key");
        }

        
        string value = ConfigurationManager.AppSettings[settingName];
        if (string.IsNullOrEmpty(value))
        {
            return defaultValue;
        }

        return value;
    }

    /// <summary>
    /// Returns an integer value
    /// </summary>
    /// <param name="settingName"></param>
    /// <param name="defaultValue"></param>
    /// <returns></returns>
    private static int GetAppSettingInteger(string settingName, int defaultValue)
    {
        if (string.IsNullOrWhiteSpace(settingName))
        {
            IwsDiagnostics.Assert(false, "151210-0602, missing setting key");
            throw new ArgumentException("1210-0602, missing setting key");
        }

        string value = ConfigurationManager.AppSettings[settingName];
        if (string.IsNullOrEmpty(value))
        {
            return defaultValue;
        }

        return System.Convert.ToInt32(value);
    }


    /// <summary>
    /// Returns an double value
    /// </summary>
    /// <param name="settingName"></param>
    /// <param name="defaultValue"></param>
    /// <returns></returns>
    private static double GetAppSettingDouble(string settingName, int defaultValue)
    {
        if (string.IsNullOrWhiteSpace(settingName))
        {
            IwsDiagnostics.Assert(false, "925-902, missing setting key");
            throw new ArgumentException("925-902, missing setting key");
        }

        string value = ConfigurationManager.AppSettings[settingName];
        if (string.IsNullOrEmpty(value))
        {
            return defaultValue;
        }

        return System.Convert.ToDouble(value);
    }



    /// <summary>
    /// Returns the local file system path for temporary working files
    /// </summary>
    public static string LocalFileSystemPath_TempSpace
    {
        get
        {
            var fullPath =
                System.IO.Path.Combine(AppSettings.LocalFileSystemPath, @"Temp");

            //Create the directory if we need to
            FileIOHelper.CreatePathIfNeeded(fullPath);

            return fullPath;
        }
    }

    /// <summary>
    /// TRUE: We want to write assert contents into files
    /// </summary>
    public static bool DiagnosticsWriteDebugOutputToFile
    {
        get
        {
            return GetAppSettingIntegerBoolean(
                "iwsDiagnosticsWriteDebugOutputToFile",
                false); //Default to not logging user actions
        }
    }

    public static string LocalFileSystemPath_CachedGuestsList
    {
        get
        {
            return System.IO.Path.Combine(LocalFileSystemPath, "cbCache_Guests.xml");
        }
    }

    public static string LocalFileSystemPath_CachedReservationsList
    {
        get
        {
            return System.IO.Path.Combine(LocalFileSystemPath, "cbCache_Reservations.xml");
        }
    }

    /// <summary>
    /// Returns the local file system path for the application
    /// </summary>
    public static string LocalFileSystemPath
    {
        get
        {
            //return AppDomain.CurrentDomain.GetData("APPBASE").ToString();
            return System.IO.Directory.GetCurrentDirectory();
        }
    }

    /// <summary>
    /// Returns the local file system path for photo storage
    /// </summary>
    public static string LocalFileSystemPath_Diagnostics
    {
        get
        {
            var fullPathToPhotoDirectory =
                System.IO.Path.Combine(AppSettings.LocalFileSystemPath, @"App_Data\iwsPrivateContent\Diagnostics");

            return fullPathToPhotoDirectory;
        }
    }
    
    /// <summary>
    /// 
    /// </summary>
    public static bool DiagnosticsWriteAssertsToFile
    {
        get
        {
            return false;
        }
    }

}