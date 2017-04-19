using Locana.Utility;

namespace Locana.Resources
{
    public enum Localization
    {
        Default = 0,
        English = 1,
        Japanese = 2,
        SimplifiedChinese = 3,
    }

    public static class LocalizationExtensions
    {
        public static string AsLang(this Localization lang)
        {
            switch (lang)
            {
                case Localization.English:
                    return "en";
                case Localization.Japanese:
                    return "jp";
                case Localization.SimplifiedChinese:
                    return "zh-cn";
            }
            return "";
        }

        public static Localization FromLang(string lang)
        {
            switch (lang.ToLower())
            {
                case "":
                    return Localization.Default;
                case "en":
                    return Localization.English;
                case "jp":
                    return Localization.Japanese;
                case "zh-cn":
                    return Localization.SimplifiedChinese;
                default:
                    DebugUtil.Log(() => "Unknown language code " + lang);
                    return Localization.Default;
            }
        }
    }
}
