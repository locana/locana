using System;
using System.Threading;

namespace Locana.Utility
{
    public static class CancellationTokenSourceExtensions
    {
        public static void ThrowIfCancelled(this CancellationTokenSource cancel)
        {
            if (cancel?.IsCancellationRequested ?? false)
            {
                throw new OperationCanceledException("Operation cancelled by caller");
            }
        }
    }
}
