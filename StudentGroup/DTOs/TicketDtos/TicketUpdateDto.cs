namespace StudentGroup.DTOs.TicketDtos
{
    public class TicketUpdateDto
    {
        public string Type { get; set; } = null!;
        public decimal Price { get; set; }
        public int QuantityAvailable { get; set; }
    }
}
