using Locana.Controls;
using Locana.DataModel;
using Locana.Utility;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Email;
using Windows.ApplicationModel.Store;
using Windows.Storage;
using Windows.Storage.Streams;
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
        public AboutPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            UpdatePurchaseInformation();
        }

        private void UpdatePurchaseInformation()
        {
            var app = Application.Current as App;
            Unlimited.Visibility = (!app.IsFunctionLimited && !app.IsTrialVersion).AsVisibility();
            Trial.Visibility = (app.IsTrialVersion).AsVisibility();
            Limited.Visibility = app.IsFunctionLimited.AsVisibility();
            TrialButton.Visibility = (app.IsFunctionLimited || app.IsTrialVersion).AsVisibility();
        }

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

        private ObservableCollection<string> logDisplayList = new ObservableCollection<string>();
        private IReadOnlyList<StorageFile> logFiles;

        private async void LoadLogFiles()
        {
            logFiles = await DebugUtil.LogFiles();

            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                foreach (var file in logFiles)
                {
                    logDisplayList.Add(string.Format("{0}: {1} Bytes", file.Name, (await file.GetBasicPropertiesAsync()).Size));
                }
            });
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
            DebugUtil.Log(() => message);
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                AppShell.Current.Toast.PushToast(new ToastContent() { Text = message });
            });
        }

        private void LogFiles_Loaded(object sender, RoutedEventArgs e)
        {
            LogFiles.ItemsSource = logDisplayList;
        }

        private void DebugLogToggle_Loaded(object sender, RoutedEventArgs e)
        {
            var data = new AppSettingData<bool>()
            {
                Title = "Save debug log file",
                Guide = "Turn on to start writing log file, turn off to stop and attach file to the email."
            };
            data.StateProvider = () => ApplicationSettings.GetInstance().EnableDebugLogging;
            data.StateObserver = async (enabled) =>
            {
                ApplicationSettings.GetInstance().EnableDebugLogging = enabled;
                if (enabled)
                {

                    if (logDisplayList.Count != 0)
                    {
                        var res = await DebugLogDialog.ShowAsync();
                        switch (res)
                        {
                            case ContentDialogResult.Primary:
                                ApplicationSettings.GetInstance().EnableDebugLogging = false;
                                data.CurrentSetting = false;
                                await SendLogFile(await DebugUtil.LatestLogFile());
                                return;
                            case ContentDialogResult.Secondary:
                                foreach (var file in logFiles)
                                {
                                    await file.DeleteAsync();
                                }
                                break;
                            default:
                                ApplicationSettings.GetInstance().EnableDebugLogging = false;
                                data.CurrentSetting = false;
                                return;
                        }
                    }
                    await DebugUtil.GrubFile();
                    logDisplayList.Clear();
                    LoadLogFiles();
                }
                else
                {
                    if (!DebugUtil.ReleaseFile())
                    {
                        return;
                    }
                    var task = Task.Run(async () =>
                    {
                        await DebugUtil.ZipLogFileDir();
                        foreach (var file in logFiles)
                        {
                            await file.DeleteAsync();
                        }
                        await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                        {
                            await SendLogFile(await DebugUtil.LatestLogFile());
                        });
                    });
                }
            };
            DebugLogToggle.SettingData = data;
        }

        private static async Task SendLogFile(StorageFile attachment)
        {
            if (attachment == null) { return; }

            EmailMessage email = new EmailMessage();
            email.To.Add(new EmailRecipient("naotaco@gmail.com"));
            email.Subject = "Log file from Locana";
            email.Body = "See attachment.";
            using (var data = await attachment.OpenReadAsync())
            {
                email.Attachments.Add(new EmailAttachment("log_file.zip", RandomAccessStreamReference.CreateFromFile(attachment)));
            }
            await EmailManager.ShowComposeNewEmailAsync(email);
        }

        private void Pivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var pivot = sender as Pivot;
            if (pivot.SelectedIndex == 2)
            {
                logDisplayList.Clear();
                LoadLogFiles();
            }
        }
    }
}
