using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Security.Claims;
using Microsoft.WindowsAzure.Mobile.Service;
using Microsoft.WindowsAzure.Mobile.Service.Security;
using alltheairgeadmobileService.DataObjects;
using alltheairgeadmobileService.Models;

namespace alltheairgeadmobileService.Controllers
{
    [AuthorizeLevel(AuthorizationLevel.Anonymous)]
    public class CustomLoginController : ApiController
    {
        public ApiServices Services { get; set; }
        public IServiceTokenHandler handler { get; set; }

        // POST api/CustomLogin
        public HttpResponseMessage Post(LoginRequest Request)
        {
            alltheairgeadmobileContext context = new alltheairgeadmobileContext();
            try
            {
                Account account = context.Accounts.Where(a => a.Username == Request.Username).SingleOrDefault();
                if (account != null)
                {
                    byte[] incoming = CustomLoginProviderUtils.hash(Request.Password);

                    if (CustomLoginProviderUtils.slowEquals(incoming, account.HashedPassword))
                    {
                        ClaimsIdentity claimsIdentity = new ClaimsIdentity();
                        claimsIdentity.AddClaim(new Claim(ClaimTypes.NameIdentifier, Request.Username));
                        LoginResult loginResult = new CustomLoginProvider(handler).CreateLoginResult(claimsIdentity, Services.Settings.MasterKey);
                        return this.Request.CreateResponse(HttpStatusCode.OK, loginResult);
                    }
                }
                return this.Request.CreateResponse(HttpStatusCode.Unauthorized, "Invalid username or password");
            }
            catch
            {
                return this.Request.CreateResponse(HttpStatusCode.Unauthorized, "Invalid username or password");
            }
        }
        public HttpResponseMessage Post(string username, string password)
        {
            LoginRequest Request = new LoginRequest();
            Request.Username = username;
            Request.Password = password;

            var result = Post(Request);
            return result;
        }
    }
}
