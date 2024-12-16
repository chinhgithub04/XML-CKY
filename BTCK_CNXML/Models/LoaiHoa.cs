using System.ComponentModel.DataAnnotations;

namespace BTCK_CNXML.Models
{
    public class LoaiHoa
    {
        [Key]
        public string Id { get; set; }
        [Required]
        [StringLength(50)]
        public string Name { get; set; }
        public string Description { get; set; }
    }
}
