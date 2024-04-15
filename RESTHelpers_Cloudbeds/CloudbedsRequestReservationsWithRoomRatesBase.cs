using System;
using System.Xml;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Net;
using System.Text.Json;
using System.ComponentModel;

/// <summary>
/// A request to get the Reservations data from Cloudbeds
/// </summary>
abstract class CloudbedsRequestReservationsWithRoomRatesBase : CloudbedsAuthenticatedRequestBase
{
    /// <summary>
    /// Derrived classes must implement this to return the necessary query URL
    /// </summary>
    /// <param name="pageNumber"></param>
    /// <param name="pageSize"></param>
    /// <returns></returns>
    protected abstract string GenerateQueryPageUrl(int pageNumber, int pageSize);


    protected readonly ICloudbedsServerInfo _cbServerInfo;
    private JsonDocument _commandResultJson = null;
    private ReadOnlyCollection<CloudbedsReservationWithRooms> _jsonResult_reservations = null;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="cbServerInfo"></param>
    /// <param name="oauthRefreshToken"></param>
    /// <param name="statusLog"></param>
    public CloudbedsRequestReservationsWithRoomRatesBase(
        ICloudbedsServerInfo cbServerInfo, 
        ICloudbedsAuthSessionId authSession, 
        TaskStatusLogs statusLog)
        : base(authSession, statusLog)
    {
        _cbServerInfo = cbServerInfo;
    }

    /// <summary>
    /// The list of reservations returned by the server
    /// </summary>
    public ReadOnlyCollection<CloudbedsReservationWithRooms> CommandResults_Reservations
    {
        get
        {
            return _jsonResult_reservations;
        }
    }

    public JsonDocument CommandResults_Json
    {
        get
        {
            return _commandResultJson;
        }
    }

    /// <summary>
    /// Text summing up the return values
    /// </summary>
    public string CommandResults_SummaryText
    {
        get
        {

            var colGuests = _jsonResult_reservations;
            if(colGuests == null)
            {
                return "No query results";
            }

            return colGuests.ToString() + " reservations";
        }
    }


    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public bool ExecuteRequest()
    {
        try
        {
            return ExecuteRequest_inner();
        }
        catch(Exception ex)
        {
            this.StatusLog.AddError("0205-1008: Error querying reservations: " + ex.Message);
            return false;
        }
    }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
    private bool ExecuteRequest_inner()
    {
        const int queryResults_pageSize = 100; //This is the # of results expects
        var allReservations = new List<CloudbedsReservationWithRooms>();
        var latchAllGuestsReturned = new SimpleLatch();

        //============================================================
        //As long as we are getting results back from each page,
        //keep requesting the next page.
        //
        //NOTE: We could probably be more efficent here, and check
        //to make sure each page is returning the 'max #' results,
        //and then stop asking for the next page if the # falls short
        //of a full page.
        //============================================================
        int currentQueryPage = 1;
        while(latchAllGuestsReturned.Value == false)
        {

            //Query for guests
            var pageResults_Items = ExecuteRequest_SinglePage(
                currentQueryPage, 
                queryResults_pageSize);

            //If we not no results back... we are all done querying for guests
            if((pageResults_Items == null) || (pageResults_Items.Count == 0))
            {
                latchAllGuestsReturned.Trigger();
            }
            else
            {
                allReservations.AddRange(pageResults_Items);
                currentQueryPage++; //Advance to the next page
            }
        }

        //Store the parsed query results
        _jsonResult_reservations = allReservations.AsReadOnly();
        return true; //Success
    }


    /// <summary>
    /// https://hotels.cloudbeds.com/api/docs/#api-Reservation-getReservations
    /// </summary>
    /// 
    public List<CloudbedsReservationWithRooms> ExecuteRequest_SinglePage(int pageNumber, int pageSize)
    {
        string url = GenerateQueryPageUrl(pageNumber, pageSize);

        //Request the data from server
        this.StatusLog.AddStatus("Querying reservations, page: " + pageNumber.ToString());
        this.StatusLog.AddStatus("Custom web request: " + url, -10);

        //========================================================================
        //Send the request
        //========================================================================
        var httpRequest = CreateHttpRequest_Get(url);

        //========================================================================
        //Get the response
        //========================================================================
        var response = GetWebResponseLogErrors(httpRequest, "request hotel reservations list, page #" + pageNumber.ToString());

        //https://docs.microsoft.com/en-us/dotnet/standard/serialization/system-text-json-use-dom-utf8jsonreader-utf8jsonwriter?pivots=dotnet-6-0
        using (response)
        {
            if(response.StatusCode == HttpStatusCode.Unauthorized)
            {
                throw new Exception("0415-833: Server request returned 'unauthorized' for " + url);
            }

            var jsonOut = GetWebResponseAsJson(response, false);
            _commandResultJson = jsonOut;

            //----------------------------------------------------------------
            //If the 'data' node is missing, specifically record this error
            //because it indicates we did not get the expected result back 
            //from the server
            //----------------------------------------------------------------
            JsonElement jsonResult_dataNode;
            if (!jsonOut.RootElement.TryGetProperty("data", out jsonResult_dataNode))
            {
                throw new Exception("0223-1015: No Json 'data' node found");
            }

            return ExecuteRequest_ParseReservationsFromResponse(jsonResult_dataNode);
        }
    }


