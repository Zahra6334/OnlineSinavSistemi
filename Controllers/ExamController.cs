using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OnlineSinavSistemi.Models;
using OnlineSinavSistemi.Services;
using System.Threading.Tasks;

namespace OnlineSinavSistemi.Controllers
{
    [Authorize(Roles = "Ogretmen")]
    public class ExamController : Controller
    {
        private readonly IExamService _examService;
        private readonly UserManager<ApplicationUser> _userManager;

        public ExamController(IExamService examService, UserManager<ApplicationUser> userManager)
        {
            _examService = examService;
            _userManager = userManager;
        }

        // 📝 Öğretmenin sınavlarını listele
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            var exams = await _examService.GetExamsForTeacherAsync(user.Id);
            return View(exams);
        }

        // 📝 Yeni sınav oluşturma formu (GET)
        public IActionResult Create()
        {
            return View();
        }

        // 📝 Yeni sınav oluşturma (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Exam model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.GetUserAsync(User);
            model.OgretmenId = user.Id;

            await _examService.CreateExamAsync(model);
            return RedirectToAction(nameof(Index));
        }

        // 📝 Sınav detayları (sorular + öğrenciler)
        public async Task<IActionResult> Details(int id)
        {
            var exam = await _examService.GetExamByIdAsync(id);
            if (exam == null)
                return NotFound();

            return View(exam);
        }

        // 📝 Sınav yayınla / yayından kaldır
        [HttpPost]
        public async Task<IActionResult> TogglePublish(int id)
        {
            await _examService.PublishExamAsync(id);
            return RedirectToAction(nameof(Index));
        }

        // 📝 Sınav sil
        public async Task<IActionResult> Delete(int id)
        {
            var exam = await _examService.GetExamByIdAsync(id);
            if (exam == null)
                return NotFound();

            // Burada servis üzerinden silme metodu ekleyebilirsin
            // await _examService.DeleteExamAsync(id);

            return RedirectToAction(nameof(Index));
        }

        // 📝 Öğrenci sınavını notlandır
        [HttpPost]
        public async Task<IActionResult> GradeStudentExam(StudentExam model)
        {
            if (ModelState.IsValid)
            {
                await _examService.GradeStudentExamAsync(model);
            }
            return RedirectToAction(nameof(Details), new { id = model.ExamId });
        }
    }
}
