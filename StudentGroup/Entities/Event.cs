using StudentGroup.Entities;

public class Event
{
    public int Id { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public DateTime Date { get; set; }

    public string Location { get; set; } = null!;

    public string? BannerImageUrl { get; set; }

    public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}