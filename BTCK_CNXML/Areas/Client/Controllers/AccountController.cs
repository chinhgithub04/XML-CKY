using Microsoft.AspNetCore.Mvc;
using System.Xml.Linq;
using Microsoft.AspNetCore.Http;
using BTCK_CNXML.Areas.Client.ViewModels;
using Microsoft.EntityFrameworkCore;
using BTCK_CNXML.Data;
using BTCK_CNXML.Models;

namespace BTCK_CNXML.Areas.Client.Controllers
{
    [Area("Client")]
    public class AccountController : Controller
    {
        private readonly AppDbContext _context;
        public AccountController(AppDbContext context)
        {
            _context = context;
        }

        [Route("/Client/Account/OrderHistory")]
        public IActionResult OrderHistory()
        {
            return View();
        }

        [Route("/Client/Account/List")]
        public IActionResult List()
        {
            // Lấy UserId từ session
            string currentUserId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(currentUserId))
            {
                return Unauthorized("User is not logged in");
            }

            // Đọc dữ liệu từ tệp client.xml
            string clientXmlFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "xml", "client.xml");
            XElement loadedClientXml = XElement.Load(clientXmlFilePath);

            var clientElement = loadedClientXml.Elements("Client")
                .FirstOrDefault(x => (string)x.Element("Id") == currentUserId);

            if (clientElement == null)
            {
                return NotFound("Client not found");
            }

