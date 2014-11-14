using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Text.RegularExpressions;
using Microsoft.WindowsAzure.Mobile.Service;
using Microsoft.WindowsAzure.Mobile.Service.Security;
using alltheairgeadmobileService.Models;
using alltheairgeadmobileService.DataObjects;
using EmailValidation;

namespace alltheairgeadmobileService.Controllers
{
    [AuthorizeLevel(AuthorizationLevel.Anonymous)]
    public class CustomRegistrationController : ApiController
    {
        public ApiServices Services { get; set; }

        // POST api/CustomRegistration
        public HttpResponseMessage Post(RegistrationRequest Request)
        {
            if (!EmailValidator.Validate(Request.Username, true))
            {
                return this.Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid username");
            }
            else if (Request.Password.Length < 8)
            {
                return this.Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid password (at least 8 chars required)");
            }

            alltheairgeadmobileContext context = new alltheairgeadmobileContext();
            Account account = context.Accounts.Where(a => a.Username == Request.Username).SingleOrDefault();
            if (account != null)
            {
                return this.Request.CreateResponse(HttpStatusCode.BadRequest, "Username already exists");
            }
            else
            {
                Account newAccount = new Account
                {
                    Id = Guid.NewGuid().ToString(),
                    Username = Request.Username,
                    HashedPassword = CustomLoginProviderUtils.hash(Request.Password)
                };
                context.Accounts.Add(newAccount);
                context.SaveChanges();
                return this.Request.CreateResponse(HttpStatusCode.Created);
            }
        }

        public HttpResponseMessage Post(string username, string password)
        {
            RegistrationRequest Request = new RegistrationRequest();
            Request.Username = username;
            Request.Password = password;

            var result = Post(Request);
            return result;
        }
    }
}
