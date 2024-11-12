using System;
using System.Collections.Generic;
using System.IO;
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
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window , IDataRefreshNotify
    {

        //What Guest has been selected in the UI presently?
        CloudbedsGuest? _currentSelectedGuest = null;

        AppNavigationState _navState = AppNavigationState.None;

        public MainWindow()
        {
            InitializeComponent();

            //Quickly load our starting data from the local file system cache
            //if it is there...
            //CloudbedsSingletons.TryLoadPropertyDataFromLocalCache();

            //Set up our scheduled data refresh
            var dataRefresher = CloudbedsSingletons.RefreshScheduler;

            //Have this UI get refresh notifications
            dataRefresher.SetRefreshNotificationTarget(this);
            dataRefresher.RefreshEnabled = false; //Do not refresh data on an automatic schedule

            UpdateSystemStatus("Started");

            //===================================================================
            //Validate that we have the necessary configuration files.
            //If we don't, go to the setup screen
            //===================================================================
            try
            {
                CloudbedsSingletons.EnsureValidConfigurationFilesExist();
                StateMachine_PerformTransition(AppNavigationState.AppHomepage);
            }
            catch
            {
                MessageBox.Show("Cloudbends client configuration missing or corrupt. Most likely file paths need to be set", "Configuration error", MessageBoxButton.OK, MessageBoxImage.Error);
                StateMachine_PerformTransition(AppNavigationState.ShowSetup);
            }
        }

        /// <summary>
        /// Perform any actions we need to do when EXITING a state
        /// </summary>
        /// <param name="currentState"></param>
        private void StateMachine_PrepareToExitState(AppNavigationState currentState)
        {
            switch (currentState)
            {
                case AppNavigationState.AppHomepage:
                    //NOTHING TO DO
                    return;

                case AppNavigationState.ShowCurrentGuests:
                    //NOTHING TO DO
                    return;

                case AppNavigationState.ShowSetup:
                    //NOTHING TO DO
                    return;

                case AppNavigationState.ShowReservations:
                    //NOTHING TO DO
                    return;

                case AppNavigationState.ShowOperationalReport_v1:
                    //NOTHING TO DO
                    return;

                case AppNavigationState.ShowOperationalReport_v2:
                    //NOTHING TO DO
                    return;

                case AppNavigationState.ShowStatusLogs:
                    //Nothing to do
                    return;

                case AppNavigationState.None:
                    //Nothing to do
                    return;

                default:
                    IwsDiagnostics.Assert(false, "1028-448: Uknown state");
                    return;
            }
        }

        /// <summary>
        /// Perform any actions we need to do when EXITING a state
        /// </summary>
        /// <param name="targetState"></param>
        /// <param name="filterParam_PosCategory">Optional parameter to pass into the new UI we are showing</param>
        private void StateMachine_PrepareToEnterState(AppNavigationState targetState, TaskStatusLogs paramStatusLogs = null)
        {
            switch (targetState)
            {

                case AppNavigationState.ShowCurrentGuests:
                    StateMachine_ReplaceActiveUIAreas(
                        new appPageGuestsList());
                    return;

                case AppNavigationState.ShowOperationalReport_v1:
                    StateMachine_ReplaceActiveUIAreas(
                        new appPageOperationalReport_v1());
                    return;

                case AppNavigationState.ShowOperationalReport_v2:
                    StateMachine_ReplaceActiveUIAreas(
                        new appPageOperationalReport_v2());
                    return;

                case AppNavigationState.ShowReservations:
                    StateMachine_ReplaceActiveUIAreas(
                        new appPageReservationsList());
                    return;

                case AppNavigationState.ShowStatusLogs:
                    var ctlStatusLogs = new appPageStatusLogs();
                    StateMachine_ReplaceActiveUIAreas(
                        ctlStatusLogs);
                    return;

                case AppNavigationState.AppHomepage:
                    var ctlHomepage = new appPageHome();
                    ctlHomepage.AppNavigationRequested += EventHandler_AppNavigationRequested;
                    StateMachine_ReplaceActiveUIAreas(
                        ctlHomepage);
                    return;

                case AppNavigationState.ShowSetup:
                    StateMachine_ReplaceActiveUIAreas(
                        new appPageSetup());
                    return;

                case AppNavigationState.None:
                    //Nothing to do
                    return;


                default:
                    IwsDiagnostics.Assert(false, "1028-448: Unknown state");
                    return;
            }
        }

        /// <summary>
        /// A common event handler that all UI pages can use if they want to request application
        /// navigation
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void EventHandler_AppNavigationRequested(object sender, RequestAppNavigationEventArgs e)
        {
            StateMachine_PerformTransition(e.AppNavigationState);
        }

        /// <summary>
        /// Update the UI for the new state
        /// </summary>
        /// <param name="uiCtl"></param>
        private void StateMachine_ReplaceActiveUIAreas(UIElement uiCtl)
        {
            var children = mainContentArea.Children;
            children.Clear();

            if(uiCtl != null)
            {
                children.Add(uiCtl);
            }
            //mainContentArea.Child = uiCtl;
        }


        /// <summary>
        /// Move from the current state into this state
        /// </summary>
        /// <param name="nextState"></param>
        private void StateMachine_PerformTransition(AppNavigationState nextState)
        {
            StateMachine_PerformTransition(_navState, nextState);
        }

        /// <summary>
        /// Called to move from 1 state into another
        /// </summary>
        /// <param name="currentState"></param>
        /// <param name="nextState"></param>
        /// <param name="filterParam_PosCategory">Optional parameter to pass into the new UI we are showing</param>
        void StateMachine_PerformTransition(
            AppNavigationState currentState,
            AppNavigationState nextState,
            TaskStatusLogs paramStatusLogs = null)
        {
            //Nothing to do
            if (currentState == nextState)
            {
                return;
            }

            StateMachine_PrepareToExitState(currentState);
            StateMachine_PrepareToEnterState(nextState, paramStatusLogs);
            _navState = nextState;
        }


        /// <summary>
        /// Shows the result of running a command
        /// </summary>
        /// <param name="text"></param>
        void UpdateSystemStatus(string text)
        {
            textSystemStatus.Text = text + " (" + DateTime.Now.ToString() + ")";
        }


        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //Load the normal application preferences
            AppPreferences_Load();
        }

        /// <summary>
        /// Load app preferences that we want to auto-load next time
        /// </summary>
        private void AppPreferences_Load()
        {
            try
            {
//                AppPreferences_Load_Inner();
            }
            catch (Exception ex)
            {
                IwsDiagnostics.Assert(false, "819-1041: Error loading app prefernces, " + ex.Message);
            }
        }
