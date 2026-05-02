using FluentValidation;
using StudentGroup.DTOs.TicketDtos;

namespace StudentGroup.Validators
{
    public class TicketCreateDtoValidator:AbstractValidator<TicketCreate>
    {
        public TicketCreateDtoValidator()
        {
            RuleFor(x => x.EventId)
                .GreaterThan(0);
            RuleFor(x => x.Type)
                .NotEmpty()
                .MaximumLength(50);
            RuleFor(x => x.Price)
                .GreaterThanOrEqualTo(0);
            RuleFor(x => x.QuantityAvailable)
                .GreaterThanOrEqualTo(0);
        }

    }
}
