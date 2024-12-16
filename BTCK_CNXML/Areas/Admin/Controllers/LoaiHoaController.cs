using BTCK_CNXML.Data;
using BTCK_CNXML.Models;
using Microsoft.AspNetCore.Mvc;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Xml;
using ClosedXML.Excel;
using BTCK_CNXML.Areas.Admin.ViewModels;

namespace BTCK_CNXML.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class LoaiHoaController : Controller
    {
        private AppDbContext _context;

        public LoaiHoaController(AppDbContext context)
        {
            _context = context;
        }

        [Route("/Admin/LoaiHoa/Index")]
        public IActionResult Index()
        {
            return View();
        }

        [Route("/Admin/LoaiHoa/List")]
        public IActionResult List()
        {
            string xmlFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "xml", "loaihoa.xml");
            string xsdFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "xsd", "loaihoa.xsd");

            if (!System.IO.File.Exists(xmlFilePath))
            {
                var loaiHoas = _context.LoaiHoas.ToList();

                XElement xml = new XElement("LoaiHoas",
                    loaiHoas.Select(r => new XElement("LoaiHoa",
                        new XElement("Id", r.Id),
                        new XElement("Name", r.Name),
                        new XElement("Description", r.Description)
                    ))
                );

                if (ValidateXmlWithXsd(xml, xsdFilePath))
                {
                    xml.Save(xmlFilePath);
                }
                else
                {
                    ModelState.AddModelError("", "XML không hợp lệ với XSD.");
                    return View("Error");
                }
            }

            XElement loadedXml = XElement.Load(xmlFilePath);
            var loaiHoasFromXml = loadedXml.Elements("LoaiHoa").Select(x => new LoaiHoa
            {
                Id = (string)x.Element("Id"),
                Name = (string)x.Element("Name"),
                Description = (string)x.Element("Description")
            }).ToList();

            return Json(new { Data = loaiHoasFromXml });
        }

        [Route("/Admin/LoaiHoa/Create")]
        public ActionResult Create()
        {
            return View(new LoaiHoaVM());
        }

        [HttpPost]
        [Route("/Admin/LoaiHoa/Create")]
        public IActionResult Create(LoaiHoaVM model)
        {
            if (ModelState.IsValid)
            {
                string xmlFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "xml", "loaihoa.xml");
                string xsdFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "xsd", "loaihoa.xsd");

                if (!System.IO.File.Exists(xmlFilePath))
                {
                    ModelState.AddModelError("", "File XML không tồn tại.");
                    return View(model);
                }

                XElement xml = XElement.Load(xmlFilePath);

                var existingLoaiHoa = xml.Elements("LoaiHoa")
                                         .FirstOrDefault(x => (string)x.Element("Name") == model.Name);
                if (existingLoaiHoa != null)
                {
                    ModelState.AddModelError("Name", "Tên loại hoa đã tồn tại.");
                    return View(model);
                }

                XElement newLoaiHoa = new XElement("LoaiHoa",
                    new XElement("Id", Guid.NewGuid().ToString()),
                    new XElement("Name", model.Name),
                    new XElement("Description", model.Description)
                );

                xml.Add(newLoaiHoa);

                if (ValidateXmlWithXsd(xml, xsdFilePath))
                {
                    xml.Save(xmlFilePath);
                    TempData["SuccessMessage"] = "Thêm loại hoa thành công!";
                    return RedirectToAction("Index");
                }
                else
                {
                    ModelState.AddModelError("", "File XML không hợp lệ sau khi thêm loại hoa.");
                }
            }

            return View(model);
        }

        [Route("/Admin/LoaiHoa/Details/{id}")]
        public IActionResult Details(string id)
        {
            string xmlFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "xml", "loaihoa.xml");

            if (!System.IO.File.Exists(xmlFilePath))
            {
                ModelState.AddModelError("", "File XML không tồn tại.");
                return RedirectToAction("Index");
            }

            XElement xml = XElement.Load(xmlFilePath);
            var loaiHoaElement = xml.Elements("LoaiHoa")
                                    .FirstOrDefault(x => (string)x.Element("Id") == id);

            if (loaiHoaElement == null)
            {
                ModelState.AddModelError("", "Không tìm thấy loại hoa.");
                return RedirectToAction("Index");
            }

            LoaiHoa loaiHoa = new LoaiHoa
            {
                Id = (string)loaiHoaElement.Element("Id"),
                Name = (string)loaiHoaElement.Element("Name"),
                Description = (string)loaiHoaElement.Element("Description")
            };

            return View(loaiHoa);
        }

        [Route("/Admin/LoaiHoa/Edit/{id}")]
        public IActionResult Edit(string id)
        {
            string xmlFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "xml", "loaihoa.xml");

            if (!System.IO.File.Exists(xmlFilePath))
            {
                ModelState.AddModelError("", "File XML không tồn tại.");
                return RedirectToAction("Index");
            }

            XElement xml = XElement.Load(xmlFilePath);
            var loaiHoaElement = xml.Elements("LoaiHoa")
                                    .FirstOrDefault(x => (string)x.Element("Id") == id);

            if (loaiHoaElement == null)
            {
                ModelState.AddModelError("", "Không tìm thấy loại hoa.");
                return RedirectToAction("Index");
            }

            LoaiHoa loaiHoa = new LoaiHoa
            {
                Id = (string)loaiHoaElement.Element("Id"),
                Name = (string)loaiHoaElement.Element("Name"),
                Description = (string)loaiHoaElement.Element("Description")
            };

            return View(loaiHoa);
        }

        [Route("/Admin/LoaiHoa/Edit/{id}")]
        [HttpPost]
        public IActionResult Edit(string id, LoaiHoa model)
        {
            if (ModelState.IsValid)
            {
                string xmlFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "xml", "loaihoa.xml");
                string xsdFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "xsd", "loaihoa.xsd");

                if (!System.IO.File.Exists(xmlFilePath))
                {
                    ModelState.AddModelError("", "File XML không tồn tại.");
                    return View(model);
                }

                XElement xml = XElement.Load(xmlFilePath);
                var loaiHoaElement = xml.Elements("LoaiHoa")
                                        .FirstOrDefault(x => (string)x.Element("Id") == id);

                if (loaiHoaElement == null)
                {
                    ModelState.AddModelError("", "Không tìm thấy loại hoa.");
                    return View(model);
                }

                loaiHoaElement.Element("Name").Value = model.Name;
                loaiHoaElement.Element("Description").Value = model.Description;

                if (ValidateXmlWithXsd(xml, xsdFilePath))
                {
                    xml.Save(xmlFilePath);
                    TempData["SuccessMessage"] = "Chỉnh sửa loại hoa thành công!";
                    return RedirectToAction("Index");
                }
                else
                {
                    ModelState.AddModelError("", "File XML không hợp lệ sau khi chỉnh sửa loại hoa.");
                }
            }

            return View(model);
        }

        [Route("/Admin/LoaiHoa/Delete/{id}")]
        [HttpPost]
        public IActionResult Delete(string id)
        {
            string xmlFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "xml", "loaihoa.xml");

            if (!System.IO.File.Exists(xmlFilePath))
            {
                return Json(new { success = false, message = "File XML không tồn tại." });
            }

            try
            {
                XElement xml = XElement.Load(xmlFilePath);
                var loaiHoaElement = xml.Elements("LoaiHoa")
                                         .FirstOrDefault(x => (string)x.Element("Id") == id);

                if (loaiHoaElement == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy loại hoa." });
                }

                loaiHoaElement.Remove();

                string xsdFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "xsd", "loaihoa.xsd");
                if (ValidateXmlWithXsd(xml, xsdFilePath))
                {
                    xml.Save(xmlFilePath);
                    return Json(new { success = true, message = "Xóa loại hoa thành công!" });
                }
                else
                {
                    return Json(new { success = false, message = "File XML không hợp lệ sau khi xóa loại hoa." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Đã xảy ra lỗi: {ex.Message}" });
            }
        }

        [Route("/Admin/LoaiHoa/UpdateDatabase")]
        public ActionResult UpdateDatabase()
        {
            string xmlFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "xml", "loaihoa.xml");

            if (!System.IO.File.Exists(xmlFilePath))
            {
                ModelState.AddModelError("", "File XML không tồn tại.");
                return RedirectToAction("Index");
            }

            XElement xml = XElement.Load(xmlFilePath);
            var loaiHoas = xml.Elements("LoaiHoa").Select(x => new LoaiHoa
            {
                Id = (string)x.Element("Id"),
                Name = (string)x.Element("Name"),
                Description = (string)x.Element("Description")
            }).ToList();

            var dbLoaiHoas = _context.LoaiHoas.ToList();

            foreach (var loaiHoa in loaiHoas)
            {
                var existingLoaiHoa = dbLoaiHoas.FirstOrDefault(r => r.Id.ToString() == loaiHoa.Id);

                if (existingLoaiHoa != null)
                {
                    existingLoaiHoa.Name = loaiHoa.Name;
                    existingLoaiHoa.Description = loaiHoa.Description;
                }
                else
                {
                    _context.LoaiHoas.Add(new LoaiHoa
                    {
                        Id = loaiHoa.Id,
                        Name = loaiHoa.Name,
                        Description = loaiHoa.Description
                    });
                }
            }

            var idsFromXml = loaiHoas.Select(r => r.Id).ToList();
            var loaiHoasToRemove = dbLoaiHoas.Where(r => !idsFromXml.Contains(r.Id.ToString())).ToList();

            foreach (var loaiHoa in loaiHoasToRemove)
            {
                _context.LoaiHoas.Remove(loaiHoa);
            }

            _context.SaveChanges();

            TempData["SuccessMessage"] = "Cập nhật dữ liệu thành công từ XML vào database!";
            return RedirectToAction("Index");
        }

        [Route("/Admin/LoaiHoa/Reset")]
        public IActionResult Reset()
        {
            string xmlFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "xml", "loaihoa.xml");
            string xsdFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "xsd", "loaihoa.xsd");

            try
            {
                var loaiHoas = _context.LoaiHoas.ToList();

                XElement xml = new XElement("LoaiHoas",
                    loaiHoas.Select(r => new XElement("LoaiHoa",
                        new XElement("Id", r.Id),
                        new XElement("Name", r.Name),
                        new XElement("Description", r.Description)
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

        [Route("/Admin/LoaiHoa/ExportToExcel")]
        public IActionResult ExportToExcel()
        {
            string xmlFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "xml", "loaihoa.xml");

            if (!System.IO.File.Exists(xmlFilePath))
            {
                ModelState.AddModelError("", "File XML không tồn tại.");
                return RedirectToAction("Index");
            }

            try
            {
                XElement xml = XElement.Load(xmlFilePath);
                var loaiHoas = xml.Elements("LoaiHoa").Select(x => new LoaiHoa
                {
                    Id = (string)x.Element("Id"),
                    Name = (string)x.Element("Name"),
                    Description = (string)x.Element("Description")
                }).ToList();

                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("LoaiHoas");
                    var currentRow = 1;

                    worksheet.Cell(currentRow, 1).Value = "Id";
                    worksheet.Cell(currentRow, 2).Value = "Tên";
                    worksheet.Cell(currentRow, 3).Value = "Mô tả";

                    foreach (var loaiHoa in loaiHoas)
                    {
                        currentRow++;
                        worksheet.Cell(currentRow, 1).Value = loaiHoa.Id;
                        worksheet.Cell(currentRow, 2).Value = loaiHoa.Name;
                        worksheet.Cell(currentRow, 3).Value = loaiHoa.Description;
                    }

                    using (var stream = new MemoryStream())
                    {
                        workbook.SaveAs(stream);
                        var content = stream.ToArray();
                        return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "LoaiHoas.xlsx");
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