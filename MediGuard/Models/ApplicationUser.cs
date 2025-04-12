using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace MediGuard.API.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Conditions { get; set; }
        public string? Allergies { get; set; }
        public DateTime DateOfBirth { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation properties
        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
        public virtual ICollection<UserMedication> UserMedications { get; set; } = new List<UserMedication>();
    }
}
