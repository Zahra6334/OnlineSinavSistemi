using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineSinavSistemi.Data;
using OnlineSinavSistemi.Models;
using OnlineSinavSistemi.Services;
using System.Security.Claims;
using System.Threading.Tasks;

namespace OnlineSinavSistemi.Controllers
{
    [Authorize(Roles = "OGRENCI")]
    public class StudentExamController : Controller
    {
        private readonly IStudentExamService _studentExamService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;
        public StudentExamController(IStudentExamService studentExamService, UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _studentExamService = studentExamService;
            _userManager = userManager;
            _context = context;
        }

        // ---------------------------------------------------------------------
        // 🔹 STUDENT DASHBOARD (ANA SAYFA)
        // ---------------------------------------------------------------------
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("AccessDenied", "Account");

            var exams = await _studentExamService.GetExamsForStudentAsync(user.Id);
            return View(exams);
        }

        [HttpGet]
        public IActionResult TakeExam(int examId)
        {
            var studentId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var studentExam = _context.StudentExams
       .Include(se => se.Exam)
           .ThenInclude(e => e.Questions)
               .ThenInclude(q => q.Choices)
       .Include(se => se.Answers)
       .FirstOrDefault(se => se.ExamId == examId && se.StudentId == studentId);



            if (studentExam == null)
                return Unauthorized();

            // sınav ilk kez açılıyorsa başlama zamanı yaz
            if (!studentExam.StartTime.HasValue)
            {
                studentExam.StartTime = DateTime.Now;
                _context.SaveChanges();
            }


            return View(studentExam);
        }


        [HttpPost]
        public async Task<IActionResult> SubmitExam(int Id) // StudentExamId
        {
            var studentExam = await _context.StudentExams
                .Include(se => se.Exam)
                .ThenInclude(e => e.Questions)
                .ThenInclude(q => q.Choices)
                .FirstOrDefaultAsync(se => se.Id == Id);

            if (studentExam == null) return Unauthorized();

            var questions = studentExam.Exam.Questions.ToList();

            for (int i = 0; i < questions.Count; i++)
            {
                var question = questions[i];
                var answer = new Answer
                {
                    StudentExamId = studentExam.Id,
                    QuestionId = question.Id
                };

                // Çoktan seçmeli
                var selectedChoice = Request.Form[$"Answers[{i}].SelectedChoiceId"];
                if (!string.IsNullOrEmpty(selectedChoice))
                    answer.SelectedChoiceId = int.Parse(selectedChoice);

                // Klasik
                var textAnswer = Request.Form[$"Answers[{i}].AnswerText"];
                if (!string.IsNullOrEmpty(textAnswer))
                    answer.AnswerText = textAnswer;

                var file = Request.Form.Files.FirstOrDefault(f => f.Name == $"Answers[{i}].FileUpload");
                if (file != null && file.Length > 0)
                {
                    var uploads = Path.Combine("wwwroot/uploads");
                    Directory.CreateDirectory(uploads);
                    var fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
                    var path = Path.Combine(uploads, fileName);
                    using var stream = new FileStream(path, FileMode.Create);
                    await file.CopyToAsync(stream);
                    answer.FilePath = "/uploads/" + fileName;
                }

                _context.Answers.Add(answer);
            }

            studentExam.Completed = true;
            studentExam.EndTime = DateTime.Now;
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }


    }
}
