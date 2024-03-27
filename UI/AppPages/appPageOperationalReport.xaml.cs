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
    /// Interaction logic for appPageReservationsList.xaml
    /// </summary>
    public partial class appPageOperationalReport : StackPanel, IRequestUiDataRefresh
    {

        private uiOccupancyChangesList _ctlUiReport;
        /// <summary>
        /// Constructor
        /// </summary>
        public appPageOperationalReport()
        {
            InitializeComponent();

            ResetUiBasedOnDataAvailability();

        }

        private void ResetUiBasedOnDataAvailability()
        {
            //If we have the data cached, then generate the report...
            if (CloudbedsSingletons.IsDataAvailableForDailyOperationsReport())
            {
                FillDataArea_QueryDataIfNeeded();

                //Hide the query data UI
                spQueryForDataIfNeeded.Visibility = Visibility.Collapsed;
            }
            else
            {
                spContent.Children.Clear(); //Hide any list data
                //Show the query data UI
                spQueryForDataIfNeeded.Visibility = Visibility.Visible;

            }

        }


        /// <summary>
        /// Fill the UI with data
        /// </summary>
        private void FillDataArea_QueryDataIfNeeded()
        {
            //========================================================================================
            //Add the occupancy changes report
            //========================================================================================
            var ctlOccupancyChanges = new uiOccupancyChangesList(CloudbedsSingletons.GenerateDailyOperationsReports());
            spContent.Children.Add(ctlOccupancyChanges);

            _ctlUiReport = ctlOccupancyChanges;

        }

        /// <summary>
        /// Force the UI to update based on newer data
        /// </summary>
        void IRequestUiDataRefresh.RefreshUiFromData()
        {
            ResetUiBasedOnDataAvailability();
            /*
            var ctl = _ctlUiReport;
            if(ctl == null)
            {
                IwsDiagnostics.Assert(false, "24020-120: No UI report control to refresh");
                return;
            }

            ctl.FillDailyReportsList(CloudbedsSingletons.GenerateDailyOperationsReports());
            */
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
            var dailyOperationsReports =  CloudbedsSingletons.GenerateDailyOperationsReports();

            //Turn it into a CSV file
            var csvReport = dailyOperationsReports.GenerateCsvReport();

            //Generate the file output
            csvReport.GenerateCSVFile(fileOutputTo);

        }

        private void ButtonQueryForData_Click(object sender, RoutedEventArgs e)
        {
            FillDataArea_QueryDataIfNeeded();
        }
    }
}
