using Locana.DataModel;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Locana.Playback.Operator
{
    public class DummyContentsOperator : RemoteContentsOperator
    {
        public DummyContentsOperator()
        {
            TitleText = "Dummy storage";
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
            throw new NotImplementedException();
        }

        public override async Task LoadContents()
        {
            var loader = new DummyContentsLoader();
            loader.PartLoaded += RemoteContentsLoader_PartLoaded;
            try
            {
                await loader.Load(ContentsSet.Images, Canceller).ConfigureAwait(false);
            }
            finally
            {
                loader.PartLoaded -= RemoteContentsLoader_PartLoaded;
            }
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
}
