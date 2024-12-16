using System.ComponentModel.DataAnnotations;

namespace BTCK_CNXML.Areas.Admin.ViewModels
{
    public class LoaiHoaVM
    {
        [Required]
        [StringLength(50)]
        public string Name { get; set; }
        public string Description { get; set; }
    }
}
