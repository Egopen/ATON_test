using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using UserApp.Application.Services.Implementations;
using UserApp.Application.Services.Interfaces;
using UserApp.Infrastructure.DB;
using UserApp.Infrastructure.Features;
using UserApp.Infrastructure.Services.implementations;
using UserApp.Infrastructure.Services.Interfaces;
using UserApp.Infrastructure.Settings;
namespace UserApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                EnvReader.Load(".env");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            var builder = WebApplication.CreateBuilder(args);

            Log.Logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] ({RequestId}) {Message:lj}{NewLine}{Exception}")
                .CreateLogger();

            builder.Host.UseSerilog();
            builder.Services.AddDbContext<DBContext>();
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer("Access", options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ClockSkew = TimeSpan.FromHours(2),
                        ValidateIssuer = true,
                        ValidIssuer = AuthOptions.ISSUER,
                        ValidateAudience = true,
                        ValidAudience = AuthOptions.AUDIENCE,
                        ValidateLifetime = true,
                        IssuerSigningKey = AuthOptions.GetSymSecurityKey(),
                        ValidateIssuerSigningKey = true,
                    };
                });
            builder.Services.AddAuthentication("Cookies");
            builder.Services.AddAuthorization();
            builder.Services.AddScoped<ITokenService, TokenService>();
            builder.Services.AddScoped<IHashService, HashService>();
            builder.Services.AddScoped<IUserRepository, UserRepository>();
            builder.Services.AddScoped<IAdminService, AdminService>();
            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<IAuthService, AuthService>();

            ///
            /// Authorization
            ///

            builder.Services.AddCors();
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
                {
                    Title = "Auth Service API",
                    Version = "v1",
                    Description = "API"
                });


                c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    Description = "Text JWT token here"
                });
                c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
                {
                    {
                        new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                        {
                            Reference = new Microsoft.OpenApi.Models.OpenApiReference
                            {
                                Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });

                c.OrderActionsBy((apiDesc) => $"{apiDesc.HttpMethod} {apiDesc.RelativePath}");
            });
            var app = builder.Build();
            app.UseMiddleware<RequestLoggingMiddleware>();
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            app.UseCors(builder => builder.AllowAnyOrigin().AllowAnyHeader());
            app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
