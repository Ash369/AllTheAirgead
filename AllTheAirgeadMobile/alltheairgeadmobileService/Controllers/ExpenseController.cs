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
using System;

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

        private UserProfile ValidateUser(ServiceUser CurrentUser)
        {
            try
            {
                // Extract email from user
                string Email = CurrentUser.Id.Substring(CurrentUser.Id.IndexOf(':') + 1);
                // Get the UserId from UserProfiles table
                alltheairgeadContext context = new alltheairgeadContext(Services.Settings["ExistingDbConnectionString"]);
                return context.UserProfiles.Where(a => a.Email == Email).SingleOrDefault();
            }
            catch
            {
                throw new HttpResponseException(System.Net.HttpStatusCode.Unauthorized);
            }
        }
        // GET tables/Expense
        public IQueryable<ExpenseDto> GetAllExpense()
        {
            UserProfile account = ValidateUser(User as ServiceUser);
            return Query().Where(Expense => Expense.UserId == account.UserId);
        }

        // GET tables/Expense/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public SingleResult<ExpenseDto> GetExpense(string id)
        {
            UserProfile account = ValidateUser(User as ServiceUser);

            var query = Lookup(id);
            var result = query.Queryable.First();
            if (result.UserId == account.UserId)
                return Lookup(id);
            else
                throw new HttpResponseException(System.Net.HttpStatusCode.Unauthorized);
        }

        // PATCH tables/Expense/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public Task<ExpenseDto> PatchExpense(string id, Delta<ExpenseDto> patch)
        {
            UserProfile account = ValidateUser(User as ServiceUser);

            ExpenseDto Expense = Lookup(id).Queryable.First();
            if (Expense.UserId == account.UserId)
                return UpdateAsync(id, patch);
            else
                throw new HttpResponseException(System.Net.HttpStatusCode.Unauthorized);
        }

        // POST tables/Expense/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public async Task<IHttpActionResult> PostExpense(ExpenseDto item)
        {
            UserProfile account = ValidateUser(User as ServiceUser);

            // Assign incoming item's UserId to current user's
            item.UserId = account.UserId;
            ExpenseDto current = await InsertAsync(item);
            return CreatedAtRoute("Tables", new { id = current.Id }, current);
        }

        // DELETE tables/Expense/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public Task DeleteExpense(string id)
        {
            UserProfile account = ValidateUser(User as ServiceUser);

            ExpenseDto Expense = Lookup(id).Queryable.First();
            if (Expense.UserId == account.UserId)
                return DeleteAsync(id);
            else
                throw new HttpResponseException(System.Net.HttpStatusCode.Unauthorized);
        }

    }
}