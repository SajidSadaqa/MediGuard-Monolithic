using Microsoft.EntityFrameworkCore;
using RecommendationService.Models;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace RecommendationService.Data
{
    public class RecommendationDbContext : DbContext
    {
        public RecommendationDbContext(DbContextOptions<RecommendationDbContext> options)
        : base(options)
        {
        }

        public DbSet<Recommendation> Recommendations { get; set; }
        public DbSet<MedicationRecommendation> MedicationRecommendations { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Recommendation>(entity =>
            {
                entity.HasKey(e => e.UserId); // Assuming one recommendation per user at a time

                entity.Property(e => e.UserId)
                      .IsRequired()
                      .HasMaxLength(450); // Match Identity UserId length

                entity.Property(e => e.RecommendationSummary)
                      .IsRequired()
                      .HasMaxLength(1000);

                entity.HasMany(e => e.MedicationRecommendations)
                      .WithOne()
                      .HasForeignKey("RecommendationUserId")
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<MedicationRecommendation>(entity =>
            {
                entity.HasKey(e => new { e.MedicationId, e.RecommendationUserId }); // Composite key

                entity.Property(e => e.MedicationId)
                      .IsRequired()
                      .HasMaxLength(50);

                entity.Property(e => e.RecommendationUserId)
                      .IsRequired()
                      .HasMaxLength(450);

                entity.Property(e => e.MedicationName)
                      .IsRequired()
                      .HasMaxLength(200);

                entity.Property(e => e.Dosage)
                      .HasMaxLength(50);

                entity.Property(e => e.Timing)
                      .HasMaxLength(100);

                entity.Property(e => e.RecommendationReason)
                      .HasMaxLength(500);
            });
        }
    }
}