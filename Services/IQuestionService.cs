using OnlineSinavSistemi.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OnlineSinavSistemi.Services
{
    public interface IQuestionService
    {
        Task<IEnumerable<Question>> GetQuestionsByExamIdAsync(int examId);
        Task<Question> GetQuestionByIdAsync(int id);
        Task<Question> CreateQuestionAsync(Question question);
        Task DeleteQuestionAsync(int id);
    }
}