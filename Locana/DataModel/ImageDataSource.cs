using Windows.UI.Xaml.Media.Imaging;

namespace Locana.DataModel
{
    public class ImageDataSource : ObservableBase
    {
        private BitmapImage _Image = null;
        public BitmapImage Image
        {
            set
            {
                _Image = value;
                NotifyChanged(nameof(Image));
            }
            get
            {
                return _Image;
            }
        }

        public LiveviewScreenViewData ScreenViewData { get; set; }
    }
}
