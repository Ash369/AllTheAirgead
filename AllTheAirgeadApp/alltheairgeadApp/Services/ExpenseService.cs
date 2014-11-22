using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Microsoft.WindowsAzure.MobileServices;
using alltheairgeadApp.DataObjects;

namespace alltheairgeadApp.Services
{
    class ExpenseService
    {
        private static IMobileServiceTable<Expense> ExpenseTable = App.alltheairgeadClient.GetTable<Expense>();
        
        public static void ValidateExpense(Expense ExpenseData)
        {
            if (String.IsNullOrWhiteSpace(ExpenseData.Category))
                throw new Exception("Category must be specified");

            if (new DateTime((ExpenseData.Date + ExpenseData.Time.TimeOfDay).Ticks) > DateTime.Now)
                throw new Exception("Date and time must not be in the future");

            if (ExpenseData.Price < 0)
                throw new Exception("Price must be positive");
        }

        public static async Task<bool> AddExpense(Expense ExpenseData)
        {
            string message;
            try
            {
                ValidateExpense(ExpenseData);
                await ExpenseTable.InsertAsync(ExpenseData);
                message = "Expense Saved";
            }
            catch (Exception ex)
            {
                message = "Expense could not be added" + Environment.NewLine + ex.Message;
                return false;
            }
            await new MessageDialog(message).ShowAsync();
            return true;
        }

        public static async Task<List<Expense>> GetExpenses()
        {
            try
            {
                var Expenses = await ExpenseTable.ToListAsync();
                return Expenses;
            }
            catch 
            {
                return null;
            }
        }

        public static async Task<bool> UpdateExpense(Expense Old, Expense New)
        {
            string message;
            bool result;
            try
            {
                // Make sure the new data is valid
                ValidateExpense(New);
                // Get ID from old expense for new expense
                New.Id = Old.Id;
                await ExpenseTable.UpdateAsync(New);
                message = "Updated Successfully";
                result = true;
            }
            catch (Exception ex)
            {
                message = "Update Failed" + Environment.NewLine + ex.Message;
                result = false;
            }
            await new MessageDialog(message).ShowAsync();
            return result;
        }

        public static async Task<bool> DeleteExpense(Expense ExpenseData)
        {
            string message;
            bool result;
            try
            {
                await ExpenseTable.DeleteAsync(ExpenseData);
                message = "Deleted";
                result = true;
            }
            catch (Exception ex)
            {
                message = "Failed to delete" + Environment.NewLine + ex.Message;
                result = false;
            }
            await new MessageDialog(message).ShowAsync();
            return result;
        }

    }
}
