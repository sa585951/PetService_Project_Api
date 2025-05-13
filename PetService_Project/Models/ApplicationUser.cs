using Microsoft.AspNetCore.Identity;

namespace PetService_Project_Api.Models
{
    public class ApplicationUser:IdentityUser
    {
        public string? GoogleAvatarUrl { get; set; }
        public string? ProviderKey { get; set; }
    }
}
