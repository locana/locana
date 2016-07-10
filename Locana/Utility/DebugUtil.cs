using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;

namespace Locana.Utility
{
    public class DebugUtil
    {
        private const string LOG_ROOT = "log_store";

        private const string LOG_FILE_TEMPLATE = "locana-debug-{0}.log";

        /// <summary>
        /// Show given string on the console, but never save to the storage.
        /// </summary>
        /// <param name="s">Log mesage</param>
        public static void LogSensitive(Func<string> s)
        {
#if DEBUG
            Debug.WriteLine(s);
#endif
        }

        /// <summary>
        /// Show given string with sensitive text on the console and save to the storage without sensitive text.
        /// </summary>
        /// <param name="format">Function to return format</param>
        /// <param name="sensitiveText"></param>
        public static void LogSensitive(Func<string> format, params object[] sensitiveText)
        {
#if DEBUG
            Debug.WriteLine(string.Format(format.Invoke(), sensitiveText));
#endif
            WriteLogAsync(format);
        }

        /// <summary>
        /// Show given string on the console and save to the storage for debugging if it is enabled.
        /// </summary>
        /// <param name="s">Log mesage</param>
        public static void Log(Func<string> s)
        {
#if DEBUG
            Debug.WriteLine(s.Invoke());
#endif
            WriteLogAsync(s);
        }

        public static async Task<bool> CheckRemainingFile()
        {
            foreach (var file in await LogFiles())
            {
                Debug.WriteLine(file.DisplayName + ": " + (await file.GetBasicPropertiesAsync()).Size);
            }
            return false;
        }

        public static async Task GrubFile()
        {
            if (file != null)
            {
                throw new InvalidOperationException("Already grubed");
            }

            var root = ApplicationData.Current.TemporaryFolder;
            var folder = await root.CreateFolderAsync(LOG_ROOT, CreationCollisionOption.OpenIfExists);
            var time = DateTimeOffset.Now.ToLocalTime().ToString("yyyyMMdd-HHmmss");
            var filename = string.Format(LOG_FILE_TEMPLATE, time);
            file = await folder.CreateFileAsync(filename, CreationCollisionOption.OpenIfExists);

            Debug.WriteLine("Log file created");
        }

        public static bool ReleaseFile()
        {
            if (file != null)
            {
                file = null;
                return true;
            }
            else
            {
                return false;
            }
        }

        private static StorageFile file;

        private static SemaphoreSlim mut = new SemaphoreSlim(1);

        private static async void WriteLogAsync(Func<string> s)
        {
            var scopedFile = file;
            if (scopedFile == null)
            {
                return;
            }

            var time = DateTimeOffset.Now.ToLocalTime();

            await mut.WaitAsync();
            try
            {
                await FileIO.AppendTextAsync(scopedFile, string.Format("[{0}] {1}\n", time.ToString("HH:mm:ss.fff"), s.Invoke()), UnicodeEncoding.Utf8);
            }
            finally
            {
                mut.Release();
            }
        }

        public static async Task<IReadOnlyList<StorageFile>> LogFiles()
        {
            var root = ApplicationData.Current.TemporaryFolder;
            Debug.WriteLine(root.Path);
            var folder = await root.CreateFolderAsync(LOG_ROOT, CreationCollisionOption.OpenIfExists);
            return await folder.GetFilesAsync();
        }

        private const string ARCHIVE_NAME = "latest-logs.zip";

        public static async Task ZipLogFileDir()
        {
            var current = await ApplicationData.Current.TemporaryFolder.TryGetFileAsync(ARCHIVE_NAME);
            if (current != null)
            {
                await current.DeleteAsync();
            }
            var root = ApplicationData.Current.TemporaryFolder.Path;
            ZipFile.CreateFromDirectory(root + "\\" + LOG_ROOT, root + "\\" + ARCHIVE_NAME);

            Debug.WriteLine(string.Format("Created zip archive: {0}/{1}", ApplicationData.Current.TemporaryFolder.Path, ARCHIVE_NAME));
        }

        public static async Task<StorageFile> LatestLogFile()
        {
            var dir = ApplicationData.Current.TemporaryFolder;
            return await dir.TryGetFileAsync(ARCHIVE_NAME);
        }
    }
}
