using System;
using System.Collections.Generic;
using System.Text;

namespace Kazyx.Uwpmm.DataModel
{
    public class ShootingParamViewData
    {
        public ShootingParamViewData() { }
        public LiveviewScreenViewData Liveview { get; set; }
        public CameraStatus Status { get; set; }
    }

    public class OptionalElementsViewData : ObservableBase
    {
        public OptionalElementsViewData() { }
        public ApplicationSettings AppSetting { get; set; }
        public LiveviewScreenViewData Liveview { get; set; }
    }
}
