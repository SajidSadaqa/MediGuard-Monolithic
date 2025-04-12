namespace MediGuard.API.DTOs
{
    public class ChatMessageDto
    {
        public int Id { get; set; }
        public string Message { get; set; } = string.Empty;
        public bool IsFromUser { get; set; }
        public DateTime Timestamp { get; set; }
        public string? Context { get; set; }
    }

    public class ChatRequestDto
    {
        public string Message { get; set; } = string.Empty;
        public string? Context { get; set; }
    }

    public class ChatResponseDto
    {
        public string Response { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public List<RecommendationDto>? Recommendations { get; set; }
    }
}
