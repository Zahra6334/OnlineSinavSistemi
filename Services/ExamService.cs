using Microsoft.EntityFrameworkCore;
using OnlineSinavSistemi.Data;
using OnlineSinavSistemi.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OnlineSinavSistemi.Services
{
    public class ExamService :IExamService
    {
        private readonly ApplicationDbContext _context;

        public ExamService(ApplicationDbContext context)
        {
            _context = context;
        }

        // 🟢 Sınav oluşturma
        public async Task<Exam> CreateExamAsync(Exam exam)
        {
            _context.Exams.Add(exam);
            await _context.SaveChangesAsync();
            return exam;
        }

        // 🟢 Sınav detaylarını getirme
        public async Task<Exam> GetExamByIdAsync(int id)
        {
            return await _context.Exams
                .Include(e => e.Questions)
                .ThenInclude(q => q.Choices)
                .Include(e => e.StudentExams)
                .ThenInclude(se => se.Student)
                .FirstOrDefaultAsync(e => e.Id == id);
        }

        // 🟢 Öğretmenin sınavlarını listeleme
        public async Task<IEnumerable<Exam>> GetExamsForTeacherAsync(string teacherId)
        {
            return await _context.Exams
                .Where(e => e.TeacherId == teacherId)
                .Include(e => e.Course)
                .ToListAsync();
        }

        // 🟢 Sınavdaki öğrencileri listele
        public async Task<List<StudentExam>> GetStudentExamsForExamAsync(int examId)
        {
            return await _context.StudentExams
                .Include(se => se.Student) // ✅ öğrenci
                .Include(se => se.Exam)    // ✅ sınav (opsiyonel ama faydalı)
                .Where(se => se.ExamId == examId)
                .ToListAsync();
        }





        // 🟢 Belirli öğrenci sınavını getir
        public async Task<StudentExam> GetStudentExamByIdAsync(int studentExamId)
        {
            return await _context.StudentExams.FindAsync(studentExamId);
        }

        // 🟢 Öğrenciyi notlandır
        public async Task GradeStudentExamAsync(StudentExam model)
        {
            var studentExam = await _context.StudentExams.FindAsync(model.Id);
            if (studentExam != null)
            {
                studentExam.Score = model.Score;
                await _context.SaveChangesAsync();
            }
        }
        public async Task PublishExamAsync(int examId)
        {
            var exam = await _context.Exams
                .Include(e => e.Course)
                .FirstOrDefaultAsync(e => e.Id == examId);

            if (exam == null) return;

            exam.IsPublished = !exam.IsPublished;

            if (exam.IsPublished)
            {
                // 🔴 Bu dersin öğrencileri
                var studentIds = await _context.CourseStudents
                    .Where(cs => cs.CourseId == exam.CourseId)
                    .Select(cs => cs.StudentId)
                    .ToListAsync();

                foreach (var studentId in studentIds)
                {
                    bool exists = await _context.StudentExams.AnyAsync(se =>
                        se.ExamId == examId && se.StudentId == studentId);

                    if (!exists)
                    {
                        _context.StudentExams.Add(new StudentExam
                        {
                            ExamId = examId,
                            StudentId = studentId,
                            Completed = false
                        });
                    }
                }
            }

            await _context.SaveChangesAsync();
        }

    }
}
