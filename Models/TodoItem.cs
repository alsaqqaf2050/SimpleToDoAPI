using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SimpleTodoAPI.Models
{
    public class TodoItem
    {
        [Key]  //  ⁄—Ì› «·Õﬁ· ﬂ„› «Õ √”«”Ì
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] //  Ê·Ìœ  ·ﬁ«∆Ì
        public int Id { get; set; }

        [Required(ErrorMessage = "«·⁄‰Ê«‰ „ÿ·Ê»")]
        [StringLength(100, ErrorMessage = "«·⁄‰Ê«‰ ÌÃ» √·« Ì Ã«Ê“ 100 Õ—›")]
        public string Title { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "«·Ê’› ÌÃ» √·« Ì Ã«Ê“ 500 Õ—›")]
        public string Description { get; set; } = string.Empty;

        public bool IsCompleted { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        [DataType(DataType.DateTime)]
        public DateTime? UpdatedDate { get; set; }
    }

    // DTO ··≈œŒ«· (»œÊ‰ Id)
    public class CreateTodoDto
    {
        [Required]
        [StringLength(100)]
        public string Title { get; set; } = string.Empty;

        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        public bool IsCompleted { get; set; }
    }

    // DTO ·· ÕœÌÀ (Id „‰ «·„”«—° »«ﬁÌ «·»Ì«‰«  „‰ «·Ã”„)
    public class UpdateTodoDto
    {
        [StringLength(100)]
        public string? Title { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        public bool? IsCompleted { get; set; }
    }
}