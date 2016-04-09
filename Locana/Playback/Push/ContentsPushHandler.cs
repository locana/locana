using Locana.UPnP;
using Locana.UPnP.XPushList;

namespace Locana.Playback.Push
{
    public class ContentsPushHandler
    {
        public static async void HandlePushedContents(UpnpDevice device)
        {
            try
            {
                var res = await device.Services[URN.XPushList].Control(new GetListRequest
                {
                    // TODO params
                }).ConfigureAwait(false);

                // TODO enqueue contents to the MediaDownloader
            }
            catch
            {

            }
        }
    }
}
