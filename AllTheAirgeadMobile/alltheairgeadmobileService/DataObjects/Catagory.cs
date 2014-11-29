namespace alltheairgeadmobileService.DataObjects
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    /// <summary>
    /// Category class for communicating with the database database
    /// </summary>
    public partial class Catagory
    {
        [Key]
        public string CategoryName { get; set; }

        public short DefaultPriority { get; set; }
    }
}
