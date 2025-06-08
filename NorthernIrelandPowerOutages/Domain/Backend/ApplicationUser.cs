using Microsoft.AspNetCore.Identity;

namespace Domain.Backend
{
    // Add profile data for application users by adding properties to the ApplicationUser class
    public class ApplicationUser : IdentityUser
    {
        public virtual ICollection<Address> FavoriteAddresses { get; set; }
    }
}
