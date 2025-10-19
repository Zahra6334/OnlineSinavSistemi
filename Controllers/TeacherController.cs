using Microsoft.AspNetCore.Mvc;
using OnlineSinavSistemi.Models;
using OnlineSinavSistemi.Data;
using System.Linq;

namespace OnlineSinavSistemi.Controllers
{
    public class TeacherController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TeacherController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Ders listesi
        public IActionResult Derslerim()
        {
            // Şimdilik tüm dersleri getiriyoruz (ileride öğretmene özel yapılacak)
            var dersler = _context.Courses.ToList();
            return View(dersler);
        }

        // Sınav oluşturma formu
        [HttpGet]
        public IActionResult SinavOlustur()
        {
            ViewBag.Dersler = _context.Courses.ToList();
            return View();
        }

        // Sınav kaydetme işlemi
        [HttpPost]
        public IActionResult SinavOlustur(Exam sinav)
        {
            if (ModelState.IsValid)
            {
                _context.Exams.Add(sinav);
                _context.SaveChanges();
                return RedirectToAction("SinavListesi");
            }

            ViewBag.Dersler = _context.Courses.ToList();
            return View(sinav);
        }

        // Sınavları listeleme
        public IActionResult SinavListesi()
        {
            var sinavlar = _context.Exams.ToList();
            return View(sinavlar);
        }

        // Öğrencilerin sınava giriş durumunu görmek
        public IActionResult SinavaGirenler(int id)
        {
            var katilimlar = _context.StudentExams
                .Where(x => x.ExamId == id)
                .ToList();

            return View(katilimlar);
        }
    }
}
