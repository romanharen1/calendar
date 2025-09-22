
namespace Art.OnShift.Shared.Models;

public class EventNotificationRequest
{
    public int EventId { get; set; }
    public string? Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public string NotificationType { get; set; } = "upcoming"; // upcoming, confirmed, cancelled
    public int MinutesUntilStart { get; set; }
    public DateTime NotificationSentAt { get; set; } = DateTime.UtcNow;
    public string? PhoneNumber { get; set; }
}