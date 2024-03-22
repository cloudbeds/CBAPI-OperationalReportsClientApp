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
    public partial class appPageOperationalReportList : StackPanel, IRequestUiDataRefresh
    {

        readonly uiOccupancyChangesList _ctlList;
        /// <summary>
        /// Constructor
        /// </summary>
        public appPageOperationalReportList()
        {
            InitializeComponent();

            //this.Children.Clear();
            _ctlList = new uiOccupancyChangesList(CloudbedsSingletons.GenerateDailyOperationsReports());
            this.Children.Add(_ctlList);
        }

        /// <summary>
        /// Force the UI to update based on newer data
        /// </summary>
        void IRequestUiDataRefresh.RefreshUiFromData()
        {
            _ctlList.FillDailyReportsList(CloudbedsSingletons.GenerateDailyOperationsReports());
        }
    }
}
