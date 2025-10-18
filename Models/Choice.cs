namespace OnlineSinavSistemi.Models
{
    public class Choice
    {
        public int Id { get; set; }
        public int QuestionId { get; set; }
        public Question Question { get; set; }

        public string Metin { get; set; }
        public bool IsCorrect { get; set; }
    }
}