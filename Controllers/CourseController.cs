using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineSinavSistemi.Data;
using OnlineSinavSistemi.Models;
using System.Linq;
using System.Threading.Tasks;

namespace OnlineSinavSistemi.Controllers
{
    [Authorize(Roles = "Ogretmen")] // Sadece öğretmenler erişebilir
    public class CourseController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public CourseController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // 📘 Tüm dersleri listele (sadece giriş yapan öğretmenin)
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            var courses = await _context.Courses
                .Where(c => c.OgretmenId == user.Id)
                .Include(c => c.Exams)
                .ToListAsync();

            return View(courses);
        }

        // 📘 Ders oluşturma ekranı (GET)
        public IActionResult Create()
        {
            return View();
        }

        // 📘 Ders oluşturma işlemi (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Course model)
        {
            var user = await _userManager.GetUserAsync(User);

            if (ModelState.IsValid)
            {
                model.OgretmenId = user.Id;
                _context.Courses.Add(model);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        // 📘 Ders detay sayfası (öğrenciler + sınavlar)
        public async Task<IActionResult> Details(int id)
        {
            var course = await _context.Courses
                .Include(c => c.CourseStudents)
                    .ThenInclude(cs => cs.Student)
                .Include(c => c.Exams)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (course == null)
                return NotFound();

            return View(course);
        }

        // 📘 Ders silme
        public async Task<IActionResult> Delete(int id)
        {
            var course = await _context.Courses.FindAsync(id);
            if (course == null)
                return NotFound();

            _context.Courses.Remove(course);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}
