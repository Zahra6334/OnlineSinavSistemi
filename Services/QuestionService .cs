using Microsoft.EntityFrameworkCore;
using OnlineSinavSistemi.Data;
using OnlineSinavSistemi.Models;

namespace OnlineSinavSistemi.Services
{
    public class QuestionService : IQuestionService
    {
        private readonly ApplicationDbContext _db;

        public QuestionService(ApplicationDbContext db) { _db = db; }

        public async Task<IEnumerable<Question>> GetQuestionsByExamIdAsync(int examId)
        {
            return await _db.Questions
                .Include(q => q.Choices)
                .Where(q => q.ExamId == examId)
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
            await _db.SaveChangesAsync();
            return question;
        }

        public async Task DeleteQuestionAsync(int id)
        {
            var q = await _db.Questions.FindAsync(id);
            if (q != null)
            {
                _db.Questions.Remove(q);
                await _db.SaveChangesAsync();
            }
        }
    }
}
