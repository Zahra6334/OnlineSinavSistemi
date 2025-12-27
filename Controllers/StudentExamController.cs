using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineSinavSistemi.Data;
using OnlineSinavSistemi.Models;
using OnlineSinavSistemi.Services;
using System.Security.Claims;
using System.Threading.Tasks;
using System.IO; // Eksik using
using System.Linq; // Eksik using

namespace OnlineSinavSistemi.Controllers
{
    [Authorize(Roles = "OGRENCI")]
    public class StudentExamController : Controller
    {
        private readonly IStudentExamService _studentExamService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public StudentExamController(IStudentExamService studentExamService,
                                   UserManager<ApplicationUser> userManager,
                                   ApplicationDbContext context)
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

            // Öğrencinin aldığı dersler (CourseStudent tablosu üzerinden)
            var dersler = await _context.CourseStudents
                .Where(cs => cs.StudentId == user.Id)
                .Include(cs => cs.Course)
                    .ThenInclude(c => c.Teacher)
                .Select(cs => cs.Course)
                .ToListAsync();

            return View(dersler);
        }

        // 🔹 SINAVA GİR
        [HttpGet]
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

            // Başlangıç zamanı kaydedilmemişse kaydet
            if (!studentExam.StartTime.HasValue)
            {
                studentExam.StartTime = DateTime.Now;
                _context.SaveChanges();
            }

            // Süre kontrolü
            if (studentExam.Exam.DurationMinutes > 0)
            {
                var endTime = studentExam.StartTime.Value.AddMinutes(studentExam.Exam.DurationMinutes);
                if (DateTime.Now > endTime)
                {
                    // Süre dolduysa sınavı tamamla
                    studentExam.Completed = true;
                    studentExam.EndTime = DateTime.Now;
                    _context.SaveChanges();
                    TempData["ErrorMessage"] = "Sınav süresi doldu!";
                    return RedirectToAction("Sinavlarim");
                }
            }

            return View(studentExam);
        }

        // 🔹 SINAVI GÖNDER
        [HttpPost]
        [HttpPost]
        public async Task<IActionResult> SubmitExam(int Id)
        {
            var studentId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var studentExam = await _context.StudentExams
                .Include(se => se.Exam)
                    .ThenInclude(e => e.Questions)
                        .ThenInclude(q => q.Choices)
                .FirstOrDefaultAsync(se => se.Id == Id && se.StudentId == studentId); // Burada studentId kontrolü eklendi

            if (studentExam == null) return Unauthorized();

            // 🛡️ Çift submit engeli
            if (studentExam.Completed)
                return RedirectToAction("Index");

            var questions = studentExam.Exam.Questions.ToList();

            for (int i = 0; i < questions.Count; i++)
            {
                var question = questions[i];
                var answer = new Answer
                {
                    StudentExamId = studentExam.Id,
                    QuestionId = question.Id
                };

                var selectedChoice = Request.Form[$"Answers[{i}].SelectedChoiceId"];
                if (!string.IsNullOrEmpty(selectedChoice))
                    answer.SelectedChoiceId = int.Parse(selectedChoice);

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

            // 🔹 OTOMATİK PUAN (SADECE ŞIKLI)
            bool hasClassicQuestion = studentExam.Exam.Questions
                .Any(q => q.Type == QuestionType.Klasik);

            if (!hasClassicQuestion)
            {
                var answers = await _context.Answers
                    .Where(a => a.StudentExamId == studentExam.Id)
                    .Include(a => a.Question)
                        .ThenInclude(q => q.Choices)
                    .ToListAsync();

                double totalScore = 0;

                foreach (var answer in answers)
                {
                    if (answer.SelectedChoiceId.HasValue)
                    {
                        var correctChoice = answer.Question.Choices
                            .FirstOrDefault(c => c.IsCorrect);

                        if (correctChoice != null &&
                            correctChoice.Id == answer.SelectedChoiceId)
                        {
                            totalScore += answer.Question.Point ?? 0;
                        }
                    }
                }

                studentExam.Score = totalScore;
                studentExam.ScoreShared = true;
            }

            // ✅ PUANI KAYDET
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        // 🔹 SINAVLARIM (SAYFALAMALI)
        [HttpGet]
        public async Task<IActionResult> Sinavlarim(int? page)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("AccessDenied", "Account");

            // Sayfalama parametreleri
            int pageSize = 10;
            int pageNumber = page ?? 1;

            // Öğrencinin tüm sınavlarını çekiyoruz
            var examsQuery = _context.StudentExams
                .Where(se => se.StudentId == user.Id)
                .Include(se => se.Exam)
                    .ThenInclude(e => e.Course)
                .OrderByDescending(se => se.Exam.StartDate);

            // Toplam kayıt sayısı
            var totalCount = await examsQuery.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            // Sayfalı veriyi al
            var pagedExams = await examsQuery
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // İstatistikler için ayrı sorgular
            var allExams = await examsQuery.ToListAsync();
            var tamamlananSinav = allExams.Count(e => e.Completed);
            var bekleyenSinav = allExams.Count(e => !e.Completed);

            // ViewBag ile verileri view'a gönder
            ViewBag.ToplamSinav = totalCount;
            ViewBag.TamamlananSinav = tamamlananSinav;
            ViewBag.BekleyenSinav = bekleyenSinav;
            ViewBag.PageNumber = pageNumber;
            ViewBag.TotalPages = totalPages;
            ViewBag.HasPreviousPage = pageNumber > 1;
            ViewBag.HasNextPage = pageNumber < totalPages;

            return View(pagedExams);
        }

        // 🔹 SINAV SONUCU GÖRÜNTÜLE
        [HttpGet]
        public async Task<IActionResult> ExamResult(int studentExamId)
        {
            var studentId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var studentExam = await _context.StudentExams
                .Include(se => se.Exam)
                    .ThenInclude(e => e.Questions)
                        .ThenInclude(q => q.Choices)
                .Include(se => se.Answers)
                .FirstOrDefaultAsync(se => se.Id == studentExamId && se.StudentId == studentId);

            if (studentExam == null)
                return Unauthorized();

            if (!studentExam.Completed)
            {
                TempData["ErrorMessage"] = "Bu sınav henüz tamamlanmadı!";
                return RedirectToAction("Sinavlarim");
            }

            return View(studentExam);
        }
    }
}