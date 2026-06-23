using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SimpleToDoAPI.Constants;
using SimpleToDoAPI.DTOs.Auth;
using SimpleToDoAPI.Helpers;
using SimpleToDoAPI.Services.Auth;

namespace SimpleToDoAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        // هنا يتم حقن AuthService تلقائيًا عبر الـ Constructor Injection
        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            var result = await _authService.RegisterAsync(dto);

            if (!result.IsSuccess)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            var result = await _authService.LoginAsync(dto);

            if (!result.IsSuccess)
                return Unauthorized(result);

            return Ok(result);
        }

        // EndPoint للتحقق من محتتويات التوكين
        [Authorize]
        [HttpGet("claims")]
        public IActionResult GetClaims()
        {
            var claims = User.Claims.Select(c => new {c.Type,c.Value});
            return Ok(claims);
        }


        //// إنشاء صلاحيات للأدمن
        //[Authorize(Roles = "Admin")]
        //[HttpGet("admin-only")]
        //public IActionResult AdminOnly()
        //{
        //    return Ok(new{Message = "Welcome Admin"});
        //}

        //// لإضافة الصلاحيات  Policies
        //// الفائدة من ال Polices جميع قواعد الصلاحيات تصبح في مكان مركزي واحد
        //// بدلا من ال Roles
        //[Authorize(Policy = "AdminOnly")]
        //[HttpGet("admin-area")]
        //public IActionResult AdminArea()
        //{
        //    return Ok("Admin Area");
        //}


        // إنشاء صلاحيات للأدمن
        [Authorize(Policy = Policies.AdminOnly)]
        [HttpGet("admin-only")]
        public IActionResult AdminOnly()
        {
            return Ok(new { Message = "Welcome Admin" });
        }


        //// إنشاء صلاحيات للمستخدم
        //[Authorize(Roles = "User")]
        //[HttpGet("user-only")]
        //public IActionResult UserOnly()
        //{
        //    return Ok(new{ Message = "Welcome User"});
        //}

        // لإضافة الصلاحيات  Policies
        // الفائدة من ال Polices جميع قواعد الصلاحيات تصبح في مكان مركزي واحد
        // بدلا من ال Roles
        [Authorize(Policy = "UserOnly")]
        [HttpGet("user-area")]
        public IActionResult UserArea()
        {
            return Ok("User Area");
        }


        //// EndPoint تقبل الإثنين المستخدم والأدمن
        //[Authorize(Roles = "Admin,User")]
        //[HttpGet("authorized")]
        //public IActionResult Authorized()
        //{
        //    return Ok(new{ Message = "Welcome Authenticated User"});
        //}

        //[Authorize(Policy = "AdminOrUser")]
        //[HttpGet("dashboard")]
        //public IActionResult Dashboard()
        //{
        //    return Ok("Dashboard");
        //}



        [HttpPost("refresh")]
        [AllowAnonymous]
        public async Task<IActionResult> RefreshToken(RefreshTokenRequestDto dto)
        {
            var result =await _authService.RefreshTokenAsync(dto);

            if (!result.IsSuccess)
                return BadRequest(result);

            return Ok(result);
        }


        //[HttpPost("logout")]
        //[Authorize]
        //public async Task<IActionResult> Logout(RevokeTokenDto dto)
        //{
        //    var result = await _authService.RevokeTokenAsync(dto.RefreshToken);

        //    if (!result)
        //        return BadRequest(new
        //        {
        //            Message = "Refresh Token غير صالح"
        //        });

        //    return Ok(new
        //    {
        //        Message = "تم تسجيل الخروج بنجاح"
        //    });
        //}


        // سنقوم بحذف الريفريش توكين الخاصة بالمستخدم الحالي
        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            var result = await _authService.LogoutAsync();

            if (!result)
                return BadRequest(new
                {
                    Message = "لا توجد جلسات نشطة"
                });

            return Ok(new
            {
                Message = "تم تسجيل الخروج بنجاح"
            });
        }


        /// <summary>
        /// دالة تقوم بجلب صلاحيات المستخدم الحالي
        /// </summary>
        /// <returns></returns>
        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> Me()
        {
            var result =
                await _authService
                    .GetCurrentUserAsync();

            if (result == null)
            {
                return NotFound(
                    new ApiErrorResponse(
                        "المستخدم غير موجود"));
            }

            return Ok(
                new ApiResponse<CurrentUserDto>
                (
                    true,
                    "تم جلب بيانات المستخدم",
                    result
                ));
        }


    }
}