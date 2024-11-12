
using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Web;
using System.Collections.Generic;

/// <summary>
/// </summary>
internal partial class CloudbedsDailyOperationsReportManager_v2_ResRoomDetails
{

    /// <summary>
    /// Daily report for operations 
    /// </summary>
    public class DailyReportSet
    {
        public readonly DateTime Date;

        /// <summary>
        /// The sub reservations active on this date
        /// </summary>
        readonly IReadOnlyCollection<CloudbedsReservationRoom_v2> _resRoomsForDate;


        /// <summary>
        /// All the sub reservations that intersect with this date
        /// </summary>
        public IReadOnlyCollection<CloudbedsReservationRoom_v2> SubReservations
        {
            get
            {
                return _resRoomsForDate;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="date"></param>
        /// <param name="reservationRoomsForDate"></param>
        public DailyReportSet(
            DateTime date,
            ICollection<CloudbedsReservationRoom_v2> reservationRoomsForDate)
        {
            this.Date = date;
            _resRoomsForDate = new List<CloudbedsReservationRoom_v2>(reservationRoomsForDate).AsReadOnly();
        }
    }
}
