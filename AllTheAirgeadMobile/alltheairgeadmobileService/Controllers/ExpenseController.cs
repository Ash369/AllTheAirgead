using System.Linq;
using System.Threading.Tasks;
using System.Web;
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
    public class ExpenseController : TableController<ExpenseDto>
    {/*
        protected override void Initialize(HttpControllerContext controllerContext)
        {
            base.Initialize(controllerContext);
            alltheairgeadContext context = new alltheairgeadContext(Services.Settings["ExistingDbConnectionString"]);
            //alltheairgeadmobileContext context = new alltheairgeadmobileContext();
            DomainManager = new EntityDomainManager<Expense>(context, Request, Services);
        }
*/
        protected override void Initialize(HttpControllerContext controllerContext)
        {
            base.Initialize(controllerContext);
            //modify the context to use the constructor that will take a connection string - stored in web.config
            alltheairgeadContext context = new alltheairgeadContext(Services.Settings["ExistingDbConnectionString"]);

            // DomainManager = new EntityDomainManager<Expense>(context, Request, Services);
            // set DomainManger to a new one that we created
            DomainManager = new SimpleMappedEntityDomainManager<ExpenseDto, Expense>(
              context,
              Request,
              Services,
              Expense => Expense.ExpenseId);

        }

        // GET tables/Expense
        public IQueryable<ExpenseDto> GetAllExpense()
        {
            return Query(); 
        }

        // GET tables/Expense/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public SingleResult<ExpenseDto> GetExpense(string id)
        {
            return Lookup(id);
        }

        // PATCH tables/Expense/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public Task<ExpenseDto> PatchExpense(string id, Delta<ExpenseDto> patch)
        {
             return UpdateAsync(id, patch);
        }

        // POST tables/Expense/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public async Task<IHttpActionResult> PostExpense(ExpenseDto item)
        {
            try
            {
                // Get the logged in user
                var CurrentUser = User as ServiceUser;
                // Extract email from user
                string Email = CurrentUser.Id.Substring(CurrentUser.Id.IndexOf(':') + 1);
                // Get the UserId from UserProfiles table
                alltheairgeadContext context = new alltheairgeadContext(Services.Settings["ExistingDbConnectionString"]);
                UserProfile account = context.UserProfiles.Where(a => a.Email == Email).SingleOrDefault();
                // Assign incoming item's UserId to current user's
                item.UserId = account.UserId;
            }
            catch
            {
                throw new HttpException(401, "User not found");
            }
            ExpenseDto current = await InsertAsync(item);
            return CreatedAtRoute("Tables", new { id = current.Id }, current);
        }

        // DELETE tables/Expense/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public Task DeleteExpense(string id)
        {
             return DeleteAsync(id);
        }

    }
}