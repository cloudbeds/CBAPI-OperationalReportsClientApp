
using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Web;

/// <summary>
/// Common singletons we want to use across the application
/// </summary>
static internal partial class CloudbedsSingletons
{
    private static ICloudbedsAuthSessionBase s_currentAuthSession;
    private static ICloudbedsServerInfo s_currentServerInfo;
    private static CloudbedsGuestManager s_guestManager;
    private static CloudbedsReservationManager s_reservationManager;
    private static CloudbedsReservationWithRoomsManager s_reservationWithRoomsManager;
    private static System.Random s_randomizer;
    private static CloudbedsDataRefreshScheduler s_refreshScheduler;
    private static CloudbedsHotelDetails s_hotelDetails;


    public static bool IsDataAvailableForDailyOperationsReport()
    {
        var reservationsManager = EnsureReservationWithRoomsManager();
        return reservationsManager.IsDataCached();

    }

    /// <summary>
    /// Genrate the daily operations report
    /// </summary>
    /// <returns></returns>
    public static CloudbedsDailyOperationsReportManager GenerateDailyOperationsReports()
    {
        var reservationsManager = EnsureReservationWithRoomsManager();
        reservationsManager.EnsureCachedData();

        //=============================================================
        //UNDONE: We will want this function to take a DATE-RANGE in
        //to ensure we have data for the known dates.
        //=============================================================
        DateTime dateReportStart = DateTime.Today;
        DateTime dateReportEnd = dateReportStart.AddDays(60);


        var reservationsSet = reservationsManager.Reservations;

        return new CloudbedsDailyOperationsReportManager(dateReportStart, dateReportEnd, reservationsSet);
    }

    /// <summary>
    /// Refresh scheduler
    /// </summary>
    public static CloudbedsDataRefreshScheduler RefreshScheduler
    {
        get
        {
            if(s_refreshScheduler == null)
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
        //NOTE: We COULD also persist these to local storage if we wanted
        if(s_hotelDetails == null)
        {
            try
            {
                EnsureAuthSessionAndSeverInfo();

                var requestHotelDetails = new CloudbedsRequestHotelDetails(
                    s_currentServerInfo, s_currentAuthSession, CloudbedsSingletons.StatusLogs);

                bool requestSuccess = requestHotelDetails.ExecuteRequest();

                if(requestSuccess == false)
                {
                    throw new Exception("Failure making server request for Hotel Details");
                }

                s_hotelDetails = new CloudbedsHotelDetails(
                    requestHotelDetails.PropertyId, 
                    requestHotelDetails.PropertyName);

            }
            catch(Exception ex)
            {
                CloudbedsSingletons.StatusLogs.AddError("0204-115: Failure requesting hotel details, " + ex.Message);
                return null; //Error getting hotel details
            }

        }

        return s_hotelDetails;
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
    /*
    /// <summary>
    /// Typically called on application start - load up any data
    /// we have stored locally in the file system cache.  That way
    /// if we cannot connect to Cloudbeds, we will have a local copy
    /// </summary>
    public static void TryLoadPropertyDataFromLocalCache()
    {
        //---------------------------------------------------------------
        //Try to load GUESTS data
        //---------------------------------------------------------------
        try
        {
            EnsureGuestManager().DepersistFromLocalStorageIfExists();
        }
        catch(Exception exLoadGuests)
        {
            StatusLogs.AddError("0128-256: Error loading local guests file, " + exLoadGuests.Message);
        }

        //---------------------------------------------------------------
        //Try to load RESERVATIONS data
        //---------------------------------------------------------------
        try
        {
            EnsureReservationManager().DepersistFromLocalStorageIfExists();
        }
        catch (Exception exLoadReservations)
        {
            StatusLogs.AddError("0205-542: Error loading local reservations file, " + exLoadReservations.Message);
        }

    }
*/
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
            EnsureAuthSessionAndSeverInfo();
            return s_currentAuthSession;
        }
    }

    /// <summary>
    /// The server-connection configuration/url/etc
    /// </summary>
    public static ICloudbedsServerInfo CloudbedsServerInfo
    {
        get
        {
            EnsureAuthSessionAndSeverInfo();
            return s_currentServerInfo;
        }
    }


    /// <summary>
    /// The cached list of guests...
    /// </summary>
    public static CloudbedsGuestManager CloudbedsGuestManager
    {
        get
        {
            return EnsureGuestManager();
        }
    }

    /// <summary>
    /// The cached list of reservations...
    /// </summary>
    public static CloudbedsReservationManager CloudbedsReservationManager
    {
        get
        {
            return EnsureReservationManager();
        }
    }

    /// <summary>
    /// The cached list of guests...
    /// </summary>
    public static CloudbedsHotelDetails CloudbedsHotelDetails
    {
        get
        {
            return EnsureHotelDetails();
        }
    }


