using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace BTCK_CNXML.Models
{
    public class Image
    {
        [Key]
        public string Id { get; set; }
        [Required]
        public string Url { get; set; }
        [ForeignKey(nameof(Hoa))]
        public string HoaId { get; set; }
        [ValidateNever]
        public Hoa Hoa { get; set; }
    }
}