/*
        /// <summary>
        /// Load app preferences that we want to auto-load next time
        /// </summary>
        private void AppPreferences_Load_Inner()
        {
            txtPathToAppSecrets.Text = AppSettings.LoadPreference_PathAppSecretsConfig();
            txtPathToUserTokenSecrets.Text = AppSettings.LoadPreference_PathUserAccessTokens();

            //If any of these are blank then generate them
            txtPathToAppSecrets.Text = AppPreferences_GenerateDefaultIfBlank(txtPathToAppSecrets.Text, "Templates_Secrets\\Example_AppSecrets.xml");
            txtPathToUserTokenSecrets.Text = AppPreferences_GenerateDefaultIfBlank(txtPathToUserTokenSecrets.Text, "Templates_Secrets\\Example_UserTokenSecrets.xml");
        }
*/
        /// <summary>
        /// If the proposed path is blank, then generate a path based on the applicaiton's path and the specified sub-path
        /// </summary>
        /// <param name="proposedPath"></param>
        /// <param name="subPath"></param>
        /// <returns></returns>
        private string AppPreferences_GenerateDefaultIfBlank(string proposedPath, string subPath)
        {
            if (!string.IsNullOrWhiteSpace(proposedPath))
            {
                return proposedPath;
            }

            var basePath = AppSettings.LocalFileSystemPath;
            return System.IO.Path.Combine(basePath, subPath);
        }

        /// <summary>
        /// Called when a GUEST is selected for us to focus on
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ctlGuestList_GuestSelected(object sender, GuestSelectedEventArgs e)
        {
            var selectedGuest = e.Guest;
            _currentSelectedGuest = selectedGuest;
            
            //Subtle update to UI to show a guest was selected
            if(selectedGuest != null)
            {
                UpdateSystemStatus("Selected guest: " + selectedGuest.Guest_Name);
            }
        }




        /// <summary>
        /// Resize the UI based on the window size
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            RepondToWindowSizeChange();
        }

        /// <summary>
        /// Resize the UI based on the window size
        /// </summary>
        private void RepondToWindowSizeChange()
        {
            var windowWidth = this.Width;
            if(this.WindowState == WindowState.Maximized)
            {
                windowWidth = SystemParameters.WorkArea.Width;
            }

            //Sanity metric...
            if (windowWidth > 10)
            {
                gridMaster.Width = windowWidth;
                //tabMaster.Width = windowWidth;
            }
        }

        /// <summary>
        /// Resize the UI based on the window size
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_StateChanged(object sender, EventArgs e)
        {
            switch(this.WindowState)
            {
                case WindowState.Maximized:
                case WindowState.Normal:
                    RepondToWindowSizeChange();
                    break;
                default:
                    break;
            }
        }







