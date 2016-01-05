
namespace Locana.Playback
{
    public class DummyContentsFlag
    {
        public static bool Enabled
        {
            get
            {
#if DEBUG
                return true;
#else
                return false;
#endif
            }
        }
    }
}
