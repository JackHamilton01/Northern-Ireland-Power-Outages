
using Domain;
using Domain.Backend;
using Infrastructure.Settings;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

namespace Infrastructure.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : IdentityDbContext<ApplicationUser>(options)
    {
        public DbSet<Address> Addresses { get; set; }
        public DbSet<Hazard> Hazards { get; set; }
        public DbSet<Service> Services { get; set; }
        public DbSet<OutagePredictionTrainingData> OutagePredictionTrainingData { get; set; }
        public DbSet<OutagePredictionModel> OutagePredictionModels { get; set; }
        public DbSet<FavouriteAddressPreferences> FavouriteAddressPreferences { get; set; }
        public DbSet<Domain.Backend.Settings> Settings { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<FavouriteAddressPreferences>()
                .HasKey(f => new { f.ApplicationUserId, f.AddressId });

            builder.Entity<FavouriteAddressPreferences>()
               .HasOne(f => f.ApplicationUser)
               .WithMany(u => u.FavouriteAddressPreferences)
               .HasForeignKey(f => f.ApplicationUserId);

            builder.Entity<FavouriteAddressPreferences>()
               .HasOne(f => f.Address)
               .WithMany(a => a.FavouriteAddressPreferences)
               .HasForeignKey(f => f.AddressId);

            builder.Entity<Domain.Backend.Settings>().HasData(
                new Domain.Backend.Settings 
                { 
                    Id = 1, 
                    Name = SettingConstants.LastPredictionTrainedTimestamp,
                    Value = string.Empty
                }
            );

            base.OnModelCreating(builder);
        }
    }
}
