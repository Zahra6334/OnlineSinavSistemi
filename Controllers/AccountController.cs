using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using OnlineSinavSistemi.Models;

public class AccountController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IWebHostEnvironment _env;
    public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IWebHostEnvironment env)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _env = env;
    }

    // Kullanıcı Menüsü Partial
    public IActionResult UserMenu()
    {
        if (!_signInManager.IsSignedIn(User))
            return new EmptyResult();  // kullanıcı login değilse boş dön

        return PartialView("_UserMenu");
    }
    [HttpPost]
    public async Task<IActionResult> ProfilResmiYukle(IFormFile profilResmi)
    {
        if (profilResmi != null && profilResmi.Length > 0)
        {
            // Giriş yapmış kullanıcıyı al
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                // Klasör yolu
                var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads/profile");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                // Benzersiz dosya adı
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(profilResmi.FileName);
                var filePath = Path.Combine(uploadsFolder, fileName);

                // Dosyayı kaydet
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await profilResmi.CopyToAsync(stream);
                }

                // Kullanıcının profil resmini güncelle
                user.ProfileImage = "/uploads/profile/" + fileName;
                await _userManager.UpdateAsync(user);
            }
        }

        // Sayfayı geri yönlendir
        return Redirect(Request.Headers["Referer"].ToString());
    }

    [HttpPost]
    [HttpPost]
    public async Task<IActionResult> ProfilResmiDegistir(IFormFile profilResmi)
    {
        if (profilResmi != null && profilResmi.Length > 0)
        {
            var user = await _userManager.GetUserAsync(User);
            var dosyaAdi = Guid.NewGuid().ToString() + Path.GetExtension(profilResmi.FileName);
            var kayitYolu = Path.Combine(_env.WebRootPath, "uploads/profile", dosyaAdi);

            // Klasör yoksa oluştur
            Directory.CreateDirectory(Path.GetDirectoryName(kayitYolu));

            using (var stream = new FileStream(kayitYolu, FileMode.Create))
            {
                await profilResmi.CopyToAsync(stream);
            }

            user.ProfileImage = "/uploads/profile/" + dosyaAdi;
            await _userManager.UpdateAsync(user);
        }

        return Ok();
    }


}
