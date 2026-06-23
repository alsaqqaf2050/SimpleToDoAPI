using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SimpleToDoAPI.Constants;
using SimpleToDoAPI.Data;
using SimpleToDoAPI.DTOs;
using SimpleToDoAPI.DTOs.Users;
using SimpleToDoAPI.Helpers;
using SimpleToDoAPI.Models;
using SimpleToDoAPI.Services.Interfaces;

namespace SimpleToDoAPI.Services
{
    public class UserService : IUserService
    {
        private readonly UserManager<ApplicationUser> _userManager;

        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IAuditService _auditService;

        public UserService(UserManager<ApplicationUser> userManager, ApplicationDbContext context, IMapper mapper, IAuditService auditService)
        {
            _userManager = userManager;
            _context = context;
            _mapper = mapper;
            _auditService = auditService;
        }

        //// دالة جلب البيانات ولكن منطقها ضعيف والسبب انه 
        //// N + 1 Query Problem
        //// 1 Query لجلب المستخدمين
        ////Query لكل مستخدم لجلب Roles
        ///
        //public async Task<List<UserResponseDto>> GetAllAsync()
        //{
        //    var users = await _userManager.Users.ToListAsync();

        //    var result = new List<UserResponseDto>();

        //    foreach (var user in users)
        //    {
        //        var roles = await _userManager.GetRolesAsync(user);

        //        result.Add(new UserResponseDto
        //        {
        //            Id = user.Id,
        //            UserName = user.UserName ?? "",
        //            Email = user.Email ?? "",
        //            FullName = user.FullName,
        //            IsActive = user.IsActive,
        //            Roles = roles
        //        });
        //    }

        //    return result;
        //}


        // دالة جلب المستخدمين 

        // الحل الأفضل (Optimized UserService)
        // 1. نجلب المستخدمين فقط
        // 2. نجلب كل Roles مرة واحدة (بدلاً من لكل مستخدم)
        //
        //public async Task<List<UserResponseDto>> GetAllAsync()
        //{
        //    // نجلب المستخدمين فقط
        //    var users = await _userManager.Users.AsNoTracking().ToListAsync();

        //    // نجلب كل Roles مرة واحدة (بدلاً من لكل مستخدم)
        //    var userRoles = await _userManager.UserRoles.AsNoTracking().ToListAsync();
        //    var roles = await _userManager.Roles.AsNoTracking().ToListAsync();


        //    // نبني Lookup سريع (Dictionary)
        //    var roleLookup = roles.ToDictionary(x => x.Id, x => x.Name);

        //    var userRolesLookup = userRoles
        //        .GroupBy(x => x.UserId)
        //        .ToDictionary(
        //            g => g.Key,
        //            g => g.Select(r => roleLookup[r.RoleId]).ToList()
        //        );

        //    // تحويل سريع بدون أي DB Calls داخل loop
        //    return users.Select(user => new UserResponseDto
        //    {
        //        Id = user.Id,
        //        UserName = user.UserName ?? "",
        //        Email = user.Email ?? "",
        //        FullName = user.FullName,
        //        IsActive = user.IsActive,
        //        Roles = userRolesLookup.ContainsKey(user.Id)
        //            ? userRolesLookup[user.Id]
        //            : new List<string>()
        //    }).ToList();
        //}


        // Get users with Pagination
        public async Task<PagedResult<UserResponseDto>> GetAllAsync(int pageNumber, int pageSize)
        {

            //var query = _userManager.Users.AsNoTracking().Where(x => !x.IsDeleted);

            // بسبب وجود الفلتر العام في ال ApplicationDbContext فلا يحتاج عمل الشرط هنا Where(x => !x.IsDeleted
            var query = _userManager.Users.AsNoTracking();

            var totalRecords = await query.CountAsync();

            var users = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            //var userRoles = await _userManager.UserRoles.ToListAsync();
            //var roles = await _userManager.Roles.ToListAsync();

            var userRoles = await _context.UserRoles.ToListAsync();
            var roles = await _context.Roles.ToListAsync();

            var roleLookup = roles.ToDictionary(x => x.Id, x => x.Name);

            var userRolesLookup = userRoles
                .GroupBy(x => x.UserId)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(r => roleLookup[r.RoleId]).ToList()
                );

            var result = users.Select(user => new UserResponseDto
            {
                Id = user.Id,
                UserName = user.UserName ?? "",
                Email = user.Email ?? "",
                FullName = user.FullName,
                IsActive = user.IsActive,
                Roles = userRolesLookup.ContainsKey(user.Id)
                    ? userRolesLookup[user.Id]
                    : new List<string>()
            }).ToList();

