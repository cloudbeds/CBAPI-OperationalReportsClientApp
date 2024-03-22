using System;
using System.Text;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Xml;

/// <summary>
/// Manages retreiving lists of Reservations from Cloudbeds
/// </summary>
partial class CloudbedsReservationManager
{
    private readonly CloudbedsServerConnectInfoBase _serverConnectInfo;
    private readonly TaskStatusLogs _statusLog;
    const int NumberDaysFutureReservations = 90;


    /// <summary>
    /// If true - we will save the data locally after re-querying it
    /// </summary>
    //private readonly bool LocalPersistDataAfterQuery = true;

    internal class CachedData
    {
        public readonly Dictionary<string, CloudbedsReservation> Items;
        public readonly DateTime LastUpdatedUtc;

        public CachedData(Dictionary<string, CloudbedsReservation> items, DateTime updatedUtc)
        {
            this.Items = items;
            this.LastUpdatedUtc = updatedUtc;
        }
    }

    /// <summary>
    /// Update time for the cached data
    /// </summary>
    public DateTime? CacheLastUpdatedTimeUtc
    {
        get
        {
            var cache = _cachedData;
            if(cache == null)
            {
                return null;
            }

            return cache.LastUpdatedUtc;
        }
    }

    CachedData _cachedData;
    public ICollection<CloudbedsReservation> Reservations
    {
        get 
        { 
            if(_cachedData == null)
            {
                return null;
            }
            return _cachedData.Items.Values; 
        }
    }

    public int ReservationCount
    {
        get
        {
            var cachedData = _cachedData; ;
            if(cachedData == null)
            {
                return 0;
            }

            return cachedData.Items.Count;
        }
    }

    /// <summary>
    /// Look for an item with the matching ID
    /// </summary>
    /// <param name="guestId"></param>
    /// <returns></returns>
    internal CloudbedsReservation FindGuestWithId(string reservationId)
    {
        EnsureCachedData();

        CloudbedsReservation reservationOut;
        if(_cachedData.Items.TryGetValue(reservationId, out reservationOut))
        {
            return reservationOut;
        }
        return null; //No such item
    }



    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="cbServerInfo"></param>
    /// <param name="authSession"></param>
    /// <param name="statusLog"></param>
    public CloudbedsReservationManager(
        CloudbedsServerConnectInfoBase serverConnectInfo,
        TaskStatusLogs statusLog)
    {
        _serverConnectInfo = serverConnectInfo;
        _statusLog = statusLog;      
    }

    /// <summary>
    /// If we do not have cached data, set up an aync job to request it
    /// </summary>
    public void EnsureCachedData_Async()
    {
        //If we have cached data, there is nothing to do...
        if (_cachedData != null)
        {
            return;
        }

        CloudbedsSingletons.StatusLogs.AddStatus("1030-1146: Starting Async request(s) to warm up Cloudbeds query data cache");

        //Run the job async to request we fill the cache
        System.Threading.Tasks.Task.Run(() => this.EnsureCachedData());
    }

    /// <summary>
    /// Thread synchronization lock.  We use this to create a critical section
    /// that prevents is from performing a cache-query to fill the Guests cache
    /// if that query is already underway
    /// </summary>
    object _syncLockForCacheQuery = new object();


    /*
    /// <summary>
    /// Store the cached data in a local file system file
    /// </summary>
    public void PersistToLocalStorage()
    {
        var persister = new Persistence_Save(this);

        string pathXml = AppSettings.LocalFileSystemPath_CachedReservationsList;

        var xmlWriter = System.Xml.XmlWriter.Create(pathXml);

        using(xmlWriter)
        {
            persister.SaveAsXml(xmlWriter);
            xmlWriter.Close();
        }
    }

    
    /// <summary>
    /// Load from XML file
    /// </summary>
    /// <returns></returns>
    public bool DepersistFromLocalStorageIfExists()
    {
        var statusLogs = CloudbedsSingletons.StatusLogs;

        statusLogs.AddStatusHeader("Attempt load from local cache: Reservations list");

        string pathXml = AppSettings.LocalFileSystemPath_CachedReservationsList;
        //Not fatal... just note if no file exists
        if(!System.IO.File.Exists(pathXml))
        {
            statusLogs.AddStatus("0128-1013: Skipping. No local reservations cache file exists at: " + pathXml);
            return false;
        }

        var xmlDoc = new System.Xml.XmlDocument();
        xmlDoc.Load(pathXml);
        var loadResults = Persistence_Load.LoadFromXml(xmlDoc);
        if(loadResults != null)
        {
            _cachedData = loadResults;
            statusLogs.AddStatus("Successfully loaded reservation list from local cache");
        }
        else
        {
            CloudbedsSingletons.StatusLogs.AddError("0205-540: Internal error. No results returned from XML Guests cache load");
            return false;
        }


        return true;
    }
    */

