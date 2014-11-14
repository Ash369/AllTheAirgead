using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Helpers;
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
            // Use local database context for testing local to service
            //alltheairgeadmobileContext context = new alltheairgeadmobileContext();
            alltheairgeadContext context = new alltheairgeadContext(Services.Settings["ExistingDbConnectionString"]);
            try
            {
                UserProfile account = context.UserProfiles.Where(a => a.Email == Request.Email).SingleOrDefault();
                var temp = context.UserProfiles;
                if (account != null)
                {
                    webpages_Membership membership = context.Memberships.Where(a => a.UserId == account.UserId).SingleOrDefault();
                    if(Crypto.VerifyHashedPassword(membership.Password, Request.Password))
                    {
                        ClaimsIdentity claimsIdentity = new ClaimsIdentity();
                        claimsIdentity.AddClaim(new Claim(ClaimTypes.NameIdentifier, Request.Email));
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
        public HttpResponseMessage Post(string Email, string Password)
        {
            LoginRequest Request = new LoginRequest();
            Request.Email = Email;
            Request.Password = Password;

            var result = Post(Request);
            return result;
        }
    }
}
