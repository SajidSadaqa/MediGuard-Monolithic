namespace MediGuard.API.Helpers
{
    /// <summary>
    /// Encapsulates conflict information for one or more medications.
    /// </summary>
    public class MedicationConflictResult
    {
        /// <summary>
        /// Indicates whether the conflict is critical or just a caution.
        /// </summary>
        public bool IsCritical { get; set; }

        /// <summary>
        /// A message describing the nature of the conflict.
        /// </summary>
        public string ConflictMessage { get; set; } = string.Empty;
    }
}
