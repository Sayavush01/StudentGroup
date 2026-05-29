namespace StudentGroup.DTOs.UserDtos
{
    public class TwoFactorLoginDto
    {
        public string Email { get; set; } = null!;
        public string Code { get; set; } = null!;
    }
}