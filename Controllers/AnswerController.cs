using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineSinavSistemi.Models;
using OnlineSinavSistemi.Services;
using System.Threading.Tasks;

namespace OnlineSinavSistemi.Controllers
{
    [Authorize]
    public class AnswerController : Controller
    {
        private readonly IAnswerService _answerService;

        public AnswerController(IAnswerService answerService)
        {
            _answerService = answerService;
        }

        // Cevap ekleme (Öğrenci için)
        [HttpPost]
        public async Task<IActionResult> Create(Answer model)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            await _answerService.CreateAnswerAsync(model);
            return Ok();
        }

        // Öğretmen için cevapları listele
        public async Task<IActionResult> Index(int studentExamId)
        {
            var answers = await _answerService.GetAnswersByStudentExamAsync(studentExamId);
            ViewBag.StudentExamId = studentExamId;
            return View(answers);
        }
    }
}
