namespace OnlineSinavSistemi.Models
{
    public enum NotificationType
    {
        SinavOlusturuldu = 0,
        SinavAtandi = 1,
        SinavYayinlandi = 2,
        SinavHatirlatma = 3,
        Genel = 99
    }
    public class Notification
    {
        public int Id { get; set; }

        // 🔑 Bildirimi alacak kullanıcı
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        // 🔔 Bildirim türü
        public NotificationType Type { get; set; }

        // 📝 Başlık (kısa)
        public string Title { get; set; }

        // 📄 Açıklama (detay)
        public string Description { get; set; }

        // 🔗 Opsiyonel ilişkiler
        public int? ExamId { get; set; }
        public Exam Exam { get; set; }

        public int? CourseId { get; set; }
        public Course Course { get; set; }

        // 👁 Okundu mu?
        public bool IsRead { get; set; } = false;

        // ⏰ Oluşturulma zamanı
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
