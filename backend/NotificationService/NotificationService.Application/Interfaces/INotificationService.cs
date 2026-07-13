using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NotificationService.Application.DTOs;

namespace NotificationService.Application.Interfaces
{
    public interface INotificationService
    {
        Task<NotificationDto> CreateNotificationAsync(CreateNotificationRequest request, CancellationToken cancellationToken = default);
        Task<IEnumerable<NotificationDto>> GetNotificationsForUserAsync(int recipientId, CancellationToken cancellationToken = default);
        Task<NotificationDto> GetNotificationByIdAsync(int id, CancellationToken cancellationToken = default);
        Task MarkAsReadAsync(int id, CancellationToken cancellationToken = default);
        Task MarkAsUnreadAsync(int id, CancellationToken cancellationToken = default);
        Task DeleteNotificationAsync(int id, CancellationToken cancellationToken = default);
    }
}
