using Microsoft.AspNetCore.Identity;
using MediGuard.API.Models;
using System.Text.Json;

namespace MediGuard.API.Data
{
    public static class DbInitializer
    {
        public static async Task InitializeAsync(AppDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            // Ensure database is created
            context.Database.EnsureCreated();

            // Add roles if they don't exist
            string[] roles = { "Admin", "User" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // Add test user if it doesn't exist
            if (await userManager.FindByEmailAsync("test@mediaguard.com") == null)
            {
                var user = new ApplicationUser
                {
                    UserName = "test@mediaguard.com",
                    Email = "test@mediaguard.com",
                    FirstName = "Test",
                    LastName = "User",
                    Conditions = "Asthma",
                    Allergies = "Penicillin",
                    DateOfBirth = new DateTime(1990, 1, 1),
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(user, "Test@123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, "User");
                }
            }

            // Add admin user if it doesn't exist
            if (await userManager.FindByEmailAsync("admin@mediaguard.com") == null)
            {
                var adminUser = new ApplicationUser
                {
                    UserName = "admin@mediaguard.com",
                    Email = "admin@mediaguard.com",
                    FirstName = "Admin",
                    LastName = "User",
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(adminUser, "Admin@123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }

            // Add medications if they don't exist
            if (!context.Medications.Any())
            {
                var medications = new List<Medication>
                {
                    new Medication
                    {
                        Name = "Advil",
                        ScientificName = "Ibuprofen",
                        Price = 5.99m,
                        Description = "Used for treating pain, fever, and inflammation.",
                        Manufacturer = "Pfizer",
                        DosageForm = "Tablet",
                        Strength = "200mg",
                        RequiresPrescription = false,
                        IsAvailable = true,
                        ConflictsWith = JsonSerializer.Serialize(new List<string> { "Warfarin", "Aspirin" })
                    },
                    new Medication
                    {
                        Name = "Tylenol",
                        ScientificName = "Acetaminophen",
                        Price = 4.99m,
                        Description = "Used for treating pain and fever.",
                        Manufacturer = "Johnson & Johnson",
                        DosageForm = "Tablet",
                        Strength = "500mg",
                        RequiresPrescription = false,
                        IsAvailable = true,
                        ConflictsWith = JsonSerializer.Serialize(new List<string>())
                    },
                    new Medication
                    {
                        Name = "Warfarin",
                        ScientificName = "Warfarin",
                        Price = 8.99m,
                        Description = "Used as a blood thinner to prevent blood clots.",
                        Manufacturer = "Bristol-Myers Squibb",
                        DosageForm = "Tablet",
                        Strength = "5mg",
                        RequiresPrescription = true,
                        IsAvailable = true,
                        ConflictsWith = JsonSerializer.Serialize(new List<string> { "Ibuprofen" })
                    },
                    new Medication
                    {
                        Name = "Aspirin",
                        ScientificName = "Acetylsalicylic acid",
                        Price = 3.99m,
                        Description = "Used to treat pain, fever, and inflammation.",
                        Manufacturer = "Bayer",
                        DosageForm = "Tablet",
                        Strength = "81mg",
                        RequiresPrescription = false,
                        IsAvailable = true,
                        ConflictsWith = JsonSerializer.Serialize(new List<string> { "Ibuprofen" })
                    },
                    new Medication
                    {
                        Name = "Lipitor",
                        ScientificName = "Atorvastatin",
                        Price = 12.99m,
                        Description = "Used to lower cholesterol levels.",
                        Manufacturer = "Pfizer",
                        DosageForm = "Tablet",
                        Strength = "10mg",
                        RequiresPrescription = true,
                        IsAvailable = true,
                        ConflictsWith = JsonSerializer.Serialize(new List<string>())
                    }
                };

                context.Medications.AddRange(medications);
                await context.SaveChangesAsync();
            }
        }
    }
}
