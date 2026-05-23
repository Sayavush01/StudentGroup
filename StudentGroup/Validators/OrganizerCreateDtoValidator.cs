using FluentValidation;
using StudentGroup.DTOs.OrganizerDtos;

namespace EventManagementApi.Validators;

public class OrganizerCreateDtoValidator : AbstractValidator<OrganizerCreate>
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