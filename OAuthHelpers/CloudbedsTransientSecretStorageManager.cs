using System;


/// <summary>
/// Abstraction to strongly type RefreshTokens secret
/// </summary>
class CloudbedsTransientSecretStorageManager
{
    private string _filePath_persistSecret = null;
    ICloudbedsAuthSessionBase _authSession;

    /// <summary>
    /// The authentication token
    /// </summary>
    public ICloudbedsAuthSessionBase AuthSession
    {
        get
        {
            return _authSession;
        }
    }


    public CloudbedsTransientSecretStorageManager(ICloudbedsAuthSessionBase authSession, string fileStoragePath)
    {
        _authSession = authSession;
        _filePath_persistSecret = fileStoragePath;
    }

    /// <summary>
    /// Write the secret to storage
    /// </summary>
    /// <exception cref="Exception"></exception>
    public void PersistSecretsToStorage()
    {
        var filePath = _filePath_persistSecret;
        if(string.IsNullOrWhiteSpace(filePath))
        {
            throw new Exception("220723-344: Missing file path");
        }

        var xmlWriter = System.Xml.XmlWriter.Create(_filePath_persistSecret);
        _authSession.WriteAsXml(xmlWriter);
        xmlWriter.Close();
    }

    /// <summary>
    /// Create an initialized secret storage manager by loading the content from a file
    /// </summary>
    /// <param name="filePathToPersistedToken"></param>
    /// <param name="appConfigAndSecrets"></param>
    /// <param name="refreshTokenIfExpired"></param>
    /// <param name="statusLogs"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    internal static CloudbedsTransientSecretStorageManager LoadAuthTokensFromFile(string filePathToPersistedToken, CloudbedsAppConfig appConfigAndSecrets, bool refreshTokenIfExpired, TaskStatusLogs statusLogs)
    {
        var authToken = CloudbedsAuthSession_OAuth.FromXmlFile(filePathToPersistedToken, statusLogs);

        //-----------------------------------------------------------------------
        //The token is up to date and usable...
        //-----------------------------------------------------------------------
        if (!authToken.IsAuthTokenPastRenewalDateTime)
        {
            statusLogs.AddStatus("Auth token loaded from storage, and NOT expired");
            return new CloudbedsTransientSecretStorageManager(authToken, filePathToPersistedToken);
        }

        //-----------------------------------------------------------------------
        //If we are not refreshing the expired token, return it with a warning
        //-----------------------------------------------------------------------
        if (!refreshTokenIfExpired)
        {
            statusLogs.AddStatus("WARNING! 220725-220: Auth token loaded from storage past renewal date/time ("
                + DateTime.UtcNow.ToString()
                + ">="
                + authToken.RenewAfterDateTime.ToString()
                + ")"
                );
            return new CloudbedsTransientSecretStorageManager(authToken, filePathToPersistedToken);
        }

        //-----------------------------------------------------------------------
        //Attempt to rewnew the OAuth token
        //-----------------------------------------------------------------------
        statusLogs.AddStatus("220725-221: Auth token past renewal date/time ("
            + DateTime.UtcNow.ToString()
            + ">="
            + authToken.RenewAfterDateTime.ToString()
            + "), attempting token refresh"
            );


        statusLogs.AddStatusHeader("Request NEW Access and Refresh tokens (using old refresh token) ");

        var cbRequestAccessToken = new CloudbedsRequestOAuthRefreshToken(
            appConfigAndSecrets,
            authToken.OAuthRefreshToken,
            statusLogs);
        cbRequestAccessToken.ExecuteRequest();

        var refreshedAuthSession = cbRequestAccessToken.CommandResult_CloudbedsAuthSession;
        //This would be unexpected
        if (refreshedAuthSession == null)
        {
            throw new Exception("220725-158: No refreshed auth-session returned");
        }
        statusLogs.AddStatus("Updated auth tokens received (new refresh date/time: " + refreshedAuthSession.RenewAfterDateTime.ToString() + ")");

        //--------------------------------------------------------------------
        //Persist the updated refresh token into storage (replacing the older/expired token)
        //--------------------------------------------------------------------
        statusLogs.AddStatus("Persist updated auth tokens into storage");
        var secretStorageManager = new CloudbedsTransientSecretStorageManager(
            refreshedAuthSession,
            filePathToPersistedToken);
        secretStorageManager.PersistSecretsToStorage();

        statusLogs.AddStatus("Updated auth tokens persisted");
        return secretStorageManager;
    }
}
