
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Web;

/// <summary>
/// Manages daily operations report items
/// </summary>
internal  partial class CloudbedsDailyOperationsReportManager
{
    public ICollection<DailyReport> DailyReports 
    { get { 
            return _dailyReports; 
        }  
    }

    private ReadOnlyCollection<DailyReport> _dailyReports;
    public CloudbedsDailyOperationsReportManager(DateTime dateStart, DateTime dateEnd, 
        ICollection<CloudbedsReservationWithRooms> reservationSet)
    {
        _dailyReports = GenerateDailyReportSet(dateStart, dateEnd, reservationSet).AsReadOnly();
    }


    /// <summary>
    /// Generate a CSV report with a row for each day
    /// </summary>
    public CsvDataGenerator GenerateCsvReport()
    {
        var csvManager = new CsvDataGenerator();


        string[] keys = { "DATE", "CHECK-INS", "CHECK-OUTS", "STAY-OVERS", "ROOM-TURNOVERS", "UNASSIGNED-CHECKINS" };


        //Add a CSV row for each of the days
        foreach(var singleDayReport in _dailyReports)
        {
            string [] values = new string[keys.Length];
            values[0] = singleDayReport.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            values[1] = singleDayReport.NumberCheckIns.ToString();
            values[2] = singleDayReport.NumberCheckOuts.ToString();
            values[3] = singleDayReport.NumberStayOvers.ToString();
            values[4] = singleDayReport.NumberRoomTurnoversRequired.ToString();
            values[5] = singleDayReport.NumberCheckIns_RoomNotAssigned.ToString();

            csvManager.AddKeyValuePairs(keys, values);
        }

        return csvManager;
    }

    /// <summary>
    /// Builds reports for the range of dates
    /// </summary>
    /// <param name="dateStart"></param>
    /// <param name="dateEnd"></param>
    /// <param name="reservationSet"></param>
    /// <returns></returns>
    private List<DailyReport> GenerateDailyReportSet (DateTime dateStart, DateTime dateEnd, ICollection<CloudbedsReservationWithRooms> reservationSet)
    {
        var outSet = new List<DailyReport>();
        var dateCurrent = dateStart.Date;
        while(dateCurrent <= dateEnd)
        {
            var reportForDate = GenerateDailyReportSet_SingleDate(dateCurrent, reservationSet);
            outSet.Add(reportForDate);

            //Advance 1 day
            dateCurrent = dateCurrent.AddDays(1);
        }

        return outSet;
    }


    /// <summary>
    /// Deternates a daily operational report object for a SINGLE day
    /// </summary>
    /// <param name="dateCurrent"></param>
    /// <param name="reservationSet"></param>
    /// <returns></returns>
    private DailyReport GenerateDailyReportSet_SingleDate(DateTime dateTarget, ICollection<CloudbedsReservationWithRooms> reservationSet)
    {
        int numCheckIns = 0;
        int numCheckInsWithUnassignedRooms = 0;
        int numStayovers = 0;
        int numCheckOuts = 0;
        var listCheckInRoomIds = new List<string>();
        var listCheckOutsRoomIds = new List<string>();


        //============================================================================
        //Look at each reservation in the set and see if it effects this date.
        //Reservations may have multiple rooms with different check in/out dates
        //============================================================================
        foreach (var thisReservation in  reservationSet) 
        {
            //==================================================
            //Any Rooms Checking in?
            //==================================================
            var checkInsInfo = DateCheck_CalculateNumberForReservation_CheckIns(dateTarget, thisReservation);
            numCheckIns += checkInsInfo.CheckInsRoomCount;
            numCheckInsWithUnassignedRooms += checkInsInfo.CheckInsRoomUnassignedCount;
            //Keep all the roomIDs for check ins
            if (checkInsInfo.KnownRoomIds != null)
            {
                listCheckInRoomIds.AddRange(checkInsInfo.KnownRoomIds);
            }

            //==================================================
            //Any Rooms Checking out?
            //==================================================
            var checkOutsInfo = DateCheck_CalculateNumberForReservation_CheckOuts(dateTarget, thisReservation);
            numCheckOuts += checkOutsInfo.CheckOutsRoomCount;
            //Keep all the roomIDs for check ins
            if (checkOutsInfo.KnownRoomIds != null)
            {
                listCheckOutsRoomIds.AddRange(checkOutsInfo.KnownRoomIds);
            }



            //==================================================
            //Any Rooms that have stayovers?
            //==================================================
            numStayovers += DateCheck_CalculateNumberForReservation_Stayovers(dateTarget, thisReservation);
        }


        //Create the report object
        return new DailyReport(dateTarget, numCheckIns, numStayovers, numCheckOuts, numCheckInsWithUnassignedRooms, listCheckInRoomIds, listCheckOutsRoomIds);
    }



