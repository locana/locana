using Locana.DataModel;
using Locana.UPnP;
using Locana.UPnP.ContentDirectory;
using Locana.Utility;
using Naotaco.ImageProcessor.MetaData;
using Naotaco.ImageProcessor.MetaData.Misc;
using Naotaco.ImageProcessor.MetaData.Structure;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Web.Http;

namespace Locana.Playback.Operator
{
    public class DlnaContentsOperator : ContentsOperator
    {
        private UpnpDevice UpnpDevice;
        private HttpClient HttpClient = new HttpClient();

        public DlnaContentsOperator(UpnpDevice upnp)
        {
            UpnpDevice = upnp;
        }

        public override async Task DeleteSelectedFile(Thumbnail item)
        {
            var cds = UpnpDevice.Services[URN.ContentDirectory];

            await DeleteDlnaContentAsync(cds, (item.Source as DlnaContentInfo)?.Id);

            ContentsCollection.Remove(item);
        }

        public override async Task DeleteSelectedFiles(IEnumerable<Thumbnail> items)
        {
            DebugUtil.Log("DeleteSelectedImages");

            var dlna = items
                .Select(item => item.Source as DlnaContentInfo)
                .Where(info => info != null)
                .Select(info => info.Id).ToList();

            await DeleteDlnaContentsAsync(dlna);

            foreach (var item in items)
            {
                ContentsCollection.Remove(item);
            }
        }

        private Task DeleteDlnaContentsAsync(IList<string> objectIdList)
        {
            var cds = UpnpDevice.Services[URN.ContentDirectory];

            return Task.WhenAll(objectIdList.Select(async id =>
            {
                await DeleteDlnaContentAsync(cds, id);
            }));
        }

        private async Task DeleteDlnaContentAsync(UpnpService cds, string id)
        {
            if (id == null)
            {
                return;
            }

            try
            {
                await cds.Control(new DestroyObjectRequest
                {
                    ObjectID = id,
                });
            }
            catch (SoapException e)
            {
                DebugUtil.Log("Failed to delete " + e.StatusCode);
            }
        }

        public override void Dispose()
        {
            HttpClient.Dispose();
        }

        public override void FinishMoviePlayback()
        {
            throw new NotImplementedException("DLNA movie playback is not supported");
        }

        public override async Task LoadContents()
        {
            var loader = new DlnaContentsLoader(UpnpDevice);
            loader.PartLoaded += RemoteContentsLoader_PartLoaded;
            try
            {
                await loader.Load(ContentsSet.Images, Canceller);
            }
            catch (SoapException e)
            {
                DebugUtil.Log("SoapException while loading: " + e.StatusCode);
                OnErrorMessage(SystemUtil.GetStringResource("Viewer_FailedToRefreshContents"));
            }
            finally
            {
                loader.PartLoaded -= RemoteContentsLoader_PartLoaded;
            }
        }

        private void RemoteContentsLoader_PartLoaded(object sender, ContentsLoadedEventArgs e)
        {
            OnPartLoaded(e);
        }

        public override Task PlaybackMovie(Thumbnail item)
        {
            throw new NotImplementedException("DLNA movie playback is not supported");
        }

        public override async Task<Tuple<BitmapImage, JpegMetaData>> PlaybackStillImage(Thumbnail content)
        {
            using (var res = await HttpClient.GetAsync(new Uri(content.Source.LargeUrl)))
            {
                if (!res.IsSuccessStatusCode)
                {
                    OnMovieStreamError();
                    throw new IOException();
                }

                var buff = await res.Content.ReadAsBufferAsync();
                using (var stream = new InMemoryRandomAccessStream())
                {
                    await stream.WriteAsync(buff); // Copy to the new stream to avoid stream crash issue.
                    if (stream.Size == 0)
                    {
                        throw new IOException();
                    }
                    stream.Seek(0);

                    var bitmap = new BitmapImage();
                    await bitmap.SetSourceAsync(stream);
                    try
                    {
                        var meta = await JpegMetaDataParser.ParseImageAsync(stream.AsStream());
                        return Tuple.Create(bitmap, meta);
                    }
                    catch (UnsupportedFileFormatException)
                    {
                        return Tuple.Create<BitmapImage, JpegMetaData>(bitmap, null);
                    }
                }
            }
        }
    }
}
