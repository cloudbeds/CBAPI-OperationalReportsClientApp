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
    


    /// <summary>
    /// Create a testing version of the class with fake data
    /// </summary>
    /// <returns></returns>
    internal static CloudbedsAppConfig TESTING_CreateSimulatedAppConfig()
    {
        return new CloudbedsAppConfig(
            "FAKE Server URL",
            "FAKE CLIENT ID",
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
    private CloudbedsAppConfig(string serverUrl, string clientId, string clientSecret, string authRedirectUri)
    {
        this.CloudbedsServerUrl = serverUrl;
        this.CloudbedsAppClientId = clientId;
        this.CloudbedsAppClientSecret = clientSecret;
        this.CloudbedsAppOAuthRedirectUri = authRedirectUri;

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
        string cloudbedsAppOAuthRedirectUri = xNode.Attributes["oAuthRedirectUri"].Value;

        return new CloudbedsAppConfig(
            cloudbedsServerUrl,
            cloudbedsAppClientId,
            cloudbedsAppClientSecret,
            cloudbedsAppOAuthRedirectUri);
    }


    string ICloudbedsServerInfo.ServerUrl
    {
        get { return this.CloudbedsServerUrl; } 
    }
}

    