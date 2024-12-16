using BTCK_CNXML.Areas.Client.ViewModels;
using BTCK_CNXML.Data;
using BTCK_CNXML.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace BTCK_CNXML.Areas.Client.Controllers
{
    [Area("Client")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly AppDbContext _context;

        public HomeController(ILogger<HomeController> logger, AppDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        [Route("/Client/Home/Index")]
        public IActionResult Index()
        {
            string hoaXmlFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "xml", "hoa.xml");
            string loaiHoaXmlFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "xml", "loaihoa.xml");

            XElement loadedHoaXml = XElement.Load(hoaXmlFilePath);
            XElement loadedLoaiHoaXml = XElement.Load(loaiHoaXmlFilePath);

            var loaiHoas = loadedLoaiHoaXml.Elements("LoaiHoa").ToDictionary(
                x => (string)x.Element("Id"),
                x => (string)x.Element("Name")
            );

            var hoasFromXml = loadedHoaXml.Elements("Hoa").Select(x => new ViewModels.HoaVM
            {
                Id = (string)x.Element("Id"),
                Name = (string)x.Element("Name"),
                Description = (string)x.Element("Description"),
                Price = (decimal)x.Element("Price"),
                StockQuantity = (int)x.Element("StockQuantity"),
                LoaiHoaId = (string)x.Element("LoaiHoaId"),
                LoaiHoaName = loaiHoas.ContainsKey((string)x.Element("LoaiHoaId")) ? loaiHoas[(string)x.Element("LoaiHoaId")] : null,
                ImgUrl = x.Element("Images")?.Elements("Image").FirstOrDefault()?.Element("Url")?.Value,
                Images = x.Element("Images")?.Elements("Image").Select(img => (string)img.Element("Url")).ToList()
            }).ToList();

            return View(hoasFromXml);
        }

        [Route("/Client/Home/List")]
        public IActionResult List()
        {
            string hoaXmlFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "xml", "hoa.xml");
            string loaiHoaXmlFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "xml", "loaihoa.xml");

            XElement loadedHoaXml = XElement.Load(hoaXmlFilePath);
            XElement loadedLoaiHoaXml = XElement.Load(loaiHoaXmlFilePath);

            var loaiHoas = loadedLoaiHoaXml.Elements("LoaiHoa").ToDictionary(
                x => (string)x.Element("Id"),
                x => (string)x.Element("Name")
            );

            var hoasFromXml = loadedHoaXml.Elements("Hoa").Select(x => new ViewModels.HoaVM
            {
                Id = (string)x.Element("Id"),
                Name = (string)x.Element("Name"),
                Description = (string)x.Element("Description"),
                Price = (decimal)x.Element("Price"),
                StockQuantity = (int)x.Element("StockQuantity"),
                LoaiHoaId = (string)x.Element("LoaiHoaId"),
                LoaiHoaName = loaiHoas.ContainsKey((string)x.Element("LoaiHoaId")) ? loaiHoas[(string)x.Element("LoaiHoaId")] : null
            }).ToList();

            return Json(new { Data = hoasFromXml });
        }

        [Route("/Client/Home/Detail/{id}")]
        public IActionResult Detail(string id)
        {
            string hoaXmlFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "xml", "hoa.xml");
            string loaiHoaXmlFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "xml", "loaihoa.xml");

            XElement loadedHoaXml = XElement.Load(hoaXmlFilePath);
            XElement loadedLoaiHoaXml = XElement.Load(loaiHoaXmlFilePath);

            var loaiHoas = loadedLoaiHoaXml.Elements("LoaiHoa").ToDictionary(
                x => (string)x.Element("Id"),
                x => (string)x.Element("Name")
            );

            var hoa = loadedHoaXml.Elements("Hoa")
                .Where(x => (string)x.Element("Id") == id)
                .Select(x => new ViewModels.HoaVM
                {
                    Id = (string)x.Element("Id"),
                    Name = (string)x.Element("Name"),
                    Description = (string)x.Element("Description"),
                    Price = (decimal)x.Element("Price"),
                    StockQuantity = (int)x.Element("StockQuantity"),
                    LoaiHoaId = (string)x.Element("LoaiHoaId"),
                    LoaiHoaName = loaiHoas.ContainsKey((string)x.Element("LoaiHoaId")) ? loaiHoas[(string)x.Element("LoaiHoaId")] : null,
                    Images = x.Element("Images")?.Elements("Image").Select(img => (string)img.Element("Url")).ToList()
                })
                .FirstOrDefault();

            if (hoa == null)
            {
                return NotFound();
            }

            return View(hoa);
        }

        [HttpPost]
        public IActionResult Order([FromBody] OrderRequest orderRequest)
        {
            // Lấy UserId từ session
            string currentUserId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(currentUserId))
            {
                return Unauthorized("User is not logged in");
            }

            // Tạo ID cho đơn đặt hàng
            string orderId = Guid.NewGuid().ToString();

            // Thêm đơn hàng vào tệp client.xml
            string clientXmlFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "xml", "client.xml");
            XElement loadedClientXml = XElement.Load(clientXmlFilePath);

            var clientElement = loadedClientXml.Elements("Client")
                .FirstOrDefault(x => (string)x.Element("Id") == currentUserId);

            if (clientElement == null)
            {
                return NotFound("Client not found");
            }

            var ordersElement = clientElement.Element("Orders");
            if (ordersElement == null)
            {
                ordersElement = new XElement("Orders");
                clientElement.Add(ordersElement);
            }

            var newOrder = new XElement("Order",
                new XElement("Id", orderId),
                new XElement("HoaId", orderRequest.HoaId),
                new XElement("Quantity", orderRequest.Quantity),
                new XElement("TotalPrice", orderRequest.TotalPrice),
                new XElement("OrderDate", DateTime.Now),
                new XElement("Status", 1)
            );

            ordersElement.Add(newOrder);
            loadedClientXml.Save(clientXmlFilePath);

            // Giảm số lượng còn lại của HoaId trong tệp hoa.xml
            string hoaXmlFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "xml", "hoa.xml");
            XElement loadedHoaXml = XElement.Load(hoaXmlFilePath);

            var hoaElement = loadedHoaXml.Elements("Hoa")
                .FirstOrDefault(x => (string)x.Element("Id") == orderRequest.HoaId);

            if (hoaElement != null)
            {
                int currentStockQuantity = (int)hoaElement.Element("StockQuantity");
                hoaElement.SetElementValue("StockQuantity", currentStockQuantity - orderRequest.Quantity);
                loadedHoaXml.Save(hoaXmlFilePath);
            }

            // Gọi hàm UpdateDatabase để đồng bộ hóa cơ sở dữ liệu với tệp XML
            UpdateDatabase();

            // Thêm đơn hàng vào cơ sở dữ liệu
            var newDatHoa = new DatHoa
            {
                Id = orderId, // Sử dụng cùng ID với tệp XML
                HoaId = orderRequest.HoaId,
                UserId = currentUserId,
                Status = 1, // Trạng thái đơn hàng
                Total = orderRequest.TotalPrice,
                OrderDate = DateTime.Now,
                Quantity = orderRequest.Quantity,
            };

            _context.DatHoas.Add(newDatHoa);
            _context.SaveChanges();

            return Ok();
        }


        public IActionResult UpdateDatabase()
        {
            string xmlFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "xml", "hoa.xml");

            if (!System.IO.File.Exists(xmlFilePath))
            {
                ModelState.AddModelError("", "File XML không tồn tại.");
                return RedirectToAction("Index");
            }

            try
            {
                XElement xml = XElement.Load(xmlFilePath);

                var hoas = xml.Elements("Hoa").Select(x => new Hoa
                {
                    Id = (string)x.Element("Id"),
                    Name = (string)x.Element("Name"),
                    Description = (string)x.Element("Description"),
                    Price = (decimal)x.Element("Price"),
                    StockQuantity = (int)x.Element("StockQuantity"),
                    LoaiHoaId = (string)x.Element("LoaiHoaId")
                }).ToList();

                var dbHoas = _context.Hoas.ToList();

                foreach (var hoa in hoas)
                {
                    var existingHoa = dbHoas.FirstOrDefault(r => r.Id.ToString() == hoa.Id);

                    if (existingHoa != null)
                    {
                        existingHoa.Name = hoa.Name;
                        existingHoa.Description = hoa.Description;
                        existingHoa.Price = hoa.Price;
                        existingHoa.StockQuantity = hoa.StockQuantity;
                        existingHoa.LoaiHoaId = hoa.LoaiHoaId;
                    }
                    else
                    {
                        _context.Hoas.Add(new Hoa
                        {
                            Id = hoa.Id,
                            Name = hoa.Name,
                            Description = hoa.Description,
                            Price = hoa.Price,
                            StockQuantity = hoa.StockQuantity,
                            LoaiHoaId = hoa.LoaiHoaId
                        });
                    }
                }

                var idsFromXml = hoas.Select(r => r.Id).ToList();
                var hoasToRemove = dbHoas.Where(r => !idsFromXml.Contains(r.Id.ToString())).ToList();

                foreach (var hoa in hoasToRemove)
                {
                    _context.Hoas.Remove(hoa);
                }

                _context.SaveChanges();

                TempData["SuccessMessage"] = "Cập nhật dữ liệu thành công từ XML vào database!";
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Đã xảy ra lỗi: {ex.Message}");
            }

            return RedirectToAction("Index");
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}