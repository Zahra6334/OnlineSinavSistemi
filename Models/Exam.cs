using System;
using System.Collections.Generic;

namespace OnlineSinavSistemi.Models
{
    public class Exam
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public int DurationMinutes { get; set; }
        public DateTime StartDate { get; set; }
        // Bitis tarihi opsiyonel olabilir, Start + SureDakika olarak hesaplanır
        public bool IsPublished { get; set; }

        public int CourseId { get; set; }
        public Course Course { get; set; }

        public string TeacherId { get; set; }
        public ApplicationUser Teacher { get; set; }

        public ICollection<Question> Questions { get; set; }
        public ICollection<StudentExam> StudentExams { get; set; }
        // 🔴 YENİ EKLENDİ (ESKİ SİSTEMİ BOZMAZ)
        public bool IsAutoGraded { get; set; } = false;
    }
}