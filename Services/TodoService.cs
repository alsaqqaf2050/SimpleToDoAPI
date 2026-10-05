using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using SimpleToDoAPI.Data;
using SimpleToDoAPI.DTOs;
using SimpleToDoAPI.Models;
using SimpleToDoAPI.Services.Interfaces;
using SimpleToDoAPI.Helpers;
using AutoMapper;
using SimpleToDoAPI.Services.Common;

namespace SimpleToDoAPI.Services
{
    public class TodoService : ITodoService
    {
        private readonly ApplicationDbContext _context;

        // Dependency Injection
        private readonly ILogger<TodoService> _logger;
        private readonly IMapper _mapper;
        private readonly ICurrentUserService _currentUser;
        private readonly IMemoryCache _cache;

        public TodoService(ApplicationDbContext context, ILogger<TodoService> logger, IMapper mapper, ICurrentUserService currentUser, IMemoryCache cache)
        {
            _context = context;
            _logger = logger;
            _mapper = mapper;
            _currentUser = currentUser;
            _cache = cache;
        }

        // ================================
        // Get All
        // ================================


        //public async Task<IEnumerable<TodoResponseDto>> GetAllAsync()
        //{
        //    var items = await _context.TodoItems
        //        .OrderByDescending(x => x.CreatedDate)
        //        .ToListAsync();

        //    return items.Select(MapToResponseDto);
        //}

        public async Task<PagedResult<TodoResponseDto>> GetAllAsync(TodoQueryParametersDto parameters)
        {
            var query = _context.TodoItems.AsQueryable();

            // =====================================
            // Just CurrentUser Tasks or Assigned Tasks
            // =====================================
            query = query.Where(x => x.UserId == _currentUser.UserId || x.AssignedToUserId == _currentUser.UserId);

            // =====================================
            // Search
            // =====================================
            if (!string.IsNullOrWhiteSpace(parameters.Search))
            {
                var search = parameters.Search.Trim();

                query = query.Where(x =>
                    x.Title.Contains(search) ||
                    x.Description.Contains(search));
            }

            // =====================================
            // Filter
            // =====================================
            if (parameters.Completed.HasValue)
            {
                query = query.Where(x =>
                    x.IsCompleted == parameters.Completed.Value);
            }

            // =====================================
            // Total Count
            // =====================================
            var totalRecords = await query.CountAsync();

            // =====================================
            // Sorting
            // =====================================
            query = query.OrderByDescending(x => x.CreatedDate);

            // =====================================
            // Pagination & Includes
            // =====================================
            var items = await query
                .Include(x => x.AssignedToUser)
                .Skip((parameters.PageNumber - 1) * parameters.PageSize)
                .Take(parameters.PageSize)
                .ToListAsync();

            return new PagedResult<TodoResponseDto>
            {
                //// الـ Mapping اليدوي
                //Items = items.Select(MapToResponseDto),

                // تنظيف الـ Mapping اليدوي
                Items = _mapper.Map<IEnumerable<TodoResponseDto>>(items),
                TotalRecords = totalRecords,
                PageNumber = parameters.PageNumber,
                PageSize = parameters.PageSize
            };
        }

        // ================================
        // Get By Id
        // ================================
        public async Task<TodoResponseDto?> GetByIdAsync(int id)
        {
            var item = await _context.TodoItems
                .Include(x => x.AssignedToUser)
                .FirstOrDefaultAsync(x => x.Id == id && (x.UserId == _currentUser.UserId || x.AssignedToUserId == _currentUser.UserId));

            if (item == null)
                return null;

            //// الـ Mapping اليدوي
            //return MapToResponseDto(item);


            // تنظيف الـ Mapping اليدوي
            return _mapper.Map<TodoResponseDto>(item);
        }

        // ================================
        // Create
        // ================================
        public async Task<TodoResponseDto> CreateAsync(CreateTodoDto dto)
        {
            var item = _mapper.Map<TodoItem>(dto);
            item.CreatedDate = DateTime.UtcNow;

            if (string.IsNullOrWhiteSpace(_currentUser.UserId))
                throw new UnauthorizedAccessException();

            item.UserId = _currentUser.UserId;
            
            // تعيين المهمة للمستخدم الحالي إذا لم يتم تحديد مستخدم آخر
            if (string.IsNullOrWhiteSpace(item.AssignedToUserId))
            {
                item.AssignedToUserId = _currentUser.UserId;
            }

            _context.TodoItems.Add(item);

            await _context.SaveChangesAsync();
            
            InvalidateDashboardCache(item);

            return _mapper.Map<TodoResponseDto>(item);
        }

        // ================================
        // Update
        // ================================
        public async Task<TodoResponseDto?> UpdateAsync(int id, UpdateTodoDto dto)
        {
            var item = await _context.TodoItems
                .Include(x => x.AssignedToUser)
                .FirstOrDefaultAsync(x => x.Id == id && (x.UserId == _currentUser.UserId || x.AssignedToUserId == _currentUser.UserId));

            if (item == null)
                return null;

            _mapper.Map(dto, item);


            item.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            
            InvalidateDashboardCache(item);

            return _mapper.Map<TodoResponseDto>(item);
        }

        // ================================
        // Delete
        // ================================
        public async Task<bool> DeleteAsync(int id)
        {
            var item = await _context.TodoItems
                .FirstOrDefaultAsync(x => x.Id == id && x.UserId == _currentUser.UserId); // Only creator can delete

            if (item == null)
                return false;

            _context.TodoItems.Remove(item);

            await _context.SaveChangesAsync();
            
            InvalidateDashboardCache(item);

            return true;
        }
        
        private void InvalidateDashboardCache(TodoItem item)
        {
            _cache.Remove("dashboard_stats_admin");
            _cache.Remove($"dashboard_stats_{item.UserId}");
            if (!string.IsNullOrEmpty(item.AssignedToUserId) && item.AssignedToUserId != item.UserId)
            {
                _cache.Remove($"dashboard_stats_{item.AssignedToUserId}");
            }
        }

        // ================================
        // Mapper  // لم نعد نحتاجها لانها لان ال MAPPING لم يعد يدويا إنا اليا بإستخدام مكتبة 
        // Install-Package AutoMapper.Extensions.Microsoft.DependencyInjection
        // ================================
        //private static TodoResponseDto MapToResponseDto(TodoItem item)
        //{
        //    return new TodoResponseDto
        //    {
        //        Id = item.Id,
        //        Title = item.Title,
        //        Description = item.Description,
        //        IsCompleted = item.IsCompleted,
        //        CreatedDate = item.CreatedDate,
        //        UpdatedDate = item.UpdatedDate
        //    };
        //}
    }
}