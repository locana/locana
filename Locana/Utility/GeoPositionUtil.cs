using Naotaco.Jpeg.MetaData;
using Naotaco.Jpeg.MetaData.Misc;
using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;
using Windows.Foundation;
using Windows.UI.Core;
using Windows.UI.Xaml;

namespace Locana.Utility
{
    class GeopositionUtil
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
