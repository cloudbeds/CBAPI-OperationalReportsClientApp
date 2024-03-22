using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

internal class RequestAppNavigationEventArgs : EventArgs
{
    public readonly AppNavigationState AppNavigationState;
    public RequestAppNavigationEventArgs(AppNavigationState requestedState) : base()
    {
        this.AppNavigationState = requestedState;
    }
}
