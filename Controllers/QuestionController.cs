using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineSinavSistemi.Models;
using OnlineSinavSistemi.Services;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace OnlineSinavSistemi.Controllers
{
    [Authorize]
    public class QuestionController : Controller
    {
        private readonly IQuestionService _questionService;

        public QuestionController(IQuestionService questionService)
        {
            _questionService = questionService;
        }

        // 🟢 Soru Listesi (Önceki kodun aynısı kalabilir)
        public async Task<IActionResult> Index(int examId)
        {
            var questions = await _questionService.GetQuestionsByExamIdAsync(examId);
            ViewBag.ExamId = examId;
            return View(questions);
        }

        // 🟢 Yeni Soru Ekleme Ekranı (GET)
        public IActionResult Create(int examId)
        {
            // Soru eklerken hangi Sınava eklediğimizi bilmemiz lazım
            var model = new Question
            {
                ExamId = examId,
                Choices = new List<Choice>() // Null hatası almamak için boş liste
            };
            return View(model);
        }

        // 🟢 Soruyu Kaydetme (POST) - KRİTİK KISIM BURASI
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Question model)
        {
            // 1. View'dan gelen doğru şıkkın sırasını (0,1,2,3) alıyoruz
            var correctIndexStr = Request.Form["CorrectIndex"];

            // 2. Choices listesi boş gelmesin diye kontrol ediyoruz
            if (model.Choices != null)
            {
                // Doğru şıkkı işaretleme işlemi
                if (!string.IsNullOrEmpty(correctIndexStr) && int.TryParse(correctIndexStr, out int index))
                {
                    // List yaptığımız için artık [index] kullanabiliriz
                    if (index >= 0 && index < model.Choices.Count)
                    {
                        model.Choices[index].IsCorrect = true;
                    }
                }

                // 3. İçi boş olan (metin girilmemiş) şıkları listeden siliyoruz
                model.Choices = model.Choices
                                     .Where(c => !string.IsNullOrWhiteSpace(c.Text)) // Choice modelinde Text özelliği varsa
                                     .ToList();

                // 4. Soru Tipini Belirle
                if (model.Choices.Any())
                {
                    model.Type = QuestionType.CoktanSecmeli;
                }
                else
                {
                    model.Type = QuestionType.Klasik;
                    model.Choices = null;
                }
            }
            else
            {
                model.Type = QuestionType.Klasik;
            }

            // 5. Kaydet
            await _questionService.CreateQuestionAsync(model);

            return RedirectToAction("Index", new { examId = model.ExamId });
        }

        // Silme Metodu (Aynen kalabilir)
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