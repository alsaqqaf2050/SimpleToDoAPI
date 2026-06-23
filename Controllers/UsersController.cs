using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SimpleToDoAPI.Constants;
using SimpleToDoAPI.DTOs.Users;
using SimpleToDoAPI.Services.Interfaces;
using System.Security.Claims;

namespace SimpleToDoAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = Policies.AdminOnly)]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        /// <summary>
        /// Get All Users (Pagination)
        /// </summary>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        [HttpGet]
        [Authorize(Policy = Policies.UserView)]
        public async Task<IActionResult> GetAll(int pageNumber = 1,int pageSize = 10)
        {
            var result = await _userService.GetAllAsync(pageNumber, pageSize);

            return Ok(result);
        }

        /// <summary>
        /// Get User By Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        [Authorize(Policy = Policies.UserView)]
        public async Task<IActionResult> GetById(string id)
        {
            var user = await _userService.GetByIdAsync(id);

            if (user == null)
                return NotFound();

            return Ok(user);
        }


        /// <summary>
        /// Create User (Admin Create User)
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorize(Policy = Policies.UserCreate)]
        public async Task<IActionResult> Create(CreateUserDto dto)
        {
            var result = await _userService.CreateAsync(dto);

            return Ok(result);
        }

        /// <summary>
        /// Update User
        /// </summary>
        /// <param name="id"></param>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPut("{id}")]
        [Authorize(Policy = Policies.UserUpdate)]
        public async Task<IActionResult> Update(string id, UpdateUserDto dto)
        {
            var result = await _userService.UpdateAsync(id, dto);

            if (result == null)
                return NotFound();

            return Ok(result);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        [Authorize(Policy = Policies.UserDelete)]
        public async Task<IActionResult> Delete(string id)
        {
            if (User.FindFirstValue(ClaimTypes.NameIdentifier) == id)
            {
                return BadRequest(new
                {
                    Message = "لا يمكنك حذف حسابك الحالي"
                });
            }

            var result = await _userService.DeleteAsync(id);

            if (!result)
                return NotFound();

            return Ok(new
            {
                Message = "تم حذف المستخدم بنجاح"
            });
        }

    }
}
