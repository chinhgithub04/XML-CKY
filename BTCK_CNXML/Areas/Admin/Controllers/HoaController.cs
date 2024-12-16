using BTCK_CNXML.Data;
using BTCK_CNXML.Models;
using Microsoft.AspNetCore.Mvc;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Xml;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc.Rendering;
using BTCK_CNXML.Areas.Admin.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace BTCK_CNXML.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class HoaController : Controller
    {
        private AppDbContext _context;

        public HoaController(AppDbContext context)
        {
            _context = context;
        }

        [Route("/Admin/Hoa/Index")]
        public IActionResult Index()
        {
            return View();
        }

        [Route("/Admin/Hoa/List")]
        public IActionResult List()
        {
            string hoaXmlFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "xml", "hoa.xml");
            string loaiHoaXmlFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "xml", "loaihoa.xml");
            string xsdFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "xsd", "hoa.xsd");

            if (!System.IO.File.Exists(hoaXmlFilePath))
            {
                var hoas = _context.Hoas.ToList();

                XElement xml = new XElement("Hoas",
                    hoas.Select(r => new XElement("Hoa",
                        new XElement("Id", r.Id),
                        new XElement("Name", r.Name),
                        new XElement("Description", r.Description),
                        new XElement("Price", r.Price),
                        new XElement("StockQuantity", r.StockQuantity),
                        new XElement("LoaiHoaId", r.LoaiHoaId)
                    ))
                );

                if (ValidateXmlWithXsd(xml, xsdFilePath))
                {
                    xml.Save(hoaXmlFilePath);
                }
                else
                {
                    ModelState.AddModelError("", "XML không hợp lệ với XSD.");
                    return View("Error");
                }
            }

            if (!System.IO.File.Exists(loaiHoaXmlFilePath))
            {
                ModelState.AddModelError("", "File XML loaihoa.xml không tồn tại.");
                return View("Error");
            }

            XElement hoaXml = XElement.Load(hoaXmlFilePath);
            XElement loaiHoaXml = XElement.Load(loaiHoaXmlFilePath);

            var loaiHoas = loaiHoaXml.Elements("LoaiHoa").ToDictionary(
                x => (string)x.Element("Id"),
                x => (string)x.Element("Name")
            );

            var hoasFromXml = hoaXml.Elements("Hoa").Select(x => new HoaVM
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

        [Route("/Admin/Hoa/Create")]
public IActionResult Create()
{
    string loaiHoaXmlFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "xml", "loaihoa.xml");

    if (!System.IO.File.Exists(loaiHoaXmlFilePath))
    {
        ModelState.AddModelError("", "File XML loaihoa.xml không tồn tại.");
        return View(new HoaCreateVM());
    }

    XElement loaiHoaXml = XElement.Load(loaiHoaXmlFilePath);
    var loaiHoas = loaiHoaXml.Elements("LoaiHoa").Select(x => new SelectListItem
    {
        Value = (string)x.Element("Id"),
        Text = (string)x.Element("Name")
    }).ToList();

    ViewBag.LoaiHoas = loaiHoas;
    return View(new HoaCreateVM());
}

