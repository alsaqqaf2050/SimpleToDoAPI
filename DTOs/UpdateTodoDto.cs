using System.ComponentModel.DataAnnotations;

namespace SimpleTodoAPI.DTOs
{
    public class UpdateTodoDto
    {
        [StringLength(100)]
        public string? Title { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        public bool? IsCompleted { get; set; }
    }
}