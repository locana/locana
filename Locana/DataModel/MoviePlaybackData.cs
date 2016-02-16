using System;

namespace Locana.DataModel
{
    public class MoviePlaybackData : ImageDataSource
    {
        public MoviePlaybackData() { }

        private TimeSpan _CurrentPosition = TimeSpan.FromMilliseconds(0);
        public TimeSpan CurrentPosition
        {
            get { return _CurrentPosition; }
            set
            {
                _CurrentPosition = value;
                NotifyChangedOnUI(nameof(CurrentPosition));
            }
        }

        private TimeSpan _Duration = TimeSpan.FromMilliseconds(0);
        public TimeSpan Duration
        {
            get { return _Duration; }
            set
            {
                _Duration = value;
                NotifyChangedOnUI(nameof(Duration));
            }
        }

        private string _FileName = "";
        public string FileName
        {
            get { return _FileName; }
            set
            {
                if (_FileName != value)
                {
                    _FileName = value;
                    NotifyChangedOnUI(nameof(FileName));
                }
            }
        }

        private bool _SeekAvailable = false;
        public bool SeekAvailable
        {
            get { return _SeekAvailable; }
            set
            {
                if (_SeekAvailable != value)
                {
                    _SeekAvailable = value;
                    NotifyChangedOnUI(nameof(SeekAvailable));
                }
            }
        }

        private string _StreamingStatus = "";
        public string StreamingStatus
        {
            get { return _StreamingStatus; }
            set
            {
                if (_StreamingStatus != value)
                {
                    _StreamingStatus = value;
                    NotifyChangedOnUI(nameof(StreamingStatus));
                }
            }
        }

        private string _StreamingStatusTransitionFactor = "";
        public string StreamingStatusTransitionFactor
        {
            get { return _StreamingStatusTransitionFactor; }
            set
            {
                if (_StreamingStatusTransitionFactor != value)
                {
                    _StreamingStatusTransitionFactor = value;
                    NotifyChangedOnUI(nameof(StreamingStatusTransitionFactor));
                }
            }
        }
    }
}
