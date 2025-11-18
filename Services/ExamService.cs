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

        public ExamService(ApplicationDbContext db)
        {
            _db = db;
        }

        // 🟢 Sınav oluşturma
        public async Task<Exam> CreateExamAsync(Exam exam)
        {
            _db.Exams.Add(exam);
            await _db.SaveChangesAsync();
            return exam;
        }

        // 🟢 Sınav detaylarını getirme
        public async Task<Exam> GetExamByIdAsync(int id)
        {
            return await _db.Exams
                .Include(e => e.Questions)
                .ThenInclude(q => q.Choices)
                .Include(e => e.StudentExams)
                .ThenInclude(se => se.Student)
                .FirstOrDefaultAsync(e => e.Id == id);
        }

        // 🟢 Öğretmenin sınavlarını listeleme
        public async Task<IEnumerable<Exam>> GetExamsForTeacherAsync(string teacherId)
        {
            return await _db.Exams
                .Where(e => e.TeacherId == teacherId)
                .Include(e => e.Course)
                .ToListAsync();
        }

        // 🟢 Sınavı yayınla / yayından kaldır
        public async Task PublishExamAsync(int examId)
        {
            var exam = await _db.Exams.FindAsync(examId);
            if (exam != null)
            {
                exam.IsPublished = !exam.IsPublished;
                await _db.SaveChangesAsync();
            }
        }

        // 🟢 Sınavdaki öğrencileri listele
        public async Task<IEnumerable<ApplicationUser>> GetStudentsForExamAsync(int examId)
        {
            var studentIds = await _db.StudentExams
                .Where(se => se.ExamId == examId)
                .Select(se => se.StudentId)
                .ToListAsync();

            return await _db.Users
                .Where(u => studentIds.Contains(u.Id))
                .ToListAsync();
        }

        // 🟢 Belirli öğrenci sınavını getir
        public async Task<StudentExam> GetStudentExamByIdAsync(int studentExamId)
        {
            return await _db.StudentExams.FindAsync(studentExamId);
        }

        // 🟢 Öğrenciyi notlandır
        public async Task GradeStudentExamAsync(StudentExam model)
        {
            var studentExam = await _db.StudentExams.FindAsync(model.Id);
            if (studentExam != null)
            {
                studentExam.Score = model.Score;
                await _db.SaveChangesAsync();
            }
        }
    }
}
