// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle

using api.Data;
using api.DTO.Sample;
using api.Managers.Jobs;
using api.Services;
using Microsoft.EntityFrameworkCore;

namespace backend
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
            app.MapControllers();
            app.UseCors("AllowSpecificOrigin");
            app.UseAuthorization();

            app.Run();
        }
        private static void ConfigureServices(WebApplicationBuilder builder)
        {
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowSpecificOrigin", builder =>
                {
                    builder.WithOrigins("http://localhost:3000").AllowAnyMethod().AllowAnyHeader();
                });
            });
            builder.Services.AddHostedService<ModelGenerationService>();
            builder.Services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();
            builder.Services.AddSingleton<JobStatusManager<SampleDTO>>();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddScoped<IWebCrawlerService, WebCrawlerService>();
            builder.Services.AddControllers();
            builder.Services.AddDbContextFactory<ApplicationDBContext>(options =>
            {
                options.UseSqlServer(builder.Configuration.GetConnectionString(ConnectionStringKey));
            });
            builder.Services.AddAuthentication().AddJwtBearer();

            // builder.Services.AddAuthentication(options =>
            // {
            //     options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            //     options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            // })
            // .AddJwtBearer(options =>
            // {
            //     options.TokenValidationParameters = new TokenValidationParameters
            //     {
            //         ValidateIssuer = true,
            //         ValidateAudience = true,
            //         ValidateLifetime = true,
            //         ValidateIssuerSigningKey = true,
            //         ValidIssuer = "*",
            //         ValidAudience = "*",
            //         IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("YourSecretKey"))
            //     };
            // });
        }
    }
}