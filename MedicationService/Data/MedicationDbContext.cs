using Microsoft.EntityFrameworkCore;
using MedicationService.Models;

namespace MedicationService.Data
{
    public class MedicationDbContext : DbContext
    {
        public MedicationDbContext(DbContextOptions<MedicationDbContext> options)
            : base(options)
        {
        }

        // DbSet for storing Medication entities.
        public DbSet<Medication> Medications { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure the Medication entity.
            modelBuilder.Entity<Medication>(entity =>
            {
                // Specify MedicationId as the primary key.
                entity.HasKey(e => e.MedicationId);

                // Optionally, set column configurations.
                entity.Property(e => e.MedicationId)
                      .IsRequired()
                      .HasMaxLength(50);

                entity.Property(e => e.ScientificName)
                      .HasMaxLength(200);

                entity.Property(e => e.BrandName)
                      .HasMaxLength(200);

                // Configure Price and Description as needed.
                entity.Property(e => e.Price)
                      .HasColumnType("decimal(18,2)");

                entity.Property(e => e.Description)
                      .HasMaxLength(1000);
            });
        }
    }
}
