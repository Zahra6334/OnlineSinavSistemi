using Microsoft.EntityFrameworkCore;
using OnlineSinavSistemi.Data;
using OnlineSinavSistemi.Models;

namespace OnlineSinavSistemi.Services
{
    public class ReminderService : IReminderService
    {
        private readonly ApplicationDbContext _db;

        public ReminderService(ApplicationDbContext db) { _db = db; }

        public async Task<IEnumerable<Reminder>> GetRemindersForStudentAsync(string studentId)
        {
            return await _db.Reminders
                .Where(r => r.StudentId == studentId && !r.IsRead)
                .OrderBy(r => r.Date) // en yakın tarih önce
                .ToListAsync();
        }

    }


}
