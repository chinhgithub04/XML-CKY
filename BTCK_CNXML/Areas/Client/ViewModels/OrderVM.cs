using BTCK_CNXML.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace BTCK_CNXML.Areas.Client.ViewModels
{
    public class OrderVM
    {
        public string Id { get; set; }
        public string HoaId { get; set; }
        public int Status { get; set; }
        public decimal TotalPrice { get; set; }
        public DateTime OrderDate { get; set; }
        public int Quantity { get; set; }
        public string HoaName { get; set; }
    }
}
