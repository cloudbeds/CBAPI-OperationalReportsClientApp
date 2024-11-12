
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
using static CloudbedsDailyOperationsReportManager_v1;

/// <summary>
/// Produces a more detailed report showing every reservation that intersects with a given date
/// Has a row for every [date]x[sub-reservation]
/// </summary>
internal partial class CloudbedsDailyOperationsReportManager_v1_ResRoomDetails
{
    private readonly ReadOnlyCollection<DailyReportSet> _dateRangeReportSet;


    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="dateStart"></param>
    /// <param name="dateEnd"></param>
    /// <param name="reservationSet"></param>
    public CloudbedsDailyOperationsReportManager_v1_ResRoomDetails(DateTime dateStart, DateTime dateEnd,
        ICollection<CloudbedsReservationWithRooms_v1> reservationSet)
    {
        _dateRangeReportSet = GenerateDateRangeReportSet(dateStart, dateEnd, reservationSet).AsReadOnly();
    }

    /// <summary>
    /// Generate a CSV report with a row for each day
    /// </summary>
    /// <param name="prefixResValues">Because Excel will by default parse these large numbers into scientific notation, we may want to make them strings</param>
    /// <param name="prefixSubResValues">Because Excel will by default parse these large numbers into scientific notation, we may want to make them strings</param>
    /// <returns></returns>
    public CsvDataGenerator GenerateCsvReport(string prefixResValues = "res:", string prefixSubResValues = "subres:")
    {
        var csvManager = new CsvDataGenerator();
        prefixResValues = StringHelpers.CannonicalizeBlankString(prefixResValues);
        prefixSubResValues = StringHelpers.CannonicalizeBlankString(prefixSubResValues);


        string[] keys = { 
              "DATE"                 //[0]
            , "RESERVATION-ID"      //[1]
            , "SUB-RESERVATION-ID"  //[2]
            , "IN-STAY-STATE"       //[3]
            , "GUEST-ID"            //[4]
            , "GUEST-NAME"          //[5]
            , "ROOM-ID"             //[6]
            , "ROOM-NAME"           //[7]
            , "ROOM-TYPE"           //[8]
            , "CHECK-IN"            //[9]
            , "CHECK-OUT"           //[10]
            , "RESERVATION-STATUS"  //[11]
        };


        //Add a CSV row for each of the days
        foreach(var singleDayReport in _dateRangeReportSet)
        {
            foreach(var subReservation in singleDayReport.SubReservations)
            {

                string stayStateForDate = helper_GenerateStateStateForDate(singleDayReport.Date, subReservation); 

                string[] values = new string[keys.Length];
                values[0] = singleDayReport.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                values[1] = prefixResValues + subReservation.ParentReservationId;
                values[2] = prefixSubResValues+ subReservation.SubReservationId;
                values[3] = stayStateForDate;
                values[4] = subReservation.Guest_Id;
                values[5] = subReservation.Guest_Name;
                values[6] = subReservation.Room_Id;
                values[7] = subReservation.Room_Name;
                values[8] = subReservation.Room_TypeName;
                values[9] = subReservation.Room_CheckIn.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                values[10] = subReservation.Room_CheckOut.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                values[11] = subReservation.Room_Status;

                csvManager.AddKeyValuePairs(keys, values);

            }
        }

        return csvManager;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="date"></param>
    /// <param name="subReservation"></param>
    /// <returns></returns>
    private static string helper_GenerateStateStateForDate(DateTime date, CloudbedsReservationRoom_v1 subReservation)
    {
        if (date < subReservation.Room_CheckIn)
        {
            return "error: before check in date";
        }

        if (date > subReservation.Room_CheckOut)
        {
            return "error: past check out date";
        }

        if (date == subReservation.Room_CheckIn)
        {
            return "check-in today";
        }

        if (date == subReservation.Room_CheckOut)
        {
            return "check-out today";
        }

        return "middle of stay";
    }

    /// <summary>
    /// Builds reports for the range of dates
    /// </summary>
    /// <param name="dateStart"></param>
    /// <param name="dateEnd"></param>
    /// <param name="reservationSet"></param>
    /// <returns></returns>
    private List<DailyReportSet> GenerateDateRangeReportSet(DateTime dateStart, DateTime dateEnd, ICollection<CloudbedsReservationWithRooms_v1> reservationSet)
    {
        var outSet = new List<DailyReportSet>();
        var dateCurrent = dateStart.Date;
        while (dateCurrent <= dateEnd)
        {
            var reportForDate = GenerateDailyReportSet_SingleDate(dateCurrent, reservationSet);

            if (reportForDate != null)
            {
                outSet.Add(reportForDate);
            }

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
    private DailyReportSet GenerateDailyReportSet_SingleDate(DateTime dateTarget, ICollection<CloudbedsReservationWithRooms_v1> reservationSet)
    {
        List<CloudbedsReservationRoom_v1> outputSetOfSubReservation = new List<CloudbedsReservationRoom_v1>();
        //============================================================================
        //Look at each reservation in the set and see if it effects this date.
        //Reservations may have multiple rooms with different check in/out dates
        //============================================================================
        foreach (var thisReservation in reservationSet)
        {
            if (thisReservation.Reservation_Status != CloudbedsReservationStatus.STATUS_CANCELED)
            {

                helper_addSubReservationsWithIntersectingDates(dateTarget, outputSetOfSubReservation, thisReservation);
            }
        }

        return new DailyReportSet(dateTarget, outputSetOfSubReservation);
    }

    /// <summary>
    /// Add any sub-reservation with an intersecting date into the set
    /// </summary>
    /// <param name="dateTarget"></param>
    /// <param name="appendTo_subReservations"></param>
    /// <param name="thisReservation"></param>
    /// <exception cref="NotImplementedException"></exception>
    private void helper_addSubReservationsWithIntersectingDates(DateTime dateTarget, List<CloudbedsReservationRoom_v1> appendTo_subReservations, CloudbedsReservationWithRooms_v1 thisReservation)
    {
        foreach(var thisSubReservation in thisReservation.ReservationRooms)
        {
            helper_addSubReservationsWithIntersectingDates_subReservation(dateTarget, thisSubReservation, appendTo_subReservations);
        }
    }

    /// <summary>
    /// Look at an individual sub-reservation and see if it needs to get added to the set
    /// </summary>
    /// <param name="dateTarget"></param>
    /// <param name="thisSubReservation"></param>
    /// <param name="appendTo_subReservations"></param>
    private void helper_addSubReservationsWithIntersectingDates_subReservation(DateTime dateTarget, CloudbedsReservationRoom_v1 thisSubReservation, List<CloudbedsReservationRoom_v1> appendTo_subReservations)
    {
        //If sub reservation is BEFORE the date, do nothing
        if(dateTarget < thisSubReservation.Room_CheckIn)
        {
            return;
        }

        //If sub reservation is AFTER the date, do nothing
        if (dateTarget > thisSubReservation.Room_CheckOut)
        {
            return;
        }

        //Add it to the set that intersects today
        appendTo_subReservations.Add(thisSubReservation);
    }
}

