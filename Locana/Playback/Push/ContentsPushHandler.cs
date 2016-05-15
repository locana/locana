using Locana.UPnP;
using Locana.UPnP.ContentDirectory;
using Locana.UPnP.XPushList;
using Locana.Utility;
using System;

namespace Locana.Playback.Push
{
    public class ContentsPushHandler
    {
        public static async void HandlePushedContents(UpnpDevice device)
        {
            try
            {
                var root = await device.Services[URN.XPushList].Control(new GetPushRootRequest()).ConfigureAwait(false) as PushRoot;
                await device.Services[URN.XPushList].Control(new TransferStartRequest()).ConfigureAwait(false);
                var contents = await device.Services[URN.ContentDirectory].Control(new BrowseRequest
                {
                    ObjectID = root.ObjectID,
                    BrowseFlag = BrowseFlag.BrowseDirectChildren,
                    Filter = "*",
                    StartingIndex = 0,
                    RequestedCount = 4
                }).ConfigureAwait(false) as RetrievedContents;

                var total = contents.Result.Items.Count;
                var completed = 0;

                foreach (var thumb in DlnaContentsLoader.Translate("Root", contents.Result.Items, device))
                {
                    await device.Services[URN.XPushList].Control(new TransferProgressRequest { NumTotal = total, NumTransferred = completed }).ConfigureAwait(false);
                    try
                    {
                        await DownloadHelper.EnqueueDownload(thumb);
                    }
                    catch (Exception e)
                    {
                        DebugUtil.Log(() => "Exception Pushed content download: " + e.StackTrace);
                    }
                }

                await device.Services[URN.XPushList].Control(new TransferEndRequest { ErrorCode = 0 }).ConfigureAwait(false); // TODO error code
            }
            catch (Exception e)
            {
                DebugUtil.Log(() => "Exception HandlePushedContents: " + e.StackTrace);
            }
        }
    }
}
