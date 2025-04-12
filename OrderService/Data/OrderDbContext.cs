using Microsoft.EntityFrameworkCore;
using OrderService.Models;
namespace OrderService.Data
{
    public class OrderDbContext : DbContext
    {
        public OrderDbContext(DbContextOptions<OrderDbContext> options)
            : base(options)
        {
        }

        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Order entity
            modelBuilder.Entity<Order>(entity =>
            {
                entity.HasKey(o => o.OrderId);
                entity.Property(o => o.UserId).IsRequired();
                entity.Property(o => o.ShippingAddress).IsRequired();
                entity.Property(o => o.TotalAmount)
                      .HasColumnType("decimal(18,2)");
                entity.Property(o => o.PaymentStatus)
                      .HasMaxLength(50);
            });

            // Configure OrderItem entity
            modelBuilder.Entity<OrderItem>(entity =>
            {
                entity.HasKey(oi => oi.OrderItemId);
                entity.Property(oi => oi.MedicationId).IsRequired();
                entity.Property(oi => oi.MedicationName).IsRequired();
                entity.Property(oi => oi.Quantity).IsRequired();
                entity.Property(oi => oi.Price)
                      .HasColumnType("decimal(18,2)");
                entity.HasOne<Order>()
                      .WithMany(o => o.Items)
                      .HasForeignKey(oi => oi.OrderId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
