
using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Web;

/// <summary>
/// Routes requests for server connect 
/// </summary>
internal abstract class CloudbedsServerConnectInfoBase
{
    internal abstract ICloudbedsServerInfo GetCloudbedsServerInfo();
    internal abstract ICloudbedsAuthSessionId GetCloudbedsAuthSession();
}
