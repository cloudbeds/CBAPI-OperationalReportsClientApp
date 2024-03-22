using System;
using System.Xml;

/// <summary>
/// Encapsulation of a Cloudbeds session, backed by OAuth
/// </summary>
class CloudbedsAuthSession_OAuth : ICloudbedsAuthSessionBase
{
    const int ExpireSecondsBuffer = 180; //3 minutes
    readonly DateTime _renewTokensAfter_utc;
    readonly OAuth_RefreshToken _oauthRefreshToken;
    readonly OAuth_AccessToken _oauthAccessToken;
    readonly TaskStatusLogs _statusLogs;

    public OAuth_RefreshToken OAuthRefreshToken
    {
        get { return _oauthRefreshToken; }
    }

    public OAuth_AccessToken OAuthAccessToken
    {
        get { return _oauthAccessToken; }
    }

    public DateTime RenewAfterDateTime
    {
        get { return _renewTokensAfter_utc; }
    }

    /// <summary>
    /// CONSTRUCTOR
    /// </summary>
    /// <param name="oauthRefreshToken"></param>
    /// <param name="oauthAccessToken"></param>
    /// <param name="renewalRequired"></param>
    public CloudbedsAuthSession_OAuth(
        OAuth_RefreshToken oauthRefreshToken,
        OAuth_AccessToken oauthAccessToken,
        DateTime renewalRequired,
        TaskStatusLogs statusLogs)
    {
        _renewTokensAfter_utc = renewalRequired;
        _oauthAccessToken = oauthAccessToken;
        _oauthRefreshToken = oauthRefreshToken;

        _statusLogs = statusLogs;

    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="oauthRefreshToken"></param>
    /// <param name="oauthAccessToken"></param>
    /// <param name="numberSecondsUntilRefreshNeeded"></param>
    public CloudbedsAuthSession_OAuth(
        OAuth_RefreshToken oauthRefreshToken,
        OAuth_AccessToken oauthAccessToken,
        int numberSecondsUntilRefreshNeeded,
        TaskStatusLogs statusLogs)
    {

        //Sanity check
        if(numberSecondsUntilRefreshNeeded <= 0)
        {
            IwsDiagnostics.Assert(numberSecondsUntilRefreshNeeded > 0, "0723-752: Seconds to renew invalid");
            throw new ArgumentException("0723-752: Seconds to renew invalid");
        }

        //-------------------------------------------------------------
        //Buffer the renewal time
        //-------------------------------------------------------------
        numberSecondsUntilRefreshNeeded = numberSecondsUntilRefreshNeeded - ExpireSecondsBuffer;
        if(numberSecondsUntilRefreshNeeded < 0)
        {
            numberSecondsUntilRefreshNeeded = 0;
            statusLogs.AddError("0723-752: Warning. Refresh seconds very close or past expiration");
        }

        _renewTokensAfter_utc = DateTime.UtcNow.AddSeconds(0 + numberSecondsUntilRefreshNeeded);
        _oauthAccessToken = oauthAccessToken;
        _oauthRefreshToken = oauthRefreshToken;

        _statusLogs = statusLogs;
    }

    /// <summary>
    /// Gives us an authenticated session ID we can use in REST API requests
    /// </summary>
    string ICloudbedsAuthSessionId.AuthSessionsId
    {
        get
        {
            var nowUtc = DateTime.UtcNow;
            //Warn if the token is expiring
            if (nowUtc >= _renewTokensAfter_utc)
            {
                _statusLogs.AddError("0723-804: WARNING! OAuth token needs to be renewed. " + nowUtc.ToString() + ">=" + _renewTokensAfter_utc);
            }

            return _oauthAccessToken.TokenValue;
        }
    }

    /// <summary>
    /// TRUE of the authentication token is past its need-to-renew-by date/time
    /// </summary>
    public bool IsAuthTokenPastRenewalDateTime {
        get
        {
            return DateTime.UtcNow >= _renewTokensAfter_utc;
        }
    }

    /// <summary>
    /// Write the secret out in XML
    /// </summary>
    /// <param name="xmlWriter"></param>
    void ICloudbedsTransientSecretStorageInfo.WriteAsXml(XmlWriter xmlWriter)
    {
        xmlWriter.WriteStartElement("CBOauthToken");
        xmlWriter.WriteAttributeString("oauthRefreshToken", _oauthRefreshToken.TokenValue);
        xmlWriter.WriteAttributeString("oauthAccessToken", _oauthAccessToken.TokenValue);
        xmlWriter.WriteAttributeString("expectedExpireTimeUtc", _renewTokensAfter_utc.ToString(System.Globalization.CultureInfo.InvariantCulture));
        xmlWriter.WriteEndElement();
    }
    public static CloudbedsAuthSession_OAuth FromXmlFile(string filePath, TaskStatusLogs statusLog)
    {
        if (!System.IO.File.Exists(filePath))
        {
            throw new Exception("22724-1209: File does not exist: " + filePath);
        }
        //==================================================================================
        //Load the XML
        //==================================================================================
        var xmlDoc = new System.Xml.XmlDocument();
        xmlDoc.Load(filePath);

        return FromXml(xmlDoc, statusLog);
    }

    public static CloudbedsAuthSession_OAuth FromXml(XmlDocument xDoc, TaskStatusLogs statusLog)
    {

        //==================================================================================
        //Load values 
        //==================================================================================
        var xNode = xDoc.SelectSingleNode("//CBOauthToken");
        string oauthRefreshToken = xNode.Attributes["oauthRefreshToken"].Value;
        string oauthAccessToken = xNode.Attributes["oauthAccessToken"].Value;
        string expireTime_text = xNode.Attributes["expectedExpireTimeUtc"].Value;

        DateTime expireTimeParsed = DateTime.Parse(
            expireTime_text,
            System.Globalization.CultureInfo.InvariantCulture);

        DateTime expireTimeUtc = DateTime.SpecifyKind(expireTimeParsed, DateTimeKind.Utc);


        return new CloudbedsAuthSession_OAuth(
            new OAuth_RefreshToken(oauthRefreshToken),
            new OAuth_AccessToken(oauthAccessToken),
            expireTimeUtc,
            statusLog);

    }

    /// <summary>
    /// Status text to help with debugging
    /// </summary>
    /// <returns></returns>
    string ICloudbedsAuthSessionId.DebugStatusText()
    {
        var sb = new System.Text.StringBuilder();
        var oauthRefreshToken = _oauthRefreshToken;
        var oauthAccessToken = _oauthAccessToken;

        //=================================================
        sb.Append("OAuth ACCESS  Token: ");
        if(oauthAccessToken == null)
        {
            sb.Append("NONE");
        }
        else
        {
            sb.Append(DebugTextForToken(oauthAccessToken.TokenValue));
        }

        //=================================================
        sb.AppendLine();
        sb.Append("OAuth REFRESH Token: ");
        if (oauthRefreshToken == null)
        {
            sb.Append("NONE");
        }
        else
        {
            sb.Append(DebugTextForToken(oauthRefreshToken.TokenValue));
        }

        //=================================================
        sb.AppendLine();
        sb.Append("Tokens renewal deadline (utc) ");
        sb.Append(_renewTokensAfter_utc.ToString());

        var nowUtc = DateTime.UtcNow;
        var expireDelta = _renewTokensAfter_utc - nowUtc;
        sb.Append(" (");
        sb.Append(((int)expireDelta.TotalMinutes).ToString());
        sb.Append(" minutes)");

        return sb.ToString();
    }

    /// <summary>
    /// Simple debug text for a token
    /// </summary>
    /// <param name="tokenText"></param>
    /// <returns></returns>
    private string DebugTextForToken(string tokenText)
    {
        if(string.IsNullOrWhiteSpace(tokenText))
        {
            return "BLANK";
        }

        //Degenerate case...
        if(tokenText.Length < 10)
        {
            return "SHORT";
        }

        return tokenText.Substring(0, 5)
            + "..." + (tokenText.Length - 10) + "..."
            + tokenText.Substring(tokenText.Length - 6, 5);
    }

}
