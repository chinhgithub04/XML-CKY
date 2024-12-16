using BTCK_CNXML.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BTCK_CNXML.Areas.Admin.ViewModels
{
    public class HoaCreateVM
    {
        [Required]
        public string Name { get; set; }
        public string Description { get; set; }
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }
        [Required]
        public int StockQuantity { get; set; }
        [Required]
        public string LoaiHoaId { get; set; }
        public IFormFile[] Images { get; set; }
    }
}