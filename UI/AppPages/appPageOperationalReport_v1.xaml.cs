using System;
using System.Collections;
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
    /// Interaction logic for appPageReservationsList.xaml
    /// </summary>
    public partial class appPageOperationalReport_v1 : StackPanel, IRequestUiDataRefresh
    {

        private uiOccupancyChangesList_v1 _ctlUiReport;
        /// <summary>
        /// Constructor
        /// </summary>
        public appPageOperationalReport_v1()
        {
            InitializeComponent();

            ResetUiBasedOnDataAvailability();

        }


        /// <summary>
        /// Gives us information about the age of the data cache
        /// </summary>
        private void UpdateCacheAgeText()
        {
            var cacheAgeUtc = CloudbedsSingletons.ReservationsWithRooms_v1_CacheLastUpdatedTimeUtc;
            if(cacheAgeUtc == null)
            {
                txtCacheAge.Text = "No data in local cache ...";
                return;
            }


            var localTime= cacheAgeUtc.Value.ToLocalTime();
            txtCacheAge.Text = "Cache updated: " + localTime.ToString();
        }


        /// <summary>
        /// Update UI visibility and contents
        /// </summary>
        private void ResetUiBasedOnDataAvailability()
        {
            //If we have the data cached, then generate the report...
            if (CloudbedsSingletons.IsDataAvailableForDailyOperationsReport_v1())
            {
                FillDataArea_QueryDataIfNeeded();

                //Hide the query data UI
                spQueryForDataIfNeeded.Visibility = Visibility.Collapsed;
                //Show the CSV options
                spGenerateCSVsOptions.Visibility = Visibility.Visible;
                spLocalCacheInfo.Visibility = Visibility.Visible;

            }
            else
            {
                spContent.Children.Clear(); //Hide any list data
                //Show the query data UI
                spQueryForDataIfNeeded.Visibility = Visibility.Visible;

                spGenerateCSVsOptions.Visibility = Visibility.Collapsed;
                spLocalCacheInfo.Visibility = Visibility.Collapsed;
            }

            UpdateCacheAgeText();
        }


        /// <summary>
        /// Fill the UI with data
        /// </summary>
        private void FillDataArea_QueryDataIfNeeded()
        {
            //========================================================================================
            //Add the occupancy changes report
            //========================================================================================
            var ctlOccupancyChanges = new uiOccupancyChangesList_v1(
                CloudbedsSingletons.GenerateDailyOperationsReports_v1());

            spContent.Children.Add(ctlOccupancyChanges);

            _ctlUiReport = ctlOccupancyChanges;

        }

        /// <summary>
        /// Force the UI to update based on newer data
        /// </summary>
        void IRequestUiDataRefresh.RefreshUiFromData()
        {
            ResetUiBasedOnDataAvailability();
        }

        /// <summary>
        /// Save the daily report as a CSV file
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonGenerateOperationalReportCsv_Click(object sender, RoutedEventArgs e)
        {
            //=====
            var fileDialog = new Microsoft.Win32.SaveFileDialog();
            fileDialog.DefaultExt = ".csv";
            fileDialog.Filter = "CSV Files *.csv|*.csv";
            var result = fileDialog.ShowDialog();

            if(result != true) 
            {
                return;
            }

            string fileOutputTo = fileDialog.FileName;

            //Generate the daily operations #s
            var dailyOperationsReports =  CloudbedsSingletons.GenerateDailyOperationsReports_v1();

            //Turn it into a CSV file
            var csvReport = dailyOperationsReports.GenerateCsvReport();

            //Generate the file output
            csvReport.GenerateCSVFile(fileOutputTo);
        }


        /// <summary>
        /// Query for data (or use data from cache) and fill in the grid
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonQueryForData_Click(object sender, RoutedEventArgs e)
        {
            //Since this may run a while, show the wait cursor
            this.Cursor = Cursors.Wait;
            
            try
            {
                FillDataArea_QueryDataIfNeeded();
            }
            catch(Exception ex) 
            {
                CloudbedsSingletons.StatusLogs.AddError("Error running query: " + ex.Message);
            }
            
            this.Cursor = null;  //revert to default

            //Update the UI
            ResetUiBasedOnDataAvailability();
        }

        /// <summary>
        /// Generate reservation details for each day
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonGenerateOperationalReportWithReservationDetailsCsv_Click(object sender, RoutedEventArgs e)
        {
            //=====
            var fileDialog = new Microsoft.Win32.SaveFileDialog();
            fileDialog.DefaultExt = ".csv";
            fileDialog.Filter = "CSV Files *.csv|*.csv";
            var result = fileDialog.ShowDialog();

            if (result != true)
            {
                return;
            }

            string fileOutputTo = fileDialog.FileName;

            //Generate the daily operations #s
            var dailyOperationsReports = CloudbedsSingletons.GenerateDailyOperationsReports_ResRoomDetails_v1();

            //Turn it into a CSV file
            var csvReport = dailyOperationsReports.GenerateCsvReport();

            //Generate the file output
            csvReport.GenerateCSVFile(fileOutputTo);

        }


        /// <summary>
        /// Clear a local data cache
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnClearCache_Click(object sender, RoutedEventArgs e)
        {
            CloudbedsSingletons.ReservationsWithRooms_v1_ClearCache();
            ResetUiBasedOnDataAvailability();
        }
    }
}
