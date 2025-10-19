using System;
using System.Collections.Generic;

namespace OnlineSinavSistemi.Models
{
    public class StudentExam
    {
        public int Id { get; set; }

        public int ExamId { get; set; }
        public Exam Exam { get; set; }

        public string StudentId { get; set; }
        public ApplicationUser Student { get; set; }

        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public bool Completed { get; set; }
        public double? Score { get; set; }
        public bool ScoreShared { get; set; }

        public ICollection<Answer> Answers { get; set; }
        public DateTime BitisZamani { get; internal set; }
        public bool Tamamlandi { get; internal set; }
        public DateTime BaslangicTarihi { get; internal set; }
    }
}