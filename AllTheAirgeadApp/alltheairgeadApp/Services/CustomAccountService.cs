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
    public static class CustomAccountService
    {
        private static string Provider = "custom";
        public static async Task<Boolean> Register(string email, string password)
        {
            string message;
            Boolean result;
            try
            {
                if (!EmailValidation.EmailValidator.Validate(email, true))
                    throw new Exception("Email not in correct format");
                else if (password.Length < 6)
                    throw new Exception("Password must be at least 6 characters");
                string HttpRequestBodyString = "{Email:\"" + email + "\", Password:\"" + password + "\"}";
                StringContent HttpRequestBody = new StringContent(HttpRequestBodyString, Encoding.UTF8, "application/json");
                var response = await App.alltheairgeadClient.InvokeApiAsync("CustomRegistration", HttpRequestBody, System.Net.Http.HttpMethod.Post, null, null);
                response.EnsureSuccessStatusCode();

                bool LoginVerbose = false;
                if(! await Login(email, password, LoginVerbose))
                {
                    message = "Registered but could not log in";
                }
                else
                {
                    message = "Registered and Logged in successfully";
                }
                result = true;
            }
            catch (Exception ex)
            {
                message = ex.Message;
                result = false;
            }
            MessageDialog Dialog = new MessageDialog(message);
            await Dialog.ShowAsync();
            return result;
        }

        public static async Task<Boolean> Login(string email, string password, Boolean verbose)
        {
            PasswordVault Vault = new PasswordVault();
            PasswordCredential Credential = new PasswordCredential();

            string message;
            Boolean result;
            try
            {
                Credential = Vault.FindAllByResource(Provider).FirstOrDefault();
            }
            catch
            {
                // Nullify the empty credential if nothing is found
                Credential = null;
            }

            if (Credential != null)
            {
                App.alltheairgeadClient.CurrentUser = new MobileServiceUser(Credential.UserName);
                Credential.RetrievePassword();
                App.alltheairgeadClient.CurrentUser.MobileServiceAuthenticationToken = Credential.Password;

                message = "Already logged in as " + Credential.UserName;
                result = true;
            }
            else
            {
                try
                {
                    string HttpRequestBodyString = "{Email:\"" + email + "\", Password:\"" + password + "\"}";
                    StringContent HttpRequestBody = new StringContent(HttpRequestBodyString, Encoding.UTF8, "application/json");
                    var response = await App.alltheairgeadClient.InvokeApiAsync("CustomLogin", HttpRequestBody, System.Net.Http.HttpMethod.Post, null, null);
                    response.EnsureSuccessStatusCode();

                    string jsonString = await response.Content.ReadAsStringAsync();
                    JObject jsonObject = JObject.Parse(jsonString);
                    string token = jsonObject["authenticationToken"].ToString();
                    JObject user = JObject.Parse(jsonObject["user"].ToString());
                    string userId = user["userId"].ToString();
                    App.alltheairgeadClient.CurrentUser = new MobileServiceUser(userId);
                    App.alltheairgeadClient.CurrentUser.MobileServiceAuthenticationToken = token;

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

        public static Boolean Logout()
        {
            PasswordVault Vault = new PasswordVault();
            List<PasswordCredential> Credentials = null;
            try
            {
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

        public static async Task<Boolean> EmailAvailableCheck(string Email)
        {
            Dictionary<string, string> Request = new Dictionary<string, string>();
            Request.Add("Email", Email);
            try
            {
                var response = await App.alltheairgeadClient.InvokeApiAsync("EmailCheck", null, HttpMethod.Get, null, Request);
                response.EnsureSuccessStatusCode();

                return true;
            }
            catch
            {
                return false;
            }
        }

        public static async Task<bool> FindStoredCredentials()
        {
            PasswordVault Vault = new PasswordVault();
            PasswordCredential Credential = null;
            while (Credential == null)
            {
                try
                {
                    Credential = Vault.RetrieveAll().FirstOrDefault();

                    App.alltheairgeadClient.CurrentUser = new MobileServiceUser(Credential.UserName);
                    Credential.RetrievePassword();
                    App.alltheairgeadClient.CurrentUser.MobileServiceAuthenticationToken = Credential.Password;

                    try
                    {
                        await App.alltheairgeadClient.GetTable<Expense>().Take(1).ToListAsync();
                    }
                    catch (MobileServiceInvalidOperationException ex)
                    {
                        if (ex.Response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                        {
                            Vault.Remove(Credential);
                            Credential = null;
                            continue;
                        }
                        else if (ex.Response.StatusCode == System.Net.HttpStatusCode.NotFound)
                        {
                            // Deal with offline
                        }
                    }

                }
                catch
                {
                    return false;
                }
            }
            MessageDialog Dialog = new MessageDialog("Signed in as " + Credential.UserName);
            await Dialog.ShowAsync();
            return true;
        }
    }
}
