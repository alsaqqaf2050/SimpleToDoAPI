using FluentValidation;
using SimpleTodoAPI.DTOs;

namespace SimpleTodoAPI.Validators
{
    public class CreateTodoDtoValidator
        : AbstractValidator<CreateTodoDto>
    {
        public CreateTodoDtoValidator()
        {
            // =====================================
            // Title
            // =====================================
            RuleFor(x => x.Title)
                .NotEmpty()
                .WithMessage("عنوان المهمة مطلوب")

                .MaximumLength(100)
                .WithMessage("العنوان يجب ألا يتجاوز 100 حرف");

            // =====================================
            // Description
            // =====================================
            RuleFor(x => x.Description)
                .MaximumLength(500)
                .WithMessage("الوصف يجب ألا يتجاوز 500 حرف");
        }
    }
}