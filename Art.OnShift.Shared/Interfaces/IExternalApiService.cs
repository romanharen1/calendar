using Art.OnShift.Shared.Models;

namespace Art.OnShift.Shared.Interfaces;

public interface IExternalApiService
{
    Task<bool> NotifyUpcomingEventAsync(EventNotificationRequest request);
    Task<bool> NotifyEventCancellationAsync(int eventId, string reason);
}