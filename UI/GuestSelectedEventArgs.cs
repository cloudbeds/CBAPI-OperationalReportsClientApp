using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

internal class GuestSelectedEventArgs : EventArgs
{
    public readonly CloudbedsGuest Guest;
    public GuestSelectedEventArgs(CloudbedsGuest guest) : base()
    {
        this.Guest = guest;
    }
}
