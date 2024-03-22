using System;
using System.Text;
using System.Collections.Generic;
using System.Xml;

/// <summary>
/// Manages retreiving lists of Guests from Cloudbeds
/// </summary>
partial class CloudbedsGuestManager
{
    private readonly CloudbedsServerConnectInfoBase _serverConnectInfo;
    private readonly TaskStatusLogs _statusLog;

    /// <summary>
    /// If true - we will save the data locally after re-querying it
    /// </summary>
    //private readonly bool LocalPersistDataAfterQuery = true;

    internal class CachedData
    {
        public readonly ICollection<CloudbedsGuest> Guests;
        public readonly DateTime LastUpdatedUtc;

        public CachedData(ICollection<CloudbedsGuest> guests, DateTime updatedUtc)
        {
            this.Guests = guests;
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
    public ICollection<CloudbedsGuest> Guests
    {
        get 
        { 
            if(_cachedData == null)
            {
                return null;
            }
            return _cachedData.Guests; 
        }
    }

    public int GuestsCount
    {
        get
        {
            var cachedData = _cachedData; ;
            if(cachedData == null)
            {
                return 0;
            }

            return cachedData.Guests.Count;
        }
    }

    /// <summary>
    /// Look for a guest with the matching ID
    /// </summary>
    /// <param name="guestId"></param>
    /// <returns></returns>
    internal CloudbedsGuest FindGuestWithId(string guestId)
    {
        EnsureCachedData();
        foreach (var thisGuest in _cachedData.Guests)
        {
            if (thisGuest.Guest_Id == guestId)
            {
                return thisGuest;
            }
        }

        return null;
    }



    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="cbServerInfo"></param>
    /// <param name="authSession"></param>
    /// <param name="statusLog"></param>
    public CloudbedsGuestManager(
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
    object _syncLockForGuestCacheQuery = new object();


    /*
    /// <summary>
    /// Store the cached data in a local file system file
    /// </summary>
    public void PersistToLocalStorage()
    {
        var persister = new Persistence_Save(this);

        string pathXml = AppSettings.LocalFileSystemPath_CachedGuestsList;

        var xmlWriter = System.Xml.XmlWriter.Create(pathXml);

        using(xmlWriter)
        {
            persister.SaveAsXml(xmlWriter);
            xmlWriter.Close();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public bool DepersistFromLocalStorageIfExists()
    {
        var statusLogs = CloudbedsSingletons.StatusLogs;

        statusLogs.AddStatusHeader("Attempt load from local cache: Guests list");

        string pathXml = AppSettings.LocalFileSystemPath_CachedGuestsList;
        //Not fatal... just note if no file exists
        if(!System.IO.File.Exists(pathXml))
        {
            statusLogs.AddStatus("0128-1013: Skipping. No local guests cache file exists at: " + pathXml);
            return false;
        }

        var xmlDoc = new System.Xml.XmlDocument();
        xmlDoc.Load(pathXml);
        var loadResults = Persistence_Load.LoadFromXml(xmlDoc);
        if(loadResults != null)
        {
            _cachedData = loadResults;
            statusLogs.AddStatus("Successfully loaded guest list from local cache");
        }
        else
        {
            CloudbedsSingletons.StatusLogs.AddError("0128-1024: Internal error. No results returned from XML Guests cache load");
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
        lock (_syncLockForGuestCacheQuery)
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
            CloudbedsSingletons.StatusLogs.AddError("0127-826: Error persisting Guest data locally: " + ex.Message);
        }
    }
    */
    /// <summary>
    /// Get the latest data in the cache
    /// </summary>
    /// <exception cref="Exception"></exception>
    public void ForceRefreshOfCachedData()
    {
        ICollection<CloudbedsGuest> queriedGuests = null;

        var queryTime = DateTime.UtcNow;

        if (AppSettings.UseSimulatedGuestData)
        {
            //Create simulated data
            queriedGuests = Testing_CreateSimulatedGuestData();
            _cachedData = new CachedData(queriedGuests, queryTime);
            return;
        }

        queriedGuests = helper_SynchronouEnsureGuestData();

        //Store the cached results
        _cachedData = new CachedData(queriedGuests, queryTime);

//        //Save to to local storage...
//        if (this.LocalPersistDataAfterQuery)
//        {
//            TryPersistToLocalStorage();
//        }

    }

    /// <summary>
    /// Query for the guest data.  Ensure that only one query can occur at a time
    /// </summary>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    private ICollection<CloudbedsGuest> helper_SynchronouEnsureGuestData()
    {
        var cbQueryGuests = new CloudbedsRequestCurrentGuests(
            _serverConnectInfo.GetCloudbedsServerInfo(),
            _serverConnectInfo.GetCloudbedsAuthSession(), 
            _statusLog);
        var querySuccess = cbQueryGuests.ExecuteRequest();
        if (!querySuccess)
        {
            throw new Exception("1021-825: CloudbedsGuestManager, query failure");
        }

        var queriedGuests = cbQueryGuests.CommandResults_Guests;
        IwsDiagnostics.Assert(queriedGuests != null, "1021-826: Expected query results");
        return queriedGuests;
    }


    /// <summary>
    /// Create test data
    /// </summary>
    /// <returns></returns>
    private static ICollection<CloudbedsGuest> Testing_CreateSimulatedGuestData()
    {
        DateTime reservationDate = DateTime.Today.AddDays(-4);
        DateTime reservationDateEnd = DateTime.Today.AddDays(14);

        var testData = new List<CloudbedsGuest>();
        testData.Add(Testing_CreateSimulatedGuest("Dopy Sevendwarves", "dopy.sevendwarves@cloudbeds.com", "111.123.1111", "Rm 201", "fakeGuest-001", "fakeRoom-001", "fakeRes-001"));
        testData.Add(Testing_CreateSimulatedGuest("Bashful Sevendwarves", "bashful.sevendwarves@cloudbeds.com", "111.123.1112", "Rm 202", "fakeGuest-002", "fakeRoom-002", "fakeRes-002"));
        testData.Add(Testing_CreateSimulatedGuest("Sleepy Sevendwarves", "bashful.sevendwarves@cloudbeds.com", "111.123.1112", "Rm 203", "fakeGuest-003", "fakeRoom-003", "fakeRes-003"));
        testData.Add(Testing_CreateSimulatedGuest("Sneezy Sevendwarves", "sneezy.sevendwarves@cloudbeds.com", "111.123.1113", "Rm 204", "fakeGuest-004", "fakeRoom-004", "fakeRes-004"));
        testData.Add(Testing_CreateSimulatedGuest("Grumpy Sevendwarves", "grumpy.sevendwarves@cloudbeds.com", "111.123.1114", "Rm 205", "fakeGuest-005", "fakeRoom-005", "fakeRes-005"));
        testData.Add(Testing_CreateSimulatedGuest("Doc Sevendwarves", "doc.sevendwarves@cloudbeds.com", "111.123.1115", "Rm 206", "fakeGuest-006", "fakeRoom-006", "fakeRes-006"));
        testData.Add(Testing_CreateSimulatedGuest("Sneezy Sevendwarves", "sneezy.sevendwarves@cloudbeds.com", "111.123.1116", "Rm 212", "fakeGuest-007", "fakeRoom-007", "fakeRes-007"));
        testData.Add(Testing_CreateSimulatedGuest("Happy Sevendwarves", "happy.sevendwarves@cloudbeds.com", "111.123.1117", "Rm 214", "fakeGuest-008", "fakeRoom-008", "fakeRes-008"));
        testData.Add(Testing_CreateSimulatedGuest("Snow White", "snow.white@cloudbeds.com", "111.123.1130", "Rm 107", "fakeGuest-009", "fakeRoom-009", "fakeRes-009"));
        testData.Add(Testing_CreateSimulatedGuest("Evil Queen", "evil.queen@cloudbeds.com", "111.123.1140", "Rm 321", "fakeGuest-010", "fakeRoom-010", "fakeRes-010"));

        return testData;
    }


    /// <summary>
    /// Create a similated guest
    /// </summary>
    /// <param name="name"></param>
    /// <param name="email"></param>
    /// <param name="phone"></param>
    /// <param name="roomName"></param>
    /// <returns></returns>
    private static CloudbedsGuest Testing_CreateSimulatedGuest(string name, string email, string phone, string roomName, string guestId, string roomId, string reservationId)
    {
        var random = new Random();

        DateTime reservationDate = DateTime.Today.AddDays(-2 - random.Next(8));
        DateTime reservationDateEnd = DateTime.Today.AddDays(14 + random.Next(32));

        return new CloudbedsGuest(
                guestId,
                name,
                email,
                phone,
                reservationId,
                reservationDate.ToShortDateString(),
                reservationDateEnd.ToShortDateString(),
                roomId,
                roomName);

    }

}
