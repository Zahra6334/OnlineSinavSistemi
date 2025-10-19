using OnlineSinavSistemi.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OnlineSinavSistemi.Services
{
    public interface IAnswerService
    {
        Task<Answer> CreateAnswerAsync(Answer answer);
        Task<IEnumerable<Answer>> GetAnswersByStudentExamAsync(int studentExamId);
    }
}