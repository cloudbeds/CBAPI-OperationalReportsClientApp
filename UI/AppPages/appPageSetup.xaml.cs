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
    public partial class appPageSetup : StackPanel, IRequestUiDataRefresh
    {

        /// <summary>
        /// Constructor
        /// </summary>
        public appPageSetup()
        {
            InitializeComponent();

            UpdateConfigPathsUI();
        }

        private void UpdateConfigPathsUI()
        {
            txtConfigPath.Text = AppSettings.LoadPreference_PathAppSecretsConfig();
            txtSecretsPath.Text = AppSettings.LoadPreference_PathUserAccessTokens();
        }


        /// <summary>
        /// </summary>
        void IRequestUiDataRefresh.RefreshUiFromData()
        {
         //   FillStatusLogsUi();
        }

        /// <summary>
        /// Edit the path to the config file
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonEditConfigPath_Click(object sender, RoutedEventArgs e)
        {
            var fileDialog = new Microsoft.Win32.OpenFileDialog();
            var startingFilePath = AppSettings.LoadPreference_PathAppSecretsConfig();

            //If the file exists, start in that dirctory 
            if(System.IO.File.Exists(startingFilePath))
            {
                fileDialog.InitialDirectory = System.IO.Path.GetDirectoryName(startingFilePath);
            }

            fileDialog.DefaultExt = ".xml";
            fileDialog.Filter = "Config file *.xml|*.xml";
            var result = fileDialog.ShowDialog();

            if (result != true)
            {
                return;
            }

            var newFilePath = fileDialog.FileName;
            AppSettings.SavePreference_PathAppSecretsConfig(newFilePath);

            //Update the UI
            UpdateConfigPathsUI();
        }


        /// <summary>
        /// Edit the path to the auth tokens
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonEditAuthTokensPath_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ButtonEditAuthTokensPath_Inner();
            }
            catch (Exception ex)
            { 
            
                MessageBox.Show(
                    "Error: " + ex.Message, 
                    "Error setting auth tokens path", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }


        }

        /// <summary>
        /// Set the auth tokens path
        /// </summary>
        private void ButtonEditAuthTokensPath_Inner()
        {
            var fileDialog = new Microsoft.Win32.OpenFileDialog();
            var startingFilePath = AppSettings.LoadPreference_PathUserAccessTokens();

            //If the file exists, start in that dirctory 
            if (System.IO.File.Exists(startingFilePath))
            {
                fileDialog.InitialDirectory = System.IO.Path.GetDirectoryName(startingFilePath);
            }

            fileDialog.DefaultExt = ".xml";
            fileDialog.Filter = "Auth tokens file *.xml|*.xml";
            var result = fileDialog.ShowDialog();

            if (result != true)
            {
                return;
            }

            var newFilePath = fileDialog.FileName;
            AppSettings.SavePreference_PathUserAccessTokens(newFilePath);

            //Update the UI
            UpdateConfigPathsUI();

        }

        /// <summary>
        /// Generate a auth bootstrap URL
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonStartAuthenticationBootstrap_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ButtonStartAuthenticationBootstrap_Inner();
            }
            catch (Exception ex)
            {

                MessageBox.Show(
                    "Error: " + ex.Message,
                    "Error generating boostrap URL",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

        /// <summary>
        /// Generate the authentication bootstrap URL a user need to navigate to
        /// </summary>
        private void ButtonStartAuthenticationBootstrap_Inner()
        {
            var serverConfig = CloudbedsSingletons.CloudbedsServerInfo as CloudbedsAppConfig;
            var urlBootstrap = CloudbedsUris.UriGenerate_RequestOAuthAccess(serverConfig);

            txtUrlBootstrapAuth.Text = urlBootstrap;

        }

        /// <summary>
        /// Pull the authentication token out from the response URL, and broker it into an authentication token we can store
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonFinishAuthBoostrap_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ButtonFinishAuthBoostrap_Inner();
            }
            catch (Exception ex)
            {

                MessageBox.Show(
                    "Error: " + ex.Message,
                    "Error generating boostrap URL",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }

            txtUrlBootstrapResult.Text = "Completed authentication bootstrap!";
        }

        /// <summary>
        /// Pull the authentication token out from the response URL, and broker it into an authentication token we can store
        /// </summary>
        private void ButtonFinishAuthBoostrap_Inner()
        {
            var statusLogs = CloudbedsSingletons.StatusLogs;
            var filePathAccessTokens = AppSettings.LoadPreference_PathUserAccessTokens();

            var directoryPath = System.IO.Path.GetDirectoryName(filePathAccessTokens);
            if(!System.IO.Directory.Exists(directoryPath))
            {
                throw new Exception("The directory for the access-token secrets does NOT exist:" + directoryPath);
            }


            var serverConfig = CloudbedsSingletons.CloudbedsServerInfo as CloudbedsAppConfig;
            if(serverConfig == null)
            {
                throw new Exception("No web server configuration available. Check if config file exists.");
            }

            //================================================================
            //Parse the URL for the access token
            //================================================================
            var urlResponseParsed = new UrlPartsParse(txtUrlBootstrapResult.Text);

            var urlResponse_accessSecretText = urlResponseParsed.GetParameterValue("code");
            if (string.IsNullOrWhiteSpace(urlResponse_accessSecretText))
            {
                statusLogs.AddError("Response URL does not contain access code");
                return;
            }
            var oauthBootstrapCode = new OAuth_BootstrapCode(urlResponse_accessSecretText);


            //===============================================================================
            //Attempt to generate OAUTH ACCESS/REFRESH tokens
            //===============================================================================
            statusLogs.AddStatusHeader("Turn bootstrap into access/refresh codes");
            var cbRequestAccessToken = new CloudbedsRequestOAuthAccessToken(
                serverConfig,
                oauthBootstrapCode,
                statusLogs);

            try
            {
                cbRequestAccessToken.ExecuteRequest();
            }

            catch (Exception exRequestInitialToken)
            {
                statusLogs.AddError("2207-1117: Error executing initial access token request: " + exRequestInitialToken.Message);
                throw;
            }

            IwsDiagnostics.Assert(cbRequestAccessToken.CommandResult_AccessToken != null, "723-003: No access token?");
            IwsDiagnostics.Assert(cbRequestAccessToken.CommandResult_RefreshToken != null, "723-004: No refresh token?");
            statusLogs.AddStatus("Success retreiving initial access/refresh token. Expire in seconds: " + cbRequestAccessToken.CommandResult_ExpiresSeconds.ToString());

            var currentAuthSession = cbRequestAccessToken.CommandResult_CloudbedsAuthSession;

            //=========================================================================
            //Store the auth-session
            //=========================================================================
            CloudbedsSingletons.SetCloudbedsAuthSession(currentAuthSession, true);
        }

        private void ButtonTest1_Click(object sender, RoutedEventArgs e)
        {
            var cbHotelDetails = new CloudbedsRequestHotelDetails(
                CloudbedsSingletons.CloudbedsServerInfo,
                CloudbedsSingletons.CloudbedsAuthSession,
                CloudbedsSingletons.StatusLogs);

            try
            {
                var boolResult = cbHotelDetails.ExecuteRequest();
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Error making request");
            }

        }
    }
}
