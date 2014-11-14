namespace alltheairgeadmobileService.DataObjects
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class Catagory
    {
        [Key]
        public string CategoryName { get; set; }

        [StringLength(50)]
        public string DefaultPriority { get; set; }
    }
}
