namespace BTCK_CNXML.Areas.Client.ViewModels
{
    public class HoaVM
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public string LoaiHoaId { get; set; }
        public string LoaiHoaName { get; set; }
        public string ImgUrl { get; set; }
        public List<string> Images { get; set; }
    }
}