using System;
using System.Text;

/// <summary>
/// Cloudbeds Guest
/// </summary>
class CloudbedsGuest
{
    public readonly string Guest_Id;
    public readonly string Guest_Name;
    public readonly string Guest_Email;
    public readonly string Guest_CellPhone;
    public readonly string Reservation_Id;
    public readonly string Reservation_StartDate_Text;
    public readonly string Reservation_EndDate_Text;
    public readonly string Room_Name;
    public readonly string Room_Id;

    /// <summary>
    /// String we will use for wildcard searches for guests
    /// </summary>
    private readonly string _cannonicalTextSearchString;
    public override string ToString()
    {
        return "Guest: " + this.Guest_Name + ", Room: " + this.Room_Name;
    }

    public CloudbedsGuest(
        string guestId,
        string guestName, 
        string guestEmail, 
        string guestCellPhone,
        string reservationId,
        string reservationStartDate,
        string reservationEndDate,
        string roomId, 
        string roomName
        )
    {
        this.Guest_Id = guestId;
        this.Guest_Name = guestName;
        this.Guest_Email = guestEmail;
        this.Guest_CellPhone = guestCellPhone;
        this.Reservation_Id = reservationId;
        this.Reservation_StartDate_Text = reservationStartDate;
        this.Reservation_EndDate_Text = reservationEndDate;
        this.Room_Id = roomId;
        this.Room_Name = roomName;

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
        helper_appendSearchTerm(sb, this.Guest_Email);
        helper_appendSearchTerm(sb, this.Guest_CellPhone);
        helper_appendSearchTerm(sb, this.Room_Name);

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
