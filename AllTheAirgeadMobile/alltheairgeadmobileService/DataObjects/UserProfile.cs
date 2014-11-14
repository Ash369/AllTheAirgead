namespace alltheairgeadmobileService.DataObjects
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("UserProfile")]
    public partial class UserProfile
    {
        [StringLength(4000)]
        public string Email { get; set; }

        [Key]
        public int UserId { get; set; }
    }
}
