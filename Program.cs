using Microsoft.EntityFrameworkCore;
using NuGet.Protocol.Plugins;
using SearchEngineMVC.Models;
using SearchEngineMVC.Repository;
using StackExchange.Redis;

namespace SearchEngineMVC
{
    public class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                var builder = WebApplication.CreateBuilder(args);

                // Add services to the container.
                builder.Services.AddControllersWithViews();
                builder.Services.AddDbContext<AppDbContext>(options => {
                    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
                });
                builder.Services.AddScoped<ISearchIndexRepository, SearchIndexRepository>();
                //builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect("redis:6379"));

                var app = builder.Build();
                using (var scope = app.Services.CreateScope())
                {
                    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                    var indexPath = Path.Combine(Directory.GetCurrentDirectory(), "Data", "inverted_index.txt");
                    var importer = new ImportService(db, indexPath);
                    importer.RunImport();
                }

                // Configure the HTTP request pipeline.
                if (!app.Environment.IsDevelopment())
                {
                    app.UseExceptionHandler("/Home/Error");
                }
                app.UseStaticFiles();

                app.UseRouting();

                app.UseAuthorization();

                app.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=SearchIndex}/{action=Index}/{id?}");


                app.Run();
            }
            catch(Exception ex)
            {
                Console.WriteLine("Application startup failed: " + ex.Message);
                Console.WriteLine(ex.StackTrace);
                throw;

            }
            
        }
    }
}
