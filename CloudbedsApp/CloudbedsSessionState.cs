
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Web;

/// <summary>
/// Common state we use in a Cloudbeds session
/// </summary>
internal partial class CloudbedsSessionState
{
    private readonly ICloudbedsServerInfo _currentServerInfo;
    private readonly CloudbedsAppConfig _currentServerInfo_appConfig;
    private ICloudbedsAuthSessionBase _currentAuthSession;
    private CloudbedsGuestManager _guestManager;
    private CloudbedsReservationManager_v1 _reservationManager_v1;
    private CloudbedsReservationWithRoomsManager_v1 _reservationWithRoomsManager_v1;
    private CloudbedsReservationWithRoomsManager_v2 _reservationWithRoomsManager_v2;

    //    private static CloudbedsDataRefreshScheduler _refreshScheduler;
    private CloudbedsHotelDetails _hotelDetails;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="serverInfo"></param>
    /// <param name="authSession">Usually NULL. We create the session on demand</param>
    public CloudbedsSessionState(ICloudbedsServerInfo serverInfo, ICloudbedsAuthSessionBase? authSession = null)
    {
        _currentServerInfo = serverInfo;
        _currentServerInfo_appConfig = serverInfo as CloudbedsAppConfig; //If it is an app-config class, store that

        _currentAuthSession = authSession;

    }

    public string Name
    {
        get
        {
            return _currentServerInfo.Name;
        }
    }
    public  bool IsDataAvailableForDailyOperationsReport_v1()
    {
        var reservationsManager = EnsureReservationWithRoomsManager_v1();
        return reservationsManager.IsDataCached();
    }

    public bool IsDataAvailableForDailyOperationsReport_v2()
    {
        var reservationsManager = EnsureReservationWithRoomsManager_v2();
        return reservationsManager.IsDataCached();
    }

    /// <summary>
    /// The time the cache was last updated
    /// </summary>
    public DateTime? ReservationsWithRooms_v1_CacheLastUpdatedTimeUtc
    {
        get
        {
            var resCache = _reservationWithRoomsManager_v1;
            if (resCache == null)
            {
                return null;
            }

            return resCache.CacheLastUpdatedTimeUtc;
        }
    }

    /// <summary>
    /// The time the cache was last updated
    /// </summary>
    public DateTime? ReservationsWithRooms_v2_CacheLastUpdatedTimeUtc
    {
        get
        {
            var resCache = _reservationWithRoomsManager_v2;
            if (resCache == null)
            {
                return null;
            }

            return resCache.CacheLastUpdatedTimeUtc;
        }
    }

    /// <summary>
    /// Generate the daily operations report
    /// </summary>
    /// <returns></returns>
    public CloudbedsDailyOperationsReportManager_v1 GenerateDailyOperationsReports_v1()
    {
        var reservationsManager = EnsureReservationWithRoomsManager_v1();
        reservationsManager.EnsureCachedData();

        //=============================================================
        //UNDONE: We will want this function to take a DATE-RANGE in
        //to ensure we have data for the known dates.
        //=============================================================
        DateTime dateReportStart = DateTime.Today;
        DateTime dateReportEnd = dateReportStart.AddDays(60);


        var reservationsSet = reservationsManager.Reservations;

        return new CloudbedsDailyOperationsReportManager_v1(dateReportStart, dateReportEnd, reservationsSet);
    }

    /// <summary>
    /// Generate the daily operations report
    /// </summary>
    /// <returns></returns>
    public CloudbedsDailyOperationsReportManager_v2 GenerateDailyOperationsReports_v2()
    {
        var reservationsManager = EnsureReservationWithRoomsManager_v2();
        reservationsManager.EnsureCachedData();

        //=============================================================
        //UNDONE: We will want this function to take a DATE-RANGE in
        //to ensure we have data for the known dates.
        //=============================================================
        DateTime dateReportStart = DateTime.Today;
        DateTime dateReportEnd = dateReportStart.AddDays(60);


        var reservationsSet = reservationsManager.Reservations;

        return new CloudbedsDailyOperationsReportManager_v2(dateReportStart, dateReportEnd, reservationsSet);
    }

