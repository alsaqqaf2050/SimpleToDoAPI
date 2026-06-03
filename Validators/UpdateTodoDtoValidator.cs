using FluentValidation;
using SimpleToDoAPI.DTOs;

namespace SimpleToDoAPI.Validators
{
    public class UpdateTodoDtoValidator
        : AbstractValidator<UpdateTodoDto>
    {
        public UpdateTodoDtoValidator()
        {
            // =====================================
            // Title
            // =====================================
            RuleFor(x => x.Title)
                .MaximumLength(100)
                .When(x => x.Title != null)
                .WithMessage("العنوان يجب ألا يتجاوز 100 حرف");

            // =====================================
            // Description
            // =====================================
            RuleFor(x => x.Description)
                .MaximumLength(500)
                .When(x => x.Description != null)
                .WithMessage("الوصف يجب ألا يتجاوز 500 حرف");
        }
    }
}