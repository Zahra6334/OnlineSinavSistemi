using OnlineSinavSistemi.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OnlineSinavSistemi.Services
{
    public interface IReminderService
    {
        Task<IEnumerable<Reminder>> GetRemindersForStudentAsync(string studentId);
    }
}