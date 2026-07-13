using System.Collections.Generic;
using System.Threading.Tasks;

namespace NotificationService.Application.DTOs
{
    public class NotificationDto
    {
        public int NotificationId { get; set; }
        public int RecipientId { get; set; }
        public string Message { get; set; } = string.Empty;
        public System.DateTime Date { get; set; }
        public bool ReadStatus { get; set; }
    }
}
