using Kazyx.RemoteApi;
using Kazyx.RemoteApi.Camera;
using Kazyx.Uwpmm.Control;
using Kazyx.Uwpmm.Playback;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kazyx.Uwpmm.Utility
{
    public class SettingValueConverter
    {
        public static int GetSelectedIndex<T>(Capability<T> info)
        {
            if (info == null || info.Candidates == null || info.Candidates.Count == 0)
            {
                return 0;
            }
            if (typeof(T) == typeof(string) || typeof(T) == typeof(int))
            {
                for (int i = 0; i < info.Candidates.Count; i++)
                {
                    if (info.Candidates[i].Equals(info.Current))
                    {
                        return i;
                    }
                }
            }
            else if (typeof(T) == typeof(StillImageSize))
            {
                var size = info as Capability<StillImageSize>;
                for (int i = 0; i < info.Candidates.Count; i++)
                {
                    if (size.Candidates[i].AspectRatio == size.Current.AspectRatio
                        && size.Candidates[i].SizeDefinition == size.Current.SizeDefinition)
                    {
                        return i;
                    }
                }
            }
            return 0;
        }

        public static int GetSelectedIndex(EvCapability info)
        {
            if (info == null || info.Candidate == null)
            {
                return 0;
            }
            return info.CurrentIndex;
        }

        private delegate string NameConverter<T>(T source);

        private static Capability<string> AsDisplayNames<T>(Capability<T> info, NameConverter<T> converter)
        {
            var res = AsDisabledCapability(info);
            if (res != null)
                return res;

            var mCandidates = new List<string>();
            foreach (T val in info.Candidates)
            {
                mCandidates.Add(converter.Invoke(val));
            }
            return new Capability<string>
            {
                Current = converter.Invoke(info.Current),
                Candidates = mCandidates
            };
        }

        private static Capability<string> AsDisabledCapability<T>(Capability<T> info)
        {
            if (info == null || info.Candidates == null || info.Candidates.Count == 0)
            {
                var disabled = SystemUtil.GetStringResource("Disabled");
                var list = new List<string>();
                list.Add(disabled);
                return new Capability<string>
                {
                    Candidates = list,
                    Current = disabled
                };
            }
            return null;
        }

        public static Capability<string> FromSelfTimer(Capability<int> info)
        {
            return AsDisplayNames<int>(info, FromSelfTimer);
        }

        private static string FromSelfTimer(int val)
        {
            if (val == 0) { return SystemUtil.GetStringResource("Off"); }
            else { return val + SystemUtil.GetStringResource("Seconds"); }
        }

        public static Capability<string> FromPostViewSize(Capability<string> info)
        {
            return AsDisplayNames<string>(info, FromPostViewSize);
        }

        private static string FromPostViewSize(string val)
        {
            switch (val)
            {
                case PostviewSizeParam.Px2M:
                    return SystemUtil.GetStringResource("Size2M");
                case PostviewSizeParam.Original:
                    return SystemUtil.GetStringResource("SizeOriginal");
                default:
                    return val;
            }
        }

        public static Capability<string> FromShootMode(Capability<string> info)
        {
            return AsDisplayNames<string>(info, FromShootMode);
        }

        private static string FromShootMode(string val)
        {
            switch (val)
            {
                case ShootModeParam.Movie:
                    return SystemUtil.GetStringResource("ShootModeMovie");
                case ShootModeParam.Still:
                    return SystemUtil.GetStringResource("ShootModeStill");
                case ShootModeParam.Audio:
                    return SystemUtil.GetStringResource("ShootModeAudio");
                case ShootModeParam.Interval:
                    return SystemUtil.GetStringResource("ShootModeIntervalStill");
                case ShootModeParam.Loop:
                    return SystemUtil.GetStringResource("LoopRec");
                default:
                    return val;
            }
        }

        public static Capability<string> FromExposureMode(Capability<string> info)
        {
            return AsDisplayNames<string>(info, FromExposureMode);
        }

        private static string FromExposureMode(string val)
        {
            switch (val)
            {
                case ExposureMode.Aperture:
                    return SystemUtil.GetStringResource("ExposureMode_A");
                case ExposureMode.SS:
                    return SystemUtil.GetStringResource("ExposureMode_S");
                case ExposureMode.Program:
                    return SystemUtil.GetStringResource("ExposureMode_P");
                case ExposureMode.Superior:
                    return SystemUtil.GetStringResource("ExposureMode_sA");
                case ExposureMode.Intelligent:
                    return SystemUtil.GetStringResource("ExposureMode_iA");
                case ExposureMode.Manual:
                    return SystemUtil.GetStringResource("ExposureMode_M");
                default:
                    return val;
            }
        }

        public static Capability<string> FromSteadyMode(Capability<string> info)
        {
            return AsDisplayNames<string>(info, FromSteadyMode);
        }

        private static string FromSteadyMode(string val)
        {
            switch (val)
            {
                case SteadyMode.On:
                    return SystemUtil.GetStringResource("On");
                case SteadyMode.Off:
                    return SystemUtil.GetStringResource("Off");
                default:
                    return val;
            }
        }

        public static Capability<string> FromBeepMode(Capability<string> info)
        {
            return AsDisplayNames<string>(info, FromBeepMode);
        }

        private static string FromBeepMode(string val)
        {
            switch (val)
            {
                case BeepMode.On:
                    return SystemUtil.GetStringResource("On");
                case BeepMode.Off:
                    return SystemUtil.GetStringResource("Off");
                case BeepMode.Shutter:
                    return SystemUtil.GetStringResource("BeepModeShutterOnly");
                default:
                    return val;
            }
        }

        public static Capability<string> FromViewAngle(Capability<int> info)
        {
            return AsDisplayNames<int>(info, FromViewAngle);
        }

        private static string FromViewAngle(int val)
        {
            return val + SystemUtil.GetStringResource("ViewAngleUnit");
        }

        public static Capability<string> FromMovieQuality(Capability<string> info)
        {
            return AsDisplayNames<string>(info, FromMovieQuality);
        }

        private static string FromMovieQuality(string p)
        {
            return p;
        }

        public static Capability<string> FromStillImageSize(Capability<StillImageSize> info)
        {
            return AsDisplayNames<StillImageSize>(info, FromStillImageSize);
        }

        private static string FromStillImageSize(StillImageSize val)
        {
            return val.SizeDefinition + " (" + val.AspectRatio + ")";
        }

        private static readonly char[] StillImageSizeIndicators = { '(', ')' };

        public static StillImageSize ToStillImageSize(string val)
        {
            var array = val.Split(StillImageSizeIndicators);
            if (array == null || array.Length != 2)
            {
                throw new ArgumentException("Failed to convert " + val + " to StillImageSize");
            }
            return new StillImageSize
            {
                AspectRatio = array[1].Trim(),
                SizeDefinition = array[2].Trim()
            };
        }

        public static Capability<string> FromWhiteBalance(Capability<string> info)
        {
            return AsDisplayNames<string>(info, FromWhiteBalance);
        }

        private static string FromWhiteBalance(string val)
        {
            switch (val)
            {
                case WhiteBalanceMode.Fluorescent_WarmWhite:
                    return SystemUtil.GetStringResource("WB_Fluorescent_WarmWhite");
                case WhiteBalanceMode.Fluorescent_CoolWhite:
                    return SystemUtil.GetStringResource("WB_Fluorescent_CoolWhite");
                case WhiteBalanceMode.Fluorescent_DayLight:
                    return SystemUtil.GetStringResource("WB_Fluorescent_DayLight");
                case WhiteBalanceMode.Fluorescent_DayWhite:
                    return SystemUtil.GetStringResource("WB_Fluorescent_DayWhite");
                case WhiteBalanceMode.Incandescent:
                    return SystemUtil.GetStringResource("WB_Incandescent");
                case WhiteBalanceMode.Shade:
                    return SystemUtil.GetStringResource("WB_Shade");
                case WhiteBalanceMode.Auto:
                    return SystemUtil.GetStringResource("WB_Auto");
                case WhiteBalanceMode.Cloudy:
                    return SystemUtil.GetStringResource("WB_Cloudy");
                case WhiteBalanceMode.DayLight:
                    return SystemUtil.GetStringResource("WB_DayLight");
                case WhiteBalanceMode.Manual:
                    return SystemUtil.GetStringResource("WB_ColorTemperture");
                case WhiteBalanceMode.Flash:
                    return SystemUtil.GetStringResource("WB_Flash");
                case WhiteBalanceMode.Custom:
                    return SystemUtil.GetStringResource("WB_Custom");
                case WhiteBalanceMode.Custom_1:
                    return SystemUtil.GetStringResource("WB_Custom1");
                case WhiteBalanceMode.Custom_2:
                    return SystemUtil.GetStringResource("WB_Custom2");
                case WhiteBalanceMode.Custom_3:
                    return SystemUtil.GetStringResource("WB_Custom3");
            }
            return val;
        }

        public static List<string> FromExposureCompensation(EvCapability info)
        {
            if (info == null)
            {
                var disabled = SystemUtil.GetStringResource("Disabled");
                var list = new List<string>();
                list.Add(disabled);
                return list;
            }

            int num = info.Candidate.MaxIndex + Math.Abs(info.Candidate.MinIndex) + 1;
            var mCandidates = new List<string>(num);
            for (int i = 0; i < num; i++)
            {
                DebugUtil.Log("ev: " + i);
                mCandidates.Add(FromExposureCompensation(i + info.Candidate.MinIndex, info.Candidate.IndexStep));
            }

            return mCandidates;
        }

        private static string FromExposureCompensation(int index, EvStepDefinition def)
        {
            var value = EvConverter.GetEv(index, def);
            var strValue = Math.Round(value, 1, MidpointRounding.AwayFromZero).ToString("0.0");

            if (value <= 0)
            {
                return "EV " + strValue;
            }
            else
            {
                return "EV +" + strValue;
            }
        }

        public static Capability<string> FromFlashMode(Capability<string> info)
        {
            return AsDisplayNames<string>(info, FromFlashMode);
        }

        private static string FromFlashMode(string val)
        {
            switch (val)
            {
                case FlashMode.Auto:
                    return SystemUtil.GetStringResource("FlashMode_Auto");
                case FlashMode.On:
                    return SystemUtil.GetStringResource("On");
                case FlashMode.Off:
                    return SystemUtil.GetStringResource("Off");
                case FlashMode.RearSync:
                    return SystemUtil.GetStringResource("FlashMode_RearSync");
                case FlashMode.SlowSync:
                    return SystemUtil.GetStringResource("FlashMode_SlowSync");
                case FlashMode.Wireless:
                    return SystemUtil.GetStringResource("FlashMode_Wireless");
            }
            return val;
        }

        public static Capability<string> FromFocusMode(Capability<string> info)
        {
            return AsDisplayNames<string>(info, FromFocusMode);
        }

        private static string FromFocusMode(string val)
        {
            switch (val)
            {
                case FocusMode.Continuous:
                    return SystemUtil.GetStringResource("FocusMode_AFC");
                case FocusMode.Single:
                    return SystemUtil.GetStringResource("FocusMode_AFS");
                case FocusMode.Manual:
                    return SystemUtil.GetStringResource("FocusMode_Manual");
            }
            return val;
        }

        internal static Capability<string> FromZoomSetting(Capability<string> info)
        {
            return AsDisplayNames<string>(info, FromZoomSetting);
        }

        private static string FromZoomSetting(string val)
        {
            switch (val)
            {
                case ZoomMode.ClearImageDigital:
                    return SystemUtil.GetStringResource("ZoomMode_ClearImageDigital");
                case ZoomMode.Optical:
                    return SystemUtil.GetStringResource("ZoomMode_Optical");
            }
            return val;
        }

        internal static Capability<string> FromStillQuality(Capability<string> info)
        {
            return AsDisplayNames<string>(info, FromStillQuality);
        }

        private static string FromStillQuality(string val)
        {
            switch (val)
            {
                case ImageQuality.RawAndJpeg:
                    return SystemUtil.GetStringResource("StillQuality_RawAndJpeg");
                case ImageQuality.Fine:
                    return SystemUtil.GetStringResource("StillQuality_Fine");
                case ImageQuality.Standard:
                    return SystemUtil.GetStringResource("StillQuality_Standard");
            }
            return val;
        }

        internal static Capability<string> FromContShootingMode(Capability<string> info)
        {
            return AsDisplayNames<string>(info, FromContShootingMode);
        }

        private static string FromContShootingMode(string val)
        {
            switch (val)
            {
                case ContinuousShootMode.Single:
                    return SystemUtil.GetStringResource("ContinuousShootMode_Single");
                case ContinuousShootMode.Cont:
                    return SystemUtil.GetStringResource("ContinuousShootMode_Cont");
                case ContinuousShootMode.SpeedPriority:
                    return SystemUtil.GetStringResource("ContinuousShootMode_SpeedPriority");
                case ContinuousShootMode.Burst:
                    return SystemUtil.GetStringResource("ContinuousShootMode_Burst");
                case ContinuousShootMode.MotionShot:
                    return SystemUtil.GetStringResource("ContinuousShootMode_MotionShot");
            }
            return val;
        }

        internal static Capability<string> FromContShootingSpeed(Capability<string> info)
        {
            return AsDisplayNames<string>(info, FromContShootingSpeed);
        }

        private static string FromContShootingSpeed(string val)
        {
            switch (val)
            {
                case ContinuousShootSpeed.FixedFrames_10_In_1_25Sec:
                    return SystemUtil.GetStringResource("ContinuousShootSpeed_FixedFrames_10_In_1_25Sec");
                case ContinuousShootSpeed.FixedFrames_10_In_2Sec:
                    return SystemUtil.GetStringResource("ContinuousShootSpeed_FixedFrames_10_In_2Sec");
                case ContinuousShootSpeed.FixedFrames_10_In_5Sec:
                    return SystemUtil.GetStringResource("ContinuousShootSpeed_FixedFrames_10_In_5Sec");
                case ContinuousShootSpeed.High:
                    return SystemUtil.GetStringResource("ContinuousShootSpeed_High");
                case ContinuousShootSpeed.Low:
                    return SystemUtil.GetStringResource("ContinuousShootSpeed_Low");
            }
            return val;
        }

        internal static Capability<string> FromFlipMode(Capability<string> info)
        {
            return AsDisplayNames<string>(info, FromFlipMode);
        }

        private static string FromFlipMode(string val)
        {
            switch (val)
            {
                case FlipMode.On:
                    return SystemUtil.GetStringResource("On");
                case FlipMode.Off:
                    return SystemUtil.GetStringResource("Off");
            }
            return val;
        }

        internal static Capability<string> FromSceneSelection(Capability<string> info)
        {
            return AsDisplayNames<string>(info, FromSceneSelection);
        }

        private static string FromSceneSelection(string val)
        {
            switch (val)
            {
                case Scene.Normal:
                    return SystemUtil.GetStringResource("Scene_Normal");
                case Scene.UnderWater:
                    return SystemUtil.GetStringResource("Scene_UnderWater");
            }
            return val;
        }

        internal static Capability<string> FromIntervalTime(Capability<string> info)
        {
            return AsDisplayNames<string>(info, FromIntervalTime);
        }

        private static string FromIntervalTime(string val)
        {
            return val + " " + SystemUtil.GetStringResource("Seconds");
        }

        internal static Capability<string> FromColorSetting(Capability<string> info)
        {
            return AsDisplayNames<string>(info, FromColorSetting);
        }

        private static string FromColorSetting(string val)
        {
            switch (val)
            {
                case ColorMode.Neutral:
                    return SystemUtil.GetStringResource("ColorMode_Neutral");
                case ColorMode.Vivid:
                    return SystemUtil.GetStringResource("ColorMode_Vivid");
            }
            return val;
        }

        internal static Capability<string> FromMovieFileFormat(Capability<string> info)
        {
            return AsDisplayNames<string>(info, FromMovieFileFormat);
        }

        private static string FromMovieFileFormat(string val)
        {
            switch (val)
            {
                case MovieFormatMode.MP4:
                    return SystemUtil.GetStringResource("MovieFormatMode_MP4");
                case MovieFormatMode.XAVCS:
                    return SystemUtil.GetStringResource("MovieFormatMode_XAVCS");
            }
            return val;
        }

        internal static Capability<string> FromInfraredRemoteControl(Capability<string> info)
        {
            return AsDisplayNames<string>(info, FromInfraredRemoteControl);
        }

        private static string FromInfraredRemoteControl(string val)
        {
            switch (val)
            {
                case IrRemoteSetting.On:
                    return SystemUtil.GetStringResource("On");
                case IrRemoteSetting.Off:
                    return SystemUtil.GetStringResource("Off");
            }
            return val;
        }

        internal static Capability<string> FromTvColorSystem(Capability<string> info)
        {
            return AsDisplayNames<string>(info, FromTvColorSystem);
        }

        private static string FromTvColorSystem(string val)
        {
            switch (val)
            {
                case TvColorSystemMode.NTSC:
                    return SystemUtil.GetStringResource("TvColorSystemMode_NTSC");
                case TvColorSystemMode.PAL:
                    return SystemUtil.GetStringResource("TvColorSystemMode_PAL");
            }
            return val;
        }

        internal static Capability<string> FromTrackingFocus(Capability<string> info)
        {
            return AsDisplayNames<string>(info, FromTrackingFocus);
        }

        private static string FromTrackingFocus(string val)
        {
            switch (val)
            {
                case TrackingFocusMode.On:
                    return SystemUtil.GetStringResource("On");
                case TrackingFocusMode.Off:
                    return SystemUtil.GetStringResource("Off");
            }
            return val;
        }

        internal static Capability<string> FromAutoPowerOff(Capability<int> info)
        {
            return AsDisplayNames<int>(info, FromAutoPowerOff);
        }

        private static string FromAutoPowerOff(int val)
        {
            if (val == 0) { return SystemUtil.GetStringResource("AutoPowerOff_Never"); }
            return val + " " + SystemUtil.GetStringResource("Seconds");
        }

        internal static Capability<string> FromLoopRecTime(Capability<string> info)
        {
            return AsDisplayNames<string>(info, FromLoopRecTime);
        }

        private static string FromLoopRecTime(string val)
        {
            switch (val)
            {
                case LoopTime.MIN_5:
                    return "5" + SystemUtil.GetStringResource("Minute_Unit");
                case LoopTime.MIN_20:
                    return "20" + SystemUtil.GetStringResource("Minute_Unit");
                case LoopTime.MIN_60:
                    return "60" + SystemUtil.GetStringResource("Minute_Unit");
                case LoopTime.MIN_120:
                    return "120" + SystemUtil.GetStringResource("Minute_Unit");
                case LoopTime.UNLIMITED:
                    return SystemUtil.GetStringResource("LoopTime_Unlimited");
            }
            return val;
        }

        internal static Capability<string> FromWindNoiseReduction(Capability<string> info)
        {
            return AsDisplayNames<string>(info, FromWindNoiseReduction);
        }

        private static string FromWindNoiseReduction(string val)
        {
            switch (val)
            {
                case WindNoiseReductionMode.On:
                    return SystemUtil.GetStringResource("On");
                case WindNoiseReductionMode.Off:
                    return SystemUtil.GetStringResource("Off");
            }
            return val;
        }

        internal static Capability<string> FromAudioRecording(Capability<string> info)
        {
            return AsDisplayNames<string>(info, FromAudioRecording);
        }

        private static string FromAudioRecording(string val)
        {
            switch (val)
            {
                case AudioRecordingMode.On:
                    return SystemUtil.GetStringResource("On");
                case AudioRecordingMode.Off:
                    return SystemUtil.GetStringResource("Off");
            }
            return val;
        }

        internal static string[] FromFramingGrid(IEnumerable<FramingGridTypes> keys)
        {
            return keys.Select(key =>
            {
                switch (key)
                {
                    case FramingGridTypes.Off:
                        return null;
                    case FramingGridTypes.RuleOfThirds:
                        return SystemUtil.GetStringResource("Grid_RuleOfThirds");
                    case FramingGridTypes.Diagonal:
                        return SystemUtil.GetStringResource("Grid_Diagonal");
                    case FramingGridTypes.Square:
                        return SystemUtil.GetStringResource("Grid_Square");
                    case FramingGridTypes.Crosshairs:
                        return SystemUtil.GetStringResource("Grid_Crosshairs");
                    case FramingGridTypes.Fibonacci:
                        return SystemUtil.GetStringResource("Grid_Fibonacci");
                    case FramingGridTypes.GoldenRatio:
                        return SystemUtil.GetStringResource("Grid_GoldenRatio");
                    default:
                        return null;
                }
            }).Where(val => val != null).ToArray();
        }

        internal static string[] FromFramingGridColor(IEnumerable<FramingGridColors> keys)
        {
            return keys.Select(key =>
            {
                switch (key)
                {
                    case FramingGridColors.White:
                        return SystemUtil.GetStringResource("White");
                    case FramingGridColors.Black:
                        return SystemUtil.GetStringResource("Black");
                    case FramingGridColors.Red:
                        return SystemUtil.GetStringResource("Red");
                    case FramingGridColors.Green:
                        return SystemUtil.GetStringResource("Green");
                    case FramingGridColors.Blue:
                        return SystemUtil.GetStringResource("Blue");
                    default:
                        throw new NotImplementedException();
                }
            }).ToArray();
        }

        internal static string[] FromFibonacciLineOrigin(IEnumerable<FibonacciLineOrigins> keys)
        {
            return keys.Select(key =>
            {
                switch (key)
                {
                    case FibonacciLineOrigins.UpperLeft:
                        return SystemUtil.GetStringResource("UpperLeft");
                    case FibonacciLineOrigins.UpperRight:
                        return SystemUtil.GetStringResource("UpperRight");
                    case FibonacciLineOrigins.BottomLeft:
                        return SystemUtil.GetStringResource("BottomLeft");
                    case FibonacciLineOrigins.BottomRight:
                        return SystemUtil.GetStringResource("BottomRight");
                    default:
                        throw new NotImplementedException();
                }
            }).ToArray();
        }

        internal static string[] FromContentsSet(IEnumerable<ContentsSet> types)
        {
            // TODO
            return types.Select(type =>
            {
                switch (type)
                {
                    case ContentsSet.ImagesAndMovies:
                        return SystemUtil.GetStringResource("ContentsSet_PictureMovie");
                    case ContentsSet.Images:
                        return SystemUtil.GetStringResource("ContentsSet_Picture");
                    case ContentsSet.Movies:
                        return SystemUtil.GetStringResource("ContentsSet_Movie");
                    default:
                        throw new NotImplementedException();
                }
            }).ToArray();
        }
    }
}
