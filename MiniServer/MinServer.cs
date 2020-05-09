using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace MiniServer
{
    public class MinServer
    {
        public enum HttpMethod
        {
            GET,
            POST,
            PUT,
            DELETE,
            PATCH
        }

        public class EndPoint
        {
            public HttpMethod HttpMethod { get; set; }
            public string UrlSuffix { get; set; }
            public Delegate Callback { get; set; }
        }

        private HttpListener _listener;
        private readonly string _serverUrl;
        private bool _runServer = true;
        private List<EndPoint> _listenerEndpoints = new List<EndPoint>();
        private List<Delegate> _middlewares = new List<Delegate>();
        public MinServer()
        {
            _serverUrl = "http://localhost:5000/";
        }
        public MinServer(string url)
        {
            if (!url.EndsWith("/"))
            { // url must end with '/'
                url += "/";
            }

            _serverUrl = url;
        }

        public void Get(string urlsuffix, Action<HttpListenerRequest, HttpListenerResponse> callback)
        {
            AddEndpoint(urlsuffix, callback, HttpMethod.GET);
        }

        public void Post(string urlsuffix, Action<HttpListenerRequest, HttpListenerResponse> callback)
        {
            AddEndpoint(urlsuffix, callback, HttpMethod.POST);
        }

        public void Put(string urlsuffix, Action<HttpListenerRequest, HttpListenerResponse> callback)
        {
            AddEndpoint(urlsuffix, callback, HttpMethod.PUT);
        }
        public void Delete(string urlsuffix, Action<HttpListenerRequest, HttpListenerResponse> callback)
        {
            AddEndpoint(urlsuffix, callback, HttpMethod.DELETE);
        }
        public void Patch(string urlsuffix, Action<HttpListenerRequest, HttpListenerResponse> callback)
        {
            AddEndpoint(urlsuffix, callback, HttpMethod.PATCH);
        }
        // Middleware which is called for each request in the order they are added.
        public void Use(Action<HttpListenerRequest, HttpListenerResponse> middleware)
        {
            _middlewares.Add(middleware);
        }

        private void AddEndpoint(string urlsuffix, Action<HttpListenerRequest, HttpListenerResponse> callback, HttpMethod requestHttpMethod)
        {
            if (urlsuffix.StartsWith("/"))
                urlsuffix = urlsuffix.Substring(1);
            _listenerEndpoints.Add(new EndPoint
            {
                HttpMethod = requestHttpMethod,
                Callback = callback,
                UrlSuffix = urlsuffix
            });
        }

        public async Task HandleIncomingConnections()
        {
            while (_runServer)
            {
                // Will wait here until we hear from a connection
                HttpListenerContext ctx = await _listener.GetContextAsync();

                // Peel out the requests and response objects
                HttpListenerRequest req = ctx.Request;
                HttpListenerResponse res = ctx.Response;

                foreach (Delegate middleware in _middlewares)
                {
                    middleware.DynamicInvoke(req, res);
                }

                // Print out some info about the request
                var requestUrl = req.Url.ToString();
                var httpMethod = req.HttpMethod;
                bool validRouteFound = false;
                foreach (EndPoint endPoint in _listenerEndpoints)
                {
                    var fullUrl = _serverUrl + endPoint.UrlSuffix;
                    if (fullUrl.Equals(requestUrl) && httpMethod.Equals(endPoint.HttpMethod.ToString()))
                    {
                        validRouteFound = true;
                        endPoint.Callback.DynamicInvoke(req, res);
                        break;
                    }
                }
                if (!validRouteFound)
                {
                    res.StatusCode = 404;
                    res.Close();
                }
            }
        }

        public void Stop()
        {
            _runServer = false;
        }
        public async Task Listen()
        {
            // Create a Http server and start listening for incoming connections
            _listener = new HttpListener();
            _listener.Prefixes.Add(_serverUrl);
            _listener.Start();
            Console.WriteLine("Listening for connections on {0}", _serverUrl);

            // Handle requests
            await HandleIncomingConnections();

            // Close the listener when _runServer has been set to false
            _listener.Close();
        }
    }
}