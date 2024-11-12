using System;
using System.Text;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Xml;

/// <summary>
/// Manages retreiving lists of Reservations from Cloudbeds
/// </summary>
partial class CloudbedsReservationWithRoomsManager_v2
{
    private readonly CloudbedsServerConnectInfoBase _serverConnectInfo;
    private readonly TaskStatusLogs _statusLog;
    const int NumberDaysFutureReservations = 120;

    /// <summary>
    /// If true - we will save the data locally after re-querying it
    /// </summary>
    private readonly bool LocalPersistDataAfterQuery = false;

    internal class CachedData
    {
        public readonly Dictionary<string, CloudbedsReservationWithRooms_v2> Items;
        public readonly DateTime LastUpdatedUtc;

        public CachedData(Dictionary<string, CloudbedsReservationWithRooms_v2> items, DateTime updatedUtc)
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
    public ICollection<CloudbedsReservationWithRooms_v2> Reservations
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
    /// Constructor
    /// </summary>
    /// <param name="cbServerInfo"></param>
    /// <param name="authSession"></param>
    /// <param name="statusLog"></param>
    public CloudbedsReservationWithRoomsManager_v2(
        CloudbedsServerConnectInfoBase serverConnectInfo,
        TaskStatusLogs statusLog)
    {
        _serverConnectInfo = serverConnectInfo;
        _statusLog = statusLog;      
    }

    /// <summary>
    /// If we do not have cached data, set up an async job to request it
    /// </summary>
    public void EnsureCachedData_Async()
    {
        //If we have cached data, there is nothing to do...
        if (_cachedData != null)
        {
            return;
        }

        CloudbedsSingletons.StatusLogs.AddStatus("241111-442: Starting Async request(s) to warm up Cloudbeds query data cache");

        //Run the job async to request we fill the cache
        System.Threading.Tasks.Task.Run(() => this.EnsureCachedData());
    }

    /// <summary>
    /// Thread synchronization lock.  We use this to create a critical section
    /// that prevents is from performing a cache-query to fill the  cache
    /// if that query is already underway
    /// </summary>
    object _syncLockForCacheQuery = new object();


    /// <summary>
    /// TRUE: The data has already been querued
    /// </summary>
    /// <exception cref="Exception"></exception>
    public bool IsDataCached()
    {
        return( _cachedData != null );
    }


    /// <summary>
    /// Queries for reservations, if needed
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

    /// <summary>
    /// Get the latest data in the cache
    /// </summary>
    /// <exception cref="Exception"></exception>
    public void ForceRefreshOfCachedData()
    {
        var queryTime = DateTime.UtcNow;


        var queriedItems = helper_SynchronouEnsureCloudbedsData();

        //Store the cached results
        _cachedData = new CachedData(queriedItems, queryTime);
    }

    /// <summary>
    /// Query for the guest data.  Ensure that only one query can occur at a time
    /// </summary>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    private Dictionary<string, CloudbedsReservationWithRooms_v2> helper_SynchronouEnsureCloudbedsData()
    {
        //The set to store all of our results...
        var buildSet = new Dictionary<string, CloudbedsReservationWithRooms_v2>();

        var serverInfo = _serverConnectInfo.GetCloudbedsServerInfo();
        var authSession = _serverConnectInfo.GetCloudbedsAuthSession();

        helper_queryAndAppendReservations(
            buildSet, 
            CloudbedsRequestReservationsCheckInWindow.ReservationStatusFilter_All);

        //Return the full set
        return buildSet;
    }

    /// <summary>
    /// Query for reservations with proximate check in times to today...
    /// </summary>
    /// <param name="buildSet"></param>
    /// <param name="reservationStatus"></param>
    /// <exception cref="Exception"></exception>
    private void helper_queryAndAppendReservations(
        Dictionary<string, CloudbedsReservationWithRooms_v2> buildSet, 
        string reservationStatus)
    {
        DateTime dateToday = DateTime.Today;
        DateTime queryDate_From = dateToday + TimeSpan.FromDays(-3); //Look a few days back
        DateTime queryDate_To = dateToday + TimeSpan.FromDays(NumberDaysFutureReservations); //Look a # days forward

        //=========================================================================================
        //LOG IT
        //=========================================================================================
        _statusLog.AddStatusHeader("Starting [v2, getReservations()] query for reservations: " + queryDate_From.ToString() + " to " + queryDate_To);

        //==========================================================================
        //Query and add the reservations that have check-in dates in the range we
        //care about (some of these may overlap with checked-in reservations - that's fine)
        //==========================================================================
        var cbQueryProximateCheckIn = new
            CloudbedsRequestReservationsWithRoomsCheckOutWindow_v2(
            _serverConnectInfo.GetCloudbedsServerInfo(),
            _serverConnectInfo.GetCloudbedsAuthSession(),
            _statusLog,
            queryDate_From,
            queryDate_To
            );

        var perfQueryStartTime = DateTime.Now;
        var querySuccessProximateCheckIn = cbQueryProximateCheckIn.ExecuteRequest();
        var perfQueryDuration = DateTime.Now - perfQueryStartTime;

        if (!querySuccessProximateCheckIn)
        {
            throw new Exception("241111-831: CloudbedsReservationWithRoomsManager, query failure for filter: " + reservationStatus);
        }

        var queryResults_Reservations = cbQueryProximateCheckIn.CommandResults_Reservations;

        //=========================================================================================
        //LOG IT
        //=========================================================================================
        _statusLog.AddStatus(
            "Reservation query performance: " 
            + perfQueryDuration.TotalSeconds.ToString("0.00") + " seconds"
            + ", " 
            + queryResults_Reservations.Count.ToString() + " items"
            );

        //Append the results to the passed in set
        helper_appendUniqueItemsToDictionary(
            buildSet,
            queryResults_Reservations);
    }

    /// <summary>
    /// Add items to the dictionary.  Replace any existing items with matching reservation ids
    /// </summary>
    /// <param name="buildSet"></param>
    /// <param name="appendReservations"></param>
    private void helper_appendUniqueItemsToDictionary(
        Dictionary<string, CloudbedsReservationWithRooms_v2> buildSet, 
        ICollection<CloudbedsReservationWithRooms_v2> appendReservations)
    {
        //Sanity test
        if(appendReservations == null)
        {
            IwsDiagnostics.Assert(appendReservations != null, "240320-401: Expected reservation query results");
            CloudbedsSingletons.StatusLogs.AddError("240320-401: Expected reservation query results");

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
