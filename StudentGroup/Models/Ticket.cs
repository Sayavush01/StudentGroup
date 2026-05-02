namespace StudentGroup.Models
{
    public class Ticket
    {
        public int Id { get; set; }
        public Event Event { get; set; }= null!;
        public int EventId { get; set; }
        public string Type { get; set; } = null!;
        public decimal Price { get; set; }
        public int QuantityAvailable { get; set; }

    }
}
