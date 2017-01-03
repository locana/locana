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
        public static ExposureModeComparer INSTANCE { get { return new ExposureModeComparer(); } }

        readonly Dictionary<string, int> Priority = new Dictionary<string, int>() {
            {ExposureMode.Program,1 },
            {ExposureMode.Aperture,2 },
            {ExposureMode.SS,3 },
            {ExposureMode.Manual,4 },
            {ExposureMode.Intelligent,5 },
            {ExposureMode.Superior,6 },
        };

        public int Compare(string x, string y)
        {
            if (Priority.ContainsKey(x) && Priority.ContainsKey(y))
            {
                return Priority[x] - Priority[y];
            }
            else if (Priority.ContainsKey(x))
            {
                return -1;
            }
            else if (Priority.ContainsKey(y))
            {
                return 1;
            }
            return 0;
        }
    }
}
