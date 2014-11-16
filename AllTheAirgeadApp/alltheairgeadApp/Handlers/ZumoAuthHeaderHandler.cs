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
            if (string.IsNullOrWhiteSpace(App.alltheairgeadClient.CurrentUser.MobileServiceAuthenticationToken))
            {
                throw new InvalidOperationException("User is not currently logged in");
            }

            request.Headers.Add("X-ZUMO-AUTH", App.alltheairgeadClient.CurrentUser.MobileServiceAuthenticationToken);

            return base.SendAsync(request, cancellationToken);
        }
    }
}
