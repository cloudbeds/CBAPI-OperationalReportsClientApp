using System;
using System.Xml;

/// <summary>
/// Abasract interface that all Auth Sessions will implement
/// </summary>
interface ICloudbedsAuthSessionBase : ICloudbedsAuthSessionId, ICloudbedsTransientSecretStorageInfo
{
}
