
using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Web;

/// <summary>
/// Routes requests for server connect info to the app singleton
/// </summary>
internal partial class CloudbedsServerConnectInfoUsingSingleton : CloudbedsServerConnectInfoBase
{
    internal override ICloudbedsAuthSessionId GetCloudbedsAuthSession()
    {
        return CloudbedsSingletons.CloudbedsAuthSession;
    }

    internal override ICloudbedsServerInfo GetCloudbedsServerInfo()
    {
        return CloudbedsSingletons.CloudbedsServerInfo;
    }
}
