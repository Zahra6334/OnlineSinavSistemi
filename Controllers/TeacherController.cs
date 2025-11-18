using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OnlineSinavSistemi.Data;
using OnlineSinavSistemi.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace OnlineSinavSistemi.Controllers
{
    public class TeacherController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public TeacherController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
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
    }
}
