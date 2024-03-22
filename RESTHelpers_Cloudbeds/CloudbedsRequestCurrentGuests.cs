using System;
using System.Xml;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Net;
using System.Text.Json;

/// <summary>
/// A request to get the dashboard data from Cloudbeds
/// </summary>
class CloudbedsRequestCurrentGuests : CloudbedsAuthenticatedRequestBase
{
    private readonly ICloudbedsServerInfo _cbServerInfo;
    private JsonDocument _commandResultJson = null;
    private ReadOnlyCollection<CloudbedsGuest> _jsonResult_guests = null;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="cbServerInfo"></param>
    /// <param name="oauthRefreshToken"></param>
    /// <param name="statusLog"></param>
    public CloudbedsRequestCurrentGuests(
        ICloudbedsServerInfo cbServerInfo, 
        ICloudbedsAuthSessionId authSession, 
        TaskStatusLogs statusLog)
        : base(authSession, statusLog)
    {
        _cbServerInfo = cbServerInfo;
    }

    /// <summary>
    /// The list of guests returned by the server
    /// </summary>
    public ReadOnlyCollection<CloudbedsGuest> CommandResults_Guests
    {
        get
        {
            return _jsonResult_guests;
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

            var colGuests = _jsonResult_guests;
            if(colGuests == null)
            {
                return "No query results";
            }

            return colGuests.ToString() + " guests";
        }
    }
    /*
    /// <summary>
    /// Helper function to format some text
    /// </summary>
    /// <param name="sb"></param>
    /// <param name="propertyName"></param>
    /// <param name="propertyValue"></param>
    private void helper_AppendValuePairText(StringBuilder sb, string propertyName, int? propertyValue)
    {
        sb.Append(propertyName);
        sb.Append(" : ");
        if(propertyValue == null)
        {
            sb.Append("null");
        }
        else
        {
            sb.Append(propertyValue.Value.ToString());
        }
        sb.AppendLine();
    }
    */

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
            this.StatusLog.AddError("1021-926: Error querying guests: " + ex.Message);
            return false;
        }
    }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
    public bool ExecuteRequest_inner()
    {
        const int queryGuestPages_pageSize = 100; //This is the # of results expects
        var allGuests = new List<CloudbedsGuest>();
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
            var pageResults_Guests = ExecuteRequest_SinglePage(
                currentQueryPage, 
                queryGuestPages_pageSize);

            //If we not no results back... we are all done querying for guests
            if((pageResults_Guests == null) || (pageResults_Guests.Count == 0))
            {
                latchAllGuestsReturned.Trigger();
            }
            else
            {
                allGuests.AddRange(pageResults_Guests);
                currentQueryPage++; //Advance to the next page
            }
        }

        //Store the parsed query results
        _jsonResult_guests = allGuests.AsReadOnly();
        return true; //Success
    }

    /// <summary>
    /// https://hotels.cloudbeds.com/api/docs/#api-Guest-getGuestsByStatus
    /// </summary>
    /// 
    public List<CloudbedsGuest> ExecuteRequest_SinglePage(int pageNumber, int pageSize)
    {
        string url = CloudbedsUris.UriGenerate_GetCurrentGuestsList(
            _cbServerInfo, pageNumber, pageSize);

        //Request the data from server
        this.StatusLog.AddStatus("Querying guests, page: " + pageNumber.ToString());
        this.StatusLog.AddStatus("Custom web request: " + url, -10);

        //========================================================================
        //Send the request
        //========================================================================
        var httpRequest = CreateHttpRequest_Get(url);

        //========================================================================
        //Get the response
        //========================================================================
        var response = GetWebResponseLogErrors(httpRequest, "request hotel checked in guests list");

        //https://docs.microsoft.com/en-us/dotnet/standard/serialization/system-text-json-use-dom-utf8jsonreader-utf8jsonwriter?pivots=dotnet-6-0
        using (response)
        {
            var jsonOut = GetWebResponseAsJson(response);
            _commandResultJson = jsonOut;

            //----------------------------------------------------------------
            //If the 'data' node is missing, specifically record this error
            //because it indicates we did not get the expected result back 
            //from the server
            //----------------------------------------------------------------
            JsonElement jsonResult_dataNode;
            if (!jsonOut.RootElement.TryGetProperty("data", out jsonResult_dataNode))
            {
                throw new Exception("221020-638: No Json 'data' node found");
                //this.StatusLog.AddError("221020-638: No Json 'data' node found");
                //return false;
            }

            return ExecuteRequest_ParseGuestsFromResponse(jsonResult_dataNode);
        }
    }


    /// <summary>
    /// Parses the array of results
    /// </summary>
    /// <param name="jsonResult_dataNode"></param>
    /// <returns></returns>
    private List<CloudbedsGuest> ExecuteRequest_ParseGuestsFromResponse(JsonElement jsonResult_dataNode)
    {
        var listOut = new List<CloudbedsGuest>();
        if(jsonResult_dataNode.ValueKind != JsonValueKind.Array)
        {
            throw new Exception("1021-906: Expected Json Array");
        }
        //var jsonArrayEnumerate = jsonResult_dataNode.EnumerateArray();

        
        foreach(var jsonSingleGuest in jsonResult_dataNode.EnumerateArray())
        {
            CloudbedsGuest thisGuest = ExecuteRequest_ParseGuestsFromResponse_SingleGuest(jsonSingleGuest);
            if(thisGuest != null)
            {
                listOut.Add(thisGuest);
            }
            else //Unxpected -- we did not parse a guest?
            {
                this.StatusLog.AddError("1021-913: NULL guest parse");
            }
        }
        
        return listOut;
    }

    private CloudbedsGuest ExecuteRequest_ParseGuestsFromResponse_SingleGuest(JsonElement jsonSingleGuest)
    {
        try
        {
            return ExecuteRequest_ParseGuestsFromResponse_SingleGuest_inner(jsonSingleGuest);
        }
        catch(Exception ex)
        {
            this.StatusLog.AddError("1021-914: Error parsing guest: " + ex.Message);
            return null;
        }
    }

    private CloudbedsGuest ExecuteRequest_ParseGuestsFromResponse_SingleGuest_inner(JsonElement jsonSingleGuest)
    {
        string guestId =
            JsonParseHelpers.FindJasonAttributeValue_String(jsonSingleGuest, "guestID");
        string guestName = 
            JsonParseHelpers.FindJasonAttributeValue_String(jsonSingleGuest, "guestName");
        string guestEmail =
            JsonParseHelpers.FindJasonAttributeValue_String(jsonSingleGuest, "guestEmail");
        string guestCellPhone =
            JsonParseHelpers.FindJasonAttributeValue_String(jsonSingleGuest, "guestCellPhone");
        string reservationId =
            JsonParseHelpers.FindJasonAttributeValue_String(jsonSingleGuest, "reservationID");
        string reservationStartDate_text=
            JsonParseHelpers.FindJasonAttributeValue_String(jsonSingleGuest, "startDate");
        string reservationEndDate_text =
            JsonParseHelpers.FindJasonAttributeValue_String(jsonSingleGuest, "endDate");
        string roomId =
            JsonParseHelpers.FindJasonAttributeValue_String(jsonSingleGuest, "roomID");
        string roomName =
            JsonParseHelpers.FindJasonAttributeValue_String(jsonSingleGuest, "roomName");


        return new CloudbedsGuest(guestId, guestName, guestEmail, guestCellPhone, reservationId, reservationStartDate_text, reservationEndDate_text, roomId, roomName);
    }
}
