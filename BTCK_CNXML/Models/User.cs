using System.ComponentModel.DataAnnotations;

namespace BTCK_CNXML.Models
{
    public class User
    {
        [Key]   
        public string Id { get; set; }
        [Required]
        [StringLength(50)]
        public string Name { get; set; }
        [Required]
        [StringLength(50)]
        public string Email { get; set; }
        [Required]
        [StringLength(20)]
        public string PhoneNumber { get; set; }
        [Required]
        [StringLength(30)]
        public string Password { get; set; }
        [StringLength(255)]
        public string? AvatarUrl { get; set; }
        public string? Role { get; set; }

    }
}
