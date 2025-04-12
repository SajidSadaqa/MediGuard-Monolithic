using Microsoft.EntityFrameworkCore;
using PaymentService.Models;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace PaymentService.Data
{
    public class PaymentDbContext : DbContext
    {
        public PaymentDbContext(DbContextOptions<PaymentDbContext> options)
        : base(options)
        {
        }

        public DbSet<PaymentTransaction> PaymentTransactions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<PaymentTransaction>(entity =>
            {
                entity.HasKey(e => e.TransactionId);

                entity.Property(e => e.TransactionId)
                      .ValueGeneratedNever(); // GUID is generated in code, not by DB

                entity.Property(e => e.OrderId)
                      .IsRequired();

                entity.Property(e => e.Amount)
                      .HasColumnType("decimal(18,2)")
                      .IsRequired();

                entity.Property(e => e.PaymentStatus)
                      .IsRequired()
                      .HasMaxLength(50);

                entity.Property(e => e.TransactionDate)
                      .IsRequired();
            });
        }
    }
}