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
    [Authorize] // Login olmuş kullanıcılar
    public class CourseController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;


    public CourseController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // 📘 Öğretmenin tüm dersleri
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user.Rol != "Ogretmen")
                return Forbid();

            var courses = await _context.Courses
                .Where(c => c.TeacherId == user.Id)
                .Include(c => c.Exams)
                .ToListAsync();

            return View(courses);
        }

        // 📘 Ders ekleme formu (GET)
        public async Task<IActionResult> Create()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user.Rol != "Ogretmen")
                return Forbid();

            return View();
        }

        // 📘 Ders ekleme işlemi (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Course model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user.Rol != "Ogretmen")
                return Forbid();

            if (ModelState.IsValid)
            {
                // Hata var, ModelState hatalarını logla
                foreach (var state in ModelState)
                    foreach (var error in state.Value.Errors)
                        Console.WriteLine(error.ErrorMessage);

                return View(model);
            }

            model.TeacherId= user.Id;
            _context.Courses.Add(model);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

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
            var user = await _userManager.GetUserAsync(User);
            if (user.Rol != "Ogretmen")
                return Forbid();

            var course = await _context.Courses.FindAsync(id);
            if (course == null)
                return NotFound();

            _context.Courses.Remove(course);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }

}
