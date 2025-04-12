// In AuthService.Data/AuthDbContext.cs
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using AuthService.Models;

namespace AuthService.Data
{
    public class AuthDbContext : IdentityDbContext<UserProfile>
    {
        public AuthDbContext(DbContextOptions<AuthDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Customize the ASP.NET Identity table names if desired
            builder.Entity<UserProfile>(entity =>
            {
                entity.ToTable("AspNetUsers");
                entity.Property(u => u.FullName).HasMaxLength(100);
                entity.Property(u => u.DateOfBirth).HasColumnType("date");
            });
        }
    }
}