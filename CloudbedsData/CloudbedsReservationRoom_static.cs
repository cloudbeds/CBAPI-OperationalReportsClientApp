using System;
using System.Text;

/// <summary>
/// Cloudbeds CloudbedsReservationRoom
/// </summary>
partial class CloudbedsReservationRoom
{
    //[2024-11-04] API docs indicate the status is "canceled" with one "l"
    //[2024-11-04]    public const string RoomStatus_Cancelled = "cancelled";
    public const string RoomStatus_Cancelled = "canceled";
    public const string RoomStatus_InHouse = "in_house";
    public const string RoomStatus_CheckedOut = "checked_out";
    public const string RoomStatus_NotCheckedIn = "not_checked_in";

    public static bool IsRoomStatusKnownState(CloudbedsReservationRoom reservationRoom)
    {
        return IsRoomStatusKnownState(reservationRoom.Room_Status);
    }

        /// <summary>
        /// TRUE if this Room Status is a known and valid state
        /// </summary>
        /// <param name="reservationRoom"></param>
        /// <returns></returns>
    public static bool IsRoomStatusKnownState(string roomStatusText)
    {
        switch (roomStatusText) 
        {
            case RoomStatus_Cancelled:
            case RoomStatus_InHouse:
            case RoomStatus_CheckedOut:
            case RoomStatus_NotCheckedIn:
                return true;

            default:
                return false;
        }
    }
}
