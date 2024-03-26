
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
        /// <param name="set1"></param>
        /// <param name="set2"></param>
        /// <returns></returns>
        private static int CalculateSetOverlap(ICollection<string> set1, ICollection<string> set2)
        {
            //Empty set?
            if ((set1 == null) || (set1.Count == 0))
            {  return 0; }


            //Empty set?
            if ((set2 == null) || (set2.Count == 0))
            { return 0; }

            int overlap = 0;
            foreach(var thisMember_set1 in set1)
            {

                if(set2.Contains(thisMember_set1))
                {
                    overlap++;
                }
            }

            return overlap;
        }
    }
}
