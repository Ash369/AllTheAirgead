using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.WindowsAzure.Mobile.Service.Security;
using alltheairgeadmobileService.Models;

namespace alltheairgeadmobileService.DataObjects
{
    public class CustomLoginProviderCredentials : ProviderCredentials
    {
        public CustomLoginProviderCredentials()
            : base(CustomLoginProvider.ProviderName)
        {
        }
    }
}