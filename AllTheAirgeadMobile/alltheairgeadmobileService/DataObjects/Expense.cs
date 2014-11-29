namespace alltheairgeadmobileService.DataObjects
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using Microsoft.WindowsAzure.Mobile.Service.Tables;

    /// <summary>
    /// Expense data for communicating with database
    /// </summary>
    [Table("Expense")]
    public partial class Expense
    {
        [Key]
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
        public DateTime Time { get; set; }

        public short? Priority { get; set; }

        /* Used for offline data syncing
        [TableColumn(TableColumnType.Deleted)]
        public bool Deleted { get; set; }

        [TableColumn(TableColumnType.Version)]
        [Timestamp]
        public byte[] Version { get; set; }
        */
    }
}
