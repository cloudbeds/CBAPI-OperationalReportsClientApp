
using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Web;

/// <summary>
/// Manages and scheduled automated data refreshes
/// </summary>
internal partial class CloudbedsDataRefreshScheduler
{
    //DateTime _nextRefreshAfterUtc = DateTime.MinValue;
    System.Threading.Timer _activeTimer = null;

    const int HeartbeatTimerInterval_Seconds = 25;//  60 * 4; //4 hours;

    /// <summary>
    /// Send notifications here
    /// </summary>
    IDataRefreshNotify _refreshOccuredNotificationTarget = null;

    /// <summary>
    /// Only run scheduled refreshes if enabled
    /// </summary>
    private bool _refreshEnabled = false; //Start off DISABLED until we are turned on

    readonly TimeSpan MaxCacheAgeBeforeRefresh = TimeSpan.FromMinutes(60 * 12); //12 hours

    //If set, make sure we wait at least this long until requerying for data
    DateTime? _cooloffTimeUntilNextScheduledRefreshUtc = null;

    public TimeSpan IntervalForCloudbedsCacheRefresh
    {
        get
        {
            return MaxCacheAgeBeforeRefresh;
        }
    }


    /// <summary>
    /// Enable or disable the timer
    /// </summary>
    public bool RefreshEnabled
    {
        get { return _refreshEnabled; }
        set
        {
            //If there is nothing to do
            if(_refreshEnabled == value)
            {
                return;
            }

            //Set of the refresh checking timer
            if(value == true)
            {
                _refreshEnabled = true;
                ResetTimer();
            }
            else
            {
                //Turn of the refresh checking timer
                _refreshEnabled = false;
                var existingTimer = _activeTimer;
                if(existingTimer != null)
                {
                    existingTimer.Dispose();
                    _activeTimer = null;
                }
            }

        }
    }

    /// <summary>
    /// Constructor
    /// </summary>
    public CloudbedsDataRefreshScheduler()
    {
        //_nextRefreshAfterUtc = DateTime.UtcNow;
        //ResetTimer();
    }

    /// <summary>
    /// Tell us who to notify when a refersh happnes
    /// </summary>
    /// <param name="target"></param>
    public void SetRefreshNotificationTarget(IDataRefreshNotify target)
    {
        _refreshOccuredNotificationTarget = target;
    }

    object _lockTimerSetup = new object();
    /// <summary>
    /// Reset our internal timer
    /// </summary>
    private void ResetTimer()
    {
        //Critical section (cannot be reentrant)
        lock(_lockTimerSetup)
        {
            //Throw out any old time
            var activeTimer = _activeTimer;
            if (activeTimer != null)
            {
                _activeTimer = null;
                activeTimer.Dispose();
            }
        }

        //====================================================
        //If refreshes are NOT enabled, then stop here
        //(do NOT create a new timer)
        //====================================================
        if (_refreshEnabled != true)
        {
            return;
        }

        //Create the new timer...
        _activeTimer = 
            new System.Threading.Timer(
                TimerCallbackTarget, 
                null, 
                HeartbeatTimerInterval_Seconds * 1000, //Trigger in N seconds...
                Timeout.Infinite //Run once
                );

    }

    /// <summary>
    /// Called when the timer is triggered.
    /// </summary>
    /// <param name="state"></param>
    private void TimerCallbackTarget(object? state)
    {
        //If we have a notification target, then call it...
        TimerCallbackTarget_TriggerHeartbeatNotification();

        //========================================================================
        //No ensure we don't over-query the Cloudbeds servers, we use
        //a cool-off time that can be set to indicate quiet periods
        //========================================================================
        var coolOffTimeUtc = _cooloffTimeUntilNextScheduledRefreshUtc;
        if((coolOffTimeUtc != null) && (DateTime.UtcNow < coolOffTimeUtc))
        {
            return;
        }

        //If we have a notification target, then call it...
        bool dataRefreshNeeded = TimerCallbackTarget_IsDataRefreshIfNeeded();

        if(dataRefreshNeeded)
        {
            //Trap and record any errors... we don't want to crash the app if something fails
            try
            {
                RefreshOfCloudbedsDataCacheAndTriggerAppNotifications();
                SetCoolOffPeriodMinutes(2); //Set at least small cool off period value for safety
            }
            catch (Exception ex)
            {
                CloudbedsSingletons.StatusLogs.AddError("0204-821: Unexpected error in background data refresh/notifications, " + ex.Message);
                SetCoolOffPeriodMinutes();
            }
        }

        //Set us up to run again...
        ResetTimer();
    }

