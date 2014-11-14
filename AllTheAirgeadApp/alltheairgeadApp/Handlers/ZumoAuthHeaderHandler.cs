using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using alltheairgeadApp.Services;

namespace alltheairgeadApp.Handlers
{
    public class ZumoAuthHeaderHandler : DelegatingHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(CustomAccountService.AuthenticationToken))
            {
                throw new InvalidOperationException("User is not currently logged in");
            }

            request.Headers.Add("X-ZUMO-AUTH", CustomAccountService.AuthenticationToken);

            return base.SendAsync(request, cancellationToken);
        }
    }
}
