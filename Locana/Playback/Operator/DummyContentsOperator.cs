#if DEBUG
using Locana.DataModel;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
#endif

namespace Locana.Playback.Operator
{
#if DEBUG
    public class DummyContentsOperator : RemoteContentsOperator
    {
        public DummyContentsOperator()
        {
            TitleText = "Dummy storage";
            ContentsCollection = new AlbumGroupCollection();
        }

        public override Task DeleteSelectedFile(Thumbnail item)
        {
            ContentsCollection.Remove(item);

            return Task.Delay(100);
        }

        public override Task DeleteSelectedFiles(IEnumerable<Thumbnail> items)
        {
            foreach (var item in items)
            {
                ContentsCollection.Remove(item);
            }

            return Task.Delay(1000);
        }

        public override void Dispose()
        {
        }

        public override void FinishMoviePlayback()
        {
        }

        public override async Task LoadContents()
        {
            var loader = new DummyContentsLoader();
            loader.PartLoaded += RemoteContentsLoader_PartLoaded;
            loader.Cancelled += RemoteContentsLoader_Cancelled;
            try
            {
                await loader.Load(ContentsSet.Images, Canceller).ConfigureAwait(false);
            }
            finally
            {
                loader.Cancelled -= RemoteContentsLoader_Cancelled;
                loader.PartLoaded -= RemoteContentsLoader_PartLoaded;
            }
        }

        private void RemoteContentsLoader_Cancelled(object sender, EventArgs e)
        {
            OnLoadCancelled();
        }

        public override async Task LoadRemainingContents(RemainingContentsHolder holder)
        {
            var loader = new DummyContentsLoader();
            loader.PartLoaded += RemoteContentsLoader_PartLoaded;
            try
            {
                await loader.LoadRemainingAsync(holder, ContentsSet.Images, Canceller).ConfigureAwait(false);
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
            throw new NotImplementedException();
        }
    }
#endif
}
