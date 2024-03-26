using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;


/// <summary>
/// Config For Cloudbeds Application API access
/// </summary>
internal partial class CloudbedsAppConfig : ICloudbedsServerInfo
{
    //ID/Password to sign into the Online Site
    public readonly string CloudbedsServerUrl;
    public readonly string CloudbedsAppOAuthRedirectUri;
    public readonly string CloudbedsAppClientId;
    public readonly string CloudbedsAppClientSecret;
    public readonly CloudbedsAppAuthenticationType AuthenticationType;
    public readonly string CloudbedsPropertyIdOrNull; //May be NULL

    /// <summary>
    /// Parse the authentication type value
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public static CloudbedsAppAuthenticationType Parse_AppAuthenticationType(string value)
    {

        if(value == "OAuthToken")
        {
            return CloudbedsAppAuthenticationType.OAuthToken;
        }

        if (value == "ApiAccessKey")
        {
            return CloudbedsAppAuthenticationType.ApiAccessKey;
        }

        if(value == null)
        {
            value = "";
        }
        throw new Exception("240322-127: Unknown App Authentication type: " + value);
    }


    /// <summary>
    /// Create a testing version of the class with fake data
    /// </summary>
    /// <returns></returns>
    internal static CloudbedsAppConfig TESTING_CreateSimulatedAppConfig()
    {
        return new CloudbedsAppConfig(
            "FAKE Server URL",
            "FAKE CLIENT ID",
            CloudbedsAppAuthenticationType.SimulationModeNoAuth,
            "FAKE CLIENT SECRET",
            "FAKE OAUTH Redirect Uri");
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="serverUrl"></param>
    /// <param name="clientId"></param>
    /// <param name="clientSecret"></param>
    /// <param name="authRedirectUri"></param>
    private CloudbedsAppConfig(string serverUrl, string clientId, CloudbedsAppAuthenticationType authType, string clientSecret = "", string authRedirectUri = "", string propertyId = null)
    {
        this.CloudbedsServerUrl = serverUrl;
        this.AuthenticationType = authType;
        this.CloudbedsAppClientId = clientId;
        this.CloudbedsAppClientSecret = clientSecret;
        this.CloudbedsAppOAuthRedirectUri = authRedirectUri;

        //Cannonicalize
        if(string.IsNullOrWhiteSpace(propertyId))
        {
            propertyId = null;
        }

        this.CloudbedsPropertyIdOrNull = propertyId;

    }

    /// <summary>
    /// Create the config
    /// </summary>
    /// <param name="filePathConfig"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public static CloudbedsAppConfig FromFile(string filePathConfig)
    {
        if (!System.IO.File.Exists(filePathConfig))
        {
            throw new Exception("722-153: File does not exist: " + filePathConfig);
        }

        //==================================================================================
        //Load values from the TARGET SITE config file
        //==================================================================================
        var xmlConfigTargetProperty = new System.Xml.XmlDocument();
        xmlConfigTargetProperty.Load(filePathConfig);

        return FromXmlDocument(xmlConfigTargetProperty);
    }

    /// <summary>
    /// Create the config
    /// </summary>
    /// <param name="filePathConfig"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public static CloudbedsAppConfig FromXmlDocument(XmlDocument xmlConfigTargetProperty)
    {
        var xNode = xmlConfigTargetProperty.SelectSingleNode("//Configuration/CloudbedsApp");
        string cloudbedsServerUrl = xNode.Attributes["serverUrl"].Value;
        string cloudbedsAppClientId = xNode.Attributes["clientId"].Value;
        string cloudbedsAppClientSecret = xNode.Attributes["secret"].Value;

        string authModeText = xNode.Attributes["authMode"].Value;
        var parsedAuthMode = CloudbedsAppConfig.Parse_AppAuthenticationType(authModeText);

        string propertyIdOrNull = XmlHelper.SafeParseXmlAttribute(xNode, "propertyId", null);

        //We only need a redirect URL attribute if it is an OAuth authentication key
        string cloudbedsAppOAuthRedirectUri = "";
        if (parsedAuthMode == CloudbedsAppAuthenticationType.OAuthToken)
        {
            cloudbedsAppOAuthRedirectUri = xNode.Attributes["oAuthRedirectUri"].Value;
        }


        return new CloudbedsAppConfig(
            cloudbedsServerUrl,
            cloudbedsAppClientId,
            parsedAuthMode,
            cloudbedsAppClientSecret,
            cloudbedsAppOAuthRedirectUri,
            propertyIdOrNull);
    }


    string ICloudbedsServerInfo.ServerUrl
    {
        get { return this.CloudbedsServerUrl; } 
    }

    string ICloudbedsServerInfo.PropertyIdOrNull
    {
        get
        {
            return this.CloudbedsPropertyIdOrNull;
        }
    }
}

    