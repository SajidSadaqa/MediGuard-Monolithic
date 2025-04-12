namespace MedicationService.Models
{
    public class Medication
    {
        /// <summary>
        /// Unique identifier for the medication.
        /// </summary>
        public string MedicationId { get; set; }

        /// <summary>
        /// The scientific name of the medication.
        /// </summary>
        public string ScientificName { get; set; }

        /// <summary>
        /// The brand name of the medication.
        /// </summary>
        public string BrandName { get; set; }

        /// <summary>
        /// The price of the medication.
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// A brief description or additional details about the medication.
        /// </summary>
        public string Description { get; set; }
    }
}