    /// <summary>
    /// Our data cache is as old as the olders sub-cache
    /// </summary>
    /// <returns></returns>
    public static TimeSpan? GetDataCacheAgeOrNull()
    {
        var oldestTimeSoFar = CloudbedsSingletons.CloudbedsGuestManager.CacheLastUpdatedTimeUtc;

        oldestTimeSoFar = helper_useOldestCacheTime(
            oldestTimeSoFar,
            CloudbedsSingletons.CloudbedsReservationManager.CacheLastUpdatedTimeUtc);

        if (oldestTimeSoFar == null)
        {
            return null;
        }


        var cacheAge = DateTime.UtcNow - oldestTimeSoFar.Value;
        //Disallow a negative result
        if(cacheAge.TotalSeconds < 0)
        {
            return TimeSpan.Zero;
        }

        return cacheAge;
    }

    /// <summary>
    /// Compare 2 cache refresh times and return the older of the 2 (NULL is treated as the oldest)
    /// </summary>
    /// <param name="time1"></param>
    /// <param name="time2"></param>
    /// <returns></returns>
    private static DateTime? helper_useOldestCacheTime(DateTime? time1, DateTime? time2)
    {
        //If either time is null, then null wins (it indicates there is NO data in the cache for that item
        if((time1 == null) || (time2 == null))
        {
            return null;
        }

        //Choose the oldest time
        if(time1.Value <= time2.Value)
        {
            return time1.Value;
        }

        return time2.Value;
    }

    /// <summary>
    /// Loads a token from persisted storage
    /// </summary>
    /// <param name="statusLogs"></param>
    /// <exception cref="NotImplementedException"></exception>
    private static void EnsureAuthSessionAndSeverInfo()
    {
        //===================================================================
        //If the auth-session and server info are already loaded we can 
        //just exit
        //===================================================================
        if ((s_currentAuthSession != null) && (s_currentServerInfo != null))
        {
            return;
        }

        TaskStatusLogs statusLogs = CloudbedsSingletons.StatusLogs;

        statusLogs.AddStatusHeader("Load Auth Tokens from storage");

        //=================================================================================
        //If we are using simlated data -- create it here...
        //=================================================================================
        if (AppSettings.UseSimulatedGuestData)
        {
            helper_EnsureAuthSessionAndSeverInfo_CreateSimulated();
            return;
        }

        //=================================================================================
        //Create the LIVE session
        //=================================================================================
        helper_EnsureAuthSessionAndSeverInfo_CreateLive(statusLogs);
    }

    /// <summary>
    /// Creates the simulated session (not a real csession)
    /// </summary>
    private static void helper_EnsureAuthSessionAndSeverInfo_CreateSimulated()
    {
        s_currentAuthSession = new CloudbedsAuthSession_OAuth(
            new OAuth_RefreshToken("FAKE REFRESH TOKEN:xxxxxxxxx"),
            new OAuth_AccessToken("FAKE ACCESS TOKEN:yyyyyyyyy"),
            DateTime.Today.AddYears(2),
            TaskStatusLogsSingleton.Singleton);

        s_currentServerInfo = CloudbedsAppConfig.TESTING_CreateSimulatedAppConfig();
        return;
    }

    /// <summary>
    /// Creates the LIVE session 
    /// </summary>
    private static void helper_EnsureAuthSessionAndSeverInfo_CreateLive(TaskStatusLogs statusLogs)
    {
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
        var appConfigAndSecrets = CloudbedsAppConfig.FromFile(filePathToAppSecrets);
        s_currentServerInfo = appConfigAndSecrets;

        ICloudbedsAuthSessionBase authSession = null;
        switch (appConfigAndSecrets.AuthenticationType)
        {
            case CloudbedsAppAuthenticationType.OAuthToken:
                //The secret needs to be an OAuth refresh/session token pair
                authSession = helper_EnsureAuthSessionAndSeverInfo_GetOAuthSession(appConfigAndSecrets, statusLogs);
                break;

            case CloudbedsAppAuthenticationType.ApiAccessKey:
                //The secret is just the API secret in the config file
                authSession = new CloudbedsAuthSession_ApiToken(appConfigAndSecrets.CloudbedsAppClientSecret, statusLogs);
                break;

            default:
                throw new Exception("240322-213: Unknown authentication mode: " + appConfigAndSecrets.ToString());
        }

        //Sanity test
        if (authSession == null)
        {
            statusLogs.AddError("240322-217: No auth session returned");
        }
        else
        {
            statusLogs.AddStatus("Successfully loaded auth token from storage");
        }

        //--------------------------------------------------------------------
        //Store this at the class' level, so that it can be used by other calls
        //--------------------------------------------------------------------
        s_currentAuthSession = authSession;
    }


