using Locana.Utility;
using System;
using System.Reflection;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Store;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Navigation;

namespace Locana
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : Application
    {
        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            Microsoft.ApplicationInsights.WindowsAppInitializer.InitializeAsync(
                Microsoft.ApplicationInsights.WindowsCollectors.Metadata |
                Microsoft.ApplicationInsights.WindowsCollectors.Session);
            this.InitializeComponent();
            this.Suspending += OnSuspending;

            Kazyx.DeviceDiscovery.SsdpDiscovery.Logger = (msg) => DebugUtil.Log(() => msg);
            Kazyx.ImageStream.StreamProcessor.Logger = (msg) => DebugUtil.Log(() => msg);
            Kazyx.RemoteApi.Util.RemoteApiLogger.Logger = (msg) => DebugUtil.Log(() => msg);
            // Kazyx.RemoteApi.Util.RemoteApiLogger.VerboseLogger = (msg) => DebugUtil.Log(() => msg);
        }

        public bool IsFunctionLimited
        {
            private set;
            get;
        }

        public bool IsTrialVersion
        {
            private set;
            get;
        }

        public string AppVersion
        {
            private set;
            get;
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {

#if DEBUG
            if (System.Diagnostics.Debugger.IsAttached)
            {
                this.DebugSettings.EnableFrameRateCounter = true;
            }
#endif
            var assembly = (typeof(App)).GetTypeInfo().Assembly;
            AppVersion = assembly.GetName().Version.ToString();

            var lastVersion = Preference.LastLaunchedVersion;
            if (lastVersion != AppVersion)
            {
                DebugUtil.Log(() => "Update detected!! from: " + lastVersion);
                Preference.LastLaunchedVersion = AppVersion;
                Preference.InitialLaunchedDateTime = DateTimeOffset.Now;
            }

            UpdatePurchaseInfo();

            AppShell shell = Window.Current.Content as AppShell;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (shell == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                shell = new AppShell();


                shell.AppFrame.NavigationFailed += OnNavigationFailed;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //TODO: Load state from previously suspended application
                }

                // Place the frame in the current Window
                Window.Current.Content = shell;
            }

            if (shell.AppFrame.Content == null)
            {
                // When the navigation stack isn't restored navigate to the first page,
                // configuring the new page by passing required information as a navigation
                // parameter
                shell.AppFrame.Navigate(typeof(Pages.EntrancePage), e.Arguments);
            }
            // Ensure the current window is active
            Window.Current.Activate();
        }

        public void UpdatePurchaseInfo()
        {
            var init = Preference.InitialLaunchedDateTime;
            DebugUtil.Log(() => "Initial launched datetime: " + init.ToString());
#if DEBUG
            IsTrialVersion = CurrentAppSimulator.LicenseInformation.IsTrial;
#else
            try
            {
                IsTrialVersion = CurrentApp.LicenseInformation.IsTrial;
            }
            catch
            {
                IsTrialVersion = true;
            }
#endif
            if (IsTrialVersion)
            {
                var diff = DateTimeOffset.Now.Subtract(init);
                IsFunctionLimited = diff.Days > 30;
            }
            else
            {
                IsFunctionLimited = false;
            }

            DebugUtil.Log(() => string.Format("Trial version: {0}", IsTrialVersion));
            DebugUtil.Log(() => string.Format("Function limited: {0}", IsFunctionLimited));
        }

        /// <summary>
        /// Invoked when Navigation to a certain page fails
        /// </summary>
        /// <param name="sender">The Frame which failed navigation</param>
        /// <param name="e">Details about the navigation failure</param>
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            //TODO: Save application state and stop any background activity
            deferral.Complete();
        }
    }
}
