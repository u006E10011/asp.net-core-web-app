using DevLog.Configuration;
using DevLog.Data;
using DevLog.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

namespace DevLog
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();
            builder.Services.Configure<AdminAuthOptions>(builder.Configuration.GetSection(AdminAuthOptions.SectionName));
            builder.Services.AddDbContext<DevLogDbContext>(options =>
                options.UseNpgsql(
                    builder.Configuration.GetConnectionString("DevLogConnection"),
                    npgsqlOptions => npgsqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(10),
                        errorCodesToAdd: null)));
            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = "/admin/login";
                    options.AccessDeniedPath = "/admin/login";
                });
            builder.Services.AddAuthorization();

            var app = builder.Build();

            DatabaseInitializationService.InitializeAsync(app.Services).GetAwaiter().GetResult();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapStaticAssets();
            app.MapGet("/health", () => Results.Ok(new { status = "ok" }));
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}")
                .WithStaticAssets();

            app.Run();
        }
    }
}
