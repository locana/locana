using Kazyx.RemoteApi;
using Kazyx.RemoteApi.Camera;
using Kazyx.Uwpmm.CameraControl;
using Kazyx.Uwpmm.Utility;
using System.Collections.Generic;
using System.Diagnostics;

namespace Kazyx.Uwpmm.DataModel
{
    class ControlPanelDataSource : ObservableBase
    {
        private TargetDevice _Device;
        public TargetDevice Device
        {
            private set
            {
                _Device = value;
                NotifyChangedOnUI(""); // Notify all properties are changed.
            }
            get { return _Device; }
        }

        public ControlPanelDataSource(TargetDevice target)
        {
            this.Device = target;
            Device.Status.PropertyChanged += (sender, e) =>
            {
                GenericPropertyChanged(e.PropertyName);
                NotifyChangedOnUI("IsPeriodicalShootingAvailable");
            };
            Device.Api.AvailiableApisUpdated += (sender, e) =>
            {
                NotifyChangedOnUI("IsAvailableExposureMode");
                NotifyChangedOnUI("IsAvailableShootMode");
                NotifyChangedOnUI("IsAvailableBeepMode");
                NotifyChangedOnUI("IsAvailableSelfTimer");
                NotifyChangedOnUI("IsAvailablePostviewSize");
                NotifyChangedOnUI("IsAvailableStillImageSize");
                NotifyChangedOnUI("IsAvailableWhiteBalance");
                NotifyChangedOnUI("IsAvailableColorTemperture");
                NotifyChangedOnUI("IsAvailableFocusMode");
                NotifyChangedOnUI("IsAvailableMovieQuality");
                NotifyChangedOnUI("IsAvailableFlashMode");
                NotifyChangedOnUI("IsAvailableSteadyMode");
                NotifyChangedOnUI("IsAvailableViewAngle");
                NotifyChangedOnUI("IsAvailableZoomSetting");
                NotifyChangedOnUI("IsAvailableSceneSelection");
                NotifyChangedOnUI("IsAvailableTrackingFocus");
                NotifyChangedOnUI("IsAvailableStillQuality");
                NotifyChangedOnUI("IsAvailableMovieFileFormat");
                NotifyChangedOnUI("IsAvailableFlipMode");
                NotifyChangedOnUI("IsAvailableIntervalTime");
                NotifyChangedOnUI("IsAvailableColorSetting");
                NotifyChangedOnUI("IsAvailableInfraredRemoteControl");
                NotifyChangedOnUI("IsAvailableTvColorSystem");
                NotifyChangedOnUI("IsAvailableAutoPowerOff");
                NotifyChangedOnUI("IsAvailableLoopRecTime");
                NotifyChangedOnUI("IsAvailableWindNoiseReduction");
                NotifyChangedOnUI("IsAvailableAudioRecording");
            };
            Device.Api.ServerVersionDetected += (sender, e) =>
            {
                NotifyChangedOnUI("IsRestrictedApiVisible");
            };
        }

        private void GenericPropertyChanged(string name)
        {
            DebugUtil.Log("PropertyChanged: " + name);
            NotifyChanged("Candidates" + name);
            NotifyChanged("SelectedIndex" + name);
            NotifyChanged("IsAvailable" + name);
        }

        public bool IsRestrictedApiAvailable
        {
            get { return Device.Api.Capability.Version.IsLiberated; }
        }

        public bool IsAvailableExposureMode
        {
            get
            {
                return Device.Api.Capability.IsAvailable("setExposureMode") &&
                    Device.Status.ExposureMode != null;
            }
        }

        public List<string> CandidatesExposureMode
        {
            get
            {
                return SettingValueConverter.FromExposureMode(Device.Status.ExposureMode).Candidates;
            }
        }

        public int SelectedIndexExposureMode
        {
            get
            {
                return SettingValueConverter.GetSelectedIndex(Device.Status.ExposureMode);
            }
            set
            {
                ParameterUtil.SetSelectedAsCurrent(Device.Status.ExposureMode, value);
            }
        }

        public bool IsAvailableShootMode
        {
            get
            {
                return Device.Api.Capability.IsAvailable("setShootMode") &&
                    Device.Status.ShootMode != null;
            }
        }

        public List<string> CandidatesShootMode
        {
            get
            {
                return SettingValueConverter.FromShootMode(Device.Status.ShootMode).Candidates;
            }
        }

        public int SelectedIndexShootMode
        {
            get
            {
                return SettingValueConverter.GetSelectedIndex(Device.Status.ShootMode);
            }
            set
            {
                ParameterUtil.SetSelectedAsCurrent(Device.Status.ShootMode, value);
            }
        }

