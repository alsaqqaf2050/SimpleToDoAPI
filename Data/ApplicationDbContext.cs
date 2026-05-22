// Data/ApplicationDbContext.cs
using Microsoft.EntityFrameworkCore;
using SimpleTodoAPI.Models;

namespace SimpleTodoAPI.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        
        // تمثل الجدول في قاعدة البيانات
        public DbSet<TodoItem> TodoItems { get; set; }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // تكوين الجدول
            modelBuilder.Entity<TodoItem>(entity =>
            {
                entity.ToTable("TodoItems"); // اسم الجدول
                
                entity.HasKey(e => e.Id); // المفتاح الأساسي

                // تكوين Id ليكون Identity (يتزايد تلقائياً)
                entity.Property(e => e.Id).ValueGeneratedOnAdd(); // ← هذا يحل المشكلة

                entity.Property(e => e.Title).IsRequired().HasMaxLength(100);
                
                entity.Property(e => e.Description).HasMaxLength(500);
                
                entity.Property(e => e.CreatedDate).IsRequired().HasDefaultValueSql("GETUTCDATE()"); // قيمة افتراضية
                
                entity.Property(e => e.UpdatedDate);
                
                // إنشاء فهرس للحقل Title لتحسين أداء البحث
                entity.HasIndex(e => e.Title).HasDatabaseName("IX_TodoItems_Title");
                
                // إنشاء فهرس للحقل IsCompleted
                entity.HasIndex(e => e.IsCompleted).HasDatabaseName("IX_TodoItems_IsCompleted");
            });
            
            // إضافة بيانات أولية (Seed Data)
            //modelBuilder.Entity<TodoItem>().HasData(
            //    new TodoItem 
            //    { 
            //        Id = 1, 
            //        Title = "تعل",
            //        Description = "إنشاء Web API مع قاعدة البيانات",
            //        IsCompleted = false
            //    },
            //    new TodoItem 
            //    { 
            //        Id = 2, 
            //        Title = "تعلم Entity Framework Core",
            //        Description = "كيفية التعامل مع SQL Server",
            //        IsCompleted = false
            //    }
            //);
        }
    }
}