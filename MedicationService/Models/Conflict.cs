using MedicationService.Models;
using System.Collections.Generic;

namespace MedicationService.Models
{
    public class Conflict
    {
        /// <summary>
        /// The severity of the conflict.
        /// Expected values might be "Red" (severe), "Yellow" (moderate), or "Green" (no conflict).
        /// </summary>
        public string Severity { get; set; }

        /// <summary>
        /// A detailed message or description of the conflict.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// List of medications involved in the conflict.
        /// </summary>
        public List<Medication> InvolvedMedications { get; set; }

        /// <summary>
        /// List of alternative medications suggested if a conflict exists.
        /// </summary>
        public List<Medication> AlternativeMedications { get; set; }
    }
}