    /// <summary>
    /// Set a cool-off period
    /// </summary>
    private void SetCoolOffPeriodMinutes(int coolOffMinutes = 5)
    {
        if(coolOffMinutes <0)
        {
            _cooloffTimeUntilNextScheduledRefreshUtc = null;
            return;
        }

        _cooloffTimeUntilNextScheduledRefreshUtc = DateTime.UtcNow + TimeSpan.FromMinutes(coolOffMinutes);
    }


    /// <summary>
    /// Force the data refresh and trigger UI updates
    /// </summary>
    public void RefreshOfCloudbedsDataCacheAndTriggerAppNotifications()
    {
        CloudbedsSingletons.ForceRefreshOfCloudbedsDataCache();

        //Notify the app that a refresh occured (usually so UI can be updated)
        SendDataRefreshSignal();
    }

    /// <summary>
    /// True if a data cache refersh is requred
    /// </summary>
    /// <returns></returns>
    private bool TimerCallbackTarget_IsDataRefreshIfNeeded()
    {
        var dataCacheAgeOrNull = CloudbedsSingletons.GetDataCacheAgeOrNull();

        //No data cache age?  Then we need a refresh
        if(dataCacheAgeOrNull == null)
        {
            return true;
        }

        var dataCacheAge = dataCacheAgeOrNull.Value;
        IwsDiagnostics.Assert(dataCacheAge.TotalMinutes >= 0, "0204-930: Unexpected, data cache age is negative minutes: " + dataCacheAge.TotalMinutes.ToString());

        return (dataCacheAge > MaxCacheAgeBeforeRefresh);
    }

    /// <summary>
    /// Helper function
    /// </summary>
    private void TimerCallbackTarget_TriggerHeartbeatNotification()
    {
        //If we have a notification target, then call it...
        var notificationTarget = _refreshOccuredNotificationTarget;
        if (notificationTarget != null)
        {
            //Trap and record any errors... we don't want to crash the app if something fails
            try
            {
                notificationTarget.DataRefreshHeartbeat();
            }
            catch (Exception ex)
            {
                CloudbedsSingletons.StatusLogs.AddError("0204-821: Unexpected error in background scheduler notifications, " + ex.Message );
            }
        }
    }

    /// <summary>
    /// Broadcast a data refresh signal
    /// </summary>
    internal void SendDataRefreshSignal()
    {
        //Notify the app that a refresh occured (usually so UI can be updated
        var notificationTarget = _refreshOccuredNotificationTarget;
        if (notificationTarget != null)
        {
            notificationTarget.DataRefreshOccured();
        }
    }

    /*

    /// <summary>
    /// Choose a new refresh time
    /// </summary>
    private void SetNextRefreshTime()
    {
        const double RefreshMinutesFuzziness = 5; //Add a little fuzziness so parallel systems runnign spread out
        const double RefreshPeriod_Period_Minutes = 60 * 4; //4 Hours

        _nextRefreshAfterUtc = DateTime.UtcNow
            + TimeSpan.FromMinutes(RefreshPeriod_Period_Minutes)
            + TimeSpan.FromMinutes(CloudbedsSingletons.Randomizer.NextDouble() * RefreshMinutesFuzziness);

    }
    */

}
