// Program.cs
using Microsoft.EntityFrameworkCore;
using SimpleToDoAPI.Data;
using SimpleToDoAPI.Services.Interfaces;
using SimpleToDoAPI.Middleware;
using SimpleToDoAPI.Mapping;
using FluentValidation;
using FluentValidation.AspNetCore;

using Microsoft.AspNetCore.Mvc;
using SimpleToDoAPI.Helpers;
using SimpleToDoAPI.Services;

using Microsoft.AspNetCore.Identity;
using SimpleToDoAPI.Models;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using SimpleToDoAPI.Configurations;
using System.Text;
using SimpleToDoAPI.Services.Auth;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container
//builder.Services.AddControllers();


builder.Services.AddControllers();

builder.Services
    .AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();


// JWT Settings
builder.Services.Configure<JwtSettings>(
    builder.Configuration.GetSection("Jwt"));

        var jwtSettings =
            builder.Configuration
                   .GetSection("Jwt")
                   .Get<JwtSettings>();

        var key =
            Encoding.UTF8.GetBytes(jwtSettings!.Key);

        builder.Services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme =
                    JwtBearerDefaults.AuthenticationScheme;

                options.DefaultChallengeScheme =
                    JwtBearerDefaults.AuthenticationScheme;

                options.DefaultScheme =
                    JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;

                options.SaveToken = true;

                options.TokenValidationParameters =
                    new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,

                        ValidIssuer = jwtSettings.Issuer,

                        ValidAudience = jwtSettings.Audience,

                        IssuerSigningKey =
                            new SymmetricSecurityKey(key),

                        ClockSkew = TimeSpan.Zero
                    };
            });
///

// التكامل مع FluentValidation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddFluentValidationClientsideAdapters();

// تسجيل الـ Validators
builder.Services.AddValidatorsFromAssemblyContaining<Program>();


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddAutoMapper(typeof(MappingProfile));




 // Validations
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var errors = context.ModelState
            .Where(x => x.Value?.Errors.Count > 0)
            .Select(x => new
            {
                Field = x.Key,
                Errors = x.Value?.Errors
                    .Select(e => e.ErrorMessage)
            });

        var response = new ApiErrorResponse(
            "Validation Error",
            errors
        );

        return new BadRequestObjectResult(response);
    };
});


// تسجيل DbContext مع SQL Server
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions =>
        {
            sqlOptions.MigrationsAssembly(typeof(Program).Assembly.FullName);
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorNumbersToAdd: null);
        }));


/////////////////////////////////////////////////////////////////////////
///
// إي خدمة نقوم بإنشائها يجب تسجيلها هنا
// Register our custom service
builder.Services.AddScoped<ITodoService, TodoService>();
builder.Services.AddScoped<IAuthService, AuthService>();

/////////////////////////////////////////////////////////////////////////

var app = builder.Build();


// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    // 🔥 إضافة هذا الكود لإنشاء قاعدة البيانات والجداول
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        //// التأكد من وجود قاعدة البيانات
        //dbContext.Database.EnsureCreated();

        // أو إذا كنت تستخدم الترحيلات (Migrations)
        //dbContext.Database.Migrate();

        Console.WriteLine("✅ تم التحقق من قاعدة البيانات والجداول");
    }
}

app.UseMiddleware<ExceptionMiddleware>();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
