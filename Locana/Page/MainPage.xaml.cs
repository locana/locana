using Kazyx.ImageStream;
using Kazyx.Uwpmm.CameraControl;
using Kazyx.Uwpmm.DataModel;
using Kazyx.Uwpmm.Settings;
using Kazyx.Uwpmm.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Locana
{
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            NetworkObserver.INSTANCE.CameraDiscovered += NetworkObserver_Discovered;
            NetworkObserver.INSTANCE.ForceRestart();
            MediaDownloader.Instance.Fetched += OnFetchdImage;
        }

        private void OnFetchdImage(StorageFolder arg1, StorageFile arg2, GeotaggingResult arg3)
        {
            throw new NotImplementedException();
        }

        private TargetDevice target;
        private StreamProcessor liveview = new StreamProcessor();
        private ImageDataSource liveview_data = new ImageDataSource();
        private ImageDataSource postview_data = new ImageDataSource();

        LiveviewScreenViewData ScreenViewData;

        async void NetworkObserver_Discovered(object sender, CameraDeviceEventArgs e)
        {
            var target = e.CameraDevice;
            try
            {
                await SequentialOperation.SetUp(target, liveview);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Failed setup: " + ex.Message);
                return;
            }

            this.target = target;
            target.Status.PropertyChanged += Status_PropertyChanged;

            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                ScreenViewData = new LiveviewScreenViewData(target);
                ScreenViewData.NotifyFriendlyNameUpdated();
                // BatteryStatusDisplay.BatteryInfo = target.Status.BatteryInfo;
                LayoutRoot.DataContext = ScreenViewData;
                var panels = SettingPanelBuilder.CreateNew(target);
                var pn = panels.GetPanelsToShow();
                foreach (var panel in pn)
                {
                    // ControlPanel.Children.Add(panel);
                }
            });
        }


        void Status_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            var status = sender as CameraStatus;
            switch (e.PropertyName)
            {
                default:
                    break;
            }
        }

        private bool IsRendering = false;

        async void liveview_JpegRetrieved(object sender, JpegEventArgs e)
        {
            if (IsRendering) { return; }

            IsRendering = true;
            await LiveviewUtil.SetAsBitmap(e.Packet.ImageData, liveview_data, null, Dispatcher);
            IsRendering = false;
        }

        void liveview_Closed(object sender, EventArgs e)
        {
            Debug.WriteLine("Liveview connection closed");
        }

        private void LiveviewImage_Loaded(object sender, RoutedEventArgs e)
        {
            var image = sender as Image;
            image.DataContext = liveview_data;
            liveview.JpegRetrieved += liveview_JpegRetrieved;
            liveview.Closed += liveview_Closed;
        }

        private void LiveviewImage_Unloaded(object sender, RoutedEventArgs e)
        {
            var image = sender as Image;
            image.DataContext = null;
            liveview.JpegRetrieved -= liveview_JpegRetrieved;
            liveview.Closed -= liveview_Closed;
            TearDownCurrentTarget();
        }

        private void TearDownCurrentTarget()
        {
            LayoutRoot.DataContext = null;
        }

        async void ShutterButtonPressed()
        {
            await SequentialOperation.StartStopRecording(
                new List<TargetDevice> { target },
                (result) =>
                {
                    switch (result)
                    {
                        case SequentialOperation.ShootingResult.StillSucceed:
                            ShowToast(SystemUtil.GetStringResource("Message_ImageCapture_Succeed"));                            
                            break;
                        case SequentialOperation.ShootingResult.StartSucceed:
                        case SequentialOperation.ShootingResult.StopSucceed:
                            break;
                        case SequentialOperation.ShootingResult.StillFailed:
                        case SequentialOperation.ShootingResult.StartFailed:
                            ShowError(SystemUtil.GetStringResource("ErrorMessage_shootingFailure"));
                            break;
                        case SequentialOperation.ShootingResult.StopFailed:
                            ShowError(SystemUtil.GetStringResource("ErrorMessage_fatal"));
                            break;
                        default:
                            break;
                    }
                });
        }

        private void ShowToast(string v)
        {
            Debug.WriteLine("toast: " + v);
        }

        private void ShowError(string v)
        {
            Debug.WriteLine("error: " + v);
        }

        private void ShutterButon_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            ShutterButtonPressed();
        }
    }
}
