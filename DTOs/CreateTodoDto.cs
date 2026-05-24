using System.ComponentModel.DataAnnotations;

namespace SimpleTodoAPI.DTOs
{
    public class CreateTodoDto
    {
        [Required(ErrorMessage = "العنوان مطلوب")]
        [StringLength(100)]
        public string Title { get; set; } = string.Empty;

        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        public bool IsCompleted { get; set; }
    }
}