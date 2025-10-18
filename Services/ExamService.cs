using Microsoft.EntityFrameworkCore;
using OnlineSinavSistemi.Data;
using OnlineSinavSistemi.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OnlineSinavSistemi.Services
{
    public class ExamService : IExamService
    {
        private readonly ApplicationDbContext _db;
        public ExamService(ApplicationDbContext db) { _db = db; }

        public async Task<Exam> CreateExamAsync(Exam exam)
        {
            _db.Exams.Add(exam);
            await _db.SaveChangesAsync();
            return exam;
        }

        public async Task<Exam> GetExamByIdAsync(int id)
        {
            return await _db.Exams
                .Include(e => e.Questions).ThenInclude(q => q.Choices)
                .FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task<IEnumerable<Exam>> GetExamsForTeacherAsync(string teacherId)
        {
            return await _db.Exams
                .Where(e => e.OgretmenId == teacherId)
                .Include(e => e.Course)
                .ToListAsync();
        }
    }
}