        public bool IsAvailableBeepMode
        {
            get
            {
                return Device.Api.Capability.IsAvailable("setBeepMode") &&
                    Device.Status.BeepMode != null;
            }
        }

        public List<string> CandidatesBeepMode
        {
            get
            {
                return SettingValueConverter.FromBeepMode(Device.Status.BeepMode).Candidates;
            }
        }

        public int SelectedIndexBeepMode
        {
            get
            {
                return SettingValueConverter.GetSelectedIndex(Device.Status.BeepMode);
            }
            set
            {
                ParameterUtil.SetSelectedAsCurrent(Device.Status.BeepMode, value);
            }
        }

        public bool IsAvailablePostviewSize
        {
            get
            {
                return Device.Api.Capability.IsAvailable("setPostviewImageSize") &&
                    Device.Status.PostviewSize != null;
            }
        }

        public List<string> CandidatesPostviewSize
        {
            get
            {
                return SettingValueConverter.FromPostViewSize(Device.Status.PostviewSize).Candidates;
            }
        }

        public int SelectedIndexPostviewSize
        {
            get
            {
                return SettingValueConverter.GetSelectedIndex(Device.Status.PostviewSize);
            }
            set
            {
                ParameterUtil.SetSelectedAsCurrent(Device.Status.PostviewSize, value);
            }
        }

        public bool IsAvailableSelfTimer
        {
            get
            {
                return Device.Api.Capability.IsAvailable("setSelfTimer") &&
                    Device.Status.SelfTimer != null;
            }
        }

        public List<string> CandidatesSelfTimer
        {
            get
            {
                return SettingValueConverter.FromSelfTimer(Device.Status.SelfTimer).Candidates;
            }
        }

        public int SelectedIndexSelfTimer
        {
            get
            {
                return SettingValueConverter.GetSelectedIndex(Device.Status.SelfTimer);
            }
            set
            {
                ParameterUtil.SetSelectedAsCurrent(Device.Status.SelfTimer, value);
            }
        }

        public bool IsAvailableStillImageSize
        {
            get
            {
                return Device.Api.Capability.IsAvailable("setStillSize") &&
                    Device.Status.StillImageSize != null;
            }
        }

        public List<string> CandidatesStillImageSize
        {
            get
            {
                return SettingValueConverter.FromStillImageSize(Device.Status.StillImageSize).Candidates;
            }
        }

        public int SelectedIndexStillImageSize
        {
            get
            {
                return SettingValueConverter.GetSelectedIndex(Device.Status.StillImageSize);
            }
            set
            {
                ParameterUtil.SetSelectedAsCurrent(Device.Status.StillImageSize, value);
            }
        }

        public int SelectedIndexWhiteBalance
        {
            get
            {
                return SettingValueConverter.GetSelectedIndex(Device.Status.WhiteBalance);
            }
            set
            {
                ParameterUtil.SetSelectedAsCurrent(Device.Status.WhiteBalance, value);
            }
        }

        public List<string> CandidatesWhiteBalance
        {
            get
            {
                return SettingValueConverter.FromWhiteBalance(Device.Status.WhiteBalance).Candidates;
            }
        }

        public bool IsAvailableWhiteBalance
        {
            get
            {
                return Device.Api.Capability.IsAvailable("setWhiteBalance") &&
                    Device.Status.WhiteBalance != null;
            }
        }

        public bool IsAvailableColorTemperture
        {
            get
            {
                var status = Device.Status;
                return IsAvailableWhiteBalance &&
                    status.ColorTempertureCandidates != null &&
                    status.WhiteBalance.Current != null &&
                    status.ColorTempertureCandidates.ContainsKey(status.WhiteBalance.Current) &&
                    status.ColorTempertureCandidates[status.WhiteBalance.Current].Length != 0 &&
                    status.ColorTemperture != -1;
            }
        }


        public int SelectedIndexViewAngle
        {
            get
            {
                return SettingValueConverter.GetSelectedIndex(Device.Status.ViewAngle);
            }
            set
            {
                ParameterUtil.SetSelectedAsCurrent(Device.Status.ViewAngle, value);
            }
        }

        public List<string> CandidatesViewAngle
        {
            get
            {
                return SettingValueConverter.FromViewAngle(Device.Status.ViewAngle).Candidates;
            }
        }

        public bool IsAvailableViewAngle
        {
            get
            {
                return Device.Api.Capability.IsAvailable("setViewAngle") &&
                    Device.Status.BeepMode != null;
            }
        }

