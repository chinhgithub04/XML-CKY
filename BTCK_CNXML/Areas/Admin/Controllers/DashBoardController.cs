using Microsoft.AspNetCore.Mvc;

namespace BTCK_CNXML.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class DashBoardController : Controller
    {
        [Route("/Admin/DashBoard/Index")]
        public IActionResult Index()
        {
            return View();
        }
    }
}
