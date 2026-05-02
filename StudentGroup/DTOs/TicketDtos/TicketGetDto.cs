namespace StudentGroup.DTOs.TicketDtos
{
    public class TicketGetDto
    {
        public int EventId { get; set; }
        public string Type { get; set; } = null!;
        public decimal Price { get; set; }
        public int QuantityAvailable { get; set; }
    }
}
