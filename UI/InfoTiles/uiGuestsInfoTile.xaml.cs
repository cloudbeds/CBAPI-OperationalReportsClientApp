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
    /// Interaction logic for xaml
    /// </summary>
    public partial class uiGuestsInfoTile : UserControl, IRequestUiDataRefresh
    {
        /*
        /// <summary>
        /// Delegate and Event for when a Guest gets clicked/selected
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        internal delegate void GuestSelectedEventHandler(object sender, GuestSelectedEventArgs e);
        internal event GuestSelectedEventHandler GuestSelected;
        */

        /// <summary>
        /// Constructor
        /// </summary>
        public uiGuestsInfoTile()
        {
            InitializeComponent();
            RefreshText();
        }

        /// <summary>
        /// Refresh the widget UI based on the Cloudbeds data we have
        /// </summary>
        public void RefreshText()
        {
            var cbGuestMgr = CloudbedsSingletons.CloudbedsGuestManager;

            var uiItems = spLineItems.Children;
            uiItems.Clear();
            uiItems.Add(new uiInfoTileLineItem(cbGuestMgr.GuestsCount.ToString(), "total"));
            uiItems.Add(new uiInfoTileLineItem("##", "in house"));
            uiItems.Add(new uiInfoTileLineItem("##", "leaving today"));

            /*
            var cbGuestMgr = CloudbedsSingletons.CloudbedsGuestManager;
            txtNumberGuests.Text =  cbGuestMgr.GuestsCount.ToString() + " Guests";
            */
        }

        /// <summary>
        /// Update UI based on new data
        /// </summary>
        void IRequestUiDataRefresh.RefreshUiFromData()
        {
            RefreshText(); 
        }
    }
}
