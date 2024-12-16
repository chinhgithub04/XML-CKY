using Microsoft.AspNetCore.Mvc;
using BTCK_CNXML.ViewModels;
using BTCK_CNXML.Data;
using BTCK_CNXML.Models;
using Microsoft.EntityFrameworkCore;
using System.Xml.Linq;

namespace BTCK_CNXML.Controllers
{
    public class AccountController : Controller
    {
        private AppDbContext _context;

        public AccountController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginVM model)
        {
            if (ModelState.IsValid)
            {
                // Check user credentials from the database
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == model.Email && u.Password == model.Password);

                if (user != null)
                {
                    // Store UserId and Role in session
                    HttpContext.Session.SetString("UserId", user.Id);
                    HttpContext.Session.SetString("Role", user.Role);

                    if (user.Role == "Admin")
                    {
                        return RedirectToAction("Index", "Home", new { area = "Admin" });
                    }
                    else if (user.Role == "Client")
                    {
                        return RedirectToAction("Index", "Home", new { area = "Client" });
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Invalid login attempt.");
                }
            }

            return View(model);
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home", new { area = "" });
        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterVM model)
        {
            if (!ModelState.IsValid)
            {
                // If model is invalid, return the View with the model to display errors
                return View(model);
            }

            // Check if the email already exists in the database
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
            if (existingUser != null)
            {
                ModelState.AddModelError("Email", "This email is already in use.");
                return View(model);
            }

            // Create a new user object from RegisterVM
            var user = new User
            {
                Id = DateTime.Now.Ticks.ToString(),
                Name = model.Name,
                Email = model.Email,
                PhoneNumber = model.PhoneNumber,
                Password = model.Password,
                Role = "Client"
            };

            // Add the new user to the database
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Send success message
            TempData["Message"] = "Registration successful!";

            // Redirect to the Login page
            return RedirectToAction("Index", "Home", new { area = "Client" });
        }
    }
}