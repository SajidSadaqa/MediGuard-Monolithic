using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MedicationService.Models;

namespace MedicationService.Services
{
    public interface IExternalMedicationApiService
    {
        /// <summary>
        /// Retrieves a medication by id.
        /// </summary>
        /// <param name="id">Medication identifier</param>
        /// <returns>A Medication object or null if not found.</returns>
        Task<Medication> GetMedicationByIdAsync(string id);

        /// <summary>
        /// Retrieves alternative medications for a given drug id.
        /// </summary>
        /// <param name="drugId">Identifier of the medication to find alternatives for.</param>
        /// <returns>A list of Medication alternatives or null if not available.</returns>
        Task<List<Medication>> GetAlternativesAsync(string drugId);
    }

    public class ExternalMedicationApiService : IExternalMedicationApiService
    {
        private readonly HttpClient _httpClient;
        private readonly IMemoryCache _cache;
        private readonly IConfiguration _configuration;
        private readonly ILogger<ExternalMedicationApiService> _logger;

        public ExternalMedicationApiService(
            HttpClient httpClient,
            IMemoryCache cache,
            IConfiguration configuration,
            ILogger<ExternalMedicationApiService> logger)
        {
            _httpClient = httpClient;
            _cache = cache;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<Medication> GetMedicationByIdAsync(string id)
        {
            // First, check if the medication data is already cached.
            if (_cache.TryGetValue(id, out Medication cachedMedication))
            {
                _logger.LogInformation($"Retrieved medication {id} from cache.");
                return cachedMedication;
            }

            try
            {
                // Build the URL for the external API. For example:
                // e.g., "https://externalapi.com/api/medications/{id}"
                string baseUrl = _configuration["ExternalApi:BaseUrl"];
                string requestUrl = $"{baseUrl}/medications/{id}";

                HttpResponseMessage response = await _httpClient.GetAsync(requestUrl);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning($"External API returned status code {response.StatusCode} for medication id: {id}");
                    return null;
                }

                string jsonResponse = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                Medication medication = JsonSerializer.Deserialize<Medication>(jsonResponse, options);

                // Cache the result for future requests (e.g., 30 minutes)
                _cache.Set(id, medication, TimeSpan.FromMinutes(30));

                return medication;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching medication with id {id} from external API.");
                return null;
            }
        }

        public async Task<List<Medication>> GetAlternativesAsync(string drugId)
        {
            try
            {
                // Build the URL for fetching alternative medications.
                // e.g., "https://externalapi.com/api/medications/{drugId}/alternatives"
                string baseUrl = _configuration["ExternalApi:BaseUrl"];
                string requestUrl = $"{baseUrl}/medications/{drugId}/alternatives";

                HttpResponseMessage response = await _httpClient.GetAsync(requestUrl);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning($"External API returned status code {response.StatusCode} for alternatives of medication id: {drugId}");
                    return null;
                }

                string jsonResponse = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                List<Medication> alternatives = JsonSerializer.Deserialize<List<Medication>>(jsonResponse, options);

                return alternatives;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching alternatives for medication id {drugId} from external API.");
                return null;
            }
        }
    }
}
