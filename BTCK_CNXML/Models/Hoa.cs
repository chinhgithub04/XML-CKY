using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BTCK_CNXML.Models
{
    public class Hoa
    {
        [Key]
        public string Id { get; set; }
        [Required]
        public string Name { get; set; }
        public string Description { get; set; }
        [Required]
        [Column(TypeName ="decimal(18,2)")]
        public decimal Price { get; set; }
        [Required]
        public int StockQuantity { get; set; }
        [ForeignKey(nameof(loaiHoa))]
        public string LoaiHoaId { get; set; }
        [ValidateNever]
        public LoaiHoa loaiHoa { get; set; }
        public ICollection<Image> Images { get; set; } = new List<Image>();
    }
}
