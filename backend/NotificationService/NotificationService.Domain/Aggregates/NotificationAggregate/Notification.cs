using System;

namespace NotificationService.Domain.Aggregates.NotificationAggregate
{
    /// <summary>
    /// The single Aggregate Root for the Notification context.
    /// </summary>
    public sealed class Notification
    {
        public int NotificationId { get; private set; }
        
        // External Aggregate Reference - NO navigation property mapped!
        public int RecipientId { get; private set; }
        
        public string Message { get; private set; } = string.Empty;
        
        public DateTime Date { get; private set; }
        
        public bool ReadStatus { get; private set; }

        // Required by EF Core for instantiation
        private Notification() { }

        public Notification(int recipientId, string message, DateTime date, bool readStatus = false)
        {
            if (string.IsNullOrWhiteSpace(message))
                throw new ArgumentException("Notification message cannot be empty.", nameof(message));
                
            if (recipientId <= 0)
                throw new ArgumentException("A valid RecipientId must be provided.", nameof(recipientId));

            RecipientId = recipientId;
            Message = message;
            Date = date;
            ReadStatus = readStatus;
        }

        public static Notification CreateNotification(int recipientId, string message)
        {
            return new Notification(recipientId, message, DateTime.UtcNow);
        }

        public void MarkAsRead()
        {
            if (ReadStatus)
                throw new Domain.Exceptions.DomainException("Notification is already marked as read.");

            ReadStatus = true;
        }

        public void MarkAsUnread()
        {
            if (!ReadStatus)
                throw new Domain.Exceptions.DomainException("Notification is already unread.");

            ReadStatus = false;
        }
    }
}
