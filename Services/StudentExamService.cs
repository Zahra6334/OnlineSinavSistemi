using Microsoft.EntityFrameworkCore;
using OnlineSinavSistemi.Data;
using OnlineSinavSistemi.Models;

namespace OnlineSinavSistemi.Services
{
    public class StudentExamService : IStudentExamService
    {
        private readonly ApplicationDbContext _db;

        public StudentExamService(ApplicationDbContext db) { _db = db; }

        public async Task<IEnumerable<StudentExam>> GetExamsForStudentAsync(string studentId)
        {
            return await _db.StudentExams
                .Include(se => se.Exam)
                .Where(se => se.StudentId == studentId)
                .ToListAsync();
        }

        public async Task<StudentExam> StartExamAsync(int examId, string studentId)
        {
            var se = await _db.StudentExams
                .FirstOrDefaultAsync(x => x.ExamId == examId && x.StudentId == studentId);

            if (se != null) return se;

            se = new StudentExam
            {
                ExamId = examId,
                StudentId = studentId,
                BaslangicTarihi = DateTime.Now
            };

            _db.StudentExams.Add(se);
            await _db.SaveChangesAsync();
            return se;
        }

        public async Task SubmitExamAsync(StudentExam model)
        {
            var se = await _db.StudentExams.FindAsync(model.Id);
            if (se != null)
            {
                se.BitisZamani = DateTime.Now;
                se.Tamamlandi = true;
                await _db.SaveChangesAsync();
            }
        }
    }
}
