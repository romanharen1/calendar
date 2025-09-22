using Art.OnShift.Shared.Models;

namespace Art.OnShift.Shared.Interfaces
{
    public interface IEventAuditService
    {
        /// <summary>
        /// Retrieves a list of audit events.
        /// </summary>
        /// <returns>A list of EventAuditViewModel containing audit event details.</returns>
        Task<List<EventAuditViewModel>> GetAuditEventsAsync();
    }
}
