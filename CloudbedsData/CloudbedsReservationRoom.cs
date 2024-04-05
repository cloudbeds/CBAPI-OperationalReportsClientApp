using System;
using System.Text;

/// <summary>
/// Cloudbeds CloudbedsReservationRoom
/// </summary>
partial class CloudbedsReservationRoom
{
    public readonly string Room_TypeId;
    public readonly string Room_TypeName;
    public readonly DateTime Room_CheckIn;
    public readonly DateTime Room_CheckOut;
    public readonly string Guest_Id;
    public readonly string Guest_Name;
    public readonly string Room_Id;
    public readonly string Room_Name;
    public readonly string Room_Status;
    public readonly string SubReservationId;
    public readonly string ParentReservationId;

    /// <summary>
    /// String we will use for wildcard searches for guests
    /// </summary>
    private readonly string _cannonicalTextSearchString;
    public override string ToString()
    {
        return "Reservation Room";// + this.Guest_Name + ", Room: " + this.Room_Name;
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="roomTypeId"></param>
    /// <param name="dateCheckIn"></param>
    /// <param name="dateCheckOut"></param>
    public CloudbedsReservationRoom(
        string parentReservationId,
        string subReservationId,
        string roomTypeId,
        string roomTypeName,
        DateTime dateCheckIn,
        DateTime dateCheckOut,
        string guest_Id,
        string guest_Name,
        string roomId,
        string roonName,
        string roomStatus
        )
    {
        //Sanity test...
        if (!IsRoomStatusKnownState(roomStatus)) 
        {
            IwsDiagnostics.Assert(false, "240327-240: Unknown room status: " + roomStatus + ", sub-reservation: " + subReservationId);
            CloudbedsSingletons.StatusLogs.AddError("Unknown room status: " + roomStatus + ", sub-reservation: " + subReservationId);
        }

        this.ParentReservationId = StringHelpers.CannonicalizeBlankString(parentReservationId);
        this.SubReservationId = StringHelpers.CannonicalizeBlankString(subReservationId);

        this.Room_TypeId = StringHelpers.CannonicalizeBlankString(roomTypeId);
        this.Room_CheckIn = dateCheckIn;
        this.Room_CheckOut = dateCheckOut;

        this.Guest_Id = StringHelpers.CannonicalizeBlankString(guest_Id);
        this.Guest_Name = StringHelpers.CannonicalizeBlankString(guest_Name);

        this.Room_Id = StringHelpers.CannonicalizeBlankString(roomId);
        this.Room_Name = StringHelpers.CannonicalizeBlankString(roonName);

        this.Room_Status = StringHelpers.CannonicalizeBlankString(roomStatus);
    }

}
