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
    }
}