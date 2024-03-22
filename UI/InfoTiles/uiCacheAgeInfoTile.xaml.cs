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
    public partial class uiCacheAgeInfoTile : UserControl, IRequestUiDataRefresh
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
        public uiCacheAgeInfoTile()
        {
            InitializeComponent();
            RefreshText();
        }

        /// <summary>
        /// Refresh the widget UI based on the Cloudbeds data we have
        /// </summary>
        public void RefreshText()
        {
            var cacheAge = CloudbedsSingletons.GetDataCacheAgeOrNull();
            if(cacheAge == null)
            {
                txtCacheAge.Text = "Cache is MISSING data (need to refresh)";
                return;
            }

            var cacheAgeLocalDate = DateTime.Now - cacheAge.Value;

            var cacheAgeValue = cacheAge.Value;
            if(cacheAgeValue.TotalMinutes <= 90)
            {
                txtCacheAge.Text = "Cache age: "
                    //+ Math.Round(cacheAgeValue.TotalMinutes).ToString("0")
                    //+ " minutes "
                    + cacheAgeLocalDate.ToShortDateString() + " " + cacheAgeLocalDate.Hour.ToString() + ":" + cacheAgeLocalDate.Minute.ToString("00")
                    + " (refresh interval: " + 
                    Math.Round(CloudbedsSingletons.RefreshScheduler.IntervalForCloudbedsCacheRefresh.TotalMinutes).ToString() 
                    + " minutes)";
                return;
            }

            if (cacheAgeValue.TotalHours <= 24)
            {
                txtCacheAge.Text = "Cache age: " 
                    + Math.Round(cacheAgeValue.TotalHours).ToString("0") 
                    + " hours";
                return;
            }

            if (cacheAgeValue.TotalDays <= 4)
            {
                txtCacheAge.Text = "Cache age: "
                    + Math.Round(cacheAgeValue.TotalDays).ToString("0")
                    + " days";
                return;
            }

            txtCacheAge.Text = "Cache age: OVER 4 days";
            //var cbGuestMgr = CloudbedsSingletons.CloudbedsGuestManager;
            //txtNumberGuests.Text =  cbGuestMgr.GuestsCount.ToString() + " Guests";
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ForceCacheRefresh_Click(object sender, RoutedEventArgs e)
        {
            txtCacheAge.Text = "Requesting data...";
            try
            {
                CloudbedsSingletons.RefreshScheduler.RefreshOfCloudbedsDataCacheAndTriggerAppNotifications();
                //CloudbedsSingletons.ForceRefreshOfCloudbedsDataCache();

                RefreshText(); //Update the tile's UI
            }
            catch (Exception ex)
            {
                CloudbedsSingletons.StatusLogs.AddError("0131-600: Error attempting Cloudbeds cache refresh: " + ex.Message);
                txtCacheAge.Text = "ERROR attempting to refresh cache";
            }
        }

        /// <summary>
        /// Update the UI based on new data
        /// </summary>
        void IRequestUiDataRefresh.RefreshUiFromData()
        {

            RefreshText();
        }
    }
}
