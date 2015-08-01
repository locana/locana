using System.Threading;
using System.Threading.Tasks;

namespace Kazyx.Uwpmm.DataModel
{
    public class AsyncConversionSource<TResult> : ObservableBase
    {
        public Task<TResult> Task { get; private set; }

        public AsyncConversionSource(Task<TResult> task)
        {
            Task = task;

            if (!task.IsCompleted)
            {
                var scheduler = (SynchronizationContext.Current == null) ? TaskScheduler.Current : TaskScheduler.FromCurrentSynchronizationContext();
                task.ContinueWith(t =>
                {
                    NotifyChangedOnUI("IsCompleted");
                    if (t.IsCanceled)
                    {
                        NotifyChangedOnUI("IsCanceled");
                    }
                    else if (t.IsFaulted)
                    {
                        NotifyChangedOnUI("IsFaulted");
                        NotifyChangedOnUI("ErrorMessage");
                    }
                    else
                    {
                        NotifyChangedOnUI("IsSuccessfullyCompleted");
                        NotifyChangedOnUI("Result");
                    }
                },
                CancellationToken.None,
                TaskContinuationOptions.ExecuteSynchronously,
                scheduler);
            }
        }

        public TResult Result { get { return (Task.Status == TaskStatus.RanToCompletion) ? Task.Result : default(TResult); } }

        public bool IsCompleted { get { return Task.IsCompleted; } }

        public bool IsSuccessfullyCompleted { get { return Task.Status == TaskStatus.RanToCompletion; } }

        public bool IsCanceled { get { return Task.IsCanceled; } }

        public bool IsFaulted { get { return Task.IsFaulted; } }
    }
}