        public int SelectedIndexSteadyMode
        {
            get
            {
                return SettingValueConverter.GetSelectedIndex(Device.Status.SteadyMode);
            }
            set
            {
                ParameterUtil.SetSelectedAsCurrent(Device.Status.SteadyMode, value);
            }
        }

        public List<string> CandidatesSteadyMode
        {
            get
            {
                return SettingValueConverter.FromSteadyMode(Device.Status.SteadyMode).Candidates;
            }
        }

        public bool IsAvailableSteadyMode
        {
            get
            {
                return Device.Api.Capability.IsAvailable("setSteadyMode") &&
                    Device.Status.SteadyMode != null;
            }
        }

        public int SelectedIndexMovieQuality
        {
            get
            {
                return SettingValueConverter.GetSelectedIndex(Device.Status.MovieQuality);
            }
            set
            {
                ParameterUtil.SetSelectedAsCurrent(Device.Status.MovieQuality, value);
            }
        }

        public List<string> CandidatesMovieQuality
        {
            get
            {
                return SettingValueConverter.FromMovieQuality(Device.Status.MovieQuality).Candidates;
            }
        }

        public bool IsAvailableMovieQuality
        {
            get
            {
                return Device.Api.Capability.IsAvailable("setMovieQuality") &&
                    Device.Status.MovieQuality != null;
            }
        }

        public int SelectedIndexFlashMode
        {
            get
            {
                return SettingValueConverter.GetSelectedIndex(Device.Status.FlashMode);
            }
            set
            {
                ParameterUtil.SetSelectedAsCurrent(Device.Status.FlashMode, value);
            }
        }

        public List<string> CandidatesFlashMode
        {
            get
            {
                return SettingValueConverter.FromFlashMode(Device.Status.FlashMode).Candidates;
            }
        }

        public bool IsAvailableFlashMode
        {
            get
            {
                return Device.Api.Capability.IsAvailable("setFlashMode") &&
                    Device.Status.FlashMode != null;
            }
        }

        public int SelectedIndexFocusMode
        {
            get
            {
                return SettingValueConverter.GetSelectedIndex(Device.Status.FocusMode);
            }
            set
            {
                ParameterUtil.SetSelectedAsCurrent(Device.Status.FocusMode, value);
            }
        }

        public List<string> CandidatesFocusMode
        {
            get
            {
                return SettingValueConverter.FromFocusMode(Device.Status.FocusMode).Candidates;
            }
        }

        public bool IsAvailableFocusMode
        {
            get
            {
                return Device.Api.Capability.IsAvailable("setFocusMode") &&
                    Device.Status.FocusMode != null;
            }
        }

        public int SelectedIndexContShootingMode
        {
            get
            {
                return SettingValueConverter.GetSelectedIndex(Device.Status.ContShootingMode);
            }
            set
            {
                ParameterUtil.SetSelectedAsCurrent(Device.Status.ContShootingMode, value);
            }
        }

        public List<string> CandidatesContShootingMode
        {
            get
            {
                return SettingValueConverter.FromContShootingMode(Device.Status.ContShootingMode).Candidates;
            }
        }

        public bool IsAvailableContShootingMode
        {
            get
            {
                return Device.Api.Capability.IsAvailable("setContShootingMode") && Device.Status.ContShootingMode != null;
            }
        }

        public int SelectedIndexContShootingSpeed
        {
            get
            {
                return SettingValueConverter.GetSelectedIndex(Device.Status.ContShootingSpeed);
            }
            set
            {
                ParameterUtil.SetSelectedAsCurrent(Device.Status.ContShootingSpeed, value);
            }
        }

        public List<string> CandidatesContShootingSpeed
        {
            get
            {
                return SettingValueConverter.FromContShootingSpeed(Device.Status.ContShootingSpeed).Candidates;
            }
        }

        public bool IsAvailableContShootingSpeed
        {
            get
            {
                return Device.Api.Capability.IsAvailable("setContShootingSpeed") && Device.Status.ContShootingSpeed != null;
            }
        }

        public int SelectedIndexZoomSetting
        {
            get
            {
                return SettingValueConverter.GetSelectedIndex(Device.Status.ZoomSetting);
            }
            set
            {
                ParameterUtil.SetSelectedAsCurrent(Device.Status.ZoomSetting, value);
            }
        }

        public List<string> CandidatesZoomSetting
        {
            get
            {
                return SettingValueConverter.FromZoomSetting(Device.Status.ZoomSetting).Candidates;
            }
        }

        public bool IsAvailableZoomSetting
        {
            get
            {
                return Device.Api.Capability.IsAvailable("setZoomSetting") && Device.Status.ZoomSetting != null;
            }
        }

