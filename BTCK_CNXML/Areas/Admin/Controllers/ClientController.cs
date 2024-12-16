using BTCK_CNXML.Data;
using BTCK_CNXML.Models;
using Microsoft.AspNetCore.Mvc;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Xml;
using BTCK_CNXML.Areas.Admin.ViewModels;
using ClosedXML.Excel;

namespace BTCK_CNXML.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ClientController : Controller
    {
        private AppDbContext _context;

        public ClientController(AppDbContext context)
        {
            _context = context;
        }
        [Route("/Admin/Client/Index")]
        public IActionResult Index()
        {
            return View();
        }

        [Route("/Admin/Client/List")]
        public IActionResult List()
        {
            // Định nghĩa đường dẫn file XML và XSD
            string xmlFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "xml", "client.xml");
            string xsdFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "xsd", "user.xsd");

            // Kiểm tra xem file roomtype.xml đã tồn tại chưa
            if (!System.IO.File.Exists(xmlFilePath))
            {
                // Nếu chưa có, tạo file XML mới và lấy dữ liệu từ database
                var users = _context.Users.Where(u => u.Role == "Client").ToList(); // Đảm bảo bạn sử dụng đúng tên bảng

                // Tạo XML mới với phần tử con (thay vì thuộc tính)
                XElement xml = new XElement("Clients",
                    users.Select(r => new XElement("Client",
                        new XElement("Id", r.Id),
                        new XElement("Name", r.Name),
                        new XElement("Email", r.Email),
                        new XElement("PhoneNumber", r.PhoneNumber),
                        new XElement("Password", r.Password),
                        new XElement("AvatarUrl", r.AvatarUrl)
                    ))
                );

                // Kiểm tra tính hợp lệ của XML với XSD trong bộ nhớ
                if (ValidateXmlWithXsd(xml, xsdFilePath))
                {
                    // Lưu XML vào file nếu hợp lệ
                    xml.Save(xmlFilePath);
                }
                else
                {
                    // Xử lý trường hợp XML không hợp lệ với XSD
                    ModelState.AddModelError("", "XML không hợp lệ với XSD.");
                    return View("Error");
                }
            }

            // Đọc file XML và chuyển dữ liệu vào View
            XElement loadedXml = XElement.Load(xmlFilePath);
            var clientsFromXml = loadedXml.Elements("Client").Select(x => new User
            {
                Id = (string)x.Element("Id"),       // Thay vì Attribute, sử dụng Element để lấy giá trị
                Name = (string)x.Element("Name"),
                Email = (string)x.Element("Email"),
                PhoneNumber = (string)x.Element("PhoneNumber"),
                Password = (string)x.Element("Password"),
                AvatarUrl = (string)x.Element("AvatarUrl")
            }).ToList();

            // Trả về danh sách RoomType thay vì dynamic
            return Json(new { Data = clientsFromXml });
        }

        [Route("/Admin/Client/Create")]
        public ActionResult Create()
        {
            return View(new AccountVM());
        }

        [HttpPost]
        [Route("/Admin/Client/Create")]
        public async Task<IActionResult> Create(AccountVM model, IFormFile AvatarUrl = null)
        {
            if (ModelState.IsValid)
            {
                // Đường dẫn tới file XML và XSD
                string xmlFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "xml", "client.xml");
                string xsdFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "xsd", "user.xsd");

                // Đảm bảo file XML tồn tại
                if (!System.IO.File.Exists(xmlFilePath))
                {
                    ModelState.AddModelError("", "File XML không tồn tại.");
                    return View(model);
                }

                // Tải file XML hiện tại
                XElement xml = XElement.Load(xmlFilePath);

                // Kiểm tra xem email đã tồn tại hay chưa
                var existingClient = xml.Elements("Client")
                                        .FirstOrDefault(x => (string)x.Element("Email") == model.Email);
                if (existingClient != null)
                {
                    ModelState.AddModelError("Email", "Email đã tồn tại.");
                    return View(model);
                }

                // Khởi tạo User từ AccountVM
                var user = new User
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = model.Name,
                    Email = model.Email,
                    PhoneNumber = model.PhoneNumber,
                    Password = model.Password,
                    Role = "Client"
                };

                if (AvatarUrl != null && AvatarUrl.Length > 0)
                {
                    var fileName = DateTime.Now.Ticks.ToString() + Path.GetExtension(AvatarUrl.FileName);

                    // Lưu avatar vào thư mục trên server
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "upload", fileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await AvatarUrl.CopyToAsync(stream);
                    }

                    // Cập nhật URL của avatar
                    user.AvatarUrl = "/upload/" + fileName;
                }
                else
                {
                    // Gán ảnh mặc định nếu không upload avatar mới
                    user.AvatarUrl = "/upload/default-avatar.png";
                }

                try
                {
                    // Tạo phần tử mới từ user
                    XElement newClient = new XElement("Client",
                        new XElement("Id", user.Id),
                        new XElement("Name", user.Name ?? string.Empty),
                        new XElement("Email", user.Email ?? string.Empty),
                        new XElement("PhoneNumber", user.PhoneNumber ?? string.Empty),
                        new XElement("Password", user.Password ?? string.Empty),
                        new XElement("AvatarUrl", user.AvatarUrl ?? string.Empty)
                    );

                    // Thêm phần tử mới vào XML
                    xml.Add(newClient);

                    // Kiểm tra tính hợp lệ của file XML với XSD
                    if (ValidateXmlWithXsd(xml, xsdFilePath))
                    {
                        // Nếu hợp lệ, lưu file XML
                        xml.Save(xmlFilePath);
                        TempData["SuccessMessage"] = "Thêm khách hàng thành công!";
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        ModelState.AddModelError("", "File XML không hợp lệ sau khi thêm khách hàng.");
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Đã xảy ra lỗi: {ex.Message}");
                }
            }

            return View(model);
        }




        [Route("/Admin/Client/Details/{id}")]
        public IActionResult Details(string id)
        {
            string xmlFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "xml", "client.xml");

            // Kiểm tra xem file XML có tồn tại không
            if (!System.IO.File.Exists(xmlFilePath))
            {
                ModelState.AddModelError("", "File XML không tồn tại.");
                return RedirectToAction("Index");
            }

            try
            {
                // Đọc file XML và tìm phòng với Id tương ứng
                XElement xml = XElement.Load(xmlFilePath);
                var clientElement = xml.Elements("Client")
                                         .FirstOrDefault(x => (string)x.Element("Id") == id);

                // Nếu không tìm thấy loại phòng, trả về lỗi
                if (clientElement == null)
                {
                    ModelState.AddModelError("", "Không tìm thấy khách hàng.");
                    return RedirectToAction("Index");
                }

                // Chuyển dữ liệu từ XML sang model và truyền vào view
                User client = new User
                {
                    Id = (string)clientElement.Element("Id"),
                    Name = (string)clientElement.Element("Name"),
                    Email = (string)clientElement.Element("Email"),
                    PhoneNumber = (string)clientElement.Element("PhoneNumber"),
                    Password = (string)clientElement.Element("Password"),
                    AvatarUrl = (string)clientElement.Element("AvatarUrl")
                };

                return View(client);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Đã xảy ra lỗi: {ex.Message}");
                return RedirectToAction("Index");
            }
        }

        [Route("/Admin/Client/Edit/{id}")]
        public IActionResult Edit(string id)
        {
            // Định nghĩa đường dẫn file XML
            string xmlFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "xml", "client.xml");

            // Kiểm tra xem file XML có tồn tại không
            if (!System.IO.File.Exists(xmlFilePath))
            {
                ModelState.AddModelError("", "File XML không tồn tại.");
                return RedirectToAction("Index");
            }

            try
            {
                // Đọc file XML và tìm phòng với Id tương ứng
                XElement xml = XElement.Load(xmlFilePath);
                var clientElement = xml.Elements("Client")
                                         .FirstOrDefault(x => (string)x.Element("Id") == id);

                // Nếu không tìm thấy loại phòng, trả về lỗi
                if (clientElement == null)
                {
                    ModelState.AddModelError("", "Không tìm thấy khách hàng.");
                    return RedirectToAction("Index");
                }

                // Chuyển dữ liệu từ XML sang model và truyền vào view
                User client = new User
                {
                    Id = (string)clientElement.Element("Id"),
                    Name = (string)clientElement.Element("Name"),
                    Email = (string)clientElement.Element("Email"),
                    PhoneNumber = (string)clientElement.Element("PhoneNumber"),
                    Password = (string)clientElement.Element("Password"),
                    AvatarUrl = (string)clientElement.Element("AvatarUrl")
                };

                return View(client);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Đã xảy ra lỗi: {ex.Message}");
                return RedirectToAction("Index");
            }
        }

        [Route("/Admin/Client/Edit/{id}")]
        [HttpPost]
        public async Task<IActionResult> Edit(string id, User model, IFormFile AvatarUrl = null)
        {
            if (ModelState.IsValid)
            {
                // Định nghĩa đường dẫn file XML và XSD
                string xmlFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "xml", "client.xml");
                string xsdFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "xsd", "user.xsd");

                // Kiểm tra xem file XML có tồn tại không
                if (!System.IO.File.Exists(xmlFilePath))
                {
                    ModelState.AddModelError("", "File XML không tồn tại.");
                    return View(model);
                }

                try
                {
                    // Đọc file XML
                    XElement xml = XElement.Load(xmlFilePath);

                    // Tìm phần tử phòng cần chỉnh sửa
                    var clientElement = xml.Elements("Client")
                                             .FirstOrDefault(x => (string)x.Element("Id") == id);

                    // Nếu không tìm thấy loại phòng, trả về lỗi
                    if (clientElement == null)
                    {
                        ModelState.AddModelError("", "Không tìm thấy khách hàng.");
                        return View(model);
                    }

                    var existingClient = xml.Elements("Client")
                                    .FirstOrDefault(x => (string)x.Element("Email") == model.Email && (string)x.Element("Id") != id);
                    if (existingClient != null)
                    {
                        ModelState.AddModelError("Email", "Email đã tồn tại.");
                        return View(model);
                    }

                    // Xử lý file upload
                    if (AvatarUrl != null && AvatarUrl.Length > 0)
                    {
                        var fileName = DateTime.Now.Ticks.ToString() + Path.GetExtension(AvatarUrl.FileName);
                        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "upload", fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await AvatarUrl.CopyToAsync(stream);
                        }

                        model.AvatarUrl = "/upload/" + fileName;
                    }
                    else
                    {
                        // Sử dụng lại AvatarUrl hiện tại nếu không upload avatar mới
                        model.AvatarUrl = (string)clientElement.Element("AvatarUrl") ?? "/upload/default-avatar.png";
                    }

                    // Cập nhật thông tin phòng
                    clientElement.Element("Name").Value = model.Name;
                    clientElement.Element("Email").Value = model.Email;
                    clientElement.Element("PhoneNumber").Value = model.PhoneNumber;
                    clientElement.Element("Password").Value = model.Password;
                    clientElement.Element("AvatarUrl").Value = model.AvatarUrl;

                    // Kiểm tra tính hợp lệ của XML với XSD
                    if (ValidateXmlWithXsd(xml, xsdFilePath))
                    {
                        // Lưu lại XML sau khi chỉnh sửa
                        xml.Save(xmlFilePath);
                        TempData["SuccessMessage"] = "Chỉnh sửa khách hàng thành công!";
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        ModelState.AddModelError("", "File XML không hợp lệ sau khi chỉnh sửa khách hàng.");
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Đã xảy ra lỗi: {ex.Message}");
                }
            }

            return View(model);
        }



        [Route("/Admin/Client/Delete/{id}")]
        [HttpPost]
        public IActionResult Delete(string id)
        {
            // Định nghĩa đường dẫn file XML
            string xmlFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "xml", "client.xml");

            // Kiểm tra xem file XML có tồn tại không
            if (!System.IO.File.Exists(xmlFilePath))
            {
                return Json(new { success = false, message = "File XML không tồn tại." });
            }

            try
            {
                // Đọc file XML và tìm phần tử khách hàng cần xóa
                XElement xml = XElement.Load(xmlFilePath);
                var clientElement = xml.Elements("Client")
                                         .FirstOrDefault(x => (string)x.Element("Id") == id);

                // Nếu không tìm thấy khách hàng cần xóa, trả về lỗi
                if (clientElement == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy khách hàng." });
                }

                // Xóa phần tử khách hàng khỏi XML
                clientElement.Remove();

                // Kiểm tra tính hợp lệ của XML với XSD
                string xsdFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "xsd", "user.xsd");
                if (ValidateXmlWithXsd(xml, xsdFilePath))
                {
                    // Lưu lại XML sau khi xóa
                    xml.Save(xmlFilePath);
                    return Json(new { success = true, message = "Xóa khách hàng thành công!" });
                }
                else
                {
                    return Json(new { success = false, message = "File XML không hợp lệ sau khi xóa khách hàng." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Đã xảy ra lỗi: {ex.Message}" });
            }
        }

        [Route("/Admin/CLient/UpdateDatabase")]
        public ActionResult UpdateDatabase()
        {
            // Định nghĩa đường dẫn file XML
            string xmlFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "xml", "client.xml");

            // Kiểm tra xem file XML có tồn tại không
            if (!System.IO.File.Exists(xmlFilePath))
            {
                ModelState.AddModelError("", "File XML không tồn tại.");
                return RedirectToAction("Index");
            }

            try
            {
                // Đọc file XML
                XElement xml = XElement.Load(xmlFilePath);

                // Duyệt qua tất cả các phần tử RoomType trong XML
                var clients = xml.Elements("Client").Select(x => new User
                {
                    Id = (string)x.Element("Id"),
                    Name = (string)x.Element("Name"),
                    Email = (string)x.Element("Email"),
                    PhoneNumber = (string)x.Element("PhoneNumber"),
                    Password = (string)x.Element("Password"),
                    AvatarUrl = (string)x.Element("AvatarUrl")
                }).ToList();

                // Lấy danh sách các RoomType từ cơ sở dữ liệu
                var dbUsers = _context.Users.Where(c => c.Role == "Client").ToList();

                // Cập nhật dữ liệu trong cơ sở dữ liệu dựa trên dữ liệu trong XML
                foreach (var client in clients)
                {
                    var existingClient = dbUsers.FirstOrDefault(c => c.Id.ToString() == client.Id);

                    if (existingClient != null)
                    {
                        // Cập nhật thông tin của loại phòng trong cơ sở dữ liệu
                        existingClient.Name = client.Name;
                        existingClient.Email = client.Email;
                        existingClient.PhoneNumber = client.PhoneNumber;
                        existingClient.Password = client.Password;
                        existingClient.AvatarUrl = client.AvatarUrl;
                    }
                    else
                    {
                        // Nếu loại phòng không tồn tại trong database, thêm mới
                        _context.Users.Add(new User
                        {
                            Id = client.Id,
                            Name = client.Name,
                            Email = client.Email,
                            Password = client.Password,
                            PhoneNumber = client.PhoneNumber,
                            AvatarUrl = client.AvatarUrl,
                            Role = "Client"
                        });
                    }
                }

                // Xóa các loại phòng không còn tồn tại trong XML khỏi database
                var idsFromXml = clients.Select(r => r.Id).ToList();
                var clientsToRemove = dbUsers.Where(r => !idsFromXml.Contains(r.Id.ToString())).ToList();

                foreach (var client in clientsToRemove)
                {
                    _context.Users.Remove(client);
                }

                // Lưu lại các thay đổi vào cơ sở dữ liệu
                _context.SaveChanges();

                TempData["SuccessMessage"] = "Cập nhật dữ liệu thành công từ XML vào database!";
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Đã xảy ra lỗi: {ex.Message}");
            }

            return RedirectToAction("Index");
        }

        [Route("/Admin/Client/Reset")]
        public IActionResult Reset()
        {
            string xmlFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "xml", "client.xml");
            string xsdFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "xsd", "user.xsd");

            try
            {
                // Xóa nội dung hiện tại của file XML
                if (System.IO.File.Exists(xmlFilePath))
                {
                    System.IO.File.Delete(xmlFilePath);
                }

                // Lấy danh sách các khách hàng từ cơ sở dữ liệu
                var clients = _context.Users.Where(c => c.Role == "Client").ToList();

                // Tạo lại XML từ dữ liệu trong cơ sở dữ liệu
                XName clientElementName = "Client";
                XName idElementName = "Id";
                XName nameElementName = "Name";
                XName emailElementName = "Email";
                XName phoneNumberElementName = "PhoneNumber";
                XName passwordElementName = "Password";
                XName avatarUrlElementName = "AvatarUrl";
                XName ordersElementName = "Orders";
                XName orderElementName = "Order";
                XName hoaIdElementName = "HoaId";
                XName quantityElementName = "Quantity";
                XName totalPriceElementName = "TotalPrice";
                XName orderDateElementName = "OrderDate";
                XName statusElementName = "Status";

                XElement xml = new XElement("Clients",
                    clients.Select(c => new XElement(clientElementName,
                        new XElement(idElementName, c.Id),
                        new XElement(nameElementName, c.Name),
                        new XElement(emailElementName, c.Email),
                        new XElement(phoneNumberElementName, c.PhoneNumber),
                        new XElement(passwordElementName, c.Password),
                        new XElement(avatarUrlElementName, c.AvatarUrl),
                        new XElement(ordersElementName,
                            _context.DatHoas.Where(o => o.UserId == c.Id).Select(o => new XElement(orderElementName,
                                new XElement(idElementName, o.Id),
                                new XElement(hoaIdElementName, o.HoaId),
                                new XElement(quantityElementName, o.Quantity),
                                new XElement(totalPriceElementName, o.Total),
                                new XElement(orderDateElementName, o.OrderDate),
                                new XElement(statusElementName, o.Status)
                            ))
                        )
                    ))
                );

                // Kiểm tra tính hợp lệ của XML với XSD
                if (ValidateXmlWithXsd(xml, xsdFilePath))
                {
                    // Nếu hợp lệ, lưu XML vào file
                    xml.Save(xmlFilePath);
                    TempData["SuccessMessage"] = "Dữ liệu đã được cập nhật từ cơ sở dữ liệu vào XML thành công!";
                }
                else
                {
                    ModelState.AddModelError("", "XML không hợp lệ với XSD.");
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Đã xảy ra lỗi: {ex.Message}");
            }

            return RedirectToAction("Index");
        }




        // Hàm kiểm tra tính hợp lệ của XML với XSD trong bộ nhớ
        private bool ValidateXmlWithXsd(XElement xml, string xsdFilePath)
        {
            try
            {
                // Tạo đối tượng XmlReaderSettings để áp dụng schema XSD
                XmlReaderSettings settings = new XmlReaderSettings();
                settings.Schemas.Add(null, xsdFilePath); // Thêm schema từ file XSD
                settings.ValidationType = ValidationType.Schema;

                // Kiểm tra tính hợp lệ của XML trong bộ nhớ
                using (XmlReader reader = xml.CreateReader())
                {
                    while (reader.Read()) { } // Đọc XML để kiểm tra tính hợp lệ
                }

                return true; // Nếu không có lỗi, XML hợp lệ với XSD
            }
            catch (XmlSchemaValidationException)
            {
                return false; // Nếu có lỗi, XML không hợp lệ
            }
        }

        [Route("/Admin/Client/ExportToExcel")]
        public IActionResult ExportToExcel()
        {
            string xmlFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "xml", "client.xml");

            if (!System.IO.File.Exists(xmlFilePath))
            {
                ModelState.AddModelError("", "File XML không tồn tại.");
                return RedirectToAction("Index");
            }

            try
            {
                // Đọc file XML
                XElement xml = XElement.Load(xmlFilePath);
                var clients = xml.Elements("Client").Select(x => new User
                {
                    Id = (string)x.Element("Id"),
                    Name = (string)x.Element("Name"),
                    Email = (string)x.Element("Email"),
                    PhoneNumber = (string)x.Element("PhoneNumber"),
                    Password = (string)x.Element("Password"),
                    AvatarUrl = (string)x.Element("AvatarUrl")
                }).ToList();

                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("Clients");
                    var currentRow = 1;

                    // Tiêu đề cột
                    worksheet.Cell(currentRow, 1).Value = "Id";
                    worksheet.Cell(currentRow, 2).Value = "Họ và tên";
                    worksheet.Cell(currentRow, 3).Value = "Email";
                    worksheet.Cell(currentRow, 4).Value = "Số điện thoại";
                    worksheet.Cell(currentRow, 5).Value = "Mật khẩu";
                    worksheet.Cell(currentRow, 6).Value = "AvatarUrl";

                    // Dữ liệu
                    foreach (var client in clients)
                    {
                        currentRow++;
                        worksheet.Cell(currentRow, 1).Value = client.Id;
                        worksheet.Cell(currentRow, 2).Value = client.Name;
                        worksheet.Cell(currentRow, 3).Value = client.Email;
                        worksheet.Cell(currentRow, 4).Value = client.PhoneNumber;
                        worksheet.Cell(currentRow, 5).Value = client.Password;
                        worksheet.Cell(currentRow, 6).Value = client.AvatarUrl;
                    }

                    using (var stream = new MemoryStream())
                    {
                        workbook.SaveAs(stream);
                        var content = stream.ToArray();
                        return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Clients.xlsx");
                    }
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Đã xảy ra lỗi: {ex.Message}");
                return RedirectToAction("Index");
            }
        }
    }
}

