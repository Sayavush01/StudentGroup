namespace StudentGroup.DTOs.OrganizerDtos
{
    using FluentValidation;
    using StudentGroup.DTOs.EventDtos;

    public class EventCreateDtoValidator 
    {
        public string Name { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? Phone { get; set; }
    }
}
