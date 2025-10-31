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
      

        public TeacherController(ApplicationDbContext context)
        {
            _context = context;
            
        }


        // ------------------------
        // INDEX: Dersler veya sınavlar listesi
        // ------------------------
        public async Task<IActionResult> Index()
        {
            // Burada User.Identity.Name kullanıyoruz, OgretmenId ile eşleşmeli
            var teacher = await _context.Users.FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);
            var sinavlar = await _context.Exams
                                         .Where(e => e.OgretmenId == teacher.Id)
                                         .Include(e => e.Course)
                                         .ToListAsync();

            return View(sinavlar);
        }

        // ------------------------
        // DETAILS: Sınav detayları + katılımcılar
        // ------------------------
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var sinav = await _context.Exams
                                      .Include(e => e.Course)
                                      .FirstOrDefaultAsync(e => e.Id == id);

            if (sinav == null) return NotFound();

            var katilimlar = await _context.StudentExams
                                           .Where(x => x.ExamId == id)
                                           .Include(x => x.Student)
                                           .ToListAsync();

            ViewBag.Katilimlar = katilimlar;
            return View(sinav);
        }

        // ------------------------
        // CREATE: Yeni sınav oluşturma
        // ------------------------
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var teacherId = User.Identity.Name;

            var dersler = await _context.Courses
                                        .Where(d => d.OgretmenId == teacherId)
                                        .ToListAsync();

            ViewBag.Dersler = new SelectList(dersler, "Id", "DersAdi");
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Exam sinav)
        {
           
            var teacher = await _context.Users.FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);
            if (teacher == null) return Unauthorized();

            if (!ModelState.IsValid)
            {
                var dersler = await _context.Courses
                                            .Where(d => d.OgretmenId == teacher.Id)
                                            .ToListAsync();
                ViewBag.Dersler = new SelectList(dersler, "Id", "DersAdi");
                return View(sinav);
            }

            sinav.OgretmenId = teacher.Id;
            sinav.BaslangicTarihi = DateTime.Now;

            _context.Exams.Add(sinav);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Sınav başarıyla oluşturuldu!";
            return RedirectToAction(nameof(Index));
        }



        // ------------------------
        // EDIT: Sınav düzenleme
        // ------------------------
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var sinav = await _context.Exams.FindAsync(id);
            if (sinav == null) return NotFound();

            var dersler = await _context.Courses
                                        .Where(d => d.OgretmenId == User.Identity.Name)
                                        .ToListAsync();
            ViewBag.Dersler = new SelectList(dersler, "Id", "Name", sinav.CourseId);

            return View(sinav);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Exam sinav)
        {
            if (id != sinav.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(sinav);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Sınav başarıyla güncellendi!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Exams.Any(e => e.Id == id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }

            var dersler = await _context.Courses
                                        .Where(d => d.OgretmenId == User.Identity.Name)
                                        .ToListAsync();
            ViewBag.Dersler = new SelectList(dersler, "Id", "Name", sinav.CourseId);
            return View(sinav);
        }

        // ------------------------
        // DELETE: Sınav silme
        // ------------------------
        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var sinav = await _context.Exams
                                      .Include(e => e.Course)
                                      .FirstOrDefaultAsync(e => e.Id == id);

            if (sinav == null) return NotFound();

            return View(sinav);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var sinav = await _context.Exams.FindAsync(id);
            _context.Exams.Remove(sinav);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Sınav başarıyla silindi!";
            return RedirectToAction(nameof(Index));
        }
    }
}
