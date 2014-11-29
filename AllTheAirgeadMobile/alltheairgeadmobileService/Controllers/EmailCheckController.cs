using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Microsoft.WindowsAzure.Mobile.Service;
using Microsoft.WindowsAzure.Mobile.Service.Security;
using alltheairgeadmobileService.Models;
using alltheairgeadmobileService.DataObjects;

namespace alltheairgeadmobileService.Controllers
{
    // Allow anybody to check if the email exists
    [AuthorizeLevel(AuthorizationLevel.Anonymous)]
    public class EmailCheckController : ApiController
    {
        public ApiServices Services { get; set; }

        /// <summary>
        /// GET api/EmailCheck Checks that an email doesn't already exist
        /// </summary>
        /// <param name="Email"></param>
        /// <returns></returns>
        public HttpResponseMessage Get(string Email)
        {
            alltheairgeadContext Context = new alltheairgeadContext(Services.Settings["ExistingDbConnectionString"]);
            try
            {
                // Check for email and return a response based on whether it exists already or not
                if (Context.UserProfiles.Where(a => a.Email == Email).Any())
                    return this.Request.CreateResponse(HttpStatusCode.Found, "Email already exists");
                else
                    return this.Request.CreateResponse(HttpStatusCode.OK);
            }
            catch
            {
                // Return an error response if something goes wrong
                return this.Request.CreateBadRequestResponse();
            }
        }

    }
}
