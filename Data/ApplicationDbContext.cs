using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using OnlineSinavSistemi.Models;

namespace OnlineSinavSistemi.Data
{
    

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        // ✅ DbSet Tanımları
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

            // =============================
            //  🔹 Course - Teacher (Ogretmen)
            // =============================
            builder.Entity<Course>()
                .HasOne(c => c.Teacher)
                .WithMany()
                .HasForeignKey(c => c.TeacherId)
                .OnDelete(DeleteBehavior.Restrict);

            // =============================
            //  🔹 Exam - Teacher (Ogretmen)
            // =============================
            builder.Entity<Exam>()
                .HasOne(e => e.Teacher)
                .WithMany()
                .HasForeignKey(e => e.TeacherId)
                .OnDelete(DeleteBehavior.Restrict);

            // =============================
            //  🔹 Course - Student (Many-to-Many)
            // =============================
            builder.Entity<CourseStudent>()
                .HasKey(cs => new { cs.CourseId, cs.StudentId }); // Composite Key

            builder.Entity<CourseStudent>()
                .HasOne(cs => cs.Course)
                .WithMany(c => c.CourseStudents)
                .HasForeignKey(cs => cs.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<CourseStudent>()
                .HasOne(cs => cs.Student)
                .WithMany()
                .HasForeignKey(cs => cs.StudentId)
                .OnDelete(DeleteBehavior.Cascade);

            // =============================
            //  🔹 StudentExam (Bir öğrenci bir sınava bir kez girebilir)
            // =============================
            builder.Entity<StudentExam>()
                .HasIndex(se => new { se.StudentId, se.ExamId })
                .IsUnique();

            builder.Entity<StudentExam>()
                .HasOne(se => se.Student)
                .WithMany(u => u.StudentExams)
                .HasForeignKey(se => se.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            // =============================
            //  🔹 Question - Choice (Cascade Delete)
            // =============================
            builder.Entity<Choice>()
                .HasOne(c => c.Question)
                .WithMany(q => q.Choices)
                .HasForeignKey(c => c.QuestionId)
                .OnDelete(DeleteBehavior.Cascade);

            // =============================
            //  🔹 Answer - StudentExam & Question
            // =============================
            builder.Entity<Answer>()
                .HasOne(a => a.StudentExam)
                .WithMany()
                .HasForeignKey(a => a.StudentExamId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Answer>()
                .HasOne(a => a.Question)
                .WithMany()
                .HasForeignKey(a => a.QuestionId)
                .OnDelete(DeleteBehavior.Restrict);

            // =============================
            //  🔹 Reminder - Student
            // =============================
            builder.Entity<Reminder>()
                .HasOne(r => r.Student)
                .WithMany()
                .HasForeignKey(r => r.StudentId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
