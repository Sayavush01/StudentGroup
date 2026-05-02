namespace StudentGroup.DTOs.TicketDtos
{
    public class Ticket
    {
        public int Id { get; set; }
        public int EventId { get; set; }
        public string Type { get; set; } = null!;
        public decimal Price { get; set; }
        public int QuantityAvailable { get; set; }
    }
}
