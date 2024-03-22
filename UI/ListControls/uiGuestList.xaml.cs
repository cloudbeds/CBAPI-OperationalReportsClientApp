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
    public partial class uiGuestList : UserControl
    {
        /// <summary>
        /// Delegate and Event for when a Guest gets clicked/selected
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        internal delegate void GuestSelectedEventHandler(object sender, GuestSelectedEventArgs e);
        internal event GuestSelectedEventHandler GuestSelected;

        CloudbedsGuest? _selectedGuest = null;

        /// <summary>
        /// True if the guest list was filled at least once
        /// </summary>
        SimpleLatch _wasGuestListStocked = new SimpleLatch();

        /// <summary>
        /// UI items for each guest
        /// </summary>
        List<uiGuestListItem> _ctlGuestListItems = new List<uiGuestListItem>();

        /// <summary>
        /// True if the guest list was filled at least once
        /// </summary>
        public bool IsGuestsListStocked
        {
            get
            {
                return _wasGuestListStocked.Value;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public uiGuestList()
        {
            InitializeComponent();
        }

        internal uiGuestList(ICollection<CloudbedsGuest> guests) : this()
        {
            FillGuestsList(guests);
        }


        /// <summary>
        /// Clear out any guest selection
        /// </summary>
        public void ClearGuestSelection()
        {
            _selectedGuest = null;
            UpdateSelectedGuestUi();
        }

        /// <summary>
        /// Called when an individual Guest list item is selected
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void EventHander_GuestSelectedEventHandler(object sender, GuestSelectedEventArgs e)
        {
            _selectedGuest = e.Guest;
            UpdateSelectedGuestUi();

            var evtGuestSelected = this.GuestSelected;
            if(evtGuestSelected != null)
            {
                //Bubble the event upward
                evtGuestSelected(this, e);
            }
        }

        /// <summary>
        /// Update the selected state ui for each guest item
        /// </summary>
        private void UpdateSelectedGuestUi()
        {
            var listCtls = _ctlGuestListItems;
            if(listCtls == null)
            {
                return;
            }


            foreach (var thisCtl in listCtls)
            {
                thisCtl.IsSelected = (thisCtl.Guest == _selectedGuest);
            }
        }


        /// <summary>
        /// Fill the visible list with controls...
        /// </summary>
        /// <param name="guests"></param>
        internal void FillGuestsList(ICollection<CloudbedsGuest>? guests)
        {
            _wasGuestListStocked.Trigger();

            var uiChildren = spListOfGuests.Children;
            _ctlGuestListItems = new List<uiGuestListItem>();

            //Get rid of all the existing items
            uiChildren.Clear();
            if((guests == null) || (guests.Count == 0))
            {

                //We could show a fancier custom control here to indicate
                //that there are no controls
                var txtCtl = new TextBlock();
                txtCtl.Text = "No guests in list";

                uiChildren.Add(txtCtl);
                return;
            }

            //Create a header control
            uiChildren.Add(new uiGuestListHeader());

            //==============================================
            //Add a control for each guest
            //==============================================
            foreach (var thisGuest in guests)
            {
                //Create the control
                var ctlGuestListItem = new uiGuestListItem(thisGuest);
                //Hook up the event to listen to it here...
                ctlGuestListItem.GuestSelected += EventHander_GuestSelectedEventHandler;

                uiChildren.Add(ctlGuestListItem);
                _ctlGuestListItems.Add(ctlGuestListItem);
            }

        }

    }
}
