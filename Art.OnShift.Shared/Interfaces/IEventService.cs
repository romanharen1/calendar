using Art.OnShift.Shared.Models;

namespace Art.OnShift.Shared.Interfaces
{
    public interface IEventService
    {
        Task<List<EventModel>> GetEventsAsync();
        Task<EventModel> GetEventByIdAsync(int id);
        Task CreateEventAsync(EventModel eventModel);
        Task UpdateEventAsync(EventModel eventModel);
        Task DeleteEventAsync(int id);
        Task<bool> IsTimeSlotAvailableAsync(DateTime? start, DateTime? end);
        Task<EventModel> GetCurrentEvent();
        Task<EventModel?> GetNextEventAsync();
        Task CreateAcknowledgeEntryAsync(EventModel eventModel);
        public Task RegisterEventAckAsync(int eventId, string ackBy);
        public Task<EventAcknowledgeModel?> GetAcknowledgeEntryAsync(int eventId);
        public Task UpdateAckMessageTimeAsync(int eventId);
    }
}