    /// <summary>
    /// See if the reservation contains any rooms that have check ins on this date...
    /// </summary>
    /// <param name="dateTarget"></param>
    /// <param name="thisReservation"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    private CheckInsInfo DateCheck_CalculateNumberForReservation_CheckIns(DateTime dateTarget, CloudbedsReservationWithRooms thisReservation)
    {

        //If it is canceleld, ignore it
        if(thisReservation.Reservation_Status == CloudbedsReservationStatus.STATUS_CANCELED)
        {
            return new CheckInsInfo();
        }

        var listRoomIds = new List<string>();
        int countUnassignedRooms = 0;
        //Check each room for a check in date
        int numberCheckIns = 0;
        foreach(var thisRoom in thisReservation.ReservationRooms)
        {
            if(thisRoom.Room_CheckIn == dateTarget)
            {
                numberCheckIns++;

                string roomIdAssigned = thisRoom.Room_Id;
                //If the room id is not yet assigned, note that
                if (string.IsNullOrWhiteSpace(roomIdAssigned))
                {
                    countUnassignedRooms++;
                }
                else 
                {
                    listRoomIds.Add(roomIdAssigned);
                }
            }
        }

        //return numberCheckIns;
        return new CheckInsInfo(numberCheckIns, countUnassignedRooms, listRoomIds);
    }

    /// <summary>
    /// See if the reservation contains any rooms that have check outs on this date...
    /// </summary>
    /// <param name="dateTarget"></param>
    /// <param name="thisReservation"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    private CheckOutsInfo DateCheck_CalculateNumberForReservation_CheckOuts(DateTime dateTarget, CloudbedsReservationWithRooms thisReservation)
    {
        //If it is canceleld, ignore it
        if (thisReservation.Reservation_Status == CloudbedsReservationStatus.STATUS_CANCELED)
        {
            return new CheckOutsInfo();
        }

        //Check each room for a check in date
        int numberCheckOuts = 0;
        var listRoomIds = new List<string>();
        foreach (var thisRoom in thisReservation.ReservationRooms)
        {
            if (thisRoom.Room_CheckOut == dateTarget)
            {
                numberCheckOuts++;

                string roomIdAssigned = thisRoom.Room_Id;
                //If the room id is not yet assigned, note that
                if (string.IsNullOrWhiteSpace(roomIdAssigned))
                {
                    //countUnassignedRooms++;
                }
                else
                {
                    listRoomIds.Add(roomIdAssigned);
                }

            }
        }

        return new CheckOutsInfo(numberCheckOuts, listRoomIds);
    }


    /// <summary>
    /// See if the reservation contains any rooms that have stayovers on this date...
    /// </summary>
    /// <param name="compareDate"></param>
    /// <param name="thisReservation"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    private int DateCheck_CalculateNumberForReservation_Stayovers(DateTime compareDate, CloudbedsReservationWithRooms thisReservation)
    {
        //If it is canceleld, ignore it
        if (thisReservation.Reservation_Status == CloudbedsReservationStatus.STATUS_CANCELED)
        {
            return 0;
        }

        //Check each room for a check in date
        int numberStayOvers = 0;
        foreach (var thisRoom in thisReservation.ReservationRooms)
        {
            if ((compareDate > thisRoom.Room_CheckIn) &&
                (compareDate < thisRoom.Room_CheckOut))
            {
                numberStayOvers++;
            }
        }

        return numberStayOvers;
    }
}
