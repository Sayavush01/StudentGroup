namespace StudentGroup.Entities
{
    public class Organizer
    {
        public int Id { get; set; }

        public string Name { get; set; } = null!;

        public string Email { get; set; } = null!;

        public string? Phone { get; set; }

        public string? LogoUrl { get; set; }
    }
}
