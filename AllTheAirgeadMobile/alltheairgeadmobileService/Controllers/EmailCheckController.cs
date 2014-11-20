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
    [AuthorizeLevel(AuthorizationLevel.Anonymous)]
    public class EmailCheckController : ApiController
    {
        public ApiServices Services { get; set; }

        // GET api/EmailCheck
        public HttpResponseMessage Get(string Email)
        {
            alltheairgeadContext Context = new alltheairgeadContext(Services.Settings["ExistingDbConnectionString"]);
            try
            {
                if (Context.UserProfiles.Where(a => a.Email == Email).Any())
                    return this.Request.CreateResponse(HttpStatusCode.Found, "Email already exists");
                else
                    return this.Request.CreateResponse(HttpStatusCode.OK);
            }
            catch
            {
                return this.Request.CreateBadRequestResponse();
            }
        }

    }
}
