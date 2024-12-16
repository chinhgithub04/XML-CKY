using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BTCK_CNXML.Models
{
    public class DatHoa
    {
        [Key]
        public string Id { get; set; }
        [ForeignKey(nameof(Hoa))]
        public string HoaId { get; set; }
        [ForeignKey(nameof(user))]
        public string UserId { get; set; }
        [Required]
        public int Status {  get; set; }
        [Required]
        [Column(TypeName ="decimal(18,2)")]
        public decimal Total { get; set; }
        [Required]
        public DateTime OrderDate { get; set; }
        public DateTime DeliveryDate { get; set; }
        [Required]
        public int Quantity { get; set; }
        public Hoa hoa { get; set; }
        public User user { get; set; }
    }
}
