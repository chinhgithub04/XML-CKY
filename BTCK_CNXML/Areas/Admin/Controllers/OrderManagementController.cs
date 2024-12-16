using BTCK_CNXML.Data;
using BTCK_CNXML.Models;
using Microsoft.AspNetCore.Mvc;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Xml;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BTCK_CNXML.Areas.Admin.ViewModels;

namespace BTCK_CNXML.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class OrderManagementController : Controller
    {
        private readonly AppDbContext _context;

        public OrderManagementController(AppDbContext context)
        {
            _context = context;
        }

        [Route("/Admin/OrderManagement/Index")]
        public IActionResult Index()
        {
            return View();
        }

        [Route("/Admin/OrderManagement/List")]
        public IActionResult List()
        {
            string clientXmlFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "xml", "client.xml");
            string xsdFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "xsd", "user.xsd");

            if (!System.IO.File.Exists(clientXmlFilePath))
            {
                ModelState.AddModelError("", "File XML client.xml không tồn tại.");
                return View("Error");
            }

            XElement clientXml = XElement.Load(clientXmlFilePath);

            if (!ValidateXmlWithXsd(clientXml, xsdFilePath))
            {
                ModelState.AddModelError("", "XML không hợp lệ với XSD.");
                return View("Error");
            }

            var ordersFromXml = clientXml.Elements("Client").SelectMany(client => client.Element("Orders")?.Elements("Order")?.Select(order => new
            {
                UserId = (string)client.Element("Id"),
                Name = (string)client.Element("Name"),
                Email = (string)client.Element("Email"),
                PhoneNumber = (string)client.Element("PhoneNumber"),
                OrderId = (string)order.Element("Id"),
                HoaId = (string)order.Element("HoaId"),
                HoaName = _context.Hoas.FirstOrDefault(h => h.Id == (string)order.Element("HoaId"))?.Name,
                Quantity = (int?)order.Element("Quantity") ?? 0,
                TotalPrice = (decimal?)order.Element("TotalPrice") ?? 0,
                OrderDate = (DateTime?)order.Element("OrderDate") ?? DateTime.MinValue,
                DeliveryDate = (DateTime?)order.Element("DeliveryDate") ?? DateTime.MinValue,
                Status = (int?)order.Element("Status") ?? 0
            }) ?? Enumerable.Empty<object>()).ToList();

            return Json(new { Data = ordersFromXml });
        }

        [Route("/Admin/OrderManagement/Details/{id}")]
