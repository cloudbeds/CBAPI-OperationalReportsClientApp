using System;
using System.Xml;

/// <summary>
/// Encapsulation of a Cloudbeds session, backed by an API access token
/// </summary>
class CloudbedsAuthSession_ApiToken : ICloudbedsAuthSessionBase
{
    readonly TaskStatusLogs _statusLogs;
    private string _accessSecret;

    /// <summary>
    /// CONSTRUCTOR
    /// </summary>
    /// <param name="oauthRefreshToken"></param>
    /// <param name="oauthAccessToken"></param>
    /// <param name="renewalRequired"></param>
    public CloudbedsAuthSession_ApiToken(
        string accessSecret,
        TaskStatusLogs statusLogs)
    {
        _accessSecret = accessSecret;
        _statusLogs = statusLogs;
    }
    
    /// <summary>
    /// Gives us an authenticated session ID we can use in REST API requests
    /// </summary>
    string ICloudbedsAuthSessionId.AuthSessionsId
    {
        get
        {
            return _accessSecret;
        }
    }

    /// <summary>
    /// Write the secret out in XML
    /// </summary>
    /// <param name="xmlWriter"></param>
    void ICloudbedsTransientSecretStorageInfo.WriteAsXml(XmlWriter xmlWriter)
    {
        _statusLogs.AddStatus("240322-224: No transient secrets to write to local storage.  Doing nothing.");
    }
    /// <summary>
    /// Status text to help with debugging
    /// </summary>
    /// <returns></returns>
    string ICloudbedsAuthSessionId.DebugStatusText()
    {
        //Nothing much to write here...
        return "CloudbedsAuthSession_ApiToken: No status needed";
    }

    /// <summary>
    /// The type of authentication we are performing
    /// </summary>
    CloudbedsAppAuthenticationType ICloudbedsAuthSessionId.AuthenticationType
    {
        get
        {
            return CloudbedsAppAuthenticationType.ApiAccessKey;
        }
    }

}
