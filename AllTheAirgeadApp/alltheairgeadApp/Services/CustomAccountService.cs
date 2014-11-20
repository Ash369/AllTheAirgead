using System;
using System.Text;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Windows.Security.Credentials;
using Newtonsoft.Json.Linq;
using Microsoft.WindowsAzure.MobileServices;
using Windows.UI.Xaml.Controls;
using System.Collections.Generic;

namespace alltheairgeadApp.Services
{
    public class CustomAccountService
    {
        string Provider = "custom";
        public async Task<Boolean> Register(string email, string password)
        {
            string message;
            Boolean result;
            try
            {
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
            catch (MobileServiceInvalidOperationException ex)
            {
                message = ex.Message;
                result = false;
            }
            MessageDialog Dialog = new MessageDialog(message);
            await Dialog.ShowAsync();
            return result;
        }

        public async Task<Boolean> Login(string email, string password, Boolean verbose)
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

        public Boolean Logout()
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
    }
}
