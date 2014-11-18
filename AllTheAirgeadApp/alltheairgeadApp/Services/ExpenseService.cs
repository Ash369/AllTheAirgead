using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices;
using alltheairgeadApp.DataObjects;

namespace alltheairgeadApp.Services
{
    class ExpenseService
    {
        private IMobileServiceTable<Expense> ExpenseTable = App.alltheairgeadClient.GetTable<Expense>();

        public async Task<Boolean> AddExpense(Expense ExpenseData)
        {
            Boolean result;
            try
            {
                await ExpenseTable.InsertAsync(ExpenseData);
                result = true;
            }
            catch { result = false; }
            return result;
        }

        public async Task<List<Expense>> GetExpenses()
        {
            try
            {
                var Expenses = await ExpenseTable.ToListAsync();
                return Expenses;
            }
            catch { return null; }
        }
    }
}
