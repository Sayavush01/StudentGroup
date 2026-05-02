using FluentValidation;
using StudentGroup.DTOs.EventDtos;

namespace StudentGroup.Validators
{
    public class EventUpdatedtoValidator: AbstractValidator<EventUpdatedto>
    {
        public EventUpdatedtoValidator() 
        {
            RuleFor(x => x.Title)
                .NotEmpty()
                .MaximumLength(150);
            RuleFor(x => x.Description)
                .MaximumLength(500);
            RuleFor(x => x.Location)
                .NotEmpty()
                .MaximumLength(200);
            RuleFor(x => x.Date)
                .Must(date => date > DateTime.Now)
                .WithMessage("Event date must be in the future");
        }

    }
}
