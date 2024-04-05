
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Web;
using System.Windows.Documents;

/// <summary>
/// Common singletons we want to use across the application
/// </summary>
static internal partial class CloudbedsSingletons
{
    private static CloudbedsSessionState s_selectedCBSessionState;
    private static List<CloudbedsSessionState> s_cloudbedsAppConfig_Set = null;
    
    private static CloudbedsDataRefreshScheduler s_refreshScheduler;
    private static System.Random s_randomizer;


    public static bool IsDataAvailableForDailyOperationsReport()
    {
        EnsureServerInfo();

        return s_selectedCBSessionState.IsDataAvailableForDailyOperationsReport();
        //var reservationsManager = EnsureReservationWithRoomsManager();
        //return reservationsManager.IsDataCached();
    }

    /// <summary>
    /// The currenly selected sesssion
    /// </summary>
    public static CloudbedsSessionState SelectedSession
    {
        get
        {
            return s_selectedCBSessionState;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public static ICollection<CloudbedsSessionState> ListOfCBSessions
    {
        get
        {
            EnsureServerInfo();
            return s_cloudbedsAppConfig_Set.AsReadOnly();
        }
    }

    /// <summary>
    /// Genrate the daily operations report
    /// </summary>
    /// <returns></returns>
    public static CloudbedsDailyOperationsReportManager GenerateDailyOperationsReports()
    {
        EnsureServerInfo();

        return s_selectedCBSessionState.GenerateDailyOperationsReports();
    }


    /// <summary>
    /// Genrate the daily operations report
    /// </summary>
    /// <returns></returns>
    public static CloudbedsDailyOperationsReportManager_ResRoomDetails GenerateDailyOperationsReports_ResRoomDetails()
    {
        EnsureServerInfo();

        return s_selectedCBSessionState.GenerateDailyOperationsReports_ResRoomDetails();
    }
    /// <summary>
    /// Refresh scheduler
    /// </summary>
    public static CloudbedsDataRefreshScheduler RefreshScheduler
    {
        get
        {
            if (s_refreshScheduler == null)
            {
                s_refreshScheduler = new CloudbedsDataRefreshScheduler();
            }
            return s_refreshScheduler;
        }
    }

    /// <summary>
    /// Load the hotel details as needed
    /// </summary>
    /// <returns></returns>
    public static CloudbedsHotelDetails EnsureHotelDetails()
    {
        EnsureServerInfo();

        return s_selectedCBSessionState.EnsureHotelDetails();
    }


    /// <summary>
    /// Random # generator
    /// </summary>
    public static System.Random Randomizer
    {
        get
        {
            if(s_randomizer == null)
            {
                s_randomizer = new Random();
            }

            return s_randomizer;
        }
    }
    /// <summary>
    /// Common status logging for the application
    /// </summary>
    public static TaskStatusLogs StatusLogs
    {
        get { return TaskStatusLogsSingleton.Singleton; }
    }

    /// <summary>
    /// The authentication session to Cloudbeds.com
    /// </summary>
    public static ICloudbedsAuthSessionBase CloudbedsAuthSession
    {
        get
        {
            EnsureServerInfo();
            return s_selectedCBSessionState.CloudbedsAuthSession;
        }
    }

    /// <summary>
    /// The server-connection configuration/url/etc
    /// </summary>
    public static ICloudbedsServerInfo CloudbedsServerInfo
    {
        get
        {
            EnsureServerInfo();
            return s_selectedCBSessionState.CloudbedsServerInfo;
        }
    }


    /// <summary>
    /// The cached list of guests...
    /// </summary>
    public static CloudbedsGuestManager CloudbedsGuestManager
    {
        get
        {
            EnsureServerInfo();
            return s_selectedCBSessionState.CloudbedsGuestManager;
        }
    }

    /// <summary>
    /// The cached list of reservations...
    /// </summary>
    public static CloudbedsReservationManager CloudbedsReservationManager
    {
        get
        {
            EnsureServerInfo();
            return s_selectedCBSessionState.CloudbedsReservationManager;
        }
    }

    /// <summary>
    /// The cached list of guests...
    /// </summary>
    public static CloudbedsHotelDetails CloudbedsHotelDetails
    {
        get
        {
            EnsureServerInfo();
            return s_selectedCBSessionState.CloudbedsHotelDetails;
        }
    }


    /// <summary>
    /// Our data cache is as old as the olders sub-cache
    /// </summary>
    /// <returns></returns>
    public static TimeSpan? GetDataCacheAgeOrNull()
    {
        EnsureServerInfo();
        return s_selectedCBSessionState.GetDataCacheAgeOrNull();
    }

    /// <summary>
    /// Loads a token from persisted storage
    /// </summary>
    /// <param name="statusLogs"></param>
    /// <exception cref="NotImplementedException"></exception>
    private static void EnsureServerInfo()
    {
        if (s_cloudbedsAppConfig_Set != null)
        {
            return;
        }

        if (!AppSettings.UseSimulatedGuestData)
        {
            //=================================================================================
            //Create the LIVE session
            //=================================================================================
            helper_EnsureAuthSessionAndSeverInfo_CreateLive(StatusLogs);

        }
        else
        {
            //=================================================================================
            //If we are using simlated data -- create it here...
            //=================================================================================
            helper_EnsureAuthSessionAndSeverInfo_CreateSimulated();
            return;
        }

    }

    /// <summary>
    /// Creates the simulated session (not a real csession)
    /// </summary>
    private static void helper_EnsureAuthSessionAndSeverInfo_CreateSimulated()
    {
        var serverInfo = CloudbedsAppConfig.TESTING_CreateSimulatedAppConfig();

        var authSession = new CloudbedsAuthSession_OAuth(
            new OAuth_RefreshToken("FAKE REFRESH TOKEN:xxxxxxxxx"),
            new OAuth_AccessToken("FAKE ACCESS TOKEN:yyyyyyyyy"),
            DateTime.Today.AddYears(2),
            TaskStatusLogsSingleton.Singleton);


        var sessionState = new CloudbedsSessionState(serverInfo, authSession);

        var configSet = new List<CloudbedsSessionState>();
        configSet.Add(sessionState);
        s_cloudbedsAppConfig_Set = configSet;

        s_selectedCBSessionState = sessionState;
        return;
    }

    /// <summary>
    /// Creates the LIVE session 
    /// </summary>
    private static void helper_EnsureAuthSessionAndSeverInfo_CreateLive(TaskStatusLogs statusLogs)
    {
        //Null out the set
        s_cloudbedsAppConfig_Set = null;
        s_selectedCBSessionState = null;

        //=================================================================================
        //Load our secrets and create an authentication session...
        //=================================================================================
        var filePathToAppSecrets = AppSettings.LoadPreference_PathAppSecretsConfig();
        if (!System.IO.File.Exists(filePathToAppSecrets))
        {
            statusLogs.AddError("220825-814: App secrets file does not exist: " + filePathToAppSecrets);
            throw new Exception("220825-814805: App secrets file does not exist: " + filePathToAppSecrets);
        }
        //----------------------------------------------------------
        //Load the application-global configuration from storage
        //(we need to to refresh the token)
        //----------------------------------------------------------
        var serverConnectInfoSet = CloudbedsAppConfig.FromFile(filePathToAppSecrets);

        var configSet = new List<CloudbedsSessionState>();

        foreach (var thisConnectInfo in serverConnectInfoSet)
        {
            configSet.Add(new CloudbedsSessionState(thisConnectInfo));

        }
        s_cloudbedsAppConfig_Set = configSet;

        //Select the first one
        s_selectedCBSessionState = configSet[0];
    }



    /// <summary>
    /// Creates a guest manager object if necessary
    /// </summary>
    /// <returns></returns>
    private static CloudbedsGuestManager EnsureGuestManager()
    {
        EnsureServerInfo();
        return s_selectedCBSessionState.EnsureGuestManager();
    }


    /// <summary>
    /// Creates a reservation manager object if necessary
    /// </summary>
    /// <returns></returns>
    private static CloudbedsReservationWithRoomsManager EnsureReservationWithRoomsManager()
    {
        EnsureServerInfo();
        return s_selectedCBSessionState.EnsureReservationWithRoomsManager();
    }


    /// <summary>
    /// Creates a reservation manager object if necessary
    /// </summary>
    /// <returns></returns>
    private static CloudbedsReservationManager EnsureReservationManager()
    {
        EnsureServerInfo();
        return s_selectedCBSessionState.EnsureReservationManager();

    }

    /// <summary>
    /// Throws out the existing server configuration data and reloads it
    /// </summary>
    internal static void ResetAndReloadConfigurationFiles()
    {
        //Throw out the old configuration
        s_cloudbedsAppConfig_Set = null;

        //Load a new configuration
        EnsureValidConfigurationFilesExist();
    }

    /// <summary>
    /// Throws error if something is wrong with the configuration
    /// </summary>
    internal static void EnsureValidConfigurationFilesExist()
    {
        try
        {
            EnsureServerInfo();
        }
        catch (Exception ex)
        {
            StatusLogs.AddError("240321-1055: Missing or incorrect configuration files, " + ex.Message);
            throw;
        }

    }

    internal static void SetCloudbedsAuthSession(CloudbedsAuthSession_OAuth authSession, bool persistToStorage)
    {
        EnsureServerInfo();
        s_selectedCBSessionState.SetCloudbedsAuthSession_OAuth(authSession, persistToStorage);
    }
    /*
/// <summary>
/// If necessary, starts common queries for data our application uses.
/// 
/// These queries will run asynchronously, and will allow user facing UI
/// to come up. 
/// 
/// NOTE: If the UI needs the data in question, it will cause a
/// synchronous call that will then wait for the query to complete
/// (This is done by putting thread-lock/critical-sections in the specific
///  query code)
/// </summary>
public static void WarmUpCloudbedsDataCachesIfNeeded_Async()
{
   //See if the Guest Manager needs to query for its data async
   EnsureGuestManager().EnsureCachedData_Async();
}
*/

/// <summary>
/// Force the refresh of our data caches
/// </summary>
internal static void ForceRefreshOfCloudbedsDataCache()
{
   try
   {
       CloudbedsSingletons.StatusLogs.AddStatusHeader("Starting refresh of Cloudbeds local cache data");

       ForceRefreshOfCloudbedsDataCache_inner();
   }
   catch(Exception ex)
   {
       CloudbedsSingletons.StatusLogs.AddError("0131-604: Error refreshing cache: " + ex.Message);
       throw;
   }

}

private static void ForceRefreshOfCloudbedsDataCache_inner()
{
   var statusLogs = StatusLogs;

   //===========================================================
   //If any of the cache refresh areas fails, continue and try
   //to refresh the others (some fresh data is better than none)
   //===========================================================

   //----------------------------------------------------------
   //Refresh GUESTS data cache
   //----------------------------------------------------------
   try
   {
       EnsureGuestManager().ForceRefreshOfCachedData();
   }
   catch(Exception exGuestManager)
   {
       statusLogs.AddError("0205-534: Error refreshesing Guests cache, " + exGuestManager.Message);
   }

   //----------------------------------------------------------
   //Refresh RESERVATIONS data cache
   //----------------------------------------------------------
   try
   {
       EnsureReservationManager().ForceRefreshOfCachedData();
   }
   catch (Exception exReservationsManager)
   {
       statusLogs.AddError("0205-535: Error refreshesing Guests cache, " + exReservationsManager.Message);
   }
}

    /// <summary>
    /// Find a session by name, and load it
    /// </summary>
    /// <param name="selectedItem"></param>
    /// <returns></returns>
    internal static CloudbedsSessionState SelectCBSessionByName(string selectedItem)
    {
        EnsureServerInfo();

        foreach(var thisItem in s_cloudbedsAppConfig_Set)
        {
            if(thisItem.Name == selectedItem)
            {
                s_selectedCBSessionState = thisItem;
                return s_selectedCBSessionState;
            }
        }

        return null;
    }

    /// <summary>
    /// Broadcast a data refresh signal
    /// </summary>
    internal static void SendDataRefreshSignal()
    {
        var refresher = s_refreshScheduler;
        if(refresher == null)
        {
            return;
        }

        refresher.SendDataRefreshSignal();
    }
}
