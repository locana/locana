using Kazyx.RemoteApi.Camera;
using Locana.DataModel;

namespace Locana.Utility
{
    public static class CameraStatusExtensions
    {
        public static bool IsRecording(this CameraStatus status)
        {
            switch (status.Status ?? "")
            {
                case EventParam.MvRecording:
                case EventParam.AuRecording:
                case EventParam.ItvRecording:
                case EventParam.LoopRecording:
                    return true;
                default:
                    return false;
            }
        }
    }
}
