using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using MusicStream.Data;
using MusicStream.Services;
using Microsoft.Extensions.FileProviders;
using System.IO;

namespace MusicStream
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            // Add services R2service
            builder.Services.AddSingleton<R2Service>();

            // Add services to the container.
            builder.Services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
                });

            // Database Context
            builder.Services.AddDbContext<MusicStreamContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            // JWT Authentication
            var jwtSettings = builder.Configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["SecretKey"] ?? "YourSuperSecretKeyThatIsAtLeast32CharactersLong!@#$%";

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings["Issuer"] ?? "MusicStreamAPI",
                    ValidAudience = jwtSettings["Audience"] ?? "MusicStreamClient",
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
                };
            });

            // CORS Configuration
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAngular", policy =>
                {
                    policy.WithOrigins("http://localhost:4200")
                          .AllowAnyHeader()
                          .AllowAnyMethod()
                          .AllowCredentials();
                });
            });

            var app = builder.Build();

            // Seed Database
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    var context = services.GetRequiredService<MusicStreamContext>();
                    
                    // Ensure database is created and migrated
                    context.Database.EnsureCreated();
                    
                    // Seed initial data
                    DatabaseSeeder.SeedData(context);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An error occurred while seeding the database: {ex.Message}");
                }
            }

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            var localImagesPath = Path.Combine(builder.Environment.ContentRootPath, "images");
            if (Directory.Exists(localImagesPath))
            {
                app.UseStaticFiles(new StaticFileOptions
                {
                    FileProvider = new PhysicalFileProvider(localImagesPath),
                    RequestPath = "/images"
                });
            }

            app.UseRouting();

            // Enable CORS
            app.UseCors("AllowAngular");

            // Authentication & Authorization
            app.UseAuthentication();
            app.UseAuthorization();

            // Chỉ sử dụng API controllers (không dùng MVC views)
            app.MapControllers();

            // Root endpoint - trả về thông báo API
            app.MapGet("/", () => Results.Ok(new 
            { 
                message = "Music Stream API is running",
                version = "1.0.0",
                endpoints = new 
                {
                    login = "/api/auth/login",
                    register = "/api/auth/register"
                }
            }));

            app.Run();
        }
    }
}
