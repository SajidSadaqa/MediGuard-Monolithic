using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MedicationService.Models;

namespace MedicationService.Services
{
    public interface IAiConflictCheckService
    {
        /// <summary>
        /// Checks for conflicts among the given medications by calling an AI API.
        /// </summary>
        /// <param name="medications">Array of medication identifiers or names.</param>
        /// <returns>A Conflict object with severity, message, involved medications, and alternatives; null if an error occurs.</returns>
        Task<Conflict> CheckConflictsAsync(string[] medications);
    }

    public class AiConflictCheckService : IAiConflictCheckService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AiConflictCheckService> _logger;

        public AiConflictCheckService(HttpClient httpClient, IConfiguration configuration, ILogger<AiConflictCheckService> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<Conflict> CheckConflictsAsync(string[] medications)
        {
            try
            {
                // Retrieve the AI API endpoint from configuration (e.g., appsettings.json under AiApi:ConflictCheckEndpoint)
                string aiEndpoint = _configuration["AiApi:ConflictCheckEndpoint"];
                if (string.IsNullOrWhiteSpace(aiEndpoint))
                {
                    _logger.LogError("AI API conflict check endpoint is not configured.");
                    return null;
                }

                // Prepare the request payload.
                // For example, the API might expect a JSON payload like:
                // { "medications": [ "med1", "med2", "med3" ] }
                var payload = new { medications = medications };
                string jsonPayload = JsonSerializer.Serialize(payload);
                var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                // Send a POST request to the AI API.
                HttpResponseMessage response = await _httpClient.PostAsync(aiEndpoint, content);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning($"AI API returned status code {response.StatusCode} when checking conflicts.");
                    return null;
                }

                string jsonResponse = await response.Content.ReadAsStringAsync();

                // Deserialize the response into a Conflict object.
                // Ensure that your AI API response structure matches your Conflict model.
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                Conflict conflictResult = JsonSerializer.Deserialize<Conflict>(jsonResponse, options);

                return conflictResult;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while checking medication conflicts using the AI API.");
                return null;
            }
        }
    }
}
