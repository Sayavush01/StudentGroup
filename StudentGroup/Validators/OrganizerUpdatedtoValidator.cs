using FluentValidation;
using StudentGroup.DTOs.OrganizerDtos;

namespace StudentGroup.Validators
{
    public class OrganizerUpdatedtoValidator:AbstractValidator<OrganizerUpdateDto>
    {
        public OrganizerUpdatedtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .MaximumLength(100);
            RuleFor(x => x.Email)
                .NotEmpty()
                .EmailAddress();
            RuleFor(x => x.Phone)
                .MaximumLength(20);
        }
    }
}
