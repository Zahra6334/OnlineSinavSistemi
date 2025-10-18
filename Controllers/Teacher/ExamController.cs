using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineSinavSistemi.Models;
using OnlineSinavSistemi.Services;
using System.Security.Claims;
using System.Threading.Tasks;

namespace OnlineSinavSistemi.Controllers.Teacher
{
    [Authorize(Roles = "Ogretmen")]
    public class ExamController : Controller
    {
        private readonly IExamService _examService;

        public ExamController(IExamService examService)
        {
            _examService = examService;
        }

        // GET: /Exam/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Exam/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Exam model)
        {
            if (!ModelState.IsValid) return View(model);

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            model.OgretmenId = userId;

            var created = await _examService.CreateExamAsync(model);

            // yaratılan sınavın edit sayfasına yönlendir (soru ekleme vs için)
            return RedirectToAction("Edit", new { id = created.Id });
        }

        // GET: /Exam/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var exam = await _examService.GetExamByIdAsync(id);
            if (exam == null) return NotFound();
            return View(exam);
        }

        // Diğer aksiyonlar: SoruEkle, Yayinla, OgrenciListesi, NotVer vs.
    }
}
