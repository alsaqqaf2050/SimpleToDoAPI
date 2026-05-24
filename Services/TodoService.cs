using Microsoft.EntityFrameworkCore;
using SimpleTodoAPI.Data;
using SimpleTodoAPI.DTOs;
using SimpleTodoAPI.Models;
using SimpleTodoAPI.Services.Interfaces;
using SimpleTodoAPI.DTOs;
using SimpleTodoAPI.Helpers;
using AutoMapper;

namespace SimpleTodoAPI.Services
{
    public class TodoService : ITodoService
    {
        private readonly ApplicationDbContext _context;

        // Dependency Injection
        private readonly ILogger<TodoService> _logger;
        private readonly IMapper _mapper;

        //public TodoService(ApplicationDbContext context,ILogger<TodoService> logger)
        //{
        //    _context = context;
        //    _logger = logger;
        //}

        public TodoService(ApplicationDbContext context,ILogger<TodoService> logger,IMapper mapper)
        {
            _context = context;
            _logger = logger;
            _mapper = mapper;
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
            // Pagination
            // =====================================
            var items = await query
                .Skip((parameters.PageNumber - 1)
                    * parameters.PageSize)
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
                .FirstOrDefaultAsync(x => x.Id == id);

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
            //// الـ Mapping اليدوي
            //var item = new TodoItem
            //{
            //    Title = dto.Title,
            //    Description = dto.Description,
            //    IsCompleted = dto.IsCompleted,
            //    CreatedDate = DateTime.UtcNow
            //};

            // تنظيف الـ Mapping اليدوي
            var item = _mapper.Map<TodoItem>(dto);
            item.CreatedDate = DateTime.UtcNow;
            /////

            _context.TodoItems.Add(item);

            await _context.SaveChangesAsync();

            //return MapToResponseDto(item);

            return _mapper.Map<TodoResponseDto>(item);
        }

        // ================================
        // Update
        // ================================
        public async Task<TodoResponseDto?> UpdateAsync(int id, UpdateTodoDto dto)
        {
            var item = await _context.TodoItems
                .FirstOrDefaultAsync(x => x.Id == id);

            if (item == null)
                return null;

            // تحديث القيم فقط إذا تم إرسالها

            //// الـ Mapping اليدوي
            //if (dto.Title != null)
            //    item.Title = dto.Title;

            //if (dto.Description != null)
            //    item.Description = dto.Description;

            //if (dto.IsCompleted.HasValue)
            //    item.IsCompleted = dto.IsCompleted.Value;

            // تنظيف الـ Mapping اليدوي
            _mapper.Map(dto, item);


            item.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            //return MapToResponseDto(item);

            return _mapper.Map<TodoResponseDto>(item);
        }

        // ================================
        // Delete
        // ================================
        public async Task<bool> DeleteAsync(int id)
        {
            var item = await _context.TodoItems
                .FirstOrDefaultAsync(x => x.Id == id);

            if (item == null)
                return false;

            _context.TodoItems.Remove(item);

            await _context.SaveChangesAsync();

            return true;
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