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
    public partial class uiInfoTileLineItem : UserControl
    {

        /// <summary>
        /// Constructor
        /// </summary>
        public uiInfoTileLineItem()
        {
            InitializeComponent();
            //RefreshText();
        }


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="summaryText"></param>
        /// <param name="detailText"></param>
        public uiInfoTileLineItem(string summaryText, string detailText) : this()
        {
            txtSummary.Text = summaryText;
            txtDetail.Text = detailText;
        }


        /*
                /// <summary>
                /// Refresh the widget UI based on the Cloudbeds data we have
                /// </summary>
                public void RefreshText()
                {
                    var cbReservationMgr = CloudbedsSingletons.CloudbedsReservationManager;
                    txtNumberReservations.Text =  cbReservationMgr.ReservationCount.ToString() + " Reservations";
                }
        */
    }
}
