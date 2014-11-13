using Microsoft.WindowsAzure.Mobile.Service;

namespace alltheairgeadmobileService.DataObjects
{
    public class Account : EntityData
    {
        public string Username { get; set; }
        public byte[] HashedPassword { get; set; }
    }
}