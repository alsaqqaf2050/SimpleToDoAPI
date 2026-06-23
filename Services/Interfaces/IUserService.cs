using SimpleToDoAPI.DTOs.Users;
using SimpleToDoAPI.Helpers;

namespace SimpleToDoAPI.Services.Interfaces
{
    public interface IUserService
    {
        //Task<List<UserResponseDto>> GetAllAsync();

        Task<PagedResult<UserResponseDto>> GetAllAsync(int pageNumber, int pageSize);

        Task<UserResponseDto?> GetByIdAsync(string id);

        Task<UserResponseDto> CreateAsync(CreateUserDto dto);

        Task<UserResponseDto?> UpdateAsync(string id, UpdateUserDto dto);

        Task<bool> DeleteAsync(string id);
    }
}
