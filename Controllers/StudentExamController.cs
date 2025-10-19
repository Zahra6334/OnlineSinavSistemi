using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineSinavSistemi.Models;
using OnlineSinavSistemi.Services;
using System.Threading.Tasks;

namespace OnlineSinavSistemi.Controllers
{
    [Authorize(Roles = "Ogrenci")]
    public class StudentExamController : Controller
    {
        private readonly IStudentExamService _studentExamService;

        public StudentExamController(IStudentExamService studentExamService)
        {
            _studentExamService = studentExamService;
        }

        // Öğrenciye ait sınavları listele
        public async Task<IActionResult> Index()
        {
            var exams = await _studentExamService.GetExamsForStudentAsync(User.Identity.Name);
            return View(exams);
        }

        // Sınava giriş
        public async Task<IActionResult> TakeExam(int examId)
        {
            var studentExam = await _studentExamService.StartExamAsync(examId, User.Identity.Name);
            if (studentExam == null)
                return RedirectToAction("Index");

            return View(studentExam);
        }

        // Sınavı bitir ve cevapları kaydet
        [HttpPost]
        public async Task<IActionResult> SubmitExam(StudentExam model)
        {
            await _studentExamService.SubmitExamAsync(model);
            return RedirectToAction("Index");
        }
    }
}
