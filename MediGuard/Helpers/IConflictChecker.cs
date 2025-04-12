using System.Collections.Generic;
using System.Threading.Tasks;

namespace MediGuard.API.Helpers
{
    public interface IConflictChecker
    {
        /// <summary>
        /// Checks for conflicts across all medications for a given user.
        /// </summary>
        /// <param name="userId">The user ID.</param>
        /// <returns>A collection of MedicationConflictResult.</returns>
        Task<IEnumerable<MedicationConflictResult>> CheckUserMedicationConflictsAsync(string userId);

        /// <summary>
        /// Checks for conflicts for a specific medication by medication ID.
        /// </summary>
        /// <param name="medicationId">The medication ID.</param>
        /// <returns>A collection of MedicationConflictResult.</returns>
        Task<IEnumerable<MedicationConflictResult>> CheckMedicationConflictsAsync(int medicationId);

        /// <summary>
        /// Checks if a specific medication (by medicationId) conflicts with any of the user's active medications.
        /// </summary>
        /// <param name="medicationId">The medication ID to check.</param>
        /// <param name="userId">The user ID.</param>
        /// <returns>True if a conflict is found; otherwise, false.</returns>
        Task<bool> CheckConflictAsync(int medicationId, string userId);
    }
}
