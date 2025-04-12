using Microsoft.AspNetCore.Identity;
using System;
using System.ComponentModel.DataAnnotations;

namespace AuthService.Models
{
    // Extend IdentityUser to add custom properties for your application's users.
    public class UserProfile : IdentityUser
    {
        [MaxLength(100)]
        public string FullName { get; set; }

        public DateTime? DateOfBirth { get; set; }
    }
}
