using Locana.DataModel;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.UI.Xaml;
using Naotaco.ImageProcessor.Histogram;
using System;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Graphics.DirectX;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Xaml.Media.Imaging;

namespace Locana.Utility
{
    public class LiveviewUtil
    {
        public static async Task SetAsBitmap(byte[] data, ImageDataSource target, HistogramCreator Histogram, CoreDispatcher Dispatcher = null)
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

                if (Histogram == null) { return; }

                if (ApplicationSettings.GetInstance().IsHistogramDisplayed && !Histogram.IsRunning)
                {
                    await Dispatcher.RunAsync(CoreDispatcherPriority.Low, () =>
                    {
                        Histogram.IsRunning = true;
                        stream.Seek(0);
                        var image = new BitmapImage();
                        image.SetSource(stream);
                        var writableImage = new WriteableBitmap(image.PixelWidth, image.PixelHeight);
                        stream.Seek(0);
                        writableImage.SetSource(stream);
                        Histogram.CreateHistogram(writableImage);
                    });
                }
                else if (Histogram.IsRunning)
                {
                    DebugUtil.Log("Histogram creating. skip.");
                }
            }
        }

        const double DEFAULT_DPI = 96;

        public static CanvasBitmap NewCanvasBitmap(WriteableBitmap writeable, CanvasControl TargetCanvas, double LiveviewMagnification)
        {
            var dpi = DEFAULT_DPI / LiveviewMagnification;
            return CanvasBitmap.CreateFromBytes(TargetCanvas, writeable.PixelBuffer.ToArray(), writeable.PixelWidth, writeable.PixelHeight, DirectXPixelFormat.B8G8R8A8UIntNormalized, (float)dpi);
        }

        public static async Task<WriteableBitmap> AsWriteableBitmap(byte[] BitmapData, BitmapImage LiveviewTempBitmap, CoreDispatcher Dispatcher)
        {
            WriteableBitmap wb = null;
            using (var temp = new InMemoryRandomAccessStream())
            {
                await temp.WriteAsync(BitmapData.AsBuffer());
                temp.Seek(0);

                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    LiveviewTempBitmap.SetSource(temp);

                    wb = new WriteableBitmap(LiveviewTempBitmap.PixelWidth, LiveviewTempBitmap.PixelHeight);
                    temp.Seek(0);

                    wb.SetSource(temp);
                });
            }
            return wb;
        }
    }
}
