using Kazyx.RemoteApi.Camera;
using System;
using System.Collections.Generic;
using System.Text;

namespace Kazyx.Uwpmm.CameraControl
{
    public class CameraStatusUtility
    {
        internal static bool IsContinuousShootingMode(TargetDevice target)
        {
            return target != null && target.Status != null && target.Status.ShootMode != null &&
                target.Status.ShootMode.Current == ShootModeParam.Still &&
                target.Status.ContShootingMode != null &&
                (target.Status.ContShootingMode.Current == ContinuousShootMode.Cont ||
                target.Status.ContShootingMode.Current == ContinuousShootMode.SpeedPriority);
        }
    }
}
