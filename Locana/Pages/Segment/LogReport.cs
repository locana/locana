using Locana.Controls;
using Locana.DataModel;
using Locana.Utility;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.ApplicationModel.Email;
using Windows.Storage;
using Windows.Storage.Streams;
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
            DebugUtil.Log(() => message);
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

            var email = new EmailMessage();
            email.To.Add(new EmailRecipient("naotaco@gmail.com"));
            email.Subject = "Log file from Locana";
            email.Body = "See attachment.";
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
