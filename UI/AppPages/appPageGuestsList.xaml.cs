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
    public partial class appPageGuestsList : StackPanel, IRequestUiDataRefresh
    {

        readonly uiGuestList _ctlUiGuestsList;
        /// <summary>
        /// Constructor
        /// </summary>
        public appPageGuestsList()
        {
            InitializeComponent();

            //this.Children.Clear();
            _ctlUiGuestsList = new uiGuestList(CloudbedsSingletons.CloudbedsGuestManager.Guests);
            this.Children.Add(_ctlUiGuestsList);
        }

        /// <summary>
        /// Force the UI to update based on newer data
        /// </summary>
        void IRequestUiDataRefresh.RefreshUiFromData()
        {
            _ctlUiGuestsList.FillGuestsList(CloudbedsSingletons.CloudbedsGuestManager.Guests);
        }
    }
}
