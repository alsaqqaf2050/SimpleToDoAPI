using AutoMapper;
using Microsoft.AspNetCore.Identity;
using SimpleToDoAPI.DTOs;
using SimpleToDoAPI.DTOs.Roles;
using SimpleToDoAPI.Models;

namespace SimpleToDoAPI.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // =====================================
            // TodoItem -> TodoResponseDto
            // =====================================
            CreateMap<TodoItem, TodoResponseDto>()
                .ForMember(dest => dest.AssignedToUserName, opt => opt.MapFrom(src => src.AssignedToUser != null ? src.AssignedToUser.FullName : null));

            // =====================================
            // CreateTodoDto -> TodoItem
            // =====================================
            CreateMap<CreateTodoDto, TodoItem>();

            // =====================================
            // UpdateTodoDto -> TodoItem
            // =====================================
            CreateMap<UpdateTodoDto, TodoItem>();

            // =====================================
            // IdentityRole -> RoleResponseDto
            // =====================================
            CreateMap<IdentityRole, RoleResponseDto>();
        }
    }
}