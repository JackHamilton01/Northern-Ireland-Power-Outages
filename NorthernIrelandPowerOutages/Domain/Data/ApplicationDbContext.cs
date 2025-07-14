
using Domain;
using Domain.Backend;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) 
        : IdentityDbContext<ApplicationUser>(options)
    {
        public DbSet<Address> Addresses { get; set;  }
        public DbSet<Hazard> Hazards { get; set;  }
        public DbSet<Service> Services { get; set; }
        public DbSet<OutagePredictionTrainingData> OutagePredictionTrainingData { get; set; }
        public DbSet<OutagePredictionModel> OutagePredictionModels { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
        }
    }
}
