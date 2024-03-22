using System;
using System.Xml;

interface ICloudbedsTransientSecretStorageInfo
{
    void WriteAsXml(XmlWriter xmlWriter);
}
