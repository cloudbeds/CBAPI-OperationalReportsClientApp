
using System;
using System.Xml;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Text.Json;

/// <summary>
/// A request to get the dashboard data from Cloudbeds
/// </summary>
class CloudbedsRequestAuthUserData : CloudbedsAuthenticatedRequestBase
{
    private readonly ICloudbedsServerInfo _cbServerInfo;
    private JsonDocument _commandResultJson;
    private int? _commandResult_userId;
    private string _commandResult_userEmail;


    public string CommandResult_UserEmail
    {
        get { return _commandResult_userEmail; }
    }

    public int CommandResult_UserId
    {
        get { return _commandResult_userId.Value; }
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="cbConfig"></param>
    /// <param name="oauthRefreshToken"></param>
    /// <param name="statusLog"></param>
    public CloudbedsRequestAuthUserData(ICloudbedsServerInfo cbServerInfo, ICloudbedsAuthSessionId authSession, TaskStatusLogs statusLog)
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
    
    /// <summary>
    /// 
    /// https://hotels.cloudbeds.com/api/docs/#api-Dashboard-getDashboard
    /// </summary>
    /// <param name="serverName"></param>
    public void ExecuteRequest()
    {
        string url = CloudbedsUris.UriGenerate_RequestAuthUserInfo(
            _cbServerInfo);

        //Request the data from server
        this.StatusLog.AddStatus("Web request: " + url, -10);

        //========================================================================
        //Send the request
        //========================================================================
        var httpRequest = CreateHttpRequest_Get(url);

        //========================================================================
        //Get the response
        //========================================================================
        var response = GetWebResponseLogErrors(httpRequest, "request auth user info");

        JsonDocument jsonOut = null;
        using (response)
        {
            jsonOut = GetWebResponseAsJson(response);
        }

        _commandResult_userId  = JsonParseHelpers.FindJasonAttributeValue_IntegerOrNull(
             jsonOut.RootElement, "user_id").Value;

        _commandResult_userEmail = JsonParseHelpers.FindJasonAttributeValue_String(
             jsonOut.RootElement, "email");


        _commandResultJson = jsonOut;

    }
}
