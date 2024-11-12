using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
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
    /// Interaction logic for uiDailyOccupancyChangesListItem.xaml
    /// </summary>
    public partial class uiDailyOccupancyChangesListItem_v2 : UserControl
    {
        private readonly CloudbedsDailyOperationsReportManager_v2.DailyReport _dailyReport = null;

        /*
        bool _isSelected;
        public bool IsSelected
        {
            get
            {
                return _isSelected;
            }
            set
            {
                if(_isSelected != value)
                {
                    _isSelected = value;
                    UpdateIsSelectedUi();
                }
            }
        }
        */

        internal CloudbedsDailyOperationsReportManager_v2.DailyReport DailyReport
        {
            get
            {
                return _dailyReport;
            }
        }

        /// <summary>
        /// Delegate and Event for when a Reservation gets clicked/selected
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        internal delegate void ReservationSelectedEventHandler(object sender, ReservationSelectedEventArgs e);
        internal event ReservationSelectedEventHandler ReservationSelected;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="reservation"></param>
        internal uiDailyOccupancyChangesListItem_v2(CloudbedsDailyOperationsReportManager_v2.DailyReport dailyReport) : this()
        {
            _dailyReport = dailyReport;

        }

        /// <summary>
        /// Constructor
        /// </summary>
        public uiDailyOccupancyChangesListItem_v2()
        {
            InitializeComponent();
            //UpdateIsSelectedUi();
        }

        /*
        private void UpdateIsSelectedUi()
        {
            if(_isSelected)
            {
                txtIsSelectedMarker.Text = "✓";
                areaExpandDetails.Visibility = Visibility.Visible;
            }
            else
            {
                txtIsSelectedMarker.Text = "";
                areaExpandDetails.Visibility = Visibility.Collapsed;
            }
        }
        */

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            var dailyReport = _dailyReport;
            //Fill in the UI elements
            if (dailyReport != null)
            {
                txtDate.Text = dailyReport.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                txtNumberCheckIns.Text = dailyReport.NumberCheckIns.ToString();
                txtNumberCheckOuts.Text = dailyReport.NumberCheckOuts.ToString();
                txtNumberStayOvers.Text = dailyReport.NumberStayOvers.ToString();
                txtUnassignedCheckins.Text = dailyReport.NumberCheckIns_RoomNotAssigned.ToString();
                txtRoomTurnovers.Text = dailyReport.NumberRoomTurnoversRequired.ToString();
            }
            /*            else //Degenerate case
                        {
                            IwsDiagnostics.Assert(false, "1022-256: Null reservation");
                        }
            */
        }

        /// <summary>
        /// The list item got clicked/tapped
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UserControl_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            //Raise the event
         //   ReservationSelected(this, new ReservationSelectedEventArgs(_dailyReport));
        }

        /*
        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            string uriReservation = "";

            try
            {

            uriReservation = CloudbedsUris.UriGenerate_BrowserReservationDetailsUrl(
                            CloudbedsSingletons.CloudbedsServerInfo,
                            CloudbedsSingletons.CloudbedsHotelDetails,
                            _dailyReport);
            }
            catch(Exception ex)
            {
                MessageBox.Show("Error getting server url. Error " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }



            //Per: https://stackoverflow.com/questions/10238694/example-using-hyperlink-in-wpf
            // for .NET Core you need to add UseShellExecute = true
            // see https://learn.microsoft.com/dotnet/api/system.diagnostics.processstartinfo.useshellexecute#property-value
//            var processInfo = new ProcessStartInfo(e.Uri.AbsoluteUri);
            var processInfo = new ProcessStartInfo(uriReservation);
            processInfo.UseShellExecute = true;
            Process.Start(processInfo);
            e.Handled = true;
        }
        */
    }
}
