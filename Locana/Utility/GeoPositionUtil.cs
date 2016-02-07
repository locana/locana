using Naotaco.Jpeg.MetaData;
using Naotaco.Jpeg.MetaData.Misc;
using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;

namespace Locana.Utility
{
    public class GeolocatorManager
    {
        private GeolocatorManager() { }

        public static GeolocatorManager INSTANCE = new GeolocatorManager();

        private Geoposition _LatestPosition = null;
        private Geoposition LatestPosition
        {
            get { return _LatestPosition; }
            set
            {
                _LatestPosition = value;
                GeopositionChanged?.Invoke(_LatestPosition);
            }
        }

        public bool IsRunning { get { return geolocator != null; } }

        /// <summary>
        /// Returns the latest position. In case failed to acquire or declined to access, null will be returned.
        /// </summary>
        /// <returns></returns>
        public async Task<Geoposition> GetLatestPosition()
        {
            if (LatestPosition == null)
            {
                LatestPosition = await GetGeoposition();
            }

            return LatestPosition;
        }

        private Geolocator geolocator = null;

        public Action<Geoposition> GeopositionChanged;

        /// <summary>
        /// Start to acquire current location. Need to be called from UI thread.
        /// </summary>
        /// <returns></returns>
        public async Task<GeolocationAccessStatus> Start()
        {
            var accessStatus = await Geolocator.RequestAccessAsync();

            switch (accessStatus)
            {
                case GeolocationAccessStatus.Allowed:
                    geolocator = new Geolocator();
                    geolocator.PositionChanged += Geolocator_PositionChanged;
                    LatestPosition = await geolocator.GetGeopositionAsync();
                    break;
            }

            return accessStatus;
        }

        private void Geolocator_PositionChanged(Geolocator sender, PositionChangedEventArgs args)
        {
            DebugUtil.Log(() => { return "Location updated: " + args.Position.Coordinate.Longitude + " , " + args.Position.Coordinate.Latitude; });
            LatestPosition = args.Position;
        }

        public void Stop()
        {
            if (geolocator != null)
            {
                geolocator.PositionChanged -= Geolocator_PositionChanged;
            }
            
            LatestPosition = null;
            geolocator = null;
        }

        private async Task<Geoposition> GetGeoposition()
        {
            if (geolocator == null) { return null; }
            return await geolocator?.GetGeopositionAsync();
        }
    }

    static class GeopositionUtil
    {
        public static async Task<GeotaggingResult> AddGeotag(Stream image, Geoposition location)
        {
            var result = new GeotaggingResult()
            {
                OperationResult = GeotaggingResult.Result.OK,
                Image = image,
            };

            // From UWP, a stream from HttpResponseMessage doesn't support seeking.
            if (!image.CanSeek)
            {
                var original = image;
                var seekable = new MemoryStream();
                image.CopyTo(seekable);
                seekable.Seek(0, SeekOrigin.Begin);
                image = seekable;
                original.Dispose();
            }

            try
            {
                result.Image = await MetaDataOperator.AddGeopositionAsync(image, location, false);
            }
            catch (GpsInformationAlreadyExistsException)
            {
                result.OperationResult = GeotaggingResult.Result.GeotagAlreadyExists;
            }
            catch
            {
                result.OperationResult = GeotaggingResult.Result.UnExpectedError;
            }

            return result;
        }
    }
}
