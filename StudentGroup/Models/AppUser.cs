using Microsoft.AspNetCore.Identity;

namespace StudentGroup.Models
{
    public class AppUser: IdentityUser
    {
        public string FullName { get; set; }= null!;
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiryTime { get; set; }
    }
}
