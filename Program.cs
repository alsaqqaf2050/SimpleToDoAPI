// Program.cs
using Microsoft.EntityFrameworkCore;
using SimpleTodoAPI.Data;
using SimpleTodoAPI.Services;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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


app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
