using Locana.DataModel;
using System;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Xaml.Media.Imaging;

namespace Locana.Utility
{
    public class LiveviewUtil
    {
        public static async Task SetAsBitmap(byte[] data, ImageDataSource target, CoreDispatcher Dispatcher = null)
        {
            using (var stream = new InMemoryRandomAccessStream())
            {
                await stream.WriteAsync(data.AsBuffer());
                stream.Seek(0);
                if (Dispatcher == null)
                {
                    Dispatcher = SystemUtil.GetCurrentDispatcher();
                }
                if (Dispatcher == null)
                {
                    return;
                }

                await Dispatcher.RunAsync(CoreDispatcherPriority.High, () =>
                {
                    var image = new BitmapImage();
                    image.SetSource(stream);
                    target.Image = image;
                });
            }
        }

        public static async Task<WriteableBitmap> AsWriteableBitmap(byte[] jpegData, CoreDispatcher Dispatcher)
        {
            WriteableBitmap wb = null;
            using (var stream = new InMemoryRandomAccessStream())
            {
                await stream.WriteAsync(jpegData.AsBuffer());
                stream.Seek(0);

                var bitmap = new BitmapImage();
                bitmap.SetSource(stream);

                wb = new WriteableBitmap(bitmap.PixelWidth, bitmap.PixelHeight);
                stream.Seek(0);

                wb.SetSource(stream);
            }
            return wb;
        }
    }
}
