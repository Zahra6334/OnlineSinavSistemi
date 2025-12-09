using OnlineSinavSistemi.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OnlineSinavSistemi.Services
{
    public interface IExamService
    {
        Task<Exam> CreateExamAsync(Exam exam);
        Task<Exam> GetExamByIdAsync(int id);
        Task<IEnumerable<Exam>> GetExamsForTeacherAsync(string teacherId);

        // Yeni metodlar
        Task PublishExamAsync(int examId);
        Task<List<StudentExam>> GetStudentExamsForExamAsync(int examId);

        Task<StudentExam> GetStudentExamByIdAsync(int studentExamId);
        Task GradeStudentExamAsync(StudentExam model);
    }
}