using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using OnlineSinavSistemi.Models;

namespace OnlineSinavSistemi.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<Course> Courses { get; set; }
        public DbSet<CourseStudent> CourseStudents { get; set; }
        public DbSet<Exam> Exams { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<Choice> Choices { get; set; }
        public DbSet<StudentExam> StudentExams { get; set; }
        public DbSet<Answer> Answers { get; set; }
        public DbSet<Reminder> Reminders { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Course -> Ogretmen (restrict delete)
            builder.Entity<Course>()
                .HasOne(c => c.Ogretmen)
                .WithMany()
                .HasForeignKey(c => c.OgretmenId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Exam>()
                .HasOne(e => e.Ogretmen)
                .WithMany()
                .HasForeignKey(e => e.OgretmenId)
                .OnDelete(DeleteBehavior.Restrict);

            // CourseStudent relation
            builder.Entity<CourseStudent>()
                .HasOne(cs => cs.Course)
                .WithMany(c => c.CourseStudents)
                .HasForeignKey(cs => cs.CourseId);

            builder.Entity<CourseStudent>()
                .HasOne(cs => cs.Student)
                .WithMany()
                .HasForeignKey(cs => cs.StudentId);

            // StudentExam -> Student relation
            builder.Entity<StudentExam>()
                .HasOne(se => se.Student)
                .WithMany(u => u.StudentExams)
                .HasForeignKey(se => se.StudentId);
        }
    }
}