    /// <summary>
    /// Load an OAuth authenticaiton session
    /// </summary>
    /// <param name="appConfig"></param>
    /// <param name="statusLogs"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    private static ICloudbedsAuthSessionBase helper_EnsureAuthSessionAndSeverInfo_GetOAuthSession(CloudbedsAppConfig appConfig, TaskStatusLogs statusLogs)
    {
        //================================================================================
        //Load the authentication secrets from storage
        //================================================================================
        var filePathToPersistedToken = AppSettings.LoadPreference_PathUserAccessTokens();
        if (!System.IO.File.Exists(filePathToPersistedToken))
        {
            statusLogs.AddError("220825-805: Auth token file does not exist: " + filePathToPersistedToken);
            throw new Exception("220825-805: Auth token file does not exist: " + filePathToPersistedToken);
        }
        CloudbedsTransientSecretStorageManager authSecretsStorageManager =
            CloudbedsTransientSecretStorageManager.LoadAuthTokensFromFile(
                filePathToPersistedToken,
                appConfig,
                true,
                statusLogs);

        var authSession = authSecretsStorageManager.AuthSession;
        //Sanity test
        if (authSession == null)
        {
            statusLogs.AddError("220725-229: No auth session returned");
        }

        return authSession;
    }



    /// <summary>
    /// Creates a guest manager object if necessary
    /// </summary>
    /// <returns></returns>
    private static CloudbedsGuestManager EnsureGuestManager()
    {
        var guestManager = s_guestManager;
        if (guestManager != null)
        {
            return guestManager;
        }

        var taskStatus = TaskStatusLogsSingleton.Singleton;

        /* 2023-03-23: We can defer validating the network connection until we actually need it to make a call
        //Make sure we are logged in
        EnsureAuthSessionAndSeverInfo();

        var authSession = CloudbedsSingletons.CloudbedsAuthSession;
        var serverInfo = CloudbedsSingletons.CloudbedsServerInfo;
        if ((authSession == null) || (serverInfo == null))
        {
            taskStatus.AddError("1021-835: No auth/server session");
            throw new Exception("1021-835: No auth/server session");
        }
        */

        guestManager = new CloudbedsGuestManager(
            new CloudbedsServerConnectInfoUsingSingleton(), 
            taskStatus);

        s_guestManager = guestManager;
        return guestManager;
    }


    /// <summary>
    /// Creates a reservation manager object if necessary
    /// </summary>
    /// <returns></returns>
    private static CloudbedsReservationWithRoomsManager EnsureReservationWithRoomsManager()
    {
        var reservationManager = s_reservationWithRoomsManager;
        if (reservationManager != null)
        {
            return reservationManager;
        }

        var taskStatus = TaskStatusLogsSingleton.Singleton;


        reservationManager = new CloudbedsReservationWithRoomsManager(
            new CloudbedsServerConnectInfoUsingSingleton(),
            taskStatus);

        s_reservationWithRoomsManager = reservationManager;
        return reservationManager;
    }


    /// <summary>
    /// Creates a reservation manager object if necessary
    /// </summary>
    /// <returns></returns>
    private static CloudbedsReservationManager EnsureReservationManager()
    {
        var reservationManager = s_reservationManager;
        if (reservationManager != null)
        {
            return reservationManager;
        }

        var taskStatus = TaskStatusLogsSingleton.Singleton;

        /* 2023-02-23: We can defer checking for the Auth Session until we actually need to make a call
        //Make sure we are logged in
        EnsureAuthSessionAndSeverInfo();

        var authSession = CloudbedsSingletons.CloudbedsAuthSession;
        var serverInfo = CloudbedsSingletons.CloudbedsServerInfo;
        if ((authSession == null) || (serverInfo == null))
        {
            taskStatus.AddError("0205-514: No auth/server session");
            throw new Exception("0205-514: No auth/server session");
        }
        */

        reservationManager = new CloudbedsReservationManager(
            new CloudbedsServerConnectInfoUsingSingleton(),
            taskStatus);
        s_reservationManager = reservationManager;
        return reservationManager;
    }

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
    /// Store the authentication session
    /// </summary>
    /// <param name="currentAuthSession"></param>
    internal static void SetCloudbedsAuthSession(CloudbedsAuthSession_OAuth currentAuthSession, bool persistToLocalStorage)
    {
        s_currentAuthSession = currentAuthSession;


        if(persistToLocalStorage)
        {

            //===============================================================================
            //Store the authenticaiton tokens
            //===============================================================================
            StatusLogs.AddStatus("Persist the access tokens to storage");

            var storeManager = new CloudbedsTransientSecretStorageManager(
                currentAuthSession,
                AppSettings.LoadPreference_PathUserAccessTokens());

            storeManager.PersistSecretsToStorage();

            StatusLogs.AddStatus("Access token storage successful");

        }

    }

    /// <summary>
    /// Throws error if something is wrong with the configuration
    /// </summary>
    internal static void EnsureValidConfigurationFilesExist()
    {
        try
        {
            EnsureAuthSessionAndSeverInfo();
        }
        catch(Exception ex)
        {
            StatusLogs.AddError("240321-1055: Missing or incorrect configuration files, " + ex.Message);
            throw;
        }

    }
}
