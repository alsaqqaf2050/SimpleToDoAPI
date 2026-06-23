using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SimpleToDoAPI.Constants;
using SimpleToDoAPI.Data;
using SimpleToDoAPI.DTOs.Roles;
using SimpleToDoAPI.Exceptions;
using SimpleToDoAPI.Helpers;
using SimpleToDoAPI.Models;
using SimpleToDoAPI.Services.Interfaces;

namespace SimpleToDoAPI.Services
{
    public class RoleService : IRoleService
    {
        private readonly RoleManager<IdentityRole> _roleManager;

        private readonly UserManager<ApplicationUser> _userManager;


        private readonly IMapper _mapper;
        private readonly ILogger<RoleService> _logger;
        private readonly IPermissionCacheService _permissionCache;
        private readonly IAuditService _auditService;

        private readonly ApplicationDbContext _context;

        public RoleService(
            RoleManager<IdentityRole> roleManager,
            UserManager<ApplicationUser> userManager,
            IMapper mapper,
            ILogger<RoleService> logger,
            ApplicationDbContext context,
            IPermissionCacheService permissionCache,
            IAuditService auditService) 
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _mapper = mapper;
            _logger = logger;
            _context = context;
            _permissionCache = permissionCache;
            _auditService = auditService;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        //    public async Task<PagedResult<RoleResponseDto>> GetAllAsync(
        //int pageNumber,
        //int pageSize)
        //    {
        //        var query = _roleManager.Roles;

        //        var totalRecords = await query.CountAsync();

        //        var roles = await query
        //            .OrderBy(x => x.Name)
        //            .Skip((pageNumber - 1) * pageSize)
        //            .Take(pageSize)
        //            .ToListAsync();

        //        return new PagedResult<RoleResponseDto>
        //        {
        //            Items = _mapper.Map<IEnumerable<RoleResponseDto>>(roles),
        //            TotalRecords = totalRecords,
        //            PageNumber = pageNumber,
        //            PageSize = pageSize
        //        };
        //    }

