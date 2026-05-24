using AutoMapper;
using SimpleTodoAPI.DTOs;
using SimpleTodoAPI.Models;

namespace SimpleTodoAPI.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // =====================================
            // TodoItem -> TodoResponseDto
            // =====================================
            CreateMap<TodoItem, TodoResponseDto>();

            // =====================================
            // CreateTodoDto -> TodoItem
            // =====================================
            CreateMap<CreateTodoDto, TodoItem>();

            // =====================================
            // UpdateTodoDto -> TodoItem
            // =====================================
            CreateMap<UpdateTodoDto, TodoItem>();
        }
    }
}