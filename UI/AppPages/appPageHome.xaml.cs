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
    public partial class appPageHome: Grid, IRequestUiDataRefresh
    {
        internal delegate void AppNavigationRequestedEventHandler(object sender, RequestAppNavigationEventArgs e);
        internal event AppNavigationRequestedEventHandler AppNavigationRequested;


        /// <summary>
        /// Constructor
        /// </summary>
        public appPageHome()
        {
            InitializeComponent();

        }

        /// <summary>
        /// Trigger the event to any listeners, that we want the application to navigate
        /// </summary>
        /// <param name="targetState"></param>
        private void TriggerAppNavigationRequestEvent(AppNavigationState targetState)
        {
            var evt = this.AppNavigationRequested;
            if(evt != null)
            { 
                evt(this, new RequestAppNavigationEventArgs(targetState));
            }
        }

        /// <summary>
        /// Ask to go to the Guests screen
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void uiGuestsInfoTile_MouseUp(object sender, MouseButtonEventArgs e)
        {
            TriggerAppNavigationRequestEvent(AppNavigationState.ShowCurrentGuests);
        }

        private void infoTileReservations_MouseUp(object sender, MouseButtonEventArgs e)
        {
            TriggerAppNavigationRequestEvent(AppNavigationState.ShowReservations);
        }

        /// <summary>
        /// Force the UI to refresh based on updated data
        /// </summary>
        void IRequestUiDataRefresh.RefreshUiFromData()
        {
            foreach(var thisCtl in this.Children)
            {
                var ctlAsDataRefreshable = thisCtl as IRequestUiDataRefresh;
                if(ctlAsDataRefreshable != null)
                {
                    RequestChildRefreshData(ctlAsDataRefreshable);
                }
            }
            
            //RequestChildRefreshData(infoTileGuests as IRequestUiDataRefresh);
            //RequestChildRefreshData(infoTileCacheAge as IRequestUiDataRefresh);
        }

        /// <summary>
        /// Called to ask an infotile to refresh itself based on new data
        /// </summary>
        /// <param name="requestUiDataRefresh"></param>
        private void RequestChildRefreshData(IRequestUiDataRefresh requestUiDataRefresh)
        {
            IwsDiagnostics.Assert(
                requestUiDataRefresh != null, 
                "0204-1018: UI element does not support refresh interface");

            if(requestUiDataRefresh != null)
            {
                requestUiDataRefresh.RefreshUiFromData();
            }
        }

    }
}