        public async Task<PagedResult<RoleResponseDto>> GetAllAsync(RoleQueryParametersDto parameters)
        {
            var query = _roleManager.Roles.AsQueryable();

            // Search

            //if (!string.IsNullOrWhiteSpace(parameters.Search))
            //{
            //    query = query.Where(x => x.Name!.Contains(parameters.Search));
            //}

            // لأنها تتحول مباشرة إلى SQL LIKE.
            if (!string.IsNullOrWhiteSpace(parameters.Search))
            {
                query = query.Where(x =>
                    EF.Functions.Like(
                        x.Name!,
                        $"%{parameters.Search}%"));
            }

            var totalRecords =
                await query.CountAsync();

            var roles = await query
                .OrderBy(x => x.Name)
                .Skip(
                    (parameters.PageNumber - 1)
                    * parameters.PageSize)
                .Take(parameters.PageSize)
                .ToListAsync();

            return new PagedResult<RoleResponseDto>
            {
                Items = _mapper.Map<
                    IEnumerable<RoleResponseDto>>(roles),

                TotalRecords = totalRecords,

                PageNumber = parameters.PageNumber,

                PageSize = parameters.PageSize
            };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<RoleResponseDto?> GetByIdAsync(
    string id)
        {
            var role = await _roleManager
                .Roles
                .FirstOrDefaultAsync(x => x.Id == id);

            if (role == null)
                return null;

            return _mapper.Map<RoleResponseDto>(role);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<RoleResponseDto> CreateAsync(CreateRoleDto dto)
        {
            var role = new IdentityRole(dto.Name);

            var result = await _roleManager.CreateAsync(role);


            // تسجيل العملية
            await _auditService.LogAsync(
                AuditActions.CreateRole,
                nameof(IdentityRole),
                role.Id,
                newValues: new
                {
                    role.Id,
                    role.Name
                });
            //

            //await _auditService.LogAsync(
            //    action: "CreateRole",
            //    entityName: "IdentityRole",
            //    entityId: role.Id,
            //    newValues: new
            //    {
            //        role.Id,
            //        role.Name
            //    });


            if (!result.Succeeded)
            {
                throw new Exception(
                    string.Join(", ",
                        result.Errors.Select(x => x.Description)));
            }

            return _mapper.Map<RoleResponseDto>(role);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="dto"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<RoleResponseDto?> UpdateAsync(
    string id,
    UpdateRoleDto dto)
        {
            var role = await _roleManager
                .FindByIdAsync(id);

            if (role == null)
                return null;

            role.Name = dto.Name;

            var result =
                await _roleManager.UpdateAsync(role);

            // تسجيل العمليا
            await _auditService.LogAsync(
            AuditActions.UpdateRole,
            nameof(IdentityRole),
            role.Id,
            //oldValues,
            new
            {
                role.Name
            });


            if (!result.Succeeded)
            {
                throw new Exception(
                    string.Join(", ",
                        result.Errors.Select(x => x.Description)));
            }

            return _mapper.Map<RoleResponseDto>(role);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<bool> DeleteAsync(string id)
        {
            var role =
                await _roleManager.FindByIdAsync(id);

            if (role == null)
                return false;

            if (role.Name == Roles.Admin ||
                role.Name == Roles.User)
            {
                throw new BusinessException(
                    "لا يمكن حذف الأدوار الأساسية");
            }

            // تسجيل العملية
            await _auditService.LogAsync(
                AuditActions.DeleteRole,
                nameof(IdentityRole),
                role.Id,
                oldValues: new
                {
                    role.Id,
                    role.Name
                });

            var result =
                await _roleManager.DeleteAsync(role);

            return result.Succeeded;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="roleName"></param>
        /// <returns></returns>
        public async Task<bool> AssignUserAsync(string userId,string roleName)
        {
            // التحقق من وجود المستخدم

            var user = await _userManager.FindByIdAsync(userId);

            if (user == null) return false;

            // التحقق من وجود الدور

            var roleExists = await _roleManager.RoleExistsAsync(roleName);

            if (!roleExists) return false;

            // منع التكرار

            var isInRole = await _userManager.IsInRoleAsync(user,roleName);

            if (isInRole) return true;

            // إضافة المستخدم للدور

            var result = await _userManager.AddToRoleAsync(user,roleName);

            // تسجيل العملية
            await _auditService.LogAsync(
                AuditActions.AssignRole,
                nameof(ApplicationUser),
                user.Id,
                newValues: new
                {
                    UserId = user.Id,
                    UserName = user.UserName,
                    Role = roleName
                });

            // تحديث الكاش
            await _permissionCache.RefreshUserPermissionsAsync(userId);

            return result.Succeeded;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="roleName"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<bool> RemoveUserAsync(string userId,string roleName)
        {
            var user =
                await _userManager.FindByIdAsync(userId);

            if (user == null)
                return false;

            var isInRole =
                await _userManager.IsInRoleAsync(
                    user,
                    roleName);

            if (!isInRole)
                return false;

            // حماية آخر Admin

            if (roleName == Roles.Admin)
            {
                // استخدمنا GetUsersInRoleAsync بدلا _context.UserRoles  لان UserManager هو المسؤول الرسمي عن إدارة المستخدمين والأدوار.
                var admins = await _userManager.GetUsersInRoleAsync(Roles.Admin);

                if (admins.Count <= 1)
                {
                    //throw new Exception("لا يمكن إزالة آخر مدير للنظام");
                    throw new BusinessException("لا يمكن إزالة آخر مدير للنظام");
                }
            }

            var result =
                await _userManager.RemoveFromRoleAsync(
                    user,
                    roleName);

            // تسجيل العملية
            await _auditService.LogAsync(
                AuditActions.RemoveRole,
                nameof(ApplicationUser),
                user.Id,
                oldValues: new
                {
                    UserId = user.Id,
                    UserName = user.UserName,
                    Role = roleName
                });

            // تحديث الكاش
            await _permissionCache.RefreshUserPermissionsAsync(userId);

            return result.Succeeded;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="roleId"></param>
        /// <returns></returns>
        public async Task<List<RolePermissionResponseDto>>
    GetPermissionsAsync(string roleId)
        {
            var permissions =
                await (
                    from rp in _context.RolePermissions
                    join p in _context.Permissions
                        on rp.PermissionId equals p.Id
                    where rp.RoleId == roleId
                    select new RolePermissionResponseDto
                    {
                        PermissionId = p.Id,
                        PermissionName = p.Name,
                        Description = p.Description
                    }
                ).ToListAsync();

            return permissions;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="roleId"></param>
        /// <param name="permissionId"></param>
        /// <returns></returns>
        public async Task<bool>
    AssignPermissionAsync(
        string roleId,
        int permissionId)
        {
            var roleExists =
                await _roleManager.FindByIdAsync(roleId);

            if (roleExists == null)
                return false;

            var permissionExists =
                await _context.Permissions
                    .AnyAsync(x => x.Id == permissionId);

            if (!permissionExists)
                return false;

            var alreadyAssigned =
                await _context.RolePermissions
                    .AnyAsync(x =>
                        x.RoleId == roleId &&
                        x.PermissionId == permissionId);

            if (alreadyAssigned)
                return true;

            _context.RolePermissions.Add(
                new RolePermission
                {
                    RoleId = roleId,
                    PermissionId = permissionId
                });

            await _context.SaveChangesAsync();

            return true;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="roleId"></param>
        /// <param name="permissionId"></param>
        /// <returns></returns>
        public async Task<bool>
    RemovePermissionAsync(
        string roleId,
        int permissionId)
        {
            var rolePermission =
                await _context.RolePermissions
                    .FirstOrDefaultAsync(x =>
                        x.RoleId == roleId &&
                        x.PermissionId == permissionId);

            if (rolePermission == null)
                return false;

            _context.RolePermissions.Remove(rolePermission);

            await _context.SaveChangesAsync();

            return true;
        }

    }
}
