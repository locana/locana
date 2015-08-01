using System;
using System.Threading;

namespace Kazyx.Uwpmm.Utility
{
    public static class CancellationTokenSourceExtensions
    {
        public static void ThrowIfCancelled(this CancellationTokenSource cancel)
        {
            if (cancel != null && cancel.IsCancellationRequested)
            {
                throw new OperationCanceledException("Operation cancelled by caller");
            }
        }

        public static void CancelIfNotNull(this CancellationTokenSource cancel)
        {
            if (cancel != null)
            {
                cancel.Cancel();
            }
        }
    }
}