        public int SelectedIndexSceneSelection
        {
            get
            {
                return SettingValueConverter.GetSelectedIndex(Device.Status.SceneSelection);
            }
            set
            {
                ParameterUtil.SetSelectedAsCurrent(Device.Status.SceneSelection, value);
            }
        }

        public List<string> CandidatesSceneSelection
        {
            get
            {
                return SettingValueConverter.FromSceneSelection(Device.Status.SceneSelection).Candidates;
            }
        }

        public bool IsAvailableSceneSelection
        {
            get
            {
                return Device.Api.Capability.IsAvailable("setSceneSelection") && Device.Status.SceneSelection != null;
            }
        }

        public int SelectedIndexTrackingFocus
        {
            get
            {
                return SettingValueConverter.GetSelectedIndex(Device.Status.TrackingFocus);
            }
            set
            {
                ParameterUtil.SetSelectedAsCurrent(Device.Status.TrackingFocus, value);
            }
        }

        public List<string> CandidatesTrackingFocus
        {
            get
            {
                return SettingValueConverter.FromTrackingFocus(Device.Status.TrackingFocus).Candidates;
            }
        }

        public bool IsAvailableTrackingFocus
        {
            get
            {
                return Device.Api.Capability.IsAvailable("setTrackingFocus") && Device.Status.TrackingFocus != null;
            }
        }

        public int SelectedIndexStillQuality
        {
            get
            {
                return SettingValueConverter.GetSelectedIndex(Device.Status.StillQuality);
            }
            set
            {
                ParameterUtil.SetSelectedAsCurrent(Device.Status.StillQuality, value);
            }
        }

        public List<string> CandidatesStillQuality
        {
            get
            {
                return SettingValueConverter.FromStillQuality(Device.Status.StillQuality).Candidates;
            }
        }

        public bool IsAvailableStillQuality
        {
            get
            {
                return Device.Api.Capability.IsAvailable("setStillQuality") && Device.Status.StillQuality != null;
            }
        }

        public int SelectedIndexMovieFileFormat
        {
            get
            {
                return SettingValueConverter.GetSelectedIndex(Device.Status.MovieFileFormat);
            }
            set
            {
                ParameterUtil.SetSelectedAsCurrent(Device.Status.MovieFileFormat, value);
            }
        }

        public List<string> CandidatesMovieFileFormat
        {
            get
            {
                return SettingValueConverter.FromMovieFileFormat(Device.Status.MovieFileFormat).Candidates;
            }
        }

        public bool IsAvailableMovieFileFormat
        {
            get
            {
                return Device.Api.Capability.IsAvailable("setMovieFileFormat") && Device.Status.MovieFileFormat != null;
            }
        }

        public int SelectedIndexFlipMode
        {
            get
            {
                return SettingValueConverter.GetSelectedIndex(Device.Status.FlipMode);
            }
            set
            {
                ParameterUtil.SetSelectedAsCurrent(Device.Status.FlipMode, value);
            }
        }

        public List<string> CandidatesFlipMode
        {
            get
            {
                return SettingValueConverter.FromFlipMode(Device.Status.FlipMode).Candidates;
            }
        }

        public bool IsAvailableFlipMode
        {
            get
            {
                return Device.Api.Capability.IsAvailable("setFlipMode") && Device.Status.FlipMode != null;
            }
        }

        public int SelectedIndexIntervalTime
        {
            get
            {
                return SettingValueConverter.GetSelectedIndex(Device.Status.IntervalTime);
            }
            set
            {
                ParameterUtil.SetSelectedAsCurrent(Device.Status.IntervalTime, value);
            }
        }

        public List<string> CandidatesIntervalTime
        {
            get
            {
                return SettingValueConverter.FromIntervalTime(Device.Status.IntervalTime).Candidates;
            }
        }

        public bool IsAvailableIntervalTime
        {
            get
            {
                return Device.Api.Capability.IsAvailable("setIntervalTime") && Device.Status.IntervalTime != null;
            }
        }

        public int SelectedIndexColorSetting
        {
            get
            {
                return SettingValueConverter.GetSelectedIndex(Device.Status.ColorSetting);
            }
            set
            {
                ParameterUtil.SetSelectedAsCurrent(Device.Status.ColorSetting, value);
            }
        }

        public List<string> CandidatesColorSetting
        {
            get
            {
                return SettingValueConverter.FromColorSetting(Device.Status.ColorSetting).Candidates;
            }
        }

        public bool IsAvailableColorSetting
        {
            get
            {
                return Device.Api.Capability.IsAvailable("setColorSetting") && Device.Status.ColorSetting != null;
            }
        }

