using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineSinavSistemi.Models;
using OnlineSinavSistemi.Services;
using System.Threading.Tasks;

namespace OnlineSinavSistemi.Controllers
{
    [Authorize(Roles = "Ogrenci")]
    public class ReminderController : Controller
    {
        private readonly IReminderService _reminderService;

        public ReminderController(IReminderService reminderService)
        {
            _reminderService = reminderService;
        }

        // Öğrenciye yaklaşan sınav hatırlatıcıları
        public async Task<IActionResult> Index()
        {
            var reminders = await _reminderService.GetRemindersForStudentAsync(User.Identity.Name);
            return View(reminders);
        }
    }
}
