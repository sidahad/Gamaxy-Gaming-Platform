using GamaxyWebApplication.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using GamaxyWebApplication.Services;

namespace GamaxyWebApplication
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();
            builder.Services.AddHttpClient();
            builder.Services.AddSession();
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddScoped<EmailService>();

            // ✅ Add Cookie Authentication
            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = "/Home/Login"; // 👈 your login path
                });

            // ✅ Add Authorization
            builder.Services.AddAuthorization();

            // Your DB context setup
            //string cs = "Server=DESKTOP-L2D3A5N; Initial Catalog=cnjnnnmmmbbbbb; Persist Security Info=False; User ID = sa; Password = anousha0347; MultipleActiveResultSets = False; Encrypt = True; TrustServerCertificate = True; Connection Timeout = 30;";
            string cs = "Server=DESKTOP-EKI58B0\\SQLEXPRESS; Initial Catalog=Gamaxy1; Persist Security Info=False; User ID = se; Password = aptech; MultipleActiveResultSets = False; Encrypt = True; TrustServerCertificate = True; Connection Timeout = 30;";
            builder.Services.AddDbContext<ApplicationDbContext>(a => a.UseSqlServer(cs));

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

           
            app.UseSession();


            // ✅ Add authentication/authorization middleware
            app.UseAuthentication(); // 👈 required for [Authorize]
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
