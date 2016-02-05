using Locana.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.Sensors;
using Windows.Graphics.Display;
using Windows.Graphics.Imaging;
using Windows.Media;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Windows.Storage.FileProperties;
using Windows.System.Display;
using Windows.System.Profile;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using ZXing;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Locana.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class QrCodePage : Page
    {
        // Codes related to Camera API is from https://github.com/Microsoft/Windows-universal-samples

        [ComImport]
        [Guid("5b0d3235-4dba-4d44-865e-8f1d0e4fd04d")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]

        unsafe interface IMemoryBufferByteAccess
        {
            void GetBuffer(out byte* buffer, out uint capacity);
        }
        public QrCodePage()
        {
            this.InitializeComponent();
        }


        // Receive notifications about rotation of the UI and apply any necessary rotation to the preview stream
        private readonly DisplayInformation _displayInformation = DisplayInformation.GetForCurrentView();

        // Rotation metadata to apply to the preview stream (MF_MT_VIDEO_ROTATION)
        // Reference: http://msdn.microsoft.com/en-us/library/windows/apps/xaml/hh868174.aspx
        private static readonly Guid RotationKey = new Guid("C380465D-2271-428C-9B83-ECEA3B4A85C1");

        // Prevent the screen from sleeping while the camera is running
        private readonly DisplayRequest _displayRequest = new DisplayRequest();

        // For listening to media property changes
        private readonly SystemMediaTransportControls _systemMediaControls = SystemMediaTransportControls.GetForCurrentView();

        // MediaCapture and its state variables
        private MediaCapture _mediaCapture;
        private bool _isInitialized = false;
        private bool _isPreviewing = false;

        private DispatcherTimer CaptureTimer;
        private DispatcherTimer FocusTimer;

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            DisplayInformation.AutoRotationPreferences =
                AnalyticsInfo.VersionInfo.DeviceFamily == DEVICE_FAMILY_MOBILE ? DisplayOrientations.Portrait : DisplayOrientations.Landscape;

            await InitializeCameraAsync();

            if (_mediaCapture == null)
            {
                AppShell.Current.Toast.PushToast(new Controls.ToastContent { Text = SystemUtil.GetStringResource("LocalCameraUnavailable") });
                var task = Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    AppShell.Current.AppFrame.GoBack();
                });
                return;
            }

            CaptureTimer = new DispatcherTimer();
            CaptureTimer.Interval = TimeSpan.FromMilliseconds(300);
            CaptureTimer.Tick += async (sender, ev) =>
            {
                await GetPreviewFrameAsSoftwareBitmapAsync(); // capture a frame and find QR code
            };

            FocusTimer = new DispatcherTimer();
            FocusTimer.Interval = TimeSpan.FromMilliseconds(2000);
            FocusTimer.Tick += (sender, ev) =>
            {
                TryToFocus();
            };

            CaptureTimer.Start();
            FocusTimer.Start();

            await SetPreviewRotationAsync();
        }

        private const string DEVCIE_FAMILY_DESKTOP = "Windows.Desktop";
        private const string DEVICE_FAMILY_MOBILE = "Windows.Mobile";

        private async Task SetPreviewRotationAsync()
        {
            // Calculate which way and how far to rotate the preview
            int rotationDegrees = ConvertDisplayOrientationToDegrees(_displayInformation.CurrentOrientation);

            var props = _mediaCapture.VideoDeviceController.GetMediaStreamProperties(MediaStreamType.VideoPreview);
            props.Properties.Add(RotationKey, rotationDegrees);

            // On Desktop, this setting may stop some of webcams.
            if (AnalyticsInfo.VersionInfo.DeviceFamily != DEVCIE_FAMILY_DESKTOP)
            {
                await _mediaCapture.SetEncodingPropertiesAsync(MediaStreamType.VideoPreview, props, null);
            }
        }

        private static PhotoOrientation ConvertOrientationToPhotoOrientation(SimpleOrientation orientation)
        {
            switch (orientation)
            {
                case SimpleOrientation.Rotated90DegreesCounterclockwise:
                    return PhotoOrientation.Rotate90;
                case SimpleOrientation.Rotated180DegreesCounterclockwise:
                    return PhotoOrientation.Rotate180;
                case SimpleOrientation.Rotated270DegreesCounterclockwise:
                    return PhotoOrientation.Rotate270;
                case SimpleOrientation.NotRotated:
                default:
                    return PhotoOrientation.Normal;
            }
        }

        private static int ConvertDisplayOrientationToDegrees(DisplayOrientations orientation)
        {
            switch (orientation)
            {
                case DisplayOrientations.Portrait:
                    return 90;
                case DisplayOrientations.LandscapeFlipped:
                    return 180;
                case DisplayOrientations.PortraitFlipped:
                    return 270;
                case DisplayOrientations.Landscape:
                default:
                    return 0;
            }
        }

        protected override async void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            DisplayInformation.AutoRotationPreferences = DisplayOrientations.None;

            CaptureTimer?.Stop();
            FocusTimer?.Stop();
            await CleanupCameraAsync();
        }

        async void TryToFocus()
        {
            if (_mediaCapture == null) { return; }

            var focusControl = _mediaCapture.VideoDeviceController.FocusControl;
            if (focusControl.Supported)
            {
                await focusControl.FocusAsync();
            }
        }

        /// <summary>
        /// In the event of the app being minimized this method handles media property change events. If the app receives a mute
        /// notification, it is no longer in the foreground.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private async void SystemMediaControls_PropertyChanged(SystemMediaTransportControls sender, SystemMediaTransportControlsPropertyChangedEventArgs args)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                // Only handle this event if this page is currently being displayed
                if (args.Property == SystemMediaTransportControlsProperty.SoundLevel && Frame.CurrentSourcePageType == typeof(MainPage))
                {
                    // Check to see if the app is being muted. If so, it is being minimized.
                    // Otherwise if it is not initialized, it is being brought into focus.
                    if (sender.SoundLevel == SoundLevel.Muted)
                    {
                        await CleanupCameraAsync();
                    }
                    else if (!_isInitialized)
                    {
                        await InitializeCameraAsync();
                    }
                }
            });
        }

        private async void MediaCapture_Failed(MediaCapture sender, MediaCaptureFailedEventArgs errorEventArgs)
        {
            Debug.WriteLine("MediaCapture_Failed: (0x{0:X}) {1}", errorEventArgs.Code, errorEventArgs.Message);

            await CleanupCameraAsync();
        }


        /// <summary>
        /// Initializes the MediaCapture, registers events, gets camera device information for mirroring and rotating, and starts preview
        /// </summary>
        /// <returns></returns>
        private async Task InitializeCameraAsync()
        {
            Debug.WriteLine("InitializeCameraAsync");

            if (_mediaCapture == null)
            {

                // Attempt to get the back camera if one is available, but use any camera device if not
                var cameraDevice = await FindCameraDeviceByPanelAsync(Windows.Devices.Enumeration.Panel.Back);

                if (cameraDevice == null)
                {
                    Debug.WriteLine("No camera device found!");
                    return;
                }

                // Create MediaCapture and its settings
                _mediaCapture = new MediaCapture();

                // Register for a notification when something goes wrong
                _mediaCapture.Failed += MediaCapture_Failed;

                var settings = new MediaCaptureInitializationSettings
                {
                    VideoDeviceId = cameraDevice.Id,
                    StreamingCaptureMode = StreamingCaptureMode.Video,
                };

                // Initialize MediaCapture
                try
                {
                    await _mediaCapture.InitializeAsync(settings);
                    _isInitialized = true;
                }
                catch (UnauthorizedAccessException)
                {
                    Debug.WriteLine("The app was denied access to the camera");
                    _mediaCapture.Dispose();
                    _mediaCapture = null;
                    // TODO another toast to request permission?
                    return;
                }

                // If initialization succeeded, start the preview
                if (_isInitialized)
                {
                    await StartPreviewAsync();
                }
            }
        }

        /// <summary>
        /// Starts the preview and adjusts it for rotation and mirroring after making a request to keep the screen on and unlocks the UI
        /// </summary>
        /// <returns></returns>
        private async Task StartPreviewAsync()
        {
            Debug.WriteLine("StartPreviewAsync");

            // Prevent the device from sleeping while the preview is running
            _displayRequest.RequestActive();

            // Register to listen for media property changes
            _systemMediaControls.PropertyChanged += SystemMediaControls_PropertyChanged;

            // Set the preview source in the UI and mirror it if necessary
            PreviewControl.Source = _mediaCapture;
            PreviewControl.FlowDirection = FlowDirection.LeftToRight;

            // Start the preview
            await _mediaCapture.StartPreviewAsync();
            _isPreviewing = true;
        }

        /// <summary>
        /// Stops the preview and deactivates a display request, to allow the screen to go into power saving modes, and locks the UI
        /// </summary>
        /// <returns></returns>
        private async Task StopPreviewAsync()
        {
            _isPreviewing = false;
            await _mediaCapture?.StopPreviewAsync();

            // Use the dispatcher because this method is sometimes called from non-UI threads
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                PreviewControl.Source = null;

                // Allow the device to sleep now that the preview is stopped
                _displayRequest.RequestRelease();
            });
        }

        /// <summary>
        /// Gets the current preview frame as a SoftwareBitmap, displays its properties in a TextBlock, and can optionally display the image
        /// in the UI and/or save it to disk as a jpg
        /// </summary>
        /// <returns></returns>
        private async Task GetPreviewFrameAsSoftwareBitmapAsync()
        {
            // Get information about the preview
            var previewProperties = _mediaCapture.VideoDeviceController.GetMediaStreamProperties(MediaStreamType.VideoPreview) as VideoEncodingProperties;

            // Create the video frame to request a SoftwareBitmap preview frame
            var videoFrame = new VideoFrame(BitmapPixelFormat.Bgra8, (int)previewProperties.Width, (int)previewProperties.Height);

            // Capture the preview frame
            using (var currentFrame = await _mediaCapture.GetPreviewFrameAsync(videoFrame))
            {
                // Collect the resulting frame
                SoftwareBitmap previewFrame = currentFrame.SoftwareBitmap;
                var data = Decode(previewFrame);
                if (data.Count > 0)
                {
                    var sb = new StringBuilder();
                    foreach (var d in data)
                    {
                        Debug.WriteLine(d);
                        sb.Append(d);
                    }

                    SonyQrData qrdata = null;
                    try
                    {
                        qrdata = SonyQrDataParser.ParseData(sb.ToString());
                    }
                    catch (FormatException ex)
                    {
                        DebugUtil.Log("QR data parse error: " + ex.Message);
                    }
                    // DebugText.Text = sb.ToString();
                    await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        if (qrdata != null)
                        {
                            Frame.Navigate(typeof(EntrancePage), qrdata);
                        }
                        else
                        {
                            AppShell.Current.Toast.PushToast(new Controls.ToastContent { Text = SystemUtil.GetStringResource("QrCodeIncompatible") });
                        }
                    });
                }
            }
        }

        BarcodeReader barcodeReader = new BarcodeReader();

        public IList<string> Decode(SoftwareBitmap image, bool tryMultipleBarcodes = false)
        {
            IList<string> txtContent = new List<string>();

            WriteableBitmap bitmap = new WriteableBitmap(image.PixelWidth, image.PixelHeight);

            image.CopyToBuffer(bitmap.PixelBuffer);

            Result[] results = null;
            Result result = barcodeReader.Decode(bitmap);
            if (result != null)
            {
                results = new[] { result };
            }

            if (results != null)
            {
                foreach (Result res in results)
                {
                    txtContent.Add(res.Text);
                }
            }
            return txtContent;
        }

        /// <summary>
        /// Cleans up the camera resources (after stopping the preview if necessary) and unregisters from MediaCapture events
        /// </summary>
        /// <returns></returns>
        private async Task CleanupCameraAsync()
        {
            if (_isInitialized)
            {
                if (_isPreviewing)
                {
                    // The call to stop the preview is included here for completeness, but can be
                    // safely removed if a call to MediaCapture.Dispose() is being made later,
                    // as the preview will be automatically stopped at that point
                    await StopPreviewAsync();
                }

                _isInitialized = false;
            }

            if (_mediaCapture != null)
            {
                _mediaCapture.Failed -= MediaCapture_Failed;
                _mediaCapture.Dispose();
                _mediaCapture = null;
            }
        }

        /// <summary>
        /// Queries the available video capture devices to try and find one mounted on the desired panel
        /// </summary>
        /// <param name="desiredPanel">The panel on the device that the desired camera is mounted on</param>
        /// <returns>A DeviceInformation instance with a reference to the camera mounted on the desired panel if available,
        ///          any other camera if not, or null if no camera is available.</returns>
        private static async Task<DeviceInformation> FindCameraDeviceByPanelAsync(Windows.Devices.Enumeration.Panel desiredPanel)
        {
            // Get available devices for capturing pictures
            var allVideoDevices = await DeviceInformation.FindAllAsync(DeviceClass.VideoCapture);

            // Get the desired camera by panel
            DeviceInformation desiredDevice = allVideoDevices.FirstOrDefault(x => x.EnclosureLocation != null && x.EnclosureLocation.Panel == desiredPanel);

            // If there is no device mounted on the desired panel, return the first device found
            return desiredDevice ?? allVideoDevices.FirstOrDefault();
        }

        private async void CaptureButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            await GetPreviewFrameAsSoftwareBitmapAsync();
        }

        private void PreviewControl_Tapped(object sender, TappedRoutedEventArgs e)
        {
            TryToFocus();
        }
    }
}
