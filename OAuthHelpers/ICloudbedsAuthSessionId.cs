using System;


/// <summary>
/// Abstraction to providing a Cloudbeds session 
/// (This will allow us to support not just OAuth powered sessions, but other kinds of sessions)
/// </summary>
interface ICloudbedsAuthSessionId
{
    /// <summary>
    /// Session ID to use for REST API requests
    /// </summary>
    string AuthSessionsId { get; }
    
    /// <summary>
    /// Status text we want to show in debugger / UI
    /// </summary>
    /// <returns></returns>
    string DebugStatusText();
}
