
namespace Kazyx.Uwpmm.Playback
{
    public class DummyContentsFlag
    {
        public static bool Enabled
        {
            get
            {
#if DEBUG
                return false;
#else
                return false;
#endif
            }
        }
    }
}
