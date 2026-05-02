using FluentValidation;
using StudentGroup.Entities;

namespace EventManagementApi.Validators;

public class OrganizerCreateDtoValidator : AbstractValidator<Organizer>
{
    public OrganizerCreateDtoValidator()
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