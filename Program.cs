// Program.cs
using Microsoft.EntityFrameworkCore;
using SimpleTodoAPI.Data;
using SimpleTodoAPI.Services;
using SimpleTodoAPI.Services.Interfaces;
using SimpleTodoAPI.Middleware;
using SimpleTodoAPI.Mapping;
using FluentValidation;
using FluentValidation.AspNetCore;

using Microsoft.AspNetCore.Mvc;
using SimpleTodoAPI.Helpers;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container
//builder.Services.AddControllers();


builder.Services.AddControllers();

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

// Register our custom service
builder.Services.AddScoped<ITodoService, TodoService>();


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

        // التأكد من وجود قاعدة البيانات
        dbContext.Database.EnsureCreated();

        // أو إذا كنت تستخدم الترحيلات (Migrations)
         //dbContext.Database.Migrate();

        Console.WriteLine("✅ تم التحقق من قاعدة البيانات والجداول");
    }
}

app.UseMiddleware<ExceptionMiddleware>();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
