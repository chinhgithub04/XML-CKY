namespace BTCK_CNXML.Models
{
    public class OrderRequest
    {
        public string HoaId { get; set; }
        public int Quantity { get; set; }
        public decimal TotalPrice { get; set; }
    }
}