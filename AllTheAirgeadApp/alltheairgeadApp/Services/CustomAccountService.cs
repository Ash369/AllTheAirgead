using System;
using System.Text;
using System.Linq;
using System.Net.Http;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Windows.Security.Credentials;
using Windows.UI.Xaml.Controls;
using Newtonsoft.Json.Linq;
using Microsoft.WindowsAzure.MobileServices;
using alltheairgeadApp.DataObjects;

namespace alltheairgeadApp.Services
{
    /// <summary>
    /// Static class with functions to handle all account functionality including logging in / out and registering a new account.
    /// </summary>
    public static class CustomAccountService
    {
        // Credential provider name for mobile service.
        private static string Provider = "custom";

        /// <summary>
        /// Registers a new user with given email and password. Checks valdity of both
        /// </summary>
        /// <param name="email"></param>
        /// <param name="password"></param>
        /// <returns> Boolean True/False value </returns>
        public static async Task<Boolean> Register(string email, string password)
        {
            // variable to hold values from try / catch block.
            string message;
            Boolean result;

            // Arrempt to register new user
            try
            {
                // Check that the email is a valid format.
                if (!EmailValidation.EmailValidator.Validate(email, true))
                    throw new Exception("Email not in correct format");
                // Minimum password length is 6 characters.
                else if (password.Length < 6)
                    throw new Exception("Password must be at least 6 characters");
                // Form HTTP request body with email and password formatted as json.
                string HttpRequestBodyString = "{Email:\"" + email + "\", Password:\"" + password + "\"}";
                StringContent HttpRequestBody = new StringContent(HttpRequestBodyString, Encoding.UTF8, "application/json");
                // Send request to the movile service and check status code.
                var response = await App.alltheairgeadClient.InvokeApiAsync("CustomRegistration", HttpRequestBody, System.Net.Http.HttpMethod.Post, null, null);
                // Throws an exception in case of failure
                response.EnsureSuccessStatusCode();

                // Attempt to login and suppress any messages during login,
                bool LoginVerbose = false;
                if(! await Login(email, password, LoginVerbose))
                {
                    // If failed to login but registered, inform the user.
                    message = "Registered but could not log in";
                }
                else
                {
                    message = "Registered and Logged in successfully";
                }
                result = true;
            }
            // Catch exceptions and format a message.
            catch (Exception ex)
            {
                message = ex.Message;
                result = false;
            }
            // Display the message.
            await new MessageDialog(message).ShowAsync();
            return result;
        }

        /// <summary>
        /// Login user with given email and password
        /// </summary>
        /// <param name="email"></param>
        /// <param name="password"></param>
        /// <param name="verbose"></param>
        /// <returns></returns>
        public static async Task<Boolean> Login(string email, string password, Boolean verbose)
        {
            // Create a handle to the password vault to look for stored credentials
            PasswordVault Vault = new PasswordVault();
            PasswordCredential Credential = new PasswordCredential();

            string message;
            Boolean result;
            // Look for stored credentials from the custom provider. Throws an error if nothing is found
            try
            {
                Credential = Vault.FindAllByResource(Provider).FirstOrDefault();
            }
            catch
            {
                // Nullify the empty credential if nothing is found
                Credential = null;
            }

            // If a credential is found, create a user from the credential.
            if (Credential != null)
            {
                // Set the current user to the credential user.
                App.alltheairgeadClient.CurrentUser = new MobileServiceUser(Credential.UserName);
                // Make the password available. and store it on the current user.
                Credential.RetrievePassword();
                App.alltheairgeadClient.CurrentUser.MobileServiceAuthenticationToken = Credential.Password;

                // Display a message to tell the user that this one is currently logged in.
                message = "Already logged in as " + Credential.UserName;
                result = true;
            }
            // If no credential was found, attempt to login with the given username and password
            else
            {
                try
                {
                    // Form the HTTP requeset body
                    string HttpRequestBodyString = "{Email:\"" + email + "\", Password:\"" + password + "\"}";
                    StringContent HttpRequestBody = new StringContent(HttpRequestBodyString, Encoding.UTF8, "application/json");
                    // Send the request to the mobile service.
                    var response = await App.alltheairgeadClient.InvokeApiAsync("CustomLogin", HttpRequestBody, System.Net.Http.HttpMethod.Post, null, null);
                    // Check that the response was succesfull. Throw error otherwise
                    response.EnsureSuccessStatusCode();

                    // Parse the resonse to get the credential
                    string jsonString = await response.Content.ReadAsStringAsync();
                    JObject jsonObject = JObject.Parse(jsonString);
                    string token = jsonObject["authenticationToken"].ToString();
                    JObject user = JObject.Parse(jsonObject["user"].ToString());
                    string userId = user["userId"].ToString();
                    // Log the user in the the retirned credential
                    App.alltheairgeadClient.CurrentUser = new MobileServiceUser(userId);
                    App.alltheairgeadClient.CurrentUser.MobileServiceAuthenticationToken = token;

                    // Store the credential in the password vault
                    Credential = new PasswordCredential(Provider, email, token);
                    Vault.Add(Credential);

                    message = "Login Successfull";
                    result = true;
                }
                catch (MobileServiceInvalidOperationException ex)
                {
                    message = ex.Message;
                    result = false;
                }
            }
            if (verbose)
            {
                MessageDialog Dialog = new MessageDialog(message);
                await Dialog.ShowAsync();
            }
            return result;
        }

