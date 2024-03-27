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
    /// Interaction logic for uiCBPropertyPicker.xaml
    /// </summary>
    public partial class uiCBPropertyPicker : UserControl
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public uiCBPropertyPicker()
        {
            InitializeComponent();

            FillPropertyPickerList();
        }


        
        private void FillPropertyPickerList()
        {
            var uiItemsSet = cboPropertyPicker.Items;
            uiItemsSet.Clear();

            var currentlyActiveCbSession = CloudbedsSingletons.SelectedSession;

            ComboBoxItem uiSelectThisListItem = null;
            foreach (var cbSession in CloudbedsSingletons.ListOfCBSessions)
            {
                var uiComboListItem = new ComboBoxItem();
                uiComboListItem.Content = cbSession.CloudbedsServerInfo.Name;


                uiItemsSet.Add(uiComboListItem);

                //If this item matches the currently "live" session, then we will want to select it
                if (cbSession == currentlyActiveCbSession)
                {
                    uiSelectThisListItem = uiComboListItem;
                }
            }

            //If we have an item to select, then select it
            if(uiSelectThisListItem!= null) 
            {
                cboPropertyPicker.SelectedItem = uiSelectThisListItem;
            }
        }

        /// <summary>
        /// A property is selected from the combo bo
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cboPropertyPicker_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var previousCBSession = CloudbedsSingletons.CloudbedsServerInfo;

            CloudbedsSessionState selectedCBSession = null;

            var cboItemSelected = cboPropertyPicker.SelectedItem as ComboBoxItem;
            if (cboItemSelected != null )
            {

                string cboItemSelected_text = cboItemSelected.Content.ToString();
                try
                {
                    selectedCBSession = CloudbedsSingletons.SelectCBSessionByName(cboItemSelected_text);
                }
                catch
                {
                    IwsDiagnostics.Assert(false, "240326-512: Unexpected. Property not found in set: " + cboItemSelected_text);
                }

                //Highlight it in the UI
                if (selectedCBSession != null)
                {
                    cboPropertyPicker.Text = selectedCBSession.Name;
                }
            }

            //If the sessions are different then trigger an application UI reset
            if(previousCBSession != selectedCBSession)
            {
                CloudbedsSingletons.SendDataRefreshSignal();
            }

        }
    }
}