    /// <summary>
    /// Queries for guests, if needed
    /// </summary>
    /// <exception cref="Exception"></exception>
    public void EnsureCachedData()
    {
        //If we have cached data, there is nothing to do...
        if(_cachedData != null)
        {
            return;
        }

        //==========================================================================
        //If another thread is already running this code, let it finish first
        //because it is ALREDY getting the data we want
        //==========================================================================
        lock (_syncLockForCacheQuery)
        {
            //======================================================================
            //Since another thread may have just completed this call - check againg
            //when we are in the lock, it ensure we dont re-query the data unncessarily
            if (_cachedData != null)
            {
                return;
            }

            //We don't have the data yet... so go ahead and re-query
            ForceRefreshOfCachedData();

        }//end: Lock
    }

    /*
    /// <summary>
    /// Try to persist the data locally
    /// </summary>
    private void TryPersistToLocalStorage()
    {
        
        try
        {
            PersistToLocalStorage();
        }
        catch(Exception ex)
        {
            CloudbedsSingletons.StatusLogs.AddError("0205-422: Error persisting Reservations data locally: " + ex.Message);
        }
        
    }
    */

    /// <summary>
    /// Get the latest data in the cache
    /// </summary>
    /// <exception cref="Exception"></exception>
    public void ForceRefreshOfCachedData()
    {
        var queryTime = DateTime.UtcNow;

        /*
        if (AppSettings.UseSimulatedGuestData)
        {
            //Create simulated data
            queriedGuests = Testing_CreateSimulatedGuestData();
            _cachedData = new CachedData(queriedGuests, queryTime);
            return;
        }
        */

        var queriedItems = helper_SynchronouEnsureCloudbedsData();

        //Store the cached results
        _cachedData = new CachedData(queriedItems, queryTime);

        /*
        //Save to to local storage...
        if (this.LocalPersistDataAfterQuery)
        {
            TryPersistToLocalStorage();
        }
        */
    }

    /// <summary>
    /// Query for the guest data.  Ensure that only one query can occur at a time
    /// </summary>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    private Dictionary<string, CloudbedsReservation> helper_SynchronouEnsureCloudbedsData()
    {
        //The set to store all of our results...
        var buildSet = new Dictionary<string, CloudbedsReservation>();

        var serverInfo = _serverConnectInfo.GetCloudbedsServerInfo();
        var authSession = _serverConnectInfo.GetCloudbedsAuthSession();
        //==========================================================================
        //Query and add the reservations that are "checked-in" now
        //==========================================================================
        var cbQueryCheckedIn = new CloudbedsRequestReservationsCheckedIn(
            serverInfo,
            authSession, 
            _statusLog);
        var querySuccess = cbQueryCheckedIn.ExecuteRequest();
        if (!querySuccess)
        {
            throw new Exception("0205-358: CloudbedsReservationManager, query failure");
        }

        helper_appendUniqueItemsToDictionary(buildSet, cbQueryCheckedIn.CommandResults_Reservations);


        helper_queryAndAppendReservations(buildSet, CloudbedsRequestReservationsCheckInWindow.ReservationStatusFilter_All);

        //Return the full set
        return buildSet;
    }

    /// <summary>
    /// Query for reservations with proximate check in times to today...
    /// </summary>
    /// <param name="buildSet"></param>
    /// <param name="reservationStatus"></param>
    /// <exception cref="Exception"></exception>
    private void helper_queryAndAppendReservations(Dictionary<string, CloudbedsReservation> buildSet, string reservationStatus)
    {
        //==========================================================================
        //Query and add the reservations that have check-in dates in the range we
        //care about (some of these may overlap with checked-in reservations - that's fine)
        //==========================================================================
        DateTime dateToday = DateTime.Today;
        var cbQueryProximateCheckIn = new
            CloudbedsRequestReservationsCheckInWindow(
            _serverConnectInfo.GetCloudbedsServerInfo(),
            _serverConnectInfo.GetCloudbedsAuthSession(),
            _statusLog,
            reservationStatus,
            dateToday + TimeSpan.FromDays(-3), //Look a few days back
            dateToday + TimeSpan.FromDays(NumberDaysFutureReservations)   //Look a few days forward
            );
        var querySuccessProximateCheckIn = cbQueryProximateCheckIn.ExecuteRequest();
        if (!querySuccessProximateCheckIn)
        {
            throw new Exception("0209-848: CloudbedsReservationManager, query failure for filter: " + reservationStatus);
        }

        helper_appendUniqueItemsToDictionary(buildSet, cbQueryProximateCheckIn.CommandResults_Reservations);
    }

    /// <summary>
    /// Add items to the dictionary.  Replace any existing items with matching reservation ids
    /// </summary>
    /// <param name="buildSet"></param>
    /// <param name="appendReservations"></param>
    private void helper_appendUniqueItemsToDictionary(Dictionary<string, CloudbedsReservation> buildSet, ICollection<CloudbedsReservation> appendReservations)
    {
        //Sanity test
        if(appendReservations == null)
        {
            IwsDiagnostics.Assert(appendReservations != null, "0205-401: Expected reservation query results");
            CloudbedsSingletons.StatusLogs.AddError("0205-401: Expected reservation query results");

            return;
        }

        //Append each of the the items
        foreach(var thisReservation in appendReservations)
        {
            var thisReservationId = thisReservation.Reservation_Id;
            //Remove the item if one exists in the set
            buildSet.Remove(thisReservationId);

            //Add it to the dictionary
            buildSet.Add(thisReservationId, thisReservation);
        }
    }



}
