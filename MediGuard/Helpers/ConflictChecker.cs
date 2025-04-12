using MediGuard.API.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MediGuard.API.Helpers
{
    public class ConflictChecker : IConflictChecker
    {
        public ConflictChecker(AppDbContext object1, ILogger<ConflictChecker> object2)
        {
            Object1 = object1;
            Object2 = object2;
        }

        public AppDbContext Object1 { get; }
        public ILogger<ConflictChecker> Object2 { get; }

        // Inject your DB context or services if needed
        // private readonly AppDbContext _context;

        // public ConflictChecker(AppDbContext context)
        // {
        //     _context = context;
        // }

        public async Task<IEnumerable<MedicationConflictResult>> CheckUserMedicationConflictsAsync(string userId)
        {
            // Replace with real logic:
            // Example: load user's medications from DB, check if there's any known conflicts
            var results = new List<MedicationConflictResult>();

            // Dummy example:
            // Suppose every user has "Advil" and "Warfarin" => conflict
            bool userHasWarfarinAndAspirin = false;
            if (userHasWarfarinAndAspirin)
            {
                results.Add(new MedicationConflictResult
                {
                    IsCritical = true,
                    ConflictMessage = "User is taking Warfarin and Aspirin concurrently."
                });
            }

            // No real conflicts in this dummy example
            return await Task.FromResult(results);
        }

        public async Task<IEnumerable<MedicationConflictResult>> CheckMedicationConflictsAsync(int medicationId)
        {
            // Replace with real logic for a single medication
            var results = new List<MedicationConflictResult>();

            // Dummy example:
            // Suppose medicationId == 10 is known to conflict with something else
            if (medicationId == 10)
            {
                results.Add(new MedicationConflictResult
                {
                    IsCritical = false,
                    ConflictMessage = "Medication 10 can cause drowsiness when combined with medication X."
                });
            }

            return await Task.FromResult(results);
        }

        Task<bool> IConflictChecker.CheckConflictAsync(int medicationId, string userId)
        {
            throw new NotImplementedException();
        }
    }
}
