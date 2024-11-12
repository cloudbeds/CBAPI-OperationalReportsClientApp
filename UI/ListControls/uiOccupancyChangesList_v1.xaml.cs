using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace OnSiteCompanion
{
    /// <summary>
    /// Interaction logic for uiOccupancyChangesList.xaml
    /// </summary>
    public partial class uiOccupancyChangesList_v1 : UserControl
    {
        /// <summary>
        /// Delegate and Event for when a Reservation gets clicked/selected
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        //internal delegate void ReservationSelectedEventHandler(object sender, ReservationSelectedEventArgs e);
        //internal event ReservationSelectedEventHandler ReservationSelected;

        CloudbedsReservation_v1? _selectedReservation = null;

        /// <summary>
        /// True if the reservation list was filled at least once
        /// </summary>
        SimpleLatch _wasReservationListStocked = new SimpleLatch();

        /// <summary>
        /// UI items for each reservation
        /// </summary>
        //List<uiReservationListItem> _ctlReservationListItems = new List<uiReservationListItem>();

        /// <summary>
        /// True if the reservation list was filled at least once
        /// </summary>
        public bool IsReservationsListStocked
        {
            get
            {
                return _wasReservationListStocked.Value;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public uiOccupancyChangesList_v1()
        {
            InitializeComponent();
        }

        
        internal uiOccupancyChangesList_v1(CloudbedsDailyOperationsReportManager_v1 operationsReports) : this()
        {
            FillDailyReportsList(operationsReports);
        }
        

        /*
        /// <summary>
        /// Clear out any reservation selection
        /// </summary>
        public void ClearReservationSelection()
        {
            _selectedReservation = null;
            UpdateSelectedReservationUi();
        }
        */

        /*

        /// <summary>
        /// Called when an individual Reservation list item is selected
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void EventHander_ReservationSelectedEventHandler(object sender, ReservationSelectedEventArgs e)
        {
            _selectedReservation = e.Reservation;
            UpdateSelectedReservationUi();

            var evtReservationSelected = this.ReservationSelected;
            if(evtReservationSelected != null)
            {
                //Bubble the event upward
                evtReservationSelected(this, e);
            }
        }
        */

        /*
        /// <summary>
        /// Update the selected state ui for each reservation item
        /// </summary>
        private void UpdateSelectedReservationUi()
        {
            var listCtls = _ctlReservationListItems;
            if(listCtls == null)
            {
                return;
            }


            foreach (var thisCtl in listCtls)
            {
                thisCtl.IsSelected = (thisCtl.Reservation == _selectedReservation);
            }
        }
        */

        /// <summary>
        /// Fill the visible list with controls...
        /// </summary>
        /// <param name="reservations"></param>
//        internal void FillReservationsList(ICollection<CloudbedsReservation>? reservations)
        internal void FillDailyReportsList(CloudbedsDailyOperationsReportManager_v1 operationsReports)
        {
            _wasReservationListStocked.Trigger();

            var uiChildren = spListOfOccupancyDates.Children;

            var dailyReports = operationsReports.DailyReports;
            //Get rid of all the existing items
            uiChildren.Clear();
            if((dailyReports == null) || (dailyReports.Count == 0))
            {

                //We could show a fancier custom control here to indicate
                //that there are no controls
                var txtCtl = new TextBlock();
                txtCtl.Text = "No days to report";

                uiChildren.Add(txtCtl);
                return;
            }
            

            //Create a header control
            uiChildren.Add(new uiDailyOccupancyChangesListHeader());

            
            //==============================================
            //Add a control for each reservation
            //==============================================
            foreach (var thisDayReport in dailyReports)
            {
                //Create the control
                var ctlSingleDay = new uiDailyOccupancyChangesListItem_v1(thisDayReport);

                //Hook up the event to listen to it here...
                //ctlReservationListItem.ReservationSelected += EventHander_ReservationSelectedEventHandler;

                uiChildren.Add(ctlSingleDay);
                //_ctlReservationListItems.Add(ctlReservationListItem);
            }
            
        }

    }
}
