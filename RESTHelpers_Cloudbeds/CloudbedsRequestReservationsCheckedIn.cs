using System;
using System.Xml;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Net;
using System.Text.Json;
using System.ComponentModel;

/// <summary>
/// A request to get the reservations data from Cloudbeds
/// </summary>
class CloudbedsRequestReservationsCheckedIn : CloudbedsRequestReservationsBase
{

    /// <summary>
    /// 
    /// </summary>
    /// <param name="cbServerInfo"></param>
    /// <param name="oauthRefreshToken"></param>
    /// <param name="statusLog"></param>
    public CloudbedsRequestReservationsCheckedIn(
        ICloudbedsServerInfo cbServerInfo, 
        ICloudbedsAuthSessionId authSession, 
        TaskStatusLogs statusLog)
        : base(cbServerInfo, authSession, statusLog)
    {
    }


    /// <summary>
    /// Generate the URL we need to query results
    /// </summary>
    /// <param name="pageNumber"></param>
    /// <param name="pageSize"></param>
    /// <returns></returns>
    protected override string GenerateQueryPageUrl(int pageNumber, int pageSize)
    {
        return CloudbedsUris.UriGenerate_GetCheckedInReservationsList(
            _cbServerInfo, pageNumber, pageSize);
    }

}