            return new PagedResult<UserResponseDto>
            {
                Items = result,
                TotalRecords = totalRecords,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<UserResponseDto?> GetByIdAsync(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user == null) return null;

            var roles = await _userManager.GetRolesAsync(user);

            return new UserResponseDto
            {
                Id = user.Id,
                UserName = user.UserName ?? "",
                Email = user.Email ?? "",
                FullName = user.FullName,
                IsActive = user.IsActive,
                Roles = roles
            };
        }

        public async Task<UserResponseDto> CreateAsync(CreateUserDto dto)
        {
            var user = new ApplicationUser
            {
                UserName = dto.UserName,
                Email = dto.Email,
                FullName = dto.FullName,
                IsActive = true
            };

            var result = await _userManager.CreateAsync(user, dto.Password);

            if (!result.Succeeded)
                throw new Exception(string.Join(", ", result.Errors.Select(x => x.Description)));

            if (dto.Roles != null)
            {
                await _userManager.AddToRolesAsync(user, dto.Roles);
            }

            var roles = await _userManager.GetRolesAsync(user);


            // تسجيل العمليات
            // سيسجل هذا
            //  {
            //    "Action": "Create User",
            //    "EntityName": "ApplicationUser",
            //    "EntityId": "123",
            //    "NewValues": {
            //    "UserName":"Ahmed",
            //    "Email":"ahmed@test.com",
            //    "FullName":"Ahmed Ali",
            //    "IsActive":true
            //      }
            //   }
            await _auditService.LogAsync(
                //action: "Create User",
                action: AuditActions.CreateUser,
                entityName: nameof(ApplicationUser),
                entityId: user.Id,
                newValues: new
                {
                    user.UserName,
                    user.Email,
                    user.FullName,
                    user.IsActive
                });

            //return new UserResponseDto
            //{
            //    Id = user.Id,
            //    UserName = user.UserName ?? "",
            //    Email = user.Email ?? "",
            //    FullName = user.FullName,
            //    IsActive = user.IsActive,
            //    Roles = roles
            //};

            // تنظيف الـ Mapping اليدوي
            return _mapper.Map<UserResponseDto>(user);
        }

        public async Task<UserResponseDto?> UpdateAsync(string id, UpdateUserDto dto)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user == null) return null;

            user.FullName = dto.FullName;
            user.Email = dto.Email;
            user.IsActive = dto.IsActive;

            await _userManager.UpdateAsync(user);

            var roles = await _userManager.GetRolesAsync(user);



            // سيسجل 
              //          {
              //              "Action":"Update User",
              //"OldValues":{
              //                  "FullName":"Ahmed"
              //},
              //"NewValues":{
              //                  "FullName":"Ahmed Alsaqqaf"
              //}
              //          }
            await _auditService.LogAsync(
                //action: "Update User",
                action: AuditActions.UpdateUser,
                entityName: nameof(ApplicationUser),
                entityId: user.Id,
                //oldValues: oldValues,
                newValues: new
                {
                    user.FullName,
                    user.Email,
                    user.IsActive
                });


            //return new UserResponseDto
            //{
            //    Id = user.Id,
            //    UserName = user.UserName ?? "",
            //    Email = user.Email ?? "",
            //    FullName = user.FullName,
            //    IsActive = user.IsActive,
            //    Roles = roles
            //};

            // تنظيف الـ Mapping اليدوي
            return _mapper.Map<UserResponseDto>(user);
        }

        //public async Task<bool> DeleteAsync(string id)
        //{
        //    var user = await _userManager.FindByIdAsync(id);

        //    if (user == null) return false;

        //    //var result = await _userManager.DeleteAsync(user);

        //    // soft Delete
        //    user.IsDeleted = true;
        //    user.DeletedDate = DateTime.UtcNow;

        //    var result = await _userManager.UpdateAsync(user);

        //    return result.Succeeded;
        //}

        /// <summary>
        ///  soft Delete
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<bool> DeleteAsync(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user == null) return false;

            //var result = await _userManager.DeleteAsync(user);

            // soft Delete
            user.IsDeleted = true;
            user.DeletedDate = DateTime.UtcNow;
            user.IsActive = false;

            // تسجيل العملية
            await _auditService.LogAsync(
                action: "Delete User",
                entityName: nameof(ApplicationUser),
                entityId: user.Id,
                oldValues: new
                {
                    user.UserName,
                    user.Email,
                    user.IsActive
                },
                newValues: new
                {
                    IsDeleted = true,
                    DeletedDate = user.DeletedDate
                });

            await _userManager.UpdateAsync(user);


            return true;
        }

        /// <summary>
        /// Restore user after soft delete
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<bool> RestoreAsync(string id)
        {
            var user = await _context.Users
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (user == null)
                return false;

            user.IsDeleted = false;
            user.IsActive = true;
            user.DeletedDate = null;

            await _userManager.UpdateAsync(user);

            return true;
        }

    }
}