    /// <summary>
    /// Generate the daily operations report
    /// </summary>
    /// <returns></returns>
    public CloudbedsDailyOperationsReportManager_v1_ResRoomDetails GenerateDailyOperationsReports_ResRoomDetails_v1()
    {
        var reservationsManager = EnsureReservationWithRoomsManager_v1();
        reservationsManager.EnsureCachedData();

        //=============================================================
        //UNDONE: We will want this function to take a DATE-RANGE in
        //to ensure we have data for the known dates.
        //=============================================================
        DateTime dateReportStart = DateTime.Today;
        DateTime dateReportEnd = dateReportStart.AddDays(60);


        var reservationsSet = reservationsManager.Reservations;

        return new CloudbedsDailyOperationsReportManager_v1_ResRoomDetails(dateReportStart, dateReportEnd, reservationsSet);
    }

    /// <summary>
    /// Generate the daily operations report
    /// </summary>
    /// <returns></returns>
    public CloudbedsDailyOperationsReportManager_v2_ResRoomDetails GenerateDailyOperationsReports_ResRoomDetails_v2()
    {
        var reservationsManager = EnsureReservationWithRoomsManager_v2();
        reservationsManager.EnsureCachedData();

        //=============================================================
        //UNDONE: We will want this function to take a DATE-RANGE in
        //to ensure we have data for the known dates.
        //=============================================================
        DateTime dateReportStart = DateTime.Today;
        DateTime dateReportEnd = dateReportStart.AddDays(60);


        var reservationsSet = reservationsManager.Reservations;

        return new CloudbedsDailyOperationsReportManager_v2_ResRoomDetails(dateReportStart, dateReportEnd, reservationsSet);
    }

    /*
    /// <summary>
    /// Refresh scheduler
    /// </summary>
    public  CloudbedsDataRefreshScheduler RefreshScheduler
    {
        get
        {
            if (_refreshScheduler == null)
            {
                _refreshScheduler = new CloudbedsDataRefreshScheduler();
            }
            return _refreshScheduler;
        }
    }
    */

    /// <summary>
    /// Load the hotel details as needed
    /// </summary>
    /// <returns></returns>
    public CloudbedsHotelDetails EnsureHotelDetails()
    {
        //NOTE: We COULD also persist these to local storage if we wanted
        if (_hotelDetails == null)
        {
            try
            {
                EnsureAuthSessionAndSeverInfo();

                var requestHotelDetails = new CloudbedsRequestHotelDetails(
                    _currentServerInfo, _currentAuthSession, CloudbedsSingletons.StatusLogs);

                bool requestSuccess = requestHotelDetails.ExecuteRequest();

                if (requestSuccess == false)
                {
                    throw new Exception("Failure making server request for Hotel Details");
                }

                _hotelDetails = new CloudbedsHotelDetails(
                    requestHotelDetails.PropertyId,
                    requestHotelDetails.PropertyName);

            }
            catch (Exception ex)
            {
                CloudbedsSingletons.StatusLogs.AddError("0204-115: Failure requesting hotel details, " + ex.Message);
                return null; //Error getting hotel details
            }

        }

        return _hotelDetails;
    }

    /// <summary>
    /// The authentication session to Cloudbeds.com
    /// </summary>
    public ICloudbedsAuthSessionBase CloudbedsAuthSession
    {
        get
        {
            EnsureAuthSessionAndSeverInfo();
            return _currentAuthSession;
        }
    }

    /// <summary>
    /// The server-connection configuration/url/etc
    /// </summary>
    public ICloudbedsServerInfo CloudbedsServerInfo
    {
        get
        {
            return _currentServerInfo;
        }
    }

    /// <summary>
    /// The cached list of guests...
    /// </summary>
    public CloudbedsGuestManager CloudbedsGuestManager
    {
        get
        {
            return EnsureGuestManager();
        }
    }

    /// <summary>
    /// The cached list of reservations...
    /// </summary>
    public CloudbedsReservationManager_v1 CloudbedsReservationManager_v1
    {
        get
        {
            return EnsureReservationManager_v1();
        }
    }

