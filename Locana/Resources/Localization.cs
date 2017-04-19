using Locana.Utility;

namespace Locana.Resources
{
    public enum Localization
    {
        Default = 0,
        English = 1,
        Japanese = 2,
        SimplifiedChinese = 3,
        Dutch = 4,
    }

    public static class LocalizationExtensions
    {
        public static string AsLang(this Localization lang)
        {
            switch (lang)
            {
                case Localization.English:
                    return "en-us";
                case Localization.Japanese:
                    return "ja-jp";
                case Localization.SimplifiedChinese:
                    return "zh-cn";
                case Localization.Dutch:
                    return "nl-nl";
            }
            return "";
        }

        public static Localization FromLang(string lang)
        {
            switch (lang.ToLower())
            {
                case "":
                    return Localization.Default;
                case "en-us":
                    return Localization.English;
                case "ja-jp":
                    return Localization.Japanese;
                case "zh-cn":
                    return Localization.SimplifiedChinese;
                case "nl-nl":
                    return Localization.Dutch;
                default:
                    DebugUtil.Log(() => "Unknown language code " + lang);
                    return Localization.Default;
            }
        }
    }
}
