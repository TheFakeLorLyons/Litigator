using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.SqlServer;
using System.Text.Json.Serialization;
using Litigator.Controllers;
using Litigator.DataAccess.Data;
using Litigator.Models.Mapping;
using Litigator.Services.Interfaces;
using Litigator.Services.Implementations;

namespace Litigator
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services
            builder.Services.AddDbContext<LitigatorDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            // Register business logic services
            builder.Services.AddScoped<ICaseService, CaseService>();
            builder.Services.AddScoped<IClientService, ClientService>();
            builder.Services.AddScoped<IAttorneyService, AttorneyService>();
            builder.Services.AddScoped<IDeadlineService, DeadlineService>();
            builder.Services.AddScoped<IDocumentService, DocumentService>();
            builder.Services.AddScoped<ICourtService, CourtService>();
            builder.Services.AddScoped<IAnalyticsService, AnalyticsService>();


            builder.Services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
                options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
            });

            builder.Services.AddAutoMapper(typeof(LitigatorMappingProfile));

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAngular", policy =>
                {
                    policy.WithOrigins("http://localhost:4200")
                          .AllowAnyHeader()
                          .AllowAnyMethod();
                });
            });

            var app = builder.Build();

            // Seed data 
            using (var scope = app.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<LitigatorDbContext>();
                context.Database.EnsureCreated();
                DbSeeder.SeedData(context, app.Environment.IsDevelopment());
            }

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseCors("AllowAngular");
            app.UseHttpsRedirection();
            app.UseAuthorization();
            app.MapControllers();

            app.Run();
        }
    }
}
