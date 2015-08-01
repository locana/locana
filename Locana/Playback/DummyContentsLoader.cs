#if DEBUG
using Kazyx.RemoteApi.AvContent;
using Kazyx.Uwpmm.DataModel;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
#endif

namespace Kazyx.Uwpmm.Playback
{
#if DEBUG
    public class DummyContentsLoader : ContentsLoader
    {
        public const int MAX_AUTO_LOAD_THUMBNAILS = 30;

        private readonly Random random;
        public DummyContentsLoader()
        {
            random = new Random();
            CurrentUuid = DummyContentsLoader.RandomUuid();
        }

        private readonly string CurrentUuid;

        public override async Task Load(ContentsSet contentsSet, CancellationTokenSource cancel)
        {
            await Task.Delay(500).ConfigureAwait(false);

            var loaded = 0;

            foreach (var date in RandomDateList(10))
            {
                await Task.Delay(200).ConfigureAwait(false);

                if (cancel != null && cancel.IsCancellationRequested)
                {
                    OnCancelled();
                    break;
                }

                var list = new List<Thumbnail>();
                var contents = RandomContentList(30);

                if (MAX_AUTO_LOAD_THUMBNAILS < loaded)
                {
                    var remaining = new RemainingContentsHolder(date, CurrentUuid, 0, contents.Count);
                    var tmplist = new List<Thumbnail>();
                    tmplist.Add(remaining);
                    OnPartLoaded(tmplist);
                    continue;
                }

                foreach (var content in contents)
                {
                    content.GroupName = date.Title;
                    list.Add(new Thumbnail(content, CurrentUuid));
                }

                OnPartLoaded(list);
                loaded += list.Count;
            }

            OnCompleted();
        }

        public override async Task LoadRemainingAsync(RemainingContentsHolder holder, ContentsSet contentsSet, CancellationTokenSource cancel)
        {
            var list = new List<Thumbnail>();
            var contents = RandomContentList(holder.RemainingCount, true);

            await Task.Delay(200).ConfigureAwait(false);

            foreach (var content in contents)
            {
                content.GroupName = holder.AlbumGroup.Title;
                Kazyx.Uwpmm.Utility.DebugUtil.Log("Add content for " + content.GroupName);
                list.Add(new Thumbnail(content, CurrentUuid));
            }

            OnPartLoaded(list);
        }

        private IList<DateInfo> RandomDateList(int count)
        {
            var list = new List<DateInfo>();
            for (int i = 0; i < count; i++)
            {
                list.Add(new DateInfo
                {
                    Title = YMDwithPadding(),
                    Uri = "dummyuri",
                });
                list.Sort((d1, d2) => { return string.CompareOrdinal(d2.Title, d1.Title); });
            }
            return list;
        }

        private IList<ContentInfo> RandomContentList(int count, bool fixedNum = false)
        {
            var list = new List<ContentInfo>();
            for (int i = 0; i < (fixedNum ? count : random.Next(1, count)); i++)
            {
                var type = ContentType();
                var thumb = ThumbnailUrl();
                list.Add(new ContentInfo
                {
                    MimeType = type,
                    ThumbnailUrl = thumb,
                    Name = FileName(),
                    CreatedTime = CreatedTime(),
                    LargeUrl = thumb,
                    OriginalUrl = type == ContentKind.StillImage ? "http://upload.wikimedia.org/wikipedia/commons/e/e5/Earth_.jpg" : "http://image.watch.impress.co.jp/avw/581732/MAH00028.mp4",
                    Protected = Protected(),
                });
            }
            return list;
        }

        private bool Protected()
        {
            return random.Next(0, 100) > 80; // protected is 20%.
        }

        public static string RandomUuid()
        {
            return "uuid:" + Guid.NewGuid().ToString();
        }

        private static readonly string[] dummyimages = new string[]{
            "http://cdn.gsmarena.com/vv/newsimg/13/12/htc-one-max-black/gsmarena_001.jpg",
            "http://www.notebookcheck.net/fileadmin/_processed_/csm_Nokia-Lumia-720-3__2__d354fb1d00.jpg",
            "http://www.technobuffalo.com/wp-content/uploads/2013/05/Verizon-Nokia-Lumia-928-VS-Nokia-Lumia-920-Front.jpg",
            "http://www.sony.jp/products/picture/ILCE-QX1_SELP1650.jpg",
        };

        private static string CreatedTime()
        {
            return DateTimeOffset.Now.ToString("yyyy-MM-ddThh:mm:ssZ");
        }

        private string FileName()
        {
            return "DUMMYFILE_" + random.Next(0, 10000);
        }

        private string ThumbnailUrl()
        {
            return dummyimages[random.Next(0, dummyimages.Length - 1)];
        }

        private string ContentType()
        {
            return random.NextDouble() > 0.1 ? MimeType.Jpeg : MimeType.Mp4;
        }

        private int Year()
        {
            return random.Next(2000, 2014);
        }

        private int Month()
        {
            return random.Next(1, 12);
        }

        private int Day()
        {
            return random.Next(1, 28);
        }

        private string YMDwithPadding()
        {
            var m = Month().ToString();
            if (m.Length == 1)
            {
                m = "0" + m;
            }
            var d = Day().ToString();
            if (d.Length == 1)
            {
                d = "0" + d;
            }
            return Year().ToString() + m + d;
        }
    }
#endif
}
