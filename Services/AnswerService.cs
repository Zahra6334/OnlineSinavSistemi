using Microsoft.EntityFrameworkCore;
using OnlineSinavSistemi.Data;
using OnlineSinavSistemi.Models;

namespace OnlineSinavSistemi.Services
{
    public class AnswerService : IAnswerService
    {
        private readonly ApplicationDbContext _db;

        public AnswerService(ApplicationDbContext db) { _db = db; }

        public async Task<Answer> CreateAnswerAsync(Answer answer)
        {
            _db.Answers.Add(answer);
            await _db.SaveChangesAsync();
            return answer;
        }

        public async Task<IEnumerable<Answer>> GetAnswersByStudentExamAsync(int studentExamId)
        {
            return await _db.Answers
                .Include(a => a.Question)
                .Where(a => a.StudentExamId == studentExamId)
                .ToListAsync();
        }
    }
}
