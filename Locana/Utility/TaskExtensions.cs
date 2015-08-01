using System.Threading.Tasks;

namespace Kazyx.Uwpmm.Utility
{
    public static class TaskExtensions
    {
        public static Task<T> IgnoreExceptions<T>(this Task<T> task, LogOptions option = LogOptions.StackTrace)
        {
            task.ContinueWith(t =>
            {
                switch (option)
                {
                    case LogOptions.IgnoredType:
                        DebugUtil.Log("Ignore " + t.Exception.GetType());
                        break;
                    case LogOptions.StackTrace:
                        DebugUtil.Log("Ignore " + t.Exception.GetType());
                        DebugUtil.Log(t.Exception.StackTrace);
                        break;
                }
            }, TaskContinuationOptions.OnlyOnFaulted);
            return task;
        }

        public static Task IgnoreExceptions(this Task task, LogOptions option = LogOptions.StackTrace)
        {
            task.ContinueWith(t =>
            {
                switch (option)
                {
                    case LogOptions.IgnoredType:
                        DebugUtil.Log("Ignore " + t.Exception.GetType());
                        break;
                    case LogOptions.StackTrace:
                        DebugUtil.Log("Ignore " + t.Exception.GetType());
                        DebugUtil.Log(t.Exception.StackTrace);
                        break;
                }
            }, TaskContinuationOptions.OnlyOnFaulted);
            return task;
        }

        public enum LogOptions
        {
            None,
            IgnoredType,
            StackTrace,
        }
    }
}
