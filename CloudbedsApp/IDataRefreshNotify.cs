
using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Web;

/// <summary>
/// </summary>
internal interface IDataRefreshNotify
{
    /// <summary>
    /// Called when data was actually refreshed
    /// </summary>
    void DataRefreshOccured();

    /// <summary>
    /// Called periodically as a heartbeat to indicate the background
    /// refresh is running
    /// </summary>
    void DataRefreshHeartbeat();

}
