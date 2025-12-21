using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OnlineSinavSistemi.Data;
using OnlineSinavSistemi.Models;
using OnlineSinavSistemi.Services;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace OnlineSinavSistemi.Controllers
{
    public class TeacherController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IExamService _examService;

        public TeacherController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IExamService examService)
        {
            _context = context;
            _userManager = userManager;
            _examService = examService;
        }

        // ---------------------------------------------------------------------
        // 🔹 TEACHER DASHBOARD (ANA SAYFA)
        // ---------------------------------------------------------------------
        public async Task<IActionResult> Index()
        {
            var teacher = await _userManager.GetUserAsync(User);
            if (teacher == null)
                return Unauthorized();

            // Öğretmenin ders ve sınav istatistiklerini getir
            var dersler = await _context.Courses
                .Where(c => c.TeacherId== teacher.Id)
                .Include(c => c.Exams)
                .ToListAsync();

            var sinavlar = await _context.Exams
                .Where(e => e.TeacherId == teacher.Id)
                .ToListAsync();

            ViewBag.DersSayisi = dersler.Count;
            ViewBag.SinavSayisi = sinavlar.Count;

            return View();
        }

        // ---------------------------------------------------------------------
        // 🔹 SINAV GİREN ÖĞRENCİLERİ GÖSTER
        // ---------------------------------------------------------------------
        public async Task<IActionResult> ExamStudents(int examId)
        {
            var exam = await _context.Exams
                .Include(e => e.Course)
                .FirstOrDefaultAsync(e => e.Id == examId);

            if (exam == null)
                return NotFound();

            var students = await _context.StudentExams
                .Where(se => se.ExamId == examId)
                .Include(se => se.Student)
                .ToListAsync();

            ViewBag.Exam = exam;
            return View(students);
        }
        [HttpPost]
        public async Task<IActionResult> PublishExam(int examId)
        {
            var exam = await _context.Exams
                .Include(e => e.Course)
                .ThenInclude(c => c.CourseStudents)
                .FirstOrDefaultAsync(e => e.Id == examId);

            if (exam == null) return NotFound();

            exam.IsPublished = true;
            _context.Exams.Update(exam);

            // Hatırlatıcı oluştur
            foreach (var student in exam.Course.CourseStudents)
            {
                var reminder = new Reminder
                {
                    StudentId = student.StudentId,
                    ExamId = exam.Id,
                    Message = $"'{exam.Title}' sınavınız yaklaşıyor!",
                    Date = exam.StartDate
                };
                _context.Reminders.Add(reminder);
            }

            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }


        [HttpPost]
        public async Task<IActionResult> SaveScore(int studentExamId, double score, bool shareScore)
        {
            var studentExam = await _context.StudentExams
                .Include(se => se.Exam)
                .FirstOrDefaultAsync(se => se.Id == studentExamId);

            if (studentExam == null)
                return NotFound();

            studentExam.Score = score;
            studentExam.ScoreShared = shareScore;

            await _context.SaveChangesAsync();

            return RedirectToAction("ExamStudents", new { examId = studentExam.ExamId });
        }

        // ---------------------------------------------------------------------
        // 🔹 ÖĞRENCİ CEVAPLARINI GÖSTER (HOCA)
        // ---------------------------------------------------------------------
        public async Task<IActionResult> StudentAnswers(int studentExamId)
        {
                    // 1️⃣ Öğrencinin cevaplarını al (ŞIKLAR DAHİL)
                    var answers = await _context.Answers
             .Where(a => a.StudentExamId == studentExamId)
             .Include(a => a.Question)
                 .ThenInclude(q => q.Choices)
             .Include(a => a.StudentExam)           // EKLENDİ
                 .ThenInclude(se => se.Exam)       // EKLENDİ
             .ToListAsync();


            // 2️⃣ Cevap yoksa mesaj ver
            if (!answers.Any())
            {
                ViewBag.Message = "Bu sınava ait cevap bulunamadı.";
            }

            // 3️⃣ View için gerekli id
            ViewBag.StudentExamId = studentExamId;

            // 4️⃣ VIEW'E GÖNDER
            return View(answers);
        }


        private double CalculateTestScore(List<Answer> answers)
        {
            int correct = answers.Count(a =>
                a.SelectedChoice != null && a.SelectedChoice.IsCorrect);

            int total = answers.Count(a => a.SelectedChoiceId != null);

            if (total == 0) return 0;

            return (double)correct / total * 100;
        }


        [HttpPost]
        public async Task<IActionResult> TogglePublish(int id)
        {
            await _examService.PublishExamAsync(id);
            return RedirectToAction("Index");
        }



    }
}
