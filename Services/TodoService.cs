// Services/TodoService.cs
using SimpleTodoAPI.Data;
using SimpleTodoAPI.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SimpleTodoAPI.Services
{
    public interface ITodoService
    {
        Task<List<TodoItem>> GetAllAsync();
        Task<TodoItem?> GetByIdAsync(int id);
        Task<TodoItem> CreateAsync(TodoItem item);
        Task<TodoItem?> UpdateAsync(int id, TodoItem item);
        Task<bool> DeleteAsync(int id);
        //Task<List<TodoItem>?> SearchAsync(string keyword);
        //Task<List<TodoItem>?> GetCompletedAsync();
    }

    public class TodoService : ITodoService
    {
        // Dependency Injection //
        private readonly ApplicationDbContext _context;

        public TodoService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<TodoItem>> GetAllAsync()
        {
            return await _context.TodoItems.OrderByDescending(x => x.CreatedDate).ToListAsync();
        }

        public async Task<TodoItem?> GetByIdAsync(int id)
        {
            return await _context.TodoItems.FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<TodoItem> CreateAsync(TodoItem item)
        {
            try
            {
                item.CreatedDate = DateTime.UtcNow;

                _context.TodoItems.Add(item);
                await _context.SaveChangesAsync();

                return item;
            }
            catch (Exception ex)
            {
                // سجل الخطأ للتصحيح
                Console.WriteLine($"❌ خطأ في إنشاء المهمة: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"❗ خطأ داخلي: {ex.InnerException.Message}");
                }
                throw;
            }
        }

        public async Task<TodoItem?> UpdateAsync(int id, TodoItem item)
        {
            var existingItem = await GetByIdAsync(id);
            if (existingItem == null)
                return null;

            // تحديث الحقول فقط إذا تم إرسالها
            if (!string.IsNullOrEmpty(item.Title))
                existingItem.Title = item.Title;

            if (!string.IsNullOrEmpty(item.Description))
                existingItem.Description = item.Description;

            existingItem.IsCompleted = item.IsCompleted;
            existingItem.UpdatedDate = DateTime.UtcNow;

            _context.TodoItems.Update(existingItem);
            await _context.SaveChangesAsync();

            return existingItem;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var item = await GetByIdAsync(id);
            if (item == null)
                return false;

            _context.TodoItems.Remove(item);
            await _context.SaveChangesAsync();

            return true;
        }

        //// دالة إضافية للبحث
        //public async Task<List<TodoItem>> SearchAsync(string keyword)
        //{
        //    return await _context.TodoItems.Where(x => x.Title.Contains(keyword) || x.Description.Contains(keyword)).ToListAsync();
        //}

        //// دالة إضافية للحصول على المهام المكتملة
        //public async Task<List<TodoItem>> GetCompletedAsync()
        //{
        //    return await _context.TodoItems.Where(x => x.IsCompleted).ToListAsync();
        //}
    }
}