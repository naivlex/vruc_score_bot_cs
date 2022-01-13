using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace vruc_score_bot_cs
{
    public class LoggingHandler : DelegatingHandler
    {
        public static CookieContainer cookies = new CookieContainer();

        public LoggingHandler(HttpClientHandler innerHandler)
            : base(innerHandler)
        {
            innerHandler.CookieContainer = cookies;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            /*
            Console.WriteLine("Request:");
            Console.WriteLine(request.ToString());
            if (request.Content != null)
            {
                Console.WriteLine(await request.Content.ReadAsStringAsync());
            }
            Console.WriteLine();
            */

            HttpResponseMessage response = await base.SendAsync(request, cancellationToken);

            /*
            Console.WriteLine("Response:");
            Console.WriteLine(response.ToString());
            if (response.Content != null)
            {
                Console.WriteLine(await response.Content.ReadAsStringAsync());
            }
            Console.WriteLine();
            */

            return response;
        }
    }
}
