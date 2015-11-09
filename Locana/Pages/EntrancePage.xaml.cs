using Kazyx.Uwpmm.Utility;
using Locana.DataModel;
using Naotaco.Nfc;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Windows.ApplicationModel.DataTransfer;
using Windows.Data.Xml.Dom;
using Windows.Networking.Proximity;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Notifications;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Locana.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class EntrancePage : Page
    {
        public EntrancePage()
        {
            this.InitializeComponent();

            appMenuGroup.Add(new EntrancePanel(SystemUtil.GetStringResource("AppBar_AppSetting"), () =>
            {
                Frame.Navigate(typeof(AppSettingPage));
            }));
            appMenuGroup.Add(new EntrancePanel(SystemUtil.GetStringResource("WifiSettingLauncherButtonText"), () =>
            {
            }));
            appMenuGroup.Add(new EntrancePanel(SystemUtil.GetStringResource("Donation"), () =>
            {
            }));
            appMenuGroup.Add(new EntrancePanel(SystemUtil.GetStringResource("About"), () =>
            {
            }));

            panelSource.Add(devicesGroup);
            panelSource.Add(appMenuGroup);
        }

        private EntrancePanelGroupCollection panelSource = new EntrancePanelGroupCollection();

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            NetworkObserver.INSTANCE.CameraDiscovered += NetworkObserver_Discovered;
            NetworkObserver.INSTANCE.ForceRestart();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            NetworkObserver.INSTANCE.CameraDiscovered -= NetworkObserver_Discovered;

            base.OnNavigatedFrom(e);
        }

        private EntrancePanelGroup appMenuGroup = new EntrancePanelGroup(SystemUtil.GetStringResource("PanelGroup_AppMenu"));

        private EntrancePanelGroup devicesGroup = new EntrancePanelGroup(SystemUtil.GetStringResource("PanelGroup_Devices"));

        private void EntranceGrid_Loaded(object sender, RoutedEventArgs e)
        {
            devicesGroup.Clear();
            foreach (var device in NetworkObserver.INSTANCE.CameraDevices)
            {
                devicesGroup.Add(new DevicePanel(device));
            }

            PanelSources.Source = panelSource;
        }

        private void EntranceGrid_Unloaded(object sender, RoutedEventArgs e)
        {
            PanelSources.Source = null;
        }

        async void NetworkObserver_Discovered(object sender, CameraDeviceEventArgs e)
        {
            var target = e.CameraDevice;

            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                devicesGroup.Add(new DevicePanel(target));
            });
        }

        private void PanelHolder_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            var grid = sender as Grid;
            var content = grid.DataContext as EntrancePanel;
            content.OnClick();
        }

        private ToastNotification BuildToast(string str, StorageFile file = null)
        {
            ToastTemplateType template = ToastTemplateType.ToastImageAndText01;
            XmlDocument toastXml = ToastNotificationManager.GetTemplateContent(template);
            XmlNodeList toastTextElements = toastXml.GetElementsByTagName("text");
            toastTextElements[0].AppendChild(toastXml.CreateTextNode(str));

            var toastImageAttributes = toastXml.GetElementsByTagName("image");

            if (file == null)
            {
                ((XmlElement)toastImageAttributes[0]).SetAttribute("src", "ms-appx:///Assets/Toast/Locana_square_full.png");
            }
            else
            {
                ((XmlElement)toastImageAttributes[0]).SetAttribute("src", file.Path);
            }
            return new ToastNotification(toastXml);
        }

        private void ShowToast(string str, StorageFile file = null)
        {
            Debug.WriteLine("toast with image: " + str);
            var toast = BuildToast(str, file);
            ToastNotificationManager.CreateToastNotifier().Show(toast);
        }

        private void ShowError(string v)
        {
            Debug.WriteLine("error: " + v);
            ShowToast(v);
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            InitializeProximityDevice();
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            StopProximityDevice();
        }

        ProximityDevice _ProximityDevice;
        long ProximitySubscribeId;

        private void InitializeProximityDevice()
        {
            StopProximityDevice();

            try
            {
                _ProximityDevice = ProximityDevice.GetDefault();
            }
            catch (FileNotFoundException)
            {
                _ProximityDevice = null;
                DebugUtil.Log("Caught ununderstandable exception. ");
                return;
            }
            catch (COMException)
            {
                _ProximityDevice = null;
                DebugUtil.Log("Caught ununderstandable exception. ");
                return;
            }

            if (_ProximityDevice == null)
            {
                DebugUtil.Log("It seems this is not NFC available device");
                return;
            }

            try
            {
                ProximitySubscribeId = _ProximityDevice.SubscribeForMessage("NDEF", ProximityMessageReceivedHandler);
            }
            catch (Exception e)
            {
                _ProximityDevice = null;
                DebugUtil.Log("Caught ununderstandable exception. " + e.Message + e.StackTrace);
                return;
            }
        }

        private void StopProximityDevice()
        {
            if (_ProximityDevice != null)
            {
                _ProximityDevice.StopSubscribingForMessage(ProximitySubscribeId);
                _ProximityDevice = null;
            }
        }

        private async void ProximityMessageReceivedHandler(ProximityDevice sender, ProximityMessage message)
        {
            var parser = new NdefParser(message);
            var ndefRecords = new List<NdefRecord>();

            var err = "";

            try { ndefRecords = parser.Parse(); }
            catch (NoSonyNdefRecordException) { err = SystemUtil.GetStringResource("ErrorMessage_CantFindSonyRecord"); }
            catch (NoNdefRecordException) { err = SystemUtil.GetStringResource("ErrorMessage_ParseNFC"); }
            catch (NdefParseException) { err = SystemUtil.GetStringResource("ErrorMessage_ParseNFC"); }
            catch (Exception) { err = SystemUtil.GetStringResource("ErrorMessage_fatal"); }

            if (err != "")
            {
                DebugUtil.Log("Failed to read NFC: " + err);
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => { ShowError(err); });
                return;
            }

            foreach (NdefRecord r in ndefRecords)
            {
                if (r.SSID.Length > 0 && r.Password.Length > 0)
                {
                    await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                    {
                        var sb = new StringBuilder();
                        sb.Append(SystemUtil.GetStringResource("Message_NFC_succeed"));
                        sb.Append(System.Environment.NewLine);
                        sb.Append(System.Environment.NewLine);
                        sb.Append("SSID: ");
                        sb.Append(r.SSID);
                        sb.Append(System.Environment.NewLine);
                        sb.Append("Password: ");
                        sb.Append(r.Password);
                        sb.Append(System.Environment.NewLine);

                        PutToClipBoard(r.Password);

                        var dialog = new MessageDialog(sb.ToString());
                        try
                        {
                            await dialog.ShowAsync();
                        }
                        catch (UnauthorizedAccessException) {/* Duplicated message dialog */}
                    });
                    break;
                }
            }
        }

        void PutToClipBoard(string s)
        {
            var package = new DataPackage();
            package.RequestedOperation = DataPackageOperation.Copy;
            package.SetText(s);
            Clipboard.SetContent(package);
        }
    }
}
