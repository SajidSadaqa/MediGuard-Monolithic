using Microsoft.EntityFrameworkCore;
using AdherenceService.Models;
using Microsoft.ML;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace AdherenceService.Data
{
    public class AdherenceDbContext : DbContext
    {
        public AdherenceDbContext(DbContextOptions<AdherenceDbContext> options)
            : base(options)
        {
        }

        public DbSet<AdherenceRecord> AdherenceRecords { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<AdherenceRecord>(entity =>
            {
                // Composite primary key: UserId + MedicationId + ScheduledDoseTime
                entity.HasKey(e => new { e.UserId, e.MedicationId, e.ScheduledDoseTime });

                entity.Property(e => e.UserId)
                      .IsRequired()
                      .HasMaxLength(450); // Match Identity UserId length

                entity.Property(e => e.MedicationId)
                      .IsRequired()
                      .HasMaxLength(50);

                entity.Property(e => e.ScheduledDoseTime)
                      .IsRequired();

                entity.Property(e => e.ActualDoseTime)
                      .IsRequired(false); // Nullable

                entity.Property(e => e.IsDoseTaken)
                      .IsRequired();

                entity.Property(e => e.AlertTriggered)
                      .IsRequired();

                entity.Property(e => e.Note)
                      .HasMaxLength(1000);
            });
        }
    }
}