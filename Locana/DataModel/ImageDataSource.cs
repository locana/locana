using Windows.UI.Core;
using Windows.UI.Xaml.Media.Imaging;

namespace Kazyx.Uwpmm.DataModel
{
    public class ImageDataSource : ObservableBase
    {
        private BitmapImage _Image = null;
        public BitmapImage Image
        {
            set
            {
                _Image = value;
                NotifyChanged("Image");
            }
            get
            {
                return _Image;
            }
        }

        public LiveviewScreenViewData ScreenViewData { get; set; }
    }
}
