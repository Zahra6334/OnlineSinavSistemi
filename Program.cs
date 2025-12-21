using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OnlineSinavSistemi.Data;
using OnlineSinavSistemi.Models;
using OnlineSinavSistemi.Services;

var builder = WebApplication.CreateBuilder(args);

// Connection string - appsettings.json i�inde tan�mla
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// Identity
builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddControllersWithViews();

// Servis kayd� (sonraki ad�mda yazaca��m�z servis)
builder.Services.AddScoped<IExamService, ExamService>();
builder.Services.AddScoped<IStudentExamService, StudentExamService>();
builder.Services.AddScoped<IQuestionService, QuestionService>();
builder.Services.AddScoped<IAnswerService, AnswerService>();
builder.Services.AddScoped<IReminderService, ReminderService>();

var app = builder.Build();
// ------------------------------------------------------------------
// ROL OLUŞTURMA (SEED DATA) İŞLEMİ BURADA YAPILIYOR
// ------------------------------------------------------------------
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        // ÖNEMLİ: Bu çağrı, SeedData sınıfınızın ve InitializeRoles metodunuzun
        // doğru (public static async Task) tanımlanmış olduğunu varsayar.
        await SeedData.InitializeRoles(services);
    }
    catch (Exception ex)
    {
        // Hata oluşursa loglama yap
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Rollerin oluşturulması sırasında bir hata oluştu.");
    }
}
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages(); // Identity UI i�in

app.Run();