namespace NotificationService.Application.DTOs
{
    public class CreateNotificationRequest
    {
        public int RecipientId { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
