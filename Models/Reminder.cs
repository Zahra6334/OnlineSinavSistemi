using System;

namespace OnlineSinavSistemi.Models
{
    public class Reminder
    {
        public int Id { get; set; }
        public string StudentId { get; set; }
        public ApplicationUser Student { get; set; }

        public int ExamId { get; set; }
        public Exam Exam { get; set; }

        public string Message { get; set; }
        public DateTime Date { get; set; }
        public bool IsRead { get; set; } = false;

    }
}