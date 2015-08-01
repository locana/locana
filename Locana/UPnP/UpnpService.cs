using Kazyx.Uwpmm.Utility;
using System;
using System.Net;
using System.Threading.Tasks;
using System.Xml.Linq;
using Windows.Web.Http;

namespace Kazyx.Uwpmm.UPnP
{
    public class UpnpService
    {
        public string RootAddress { get; set; }

        public string ServiceType { get; set; }
        public string ServiceId { get; set; }
        public string ScpdUrl { get; set; }
        public string ControlUrl { get; set; }
        public string EventSubUrl { get; set; }

        private HttpClient HttpClient = new HttpClient();

        public async Task FetchServiceDescription()
        {
            var uri = new Uri(RootAddress + ScpdUrl);
            var response = await HttpClient.GetAsync(uri);
            if (response.IsSuccessStatusCode)
            {
                var res = await response.Content.ReadAsStringAsync();
                // DebugUtil.Log(res);
            }
            else
            {
                DebugUtil.Log("Http Status Error while getting SCPD: " + response.StatusCode);
            }
        }

        public async Task<Response> Control(Request request)
        {
            var body = request.BuildMessage();
            // DebugUtil.Log(body);

            var content = new HttpStringContent(body);
            content.Headers["Content-Type"] = "text/xml";
            request.UpdateSoapActionHeader(HttpClient.DefaultRequestHeaders);

            var uri = new Uri(RootAddress + ControlUrl);

            // DebugUtil.Log("Access to " + uri.ToString());
            var response = await HttpClient.PostAsync(uri, content);

            if (response.IsSuccessStatusCode)
            {
                var res = await response.Content.ReadAsStringAsync();
                res = WebUtility.HtmlDecode(res);
                // DebugUtil.Log(res);
                return request.ParseResponse(XDocument.Parse(res));
            }
            else
            {
                DebugUtil.Log("Http Status Error in SOAP request: " + response.StatusCode);
                var res = await response.Content.ReadAsStringAsync();
                DebugUtil.Log(res);
                Response.TryThrowErrorCode(res);
                throw new SoapException((int)response.StatusCode, "Http status code");
            }
        }
    }
}
