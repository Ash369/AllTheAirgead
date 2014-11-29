using System;

namespace alltheairgeadmobileService.DataObjects
{
    /// <summary>
    /// Registration request item. Holds email address and password.
    /// </summary>
    public class RegistrationRequest
    {
        public String Email { get; set; }
        public String Password { get; set; }
    }
}