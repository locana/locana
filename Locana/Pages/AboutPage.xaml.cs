using Locana.Common;
using Locana.Utility;
using System;
using System.IO;
using System.Reflection;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Store;
using Windows.System;
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

            CommandBarManager.SetEvent(AppBarItem.WifiSetting, async (s, args) =>
            {
                await Launcher.LaunchUriAsync(new Uri("ms-settings-wifi:"));
            });
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

            if ((App.Current as App).IsFunctionLimited)
            {
                Unlimited.Visibility = Visibility.Collapsed;
                Trial.Visibility = Visibility.Collapsed;
                Limited.Visibility = Visibility.Visible;
                TrialButton.Visibility = Visibility.Visible;
            }
            else if ((App.Current as App).IsTrialVersion)
            {
                Unlimited.Visibility = Visibility.Collapsed;
                Trial.Visibility = Visibility.Visible;
                Limited.Visibility = Visibility.Collapsed;
                TrialButton.Visibility = Visibility.Visible;
            }
            else
            {
                Unlimited.Visibility = Visibility.Visible;
                Trial.Visibility = Visibility.Collapsed;
                Limited.Visibility = Visibility.Collapsed;
                TrialButton.Visibility = Visibility.Collapsed;
            }

            CommandBarManager.Clear().Command(AppBarItem.WifiSetting).ApplyAll(AppBarUnit);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedFrom(e);
        }

        #endregion

        private static bool IsManifestLoaded = false;
        private static string license = "";
        private static string copyright = "";
        private const string developer = "kazyx and naotaco (@naotaco_dev)";

        CommandBarManager CommandBarManager = new CommandBarManager();

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (!IsManifestLoaded)
            {
                LoadAssemblyInformation();
            }
            VERSION_STR.Text = (App.Current as App).AppVersion;

            COPYRIGHT.Text = copyright;

            DEV_BY.Text = developer;

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
            if (string.IsNullOrEmpty(license))
            {
                var installedFolder = Package.Current.InstalledLocation;
                var folder = await installedFolder.GetFolderAsync("Assets");
                var file = await folder.GetFileAsync("License.txt");
                var stream = await file.OpenReadAsync();
                var reader = new StreamReader(stream.AsStreamForRead());
                license = reader.ReadToEnd();
                license = license.Replace("\r\n", "\n").Replace("\r", "\n").Replace("\n", "\r\n"); // Avoid autocrlf effect
            }
            await SystemUtil.GetCurrentDispatcher().RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                FormatRichText(Contents, license);
            });
        }

        private static void FormatRichText(Paragraph place, string text)
        {
            if (text != null && text.Length != 0)
            {
                char[] separators = { ' ', '\n', '\t', '　' };
                var words = text.Split(separators);
                foreach (var word in words)
                {
                    if (word.StartsWith("http://") || word.StartsWith("https://"))
                    {
                        place.Inlines.Add(GetAsLink(word));
                        place.Inlines.Add(new Run()
                        {
                            Text = " ",
                        });
                    }
                    else
                    {
                        place.Inlines.Add(new Run()
                        {
                            Text = word + " ",
                        });
                    }
                }
            }
        }

        private static Hyperlink GetAsLink(string word)
        {
            var hl = new Hyperlink
            {
                NavigateUri = new Uri(word),
                Foreground = (SolidColorBrush)(Application.Current.Resources["ProgressBarForegroundThemeBrush"]),
            };

            hl.Inlines.Add(new Run()
            {
                Text = word
            });

            return hl;
        }

        private async void SourceCode_Click(object sender, RoutedEventArgs e)
        {
            var success = await Launcher.LaunchUriAsync(new Uri(SystemUtil.GetStringResource("RepoURL")));
            if (!success) DebugUtil.Log("Failed to open Github page.");
        }

        private async void FAQ_Click(object sender, RoutedEventArgs e)
        {
            var success = await Launcher.LaunchUriAsync(new Uri(SystemUtil.GetStringResource("FAQURL")));
            if (!success) DebugUtil.Log("Failed to open FAQ page.");
        }

        private async void Support_Click(object sender, RoutedEventArgs e)
        {
            var success = await Launcher.LaunchUriAsync(new Uri(SystemUtil.GetStringResource("SupportTwitterURL")));
            if (!success) DebugUtil.Log("Failed to open Support page.");
        }

        private async void TrialButton_Click(object sender, RoutedEventArgs e)
        {
            await Launcher.LaunchUriAsync(CurrentApp.LinkUri);
        }
    }
}