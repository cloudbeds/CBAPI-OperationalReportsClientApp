using System;
using System.Xml;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Text.Json;

/// <summary>
/// A request to get the dashboard data from Cloudbeds
/// </summary>
class CloudbedsRequestHotelDetails: CloudbedsAuthenticatedRequestBase
{
    private readonly ICloudbedsServerInfo _cbServerInfo;
    private JsonDocument _commandResultJson;
    //private int? jsonResult_roomsOccupied;
    //private int? jsonResult_percentageOccupied;
    //private int? jsonResult_arrivals;
    //private int? jsonResult_departures;
    //private int? jsonResult_inHouse;
    private string jsonResult_propertyId;
    private string jsonResult_propertyName;

    public string PropertyId
    {
        get
        {
            return jsonResult_propertyId;
        }
    }
    public string PropertyName
    {
        get
        {
            return jsonResult_propertyName;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="cbServerInfo"></param>
    /// <param name="oauthRefreshToken"></param>
    /// <param name="statusLog"></param>
    public CloudbedsRequestHotelDetails(ICloudbedsServerInfo cbServerInfo, ICloudbedsAuthSessionId authSession, TaskStatusLogs statusLog)
        : base(authSession, statusLog)
    {
        _cbServerInfo = cbServerInfo;
    }

    public JsonDocument CommandResults_Json
    {
        get
        {
            return _commandResultJson;
        }
    }

    /*
    /// <summary>
    /// Text summing up the return values
    /// </summary>
    public string CommandResults_SummaryText
    {
        get
        {
            var sb = new StringBuilder();
            helper_AppendValuePairText(sb, "propertyId", jsonResult_propertyId);
            helper_AppendValuePairText(sb, "propertyName", jsonResult_propertyName);
            return sb.ToString();
        }
    }

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
        if (propertyValue == null)
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
    /// https://hotels.cloudbeds.com/api/docs/#api-Hotel-getHotelDetails
    /// </summary>
    /// <param name="serverName"></param>
    public bool ExecuteRequest()
    {
        string url = CloudbedsUris.UriGenerate_RequestHotelDetails(
            _cbServerInfo);

        //string postContents = "";
        //Request the data from server
        this.StatusLog.AddStatus("Custom web request: " + url, -10);

        //========================================================================
        //Send the request
        //========================================================================
        var httpRequest = CreateHttpRequest_Get(url);

        //========================================================================
        //Get the response
        //========================================================================
        var response = GetWebResponseLogErrors(httpRequest, "request hotel details");

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
                this.StatusLog.AddError("220725-611: No Json 'data' node found");
                return false;
            }

            jsonResult_propertyId= JsonParseHelpers.FindJasonAttributeValue_String(jsonResult_dataNode, "propertyID");
            jsonResult_propertyName = JsonParseHelpers.FindJasonAttributeValue_String(jsonResult_dataNode, "propertyName");
            return true;
        }
    }
}
