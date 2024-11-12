using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

/// <summary>
/// Cloudbeds Reservation
/// </summary>
class CloudbedsReservationWithRooms_v2
{
    public readonly string Guest_Id;
    public readonly string Guest_Name;
    public readonly string Reservation_Id;
    public readonly string Reservation_Status;
    public readonly string PropertyId;
    public readonly string Reservation_StartDate_Text;
    public readonly DateTime Reservation_StartDate;
    public readonly DateTime Reservation_EndDate;
    public readonly string Reservation_EndDate_Text;
    public readonly IReadOnlyCollection<CloudbedsReservationRoom_v2> ReservationRooms;

    const string CB_DATE_FORMAT = "yyyy-MM-dd";

    /// <summary>
    /// String we will use for wildcard searches for guests
    /// </summary>
    private readonly string _cannonicalTextSearchString;
    public override string ToString()
    {
        return "Reservation: " + this.Guest_Name; // + ", Room: " + this.Room_Name;
    }

    public CloudbedsReservationWithRooms_v2(
        string reservationId,
        string reservationStatus,
        string propertyId,
        string reservationStartDate,
        string reservationEndDate,
        string guestId,
        string guestName,
        ICollection<CloudbedsReservationRoom_v2> reservationRooms
        )
    {
        this.Reservation_Id = reservationId;
        this.Reservation_Status = reservationStatus;
//        this.Reservation_Balance = reservationBalance;
        this.PropertyId = propertyId;
//        this.Reservation_Adults = reservationAdults;
//        this.Reservation_Children = reservationChildren;
        this.Reservation_StartDate_Text = reservationStartDate;
        Reservation_StartDate = DateTime.ParseExact(reservationStartDate, CB_DATE_FORMAT, CultureInfo.InvariantCulture);

        this.Reservation_EndDate_Text = reservationEndDate;
        Reservation_EndDate = DateTime.ParseExact(reservationEndDate, CB_DATE_FORMAT, CultureInfo.InvariantCulture);

        this.Guest_Id = guestId;
        this.Guest_Name = guestName;

        this.ReservationRooms = new List<CloudbedsReservationRoom_v2>(reservationRooms).AsReadOnly();


        _cannonicalTextSearchString = helper_CreateCannonicalSearchTerm();
        
    }

    /// <summary>
    /// Builds a string we can use for text match searches to find a guest from common
    /// criteria
    /// </summary>
    /// <returns></returns>
    private string helper_CreateCannonicalSearchTerm()
    {
        //Buid the cannonical text-search string
        var sb = new StringBuilder();
        helper_appendSearchTerm(sb, this.Guest_Name);
        //helper_appendSearchTerm(sb, this.Guest_Email);
        //helper_appendSearchTerm(sb, this.Guest_CellPhone);
//        helper_appendSearchTerm(sb, this.Room_Name);

        return sb.ToString().Trim().ToLower();
    }

    /// <summary>
    /// Append the search term
    /// </summary>
    /// <param name="sb"></param>
    /// <param name="text"></param>
    private void helper_appendSearchTerm(StringBuilder sb, string text)
    {
        if(string.IsNullOrWhiteSpace(text))
        {
            return;
        }

        if(sb.Length > 0)
        {
            sb.Append("|"); //Put a seperator in there between terms that is not used in the search
        }
        sb.Append(text.Trim());
    }
}
