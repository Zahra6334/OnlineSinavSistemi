using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OnlineSinavSistemi.Models; // ApplicationUser modelinizin bulunduğu yer

[Authorize] // Sadece giriş yapmış kullanıcılar erişebilir
public class AccountController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;

    public AccountController(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<IActionResult> Profile()
    {
        // Mevcut kullanıcının tüm bilgilerini çek
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound($"Kullanıcı yüklenemedi.");
        }

        // View'a kullanıcının ApplicationUser nesnesini gönderelim
        return View(user);
    }
}