    /// <summary>
    /// The cached list of guests...
    /// </summary>
    public  CloudbedsHotelDetails CloudbedsHotelDetails
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
    public  TimeSpan? GetDataCacheAgeOrNull()
    {
        var oldestTimeSoFar = CloudbedsSingletons.CloudbedsGuestManager.CacheLastUpdatedTimeUtc;

        oldestTimeSoFar = helper_useOldestCacheTime(
            oldestTimeSoFar,
            CloudbedsSingletons.CloudbedsReservationManager_v1.CacheLastUpdatedTimeUtc);

        if (oldestTimeSoFar == null)
        {
            return null;
        }


        var cacheAge = DateTime.UtcNow - oldestTimeSoFar.Value;
        //Disallow a negative result
        if (cacheAge.TotalSeconds < 0)
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
        if ((time1 == null) || (time2 == null))
        {
            return null;
        }

        //Choose the oldest time
        if (time1.Value <= time2.Value)
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
    private void EnsureAuthSessionAndSeverInfo()
    {
        //===================================================================
        //If the auth-session and server info are already loaded we can 
        //just exit
        //===================================================================
        if ((_currentAuthSession != null) && (_currentServerInfo != null))
        {
            return;
        }

        TaskStatusLogs statusLogs = CloudbedsSingletons.StatusLogs;

        statusLogs.AddStatusHeader("Load Auth Tokens from storage");

        if (!AppSettings.UseSimulatedGuestData)
        {
            //=================================================================================
            //Create the LIVE session
            //=================================================================================
            helper_EnsureAuthSessionAndSeverInfo_CreateLive(statusLogs);

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
        /// Creates the LIVE session 
        /// </summary>
        private void helper_EnsureAuthSessionAndSeverInfo_CreateLive(TaskStatusLogs statusLogs)
        {
            if(_currentAuthSession != null) 
            { return;
            }

        IwsDiagnostics.Assert(_currentServerInfo_appConfig != null, "240326-401: Missing app config object");

            ICloudbedsAuthSessionBase authSession = null;
            switch (_currentServerInfo.AppAuthenticationType)
            {
                case CloudbedsAppAuthenticationType.OAuthToken:
                    //The secret needs to be an OAuth refresh/session token pair
                    authSession = helper_EnsureAuthSessionAndSeverInfo_GetOAuthSession(_currentServerInfo, statusLogs);
                    break;

                case CloudbedsAppAuthenticationType.ApiAccessKey:
                    //The secret is just the API secret in the config file
                    authSession = new CloudbedsAuthSession_ApiToken(_currentServerInfo_appConfig.CloudbedsAppClientSecret, statusLogs);
                    break;

                default:
                    throw new Exception("240322-213: Unknown authentication mode: " + _currentServerInfo_appConfig.ToString());
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
            _currentAuthSession = authSession;
        }


        /// <summary>
        /// Load an OAuth authenticaiton session
        /// </summary>
        /// <param name="appConfig"></param>
        /// <param name="statusLogs"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private  ICloudbedsAuthSessionBase helper_EnsureAuthSessionAndSeverInfo_GetOAuthSession(ICloudbedsServerInfo appConfig, TaskStatusLogs statusLogs)
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
                    _currentServerInfo_appConfig,
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
    /// Creates the simulated session (not a real csession)
    /// </summary>
    private  void helper_EnsureAuthSessionAndSeverInfo_CreateSimulated()
    {
        var authSession = new CloudbedsAuthSession_OAuth(
            new OAuth_RefreshToken("FAKE REFRESH TOKEN:xxxxxxxxx"),
            new OAuth_AccessToken("FAKE ACCESS TOKEN:yyyyyyyyy"),
            DateTime.Today.AddYears(2),
            TaskStatusLogsSingleton.Singleton);

        _currentAuthSession = authSession; 
        return;
    }

    private CloudbedsServerConnectInfoBase GenerateServerConnectionWrapper()
    {
        EnsureAuthSessionAndSeverInfo();
        return new CloudbedsServerConnectInfo(_currentServerInfo, _currentAuthSession);
    }

    /// <summary>
    /// Creates a guest manager object if necessary
    /// </summary>
    /// <returns></returns>
    public CloudbedsGuestManager EnsureGuestManager()
    {
        var guestManager = _guestManager;
        if (guestManager != null)
        {
            return guestManager;
        }

        EnsureAuthSessionAndSeverInfo();
        var taskStatus = TaskStatusLogsSingleton.Singleton;

        guestManager = new CloudbedsGuestManager(
            GenerateServerConnectionWrapper(),
            taskStatus);

        _guestManager = guestManager;
        return guestManager;
    }

    /// <summary>
    /// Clear a cache...
    /// </summary>
    public void ReservationsWithRooms_v1_ClearCache()
    {
        _reservationWithRoomsManager_v1 = null;
    }

    /// <summary>
    /// Clear a cache...
    /// </summary>
    public void ReservationsWithRooms_v2_ClearCache()
    {
        _reservationWithRoomsManager_v2 = null;
    }

    /// <summary>
    /// Creates a reservation manager object if necessary
    /// </summary>
    /// <returns></returns>
    public CloudbedsReservationWithRoomsManager_v1 EnsureReservationWithRoomsManager_v1()
    {
        var reservationManager = _reservationWithRoomsManager_v1;
        if (reservationManager != null)
        {
            return reservationManager;
        }

        EnsureAuthSessionAndSeverInfo();
        var taskStatus = TaskStatusLogsSingleton.Singleton;

        reservationManager = new CloudbedsReservationWithRoomsManager_v1(
            GenerateServerConnectionWrapper(),
            taskStatus);

        _reservationWithRoomsManager_v1 = reservationManager;
        return reservationManager;
    }


    /// <summary>
    /// Creates a reservation manager object if necessary
    /// </summary>
    /// <returns></returns>
    public CloudbedsReservationWithRoomsManager_v2 EnsureReservationWithRoomsManager_v2()
    {
        var reservationManager = _reservationWithRoomsManager_v2;
        if (reservationManager != null)
        {
            return reservationManager;
        }

        EnsureAuthSessionAndSeverInfo();
        var taskStatus = TaskStatusLogsSingleton.Singleton;

        reservationManager = new CloudbedsReservationWithRoomsManager_v2(
            GenerateServerConnectionWrapper(),
            taskStatus);

        _reservationWithRoomsManager_v2 = reservationManager;
        return reservationManager;
    }


    /// <summary>
    /// Creates a reservation manager object if necessary
    /// </summary>
    /// <returns></returns>
    public CloudbedsReservationManager_v1 EnsureReservationManager_v1()
    {
        var reservationManager = _reservationManager_v1;
        if (reservationManager != null)
        {
            return reservationManager;
        }

        EnsureAuthSessionAndSeverInfo();
        var taskStatus = TaskStatusLogsSingleton.Singleton;

        reservationManager = new CloudbedsReservationManager_v1(
            GenerateServerConnectionWrapper(),
            taskStatus);
        _reservationManager_v1 = reservationManager;
        return reservationManager;
    }

    /// <summary>
    /// Force a specific authentication session
    /// </summary>
    /// <param name="authSession"></param>
    /// <param name="persistToLocalStorage"></param>
    /// <exception cref="Exception"></exception>
    internal void SetCloudbedsAuthSession_OAuth(CloudbedsAuthSession_OAuth authSession, bool persistToLocalStorage)
    {
        _currentAuthSession = authSession;

        var statusLogs = CloudbedsSingletons.StatusLogs;

        if (persistToLocalStorage)
        {
            if (_currentServerInfo.AppAuthenticationType != CloudbedsAppAuthenticationType.OAuthToken)
            {
                throw new Exception("240326-423: Local persistance only valid for an OAuth connection");
            }

            //===============================================================================
            //Store the authenticaiton tokens
            //===============================================================================
            statusLogs.AddStatus("Persist the access tokens to storage");

            var storeManager = new CloudbedsTransientSecretStorageManager(
                authSession,
                AppSettings.LoadPreference_PathUserAccessTokens());

            storeManager.PersistSecretsToStorage();

            statusLogs.AddStatus("Access token storage successful");
        }
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

        /*
        /// <summary>
        /// Store the authentication session
        /// </summary>
        /// <param name="currentAuthSession"></param>
        internal static void SetCloudbedsAuthSession(CloudbedsAuthSession_OAuth currentAuthSession, bool persistToLocalStorage)
        {
            s_currentAuthSession = currentAuthSession;


            if (persistToLocalStorage)
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
    */
    }
