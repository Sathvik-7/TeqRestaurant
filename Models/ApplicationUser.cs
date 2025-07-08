using Microsoft.AspNetCore.Identity;

namespace TeqRestaurant.Models
{
    public class ApplicationUser : IdentityUser
    {
        public ICollection<Order>? Orders { get; set; }
    }
}
