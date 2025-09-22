using Art.OnShift.Shared.Interfaces;
using Art.OnShift.Shared.Models;

namespace Art.OnShift.Scheduler.Services;

public class ExternalApiService : IExternalApiService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ExternalApiService> _logger;

    public ExternalApiService(HttpClient httpClient, ILogger<ExternalApiService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<bool> NotifyUpcomingEventAsync(EventNotificationRequest request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("/api/events/upcoming", request);
            response.EnsureSuccessStatusCode();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error notifying upcoming event.");
            return false;
        }
    }
    public async Task<bool> NotifyEventCancellationAsync(int eventId, string reason)
    {
        try
        {
            var cancellationRequest = new
            {
                EventId = eventId,
                Reason = reason
            };

            var response = await _httpClient.PostAsJsonAsync("/api/events/cancellation", cancellationRequest);
            response.EnsureSuccessStatusCode();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error notifying event cancellation.");
            return false;
        }
    }
}