/****************************************************************************************
TESTING BUTTONS
Buttons used to trigger testing/diagnostic code (not part of main functional application)
*****************************************************************************************/
#region "Testing Buttons"
        #endregion

/****************************************************************************************
CONFIGURATION TAB
*****************************************************************************************/

        private void NavigateApp_ShowCurrentGuests(object sender, RoutedEventArgs e)
        {
            StateMachine_PerformTransition(AppNavigationState.ShowCurrentGuests);
        }

        private void NavigateApp_ShowStatusLogs(object sender, RoutedEventArgs e)
        {
            StateMachine_PerformTransition(AppNavigationState.ShowStatusLogs);
        }

        private void NavigateApp_ShowHome(object sender, RoutedEventArgs e)
        {

            try
            {
                StateMachine_PerformTransition(AppNavigationState.AppHomepage);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Cloudbeds client application", MessageBoxButton.OK, MessageBoxImage.Error);
                //Go to the status logs screen
                StateMachine_PerformTransition(AppNavigationState.ShowStatusLogs);
            }

        }

        private void NavigateApp_ShowSetup(object sender, RoutedEventArgs e)
        {
            StateMachine_PerformTransition(AppNavigationState.ShowSetup);
        }

        
        private void NavigateApp_ShowOperationalReport_v1(object sender, RoutedEventArgs e)
        {
            try
            {
                StateMachine_PerformTransition(AppNavigationState.ShowOperationalReport_v1);
            }
            catch (Exception ex) 
            {
                MessageBox.Show("Error: " + ex.Message, "Cloudbeds client application", MessageBoxButton.OK, MessageBoxImage.Error);
                //Go to the status logs screen
                StateMachine_PerformTransition(AppNavigationState.ShowStatusLogs);
            }
        }

        private void NavigateApp_ShowOperationalReport_v2(object sender, RoutedEventArgs e)
        {
            try
            {
                StateMachine_PerformTransition(AppNavigationState.ShowOperationalReport_v2);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Cloudbeds client application", MessageBoxButton.OK, MessageBoxImage.Error);
                //Go to the status logs screen
                StateMachine_PerformTransition(AppNavigationState.ShowStatusLogs);
            }
        }
        /// <summary>
        /// Inbound calls from our background thread.  Called when Cloudbeds cache data was refreshed
        /// </summary>
        void IDataRefreshNotify.DataRefreshOccured()
        {
            this.Dispatcher.Invoke(CrossThreadDataRefreshOccured_UIThread, null);
        }

        /// <summary>
        /// Inbound calls from our background thread.  Called as a fairly frequent heartbeat
        /// </summary>
        void IDataRefreshNotify.DataRefreshHeartbeat()
        {
            this.Dispatcher.Invoke(CrossThreadDataRefreshHeartbeat_UIThread, null);
        }

        /// <summary>
        /// This will run on the UI thread (called by ICrossThreadRefreshNotfy.CrossThreadDataRefreshOccured)
        /// </summary>
        private void CrossThreadDataRefreshHeartbeat_UIThread()
        {
            UpdateSystemStatus("Background heartbeat");
        }

        /// <summary>
        /// This will run on the UI thread (called by IDataRefreshNotify.DataRefreshOccured)
        /// </summary>
        private void CrossThreadDataRefreshOccured_UIThread()
        {
            UpdateSystemStatus("Refresh occured");

            CrossThreadDataRefreshOccured_UIThread_updateUiChildren();
        }

        /// <summary>
        /// Update UI elements to let them know there is new data
        /// </summary>
        private void CrossThreadDataRefreshOccured_UIThread_updateUiChildren()
        {
            //Catch any errors - we don't want the UI to halt if there is a UI update bug
            try
            {
                //Look at any of our main UI elements and see if they want notifications
                //when the data is updated
                foreach(var thisUiElement in mainContentArea.Children)
                {
                    var asRequestUiDataRefresh = thisUiElement as IRequestUiDataRefresh;
                    if(asRequestUiDataRefresh != null)
                    {
                        asRequestUiDataRefresh.RefreshUiFromData();
                    }
                }
            }
            catch(Exception ex)
            {
                IwsDiagnostics.Assert(false, "0204-1024: Unexpected error updating UI after data refresh");
                CloudbedsSingletons.StatusLogs.AddError("0204-1024: Unexpected error updating UI after data refresh");
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            //Shut down the refresh timer...
            CloudbedsSingletons.RefreshScheduler.RefreshEnabled = false;
        }
    }
}
