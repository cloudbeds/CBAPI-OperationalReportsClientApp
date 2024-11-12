using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

internal class ReservationSelectedEventArgs : EventArgs
{
    public readonly CloudbedsReservation_v1 Reservation;
    public ReservationSelectedEventArgs(CloudbedsReservation_v1 reservation) : base()
    {
        this.Reservation = reservation;
    }
}