        public int SelectedIndexInfraredRemoteControl
        {
            get
            {
                return SettingValueConverter.GetSelectedIndex(Device.Status.InfraredRemoteControl);
            }
            set
            {
                ParameterUtil.SetSelectedAsCurrent(Device.Status.InfraredRemoteControl, value);
            }
        }

        public List<string> CandidatesInfraredRemoteControl
        {
            get
            {
                return SettingValueConverter.FromInfraredRemoteControl(Device.Status.InfraredRemoteControl).Candidates;
            }
        }

        public bool IsAvailableInfraredRemoteControl
        {
            get
            {
                return Device.Api.Capability.IsAvailable("setInfraredRemoteControl") && Device.Status.InfraredRemoteControl != null;
            }
        }

        public int SelectedIndexTvColorSystem
        {
            get
            {
                return SettingValueConverter.GetSelectedIndex(Device.Status.TvColorSystem);
            }
            set
            {
                ParameterUtil.SetSelectedAsCurrent(Device.Status.TvColorSystem, value);
            }
        }

        public List<string> CandidatesTvColorSystem
        {
            get
            {
                return SettingValueConverter.FromTvColorSystem(Device.Status.TvColorSystem).Candidates;
            }
        }

        public bool IsAvailableTvColorSystem
        {
            get
            {
                return Device.Api.Capability.IsAvailable("setTvColorSystem") && Device.Status.TvColorSystem != null;
            }
        }

        public int SelectedIndexAutoPowerOff
        {
            get
            {
                return SettingValueConverter.GetSelectedIndex(Device.Status.AutoPowerOff);
            }
            set
            {
                ParameterUtil.SetSelectedAsCurrent(Device.Status.AutoPowerOff, value);
            }
        }

        public List<string> CandidatesAutoPowerOff
        {
            get
            {
                return SettingValueConverter.FromAutoPowerOff(Device.Status.AutoPowerOff).Candidates;
            }
        }

        public bool IsAvailableAutoPowerOff
        {
            get
            {
                return Device.Api.Capability.IsAvailable("setAutoPowerOff") && Device.Status.AutoPowerOff != null;
            }
        }

        public int SelectedIndexLoopRecTime
        {
            get
            {
                return SettingValueConverter.GetSelectedIndex(Device.Status.LoopRecTime);
            }
            set
            {
                ParameterUtil.SetSelectedAsCurrent(Device.Status.LoopRecTime, value);
            }
        }

        public List<string> CandidatesLoopRecTime
        {
            get
            {
                return SettingValueConverter.FromLoopRecTime(Device.Status.LoopRecTime).Candidates;
            }
        }

        public bool IsAvailableLoopRecTime
        {
            get
            {
                return Device.Api.Capability.IsAvailable("setLoopRecTime") && Device.Status.LoopRecTime != null;
            }
        }

        public int SelectedIndexWindNoiseReduction
        {
            get
            {
                return SettingValueConverter.GetSelectedIndex(Device.Status.WindNoiseReduction);
            }
            set
            {
                ParameterUtil.SetSelectedAsCurrent(Device.Status.WindNoiseReduction, value);
            }
        }

        public List<string> CandidatesWindNoiseReduction
        {
            get
            {
                return SettingValueConverter.FromWindNoiseReduction(Device.Status.WindNoiseReduction).Candidates;
            }
        }

        public bool IsAvailableWindNoiseReduction
        {
            get
            {
                return Device.Api.Capability.IsAvailable("setWindNoiseReduction") && Device.Status.WindNoiseReduction != null;
            }
        }

        public int SelectedIndexAudioRecording
        {
            get
            {
                return SettingValueConverter.GetSelectedIndex(Device.Status.AudioRecording);
            }
            set
            {
                ParameterUtil.SetSelectedAsCurrent(Device.Status.AudioRecording, value);
            }
        }

        public List<string> CandidatesAudioRecording
        {
            get
            {
                return SettingValueConverter.FromAudioRecording(Device.Status.AudioRecording).Candidates;
            }
        }

        public bool IsAvailableAudioRecording
        {
            get
            {
                return Device.Api.Capability.IsAvailable("setAudioRecording") && Device.Status.AudioRecording != null;
            }
        }


        public bool IsPeriodicalShootingAvailable
        {
            get
            {
                return Device.Status.ShootMode.Current == ShootModeParam.Still &&
                    (Device.Status.ContShootingMode == null || (Device.Status.ContShootingMode != null && Device.Status.ContShootingMode.Current == ContinuousShootMode.Single));
            }
        }
    }
}
