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
    // Only allow authenticated users access to the expenses database
    [AuthorizeLevel(AuthorizationLevel.User)]
    public class ExpenseController : TableController<ExpenseDto>
    {
        /// <summary>
        /// Initialize the table controller
        /// </summary>
        protected override void Initialize(HttpControllerContext controllerContext)
        {
            base.Initialize(controllerContext);
            //modify the context to use the constructor that will take a connection string - stored in web.config
            alltheairgeadContext context = new alltheairgeadContext(Services.Settings["ExistingDbConnectionString"]);

            // set DomainManger to a new one that we created
            DomainManager = new SimpleMappedEntityDomainManager<ExpenseDto, Expense>(
              context,
              Request,
              Services,
              Expense => Expense.ExpenseId);

        }

        /// <summary>
        /// Get the email from the current user structure
        /// </summary>
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

        /// <summary>
        /// GET tables/Expense Return all expenses from the current user
        /// </summary>
        public IQueryable<ExpenseDto> GetAllExpense()
        {
            // Get the user that sent the request
            UserProfile account = ValidateUser(User as ServiceUser);
            // Return query for the expenses for the current user
            return Query().Where(ExpenseData => ExpenseData.UserId == account.UserId);
        }

        /// <summary>
        /// GET tables/Expense/{id} Get the specified expense id from the database
        /// </summary>
        public SingleResult<ExpenseDto> GetExpense(string id)
        {
            // Get the user that sent the request
            UserProfile account = ValidateUser(User as ServiceUser);

            // Get the user associated with the requested item
            var query = Lookup(id);
            var result = query.Queryable.First();
            // If the item belongs to the user, return the item.
            if (result.UserId == account.UserId)
                return query;
            // Else throw an unautherized response
            else
                throw new HttpResponseException(System.Net.HttpStatusCode.Unauthorized);
        }

        /// <summary>
        /// PATCH tables/Expense/{id}
        /// </summary>
        public Task<ExpenseDto> PatchExpense(string id, Delta<ExpenseDto> patch)
        {
            // Get the user that sent the request
            UserProfile account = ValidateUser(User as ServiceUser);

            // Get the requested existing expense.
            ExpenseDto ExpenseData = Lookup(id).Queryable.First();
            // Check that it belongs to the requester
            if (ExpenseData.UserId == account.UserId)
                // Update the request
                return UpdateAsync(id, patch);
            // If it doesn't belong to the requester, throw an unautherized error
            else
                throw new HttpResponseException(System.Net.HttpStatusCode.Unauthorized);
        }

        /// <summary>
        /// POST tables/Expense/{id}
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public async Task<IHttpActionResult> PostExpense(ExpenseDto item)
        {
            // Get the usser that sent the request
            UserProfile account = ValidateUser(User as ServiceUser);

            // Assign incoming item's UserId to current user's
            item.UserId = account.UserId;
            ExpenseDto current = await InsertAsync(item);
            // Add the item to the database
            return CreatedAtRoute("Tables", new { id = current.Id }, current);
        }

        /// <summary>
        /// DELETE tables/Expense/48D68C86-6EA6-4C25-AA33-223FC9A27959
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Task DeleteExpense(string id)
        {
            // Get the user that sent the request
            UserProfile account = ValidateUser(User as ServiceUser);

            // Get the expense to be deleted
            ExpenseDto ExpenseData = Lookup(id).Queryable.First();
            // Check that the expense belongs to the requester
            if (ExpenseData.UserId == account.UserId)
                // Delete the expense
                return DeleteAsync(id);
            // Throw an unatyheriszed error if it doesn't belong
            else
                throw new HttpResponseException(System.Net.HttpStatusCode.Unauthorized);
        }

    }
}