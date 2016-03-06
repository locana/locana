using System.Threading.Tasks;

namespace Locana.Utility
{
    public static class TaskExtensions
    {
        public static void IgnoreExceptions<T>(this Task<T> task)
        {
            task.ContinueWith(t =>
            {
                DebugUtil.Log(() => "Ignore " + t?.Exception?.InnerException?.GetType());
                DebugUtil.Log(() => t?.Exception?.InnerException?.StackTrace);
            }, TaskContinuationOptions.NotOnRanToCompletion);
        }

        public static void IgnoreExceptions(this Task task)
        {
            task.ContinueWith(t =>
            {
                DebugUtil.Log(() => "Ignore " + t?.Exception?.InnerException?.GetType());
                DebugUtil.Log(() => t?.Exception?.InnerException?.StackTrace);
            }, TaskContinuationOptions.NotOnRanToCompletion);
        }
    }
}