[HttpPost]
[Route("/Admin/Hoa/Create")]
public async Task<IActionResult> Create(HoaCreateVM model)
{
    if (ModelState.IsValid)
    {
        // Đường dẫn tới file XML và XSD
        string xmlFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "xml", "hoa.xml");
        string xsdFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "xsd", "hoa.xsd");

        // Đảm bảo file XML tồn tại
        if (!System.IO.File.Exists(xmlFilePath))
        {
            ModelState.AddModelError("", "File XML không tồn tại.");
            return View(model);
        }

        // Tải file XML hiện tại
        XElement xml = XElement.Load(xmlFilePath);

        // Khởi tạo Hoa từ model
        var hoa = new Hoa
        {
            Id = Guid.NewGuid().ToString(),
            Name = model.Name,
            Description = model.Description,
            Price = model.Price,
            StockQuantity = model.StockQuantity,
            LoaiHoaId = model.LoaiHoaId
        };

        // Xử lý file upload
        List<XElement> imageElements = new List<XElement>();
        if (model.Images != null && model.Images.Length > 0)
        {
            hoa.Images = new List<Image>();
            foreach (var image in model.Images)
            {
                var fileName = DateTime.Now.Ticks.ToString() + Path.GetExtension(image.FileName);
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "upload", fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await image.CopyToAsync(stream);
                }

                var imageUrl = "/upload/" + fileName;
                hoa.Images.Add(new Image
                {
                    Id = Guid.NewGuid().ToString(),
                    Url = imageUrl,
                    HoaId = hoa.Id
                });

                imageElements.Add(new XElement("Image",
                    new XElement("Id", Guid.NewGuid().ToString()),
                    new XElement("Url", imageUrl)
                ));
            }
        }

        try
        {
            // Tạo phần tử mới từ hoa
            XElement newHoa = new XElement("Hoa",
                new XElement("Id", hoa.Id),
                new XElement("Name", hoa.Name ?? string.Empty),
                new XElement("Description", hoa.Description ?? string.Empty),
                new XElement("Price", hoa.Price),
                new XElement("StockQuantity", hoa.StockQuantity),
                new XElement("LoaiHoaId", hoa.LoaiHoaId),
                new XElement("Images", imageElements)
            );

            // Thêm phần tử mới vào XML
            xml.Add(newHoa);

            // Kiểm tra tính hợp lệ của file XML với XSD
            if (ValidateXmlWithXsd(xml, xsdFilePath))
            {
                // Nếu hợp lệ, lưu file XML
                xml.Save(xmlFilePath);
                TempData["SuccessMessage"] = "Thêm hoa thành công!";
                return RedirectToAction("Index");
            }
            else
            {
                ModelState.AddModelError("", "File XML không hợp lệ sau khi thêm hoa.");
            }
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", $"Đã xảy ra lỗi: {ex.Message}");
        }
    }

    // Reload LoaiHoas in case of error
    string loaiHoaXmlFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "xml", "loaihoa.xml");
    XElement loaiHoaXml = XElement.Load(loaiHoaXmlFilePath);
    var loaiHoas = loaiHoaXml.Elements("LoaiHoa").Select(x => new SelectListItem
    {
        Value = (string)x.Element("Id"),
        Text = (string)x.Element("Name")
    }).ToList();
    ViewBag.LoaiHoas = loaiHoas;

    return View(model);
}

        [Route("/Admin/Hoa/Details/{id}")]
        public IActionResult Details(string id)
        {
            string xmlFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "xml", "hoa.xml");
            string loaiHoaXmlFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "xml", "loaihoa.xml");

            if (!System.IO.File.Exists(xmlFilePath) || !System.IO.File.Exists(loaiHoaXmlFilePath))
            {
                ModelState.AddModelError("", "File XML không tồn tại.");
                return RedirectToAction("Index");
            }

            XElement xml = XElement.Load(xmlFilePath);
            XElement loaiHoaXml = XElement.Load(loaiHoaXmlFilePath);

            var hoaElement = xml.Elements("Hoa").FirstOrDefault(x => (string)x.Element("Id") == id);
            if (hoaElement == null)
            {
                ModelState.AddModelError("", "Không tìm thấy hoa.");
                return RedirectToAction("Index");
            }

            var loaiHoas = loaiHoaXml.Elements("LoaiHoa").ToDictionary(
                x => (string)x.Element("Id"),
                x => new LoaiHoa
                {
                    Id = (string)x.Element("Id"),
                    Name = (string)x.Element("Name"),
                    Description = (string)x.Element("Description")
                }
            );

            var hoa = new Hoa
            {
                Id = (string)hoaElement.Element("Id"),
                Name = (string)hoaElement.Element("Name"),
                Description = (string)hoaElement.Element("Description"),
                Price = (decimal)hoaElement.Element("Price"),
                StockQuantity = (int)hoaElement.Element("StockQuantity"),
                LoaiHoaId = (string)hoaElement.Element("LoaiHoaId"),
                loaiHoa = loaiHoas.ContainsKey((string)hoaElement.Element("LoaiHoaId")) ? loaiHoas[(string)hoaElement.Element("LoaiHoaId")] : null,
                Images = hoaElement.Element("Images")?.Elements("Image").Select(x => new Image
                {
                    Id = (string)x.Element("Id"),
                    Url = (string)x.Element("Url"),
                    HoaId = (string)x.Element("HoaId")
                }).ToList() ?? new List<Image>()
            };

            return View(hoa);
        }
        [Route("/Admin/Hoa/Edit/{id}")]
        public IActionResult Edit(string id)
        {
            string hoaXmlFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "xml", "hoa.xml");
            string loaiHoaXmlFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "xml", "loaihoa.xml");

            if (!System.IO.File.Exists(hoaXmlFilePath) || !System.IO.File.Exists(loaiHoaXmlFilePath))
            {
                ModelState.AddModelError("", "File XML không tồn tại.");
                return RedirectToAction("Index");
            }

            XElement hoaXml = XElement.Load(hoaXmlFilePath);
            XElement loaiHoaXml = XElement.Load(loaiHoaXmlFilePath);

            var hoaElement = hoaXml.Elements("Hoa").FirstOrDefault(x => (string)x.Element("Id") == id);
            if (hoaElement == null)
            {
                ModelState.AddModelError("", "Không tìm thấy hoa.");
                return RedirectToAction("Index");
            }

            var model = new HoaCreateVM
            {
                Name = (string)hoaElement.Element("Name"),
                Description = (string)hoaElement.Element("Description"),
                Price = (decimal)hoaElement.Element("Price"),
                StockQuantity = (int)hoaElement.Element("StockQuantity"),
                LoaiHoaId = (string)hoaElement.Element("LoaiHoaId")
            };

            var loaiHoas = loaiHoaXml.Elements("LoaiHoa").Select(x => new SelectListItem
            {
                Value = (string)x.Element("Id"),
                Text = (string)x.Element("Name")
            }).ToList();

            ViewBag.LoaiHoas = loaiHoas;
            ViewBag.HoaId = id;

            return View(model);
        }

        [HttpPost]
        [Route("/Admin/Hoa/Edit/{id}")]
        public async Task<IActionResult> Edit(string id, HoaCreateVM model)
        {
            if (ModelState.IsValid)
            {
                string xmlFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "xml", "hoa.xml");
                string xsdFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "xsd", "hoa.xsd");

                if (!System.IO.File.Exists(xmlFilePath))
                {
                    ModelState.AddModelError("", "File XML không tồn tại.");
                    return View(model);
                }

                XElement xml = XElement.Load(xmlFilePath);
                var hoaElement = xml.Elements("Hoa").FirstOrDefault(x => (string)x.Element("Id") == id);

                if (hoaElement == null)
                {
                    ModelState.AddModelError("", "Không tìm thấy hoa.");
                    return View(model);
                }

                hoaElement.SetElementValue("Name", model.Name);
                hoaElement.SetElementValue("Description", model.Description);
                hoaElement.SetElementValue("Price", model.Price);
                hoaElement.SetElementValue("StockQuantity", model.StockQuantity);
                hoaElement.SetElementValue("LoaiHoaId", model.LoaiHoaId);

                if (model.Images != null && model.Images.Length > 0)
                {
                    List<XElement> imageElements = new List<XElement>();
                    foreach (var image in model.Images)
                    {
                        var fileName = DateTime.Now.Ticks.ToString() + Path.GetExtension(image.FileName);
                        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "upload", fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await image.CopyToAsync(stream);
                        }

                        var imageUrl = "/upload/" + fileName;
                        imageElements.Add(new XElement("Image",
                            new XElement("Id", Guid.NewGuid().ToString()),
                            new XElement("Url", imageUrl)
                        ));
                    }
                    hoaElement.Element("Images")?.Remove();
                    hoaElement.Add(new XElement("Images", imageElements));
                }

                if (ValidateXmlWithXsd(xml, xsdFilePath))
                {
                    xml.Save(xmlFilePath);
                    TempData["SuccessMessage"] = "Cập nhật hoa thành công!";
                    return RedirectToAction("Index");
                }
                else
                {
                    ModelState.AddModelError("", "File XML không hợp lệ sau khi cập nhật hoa.");
                }
            }

            string loaiHoaXmlFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "xml", "loaihoa.xml");
            XElement loaiHoaXml = XElement.Load(loaiHoaXmlFilePath);
            var loaiHoas = loaiHoaXml.Elements("LoaiHoa").Select(x => new SelectListItem
            {
                Value = (string)x.Element("Id"),
                Text = (string)x.Element("Name")
            }).ToList();
            ViewBag.LoaiHoas = loaiHoas;
            ViewBag.HoaId = id;

            return View(model);
        }


        [Route("/Admin/Hoa/Delete/{id}")]
        [HttpPost]
        public IActionResult Delete(string id)
        {
            string xmlFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "xml", "hoa.xml");

            if (!System.IO.File.Exists(xmlFilePath))
            {
                return Json(new { success = false, message = "File XML không tồn tại." });
            }

            XElement xml = XElement.Load(xmlFilePath);
            var hoaElement = xml.Elements("Hoa")
                                .FirstOrDefault(x => (string)x.Element("Id") == id);

            if (hoaElement == null)
            {
                return Json(new { success = false, message = "Không tìm thấy hoa." });
            }

            hoaElement.Remove();

            string xsdFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "xsd", "hoa.xsd");
            if (ValidateXmlWithXsd(xml, xsdFilePath))
            {
                xml.Save(xmlFilePath);
                return Json(new { success = true, message = "Xóa hoa thành công!" });
            }
            else
            {
                return Json(new { success = false, message = "File XML không hợp lệ sau khi xóa hoa." });
            }
        }

        [Route("/Admin/Hoa/UpdateDatabase")]
        public ActionResult UpdateDatabase()
        {
            string xmlFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "xml", "hoa.xml");

            if (!System.IO.File.Exists(xmlFilePath))
            {
                ModelState.AddModelError("", "File XML không tồn tại.");
                return RedirectToAction("Index");
            }

            XElement xml = XElement.Load(xmlFilePath);
            var hoas = xml.Elements("Hoa").Select(x => new Hoa
            {
                Id = (string)x.Element("Id"),
                Name = (string)x.Element("Name"),
                Description = (string)x.Element("Description"),
                Price = (decimal)x.Element("Price"),
                StockQuantity = (int)x.Element("StockQuantity"),
                LoaiHoaId = (string)x.Element("LoaiHoaId"),
                Images = x.Element("Images")?.Elements("Image").Select(img => new Image
                {
                    Id = (string)img.Element("Id"),
                    Url = (string)img.Element("Url"),
                    HoaId = (string)x.Element("Id") // Ensure HoaId is set correctly
                }).ToList() ?? new List<Image>()
            }).ToList();

            var dbHoas = _context.Hoas.ToList();
            var dbImages = _context.Images.ToList();

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

                    // Update images
                    var existingImages = dbImages.Where(img => img.HoaId == hoa.Id).ToList();
                    _context.Images.RemoveRange(existingImages);
                    _context.Images.AddRange(hoa.Images);
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
                        LoaiHoaId = hoa.LoaiHoaId,
                        Images = hoa.Images
                    });
                }
            }

            var idsFromXml = hoas.Select(r => r.Id).ToList();
            var hoasToRemove = dbHoas.Where(r => !idsFromXml.Contains(r.Id.ToString())).ToList();

            foreach (var hoa in hoasToRemove)
            {
                var imagesToRemove = dbImages.Where(img => img.HoaId == hoa.Id).ToList();
                _context.Images.RemoveRange(imagesToRemove);
                _context.Hoas.Remove(hoa);
            }

            _context.SaveChanges();

            TempData["SuccessMessage"] = "Cập nhật dữ liệu thành công từ XML vào database!";
            return RedirectToAction("Index");
        }

        [Route("/Admin/Hoa/Reset")]
        public IActionResult Reset()
        {
            string xmlFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "xml", "hoa.xml");
            string xsdFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "xsd", "hoa.xsd");

            try
            {
                var hoas = _context.Hoas.Include(h => h.Images).ToList();

                XElement xml = new XElement("Hoas",
                    hoas.Select(r => new XElement("Hoa",
                        new XElement("Id", r.Id),
                        new XElement("Name", r.Name),
                        new XElement("Description", r.Description),
                        new XElement("Price", r.Price),
                        new XElement("StockQuantity", r.StockQuantity),
                        new XElement("LoaiHoaId", r.LoaiHoaId),
                        new XElement("Images", r.Images.Select(img => new XElement("Image",
                            new XElement("Id", img.Id),
                            new XElement("Url", img.Url),
                            new XElement("HoaId", img.HoaId)
                        )))
                    ))
                );

                if (ValidateXmlWithXsd(xml, xsdFilePath))
                {
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

        [Route("/Admin/Hoa/ExportToExcel")]
        public IActionResult ExportToExcel()
        {
            string xmlFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "xml", "hoa.xml");
            string loaiHoaXmlFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "xml", "loaihoa.xml");

            if (!System.IO.File.Exists(xmlFilePath) || !System.IO.File.Exists(loaiHoaXmlFilePath))
            {
                ModelState.AddModelError("", "File XML không tồn tại.");
                return RedirectToAction("Index");
            }

            try
            {
                XElement hoaXml = XElement.Load(xmlFilePath);
                XElement loaiHoaXml = XElement.Load(loaiHoaXmlFilePath);

                var loaiHoas = loaiHoaXml.Elements("LoaiHoa").ToDictionary(
                    x => (string)x.Element("Id"),
                    x => (string)x.Element("Name")
                );

                var hoas = hoaXml.Elements("Hoa").Select(x => new
                {
                    Id = (string)x.Element("Id"),
                    Name = (string)x.Element("Name"),
                    Description = (string)x.Element("Description"),
                    Price = (decimal)x.Element("Price"),
                    StockQuantity = (int)x.Element("StockQuantity"),
                    LoaiHoaName = loaiHoas.ContainsKey((string)x.Element("LoaiHoaId")) ? loaiHoas[(string)x.Element("LoaiHoaId")] : null
                }).ToList();

                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("Hoas");
                    var currentRow = 1;

                    worksheet.Cell(currentRow, 1).Value = "Id";
                    worksheet.Cell(currentRow, 2).Value = "Tên";
                    worksheet.Cell(currentRow, 3).Value = "Mô tả";
                    worksheet.Cell(currentRow, 4).Value = "Giá";
                    worksheet.Cell(currentRow, 5).Value = "Số lượng tồn kho";
                    worksheet.Cell(currentRow, 6).Value = "Loại hoa";

                    foreach (var hoa in hoas)
                    {
                        currentRow++;
                        worksheet.Cell(currentRow, 1).Value = hoa.Id;
                        worksheet.Cell(currentRow, 2).Value = hoa.Name;
                        worksheet.Cell(currentRow, 3).Value = hoa.Description;
                        worksheet.Cell(currentRow, 4).Value = hoa.Price;
                        worksheet.Cell(currentRow, 5).Value = hoa.StockQuantity;
                        worksheet.Cell(currentRow, 6).Value = hoa.LoaiHoaName;
                    }

                    using (var stream = new MemoryStream())
                    {
                        workbook.SaveAs(stream);
                        var content = stream.ToArray();
                        return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Hoas.xlsx");
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
