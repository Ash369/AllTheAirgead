using System;
using System.Text;
using System.Net.Http;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using Windows.UI.Popups;
using Newtonsoft.Json.Linq;
using Microsoft.WindowsAzure.MobileServices;
using alltheairgeadApp.Handlers;
using alltheairgeadApp.DataObjects;

namespace alltheairgeadApp.Services
{
    public class CustomAccountService
    {
        public static string AuthenticationToken { get; set; }

        public async Task<Boolean> Register(string username, string password)
        {
            string message;
            Boolean result;
            try
            {
                string HttpRequestBodyString = "{Email:\"" + username + "\", Password:\"" + password + "\"}";
                StringContent HttpRequestBody = new StringContent(HttpRequestBodyString, Encoding.UTF8, "application/json");
                var response = await App.alltheairgeadClient.InvokeApiAsync("CustomRegistration", HttpRequestBody, System.Net.Http.HttpMethod.Post, null, null);
                response.EnsureSuccessStatusCode();

                bool LoginVerbose = false;
                if(! await Login(username, password, LoginVerbose))
                {
                    message = "Registered but could not log in";
                    result = true;
                }
                message = "Registered and Logged in successfully";
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

        public async Task<Boolean> Login(string username, string password, Boolean verbose)
        {
            string message;
            Boolean result;
            try
            {
                string HttpRequestBodyString = "{Email:\"" + username + "\", Password:\"" + password + "\"}";
                StringContent HttpRequestBody = new StringContent(HttpRequestBodyString, Encoding.UTF8, "application/json");
                var response = await App.alltheairgeadClient.InvokeApiAsync("CustomLogin", HttpRequestBody, System.Net.Http.HttpMethod.Post, null, null);
                response.EnsureSuccessStatusCode();

                string jsonString = await response.Content.ReadAsStringAsync();
                JObject jsonObject = JObject.Parse(jsonString);
                string token = jsonObject["authenticationToken"].ToString();
                CustomAccountService.AuthenticationToken = token;

                message = "Login Successfull";
                result = true;
            }
            catch (MobileServiceInvalidOperationException ex)
            {
                message = ex.Message;
                result = false;
            }
            if (verbose)
            {
                MessageDialog Dialog = new MessageDialog(message);
                await Dialog.ShowAsync();
            }
            return result;
        }
    }
}
