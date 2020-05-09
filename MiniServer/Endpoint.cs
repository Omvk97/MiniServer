using System;
namespace MiniServer
{
    public class EndPoint
    {
        public HttpMethod HttpMethod { get; set; }
        public string UrlSuffix { get; set; }
        public Delegate Callback { get; set; }
    }
}
