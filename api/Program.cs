// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle

using api.Data;
using api.DTO.Model;
using api.DTO.Sample;
using api.Managers.Jobs;
using api.Services;
using Microsoft.EntityFrameworkCore;

namespace api
{
    public class Program
    {
        //  Constants
        public const string ConnectionStringKey = "DefaultConnection";

        //  Methods
        public static void Main(string[] args)
        {
            //  Create builder to configure web application
            WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

            // Add services to the container
            ConfigureServices(builder);

            //  Build web application
            WebApplication app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseCors("AllowSpecificOrigin");

            //  Add authentication and authorization middleware
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();
            app.Run();
        }
        private static void ConfigureServices(WebApplicationBuilder builder)
        {
            //  CORS configuration
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowSpecificOrigin", builder =>
                {
                    builder.WithOrigins("http://localhost:3000").AllowAnyMethod().AllowAnyHeader();
                });
            });

            //  Services
            builder.Services.AddHostedService<ModelGenerationService>();
            builder.Services.AddScoped<IWebCrawlerService, WebCrawlerService>();
            builder.Services.AddScoped<ICleanupService, CleanupService>();
                        
            //  Singletons
            builder.Services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();
            builder.Services.AddSingleton<JobStatusManager<SampleDTO>>();
            builder.Services.AddSingleton<JobStatusManager<ModelDTO>>();

            //  Other
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddControllers();
            builder.Services.AddDbContextFactory<ApplicationDBContext>(options => options.UseSqlite("Data Source=app.db"));

            //  Authentication
            builder.Services.AddAuthentication().AddJwtBearer();
        }
    }
}