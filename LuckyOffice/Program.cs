using LuckyOffice.Utility;
using System.Diagnostics;

namespace LuckyOffice
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // var port = SocketHelper.GetAvailablePort(5000);

            var builder = WebApplication.CreateBuilder(args);

            // // Apply the available port to the server URL — this is essential
            // if (builder.Environment.IsProduction())
            // {
            //     builder.WebHost.UseUrls($"http://localhost:{port}");
            // }

            // Add services
            builder.Services.AddControllersWithViews();
            builder.Services.AddHttpClient();

            var app = builder.Build();

            // Middleware pipeline
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
                app.UseHttpsRedirection();
            }

            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            // Open browser only in production
            // if (app.Environment.IsProduction())
            // {
            //     app.Lifetime.ApplicationStarted.Register(() =>
            //     {
            //         var url = $"http://localhost:{port}";
            //         Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
            //     });
            // }

            app.Run();
        }
    }
}
