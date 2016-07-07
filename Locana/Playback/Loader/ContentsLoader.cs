using Locana.DataModel;
using Locana.Utility;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Locana.Playback
{
    public abstract class ContentsLoader
    {
        public event EventHandler<ContentsLoadedEventArgs> PartLoaded;
        public event EventHandler<SingleContentEventArgs> SingleContentLoaded;

        protected void OnPartLoaded(IList<Thumbnail> contents)
        {
            PartLoaded?.Invoke(this, new ContentsLoadedEventArgs(contents));
        }

        protected void OnSingleContentLoaded(Thumbnail file)
        {
            SingleContentLoaded?.Invoke(this, new SingleContentEventArgs { File = file });
        }

        public event EventHandler Completed;

        protected void OnCompleted()
        {
            DebugUtil.Log(() => "ContentsLoader OnCompleted");
            Completed?.Invoke(this, null);
        }

        public event EventHandler Cancelled;

        protected void OnCancelled()
        {
            DebugUtil.Log(() => "ContentsLoader OnCancelled");
            Cancelled?.Invoke(this, null);
        }

        public abstract Task Load(ContentsSet contentsSet, CancellationTokenSource cancel);

        public abstract Task LoadRemainingAsync(RemainingContentsHolder holder, ContentsSet contentsSet, CancellationTokenSource cancel);
    }

    public class ContentsLoadedEventArgs : EventArgs
    {
        public IList<Thumbnail> Contents { get; private set; }

        public ContentsLoadedEventArgs(IList<Thumbnail> contents)
        {
            Contents = contents;
        }
    }

    public enum ContentsSet
    {
        ImagesAndMovies = 0,
        Images = 1,
        Movies = 2,
    }
}
