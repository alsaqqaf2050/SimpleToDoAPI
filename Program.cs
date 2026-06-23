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
using SimpleToDoAPI.Services.Common;
using Microsoft.OpenApi.Models;
using SimpleToDoAPI.Constants;
using Microsoft.AspNetCore.Authorization;
using SimpleToDoAPI.Authorization;

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

                // مؤقتاً
                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        Console.WriteLine(
                            context.Exception.Message);

                        return Task.CompletedTask;
                    }
                };

            });
///

// التكامل مع FluentValidation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddFluentValidationClientsideAdapters();

// تسجيل الـ Validators
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

// لإضافة الصلاحيات  Policies
// الفائدة من ال Polices جميع قواعد الصلاحيات تصبح في مكان مركزي واحد
// بدلا من ال Roles

//builder.Services.AddAuthorization(options =>
//{
//    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
//    options.AddPolicy("UserOnly", policy => policy.RequireRole("User"));
//    options.AddPolicy("AdminOrUser", policy => policy.RequireRole("Admin", "User"));
//});

//builder.Services.AddAuthorization(options =>
//{
//    options.AddPolicy(Policies.AdminOnly,policy => policy.RequireRole("Admin"));
//});

//builder.Services.AddAuthorization(options =>
//{
//    options.AddPolicy(
//        "Permissions.Todos.View",
//        policy =>
//        {
//            policy.Requirements.Add(
//                new PermissionRequirement(
//                    "Permissions.Todos.View"));
//        });
//});

//builder.Services.AddSingleton<IAuthorizationHandler, PermissionHandler>();

//بدل AddSingleton الأفضل  لأنه يعتمد على HttpContext/User per request.
builder.Services.AddScoped<IAuthorizationHandler, PermissionHandler>();

builder.Services.AddAuthorization(options =>
{

    // Todos

    options.AddPolicy(Policies.AdminOnly,policy => policy.RequireRole("Admin"));

    options.AddPolicy(Policies.TodoView, policy => policy.Requirements.Add(new PermissionRequirement(Permissions.Todos.View)));

    options.AddPolicy(Policies.TodoCreate,policy => policy.Requirements.Add(new PermissionRequirement(Permissions.Todos.Create)));

    options.AddPolicy(Policies.TodoUpdate,policy => policy.Requirements.Add(new PermissionRequirement(Permissions.Todos.Update)));

    options.AddPolicy(Policies.TodoDelete,policy => policy.Requirements.Add(new PermissionRequirement(Permissions.Todos.Delete)));


    // Users

    options.AddPolicy(
        Policies.UserView,
        policy => policy.Requirements.Add(
            new PermissionRequirement(
                Permissions.Users.View)));

    options.AddPolicy(
        Policies.UserCreate,
        policy => policy.Requirements.Add(
            new PermissionRequirement(
                Permissions.Users.Create)));

    options.AddPolicy(
        Policies.UserUpdate,
        policy => policy.Requirements.Add(
            new PermissionRequirement(
                Permissions.Users.Update)));

    options.AddPolicy(
        Policies.UserDelete,
        policy => policy.Requirements.Add(
            new PermissionRequirement(
                Permissions.Users.Delete)));


    // Roles

    options.AddPolicy(
        Policies.RoleView,
        policy => policy.Requirements.Add(
            new PermissionRequirement(
                Permissions.Roles.View)));

    options.AddPolicy(
        Policies.RoleCreate,
        policy => policy.Requirements.Add(
            new PermissionRequirement(
                Permissions.Roles.Create)));

    options.AddPolicy(
        Policies.RoleUpdate,
        policy => policy.Requirements.Add(
            new PermissionRequirement(
                Permissions.Roles.Update)));

    options.AddPolicy(
        Policies.RoleDelete,
        policy => policy.Requirements.Add(
            new PermissionRequirement(
                Permissions.Roles.Delete)));

    // Audit

    options.AddPolicy(
        Policies.AuditView,
        policy => policy.Requirements.Add(
            new PermissionRequirement(
                Permissions.Audit.View)));

});


///////

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Simple Todo API",
        Version = "v1"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "أدخل JWT Token"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

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

builder.Services.AddHttpContextAccessor();

/////////////////////////////////////////////////////////////////////////
///
// إي خدمة مرتبطة بواجهه نقوم بإنشائها يجب تسجيلها هنا
// Register our custom service
builder.Services.AddScoped<ITodoService, TodoService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ICurrentUserService,CurrentUserService>();

builder.Services.AddScoped<IRoleService, RoleService>();

builder.Services.AddScoped<IPermissionService, PermissionService>();
builder.Services.AddScoped<IRolePermissionService, RolePermissionService>();
builder.Services.AddScoped<IPermissionCacheService, PermissionCacheService>();

builder.Services.AddScoped<IAuditService, AuditService>();
/////////////////////////////////////////////////////////////////////////

var app = builder.Build();

// الخاص بمعالجة الأخطاء
app.UseMiddleware<ExceptionMiddleware>();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    //// 🔥 إضافة هذا الكود لإنشاء قاعدة البيانات والجداول
    //using (var scope = app.Services.CreateScope())
    //{
    //    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    //    //// التأكد من وجود قاعدة البيانات
    //    //dbContext.Database.EnsureCreated();

    //    // أو إذا كنت تستخدم الترحيلات (Migrations)
    //    //dbContext.Database.Migrate();

    //    Console.WriteLine("✅ تم التحقق من قاعدة البيانات والجداول");
    //}

    ///  استدعاء Seeder عند تشغيل التطبيق
    ///  أول جزء من نظام الصلاحيات
    //using (var scope = app.Services.CreateScope())
    //{
    //    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    //    await RoleSeeder.SeedRolesAsync(roleManager);
    //}

    ///  استدعاء Seeder عند تشغيل التطبيق
    ///  أول جزء من نظام الصلاحيات
    //using (var scope = app.Services.CreateScope())
    //{
    //    var services = scope.ServiceProvider;

    //    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

    //    await RoleSeeder.SeedRolesAsync(roleManager);

    //    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

    //    await AdminSeeder.SeedAdminAsync(userManager);

    //    var dbContext = services.GetRequiredService<ApplicationDbContext>();

    //    await PermissionSeeder.SeedPermissionsAsync(roleManager,dbContext);
    //}
    /////
}





app.UseHttpsRedirection();

app.UseAuthentication();

//// فحص الـ JWT + Permissions في Middleware واحد قبل دخول أي Controller
//app.UseMiddleware<PermissionMiddleware>();

app.UseAuthorization();

app.MapControllers();
//app.MapControllers().RequireAuthorization();

app.Run();