        /// <summary>
        /// Log the user out of the service
        /// </summary>
        /// <returns></returns>
        public static Boolean Logout()
        {
            PasswordVault Vault = new PasswordVault();
            List<PasswordCredential> Credentials = null;
            try
            {
                // Find all the credentials and remive them.
                Credentials = Vault.FindAllByUserName(App.alltheairgeadClient.CurrentUser.UserId).ToList();
                foreach (PasswordCredential Credential in Credentials)
                    Vault.Remove(Credential);
            }
            catch
            {
            }
            App.alltheairgeadClient.CurrentUser = null;

            return true;
        }

        /// <summary>
        /// Sends a request to the mobile service to ensure the email is available.
        /// </summary>
        /// <param name="Email"></param>
        /// <returns></returns>
        public static async Task<Boolean> EmailAvailableCheck(string Email)
        {
            // Build the HTTP request query.
            Dictionary<string, string> Request = new Dictionary<string, string>();
            Request.Add("Email", Email);
            try
            {
                // Send the request. Returns an error response if the email is invalid or not available.
                var response = await App.alltheairgeadClient.InvokeApiAsync("EmailCheck", null, HttpMethod.Get, null, Request);
                response.EnsureSuccessStatusCode();

                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Search the password vault for a valid credential.
        /// </summary>
        /// <returns></returns>
        public static async Task<bool> FindStoredCredentials()
        {
            // Get handle to the password vault
            PasswordVault Vault = new PasswordVault();
            PasswordCredential Credential = null;
            while (Credential == null)
            {
                try
                {
                    // Get the first credential in the vault.
                    Credential = Vault.RetrieveAll().FirstOrDefault();

                    // Set the current user to the one from the credential
                    App.alltheairgeadClient.CurrentUser = new MobileServiceUser(Credential.UserName);
                    Credential.RetrievePassword();
                    App.alltheairgeadClient.CurrentUser.MobileServiceAuthenticationToken = Credential.Password;

                    // Try to make a request.
                    try
                    {
                        await App.alltheairgeadClient.GetTable<Expense>().Take(1).ToListAsync();
                    }
                    // Catch an unaitherized request to try again.
                    catch (MobileServiceInvalidOperationException ex)
                    {
                        if (ex.Response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                        {
                            // Remove the credential from the vault and go again.
                            Vault.Remove(Credential);
                            Credential = null;
                            continue;
                        }
                        else if (ex.Response.StatusCode == System.Net.HttpStatusCode.NotFound)
                        {
                            // Deal with offline. Future work
                        }
                    }

                }
                // When none are found. Return false.
                catch
                {
                    return false;
                }
            }
            // Show a message to indicate the the user has logged in.
            MessageDialog Dialog = new MessageDialog("Signed in as " + Credential.UserName);
            await Dialog.ShowAsync();
            return true;
        }
    }
}
