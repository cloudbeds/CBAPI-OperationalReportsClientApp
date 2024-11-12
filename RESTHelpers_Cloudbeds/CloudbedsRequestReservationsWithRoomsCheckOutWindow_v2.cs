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
class CloudbedsRequestReservationsWithRoomsCheckOutWindow_v2 : 
                                CloudbedsRequestReservationsBase_v2
{

    public const string ReservationStatusFilter_All = "";
    public const string ReservationStatusFilter_CheckedIn = "checked_in";
    public const string ReservationStatusFilter_Confirmed = "confirmed";
    public const string ReservationStatusFilter_NotConfirmed = "not_confirmed";
    public const string ReservationStatusFilter_NoShow = "no_show";

    private readonly DateTime _startDateCheckIn;
    private readonly DateTime _endDateCheckIn;
    private readonly string _reservationStatusFilter;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="cbServerInfo"></param>
    /// <param name="oauthRefreshToken"></param>
    /// <param name="statusLog"></param>
    public CloudbedsRequestReservationsWithRoomsCheckOutWindow_v2(
        ICloudbedsServerInfo cbServerInfo, 
        ICloudbedsAuthSessionId authSession, 
        TaskStatusLogs statusLog,
        DateTime startDateCheckIn,
        DateTime endDateCheckIn)
        : base(cbServerInfo, authSession, statusLog)
    {
        _startDateCheckIn = startDateCheckIn;
        _endDateCheckIn = endDateCheckIn;
    }


    /// <summary>
    /// Generate the URL we need to query results
    /// </summary>
    /// <param name="pageNumber"></param>
    /// <param name="pageSize"></param>
    /// <returns></returns>
    protected override string GenerateQueryPageUrl(int pageNumber, int pageSize)
    {
        return CloudbedsUris.GetRoomReservationsWithRooms(
            _cbServerInfo, _startDateCheckIn, _endDateCheckIn, pageNumber, pageSize);
    }

}