            // Đọc dữ liệu từ tệp hoa.xml để lấy tên hoa
            string hoaXmlFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "xml", "hoa.xml");
            XElement loadedHoaXml = XElement.Load(hoaXmlFilePath);

            var hoaDictionary = loadedHoaXml.Elements("Hoa").ToDictionary(
                x => (string)x.Element("Id"),
                x => (string)x.Element("Name")
            );

            var orders = clientElement.Element("Orders")?.Elements("Order").Select(order => new OrderVM
            {
                Id = (string)order.Element("Id"),
                HoaId = (string)order.Element("HoaId"),
                HoaName = hoaDictionary.ContainsKey((string)order.Element("HoaId")) ? hoaDictionary[(string)order.Element("HoaId")] : "Unknown",
                Quantity = (int)order.Element("Quantity"),
                TotalPrice = (decimal)order.Element("TotalPrice"),
                OrderDate = (DateTime)order.Element("OrderDate"),
                Status = (int)order.Element("Status")
            }).ToList();

            return Json(new { data = orders });
        }

        [HttpPost]
        [Route("/Client/Account/CancelOrder")]
        public IActionResult CancelOrder([FromBody] string orderId)
        {
            // Lấy UserId từ session
            string currentUserId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(currentUserId))
            {
                return Unauthorized("User is not logged in");
            }

            // Đọc dữ liệu từ tệp client.xml
            string clientXmlFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "xml", "client.xml");
            XElement loadedClientXml = XElement.Load(clientXmlFilePath);

            var clientElement = loadedClientXml.Elements("Client")
                .FirstOrDefault(x => (string)x.Element("Id") == currentUserId);

            if (clientElement == null)
            {
                return NotFound("Client not found");
            }

            var orderElement = clientElement.Element("Orders")?.Elements("Order")
                .FirstOrDefault(x => (string)x.Element("Id") == orderId);

            if (orderElement == null)
            {
                return NotFound("Order not found");
            }

            // Cập nhật trạng thái đơn hàng trong XML
            orderElement.Element("Status").Value = "3";
            loadedClientXml.Save(clientXmlFilePath);

            // Cập nhật trạng thái đơn hàng trong cơ sở dữ liệu
            var order = _context.DatHoas.FirstOrDefault(o => o.Id == orderId);
            if (order != null)
            {
                order.Status = 3;
                _context.DatHoas.Update(order); // Đảm bảo rằng đối tượng được đánh dấu là đã thay đổi
                _context.SaveChanges();
            }

            return Ok();
        }

        [Route("/Client/Account/Index")]
        public IActionResult Index()
        {
            // Lấy UserId từ session
            string currentUserId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(currentUserId))
            {
                return Unauthorized("User is not logged in");
            }

            // Đọc dữ liệu từ tệp client.xml
            string clientXmlFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "xml", "client.xml");
            XElement loadedClientXml = XElement.Load(clientXmlFilePath);

            var clientElement = loadedClientXml.Elements("Client")
                .FirstOrDefault(x => (string)x.Element("Id") == currentUserId);

            if (clientElement == null)
            {
                return NotFound("Client not found");
            }

            var user = new User
            {
                Id = (string)clientElement.Element("Id"),
                Name = (string)clientElement.Element("Name"),
                Email = (string)clientElement.Element("Email"),
                PhoneNumber = (string)clientElement.Element("PhoneNumber"),
                AvatarUrl = (string)clientElement.Element("AvatarUrl")
            };

            var model = new AccountVM
            {
                User = user,
                ChangePassword = new ChangePasswordVM()
            };

            return View(model);
        }

        [Route("/Client/Account/UpdateProfile")]
        public IActionResult UpdateProfile()
        {
            // Lấy UserId từ session
            string currentUserId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(currentUserId))
            {
                return Unauthorized("User is not logged in");
            }

            // Đọc dữ liệu từ tệp client.xml
            string clientXmlFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "xml", "client.xml");
            XElement loadedClientXml = XElement.Load(clientXmlFilePath);

            var clientElement = loadedClientXml.Elements("Client")
                .FirstOrDefault(x => (string)x.Element("Id") == currentUserId);

            if (clientElement == null)
            {
                return NotFound("Client not found");
            }

            var user = new User
            {
                Id = (string)clientElement.Element("Id"),
                Name = (string)clientElement.Element("Name"),
                Email = (string)clientElement.Element("Email"),
                PhoneNumber = (string)clientElement.Element("PhoneNumber"),
                AvatarUrl = (string)clientElement.Element("AvatarUrl")
            };

            var model = new AccountVM
            {
                User = user,
                ChangePassword = new ChangePasswordVM()
            };

            return View(model);
        }

        [HttpPost]
        [Route("/Client/Account/UpdateProfile")]
        public async Task<IActionResult> UpdateProfile(User updatedUser, IFormFile AvatarFile)
        {
            // Lấy UserId từ session
            string currentUserId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(currentUserId))
            {
                return Unauthorized("User is not logged in");
            }

            // Đọc dữ liệu từ tệp client.xml
            string clientXmlFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "xml", "client.xml");
            XElement loadedClientXml = XElement.Load(clientXmlFilePath);

            var clientElement = loadedClientXml.Elements("Client")
                .FirstOrDefault(x => (string)x.Element("Id") == currentUserId);

            if (clientElement == null)
            {
                return NotFound("Client not found");
            }

            // Xử lý file upload
            if (AvatarFile != null && AvatarFile.Length > 0)
            {
                var fileName = DateTime.Now.Ticks.ToString() + Path.GetExtension(AvatarFile.FileName);
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "upload", fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await AvatarFile.CopyToAsync(stream);
                }

                updatedUser.AvatarUrl = "/upload/" + fileName;
            }

            // Cập nhật thông tin cá nhân trong XML
            clientElement.Element("Name").Value = updatedUser.Name;
            clientElement.Element("Email").Value = updatedUser.Email;
            clientElement.Element("PhoneNumber").Value = updatedUser.PhoneNumber;
            clientElement.Element("AvatarUrl").Value = updatedUser.AvatarUrl;
            loadedClientXml.Save(clientXmlFilePath);

            // Cập nhật thông tin cá nhân trong cơ sở dữ liệu
            var user = _context.Users.FirstOrDefault(u => u.Id == currentUserId);
            if (user == null)
            {
                return NotFound("User not found");
            }

            user.Name = updatedUser.Name;
            user.Email = updatedUser.Email;
            user.PhoneNumber = updatedUser.PhoneNumber;
            user.AvatarUrl = updatedUser.AvatarUrl;

            _context.Users.Update(user);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        [HttpPost]
        [Route("/Client/Account/ChangePassword")]
        public IActionResult ChangePassword(ChangePasswordVM model)
        {
            // Lấy UserId từ session
            string currentUserId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(currentUserId))
            {
                return Unauthorized("User is not logged in");
            }

            // Đọc dữ liệu từ tệp client.xml
            string clientXmlFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "xml", "client.xml");
            XElement loadedClientXml = XElement.Load(clientXmlFilePath);

            var clientElement = loadedClientXml.Elements("Client")
                .FirstOrDefault(x => (string)x.Element("Id") == currentUserId);

            if (clientElement == null)
            {
                return NotFound("Client not found");
            }

            // Kiểm tra mật khẩu cũ
            if ((string)clientElement.Element("Password") != model.OldPassword)
            {
                return Json(new { success = false, message = "Mật khẩu cũ không chính xác", field = "OldPassword" });
            }

            // Kiểm tra xác nhận mật khẩu mới
            if (model.NewPassword != model.ConfirmNewPassword)
            {
                return Json(new { success = false, message = "Mật khẩu không trùng khớp", field = "ConfirmNewPassword" });
            }

            // Cập nhật mật khẩu mới trong XML
            clientElement.Element("Password").Value = model.NewPassword;
            loadedClientXml.Save(clientXmlFilePath);

            // Cập nhật mật khẩu mới trong cơ sở dữ liệu
            var user = _context.Users.FirstOrDefault(u => u.Id == currentUserId);
            if (user == null)
            {
                return NotFound("User not found");
            }

            user.Password = model.NewPassword;
            _context.Users.Update(user);
            _context.SaveChanges();

            return Json(new { success = true, message = "Password changed successfully" });
        }

    }
}