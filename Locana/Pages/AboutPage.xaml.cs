using Locana.Common;
using Locana.Controls;
using Locana.Utility;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Reflection;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Store;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace Locana.Pages
{
    public sealed partial class AboutPage : Page
    {
        private NavigationHelper navigationHelper;

        public AboutPage()
        {
            this.InitializeComponent();

            this.navigationHelper = new NavigationHelper(this);
        }

        /// <summary>
        /// Gets the <see cref="NavigationHelper"/> associated with this <see cref="Page"/>.
        /// </summary>
        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }

        #region NavigationHelper registration

        /// <summary>
        /// The methods provided in this section are simply used to allow
        /// NavigationHelper to respond to the page's navigation methods.
        /// <para>
        /// Page specific logic should be placed in event handlers for the  
        /// <see cref="NavigationHelper.LoadState"/>
        /// and <see cref="NavigationHelper.SaveState"/>.
        /// The navigation parameter is available in the LoadState method 
        /// in addition to page state preserved during an earlier session.
        /// </para>
        /// </summary>
        /// <param name="e">Provides data for navigation methods and event
        /// handlers that cannot cancel the navigation request.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedTo(e);

            UpdatePurchaseInformation();
        }

        private void UpdatePurchaseInformation()
        {
            var app = Application.Current as App;
            Unlimited.Visibility = (app.IsFunctionLimited || app.IsTrialVersion).AsReverseVisibility();
            Trial.Visibility = (app.IsTrialVersion).AsVisibility();
            Limited.Visibility = app.IsFunctionLimited.AsVisibility();
            TrialButton.Visibility = (app.IsFunctionLimited || app.IsTrialVersion).AsVisibility();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedFrom(e);
        }

        #endregion

        private static bool IsManifestLoaded = false;
        private static LicenseJson license;
        private static string copyright = "";

        CommandBarManager CommandBarManager = new CommandBarManager();

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (!IsManifestLoaded)
            {
                LoadAssemblyInformation();
            }
            VERSION_STR.Text = string.Format(SystemUtil.GetStringResource("VersionNumber"), (Application.Current as App).AppVersion);

            COPYRIGHT.Text = copyright;

            DEV_BY.Inlines.Add(GetAsLink("kazyx", "https://github.com/kazyx"));
            DEV_BY.Inlines.Add(new Run() { Text = " and ", Foreground = (Brush)Resources["ApplicationSecondaryForegroundThemeBrush"] });
            DEV_BY.Inlines.Add(GetAsLink("naotaco", "https://twitter.com/naotaco_dev"));

            FaqLink.Inlines.Add(GetAsLink(SystemUtil.GetStringResource("OpenFAQ"), SystemUtil.GetStringResource("FAQURL")));
            SupportLink.Inlines.Add(GetAsLink(SystemUtil.GetStringResource("OpenSupportTwitter"), SystemUtil.GetStringResource("SupportTwitterURL")));
            RepoLink.Inlines.Add(GetAsLink(SystemUtil.GetStringResource("OpenGithub"), SystemUtil.GetStringResource("RepoURL")));

            LoadLicenseFile();
        }

        private static void LoadAssemblyInformation()
        {
            var assembly = (typeof(App)).GetTypeInfo().Assembly;
            foreach (var attr in assembly.CustomAttributes)
            {
                if (attr.AttributeType == typeof(AssemblyCopyrightAttribute))
                {
                    copyright = attr.ConstructorArguments[0].Value.ToString();
                    break;
                }
            }
        }

        private async void LoadLicenseFile()
        {
            if (license == null)
            {
                var installedFolder = Package.Current.InstalledLocation;
                var folder = await installedFolder.GetFolderAsync("Assets");
                var file = await folder.GetFileAsync("License.txt");
                using (var stream = await file.OpenReadAsync())
                {
                    using (var reader = new StreamReader(stream.AsStreamForRead()))
                    {
                        license = JsonConvert.DeserializeObject<LicenseJson>(reader.ReadToEnd());
                    }
                }
            }

            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                foreach (var oss in license.OssList)
                {
                    Contents.Inlines.Add(new Run() { Text = oss.Name, FontSize = 18 });
                    Contents.Inlines.Add(new LineBreak());
                    Contents.Inlines.Add(GetAsLink(oss.License, oss.Url));
                    Contents.Inlines.Add(new LineBreak());
                    Contents.Inlines.Add(new LineBreak());
                };
            });
        }

        private Hyperlink GetAsLink(string word, string link = null)
        {
            var hl = new Hyperlink
            {
                NavigateUri = new Uri(link == null ? word : link),
                Foreground = (SolidColorBrush)(Resources["SystemControlForegroundAccentBrush"]),
            };

            hl.Inlines.Add(new Run()
            {
                Text = word
            });

            return hl;
        }

        private async void TrialButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
#if DEBUG
                await CurrentAppSimulator.RequestAppPurchaseAsync(false);
#else
                await CurrentApp.RequestAppPurchaseAsync(false);
#endif
            }
            catch
            {
                ShowToast(SystemUtil.GetStringResource("ErrorMessage_fatal"));
                return;
            }

            (Application.Current as App).UpdatePurchaseInfo();
            UpdatePurchaseInformation();
        }

        private async void ShowToast(string message)
        {
            DebugUtil.Log(message);
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                AppShell.Current.Toast.PushToast(new ToastContent() { Text = message });
            });
        }
    }
}