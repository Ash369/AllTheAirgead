using System;
using System.Security.Claims;
using Owin;
using Newtonsoft.Json.Linq;
using Microsoft.WindowsAzure.Mobile.Service;
using Microsoft.WindowsAzure.Mobile.Service.Security;
using alltheairgeadmobileService.DataObjects;

namespace alltheairgeadmobileService.Models
{
    /// <summary>
    /// Custom login provider. Allows users to login with custom credentials instead of relying on external providers
    /// </summary>
    public class CustomLoginProvider : LoginProvider
    {
        public const string ProviderName = "custom";

        public override string Name
        {
            get { return ProviderName; }
        }

        public CustomLoginProvider(IServiceTokenHandler tokenHandler)
            : base(tokenHandler)
        {
            // Setup tokens to have a 30 day lifetime
            this.TokenLifetime = new TimeSpan(30, 0, 0, 0);
        }

        public override void ConfigureMiddleware(IAppBuilder appBuilder, ServiceSettingsDictionary settings)
        {
            // Not Applicable - used for federated identity flows
            return;
        }

        /// <summary>
        /// Parse JSON object data
        /// </summary>
        public override ProviderCredentials ParseCredentials(JObject serialized)
        {
            if (serialized == null)
            {
                throw new ArgumentNullException("serialized");
            }
            return serialized.ToObject<CustomLoginProviderCredentials>();
        }

        /// <summary>
        /// Create user credentials
        /// </summary>
        public override ProviderCredentials CreateCredentials(ClaimsIdentity claimsIdentity)
        {
            if (claimsIdentity == null)
            {
                throw new ArgumentNullException("claimsIdentity");
            }

            // Build custom credential
            string username = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            CustomLoginProviderCredentials credentials = new CustomLoginProviderCredentials
            {
                UserId = this.TokenHandler.CreateUserId(this.Name, username)
            };

            return credentials;
        }
    }
}