using System;

namespace Kazyx.Uwpmm.UPnP
{
    public class SoapException : Exception
    {
        public int StatusCode { private set; get; }

        public string Description { private set; get; }

        public SoapException(int code, string description)
            : base()
        {
            StatusCode = code;
            Description = description;
        }
    }
}
