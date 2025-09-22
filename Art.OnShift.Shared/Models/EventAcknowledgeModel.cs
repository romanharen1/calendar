namespace Art.OnShift.Shared.Models;

public class EventAcknowledgeModel
{
    public int Id { get; set; }
    public int EventId { get; set; }
    public bool Acknowledged { get; set; } = false;
    public string? AcknowledgedBy { get; set; }
    public DateTime? AckTime { get; set; }
    public DateTime NotificationSentAt { get; set; } = DateTime.UtcNow;
}