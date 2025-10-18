using System;

namespace OnlineSinavSistemi.Models
{
    public class Answer
    {
        public int Id { get; set; }

        public int QuestionId { get; set; }
        public Question Question { get; set; }

        public int StudentExamId { get; set; }
        public StudentExam StudentExam { get; set; }

        public string AnswerText { get; set; } // klasik metin
        public string FilePath { get; set; } // dosya yolu (varsa)
        public int? SelectedChoiceId { get; set; } // çoktan seçmeli için seçilen şık Id
    }
}