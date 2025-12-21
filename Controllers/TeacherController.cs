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
        public async Task PublishExamAsync(int examId)
        {
            var exam = await _context.Exams
                .FirstOrDefaultAsync(e => e.Id == examId);

            if (exam == null) return;

            // ✅ Sınavı yayınla
            exam.IsPublished = true;

            // ✅ Bu derse kayıtlı öğrencileri al
            var studentIds = await _context.CourseStudents
                .Where(cs => cs.CourseId == exam.CourseId)
                .Select(cs => cs.StudentId)
                .ToListAsync();

            foreach (var studentId in studentIds)
            {
                bool exists = await _context.StudentExams.AnyAsync(se =>
                    se.ExamId == examId && se.StudentId == studentId);

                if (!exists)
                {
                    _context.StudentExams.Add(new StudentExam
                    {
                        ExamId = examId,
                        StudentId = studentId,
                        Completed = false
                    });
                }
            }

            await _context.SaveChangesAsync();
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


        [HttpPost]
        public async Task<IActionResult> TogglePublish(int id)
        {
            await _examService.PublishExamAsync(id);
            return RedirectToAction("Index");
        }



    }
}
