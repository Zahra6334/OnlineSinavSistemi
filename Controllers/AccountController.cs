using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OnlineSinavSistemi.Models;
using OnlineSinavSistemi.ViewModels;
using System.Threading.Tasks;

namespace OnlineSinavSistemi.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
        }

        // -------------------
        // Register GET
        // -------------------
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // -------------------
        // Register POST
        // -------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = new ApplicationUser { UserName = model.Email, Email = model.Email, AdSoyad = model.Name };
            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                // Rol oluşturma kontrolü
                if (!await _roleManager.RoleExistsAsync("Teacher"))
                    await _roleManager.CreateAsync(new IdentityRole("Teacher"));
                if (!await _roleManager.RoleExistsAsync("Student"))
                    await _roleManager.CreateAsync(new IdentityRole("Student"));

                // Rol atama
                await _userManager.AddToRoleAsync(user, model.Rol);

                // Giriş yap
                await _signInManager.SignInAsync(user, isPersistent: false);

                // Rolüne göre yönlendir
                if (model.Rol == "Teacher")
                    return RedirectToAction("Index", "Teacher");
                else
                    return RedirectToAction("Index", "StudentExam");
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError("", error.Description);

            return View(model);
        }

        // -------------------
        // Login GET
        // -------------------
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // -------------------
        // Login POST
        // -------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user != null)
            {
                var result = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, false);
                if (result.Succeeded)
                {
                    // Rolüne göre yönlendir
                    if (await _userManager.IsInRoleAsync(user, "Teacher"))
                        return RedirectToAction("Index", "Teacher");
                    else
                        return RedirectToAction("Index", "StudentExam");
                }
            }

            ModelState.AddModelError("", "Geçersiz giriş bilgileri.");
            return View(model);
        }

        // -------------------
        // Logout
        // -------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login", "Account");
        }
    }
}