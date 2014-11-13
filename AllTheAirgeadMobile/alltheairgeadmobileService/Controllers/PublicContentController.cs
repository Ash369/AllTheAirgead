using System.Net.Http;
using System.Web.Http;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Mobile.Service.Security;
using Microsoft.WindowsAzure.Mobile.Service.Controllers;

namespace alltheairgeadmobileService.Controllers
{
    [AuthorizeLevel(AuthorizationLevel.Anonymous)]
    public class PublicContentController : ContentController
    {
        [HttpGet]
        [Route("content/{*path}")]
        public new async Task<HttpResponseMessage> Index(string path = null)
        {
            return await base.Index(path);
        }
    }
}
