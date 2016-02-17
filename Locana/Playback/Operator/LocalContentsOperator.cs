﻿using Locana.Controls;
using Locana.DataModel;
using Locana.Utility;
using Naotaco.Jpeg.MetaData;
using Naotaco.Jpeg.MetaData.Misc;
using Naotaco.Jpeg.MetaData.Structure;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Imaging;

namespace Locana.Playback.Operator
{
    public class LocalContentsOperator : ContentsOperator
    {
        private readonly MoviePlaybackScreen MovieScreen;

        public LocalContentsOperator(MoviePlaybackScreen movieScreen)
        {
            TitleText = SystemUtil.GetStringResource("Viewer_StoragePhone");

            ContentsCollection = new AlbumGroupCollection(false)
            {
                ContentSortOrder = Album.SortOrder.NewOneFirst,
            };
            MovieScreen = movieScreen;
            MovieScreen.MovieType = MovieFileType.LocalMovie;
            MovieScreen.LocalMediaFailed += MovieScreen_LocalMediaFailed;
        }

        private void MovieScreen_LocalMediaFailed(object sender, string e)
        {
            DebugUtil.Log("LocalMoviePlayer MediaFailed: " + e);
            OnErrorMessage("Viewer_FailedPlaybackMovie");
            OnMovieStreamError();
        }

        public override Task DeleteSelectedFile(Thumbnail item)
        {
            return TryDeleteLocalFile(item);
        }

        public override Task DeleteSelectedFiles(IEnumerable<Thumbnail> items)
        {
            return Task.WhenAll(items.Select(async item =>
            {
                await TryDeleteLocalFile(item);
            }));
        }

        public override async Task LoadContents()
        {
            var loader = new LocalContentsLoader();
            loader.SingleContentLoaded += Loader_SingleContentLoaded;
            loader.Cancelled += Loader_Cancelled;
            try
            {
                await loader.Load(ContentsSet.Images, Canceller);
            }
            finally
            {
                loader.Cancelled -= Loader_Cancelled;
                loader.SingleContentLoaded -= Loader_SingleContentLoaded;
            }
        }

        private void Loader_Cancelled(object sender, EventArgs e)
        {
            OnLoadCancelled();
        }

        private void Loader_SingleContentLoaded(object sender, SingleContentEventArgs e)
        {
            OnSingleContentLoaded(e);
        }

        public override async Task PlaybackMovie(Thumbnail content)
        {
            var tcs = new TaskCompletionSource<bool>();
            EventHandler<RoutedEventArgs> opened = (sender, e) =>
            {
                tcs.SetResult(true);
            };
            EventHandler<string> failed = (sender, e) =>
            {
                tcs.SetException(new IOException());
            };
            MovieScreen.LocalMediaOpened += opened;
            MovieScreen.LocalMediaFailed += failed;

            try
            {
                MovieScreen.SetLocalContent(content);
                await tcs.Task;
            }
            finally
            {
                MovieScreen.LocalMediaOpened -= opened;
            }
        }

        public override async Task<Tuple<BitmapImage, JpegMetaData>> PlaybackStillImage(Thumbnail content)
        {
            using (var stream = await content.CacheFile.OpenStreamForReadAsync())
            {
                var bitmap = new BitmapImage();
                await bitmap.SetSourceAsync(stream.AsRandomAccessStream());
                try
                {
                    var meta = await JpegMetaDataParser.ParseImageAsync(stream);
                    return Tuple.Create(bitmap, meta);
                }
                catch (UnsupportedFileFormatException)
                {
                    return Tuple.Create<BitmapImage, JpegMetaData>(bitmap, null);
                }
            }
        }

        private async Task TryDeleteLocalFile(Thumbnail data)
        {
            try
            {
                DebugUtil.Log("Delete " + data.CacheFile?.DisplayName);
                await data.CacheFile?.DeleteAsync();
                ContentsCollection.Remove(data);
            }
            catch (Exception ex)
            {
                DebugUtil.Log("Failed to delete file: " + ex.StackTrace);
            }
        }

        public override void FinishMoviePlayback()
        {
            MovieScreen.Finish();
        }

        public override void Dispose()
        {
            MovieScreen.LocalMediaFailed -= MovieScreen_LocalMediaFailed;
        }

        public override Task LoadRemainingContents(RemainingContentsHolder holder)
        {
            throw new NotImplementedException();
        }
    }
}
