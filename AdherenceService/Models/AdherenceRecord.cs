using System;

namespace AdherenceService.Models
{
    public class AdherenceRecord
    {
        /// <summary>
        /// The identifier of the user.
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// The identifier of the medication.
        /// </summary>
        public string MedicationId { get; set; }

        /// <summary>
        /// The scheduled time for the medication dose.
        /// </summary>
        public DateTime ScheduledDoseTime { get; set; }

        /// <summary>
        /// The actual time when the dose was taken (if taken).
        /// </summary>
        public DateTime? ActualDoseTime { get; set; }

        /// <summary>
        /// Indicates whether the dose was taken.
        /// </summary>
        public bool IsDoseTaken { get; set; }

        /// <summary>
        /// Indicates whether an alert has been triggered for a missed dose.
        /// </summary>
        public bool AlertTriggered { get; set; }

        /// <summary>
        /// Additional notes or details.
        /// </summary>
        public string Note { get; set; }
    }
}
