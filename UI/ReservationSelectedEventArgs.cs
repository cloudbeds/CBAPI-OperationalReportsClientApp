using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

internal class ReservationSelectedEventArgs : EventArgs
{
    public readonly CloudbedsReservation Reservation;
    public ReservationSelectedEventArgs(CloudbedsReservation reservation) : base()
    {
        this.Reservation = reservation;
    }
}
