using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
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
        private readonly UserManager<ApplicationUser> _userManager;

        public StudentExamController(IStudentExamService studentExamService, UserManager<ApplicationUser> userManager)
        {
            _studentExamService = studentExamService;
            _userManager = userManager;
        }

        // ---------------------------------------------------------------------
        // 🔹 STUDENT DASHBOARD (ANA SAYFA)
        // ---------------------------------------------------------------------
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User); // buradan kullanıcı Id al
            if (user == null)
                return RedirectToAction("AccessDenied", "Account");

            var exams = await _studentExamService.GetExamsForStudentAsync(user.Id); // Id kullan
            return View(exams);
        }

        public async Task<IActionResult> TakeExam(int examId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("AccessDenied", "Account");

            var studentExam = await _studentExamService.StartExamAsync(examId, user.Id); // Id kullan
            if (studentExam == null)
                return RedirectToAction("Index");

            return View(studentExam);
        }

        [HttpPost]
        public async Task<IActionResult> SubmitExam(StudentExam model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("AccessDenied", "Account");

            await _studentExamService.SubmitExamAsync(model);
            return RedirectToAction("Index");
        }
    }
}
