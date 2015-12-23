using Kazyx.Uwpmm.Utility;
using Naotaco.ImageProcessor.MetaData.Structure;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Kazyx.Uwpmm.DataModel
{
    public class PhotoPlaybackData : ObservableBase
    {
        public PhotoPlaybackData() { }
        private ObservableCollection<EntryViewData> _EntryList = new ObservableCollection<EntryViewData>();
        public ObservableCollection<EntryViewData> EntryList
        {
            get { return _EntryList; }
            set
            {
                _EntryList = value;
                NotifyChangedOnUI(nameof(EntryList));
            }
        }

        private JpegMetaData _MetaData;
        public JpegMetaData MetaData
        {
            get { return _MetaData; }
            set
            {
                _MetaData = value;
                if (value == null)
                {
                    ShowInvalidData();
                }
                else
                {
                    UpdateEntryList(value);
                    if (EntryList.Count == 0)
                    {
                        ShowInvalidData();
                    }
                }
            }
        }

        uint[] GeneralMetaDataKeys = new uint[]
        {
            ExifKeys.Fnumber,
            ExifKeys.Iso,
            ExifKeys.DateTime,
        };

        void ShowInvalidData()
        {
            EntryList.Clear();
            EntryList.Add(CreateEntry("NO DATA"));
        }

        private void UpdateEntryList(JpegMetaData metadata)
        {
            EntryList.Clear();

            var exposureModeEntry = FindFirstEntry(metadata, ExifKeys.ExposureProgram);
            if (exposureModeEntry != null)
            {
                EntryList.Add(CreateEntry(MetaDataValueConverter.MetaDataEntryName(ExifKeys.ExposureProgram),
                    MetaDataValueConverter.ExposuteProgramName(exposureModeEntry.UIntValues[0])));
            }

            var ssEntry = FindFirstEntry(metadata, ExifKeys.ExposureTime);
            if (ssEntry != null)
            {
                EntryList.Add(CreateEntry(MetaDataValueConverter.MetaDataEntryName(ExifKeys.ExposureTime),
                 MetaDataValueConverter.ShutterSpeed(ssEntry.UFractionValues[0].Numerator, ssEntry.UFractionValues[0].Denominator)));
            }

            var focalLengthEntry = FindFirstEntry(metadata, ExifKeys.FocalLength);
            if (focalLengthEntry != null)
            {
                EntryList.Add(CreateEntry(MetaDataValueConverter.MetaDataEntryName(ExifKeys.FocalLength),
                    GetStringValue(metadata, ExifKeys.FocalLength) + "mm"));
            }

            foreach (uint key in GeneralMetaDataKeys)
            {
                if (FindFirstEntry(metadata, key) == null) { continue; }
                EntryList.Add(CreateEntry(MetaDataValueConverter.MetaDataEntryName(key),
                     GetStringValue(metadata, key)));
            }

            var wbEntry = FindFirstEntry(metadata, ExifKeys.WhiteBalanceMode);
            var wbDetailEntry = FindFirstEntry(metadata, ExifKeys.WhiteBalanceDetailType);
            if (wbEntry != null && wbDetailEntry != null)
            {
                string value;
                if (wbEntry.UIntValues[0] == 0x0)
                {
                    value = SystemUtil.GetStringResource("WB_Auto");
                }
                else
                {
                    value = MetaDataValueConverter.WhitebalanceName(wbDetailEntry.UIntValues[0]);
                }
                EntryList.Add(CreateEntry(MetaDataValueConverter.MetaDataEntryName(ExifKeys.WhiteBalanceMode), value));
            }

            var evEntry = FindFirstEntry(metadata, ExifKeys.ExposureCompensation);
            if (evEntry != null)
            {
                EntryList.Add(CreateEntry(SystemUtil.GetStringResource("MetaDataName_ExposureCompensation"), MetaDataValueConverter.EV(evEntry.DoubleValues[0])));
            }

            var meteringModeEntry = FindFirstEntry(metadata, ExifKeys.MeteringMode);
            if (meteringModeEntry != null)
            {
                EntryList.Add(CreateEntry(SystemUtil.GetStringResource("MetaDataName_MeteringMode"), MetaDataValueConverter.MeteringModeName(meteringModeEntry.UIntValues[0])));
            }

            var flashEntry = FindFirstEntry(metadata, ExifKeys.Flash);
            if (flashEntry != null)
            {
                EntryList.Add(CreateEntry(SystemUtil.GetStringResource("MetaDataName_Flash"), MetaDataValueConverter.FlashNames(flashEntry.UIntValues[0])));
            }

            var heightEntry = FindFirstEntry(metadata, ExifKeys.ImageHeight);
            var widthEntry = FindFirstEntry(metadata, ExifKeys.ImageWidth);
            if (heightEntry != null && widthEntry != null)
            {
                EntryList.Add(CreateEntry(SystemUtil.GetStringResource("MetaDataName_ImageSize"),
                    widthEntry.UIntValues[0] + " x " + heightEntry.UIntValues[0]));
            }

            var makerEntry = FindFirstEntry(metadata, ExifKeys.CameraMaker);
            var modelEntry = FindFirstEntry(metadata, ExifKeys.CameraModel);
            if (makerEntry != null && modelEntry != null)
            {
                EntryList.Add(CreateEntry(MetaDataValueConverter.MetaDataEntryName(ExifKeys.CameraModel),
                     makerEntry.StringValue + " " + modelEntry.StringValue));
            }

            var lensNameEntry = FindFirstEntry(metadata, ExifKeys.LensModel);
            if (lensNameEntry != null)
            {
                EntryList.Add(CreateEntry(MetaDataValueConverter.MetaDataEntryName(ExifKeys.LensModel), lensNameEntry.StringValue));
            }

            var geotagEntry = FindFirstEntry(metadata, ExifKeys.GpsLatitude);
            if (geotagEntry != null)
            {
                EntryList.Add(CreateEntry(SystemUtil.GetStringResource("MetaData_Geotag"), MetaDataValueConverter.Geoinfo(metadata.GpsIfd)));
            }
        }

        EntryViewData CreateEntry(string name, string value = "")
        {
            return new EntryViewData()
            {
                Name = name,
                ValuesList = new List<string>() { value },
            };
        }

        EntryViewData CreateEntry(string name, List<string> value)
        {
            return new EntryViewData()
            {
                Name = name,
                ValuesList = value,
            };
        }

        string GetStringValue(JpegMetaData metadata, uint key)
        {
            var entry = FindFirstEntry(metadata, key);
            if (entry == null) { return "null"; }
            switch (entry.Type)
            {
                case Entry.EntryType.Ascii:
                    return entry.StringValue;
                case Entry.EntryType.Byte:
                    return entry.value.ToString();
                case Entry.EntryType.Long:
                case Entry.EntryType.Short:
                    return entry.UIntValues[0].ToString();
                case Entry.EntryType.SLong:
                case Entry.EntryType.SShort:
                    return entry.SIntValues[0].ToString();
                case Entry.EntryType.Rational:
                case Entry.EntryType.SRational:
                    return entry.DoubleValues[0].ToString();
            }
            return "--";
        }

        Entry FindFirstEntry(JpegMetaData metadata, uint key)
        {
            if (metadata == null) { return null; }

            if (metadata.PrimaryIfd != null && metadata.PrimaryIfd.Entries.ContainsKey(key))
            {
                return metadata.PrimaryIfd.Entries[key];
            }
            else if (metadata.ExifIfd != null && metadata.ExifIfd.Entries.ContainsKey(key))
            {
                return metadata.ExifIfd.Entries[key];
            }
            else if (metadata.GpsIfd != null && metadata.GpsIfd.Entries.ContainsKey(key))
            {
                return metadata.GpsIfd.Entries[key];
            }
            return null;
        }
    }

    public class EntryViewData
    {
        public string Name { get; set; }
        public List<string> ValuesList { get; set; }
    }

    public static class ExifKeys
    {
        public const uint Fnumber = 0x829D;
        public const uint ExposureTime = 0x829A;
        public const uint Iso = 0x8827;
        public const uint FocalLength = 0x920A;
        public const uint CameraModel = 0x0110;
        public const uint CameraMaker = 0x010F;
        public const uint ImageWidth = 0xA002;
        public const uint ImageHeight = 0xA003;
        public const uint DateTime = 0x9003;
        public const uint ExposureProgram = 0x8822;
        public const uint WhiteBalanceMode = 0xA403;
        public const uint WhiteBalanceDetailType = 0x9208;
        public const uint DocumentName = 0x010D;
        public const uint ExposureCompensation = 0x9204;
        public const uint MeteringMode = 0x9207;
        public const uint Flash = 0x9209;
        public const uint LensModel = 0xA434;
        public const uint GpsLatitude = 0x01;
    }
}
