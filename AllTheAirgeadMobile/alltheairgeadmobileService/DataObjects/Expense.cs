namespace alltheairgeadmobileService.DataObjects
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Expense")]
    public partial class Expense
    {
        public int ExpenseId { get; set; }

        public int UserId { get; set; }

        public DateTime Date { get; set; }

        [Required]
        [StringLength(128)]
        public string Category { get; set; }

        [Column(TypeName = "money")]
        public decimal Price { get; set; }

        [StringLength(128)]
        public string MoreInfo { get; set; }

        [Column(TypeName = "smalldatetime")]
        public DateTime? Time { get; set; }

        public short? Priority { get; set; }
    }
}
