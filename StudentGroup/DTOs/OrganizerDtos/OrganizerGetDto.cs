using FluentValidation;

namespace StudentGroup.DTOs.OrganizerDtos
{
    public class OrganizerGetDto:AbstractValidator<OrganizerGetDto>
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? LogoUrl { get; set; }
        public string Email { get; set; } = null!;
        public string? Phone { get; set; }
    }
}
