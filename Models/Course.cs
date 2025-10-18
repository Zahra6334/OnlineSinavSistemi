using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace OnlineSinavSistemi.Models
{
    public class Course
    {
        public int Id { get; set; }

        [Required]
        public string DersAdi { get; set; }

        public string OgretmenId { get; set; }
        public ApplicationUser Ogretmen { get; set; }

        // Many-to-many öğrenciler için ayrıca junction table/ entity oluşturacağız (CourseStudent)
        public ICollection<CourseStudent> CourseStudents { get; set; }

        public ICollection<Exam> Exams { get; set; }
    }
}