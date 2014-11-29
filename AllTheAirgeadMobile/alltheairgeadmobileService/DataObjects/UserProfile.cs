namespace alltheairgeadmobileService.DataObjects
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    /// <summary>
    /// User profile data object for communicating with database UserProfile table
    /// </summary>
    [Table("UserProfile")]
    public partial class UserProfile
    {
        [StringLength(4000)]
        public string Email { get; set; }

        [Key]
        public int UserId { get; set; }
    }
}
