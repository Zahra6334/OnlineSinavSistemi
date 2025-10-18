namespace OnlineSinavSistemi.Models
{
    public class CourseStudent
    {
        public int Id { get; set; }
        public int CourseId { get; set; }
        public Course Course { get; set; }

        public string StudentId { get; set; }
        public ApplicationUser Student { get; set; }
    }
}