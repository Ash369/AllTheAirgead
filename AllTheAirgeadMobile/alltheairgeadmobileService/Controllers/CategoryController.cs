using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.OData;
using Microsoft.WindowsAzure.Mobile.Service;
using Microsoft.WindowsAzure.Mobile.Service.Security;
using alltheairgeadmobileService.DataObjects;
using alltheairgeadmobileService.Models;

namespace alltheairgeadmobileService.Controllers
{
    [AuthorizeLevel(AuthorizationLevel.User)]
    public class CategoryController : TableController<CategoryDto>
    {
        protected override void Initialize(HttpControllerContext controllerContext)
        {
            base.Initialize(controllerContext);
            alltheairgeadContext context = new alltheairgeadContext(Services.Settings["ExistingDbConnectionString"]);

            // set DomainManger to a new one that we created
            DomainManager = new SimpleMappedEntityDomainManager<CategoryDto, Catagory>(
              context,
              Request,
              Services,
              Category => Category.CategoryName);
        }

        // GET tables/Category
        public IQueryable<CategoryDto> GetAllCategory()
        {
            return Query(); 
        }

        // GET tables/Category/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public SingleResult<CategoryDto> GetCategory(string id)
        {
            return Lookup(id);
        }
/*      Methods not used for now.
        // PATCH tables/Category/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public Task<CategoryDto> PatchCategory(string id, Delta<CategoryDto> patch)
        {
             return UpdateAsync(id, patch);
        }

        // POST tables/Category/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public async Task<IHttpActionResult> PostCategory(CategoryDto item)
        {
            CategoryDto current = await InsertAsync(item);
            return CreatedAtRoute("Tables", new { id = current.Id }, current);
        }

        // DELETE tables/Category/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public Task DeleteCategory(string id)
        {
             return DeleteAsync(id);
        }
*/
    }
}