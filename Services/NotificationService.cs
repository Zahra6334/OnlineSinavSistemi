using OnlineSinavSistemi.Data;
using OnlineSinavSistemi.Models;

namespace OnlineSinavSistemi.Services
{
    public class NotificationService : INotificationService
    {
        private readonly ApplicationDbContext _db;

        public NotificationService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task CreateAsync(
            string userId,
            NotificationType type,
            string title,
            string description,
            int? examId = null,
            int? courseId = null
        )
        {
            var notification = new Notification
            {
                UserId = userId,
                Type = type,
                Title = title,
                Description = description,
                ExamId = examId,
                CourseId = courseId,
                IsRead = false,
                CreatedAt = DateTime.Now
            };

            //_db.Notifications.Add(notification);
            await _db.SaveChangesAsync();
        }
    }
}
