using System;
using System.Xml.Linq;

namespace Kazyx.Uwpmm.UPnP
{
    public class OptionalValueHelper
    {
        public static bool ParseBool(XAttribute attr, bool defaultVal = false)
        {
            if (attr == null)
            {
                return defaultVal;
            }

            return BoolConversionHelper.Parse(attr);
        }

        public static int ParseInt(XAttribute attr, int defaultVal = -1)
        {
            if (attr == null)
            {
                return defaultVal;
            }

            var result = 0;
            if (int.TryParse(attr.Value, out result))
            {
                return result;
            }
            else
            {
                return defaultVal;
            }
        }

        public static string ParseString(XAttribute attr, string defaultVal = null)
        {
            if (attr == null)
            {
                return defaultVal;
            }

            return attr.Value;
        }

        public static string ParseString(XElement element, string defaultVal = null)
        {
            if (element == null)
            {
                return defaultVal;
            }

            return element.Value;
        }
    }

    public class BoolConversionHelper
    {
        public static bool Parse(XAttribute attr)
        {
            var result = false;
            if (bool.TryParse(attr.Value, out result))
            {
                return result;
            }

            var num = 0;
            int.TryParse(attr.Value, out num);

            // If the value exists and it is not bool or int, fallback to false.
            return Convert.ToBoolean(num);
        }
    }
}
