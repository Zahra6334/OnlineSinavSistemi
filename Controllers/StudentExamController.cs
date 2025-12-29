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
            var now = DateTime.Now;
            var examStartDate = studentExam.Exam.StartDate;
            var examDuration = studentExam.Exam.DurationMinutes;
            var examGlobalEndDate = examStartDate.AddMinutes(examDuration);

            // A. Sınav henüz başlamadıysa
            if (now < examStartDate)
            {
                TempData["ErrorMessage"] = "Sınav zamanı henüz gelmedi!";
                return RedirectToAction("Sinavlarim");
            }

            // B. Sınavın genel süresi dolduysa (Başlangıç saati + Süre)
            if (now > examGlobalEndDate)
            {
                TempData["ErrorMessage"] = "Sınavın geçerlilik süresi doldu, artık giriş yapamazsınız.";
                return RedirectToAction("Sinavlarim");
            }
            // ---------------------------------------------------------------------

            // Sınav zaten tamamlandıysa tekrar giremesin
            if (studentExam.Completed)
            {
                TempData["ErrorMessage"] = "Bu sınavı zaten tamamladınız.";
                return RedirectToAction("Sinavlarim");
            }

            // Başlangıç zamanı kaydedilmemişse (Öğrenci ilk kez giriyorsa) kaydet
            if (!studentExam.StartTime.HasValue)
            {
                studentExam.StartTime = DateTime.Now;
                _context.SaveChanges();
            }

            // Öğrencinin KENDİ süresinin kontrolü (Örn: Sınava geç girdi, ne kadar vakti kaldı?)
            if (studentExam.Exam.DurationMinutes > 0)
            {
                // Öğrencinin girdiği andan itibaren değil, sınavın BİTİŞ saatine göre kalan süre
                // Burada mantık tercihe bağlıdır: 
                // 1. Yöntem: Öğrenci geç girse bile tam süre verilir (StartTime + Duration).
                // 2. Yöntem: Sınav 10:00-11:00 arasındaysa ve 10:30'da girdiyse sadece 30 dk verilir.

                // Senin kodundaki mevcut yapı 1. Yönteme benziyor ama "Sınav Süresi" kavramı genellikle
                // sınavın global bitiş saatini aşamaz.

                // Bu yüzden şu kontrolü de ekliyoruz:
                // Öğrencinin bitirmesi gereken tahmini zaman
                var studentEndTime = studentExam.StartTime.Value.AddMinutes(studentExam.Exam.DurationMinutes);

                // Eğer şu anki zaman, öğrencinin süresini aştıysa VEYA sınavın global süresini aştıysa
                if (now > studentEndTime || now > examGlobalEndDate)
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
        public async Task<IActionResult> SubmitExam(int Id)
        {
            var studentId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // 1. Sınavı ve soruları, şıklarıyla birlikte çek
            var studentExam = await _context.StudentExams
                .Include(se => se.Exam)
                    .ThenInclude(e => e.Questions)
                        .ThenInclude(q => q.Choices)
                .FirstOrDefaultAsync(se => se.Id == Id && se.StudentId == studentId);

            if (studentExam == null) return Unauthorized();

            // Çift gönderimi engelle
            if (studentExam.Completed) return RedirectToAction("Index");

            var questions = studentExam.Exam.Questions.ToList();

            // Klasik soru var mı kontrol et (Varsa otomatik puanlama devre dışı kalacak)
            bool hasClassicQuestion = questions.Any(q => q.Type == QuestionType.Klasik);

            double totalScore = 0;

            for (int i = 0; i < questions.Count; i++)
            {
                var question = questions[i];
                var answer = new Answer
                {
                    StudentExamId = studentExam.Id,
                    QuestionId = question.Id
                };

                // Formdan gelen verileri al
                var selectedChoice = Request.Form[$"Answers[{i}].SelectedChoiceId"];
                var textAnswer = Request.Form[$"Answers[{i}].AnswerText"];
                var file = Request.Form.Files.FirstOrDefault(f => f.Name == $"Answers[{i}].FileUpload");

                // Şık seçimi varsa ata
                if (!string.IsNullOrEmpty(selectedChoice) && int.TryParse(selectedChoice, out int choiceId))
                {
                    answer.SelectedChoiceId = choiceId;

                    // --- OTOMATİK PUAN HESAPLAMA (BURAYA EKLENDİ) ---
                    // Eğer klasik soru yoksa, döngü içindeyken puanı hesapla
                    if (!hasClassicQuestion)
                    {
                        var correctChoice = question.Choices.FirstOrDefault(c => c.IsCorrect);
                        if (correctChoice != null && correctChoice.Id == choiceId)
                        {
                            totalScore += question.Point ?? 0;
                        }
                    }
                    // ------------------------------------------------
                }

                if (!string.IsNullOrEmpty(textAnswer))
                    answer.AnswerText = textAnswer;

                // Dosya yükleme işlemi
                if (file != null && file.Length > 0)
                {
                    var uploads = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads"); // Path düzeltildi
                    if (!Directory.Exists(uploads)) Directory.CreateDirectory(uploads);

                    var fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
                    var path = Path.Combine(uploads, fileName);
                    using var stream = new FileStream(path, FileMode.Create);
                    await file.CopyToAsync(stream);
                    answer.FilePath = "/uploads/" + fileName;
                }

                _context.Answers.Add(answer);
            }

            // Sınavı tamamlandı olarak işaretle
            studentExam.Completed = true;
            studentExam.EndTime = DateTime.Now;

            // Eğer klasik soru yoksa puanı ve yayınlama durumunu güncelle
            if (!hasClassicQuestion)
            {
                studentExam.Score = totalScore;
                studentExam.ScoreShared = true; // Puanı öğrenciye göster
            }
            else
            {
                studentExam.ScoreShared = false; // Hoca onayı bekle
            }

            // TEK SEFERDE KAYDET
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