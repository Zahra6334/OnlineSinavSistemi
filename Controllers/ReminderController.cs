using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineSinavSistemi.Data;
using OnlineSinavSistemi.Models;
using OnlineSinavSistemi.Services;
using System.Security.Claims;
using System.Threading.Tasks;

namespace OnlineSinavSistemi.Controllers
{
    [Authorize(Roles = "OGRENCI")]
    public class ReminderController : Controller
    {
        private readonly IReminderService _reminderService;
        private readonly ApplicationDbContext _context;
        public ReminderController(IReminderService reminderService, ApplicationDbContext context)
        {
            _reminderService = reminderService;
            _context = context;

        }

        public async Task<IActionResult> Index()
        {
            var studentId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var reminders = await _context.Reminders
                .Where(r => r.StudentId == studentId && r.Date <= DateTime.Now && !r.IsRead)
                .Include(r => r.Exam)
                .ToListAsync();

            return View(reminders);
        }



    }
}
