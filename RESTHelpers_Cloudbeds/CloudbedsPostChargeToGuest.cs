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
class CloudbedsPostChargeToGuest : CloudbedsAuthenticatedRequestBase
{
    private readonly ICloudbedsServerInfo _cbServerInfo;
    private readonly CloudbedsGuest _guestToCharge;
    PosOrderManager _posOrderManager;
    private JsonDocument? _commandResultJson = null;
    private string _commandResult_SoldProductId = null;
    private string _commandResult_TransactionId = null;
    private string _commandResult_Notice = null;


    /// <summary>
    /// 
    /// </summary>
    /// <param name="cbServerInfo"></param>
    /// <param name="oauthRefreshToken"></param>
    /// <param name="statusLog"></param>
    public CloudbedsPostChargeToGuest(
        ICloudbedsServerInfo cbServerInfo,
        ICloudbedsAuthSessionId authSession,
        TaskStatusLogs statusLog,
        CloudbedsGuest guest,
        PosOrderManager posOrderManager
        )
        : base(authSession, statusLog)
    {
        _cbServerInfo = cbServerInfo;
        IwsDiagnostics.Assert(guest != null, "1023-1146, guest is null");
        IwsDiagnostics.Assert(posOrderManager != null, "1023-230, order manager is null");

        _guestToCharge = guest;
        _posOrderManager = posOrderManager;
    }


    public JsonDocument? CommandResults_Json
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
            if(_commandResultJson == null)
            {
                return "No results";
            }

            var sb = new StringBuilder();

            helper_appendIfNotNull(sb, "Transaction ID", _commandResult_TransactionId);
            sb.AppendLine();

            helper_appendIfNotNull(sb, "Product ID", _commandResult_SoldProductId);
            sb.AppendLine();

            helper_appendIfNotNull(sb, "Notes", _commandResult_Notice);
            sb.AppendLine();

            return sb.ToString();
        }
    }

    /// <summary>
    /// Simple text append
    /// </summary>
    /// <param name="sb"></param>
    /// <param name="prefixText"></param>
    /// <param name="textToAppend"></param>
    private static void helper_appendIfNotNull(StringBuilder sb, string prefixText, string textToAppend)
    {
        if(textToAppend == null)
        {
            sb.Append(prefixText);
            sb.Append(": none");
            return;
        }

        sb.Append(prefixText);
        sb.Append(": ");
        sb.Append(textToAppend);
    }


    public string CommandResults_Notice
    {
        get
        {
            return _commandResult_Notice;
        }
    }

    public string CommandResults_ProductId
    {
        get
        {
            return _commandResult_SoldProductId;
        }
    }

    public string CommandResults_TransactionId
    {
        get
        {
            return _commandResult_TransactionId;
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
            this.StatusLog.AddError("1023-1148: Error posting adjustment: " + ex.Message);
            return false;
        }
    }

    /// <summary>
    ///Post the request to Cloudbeds
    ///https://hotels.cloudbeds.com/api/docs/#api-Item-postCustomItem
    /// </summary>
    /// <returns></returns>
    private bool ExecuteRequest_inner()
    {
        string url = CloudbedsUris.UriGenerate_PostCustomItemToReservation(
            this._cbServerInfo);

        string postContents = CloudbedsUris.UriGenerate_PostCustomItemToReservation_PostContents(
            _guestToCharge, _posOrderManager);

        //Request the data from server
        this.StatusLog.AddStatus("Custom web request (1023-1210): " + url, -10);

        //========================================================================
        //Send the request
        //========================================================================
        var httpRequest = CreateHttpRequest_Post_WithUriEncodedForm(url, postContents);

        //========================================================================
        //Get the response
        //========================================================================
        var response = GetWebResponseLogErrors(httpRequest, "request hotel checked in guests list");

        //https://docs.microsoft.com/en-us/dotnet/standard/serialization/system-text-json-use-dom-utf8jsonreader-utf8jsonwriter?pivots=dotnet-6-0
        using (response)
        {
            var jsonOut = GetWebResponseAsJson(response);
            _commandResultJson = jsonOut;


            //Check the SUCCESS node explicitly for TRUE
            ValidateResponseForSuccessFlag(jsonOut);

            //----------------------------------------------------------------
            //If the 'data' node is missing, specifically record this error
            //because it indicates we did not get the expected result back 
            //from the server
            //----------------------------------------------------------------
            JsonElement jsonResult_dataNode;
            if (!jsonOut.RootElement.TryGetProperty("data", out jsonResult_dataNode))
            {
                throw new Exception("1023-1211: No Json 'data' node found");
            }

            _commandResult_SoldProductId = 
                JsonParseHelpers.FindJasonAttributeValue_String(jsonResult_dataNode, "soldProductID");

            _commandResult_TransactionId =
                JsonParseHelpers.FindJasonAttributeValue_String(jsonResult_dataNode, "transactionID");

            _commandResult_Notice =
                JsonParseHelpers.FindJasonAttributeValue_String(jsonResult_dataNode, "notice");

            return true; //Success
        }
    }

}
