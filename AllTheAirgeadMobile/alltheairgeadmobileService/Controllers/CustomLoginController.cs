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
    // Allow anybody to connect to the mobile service in order to login
    [AuthorizeLevel(AuthorizationLevel.Anonymous)]
    public class CustomLoginController : ApiController
    {
        public ApiServices Services { get; set; }
        public IServiceTokenHandler handler { get; set; }
        
        /// <summary>
        /// POST api/CustomLogin HTTP request handler
        /// </summary>
        public HttpResponseMessage Post(LoginRequest Request)
        {
            // Use local database context for testing local to service
            //alltheairgeadmobileContext context = new alltheairgeadmobileContext();
            // Setup the connection to the remote database
            alltheairgeadContext context = new alltheairgeadContext(Services.Settings["ExistingDbConnectionString"]);
            try
            {
                // Look for an account with the provided details
                UserProfile account = context.UserProfiles.Where(a => a.Email == Request.Email).SingleOrDefault();
                if (account != null)
                {
                    // Store membership data from database in a webpages_Membership
                    webpages_Membership membership = context.Memberships.Where(a => a.UserId == account.UserId).SingleOrDefault();
                    // Attempt to verify the supplied password
                    if(Crypto.VerifyHashedPassword(membership.Password, Request.Password))
                    {
                        // Generate authentication token
                        ClaimsIdentity claimsIdentity = new ClaimsIdentity();
                        claimsIdentity.AddClaim(new Claim(ClaimTypes.NameIdentifier, Request.Email));
                        LoginResult loginResult = new CustomLoginProvider(handler).CreateLoginResult(claimsIdentity, Services.Settings.MasterKey);
                        return this.Request.CreateResponse(HttpStatusCode.OK, loginResult);
                    }
                }
                // If an account could not be found with the username, return an unautherized response
                return this.Request.CreateResponse(HttpStatusCode.Unauthorized, "Invalid username or password");
            }
            catch
            {
                return this.Request.CreateResponse(HttpStatusCode.Unauthorized, "Invalid username or password");
            }
        }
    }
}
