using Microsoft.WindowsAzure.Mobile.Service;

namespace alltheairgeadmobileService.DataObjects
{
    /// <summary>
    /// Category object used for communicating with client. Uses Microsoft Entuty Data Framework
    /// </summary>
    public class CategoryDto : EntityData
    {
        public short DefaultPriority { get; set; }
    }
}