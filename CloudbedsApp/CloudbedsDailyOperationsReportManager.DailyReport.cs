
using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Web;
using System.Collections.Generic;

/// <summary>
/// </summary>
internal partial class CloudbedsDailyOperationsReportManager
{
    /// <summary>
    /// Daily report for operations 
    /// </summary>
    public class DailyReport
    {
        public readonly DateTime Date;
        public readonly int NumberCheckIns;
        public readonly int NumberStayOvers;
        public readonly int NumberCheckOuts;
        public readonly int NumberCheckIns_RoomNotAssigned;
        public readonly int NumberRoomTurnoversRequired;

        private readonly ICollection<string> _checkInRoomIds;
        private readonly ICollection<string> _checkOutRoomIds;
        public DailyReport(
            DateTime date, 
            int numberCheckIns, 
            int numberStayOvers, 
            int numberCheckOuts, 
            int numCheckInsWithUnassignedRooms, 
            ICollection<string> checkInRoomIds, 
            ICollection<string> checkOutsRoomIds)
        {
            this.Date = date;
            this.NumberCheckIns = numberCheckIns;
            this.NumberStayOvers = numberStayOvers;
            this.NumberCheckOuts = numberCheckOuts;

            this.NumberCheckIns_RoomNotAssigned = numCheckInsWithUnassignedRooms;
            _checkInRoomIds = checkInRoomIds;
            _checkOutRoomIds = checkOutsRoomIds;

            this.NumberRoomTurnoversRequired = CalculateSetOverlap(checkInRoomIds, checkOutsRoomIds);
        }

        /// <summary>
        /// Looks for overlap
        /// </summary>
        /// <param name="checkInRoomIds"></param>
        /// <param name="checkOutsRoomIds"></param>
        /// <returns></returns>
        private int CalculateSetOverlap(ICollection<string> checkInRoomIds, ICollection<string> checkOutsRoomIds)
        {
            //Empty set?
            if ((checkInRoomIds == null) || (checkInRoomIds.Count == 0))
            {  return 0; }


            //Empty set?
            if ((checkOutsRoomIds == null) || (checkOutsRoomIds.Count == 0))
            { return 0; }

            int overlap = 0;
            foreach(var thisRoomId in checkInRoomIds)
            {
                if(checkInRoomIds.Contains(thisRoomId))
                {
                    overlap++;
                }
            }

            return overlap;
        }
    }
}
