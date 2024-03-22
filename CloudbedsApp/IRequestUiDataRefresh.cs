
using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Web;

/// <summary>
/// </summary>
internal interface IRequestUiDataRefresh
{
    /// <summary>
    /// Called when someone wants the UI to refresh itself with new data
    /// </summary>
    void RefreshUiFromData();

}
