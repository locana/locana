using Kazyx.Uwpmm.DataModel;
using Kazyx.Uwpmm.Utility;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Kazyx.Uwpmm.Playback
{
    public abstract class ContentsLoader
    {
        public event EventHandler<ContentsLoadedEventArgs> PartLoaded;

        protected void OnPartLoaded(IList<Thumbnail> contents)
        {
            DebugUtil.Log("ContentsLoader OnPartLoaded");
            PartLoaded.Raise(this, new ContentsLoadedEventArgs(contents));
        }

        public event EventHandler Completed;

        protected void OnCompleted()
        {
            DebugUtil.Log("ContentsLoader OnCompleted");
            Completed.Raise(this, null);
        }

        public event EventHandler Cancelled;

        protected void OnCancelled()
        {
            DebugUtil.Log("ContentsLoader OnCancelled");
            Cancelled.Raise(this, null);
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
