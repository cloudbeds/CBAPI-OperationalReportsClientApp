using System;


/// <summary>
/// Abstraction to providing a Cloudbeds session 
/// (This will allow us to support not just OAuth powered sessions, but other kinds of sessions)
/// </summary>
interface ICloudbedsServerInfo
{
    string ServerUrl { get; }
}
