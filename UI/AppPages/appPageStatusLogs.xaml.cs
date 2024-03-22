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
    /// Interaction logic for uiGuestList.xaml
    /// </summary>
    public partial class appPageStatusLogs : StackPanel, IRequestUiDataRefresh
    {

        /// <summary>
        /// Constructor
        /// </summary>
        public appPageStatusLogs()
        {
            InitializeComponent();

            FillStatusLogsUi();
        }

        private void FillStatusLogsUi()
        {
            var statusLogs = CloudbedsSingletons.StatusLogs;

            txtStatusLogs.Text = statusLogs.StatusText;
            txtErrorLogs.Text = statusLogs.ErrorText;
        }

        /// <summary>
        /// If we've got new data in the cache, we probably have new status entries
        /// we want to show as well...
        /// </summary>
        void IRequestUiDataRefresh.RefreshUiFromData()
        {
            FillStatusLogsUi();
        }
    }
}
