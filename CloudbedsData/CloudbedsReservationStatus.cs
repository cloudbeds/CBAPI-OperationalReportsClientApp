using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

class CloudbedsReservationStatus
{
  public const string STATUS_NOTCONFIRMED = "not_confirmed"; // Reservation is pending confirmation
  public const string STATUS_CONFIRMED = "confirmed"; // Reservation is confirmed
  public const string STATUS_CANCELED = "canceled"; // Reservation is canceled
  public const string STATUS_CHECKED_IN = "checked_in"; // Guest is in hotel
  public const string STATUS_CHECKED_OUT = "checked_out"; // Guest already left hotel
  public const string STATUS_NOSHOW = "no_show"; // Guest didn't showed up on check-in date
}
