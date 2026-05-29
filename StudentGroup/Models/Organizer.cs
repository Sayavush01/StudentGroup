namespace StudentGroup.Entities
{
    public class Organizer
    {
        public int Id { get; set; }

        public string Name { get; set; } = null!;

        public string Email { get; set; } = null!;

        public string? Phone { get; set; }

        public string? LogoUrl { get; set; }

        public string? AppUserId { get; set; }
        public StudentGroup.Models.AppUser? AppUser { get; set; }

        public ICollection<Event> Events { get; set; } = new List<Event>();
    }
}