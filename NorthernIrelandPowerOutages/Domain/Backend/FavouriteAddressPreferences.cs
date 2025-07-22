using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Backend
{
    public class FavouriteAddressPreferences
    {
        public virtual required string ApplicationUserId { get; set; } 
        public virtual required int AddressId { get; set; }

        public virtual required bool EmailAlertsEnabled { get; set; } = false; 
        public virtual required bool SmsAlertsEnabled { get; set; } = false; 

        public ApplicationUser ApplicationUser { get; set; }
        public Address Address { get; set; }
    }
}
