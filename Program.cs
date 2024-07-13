// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle

using api.data;
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
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddControllers();
            builder.Services.AddDbContext<ApplicationDBContext>(options =>
            {
                options.UseSqlServer(builder.Configuration.GetConnectionString(ConnectionStringKey));
            });

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

            app.Run();
        }
    }
}