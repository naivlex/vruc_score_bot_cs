using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace vruc_score_bot_cs
{
    public class CustomHttpHandler : DelegatingHandler
    {
        public static readonly bool logging = false;
        public static CookieContainer cookies = new CookieContainer();

        public CustomHttpHandler(HttpClientHandler innerHandler)
            : base(innerHandler)
        {
            innerHandler.CookieContainer = cookies;
        }

        protected async Task<HttpResponseMessage> SendAsyncWithLog(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (logging)
            {
                Console.WriteLine("Request:");
                Console.WriteLine(request.ToString());
                if (request.Content != null)
                {
                    Console.WriteLine(await request.Content.ReadAsStringAsync());
                }
                Console.WriteLine();
            }

            HttpResponseMessage response = await base.SendAsync(request, cancellationToken);

            if (logging)
            {
                Console.WriteLine("Response:");
                Console.WriteLine(response.ToString());
                if (response.Content != null)
                {
                    Console.WriteLine(await response.Content.ReadAsStringAsync());
                }
                Console.WriteLine();
            }
            return response;
        }

        protected async override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            HttpResponseMessage response = await SendAsyncWithLog(request, cancellationToken);

            for (int i = 0; i < 5; i++)
            {
                if (response.StatusCode == HttpStatusCode.Moved || response.StatusCode == HttpStatusCode.Found)
                {
                    var target_uri = response.Headers.Location;

                    if (logging)
                    {
                        Console.WriteLine($"Redirecting from {response.RequestMessage.RequestUri} to {target_uri}");
                    }

                    if (!target_uri.IsAbsoluteUri)
                        target_uri = new Uri(response.RequestMessage.RequestUri, target_uri);

                    if (!target_uri.Host.EndsWith("ruc.edu.cn"))
                        return response;

                    response.Dispose();

                    request.Method = HttpMethod.Get;
                    request.RequestUri = target_uri;

                    response = await SendAsyncWithLog(request, cancellationToken);
                }
                else
                    return response;
            }

            if (logging)
            {
                Console.WriteLine($"Too many redirects at {response.RequestMessage.RequestUri}");
            }

            return response;
        }
    }
}
