using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using OnlineSinavSistemi.Data;
using OnlineSinavSistemi.Models;
using OnlineSinavSistemi.Services;
using System.Threading.Tasks;

namespace OnlineSinavSistemi.Controllers
{
    [Authorize]
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

        // 🟢 Sınav listesi

        public async Task<IActionResult> Index(int? page)
        {
            int pageSize = 6; // Her sayfada gösterilecek kart sayısı
            int pageNumber = page ?? 1;

            var exams = _db.Exams
                .Include(e => e.Course)
                .OrderByDescending(e => e.StartDate);

            var totalCount = await exams.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var pagedExams = await exams
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.PageNumber = pageNumber;
            ViewBag.TotalPages = totalPages;
            ViewBag.HasPreviousPage = pageNumber > 1;
            ViewBag.HasNextPage = pageNumber < totalPages;

            return View(pagedExams);
        }
        

        // 🟢 Yeni sınav oluştur (GET)
        public IActionResult Create()
        {
            var userId = _userManager.GetUserId(User);
            var dersler = _db.Courses.Where(c => c.TeacherId == userId).ToList();
            ViewBag.Dersler = new SelectList(dersler, "Id", "CourseName");

            var model = new Exam
            {
                StartDate = DateTime.Now // default değer
            };

            return View(model);
        }



        // 🟢 ExamController > Create (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Exam model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            // 🔴 SİHİRLİ KISIM: Gitmemesini sağlayan engelleri kaldırıyoruz.
            // "StudentExams" hatası yüzünden gitmiyordu, artık gidecek.
            ModelState.Remove("StudentExams");
            ModelState.Remove("Questions");
            ModelState.Remove("TeacherId");
            ModelState.Remove("Teacher");
            ModelState.Remove("Course");

            // Validasyon (Ders seçilmemişse uyar)
            if (model.CourseId == 0) ModelState.AddModelError("CourseId", "Lütfen bir ders seçin.");

            if (ModelState.IsValid)
            {
                // 1. Öğretmen ve Tarih bilgisini ekle
                model.TeacherId = user.Id;
                if (model.StartDate == default) model.StartDate = DateTime.Now;

                // 2. Veritabanına Kaydet (Sınav ID'si burada oluşur)
                _db.Exams.Add(model);
                await _db.SaveChangesAsync();

                // 3. 🚀 YÖNLENDİRME: İşlem bitti, Soru Ekleme sayfasına git!
                return RedirectToAction("Create", "Question", new { examId = model.Id });
            }

            // Hata varsa sayfayı yenile (Gitme)
            var dersler = _db.Courses.Where(c => c.TeacherId == user.Id).ToList();
            ViewBag.Dersler = new SelectList(dersler, "Id", "CourseName");
            return View(model);
        }


        // 🟢 Sınav detayları (öğretmen)
        public async Task<IActionResult> Details(int id)
        {
            var exam = await _db.Exams
                .Include(e => e.Course)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (exam == null)
            {
                Console.WriteLine("❌ Exam bulunamadı. Id: " + id);
                return NotFound();
            }

            Console.WriteLine($"✅ Exam bulundu → Id: {exam.Id}, CourseId: {exam.CourseId}");

            // ✅ StudentExam tablosundan çek
            var studentExams = await _examService.GetStudentExamsForExamAsync(id);

            // 🔍 NULL / BOŞ KONTROLÜ
            if (studentExams == null)
            {
                Console.WriteLine("❌ studentExams NULL geliyor!");
            }
            else if (!studentExams.Any())
            {
                Console.WriteLine("⚠ studentExams boş (kayıt yok). ExamId: " + id);
            }
            else
            {
                Console.WriteLine($"✅ studentExams dolu → Toplam {studentExams.Count} kayıt");

                foreach (var se in studentExams)
                {
                    Console.WriteLine(
                        $"➡ StudentExamId: {se.Id}, " +
                        $"ExamId: {se.ExamId}, " +
                        $"StudentId: {se.StudentId}, " +
                        $"StudentName: {se.Student?.full_name}, " +
                        $"Score: {se.Score}, " +
                        $"Completed: {se.Completed}"
                    );
                }
            }

            ViewBag.StudentExams = studentExams;

            return View(exam);
        }




        // 🟢 Sınav düzenle (GET)
        public async Task<IActionResult> Edit(int id)
        {
            var exam = await _examService.GetExamByIdAsync(id);
            if (exam == null) return NotFound();

            ViewBag.Dersler = new SelectList(_db.Courses.Where(c => c.TeacherId == exam.TeacherId).ToList(), "Id", "CourseName", exam.CourseId);
            return View(exam);
        }

        // 🟢 Sınav düzenle (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Exam model)
        {
            if (!ModelState.IsValid) return View(model);

            var exam = await _examService.GetExamByIdAsync(model.Id);
            if (exam == null) return NotFound();

            exam.Title= model.Title;
            exam.CourseId = model.CourseId;
            exam.DurationMinutes = model.DurationMinutes;
            exam.StartDate= model.StartDate;

            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        // 🟢 Sınav sil (GET)
        public async Task<IActionResult> Delete(int id)
        {
            var exam = await _examService.GetExamByIdAsync(id);
            if (exam == null) return NotFound();

            return View(exam);
        }

        // 🟢 Sınav sil (POST)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var exam = await _examService.GetExamByIdAsync(id);
            if (exam == null) return NotFound();

            _db.Exams.Remove(exam);
            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        // 🟢 Sınav yayınla / yayından kaldır
        [HttpPost]
        public async Task<IActionResult> TogglePublish(int id)
        {
            await _examService.PublishExamAsync(id);
            return RedirectToAction("Details", new { id });
        }
    }
}