public IActionResult Details(string id)
{
    string clientXmlFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "xml", "client.xml");

    if (!System.IO.File.Exists(clientXmlFilePath))
    {
        ModelState.AddModelError("", "File XML client.xml không tồn tại.");
        return RedirectToAction("Index");
    }

    XElement clientXml = XElement.Load(clientXmlFilePath);

    var orderElement = clientXml.Elements("Client")
        .SelectMany(client => client.Element("Orders")?.Elements("Order")?.Where(order => (string)order.Element("Id") == id)
        .Select(order => new
        {
            OrderId = (string)order.Element("Id"),
            UserId = (string)client.Element("Id"),
            UserName = (string)client.Element("Name"),
            Email = (string)client.Element("Email"),
            PhoneNumber = (string)client.Element("PhoneNumber"),
            HoaName = _context.Hoas.FirstOrDefault(h => h.Id == (string)order.Element("HoaId"))?.Name,
            Quantity = (int?)order.Element("Quantity") ?? 0,
            TotalPrice = (decimal?)order.Element("TotalPrice") ?? 0,
            Status = (int?)order.Element("Status") ?? 0,
            OrderDate = (DateTime?)order.Element("OrderDate") ?? DateTime.MinValue,
            DeliveryDate = (DateTime?)order.Element("DeliveryDate") ?? DateTime.MinValue
        }) ?? Enumerable.Empty<object>()).FirstOrDefault();

    if (orderElement == null)
    {
        ModelState.AddModelError("", "Không tìm thấy đơn hàng.");
        return RedirectToAction("Index");
    }

    return View(orderElement);
}

        [Route("/Admin/OrderManagement/Edit/{id}")]
        public IActionResult Edit(string id)
        {
            string clientXmlFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "xml", "client.xml");

            if (!System.IO.File.Exists(clientXmlFilePath))
            {
                ModelState.AddModelError("", "File XML client.xml không tồn tại.");
                return RedirectToAction("Index");
            }

            XElement clientXml = XElement.Load(clientXmlFilePath);

            var orderElement = clientXml.Elements("Client")
                .SelectMany(client => client.Element("Orders").Elements("Order")
                .Where(order => (string)order.Element("Id") == id)
                .Select(order => new OrderEditViewModel
                {
                    OrderId = (string)order.Element("Id"),
                    UserId = (string)client.Element("Id"),
                    UserName = (string)client.Element("Name"),
                    Email = (string)client.Element("Email"),
                    PhoneNumber = (string)client.Element("PhoneNumber"),
                    HoaName = _context.Hoas.FirstOrDefault(h => h.Id == (string)order.Element("HoaId"))?.Name,
                    Quantity = (int)order.Element("Quantity"),
                    TotalPrice = (decimal)order.Element("TotalPrice"),
                    Status = (int)order.Element("Status"),
                    OrderDate = (DateTime)order.Element("OrderDate"),
                    DeliveryDate = (DateTime?)order.Element("DeliveryDate")
                })).FirstOrDefault();

            if (orderElement == null)
            {
                ModelState.AddModelError("", "Không tìm thấy đơn hàng.");
                return RedirectToAction("Index");
            }

            return View(orderElement);
        }

        [HttpPost]
        [Route("/Admin/OrderManagement/Edit/{id}")]
        public IActionResult Edit(string id, OrderEditViewModel model)
        {
            string clientXmlFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "xml", "client.xml");
            string xsdFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "xsd", "user.xsd");

            if (!System.IO.File.Exists(clientXmlFilePath))
            {
                ModelState.AddModelError("", "File XML client.xml không tồn tại.");
                return View(model);
            }

            XElement clientXml = XElement.Load(clientXmlFilePath);

            var orderElement = clientXml.Elements("Client")
                .SelectMany(client => client.Element("Orders").Elements("Order")
                .Where(order => (string)order.Element("Id") == id))
                .FirstOrDefault();

            if (orderElement == null)
            {
                ModelState.AddModelError("", "Không tìm thấy đơn hàng.");
                return View(model);
            }

            orderElement.SetElementValue("Status", model.Status);

            if (model.Status == 2) // If status is "Đã giao"
            {
                orderElement.SetElementValue("DeliveryDate", DateTime.Now);
            }

            if (ValidateXmlWithXsd(clientXml, xsdFilePath))
            {
                clientXml.Save(clientXmlFilePath);
                TempData["SuccessMessage"] = "Cập nhật đơn hàng thành công!";
                return RedirectToAction("Index");
            }
            else
            {
                ModelState.AddModelError("", "File XML không hợp lệ sau khi cập nhật đơn hàng.");
            }

            return View(model);
        }

        [Route("/Admin/OrderManagement/UpdateDatabase")]
        public ActionResult UpdateDatabase()
        {
            string clientXmlFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "xml", "client.xml");

            if (!System.IO.File.Exists(clientXmlFilePath))
            {
                ModelState.AddModelError("", "File XML không tồn tại.");
                return RedirectToAction("Index");
            }

            XElement clientXml = XElement.Load(clientXmlFilePath);
            var orders = clientXml.Elements("Client").SelectMany(client => client.Element("Orders").Elements("Order").Select(order => new DatHoa
            {
                Id = (string)order.Element("Id"),
                HoaId = (string)order.Element("HoaId"),
                UserId = (string)client.Element("Id"),
                Status = (int?)order.Element("Status") ?? 0,
                Total = (decimal?)order.Element("TotalPrice") ?? 0,
                OrderDate = (DateTime?)order.Element("OrderDate") ?? DateTime.MinValue,
                DeliveryDate = (DateTime?)order.Element("DeliveryDate") ?? DateTime.MinValue,
                Quantity = (int?)order.Element("Quantity") ?? 0
            })).ToList();

            var dbOrders = _context.DatHoas.ToList();

            foreach (var order in orders)
            {
                var existingOrder = dbOrders.FirstOrDefault(r => r.Id == order.Id);

                if (existingOrder != null)
                {
                    existingOrder.HoaId = order.HoaId;
                    existingOrder.UserId = order.UserId;
                    existingOrder.Status = order.Status;
                    existingOrder.Total = order.Total;
                    existingOrder.OrderDate = order.OrderDate;
                    existingOrder.DeliveryDate = order.DeliveryDate;
                    existingOrder.Quantity = order.Quantity;
                }
                else
                {
                    _context.DatHoas.Add(order);
                }
            }

            var idsFromXml = orders.Select(r => r.Id).ToList();
            var ordersToRemove = dbOrders.Where(r => !idsFromXml.Contains(r.Id)).ToList();

            foreach (var order in ordersToRemove)
            {
                _context.DatHoas.Remove(order);
            }

            _context.SaveChanges();

            TempData["SuccessMessage"] = "Cập nhật dữ liệu thành công từ XML vào database!";
            return RedirectToAction("Index");
        }

        [Route("/Admin/OrderManagement/Reset")]
        public IActionResult Reset()
        {
            string clientXmlFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "xml", "client.xml");
            string xsdFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "xsd", "user.xsd");

            try
            {
                var orders = _context.DatHoas.Include(o => o.hoa).Include(o => o.user).ToList();

                XElement xml = new XElement("Clients",
                    orders.GroupBy(o => o.UserId).Select(g => new XElement("Client",
                        new XElement("Id", g.First().user.Id),
                        new XElement("Name", g.First().user.Name),
                        new XElement("Email", g.First().user.Email),
                        new XElement("PhoneNumber", g.First().user.PhoneNumber),
                        new XElement("Password", g.First().user.Password),
                        new XElement("AvatarUrl", g.First().user.AvatarUrl),
                        new XElement("Orders", g.Select(o => new XElement("Order",
                            new XElement("Id", o.Id),
                            new XElement("HoaId", o.HoaId),
                            new XElement("Quantity", o.Quantity),
                            new XElement("TotalPrice", o.Total),
                            new XElement("OrderDate", o.OrderDate),
                            new XElement("DeliveryDate", o.DeliveryDate),
                            new XElement("Status", o.Status)
                        )))
                    ))
                );

                if (ValidateXmlWithXsd(xml, xsdFilePath))
                {
                    xml.Save(clientXmlFilePath);
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

        [Route("/Admin/OrderManagement/ExportToExcel")]
        public IActionResult ExportToExcel()
        {
            string clientXmlFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "xml", "client.xml");

            if (!System.IO.File.Exists(clientXmlFilePath))
            {
                ModelState.AddModelError("", "File XML không tồn tại.");
                return RedirectToAction("Index");
            }

            try
            {
                XElement clientXml = XElement.Load(clientXmlFilePath);

                var orders = clientXml.Elements("Client").SelectMany(client => client.Element("Orders").Elements("Order").Select(order => new
                {
                    UserId = (string)client.Element("Id"),
                    Name = (string)client.Element("Name"),
                    Email = (string)client.Element("Email"),
                    PhoneNumber = (string)client.Element("PhoneNumber"),
                    OrderId = (string)order.Element("Id"),
                    HoaId = (string)order.Element("HoaId"),
                    HoaName = _context.Hoas.FirstOrDefault(h => h.Id == (string)order.Element("HoaId"))?.Name,
                    Quantity = (int)order.Element("Quantity"),
                    TotalPrice = (decimal)order.Element("TotalPrice"),
                    OrderDate = (DateTime)order.Element("OrderDate"),
                    DeliveryDate = (DateTime?)order.Element("DeliveryDate"),
                    Status = (int)order.Element("Status")
                })).ToList();

                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("Orders");
                    var currentRow = 1;

                    worksheet.Cell(currentRow, 1).Value = "UserId";
                    worksheet.Cell(currentRow, 2).Value = "Name";
                    worksheet.Cell(currentRow, 3).Value = "Email";
                    worksheet.Cell(currentRow, 4).Value = "PhoneNumber";
                    worksheet.Cell(currentRow, 5).Value = "OrderId";
                    worksheet.Cell(currentRow, 6).Value = "HoaId";
                    worksheet.Cell(currentRow, 7).Value = "HoaName";
                    worksheet.Cell(currentRow, 8).Value = "Quantity";
                    worksheet.Cell(currentRow, 9).Value = "TotalPrice";
                    worksheet.Cell(currentRow, 10).Value = "OrderDate";
                    worksheet.Cell(currentRow, 11).Value = "DeliveryDate";
                    worksheet.Cell(currentRow, 12).Value = "Status";

                    foreach (var order in orders)
                    {
                        currentRow++;
                        worksheet.Cell(currentRow, 1).Value = order.UserId;
                        worksheet.Cell(currentRow, 2).Value = order.Name;
                        worksheet.Cell(currentRow, 3).Value = order.Email;
                        worksheet.Cell(currentRow, 4).Value = order.PhoneNumber;
                        worksheet.Cell(currentRow, 5).Value = order.OrderId;
                        worksheet.Cell(currentRow, 6).Value = order.HoaId;
                        worksheet.Cell(currentRow, 7).Value = order.HoaName;
                        worksheet.Cell(currentRow, 8).Value = order.Quantity;
                        worksheet.Cell(currentRow, 9).Value = order.TotalPrice.ToString("C", new System.Globalization.CultureInfo("vi-VN"));
                        worksheet.Cell(currentRow, 10).Value = order.OrderDate.ToString("dd/MM/yyyy");
                        worksheet.Cell(currentRow, 11).Value = order.DeliveryDate?.ToString("dd/MM/yyyy");
                        worksheet.Cell(currentRow, 12).Value = order.Status == 1 ? "Đang giao" : "Đã giao";
                    }

                    using (var stream = new MemoryStream())
                    {
                        workbook.SaveAs(stream);
                        var content = stream.ToArray();
                        return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Orders.xlsx");
                    }
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Đã xảy ra lỗi: {ex.Message}");
                return RedirectToAction("Index");
            }
        }

        private bool ValidateXmlWithXsd(XElement xml, string xsdFilePath)
        {
            try
            {
                XmlReaderSettings settings = new XmlReaderSettings();
                settings.Schemas.Add(null, xsdFilePath);
                settings.ValidationType = ValidationType.Schema;

                using (XmlReader reader = xml.CreateReader())
                {
                    while (reader.Read()) { }
                }

                return true;
            }
            catch (XmlSchemaValidationException)
            {
                return false;
            }
        }
    }
}