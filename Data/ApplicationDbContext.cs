// Data/ApplicationDbContext.cs
using Microsoft.EntityFrameworkCore;
using SimpleToDoAPI.Models;

using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace SimpleToDoAPI.Data
{

    public class ApplicationDbContext: IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext( DbContextOptions<ApplicationDbContext> options): base(options)
        {
        }

        // تمثل الجدول في قاعدة البيانات
        public DbSet<TodoItem> TodoItems { get; set; }

        public DbSet<Permission> Permissions { get; set; }

        public DbSet<RolePermission> RolePermissions { get; set; }

        public DbSet<RefreshToken> RefreshTokens { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // إنشاء جدول المهام

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

                // العلاقة مع المستخدم

                //entity.Property(e => e.UserId).IsRequired();
                entity.HasOne(e => e.User).WithMany(u => u.Todos).HasForeignKey(e => e.UserId).OnDelete(DeleteBehavior.Cascade);
            });


            // إنشاء جدول أدوار الصلاحيات

            //modelBuilder.Entity<RolePermission>().HasKey(x => new {x.RoleId, x.PermissionId});
            //modelBuilder.Entity<RolePermission>().HasOne(x => x.Role).WithMany().HasForeignKey(x => x.RoleId);

            modelBuilder.Entity<RolePermission>(entity =>
            {
                entity.ToTable("RolePermissions");

                entity.HasKey(x => new { x.RoleId, x.PermissionId });
                entity.HasOne(x => x.Role).WithMany().HasForeignKey(x => x.RoleId);
                entity.HasOne(x => x.Permission).WithMany(x => x.RolePermissions).HasForeignKey(x => x.PermissionId);
            });

            // إنشاء جدول الصلاحيات
            //modelBuilder.Entity<Permission>().HasIndex(x => x.Name).IsUnique();

            modelBuilder.Entity<Permission>(entity =>
            {
                entity.ToTable("Permissions");

                entity.HasIndex(x => x.Name).IsUnique();
                entity.HasData(

                new Permission
                {
                    Id = 1,
                    Name = "Permissions.Todos.View",
                    Description = "View Todos"
                },

                new Permission
                {
                    Id = 2,
                    Name = "Permissions.Todos.Create",
                    Description = "Create Todos"
                },

                new Permission
                {
                    Id = 3,
                    Name = "Permissions.Todos.Update",
                    Description = "Update Todos"
                },

                new Permission
                {
                    Id = 4,
                    Name = "Permissions.Todos.Delete",
                    Description = "Delete Todos"
                },

                new Permission
                {
                    Id = 5,
                    Name = "Permissions.Users.View",
                    Description = "View Users"
                },

                new Permission
                {
                    Id = 6,
                    Name = "Permissions.Users.Create",
                    Description = "Create Users"
                },

                new Permission
                {
                    Id = 7,
                    Name = "Permissions.Users.Update",
                    Description = "Update Users"
                },

                new Permission
                {
                    Id = 8,
                    Name = "Permissions.Users.Delete",
                    Description = "Delete Users"
                }

            );
            });


            // إنشاء جدول Refersh Token

            modelBuilder.Entity<RefreshToken>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.Property(x => x.Token).IsRequired();
                entity.HasOne(x => x.User).WithMany(x => x.RefreshTokens).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
            });


        }
    }
}