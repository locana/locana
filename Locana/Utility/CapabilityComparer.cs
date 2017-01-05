using Kazyx.RemoteApi;
using Kazyx.RemoteApi.Camera;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Locana.Utility
{
    public class ExposureModeComparer : IComparer<string>
    {
        private ExposureModeComparer() { }
        private static readonly ExposureModeComparer instance = new ExposureModeComparer();
        public static ExposureModeComparer INSTANCE { get { return instance; } }

        /// <summary>
        /// Defines order of sorted modes.
        /// P/A/S/M should be first for expert users.
        /// </summary>
        private readonly Dictionary<string, int> Priority = new Dictionary<string, int>() {
            {ExposureMode.Program,1 },
            {ExposureMode.Aperture,2 },
            {ExposureMode.SS,3 },
            {ExposureMode.Manual,4 },
            {ExposureMode.Intelligent,5 },
            {ExposureMode.Superior,6 },
        };

        public int Compare(string x, string y)
        {
            int xOrder = 0;
            int yOrder = 0;
            var xIsValid = Priority.TryGetValue(x, out xOrder);
            var yIsValid = Priority.TryGetValue(y, out yOrder);

            if (xIsValid && yIsValid)
            {
                return xOrder - yOrder;
            }
            else if (xIsValid)
            {
                return -1;
            }
            else if (yIsValid)
            {
                return 1;
            }
            return 0;
        }
    }
}
