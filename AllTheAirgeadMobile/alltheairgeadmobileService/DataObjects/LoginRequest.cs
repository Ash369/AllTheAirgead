using System;

namespace alltheairgeadmobileService.DataObjects
{
    /// <summary>
    /// Login request data item. Holds email and password
    /// </summary>
    public class LoginRequest
    {
        public String Email { get; set; }
        public String Password { get; set; }
    }
}