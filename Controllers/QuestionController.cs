using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineSinavSistemi.Models;
using OnlineSinavSistemi.Services;
using System.Threading.Tasks;

namespace OnlineSinavSistemi.Controllers
{
    [Authorize(Roles = "Ogretmen")]
    public class QuestionController : Controller
    {
        private readonly IQuestionService _questionService;

        public QuestionController(IQuestionService questionService)
        {
            _questionService = questionService;
        }

        // Soru detayları / listesi
        public async Task<IActionResult> Index(int examId)
        {
            var questions = await _questionService.GetQuestionsByExamIdAsync(examId);
            ViewBag.ExamId = examId;
            return View(questions);
        }

        // Yeni soru ekleme (GET)
        public IActionResult Create(int examId)
        {
            var model = new Question { ExamId = examId };
            return View(model);
        }

        // Yeni soru ekleme (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Question model)
        {
            if (!ModelState.IsValid)
                return View(model);

            await _questionService.CreateQuestionAsync(model);
            return RedirectToAction("Index", new { examId = model.ExamId });
        }

        // Soru sil
        public async Task<IActionResult> Delete(int id)
        {
            var question = await _questionService.GetQuestionByIdAsync(id);
            if (question != null)
            {
                await _questionService.DeleteQuestionAsync(id);
                return RedirectToAction("Index", new { examId = question.ExamId });
            }
            return NotFound();
        }
    }
}
