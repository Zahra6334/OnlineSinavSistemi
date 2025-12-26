using OnlineSinavSistemi.Models;

namespace OnlineSinavSistemi.Services
{
    public interface INotificationService
    {
        Task CreateAsync(
            string userId,
            NotificationType type,
            string title,
            string description,
            int? examId = null,
            int? courseId = null
        );
    }
}
