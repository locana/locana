using Locana.Utility;

namespace Locana.Resources
{
    public enum Localization
    {
        Default = 0,
        English = 1,
        Japanese = 2,
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
            }
            return "";
        }

        public static Localization FromLang(string lang)
        {
            switch (lang)
            {
                case "":
                    return Localization.Default;
                case "en":
                    return Localization.English;
                case "jp":
                    return Localization.Japanese;
                default:
                    DebugUtil.Log(() => "Unknown language code " + lang);
                    return Localization.Default;
            }
        }
    }
}
