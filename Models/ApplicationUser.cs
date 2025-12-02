using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace OnlineSinavSistemi.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string full_name { get; set; }
        public string Rol { get; set; } // "Ogretmen" veya "Ogrenci"
        public string Numara { get; set; } // öğrenci numarası (opsiyonel)
        public string Brans { get; set; } // öğretmen branşı (opsiyonel)
        public string? ProfileImage { get; set; }
        public ICollection<StudentExam> StudentExams { get; set; }
    }
}