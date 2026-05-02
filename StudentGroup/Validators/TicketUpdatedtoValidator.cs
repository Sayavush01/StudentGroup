using FluentValidation;
using StudentGroup.DTOs.TicketDtos;

namespace StudentGroup.Validators
{
    public class TicketUpdatedtoValidator:AbstractValidator<TicketUpdateDto>
    {
        public TicketUpdatedtoValidator()
        {
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
