using System;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;
using Windows.Foundation;
using Windows.UI.Core;
using Windows.UI.Xaml;

namespace Kazyx.Uwpmm.Utility
{
    class GeopositionManager
    {
        private static GeopositionManager _GeopositionManager = new GeopositionManager();

        private Geoposition _LatestPosition;
        internal Geoposition LatestPosition
        {
            get { return _LatestPosition; }
            set
            {
                _LatestPosition = value;
                _TimeStamp = DateTime.Now;
                DebugUtil.Log("Geoposition updated: " + TimeStamp.ToString());
            }
        }

        private DateTime _TimeStamp;
        internal DateTime TimeStamp { get { return _TimeStamp; } }

        public static GeopositionManager INSTANCE
        {
            get { return _GeopositionManager; }
        }

        public void Clear()
        {
            _LatestPosition = null;
        }
    }
}
