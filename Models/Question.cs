using System.Collections.Generic;

namespace OnlineSinavSistemi.Models
{
    public enum QuestionType { CoktanSecmeli = 0, Klasik = 1 }

    public class Question
    {
        public int Id { get; set; }
        public int ExamId { get; set; }
        public Exam Exam { get; set; }

        public string QuestionText { get; set; }
        public QuestionType Type { get; set; }

        public ICollection<Choice> Choices { get; set; } // çoktan seçmeli ise
    }
}