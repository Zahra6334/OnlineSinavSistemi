using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineSinavSistemi.Data;
using OnlineSinavSistemi.Models;
using OnlineSinavSistemi.Services;
using System.Security.Claims;
using System.Threading.Tasks;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Http; // Session için gerekli

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

        // 🔹 STUDENT DASHBOARD
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("AccessDenied", "Account");

            var dersler = await _context.CourseStudents
                .Where(cs => cs.StudentId == user.Id)
                .Include(cs => cs.Course)
                    .ThenInclude(c => c.Teacher)
                .Select(cs => cs.Course)
                .ToListAsync();

            return View(dersler);
        }

        // ---------------------------------------------------------------------
        // 🔹 SINAVA GİR (GÜVENLİK VE OTURUM KONTROLÜ EKLENDİ)
        // ---------------------------------------------------------------------
        [HttpGet]
        public async Task<IActionResult> TakeExam(int examId)
        {
            var studentId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Sınavı ve daha önceki cevapları (varsa) getiriyoruz
            var studentExam = await _context.StudentExams
               .Include(se => se.Exam)
                   .ThenInclude(e => e.Questions)
                       .ThenInclude(q => q.Choices)
               .Include(se => se.Answers)
               .FirstOrDefaultAsync(se => se.ExamId == examId && se.StudentId == studentId);

            if (studentExam == null) return Unauthorized();

            // 1. Temel Kontroller
            if (studentExam.Completed)
            {
                TempData["ErrorMessage"] = "Bu sınavı zaten tamamladınız.";
                return RedirectToAction("Sinavlarim");
            }

            var now = DateTime.Now;
            var examEndDate = studentExam.Exam.StartDate.AddMinutes(studentExam.Exam.DurationMinutes);

            if (now < studentExam.Exam.StartDate)
            {
                TempData["ErrorMessage"] = "Sınav zamanı henüz gelmedi!";
                return RedirectToAction("Sinavlarim");
            }

            if (now > examEndDate)
            {
                TempData["ErrorMessage"] = "Sınavın geçerlilik süresi doldu.";
                return RedirectToAction("Sinavlarim");
            }

            // 2. TEK GİRİŞ HAKKI (Session Koruması)
            string sessionKey = $"ExamSession_{studentExam.Id}";

            if (studentExam.StartTime.HasValue)
            {
                // Veritabanında giriş saati var. Peki Session var mı?
                var sessionStatus = HttpContext.Session.GetString(sessionKey);

                if (string.IsNullOrEmpty(sessionStatus))
                {
                    // KRİTİK: Başlangıç saati var ama Session yok. 
                    // Demek ki öğrenci tarayıcıyı kapatmış veya başka cihazdan deniyor.
                    // CEZA: Sınavı bitir.

                    studentExam.Completed = true;
                    studentExam.EndTime = DateTime.Now;
                    await _context.SaveChangesAsync();

                    TempData["ErrorMessage"] = "Sınav ekranından ayrıldığınız veya tarayıcıyı kapattığınız için sınavınız sonlandırıldı.";
                    return RedirectToAction("Sinavlarim");
                }

                // Session varsa sorun yok, sayfa yenilemiştir. Devam etsin.
            }
            else
            {
                // İlk defa giriyor
                studentExam.StartTime = DateTime.Now;
                await _context.SaveChangesAsync();

                // Session Damgası Vuruyoruz
                HttpContext.Session.SetString(sessionKey, "Active");
            }

            // 3. Kişisel Süre Kontrolü
            var studentEndTime = studentExam.StartTime.Value.AddMinutes(studentExam.Exam.DurationMinutes);
            if (now > studentEndTime)
            {
                studentExam.Completed = true;
                studentExam.EndTime = DateTime.Now;
                await _context.SaveChangesAsync();

                // Session'ı temizle
                HttpContext.Session.Remove(sessionKey);

                TempData["ErrorMessage"] = "Süreniz doldu!";
                return RedirectToAction("Sinavlarim");
            }

            return View(studentExam);
        }

        // ---------------------------------------------------------------------
        // 🔹 ANLIK KAYIT (AJAX İLE ÇAĞRILACAK) - YENİ EKLENDİ
        // ---------------------------------------------------------------------
        [HttpPost]
        public async Task<IActionResult> SaveSingleAnswer(int studentExamId, int questionId, int? selectedChoiceId, string answerText)
        {
            var studentId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Sınavın geçerliliğini kontrol et
            var studentExam = await _context.StudentExams
                .FirstOrDefaultAsync(se => se.Id == studentExamId && se.StudentId == studentId);

            if (studentExam == null || studentExam.Completed)
            {
                return Json(new { success = false, message = "Sınav aktif değil." });
            }

            // Mevcut cevabı bul veya yenisini oluştur
            var existingAnswer = await _context.Answers
                .FirstOrDefaultAsync(a => a.StudentExamId == studentExamId && a.QuestionId == questionId);

            if (existingAnswer == null)
            {
                existingAnswer = new Answer
                {
                    StudentExamId = studentExamId,
                    QuestionId = questionId
                };
                _context.Answers.Add(existingAnswer);
            }

            // Verileri güncelle
            if (selectedChoiceId.HasValue)
                existingAnswer.SelectedChoiceId = selectedChoiceId.Value;

            if (answerText != null) // Boş string ("") gelebilir, null kontrolü yeterli
                existingAnswer.AnswerText = answerText;

            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }

        // ---------------------------------------------------------------------
        // 🔹 SINAVI BİTİR (GÜNCELLENDİ: Çift Kayıt Önleme)
        // ---------------------------------------------------------------------
        [HttpPost]
        public async Task<IActionResult> SubmitExam(int Id)
        {
            var studentId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Cevapları da Include ediyoruz ki güncelleme yapabilelim
            var studentExam = await _context.StudentExams
                .Include(se => se.Answers)
                .Include(se => se.Exam)
                    .ThenInclude(e => e.Questions)
                        .ThenInclude(q => q.Choices)
                .FirstOrDefaultAsync(se => se.Id == Id && se.StudentId == studentId);

            if (studentExam == null) return Unauthorized();
            if (studentExam.Completed) return RedirectToAction("Index");

            var questions = studentExam.Exam.Questions.ToList();
            bool hasClassicQuestion = questions.Any(q => q.Type == QuestionType.Klasik);
            double totalScore = 0;

            for (int i = 0; i < questions.Count; i++)
            {
                var question = questions[i];

                // 1. Önce bu soru için veritabanında zaten kayıtlı bir cevap var mı? (Anlık kayıt sayesinde olabilir)
                var answer = studentExam.Answers.FirstOrDefault(a => a.QuestionId == question.Id);

                // Eğer yoksa yeni oluştur
                if (answer == null)
                {
                    answer = new Answer
                    {
                        StudentExamId = studentExam.Id,
                        QuestionId = question.Id
                    };
                    _context.Answers.Add(answer);
                }

                // 2. Formdan gelen en son veriyi al
                var selectedChoice = Request.Form[$"Answers[{i}].SelectedChoiceId"];
                var textAnswer = Request.Form[$"Answers[{i}].AnswerText"];
                var file = Request.Form.Files.FirstOrDefault(f => f.Name == $"Answers[{i}].FileUpload");

                // Şık Cevabı Güncelle
                if (!string.IsNullOrEmpty(selectedChoice) && int.TryParse(selectedChoice, out int choiceId))
                {
                    answer.SelectedChoiceId = choiceId;
                }

                // Metin Cevabı Güncelle
                if (!string.IsNullOrEmpty(textAnswer))
                {
                    answer.AnswerText = textAnswer;
                }

                // Dosya Varsa Yükle
                if (file != null && file.Length > 0)
                {
                    var uploads = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");
                    if (!Directory.Exists(uploads)) Directory.CreateDirectory(uploads);

                    var fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
                    var path = Path.Combine(uploads, fileName);
                    using var stream = new FileStream(path, FileMode.Create);
                    await file.CopyToAsync(stream);
                    answer.FilePath = "/uploads/" + fileName;
                }

                // 3. PUAN HESAPLAMA (Final Kontrol)
                if (!hasClassicQuestion && answer.SelectedChoiceId.HasValue)
                {
                    var correctChoice = question.Choices.FirstOrDefault(c => c.IsCorrect);
                    if (correctChoice != null && correctChoice.Id == answer.SelectedChoiceId)
                    {
                        totalScore += question.Point ?? 0;
                    }
                }
            }

            // Sınavı Kapat
            studentExam.Completed = true;
            studentExam.EndTime = DateTime.Now;

            // Session'ı temizle (Sınav bittiği için artık session'a gerek yok)
            HttpContext.Session.Remove($"ExamSession_{studentExam.Id}");

            if (!hasClassicQuestion)
            {
                studentExam.Score = totalScore;
                studentExam.ScoreShared = true;
            }
            else
            {
                studentExam.ScoreShared = false;
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        // 🔹 SINAVLARIM
        [HttpGet]
        public async Task<IActionResult> Sinavlarim(int? page)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("AccessDenied", "Account");

            int pageSize = 5;
            int pageNumber = page ?? 1;

            var examsQuery = _context.StudentExams
                .Where(se => se.StudentId == user.Id)
                .Include(se => se.Exam)
                    .ThenInclude(e => e.Course)
                .OrderByDescending(se => se.Exam.StartDate);

            var totalCount = await examsQuery.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var pagedExams = await examsQuery
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var allExams = await examsQuery.ToListAsync();

            ViewBag.ToplamSinav = totalCount;
            ViewBag.TamamlananSinav = allExams.Count(e => e.Completed);
            ViewBag.BekleyenSinav = allExams.Count(e => !e.Completed);
            ViewBag.PageNumber = pageNumber;
            ViewBag.TotalPages = totalPages;
            ViewBag.HasPreviousPage = pageNumber > 1;
            ViewBag.HasNextPage = pageNumber < totalPages;

            return View(pagedExams);
        }

        // 🔹 SINAV SONUCU
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

            if (studentExam == null) return Unauthorized();

            if (!studentExam.Completed)
            {
                TempData["ErrorMessage"] = "Bu sınav henüz tamamlanmadı!";
                return RedirectToAction("Sinavlarim");
            }

            return View(studentExam);
        }
    }
}