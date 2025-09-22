using Art.OnShift.Shared.Interfaces;
using Art.OnShift.Scheduler.Data;
using Art.OnShift.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Art.OnShift.Scheduler.Services
{
    public class EventAuditService : IEventAuditService
    {
        private readonly AppDbContext _context;

        public EventAuditService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<EventAuditViewModel>> GetAuditEventsAsync()
        {
            return await _context.EventsAudits
                .Select(a => new EventAuditViewModel
                {
                    EventId = a.EventId,
                    Operation = a.Operation,
                    Start = a.Start,
                    End = a.End,
                    Level1Name = _context.Users.Where(u => u.Id == a.Level1Id).Select(u => u.Name).FirstOrDefault(),
                    Level2Name = _context.Users.Where(u => u.Id == a.Level2Id).Select(u => u.Name).FirstOrDefault(),
                    Level3Name = _context.Users.Where(u => u.Id == a.Level3Id).Select(u => u.Name).FirstOrDefault(),
                    PreviousLevel1Name = _context.Users.Where(u => u.Id == a.PreviousLevel1Id).Select(u => u.Name).FirstOrDefault(),
                    PreviousLevel2Name = _context.Users.Where(u => u.Id == a.PreviousLevel2Id).Select(u => u.Name).FirstOrDefault(),
                    PreviousLevel3Name = _context.Users.Where(u => u.Id == a.PreviousLevel3Id).Select(u => u.Name).FirstOrDefault(),
                    Title = a.Title,
                    PreviousTitle = a.PreviousTitle,
                    EventDate = a.EventDate,
                    PreviousStart = a.PreviousStart,
                    PreviousEnd = a.PreviousEnd,
                    UserId = a.UserId
                })
                .ToListAsync();
        }

    }
}
