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
using SimpleToDoAPI.Infrastructure.Audit;
using SimpleToDoAPI.Data.Seeders;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// ─── Serilog ──────────────────────────────────────────────────
builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration));

// ─── Caching ──────────────────────────────────────────────────
builder.Services.AddMemoryCache();

// ─── Controllers ──────────────────────────────────────────────
builder.Services.AddControllers();

// ─── Identity ─────────────────────────────────────────────────
builder.Services
    .AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// ─── JWT ──────────────────────────────────────────────────────
builder.Services.Configure<JwtSettings>(
    builder.Configuration.GetSection("Jwt"));

var jwtSettings = builder.Configuration
    .GetSection("Jwt")
    .Get<JwtSettings>();

var key = Encoding.UTF8.GetBytes(jwtSettings!.Key);

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ClockSkew = TimeSpan.Zero
        };
    });

// ─── FluentValidation ─────────────────────────────────────────
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddFluentValidationClientsideAdapters();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

// ─── Authorization + Permission Policies ──────────────────────
builder.Services.AddScoped<IAuthorizationHandler, PermissionHandler>();

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(Policies.AdminOnly, policy => policy.RequireRole("Admin"));

    // Todos
    options.AddPolicy(Policies.TodoView,   policy => policy.Requirements.Add(new PermissionRequirement(Permissions.Todos.View)));
    options.AddPolicy(Policies.TodoCreate, policy => policy.Requirements.Add(new PermissionRequirement(Permissions.Todos.Create)));
    options.AddPolicy(Policies.TodoUpdate, policy => policy.Requirements.Add(new PermissionRequirement(Permissions.Todos.Update)));
    options.AddPolicy(Policies.TodoDelete, policy => policy.Requirements.Add(new PermissionRequirement(Permissions.Todos.Delete)));

    // Users
    options.AddPolicy(Policies.UserView,   policy => policy.Requirements.Add(new PermissionRequirement(Permissions.Users.View)));
    options.AddPolicy(Policies.UserCreate, policy => policy.Requirements.Add(new PermissionRequirement(Permissions.Users.Create)));
    options.AddPolicy(Policies.UserUpdate, policy => policy.Requirements.Add(new PermissionRequirement(Permissions.Users.Update)));
    options.AddPolicy(Policies.UserDelete, policy => policy.Requirements.Add(new PermissionRequirement(Permissions.Users.Delete)));

    // Roles
    options.AddPolicy(Policies.RoleView,   policy => policy.Requirements.Add(new PermissionRequirement(Permissions.Roles.View)));
    options.AddPolicy(Policies.RoleCreate, policy => policy.Requirements.Add(new PermissionRequirement(Permissions.Roles.Create)));
    options.AddPolicy(Policies.RoleUpdate, policy => policy.Requirements.Add(new PermissionRequirement(Permissions.Roles.Update)));
    options.AddPolicy(Policies.RoleDelete, policy => policy.Requirements.Add(new PermissionRequirement(Permissions.Roles.Delete)));

    // Audit
    options.AddPolicy(Policies.AuditView, policy => policy.Requirements.Add(new PermissionRequirement(Permissions.Audit.View)));
});

// ─── CORS ─────────────────────────────────────────────────────
builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendPolicy", policy =>
    {
        policy
            .WithOrigins(
                "http://localhost:5173",   // Vite dev server
                "http://localhost:3000",   // React dev server (alternative)
                "http://localhost:4173"    // Vite preview
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// ─── Rate Limiting ────────────────────────────────────────────
builder.Services.AddRateLimiter(options =>
{
    // حد عام: 100 طلب/دقيقة لكل IP
    options.AddFixedWindowLimiter("GlobalPolicy", opt =>
    {
        opt.Window = TimeSpan.FromMinutes(1);
        opt.PermitLimit = 100;
        opt.QueueLimit = 0;
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
    });

    // حد صارم لـ Auth endpoints: 10 محاولات/دقيقة (حماية من Brute Force)
    options.AddFixedWindowLimiter("AuthPolicy", opt =>
    {
        opt.Window = TimeSpan.FromMinutes(1);
        opt.PermitLimit = 10;
        opt.QueueLimit = 0;
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
    });

    options.RejectionStatusCode = 429;
});

// ─── Swagger ──────────────────────────────────────────────────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Simple Todo API",
        Version = "v1",
        Description = "REST API احترافي لإدارة المهام مع نظام صلاحيات متقدم"
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

// ─── AutoMapper ───────────────────────────────────────────────
builder.Services.AddAutoMapper(typeof(MappingProfile));

// ─── Audit Interceptor ────────────────────────────────────────
builder.Services.AddScoped<AuditSaveChangesInterceptor>();

// ─── Validation Response Factory ──────────────────────────────
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var errors = context.ModelState
            .Where(x => x.Value?.Errors.Count > 0)
            .Select(x => new
            {
                Field = x.Key,
                Errors = x.Value?.Errors.Select(e => e.ErrorMessage)
            });

        var response = new ApiErrorResponse("Validation Error", errors);
        return new BadRequestObjectResult(response);
    };
});

// ─── DbContext ────────────────────────────────────────────────
builder.Services.AddDbContext<ApplicationDbContext>(
    (serviceProvider, options) =>
    {
        options.UseSqlServer(
            builder.Configuration.GetConnectionString("DefaultConnection"),
            sqlOptions =>
            {
                sqlOptions.MigrationsAssembly(typeof(Program).Assembly.FullName);
                sqlOptions.EnableRetryOnFailure(5, TimeSpan.FromSeconds(30), null);
            });

        options.AddInterceptors(
            serviceProvider.GetRequiredService<AuditSaveChangesInterceptor>());
    });

builder.Services.AddHttpContextAccessor();

// ─── Health Checks ────────────────────────────────────────────
builder.Services.AddHealthChecks()
    .AddSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")!);

// ─── Application Services ─────────────────────────────────────
builder.Services.AddScoped<ITodoService, TodoService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<IPermissionService, PermissionService>();
builder.Services.AddScoped<IRolePermissionService, RolePermissionService>();
builder.Services.AddScoped<IPermissionCacheService, PermissionCacheService>();
builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IUserService, UserService>();

// ═══════════════════════════════════════════════════════════════
var app = builder.Build();
// ═══════════════════════════════════════════════════════════════

// ─── Database Seeding (تشغيل تلقائي عند بدء التطبيق) ─────────
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var dbContext = services.GetRequiredService<ApplicationDbContext>();

        // تطبيق أي Migrations معلقة تلقائياً
        await dbContext.Database.MigrateAsync();

        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

        // Seed Roles → Admin User → Permissions
        await RoleSeeder.SeedRolesAsync(roleManager);
        await AdminSeeder.SeedAdminAsync(userManager);
        await PermissionSeeder.SeedPermissionsAsync(roleManager, dbContext);

        Console.WriteLine("✅ تم تهيئة قاعدة البيانات والبيانات الأولية بنجاح");
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "خطأ أثناء تهيئة قاعدة البيانات: {Message}", ex.Message);
    }
}

// ─── Middleware Pipeline ───────────────────────────────────────
app.UseMiddleware<ExceptionMiddleware>();

app.UseSerilogRequestLogging();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Simple Todo API v1");
        c.RoutePrefix = "swagger";
    });
}

app.UseHttpsRedirection();

// CORS يجب أن يكون قبل Authentication
app.UseCors("FrontendPolicy");

app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

// Health Check endpoint
app.MapHealthChecks("/health");

app.MapControllers();

app.Run();
