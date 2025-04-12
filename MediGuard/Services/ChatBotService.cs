using MediGuard.API.DTOs;
using System.Text.Json;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using MediGuard.API.Data;
using Microsoft.EntityFrameworkCore;
using MediGuard.API.Models;

namespace MediGuard.API.Services
{
    public interface IChatBotService
    {
        Task<ChatResponseDto> GetChatResponseAsync(string userId, ChatRequestDto chatRequest);
        Task<List<ChatMessageDto>> GetUserChatHistoryAsync(string userId);
        Task<bool> SaveChatMessageAsync(string userId, string message, bool isFromUser, string? context = null);
    }

    public class ChatBotService : IChatBotService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<ChatBotService> _logger;
        private readonly HttpClient _httpClient;
        private readonly AppDbContext _context;
        private readonly IMedicationService _medicationService;

        public ChatBotService(
            IConfiguration configuration,
            ILogger<ChatBotService> logger,
            HttpClient httpClient,
            AppDbContext context,
            IMedicationService medicationService)
        {
            _configuration = configuration;
            _logger = logger;
            _httpClient = httpClient;
            _context = context;
            _medicationService = medicationService;
        }

        public async Task<ChatResponseDto> GetChatResponseAsync(string userId, ChatRequestDto chatRequest)
        {
            try
            {
                // Save user message to database
                await SaveChatMessageAsync(userId, chatRequest.Message, true, chatRequest.Context);

                // Get user information for context
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    throw new ApplicationException("User not found");
                }

                // Get user medications for context
                var userMedications = await _context.UserMedications
                    .Where(um => um.UserId == userId && um.IsActive)
                    .Include(um => um.Medication)
                    .ToListAsync();

                // Prepare DeepSeek API request
                var deepSeekRequest = new
                {
                    model = "deepseek-chat",
                    messages = new[]
                    {
                        new { role = "system", content = GetSystemPrompt(user, userMedications) },
                        new { role = "user", content = chatRequest.Message }
                    },
                    temperature = 0.7,
                    max_tokens = 1000
                };

                // Call DeepSeek API (simulated in this implementation)
                var response = await SimulateDeepSeekApiCall(deepSeekRequest, chatRequest.Message);

                // Save bot response to database
                await SaveChatMessageAsync(userId, response.Response, false, chatRequest.Context);

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting chat response");
                throw;
            }
        }

        public async Task<List<ChatMessageDto>> GetUserChatHistoryAsync(string userId)
        {
            var chatHistory = await _context.ChatMessages
                .Where(cm => cm.UserId == userId)
                .OrderBy(cm => cm.Timestamp)
                .ToListAsync();

            return chatHistory.Select(cm => new ChatMessageDto
            {
                Id = cm.Id,
                Message = cm.Message,
                IsFromUser = cm.IsFromUser,
                Timestamp = cm.Timestamp,
                Context = cm.Context
            }).ToList();
        }

        public async Task<bool> SaveChatMessageAsync(string userId, string message, bool isFromUser, string? context = null)
        {
            var chatMessage = new ChatMessage
            {
                UserId = userId,
                Message = message,
                IsFromUser = isFromUser,
                Timestamp = DateTime.UtcNow,
                Context = context
            };

            _context.ChatMessages.Add(chatMessage);
            await _context.SaveChangesAsync();
            return true;
        }

        private string GetSystemPrompt(ApplicationUser user, List<UserMedication> userMedications)
        {
            var medicationNames = userMedications.Select(um => um.Medication?.Name).Where(n => n != null);
            
            return $@"You are MediGuard's AI health assistant. You provide helpful, accurate, and concise information about medications and health.

User Information:
- Conditions: {user.Conditions ?? "None specified"}
- Allergies: {user.Allergies ?? "None specified"}
- Current Medications: {string.Join(", ", medicationNames)}

Guidelines:
1. Provide accurate medical information and always recommend consulting healthcare professionals for medical advice.
2. Be aware of the user's conditions and allergies when discussing medications.
3. If asked about medication conflicts, be cautious and highlight potential issues.
4. Never recommend medications that conflict with the user's current medications or allergies.
5. Keep responses concise, informative, and easy to understand.
6. If you don't know something, admit it rather than providing incorrect information.
7. Focus on providing factual information about medications, side effects, and general health advice.";
        }

        private async Task<ChatResponseDto> SimulateDeepSeekApiCall(object request, string userMessage)
        {
            // This is a simulation of the DeepSeek API call
            // In a real implementation, this would make an HTTP request to the DeepSeek API

            // Simulate API latency
            await Task.Delay(500);

            string response;
            List<RecommendationDto>? recommendations = null;

            // Generate a contextual response based on the user message
            if (userMessage.Contains("headache", StringComparison.OrdinalIgnoreCase))
            {
                response = "For headaches, over-the-counter pain relievers like acetaminophen (Tylenol) or ibuprofen (Advil) can be effective. Make sure to follow the recommended dosage. If headaches are severe or persistent, please consult with your healthcare provider.";
                
                // Add recommendations for headache medications
                recommendations = new List<RecommendationDto>
                {
                    new RecommendationDto
                    {
                        Id = 1,
                        MedicationId = 1, // Advil
                        MedicationName = "Advil",
                        RecommendationText = "Effective for headache relief",
                        RecommendationScore = 85,
                        RecommendationReason = "Common first-line treatment for headaches",
                        IsViewed = false,
                        IsAccepted = false,
                        CreatedAt = DateTime.UtcNow
                    },
                    new RecommendationDto
                    {
                        Id = 2,
                        MedicationId = 2, // Tylenol
                        MedicationName = "Tylenol",
                        RecommendationText = "Alternative for headache relief",
                        RecommendationScore = 80,
                        RecommendationReason = "Good option if you have stomach sensitivity",
                        IsViewed = false,
                        IsAccepted = false,
                        CreatedAt = DateTime.UtcNow
                    }
                };
            }
            else if (userMessage.Contains("cold", StringComparison.OrdinalIgnoreCase) || 
                     userMessage.Contains("flu", StringComparison.OrdinalIgnoreCase))
            {
                response = "For cold and flu symptoms, rest and hydration are important. Over-the-counter medications can help manage symptoms. Acetaminophen can reduce fever and pain, while decongestants can help with nasal congestion. Remember that antibiotics are not effective against viral infections like colds and flu.";
            }
            else if (userMessage.Contains("sleep", StringComparison.OrdinalIgnoreCase) || 
                     userMessage.Contains("insomnia", StringComparison.OrdinalIgnoreCase))
            {
                response = "For sleep issues, it's important to practice good sleep hygiene. This includes maintaining a regular sleep schedule, creating a comfortable sleep environment, and avoiding caffeine and electronics before bedtime. If you're considering sleep medications, please consult with your healthcare provider as many have potential side effects and risks.";
            }
            else if (userMessage.Contains("side effect", StringComparison.OrdinalIgnoreCase) || 
                     userMessage.Contains("interaction", StringComparison.OrdinalIgnoreCase))
            {
                response = "Medication side effects and interactions can vary widely between individuals. It's important to read medication labels carefully and consult with your healthcare provider or pharmacist about potential interactions, especially if you're taking multiple medications. Always report unusual symptoms to your healthcare provider.";
            }
            else
            {
                response = "Thank you for your question. To provide the most accurate information, I'd recommend discussing specific medical concerns with your healthcare provider. They can offer personalized advice based on your complete medical history. Is there something specific about your medications or health that I can help clarify?";
            }

            return new ChatResponseDto
            {
                Response = response,
                Timestamp = DateTime.UtcNow,
                Recommendations = recommendations
            };
        }
    }
}
