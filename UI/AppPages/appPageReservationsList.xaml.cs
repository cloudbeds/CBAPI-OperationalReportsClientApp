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
    public partial class appPageReservationsList : StackPanel, IRequestUiDataRefresh
    {

        readonly uiReservationList _ctlUiReservationsList;
        /// <summary>
        /// Constructor
        /// </summary>
        public appPageReservationsList()
        {
            InitializeComponent();

            //this.Children.Clear();
            _ctlUiReservationsList = new uiReservationList(CloudbedsSingletons.CloudbedsReservationManager_v1.Reservations);
            this.Children.Add(_ctlUiReservationsList);
        }

        /// <summary>
        /// Force the UI to update based on newer data
        /// </summary>
        void IRequestUiDataRefresh.RefreshUiFromData()
        {
            _ctlUiReservationsList.FillReservationsList(CloudbedsSingletons.CloudbedsReservationManager_v1.Reservations);
        }
    }
}
