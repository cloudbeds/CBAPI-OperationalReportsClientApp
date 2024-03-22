using System;
using System.Xml;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Text.Json;

/// <summary>
/// A request to get an an OAuth REFRESH token (used to replace the old one)
/// </summary>
class CloudbedsRequestOAuthRefreshToken : CloudbedsRequestBase
{
    /// <summary>
    /// URL manager
    /// </summary>
    private readonly CloudbedsAppConfig _cbConfig;
    private readonly OAuth_RefreshToken _oldOauthRefreshToken;

    private JsonDocument _commandResultJson;

    CloudbedsAuthSession_OAuth _cloudbedsOAuthSession;
    public CloudbedsAuthSession_OAuth CommandResult_CloudbedsAuthSession
    {
        get
        {
            return _cloudbedsOAuthSession;
        }
    }

    /*   
       private OAuth_AccessToken _commandResult_accessToken = null;
       private OAuth_RefreshToken _commandResult_refreshToken = null;
   */
    private string _commandResult_tokenType = null;

    private int? _commandResult_ExpiresSeconds = null;

    public OAuth_AccessToken CommandResult_AccessToken
    { get { return _cloudbedsOAuthSession.OAuthAccessToken; } }

    public OAuth_RefreshToken CommandResult_RefreshToken
    { get { return _cloudbedsOAuthSession.OAuthRefreshToken; } }



    public string CommandResult_TokenType
    { get { return _commandResult_tokenType; } }

    public int CommandResult_ExpiresSeconds
    { get { return _commandResult_ExpiresSeconds.Value; } }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="cbConfig"></param>
    /// <param name="oauthRefreshToken"></param>
    /// <param name="statusLog"></param>
    public CloudbedsRequestOAuthRefreshToken(CloudbedsAppConfig cbConfig, OAuth_RefreshToken oauthRefreshToken, TaskStatusLogs statusLog)
        : base(statusLog)
    {
        _cbConfig = cbConfig;
        _oldOauthRefreshToken = oauthRefreshToken;
    }

    /// <summary>
    /// 
    /// https://hotels.cloudbeds.com/api/docs/#api-Authentication-token
    /// </summary>
    /// <param name="serverName"></param>
    public void ExecuteRequest()
    {
        string url = CloudbedsUris.UriGenerate_RequestOAuthRefreshToken(
            _cbConfig);

        string postContents = CloudbedsUris.UriGenerate_RequestOAuthRefreshToken_PostContents(
            _cbConfig, _oldOauthRefreshToken);


        //Request the data from server
        this.StatusLog.AddStatus("Custom web request: " + url, -10);

        //========================================================================
        //Send the request
        //========================================================================
        var httpRequest = CreateHttpRequest_Post_WithUriEncodedForm(url, postContents);

        //========================================================================
        //Get the response
        //========================================================================
        var response = GetWebResponseLogErrors(httpRequest, "request OAuth Access/Refresh Token Replacement");

        JsonDocument jsonOut = null;
        using (response)
        {
            if(!response.IsSuccessStatusCode)
            {
                this.StatusLog.AddError("320-922: API Refresh token response error. Status code: " + response.StatusCode.ToString());
                return;
            }

            jsonOut = GetWebResponseAsJson(response);
        }
        _commandResultJson = jsonOut;

        OAuth_AccessToken commandResult_accessToken = null;
        OAuth_RefreshToken commandResult_refreshToken = null;

        string oauthAccessTokenText = JsonParseHelpers.FindJasonAttributeValue_String(
                jsonOut.RootElement, "access_token");
        if (!string.IsNullOrWhiteSpace(oauthAccessTokenText))
        {
            commandResult_accessToken = new OAuth_AccessToken(oauthAccessTokenText);
        }

        string commandResult_tokenType =
        JsonParseHelpers.FindJasonAttributeValue_String(
            jsonOut.RootElement, "token_type");


        string oauthRefreshTokenText = JsonParseHelpers.FindJasonAttributeValue_String(
                jsonOut.RootElement, "refresh_token");
        if (!string.IsNullOrWhiteSpace(oauthRefreshTokenText))
        {
            commandResult_refreshToken = new OAuth_RefreshToken(oauthRefreshTokenText);
        }

        int commandResult_ExpiresSeconds =
            JsonParseHelpers.FindJasonAttributeValue_IntegerOrNull(
                jsonOut.RootElement, "expires_in").Value;

        //Wrap it in an authentication session
        _cloudbedsOAuthSession = new CloudbedsAuthSession_OAuth(
            commandResult_refreshToken,
            commandResult_accessToken,
            commandResult_ExpiresSeconds,
            this.StatusLog);

        //Store this other response data 
        _commandResult_ExpiresSeconds = commandResult_ExpiresSeconds;
        _commandResult_tokenType = commandResult_tokenType;

    }
}
