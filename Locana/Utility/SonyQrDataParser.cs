using System;
using System.Collections.Generic;
using System.Linq;

namespace Locana.Utility
{
    public class SonyQrDataParser
    {
        const string PASSWORD_KEY = "P";
        const string SSID_PREFIX_KEY = "S";
        const string MODEL_NAME_KEY = "C";

        public static SonyQrData ParseData(string input)
        {
            var data = new SonyQrData()
            {
                SSID = "",
                Password = "",
            };

            // trim first unknown section. e.g. "W01:"
            var first_colon_index = input.IndexOf(':');
            if (first_colon_index > 2)
            {
                input = input.Remove(0, first_colon_index + 1);
            }

            var elements = new Dictionary<string, string>();
            foreach (var s in input.Split(';'))
            {
                var substrings = s.Split(':');
                if (substrings.Count() > 1)
                {
                    elements.Add(substrings[0], substrings[1]);
                }
            }

            if (elements.ContainsKey(PASSWORD_KEY))
            {
                data.Password = elements[PASSWORD_KEY];
            }
            else
            {
                throw new FormatException("Couldn't find password section");
            }

            if (elements.ContainsKey(SSID_PREFIX_KEY) && elements.ContainsKey(MODEL_NAME_KEY))
            {
                data.SSID = "DIRECT-" + elements[SSID_PREFIX_KEY] + ":" + elements[MODEL_NAME_KEY];
            }
            else
            {
                throw new FormatException("Couldn't find ssid or model name section");
            }

            return data;
        }
    }

    public class SonyQrData
    {
        public string SSID { get; set; }
        public string Password { get; set; }
    }
}
