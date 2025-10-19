using OnlineSinavSistemi.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OnlineSinavSistemi.Services
{
    public interface IStudentExamService
    {
        Task<IEnumerable<StudentExam>> GetExamsForStudentAsync(string studentId);
        Task<StudentExam> StartExamAsync(int examId, string studentId);
        Task SubmitExamAsync(StudentExam model);
    }
}