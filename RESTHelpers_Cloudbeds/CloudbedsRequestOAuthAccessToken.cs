using System;
using System.Xml;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Text.Json;

/// <summary>
/// A request to get an an OAuth Access Token
/// </summary>
class CloudbedsRequestOAuthAccessToken : CloudbedsRequestBase
{
    private readonly CloudbedsAppConfig _cbConfig;
    private readonly OAuth_BootstrapCode _oAuthSecretCode;

    //private string _commandResult;
    private JsonDocument _commandResultJson;

    CloudbedsAuthSession_OAuth _cloudbedsOAuthSession;
    public CloudbedsAuthSession_OAuth CommandResult_CloudbedsAuthSession 
    {
        get 
        {
            return _cloudbedsOAuthSession;
        }
    }

    private string _commandResult_tokenType = null;
    
    private int? _commandResult_ExpiresSeconds = null;
    
    public OAuth_AccessToken CommandResult_AccessToken
    { get { return _cloudbedsOAuthSession.OAuthAccessToken; } }

    public string CommandResult_TokenType
    { get { return _commandResult_tokenType; } }

    public OAuth_RefreshToken CommandResult_RefreshToken
    { get { return _cloudbedsOAuthSession.OAuthRefreshToken; } }

    public int CommandResult_ExpiresSeconds
    { get { return _commandResult_ExpiresSeconds.Value; } }
    

    public CloudbedsRequestOAuthAccessToken(CloudbedsAppConfig cbConfig, OAuth_BootstrapCode oauthSecretCode, TaskStatusLogs statusLog)
        : base(statusLog)
    {
        _cbConfig = cbConfig;
        _oAuthSecretCode =  oauthSecretCode;
    }

    /// <summary>
    /// 
    /// https://hotels.cloudbeds.com/api/docs/#api-Authentication-token
    /// </summary>
    /// <param name="serverName"></param>
    public void ExecuteRequest()
    {
        string url = CloudbedsUris.UriGenerate_RequestOAuthAccessToken(
            _cbConfig);

        string postContents = CloudbedsUris.UriGenerate_RequestOAuthAccessToken_PostContents(
            _cbConfig, _oAuthSecretCode);


        //Request the data from server
        this.StatusLog.AddStatus("Custom web request: " + url, -10);

        //========================================================================
        //Send the request
        //========================================================================
        //var webRequest = CreateWebRequest(url, "POST");
        var httpRequest = CreateHttpRequest_Post_WithUriEncodedForm(url, postContents);


        //========================================================================
        //Get the response
        //========================================================================
        var response = GetWebResponseLogErrors(httpRequest, "request OAuth Access/Refresh Token Boostrap");

        using (response)
        {
            var jsonOut = GetWebResponseAsJson(response);

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
}
