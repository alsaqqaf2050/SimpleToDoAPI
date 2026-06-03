using SimpleToDoAPI.DTOs;
using SimpleToDoAPI.Helpers;

namespace SimpleToDoAPI.Services.Interfaces
{
    public interface ITodoService
    {
        //Task<IEnumerable<TodoResponseDto>> GetAllAsync();
        //Task<IEnumerable<TodoResponseDto>> GetAllAsync(TodoQueryParametersDto parameters);
        Task<PagedResult<TodoResponseDto>> GetAllAsync(TodoQueryParametersDto parameters);

        Task<TodoResponseDto?> GetByIdAsync(int id);

        Task<TodoResponseDto> CreateAsync(CreateTodoDto dto);

        Task<TodoResponseDto?> UpdateAsync(int id, UpdateTodoDto dto);

        Task<bool> DeleteAsync(int id);
    }
}