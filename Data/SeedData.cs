using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

    public static class SeedData
    {
        // Bu metot, program çalışırken rollerin varlığını kontrol eder ve ekler.
        public static async Task InitializeRoles(IServiceProvider serviceProvider)
        {
            // Gerekli servisleri DI konteynırından alıyoruz
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            // Eklemek istediğiniz roller
            string[] roleNames = new[] { "OGRENCI", "OGRETMEN" };

            foreach (var roleName in roleNames)
            {
                // Eğer rol veritabanında (AspNetRoles tablosu) yoksa
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    // Yeni rolü oluştur
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }
        }
    }
