using Jira.Data;
using Jira.Models.Entities;
using Jira.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

namespace Jira
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // 1. ��������� ������� ������������ � Swagger
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();

            // 2. ��������� Swagger (JWT-������������)
            builder.Services.AddSwaggerGen(c =>
            {
                var securityScheme = new OpenApiSecurityScheme
                {
                    Name = "JWT Authentication",
                    Description = "Enter JWT Bearer token **_only_**",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    Reference = new OpenApiReference
                    {
                        Id = JwtBearerDefaults.AuthenticationScheme,
                        Type = ReferenceType.SecurityScheme
                    }
                };
                c.AddSecurityDefinition(securityScheme.Reference.Id, securityScheme);
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    { securityScheme, Array.Empty<string>() }
                });
            });

            // 3. ��������� �� (PostgreSQL)
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

            // 4. ��������� Identity (������ ���� �� ��������������)
            builder.Services.AddIdentity<User, IdentityRole>()
                .AddEntityFrameworkStores<AppDbContext>()
                .AddDefaultTokenProviders();

            // 5. ��������� JWT-��������������
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
                    ValidIssuer = builder.Configuration["Jwt:Issuer"],
                    ValidateAudience = true,
                    ValidAudience = builder.Configuration["Jwt:Audience"],
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
                };
            });

            // 6. ��������� ����������� (����� ��������������)
            builder.Services.AddAuthorization();

            // 7. ����������� ��������� �������� (��������, RoleService)
            builder.Services.AddScoped<RoleService>();

            // ==============================================
            // ������������ middleware (������� ��������!)
            // ==============================================
            var app = builder.Build();

            // 1. Swagger (������ ��� Development)
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            // 2. HTTPS-�������� (�����������)
            app.UseHttpsRedirection();

            // 3. ������������� (������ ���� ����� ���������������)
            app.UseRouting();

            // 4. �������������� (������ ���� ����� ������������)
            app.UseAuthentication();

            // 5. �����������
            app.UseAuthorization();

            // 6. ������������� ����� (����� UseRouting)
            using (var scope = app.Services.CreateScope())
            {
                var roleService = scope.ServiceProvider.GetRequiredService<RoleService>();
                roleService.CreateRoleIfNotExists("Admin").Wait(); // ����������� ����� � Wait()
                roleService.CreateRoleIfNotExists("User").Wait(); // ����������� ����� � Wait()
            }

            // 7. ������� ������������ (����� ���� middleware)
            app.MapControllers();

            app.Run();
        }
    }
}
