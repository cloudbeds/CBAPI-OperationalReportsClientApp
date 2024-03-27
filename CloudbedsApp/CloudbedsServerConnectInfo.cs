
using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Web;

/// <summary>
/// Routes requests for server connect info to the app singleton
/// </summary>
internal partial class CloudbedsServerConnectInfo : CloudbedsServerConnectInfoBase
{
    private readonly ICloudbedsServerInfo _serverInfo;
    private readonly ICloudbedsAuthSessionId _authSession;
    public CloudbedsServerConnectInfo(ICloudbedsServerInfo serverInfo, ICloudbedsAuthSessionId authSession)
    {
        _serverInfo = serverInfo;
        _authSession = authSession;

    }
    internal override ICloudbedsServerInfo GetCloudbedsServerInfo()
    {
        return _serverInfo;
    }

    internal override ICloudbedsAuthSessionId GetCloudbedsAuthSession()
    {
        return _authSession;
    }

}
