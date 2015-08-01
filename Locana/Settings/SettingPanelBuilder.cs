using Kazyx.RemoteApi;
using Kazyx.RemoteApi.Camera;
using Kazyx.Uwpmm.CameraControl;
using Kazyx.Uwpmm.DataModel;
using Kazyx.Uwpmm.Utility;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace Kazyx.Uwpmm.Settings
{
    class SettingPanelBuilder
    {
        private readonly ControlPanelDataSource DataSource;

        private readonly Binding VisibilityBinding;

        private DeviceApiHolder Api { get { return DataSource.Device.Api; } }

        private CameraStatus Status { get { return DataSource.Device.Status; } }

        private Dictionary<string, StackPanel> Panels = new Dictionary<string, StackPanel>();

        public static SettingPanelBuilder CreateNew(TargetDevice device)
        {
            return new SettingPanelBuilder(device);
        }

        private SettingPanelBuilder(TargetDevice device)
        {
            DataSource = new ControlPanelDataSource(device);

            Panels.Add("setShootMode", BuildComboBoxPanel("ShootMode", "ShootMode", OnShootModeChanged));
            Panels.Add("setExposureMode", BuildComboBoxPanel("ExposureMode", "ExposureMode", OnExposureModeChanged));
            Panels.Add("setFocusMode", BuildComboBoxPanel("FocusMode", "FocusMode", OnFocusModeChanged));
            Panels.Add("setTrackingFocus", BuildComboBoxPanel("TrackingFocus", "TrackingFocusMode", OnTrackingFocusChanged));
            Panels.Add("setContShootingMode", BuildComboBoxPanel("ContShootingMode", "ContShootingMode", OnContShootingModeChanged));
            Panels.Add("setContShootingSpeed", BuildComboBoxPanel("ContShootingSpeed", "ContShootingSpeed", OnContShootingSpeedChanged));
            Panels.Add("setIntervalTime", BuildComboBoxPanel("IntervalTime", "IntervalTime1", OnIntervalTimeChanged));
            Panels.Add("setFlashMode", BuildComboBoxPanel("FlashMode", "FlashMode", OnFlashModeChanged));
            Panels.Add("setLoopRecTime", BuildComboBoxPanel("LoopRecTime", "LoopRecTime", OnLoopRecTimeChanged));

            Panels.Add("setWhiteBalance", BuildComboBoxPanel("WhiteBalance", "WhiteBalance", OnWhiteBalanceChanged));
            Panels.Add("ColorTemperture", BuildColorTemperturePanel());
            Panels.Add("setColorSetting", BuildComboBoxPanel("ColorSetting", "ColorSetting", OnColorSettingChanged));
            Panels.Add("setSceneSelection", BuildComboBoxPanel("SceneSelection", "SceneSelection", OnSceneSelectionChanged));

            Panels.Add("setMovieFileFormat", BuildComboBoxPanel("MovieFileFormat", "MovieFileFormat", OnMovieFormatChanged));
            Panels.Add("setMovieQuality", BuildComboBoxPanel("MovieQuality", "MovieQuality", OnMovieQualityChanged));
            Panels.Add("setStillSize", BuildComboBoxPanel("StillImageSize", "StillImageSize", OnStillImageSizeChanged));
            Panels.Add("setStillQuality", BuildComboBoxPanel("StillQuality", "StillQuality", OnStillQualityChanged));

            Panels.Add("setViewAngle", BuildComboBoxPanel("ViewAngle", "ViewAngle", OnViewAngleChanged));
            Panels.Add("setSteadyMode", BuildComboBoxPanel("SteadyMode", "SteadyShot", OnSteadyModeChanged));
            Panels.Add("setSelfTimer", BuildComboBoxPanel("SelfTimer", "SelfTimer", OnSelfTimerChanged));
            Panels.Add("setBeepMode", BuildComboBoxPanel("BeepMode", "BeepMode", OnBeepModeChanged));
            Panels.Add("setFlipMode", BuildComboBoxPanel("FlipMode", "FlipMode", OnFlipModeChanged));
            Panels.Add("setZoomSetting", BuildComboBoxPanel("ZoomSetting", "ZoomSetting", OnZoomSettingChanged));
            Panels.Add("setInfraredRemoteControl", BuildComboBoxPanel("InfraredRemoteControl", "InfraredRemoteControl", OnInfraredRemoteControlChanged));
            Panels.Add("setPostviewImageSize", BuildComboBoxPanel("PostviewSize", "Setting_PostViewImageSize", OnPostviewSizeChanged));
            Panels.Add("setAutoPowerOff", BuildComboBoxPanel("AutoPowerOff", "AutoPowerOff", OnAutoPowerOffChanged));
            Panels.Add("setTvColorSystem", BuildComboBoxPanel("TvColorSystem", "TvColorSystem", OnTvColorSystemChanged));
            Panels.Add("setAudioRecording", BuildComboBoxPanel("AudioRecording", "AudioRecording", OnAudioRecordingChanged));
            Panels.Add("setWindNoiseReduction", BuildComboBoxPanel("WindNoiseReduction", "WindNoiseReduction", OnWindNoiseReductionChanged));

            VisibilityBinding = new Binding()
            {
                Source = DataSource,
                Path = new PropertyPath("IsRestrictedApiAvailable"),
                Mode = BindingMode.OneWay,
                Converter = new BoolToVisibilityConverter(),
                FallbackValue = Visibility.Collapsed
            };
        }

        public List<StackPanel> GetPanelsToShow()
        {
            var list = new List<StackPanel>();

            foreach (var key in Panels.Keys)
            {
                if (Api.Capability.IsSupported(key) ||
                    (key == "ColorTemperture" && Api.Capability.IsSupported("setWhiteBalance")))
                {
                    list.Add(Panels[key]);
                }
                if (Api.Capability.IsRestrictedApi(key))
                {
                    Panels[key].SetBinding(StackPanel.VisibilityProperty, VisibilityBinding);
                }
            }

            list.Add(BuildPeriodicalShootingPanel());

            return list;
        }

        private async void OnFocusModeChanged(object sender, object e)
        {
            await OnSelectionChanged(sender, Status.FocusMode, Api.Camera.SetFocusModeAsync);
        }

        private async void OnMovieQualityChanged(object sender, object e)
        {
            await OnSelectionChanged(sender, Status.MovieQuality, Api.Camera.SetMovieQualityAsync);
        }

        private async void OnSteadyModeChanged(object sender, object e)
        {
            await OnSelectionChanged(sender, Status.SteadyMode, Api.Camera.SetSteadyModeAsync);
        }

        private async void OnViewAngleChanged(object sender, object e)
        {
            await OnSelectionChanged(sender, Status.ViewAngle, Api.Camera.SetViewAngleAsync);
        }

        private async void OnFlashModeChanged(object sender, object e)
        {
            await OnSelectionChanged(sender, Status.FlashMode, Api.Camera.SetFlashModeAsync);
        }

        private async void OnShootModeChanged(object sender, object e)
        {
            await OnSelectionChanged(sender, Status.ShootMode, Api.Camera.SetShootModeAsync);
        }

        private async void OnExposureModeChanged(object sender, object e)
        {
            await OnSelectionChanged(sender, Status.ExposureMode, Api.Camera.SetExposureModeAsync);
        }

        private async void OnSelfTimerChanged(object sender, object e)
        {
            await OnSelectionChanged(sender, Status.SelfTimer, Api.Camera.SetSelfTimerAsync);
        }

        private async void OnPostviewSizeChanged(object sender, object e)
        {
            await OnSelectionChanged(sender, Status.PostviewSize, Api.Camera.SetPostviewImageSizeAsync);
        }

        private async void OnBeepModeChanged(object sender, object e)
        {
            await OnSelectionChanged(sender, Status.BeepMode, Api.Camera.SetBeepModeAsync);
        }

        private async void OnStillImageSizeChanged(object sender, object e)
        {
            await OnSelectionChanged(sender, Status.StillImageSize, Api.Camera.SetStillImageSizeAsync);
        }

        private async void OnContShootingModeChanged(object sender, object e)
        {
            await OnSelectionChanged(sender, Status.ContShootingMode,
                async (mode) => { await Api.Camera.SetContShootingModeAsync(new ContinuousShootSetting() { Mode = mode }); });
        }

        private async void OnContShootingSpeedChanged(object sender, object e)
        {
            await OnSelectionChanged(sender, Status.ContShootingSpeed,
                async (mode) => { await Api.Camera.SetContShootingSpeedAsync(new ContinuousShootSpeedSetting() { Mode = mode }); });
        }

        private async void OnAutoPowerOffChanged(object sender, object e)
        {
            await OnSelectionChanged(sender, Status.AutoPowerOff,
                async (time) => { await Api.Camera.SetAutoPowerOffAsync(new AutoPowerOff() { TimeInSeconds = time }); });
        }

        private async void OnTvColorSystemChanged(object sender, object e)
        {
            await OnSelectionChanged(sender, Status.TvColorSystem,
                async (mode) => { await Api.Camera.SetTvColorSystemAsync(new TvColorSystem() { Mode = mode }); });
        }

        private async void OnInfraredRemoteControlChanged(object sender, object e)
        {
            await OnSelectionChanged(sender, Status.InfraredRemoteControl,
                async (mode) => { await Api.Camera.SetInfraredRemoteControlAsync(new InfraredRemoteControl() { Mode = mode }); });
        }

        private async void OnColorSettingChanged(object sender, object e)
        {
            await OnSelectionChanged(sender, Status.ColorSetting,
                async (mode) => { await Api.Camera.SetColorSettingAsync(new ColorSetting() { Mode = mode }); });
        }

        private async void OnIntervalTimeChanged(object sender, object e)
        {
            await OnSelectionChanged(sender, Status.IntervalTime,
                async (time) => { await Api.Camera.SetIntervalTimeAsync(new IntervalTimeSetting() { TimeInSeconds = time }); });
        }

        private async void OnFlipModeChanged(object sender, object e)
        {
            await OnSelectionChanged(sender, Status.FlipMode,
                async (mode) => { await Api.Camera.SetFlipSettingAsync(new FlipSetting() { Mode = mode }); });
        }

        private async void OnMovieFormatChanged(object sender, object e)
        {
            await OnSelectionChanged(sender, Status.MovieFileFormat,
                async (mode) => { await Api.Camera.SetMovieFileFormatAsync(new MovieFormat() { Mode = mode }); });
        }

        private async void OnStillQualityChanged(object sender, object e)
        {
            await OnSelectionChanged(sender, Status.StillQuality,
                async (mode) => { await Api.Camera.SetStillQualityAsync(new ImageQualitySetting() { Mode = mode }); });
        }

        private async void OnTrackingFocusChanged(object sender, object e)
        {
            await OnSelectionChanged(sender, Status.TrackingFocus,
                async (mode) => { await Api.Camera.SetTrackingFocusAsync(new TrackingFocusSetting() { Mode = mode }); });
        }

        private async void OnSceneSelectionChanged(object sender, object e)
        {
            await OnSelectionChanged(sender, Status.SceneSelection,
                async (mode) => { await Api.Camera.SetSceneSelectionAsync(new SceneSelectionSetting() { Mode = mode }); });
        }

        private async void OnZoomSettingChanged(object sender, object e)
        {
            await OnSelectionChanged(sender, Status.ZoomSetting,
                async (mode) => { await Api.Camera.SetZoomSettingAsync(new ZoomSetting() { Mode = mode }); });
        }
        private async void OnWhiteBalanceChanged(object sender, object e)
        {
            await OnComboBoxChanged(sender, Status.WhiteBalance,
                async (selected) =>
                {
                    if (selected != WhiteBalanceMode.Manual)
                    {
                        Status.ColorTemperture = -1;
                        await Api.Camera.SetWhiteBalanceAsync(new WhiteBalance { Mode = selected });
                    }
                    else
                    {
                        var min = Status.ColorTempertureCandidates[WhiteBalanceMode.Manual][0];
                        await Api.Camera.SetWhiteBalanceAsync(new WhiteBalance { Mode = selected, ColorTemperature = min });
                        Status.ColorTemperture = min;
                        if (ColorTempertureSlider != null)
                        {
                            var val = Status.ColorTempertureCandidates[selected];
                            ColorTempertureSlider.Maximum = val[val.Length - 1];
                            ColorTempertureSlider.Minimum = val[0];
                            ColorTempertureSlider.Value = Status.ColorTemperture;
                        }
                    }
                });
        }

        private async void OnLoopRecTimeChanged(object sender, object e)
        {
            await OnSelectionChanged(sender, Status.LoopRecTime,
                async (value) => { await Api.Camera.SetLoopRecTimeAsync(new LoopRecTimeSetting() { TimeInMinutes = value }); });
        }

        private async void OnWindNoiseReductionChanged(object sender, object e)
        {
            await OnSelectionChanged(sender, Status.WindNoiseReduction,
                async (mode) => { await Api.Camera.SetWindNoiseReductionAsync(new WindNoiseReductionSetting() { Mode = mode }); });
        }

        private async void OnAudioRecordingChanged(object sender, object e)
        {
            await OnSelectionChanged(sender, Status.AudioRecording,
                async (mode) => { await Api.Camera.SetAudioRecordingAsync(new AudioRecordingSetting() { Mode = mode }); });
        }

        private async Task OnSelectionChanged<T>(object sender, Capability<T> param, AsyncAction<T> action)
        {
            await OnComboBoxChanged(sender, param, async (selected) => { await action.Invoke(selected); });
        }

        private async Task OnComboBoxChanged<T>(object sender, Capability<T> param, AsyncAction<T> action)
        {
            if (param == null || param.Candidates == null || param.Candidates.Count == 0)
            {
                return;
            }

            var selected = (sender as ComboBox).SelectedIndex;
            var currentSetting = SettingValueConverter.GetSelectedIndex(param);
            if (selected != currentSetting)
            {
                return;
            }

            if (selected < 0 || param.Candidates.Count <= selected)
            {
                DebugUtil.Log("ignore out of range");
                return;
            }

            try
            {
                await action.Invoke(param.Candidates[selected]);
                return;
            }
            catch (RemoteApiException e)
            {
                DebugUtil.Log("Failed to set parameter: " + e.code);
            }
            catch (NullReferenceException e)
            {
                DebugUtil.Log("Failed to set parameter: " + e.Message);
            }
            await DataSource.Device.Observer.Refresh();
        }

        private StackPanel BuildComboBoxPanel(string key, string title_key, EventHandler<object> handler)
        {
            var box = new ComboBox
            {
                Margin = new Thickness(8, 0, 6, 0),
                BorderThickness = new Thickness(1),
            };
            box.SetBinding(ComboBox.IsEnabledProperty, new Binding
            {
                Source = DataSource,
                Path = new PropertyPath("IsAvailable" + key),
                Mode = BindingMode.OneWay
            });
            box.SetBinding(ComboBox.ItemsSourceProperty, new Binding
            {
                Source = DataSource,
                Path = new PropertyPath("Candidates" + key),
                Mode = BindingMode.OneWay
            });
            box.SetBinding(ComboBox.SelectedIndexProperty, new Binding
            {
                Source = DataSource,
                Path = new PropertyPath("SelectedIndex" + key),
                Mode = BindingMode.TwoWay
            });
            box.DropDownClosed += handler;

            var parent = BuildBasicPanel(SystemUtil.GetStringResource(title_key));
            parent.Children.Add(box);
            return parent;
        }

        private StackPanel BuildPeriodicalShootingPanel()
        {
            var indicator = new TextBlock()
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center,
                FontSize = 16,
            };
            indicator.SetBinding(TextBlock.TextProperty, new Binding()
            {
                Source = ApplicationSettings.GetInstance(),
                Path = new PropertyPath("IntervalTimeDisplayString"),
                Mode = BindingMode.OneWay,
            });

            var checkbox = new CheckBox()
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center,
                MinWidth = 30,
            };
            checkbox.SetBinding(CheckBox.IsCheckedProperty, new Binding()
            {
                Source = ApplicationSettings.GetInstance(),
                Path = new PropertyPath("IsIntervalShootingEnabled"),
                Mode = BindingMode.TwoWay,
            });
            checkbox.SetBinding(CheckBox.IsEnabledProperty, new Binding()
            {
                Source = DataSource,
                Path = new PropertyPath("IsPeriodicalShootingAvailable"),
                Mode = BindingMode.OneWay,
            });

            var firstPanel = new StackPanel()
            {
                Orientation = Windows.UI.Xaml.Controls.Orientation.Horizontal,
                Margin = new Thickness(3, 2, 3, 2),
                HorizontalAlignment = HorizontalAlignment.Stretch,
            };
            firstPanel.Children.Add(checkbox);
            firstPanel.Children.Add(indicator);

            var slider = BuildSlider(2, 60);
            slider.Value = ApplicationSettings.GetInstance().IntervalTime;
            slider.ValueChanged += (sender, e) =>
            {
                ApplicationSettings.GetInstance().IntervalTime = (int)(sender as Slider).Value;
                DebugUtil.Log("Interval updated: " + (int)(sender as Slider).Value);
            };
            slider.SetBinding(Slider.IsEnabledProperty, new Binding()
            {
                Source = DataSource,
                Path = new PropertyPath("IsPeriodicalShootingAvailable"),
                Mode = BindingMode.OneWay,
            });
            slider.SetBinding(Slider.VisibilityProperty, new Binding()
            {
                Source = checkbox,
                Path = new PropertyPath("IsChecked"),
                Mode = BindingMode.OneWay,
                Converter = new BoolToVisibilityConverter(),
            });

            var parent = BuildBasicPanel(SystemUtil.GetStringResource("IntervalSetting"));
            parent.Children.Add(firstPanel);
            parent.Children.Add(slider);

            return parent;
        }

        private Slider ColorTempertureSlider = null;

        private StackPanel BuildColorTemperturePanel()
        {
            var slider = BuildSlider(null, null);
            slider.Value = 0;

            slider.ManipulationCompleted += async (sender, e) =>
            {
                var target = ParameterUtil.AsValidColorTemperture((int)slider.Value, Status);
                slider.Value = target;
                try
                {
                    await Api.Camera.SetWhiteBalanceAsync(new WhiteBalance { Mode = Status.WhiteBalance.Current, ColorTemperature = target });
                }
                catch (RemoteApiException ex)
                {
                    DebugUtil.Log("Failed to set color temperture: " + ex.code);
                }
                catch (NullReferenceException ex)
                {
                    DebugUtil.Log("Failed to set color temperture: " + ex.Message);
                }
            };

            var indicator = new TextBlock
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                Style = Application.Current.Resources["BaseTextBlockStyle"] as Style,
                Margin = new Thickness(10, 22, 0, 0),
            };

            indicator.SetBinding(TextBlock.TextProperty, new Binding()
            {
                Source = Status,
                Path = new PropertyPath("ColorTemperture"),
                Mode = BindingMode.OneWay,
            });

            slider.SetBinding(Slider.ValueProperty, new Binding()
            {
                Source = Status,
                Path = new PropertyPath("ColorTemperture"),
                Mode = BindingMode.TwoWay
            });

            ColorTempertureSlider = slider;

            var parent = BuildBasicPanel(SystemUtil.GetStringResource("WB_ColorTemperture"));
            (parent.Children[0] as StackPanel).Children.Add(indicator);
            parent.Children.Add(slider);
            parent.SetBinding(StackPanel.VisibilityProperty, new Binding()
            {
                Source = DataSource,
                Path = new PropertyPath("IsAvailableColorTemperture"),
                Mode = BindingMode.OneWay,
                Converter = new BoolToVisibilityConverter()
            });

            return parent;
        }

        private static Slider BuildSlider(int? min, int? max)
        {
            return new Slider
            {
                Maximum = max != null ? max.Value : 1,
                Minimum = min != null ? min.Value : 0,
                Margin = new Thickness(16, 0, 8, 0),
                Width = double.NaN,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Top,
                Background = Application.Current.Resources["ProgressBarBackgroundThemeBrush"] as Brush
            };
        }

        private static StackPanel BuildBasicPanel(string title)
        {
            var panel = new StackPanel
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                MaxWidth = 200
            };

            var titlePanel = new StackPanel
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                Orientation = Windows.UI.Xaml.Controls.Orientation.Horizontal,
                Margin = new Thickness(0, 4, 0, 2)
            };

            titlePanel.Children.Add(new TextBlock
            {
                Text = title,
                HorizontalAlignment = HorizontalAlignment.Left,
                Margin = new Thickness(8, 0, 0, 0),
                Style = Application.Current.Resources["SubheaderTextBlockStyle"] as Style,
                FontSize = 20,
            });

            panel.Children.Add(titlePanel);

            return panel;
        }
    }

    delegate Task AsyncAction<T>(T arg);
}
