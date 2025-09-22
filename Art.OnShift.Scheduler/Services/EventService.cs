using Art.OnShift.Scheduler.Data;
using Art.OnShift.Shared.Interfaces;
using Art.OnShift.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Art.OnShift.Scheduler.Services
{
    public class EventService : IEventService
    {
        private readonly AppDbContext _context;

        public EventService(AppDbContext context)
        {
            _context = context;
        }


        public async Task<List<EventModel>> GetEventsAsync()
        {
            return await _context.Events
                .Include(e => e.Level1)
                .Include(e => e.Level2)
                .Include(e => e.Level3)
                .ToListAsync();
        }


        public async Task<EventModel> GetEventByIdAsync(int id)
        {
            return await _context.Events
                .Include(e => e.Level1)
                .Include(e => e.Level2)
                .Include(e => e.Level3)
                .FirstOrDefaultAsync(e => e.Id == id);
        }


        public async Task CreateEventAsync(EventModel eventModel)
        {
            bool isAvailable = await IsTimeSlotAvailableAsync(eventModel.Start, eventModel.End);

            if (!isAvailable)
            {
                throw new InvalidOperationException("The selected time slot is not available. Please choose another time.");
            }
            _context.Events.Add(eventModel);
            await _context.SaveChangesAsync();
        }


        public async Task UpdateEventAsync(EventModel eventModel)
        {
            _context.Events.Update(eventModel);
            await _context.SaveChangesAsync();
        }


        public async Task DeleteEventAsync(int id)
        {
            var eventModel = await _context.Events.FindAsync(id);
            if (eventModel != null)
            {
                _context.Events.Remove(eventModel);
                await _context.SaveChangesAsync();
            }
        }


        public async Task<bool> IsTimeSlotAvailableAsync(DateTime? start, DateTime? end)
        {
            var conflictingEvents = await _context.Events
                .Where(e =>
                    start < e.End && end > e.Start)
                .ToListAsync();

            return !conflictingEvents.Any();
        }


        public async Task<EventModel> GetCurrentEvent()
        {
            var currentTime = DateTime.UtcNow;
            return await _context.Events
                .Include(e => e.Level1)
                .Include(e => e.Level2)
                .Include(e => e.Level3)
                .FirstOrDefaultAsync(e => e.Start <= currentTime && e.End >= currentTime);
        }


        public async Task<EventModel?> GetNextEventAsync()
        {
            //Não obtém evento passado
            //Não obtém evento confirmado pelo analista
            var now = DateTime.UtcNow;

            return await _context.Events
                .Include(e => e.Level1) // Inclui os dados do Level1
                .Where(e => e.Start > now &&
                           (!_context.EventAcknowledges.Any(ea => ea.EventId == e.Id) ||
                            _context.EventAcknowledges.Where(ea => ea.EventId == e.Id).Any(ea => ea.Acknowledged == false)))
                .OrderBy(e => e.Start)
                .FirstOrDefaultAsync();
        }


        public async Task CreateAcknowledgeEntryAsync(EventModel eventModel)
        {
            var acknowledgeEntry = new EventAcknowledgeModel
            {
                EventId = eventModel.Id,
                Acknowledged = false,
                AcknowledgedBy = null,
                AckTime = null,
                NotificationSentAt = DateTime.UtcNow
            };

            _context.EventAcknowledges.Add(acknowledgeEntry);
            await _context.SaveChangesAsync();
        }

        public async Task RegisterEventAckAsync(int eventId, string ackBy)
        {
            var ackModel = await _context.EventAcknowledges
                .FirstOrDefaultAsync(a => a.EventId == eventId);

            if (ackModel != null)
            {
                ackModel.Acknowledged = true;
                ackModel.AcknowledgedBy = ackBy;
                ackModel.AckTime = DateTime.UtcNow;

                await _context.SaveChangesAsync();
            }
            else
            {
                throw new InvalidOperationException($"Registro de Ack não encontrado para o EventId: {eventId}");
            }
        }


        public async Task UpdateAckMessageTimeAsync(int eventId)
        {
            var ackModel = await _context.EventAcknowledges.FindAsync(eventId);
            if (ackModel == null)
            {
                throw new InvalidOperationException($"Registro de Ack não Encontrado para o evento ID: {eventId}");
            }
            else
            {
                ackModel.NotificationSentAt = DateTime.UtcNow;
                _context.EventAcknowledges.Update(ackModel);
                await _context.SaveChangesAsync();
            }
        }


        public async Task<EventAcknowledgeModel?> GetAcknowledgeEntryAsync(int eventId)
        {
            return await _context.EventAcknowledges
                .FirstOrDefaultAsync(ea => ea.EventId == eventId);
        }


    }

}