    /// <summary>
    /// Parses the array of results
    /// </summary>
    /// <param name="jsonResult_dataNode"></param>
    /// <returns></returns>
    private List<CloudbedsReservationWithRooms> ExecuteRequest_ParseReservationsFromResponse(JsonElement jsonResult_dataNode)
    {
        var listOut = new List<CloudbedsReservationWithRooms>();
        if(jsonResult_dataNode.ValueKind != JsonValueKind.Array)
        {
            throw new Exception("0205-1016: Expected Json Array");
        }

        var reservationSet = jsonResult_dataNode.EnumerateArray();
        foreach (var jsonSingleReservation in reservationSet)
        {
            CloudbedsReservationWithRooms thisItem = ExecuteRequest_ParseGuestsFromResponse_SingleReservation(jsonSingleReservation);
            if(thisItem != null)
            {
                listOut.Add(thisItem);
            }
            else //Unxpected -- we did not parse a guest?
            {
                this.StatusLog.AddError("0205-1021: NULL reservation parse");
            }
        }
        
        return listOut;
    }

    private CloudbedsReservationWithRooms ExecuteRequest_ParseGuestsFromResponse_SingleReservation(JsonElement jsonSingleGuest)
    {
        try
        {
            return ExecuteRequest_ParseGuestsFromResponse_SingleReservation_inner(jsonSingleGuest);
        }
        catch(Exception ex)
        {
            this.StatusLog.AddError("0205-1022: Error parsing reservation: " + ex.Message);
            return null;
        }
    }

    private CloudbedsReservationWithRooms ExecuteRequest_ParseGuestsFromResponse_SingleReservation_inner(JsonElement jsonSingleReservation)
    {
        string reservationId =
            JsonParseHelpers.FindJasonAttributeValue_String(jsonSingleReservation, "reservationID");

        string propertyId =
            JsonParseHelpers.FindJasonAttributeValue_String(jsonSingleReservation, "propertyID");

        string reservationStatus =
        JsonParseHelpers.FindJasonAttributeValue_String(jsonSingleReservation, "status");



        string reservationStartDate_text =
            JsonParseHelpers.FindJasonAttributeValue_String(jsonSingleReservation, "reservationCheckIn");
        string reservationEndDate_text =
            JsonParseHelpers.FindJasonAttributeValue_String(jsonSingleReservation, "reservationCheckOut");
        string guestId =
            JsonParseHelpers.FindJasonAttributeValue_String(jsonSingleReservation, "guestID");
        string guestName = 
            JsonParseHelpers.FindJasonAttributeValue_String(jsonSingleReservation, "guestName");

        //Get the list of ROOMS in the reservation
        var jsonResRooms = jsonSingleReservation.GetProperty("rooms");
        var jsonArrRooms = jsonResRooms.EnumerateArray();
        var resRooms = new List<CloudbedsReservationRoom>();
        foreach(var jsonSingleRoom in jsonArrRooms)
        {
            var parsedSingleRooms = ParseSingleRoomFromJSon(jsonSingleRoom, reservationId);
            resRooms.Add(parsedSingleRooms);
        }


        return new CloudbedsReservationWithRooms(
            reservationId
            , reservationStatus
            , propertyId
            , reservationStartDate_text, reservationEndDate_text
            , guestId, guestName
            , resRooms
            );
    }

    /// <summary>
    /// Parse the single room's data
    /// </summary>
    /// <param name="jsonRoom"></param>
    /// <returns></returns>
    private CloudbedsReservationRoom ParseSingleRoomFromJSon(JsonElement jsonRoom, string reservationId)
    {
        string roomTypeId =
            JsonParseHelpers.FindJasonAttributeValue_String(jsonRoom, "roomTypeID");

        string roomTypeName =
            JsonParseHelpers.FindJasonAttributeValue_String(jsonRoom, "roomTypeName");

        string checkInDate_text =
            JsonParseHelpers.FindJasonAttributeValue_String(jsonRoom, "roomCheckIn");
        var checkInDate = JsonParseHelpers.ParseDateFromJsonText(checkInDate_text);

        string checkOutDate_text =
            JsonParseHelpers.FindJasonAttributeValue_String(jsonRoom, "roomCheckOut");

        var checkOutDate = JsonParseHelpers.ParseDateFromJsonText(checkOutDate_text);

        string guestId =
            JsonParseHelpers.FindJasonAttributeValue_String(jsonRoom, "guestID");

        string guestName=
            JsonParseHelpers.FindJasonAttributeValue_String(jsonRoom, "guestName");

        string roomId =
            JsonParseHelpers.FindJasonAttributeValue_String(jsonRoom, "roomID");

        string roomName =
            JsonParseHelpers.FindJasonAttributeValue_String(jsonRoom, "roomName");

        string roomStatus =
            JsonParseHelpers.FindJasonAttributeValue_String(jsonRoom, "roomStatus");

        IwsDiagnostics.Assert(!string.IsNullOrWhiteSpace(roomStatus), "240327-224: no room status found" + jsonRoom.ToString());

        string subReservationId =
            JsonParseHelpers.FindJasonAttributeValue_String(jsonRoom, "subReservationID");

        IwsDiagnostics.Assert(!string.IsNullOrWhiteSpace(roomStatus), "240327-225: no sub-reservation id found" + jsonRoom.ToString());

        return new CloudbedsReservationRoom(
            reservationId,
            subReservationId, 
            roomTypeId, roomTypeName, 
            checkInDate, checkOutDate, 
            guestId, guestName,
            roomId, roomName, roomStatus);
    }
}
