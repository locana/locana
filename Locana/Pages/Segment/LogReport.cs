using Locana.Controls;
using Locana.DataModel;
using Locana.Utility;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Email;
using Windows.Security.ExchangeActiveSyncProvisioning;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.System.Profile;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Locana.Pages.Segment
{
    public class LogReport
    {
        public void Setup(CoreDispatcher dispatcher)
        {
            Dispatcher = dispatcher;
        }

        private CoreDispatcher Dispatcher;
        private ContentDialog DebugLogDialog;

        private IReadOnlyList<StorageFile> logFiles;

        public async void LoadLogFiles()
        {
            logFiles = await DebugUtil.LogFiles();
        }

        private async void ShowToast(string message)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                AppShell.Current.Toast.PushToast(new ToastContent() { Text = message });
            });
        }

        private ContentDialogInfo dialogInfo = new ContentDialogInfo();

        public void DebugLogToggle_Loaded(object sender, RoutedEventArgs e)
        {
            var data = new AppSettingData<bool>()
            {
                Title = SystemUtil.GetStringResource("DebugLog_Setting_Title"),
                Guide = SystemUtil.GetStringResource("DebugLog_Setting_Guide")
            };
            data.StateProvider = () => ApplicationSettings.GetInstance().EnableDebugLogging;
            data.StateObserver = async (enabled) =>
            {
                ApplicationSettings.GetInstance().EnableDebugLogging = enabled;
                if (enabled)
                {
                    if (logFiles.Count != 0)
                    {
                        dialogInfo.DescriptionId = "DebugLog_DetectPreviousDialog_Description";
                        dialogInfo.PrimaryTextId = "DebugLog_DetectPreviousDialog_Deny";
                        dialogInfo.SecondaryTextId = "DebugLog_DetectPreviousDialog_Accept";
                        switch (await DebugLogDialog.ShowAsync())
                        {
                            case ContentDialogResult.Primary:
                                foreach (var file in logFiles)
                                {
                                    await file.DeleteAsync();
                                }
                                LoadLogFiles();
                                break;
                            case ContentDialogResult.Secondary: // Previous operation may be finished abnormally.
                                await DebugUtil.ZipLogFileDir();
                                foreach (var file in logFiles)
                                {
                                    await file.DeleteAsync();
                                }
                                LoadLogFiles();
                                await SendLogFile(await DebugUtil.LatestLogFile());
                                break;
                            default:
                                break;
                        }
                        ApplicationSettings.GetInstance().EnableDebugLogging = false;
                        data.CurrentSetting = false;
                        return;
                    }

                    dialogInfo.DescriptionId = "DebugLog_EnableDialog_Description";
                    dialogInfo.PrimaryTextId = "DebugLog_EnableDialog_Deny";
                    dialogInfo.SecondaryTextId = "DebugLog_EnableDialog_Accept";
                    switch (await DebugLogDialog.ShowAsync())
                    {
                        case ContentDialogResult.Secondary:
                            break;
                        case ContentDialogResult.Primary:
                        default:
                            ApplicationSettings.GetInstance().EnableDebugLogging = false;
                            data.CurrentSetting = false;
                            return;
                    }

                    await DebugUtil.GrubFile();
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
                        LoadLogFiles();

                        await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                        {
                            dialogInfo.DescriptionId = "DebugLog_LaunchEmailDialog_Description";
                            dialogInfo.PrimaryTextId = "DebugLog_LaunchEmailDialog_Deny";
                            dialogInfo.SecondaryTextId = "DebugLog_LaunchEmailDialog_Accept";
                            switch (await DebugLogDialog.ShowAsync())
                            {
                                case ContentDialogResult.Secondary:
                                    await SendLogFile(await DebugUtil.LatestLogFile());
                                    break;
                                case ContentDialogResult.Primary:
                                default:
                                    ApplicationSettings.GetInstance().EnableDebugLogging = false;
                                    data.CurrentSetting = false;
                                    return;
                            }
                        });
                    });
                }
            };
            (sender as ToggleSetting).SettingData = data;
        }

        private static async Task SendLogFile(StorageFile attachment)
        {
            if (attachment == null) { return; }

            var sv = ulong.Parse(AnalyticsInfo.VersionInfo.DeviceFamilyVersion);
            var v1 = (sv & 0xFFFF000000000000L) >> 48;
            var v2 = (sv & 0x0000FFFF00000000L) >> 32;
            var v3 = (sv & 0x00000000FFFF0000L) >> 16;
            var v4 = (sv & 0x000000000000FFFFL);
            var svs = $"{v1}.{v2}.{v3}.{v4}";
            var eas = new EasClientDeviceInformation();

            var pv = Package.Current.Id.Version;
            var appVer = $"{pv.Major}.{pv.Minor}.{pv.Build}.{pv.Revision}";

            var email = new EmailMessage();
            email.To.Add(new EmailRecipient("locana.dev@gmail.com"));
            email.Subject = "Locana log reoprt";
            email.Body = string.Format("[System information]\nSystem family: {0}\nOS build number: {1}\nArchitecture: {2}\nManufacturer: {3}\nDevice model: {4}\nApp version: {5}",
                AnalyticsInfo.VersionInfo.DeviceFamily,
                svs,
                Package.Current.Id.Architecture.ToString(),
                eas.SystemManufacturer,
                eas.SystemProductName,
                appVer);

            using (var data = await attachment.OpenReadAsync())
            {
                email.Attachments.Add(new EmailAttachment("log_file.zip", RandomAccessStreamReference.CreateFromFile(attachment)));
            }
            await EmailManager.ShowComposeNewEmailAsync(email);
        }

        public void Pivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var pivot = sender as Pivot;
            if (pivot.SelectedIndex == 2)
            {
                LoadLogFiles();
            }
        }

        public void DebugLogDialog_Loaded(object sender, RoutedEventArgs e)
        {
            DebugLogDialog = sender as ContentDialog;
            DebugLogDialog.DataContext = dialogInfo;
        }

        public void DebugLogToggle_Unloaded(object sender, RoutedEventArgs e)
        {

        }

        public void DebugLogDialog_Unloaded(object sender, RoutedEventArgs e)
        {
            DebugLogDialog = null;
        }
    }
}
