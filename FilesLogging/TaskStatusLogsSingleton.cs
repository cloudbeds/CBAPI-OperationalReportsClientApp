using System;
using System.Collections.Generic;
using System.Text;

/// <summary>
/// Singleton to allow a common global status log
/// </summary>
internal static class TaskStatusLogsSingleton
{
    static TaskStatusLogs _statusLogs = new TaskStatusLogs();

    public static TaskStatusLogs Singleton
    {
        get { return _statusLogs; }
    }

    public static void ClearAll()
    {
        _statusLogs.ClearAll();
    }
}
