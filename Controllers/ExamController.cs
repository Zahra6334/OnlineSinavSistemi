using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using OnlineSinavSistemi.Data;
using OnlineSinavSistemi.Models;
using OnlineSinavSistemi.Services;
using System.Threading.Tasks;

namespace OnlineSinavSistemi.Controllers
{
    [Authorize] // Login kontrolü
    public class ExamController : Controller
    {
        private readonly IExamService _examService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _db;
        public ExamController(IExamService examService, UserManager<ApplicationUser> userManager, ApplicationDbContext db)
        {
            _examService = examService;
            _userManager = userManager;
            _db = db;

        }

        // Sınav listesi
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            var dersler = _db.Courses.Where(c => c.OgretmenId == userId).ToList();
            ViewBag.Dersler = new SelectList(dersler, "Id", "DersAdi");
            return View();
        }

        // Yeni sınav oluştur (GET)
        public IActionResult Create()
        {
            var userId = _userManager.GetUserId(User);

            // Öğretmenin derslerini çekiyoruz
            var dersler = _db.Courses
                             .Where(c => c.OgretmenId == userId)
                             .ToList();

            ViewBag.Dersler = new SelectList(dersler, "Id", "DersAdi");
            return View();

        }

        // Yeni sınav oluştur (POST)
        // ExamController.cs
        // ... (diğer kodlar)

        // Yeni sınav oluştur (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Exam model)
        {
            if (!ModelState.IsValid || model.CourseId == 0) // Tek bir kontrol bloğuna alalım
            {
                // **!!! BURASI YENİ EKLENEN KISIM !!!**
                var userId = _userManager.GetUserId(User);
                var dersler = _db.Courses
                                 .Where(c => c.OgretmenId == userId)
                                 .ToList();

                // ViewBag.Dersler'i yeniden dolduruyoruz
                ViewBag.Dersler = new SelectList(dersler, "Id", "DersAdi");

                if (model.CourseId == 0)
                {
                    ModelState.AddModelError("CourseId", "Lütfen bir ders seçin.");
                }

                return View(model); // View'a geri dönülüyor
            }

           

            var user = await _userManager.GetUserAsync(User);
            model.OgretmenId = user.Id;

           
            var createdExam = await _examService.CreateExamAsync(model);

            return RedirectToAction("Create", "Question", new { examId = createdExam.Id });
        }


        // Sınav detayları (soru listesi)
        public async Task<IActionResult> Details(int id)
        {
            var exam = await _examService.GetExamByIdAsync(id);
            if (exam == null)
                return NotFound();

            return View(exam);
        }
    }
}
