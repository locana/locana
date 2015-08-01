using System.Text;
using System.Xml.Linq;
using Windows.Web.Http;
using Windows.Web.Http.Headers;

namespace Kazyx.Uwpmm.UPnP
{
    public abstract class Request
    {
        public abstract string Urn { get; }
        public abstract string ActionName { get; }

        public string SoapHeader { get { return "\"" + Urn + "#" + ActionName + "\""; } }

        public string BuildMessage()
        {
            var builder = new StringBuilder();

            builder.Append("<?xml version=\"1.0\" encoding=\"utf-8\"?>").Append("\r\n")
                .Append("<s:Envelope s:encodingStyle=\"http://schemas.xmlsoap.org/soap/encoding/\" xmlns:s=\"http://schemas.xmlsoap.org/soap/envelope/\">").Append("\r\n")
                .Append("<s:Body>").Append("\r\n")
                .Append("<u:").Append(ActionName).Append(" xmlns:u=\"").Append(Urn).Append("\">").Append("\r\n");

            AppendSpecificMessage(builder);

            builder.Append("</u:").Append(ActionName).Append(">").Append("\r\n")
                .Append("</s:Body>").Append("\r\n")
                .Append("</s:Envelope>").Append("\r\n");

            return builder.ToString();
        }

        public void UpdateSoapActionHeader(HttpRequestHeaderCollection headers)
        {
            headers.Clear();
            headers.Add("SOAPAction", SoapHeader);
        }

        protected abstract void AppendSpecificMessage(StringBuilder builder);

        public abstract Response ParseResponse(XDocument xml);
    }

    public abstract class Response
    {
        protected const string NS_S = "{http://schemas.xmlsoap.org/soap/envelope/}";

        protected const string NS_CTL = "{urn:schemas-upnp-org:control-1-0}";

        public static void TryThrowErrorCode(string responseBody)
        {
            int code = -1;
            string message = "";
            try
            {
                if (responseBody == null)
                {
                    return;
                }
                var root = XDocument.Parse(responseBody);

                var body = root.Root.Element(NS_S + "Body");
                var fault = body.Element(NS_S + "Fault");
                var detail = fault.Element("detail");
                var error = detail.Element(NS_CTL + "UPnPError");
                code = int.Parse(error.Element(NS_CTL + "errorCode").Value);
                message = error.Element(NS_CTL + "errorDescription").Value;
            }
            catch { return; }

            throw new SoapException(code, message);
        }

        public static XElement GetBodyOrThrowError(XDocument root)
        {
            var body = root.Root.Element(NS_S + "Body");
            var fault = body.Element(NS_S + "Fault");
            if (fault == null)
            {
                return body;
            }
            var detail = fault.Element("detail");
            var error = detail.Element(NS_CTL + "UPnPError");
            var code = int.Parse(error.Element(NS_CTL + "errorCode").Value);
            var message = error.Element(NS_CTL + "errorDescription").Value;
            throw new SoapException(code, message);
        }
    }
}
