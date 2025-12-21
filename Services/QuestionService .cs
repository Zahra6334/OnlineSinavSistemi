using Microsoft.EntityFrameworkCore;
using OnlineSinavSistemi.Data;
using OnlineSinavSistemi.Models;

namespace OnlineSinavSistemi.Services
{
    public class QuestionService : IQuestionService
    {
        private readonly ApplicationDbContext _db;

        public QuestionService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<IEnumerable<Question>> GetQuestionsByExamIdAsync(int examId)
        {
            return await _db.Questions
                .Where(q => q.ExamId == examId)
                .Include(q => q.Choices) // 🔹 Şıkları getir
                .ToListAsync();
        }

        public async Task<Question> GetQuestionByIdAsync(int id)
        {
            return await _db.Questions
                .Include(q => q.Choices)
                .FirstOrDefaultAsync(q => q.Id == id);
        }

        public async Task<Question> CreateQuestionAsync(Question question)
        {
            _db.Questions.Add(question);
            await _db.SaveChangesAsync(); // 🔴 ID BURADA OLUŞUR

            if (question.Choices != null)
            {
                foreach (var choice in question.Choices)
                {
                    choice.QuestionId = question.Id; // ✅ artık ID var
                }

                await _db.SaveChangesAsync();
            }

            return question;
        }

        public async Task DeleteQuestionAsync(int id)
        {
            var q = await _db.Questions
                .Include(x => x.Choices)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (q != null)
            {
                if (q.Choices != null && q.Choices.Any())
                    _db.Choices.RemoveRange(q.Choices);

                _db.Questions.Remove(q);
                await _db.SaveChangesAsync();
            }
        }
    }
}