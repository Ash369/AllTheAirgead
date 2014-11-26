using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Microsoft.WindowsAzure.MobileServices;
using alltheairgeadApp.DataObjects;

namespace alltheairgeadApp.Services
{
    /// <summary>
    /// Handles all expense related queries
    /// </summary>
    class ExpenseService
    {
        // Handle to the expense table in the database.
        private static IMobileServiceTable<Expense> ExpenseTable = App.alltheairgeadClient.GetTable<Expense>();
        
        /// <summary>
        /// Ensures the given expense is valid
        /// </summary>
        /// <param name="ExpenseData"></param>
        public static void ValidateExpense(Expense ExpenseData)
        {
            if (String.IsNullOrWhiteSpace(ExpenseData.Category))
                throw new Exception("Category must be specified");

            if (new DateTime((ExpenseData.Date + ExpenseData.Time.TimeOfDay).Ticks) > DateTime.Now)
                throw new Exception("Date and time must not be in the future");

            if (ExpenseData.Price < 0)
                throw new Exception("Price must be positive");
        }

        /// <summary>
        /// Adds a new expense to the database.
        /// </summary>
        /// <param name="ExpenseData"></param>
        /// <returns></returns>
        public static async Task<bool> AddExpense(Expense ExpenseData)
        {
            string message;
            try
            {
                // Ensure the expense is valid and call the movile service
                ValidateExpense(ExpenseData);
                await ExpenseTable.InsertAsync(ExpenseData);
                message = "Expense Saved";
            }
            catch (Exception ex)
            {
                // In case of an error, give the reason for the failure
                message = "Expense could not be added" + Environment.NewLine + ex.Message;
                return false;
            }
            // Show the message
            await new MessageDialog(message).ShowAsync();
            return true;
        }

        /// <summary>
        /// Get all user expenses from the database.
        /// </summary>
        /// <returns></returns>
        public static async Task<List<Expense>> GetExpenses()
        {
            try
            {
                // Request the expenses from the mobile service
                var Expenses = await ExpenseTable.ToListAsync();
                return Expenses;
            }
            // If an error occured, return a null object
            catch 
            {
                return null;
            }
        }

        /// <summary>
        /// Update a specified expense.
        /// </summary>
        /// <param name="Old"></param>
        /// <param name="New"></param>
        /// <returns></returns>
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
                // Call the mobile service to update the database.
                await ExpenseTable.UpdateAsync(New);
                message = "Updated Successfully";
                result = true;
            }
            // If an error occured inform the user
            catch (Exception ex)
            {
                message = "Update Failed" + Environment.NewLine + ex.Message;
                result = false;
            }
            await new MessageDialog(message).ShowAsync();
            return result;
        }

        /// <summary>
        /// Delete a specified expense
        /// </summary>
        /// <param name="ExpenseData"></param>
        /// <returns></returns>
        public static async Task<bool> DeleteExpense(Expense ExpenseData)
        {
            string message;
            bool result;
            try
            {
                // Call the mobile service to perform the delete
                await ExpenseTable.DeleteAsync(ExpenseData);
                message = "Deleted";
                result = true;
            }
            // Inform the user in case of an error
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
