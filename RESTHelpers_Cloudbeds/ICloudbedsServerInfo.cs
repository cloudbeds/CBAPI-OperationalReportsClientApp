using System;


/// <summary>
/// Abstraction to providing a Cloudbeds session 
/// (This will allow us to support not just OAuth powered sessions, but other kinds of sessions)
/// </summary>
interface ICloudbedsServerInfo
{
    /// <summary>
    /// This is the server URL we are calling the APIs on
    /// </summary>
    string ServerUrl { get; }

    /// <summary>
    /// If specified, this will be the property id
    /// </summary>
    string PropertyIdOrNull { get; }
}
