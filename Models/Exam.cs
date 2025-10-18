using System;
using System.Collections.Generic;

namespace OnlineSinavSistemi.Models
{
    public class Exam
    {
        public int Id { get; set; }
        public string Baslik { get; set; }
        public int SureDakika { get; set; }
        public DateTime BaslangicTarihi { get; set; }
        // Bitis tarihi opsiyonel olabilir, Start + SureDakika olarak hesaplanır
        public bool Yayinlandi { get; set; }

        public int CourseId { get; set; }
        public Course Course { get; set; }

        public string OgretmenId { get; set; }
        public ApplicationUser Ogretmen { get; set; }

        public ICollection<Question> Questions { get; set; }
        public ICollection<StudentExam> StudentExams { get; set; }
    }
}