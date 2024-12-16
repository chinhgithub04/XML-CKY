namespace BTCK_CNXML.Areas.Admin.ViewModels
{
    public class OrderEditViewModel
    {
        public string OrderId { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string HoaName { get; set; }
        public int Quantity { get; set; }
        public decimal TotalPrice { get; set; }
        public int Status { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime? DeliveryDate { get; set; }
    }
}
