using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Art.OnShift.Scheduler.Data;
using Art.OnShift.Shared.Models;
using Art.OnShift.Shared.Interfaces;

namespace Art.OnShift.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EventsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private IEventService _eventService;
        private readonly ILogger<EventsController> _logger;

        public EventsController(AppDbContext context, IEventService eventService, ILogger<EventsController> logger)
        {
            _context = context;
            _eventService = eventService;
            _logger = logger;
        }

        // GET: api/Events
        [HttpGet]
        public async Task<ActionResult<IEnumerable<EventModel>>> GetEvents()
        {
            return await _context.Events.ToListAsync();
        }


        // GET: api/Events/current
        [HttpGet("current")]
        public async Task<ActionResult<EventModel?>> GetCurrentEvent()
        {
            var now = DateTime.UtcNow;
            var currentEvent = await _context.Events
                .Include(e => e.Level1)
                .Include(e => e.Level2)
                .Include(e => e.Level3)
                .Where(e => e.Start.HasValue && e.End.HasValue &&
                            e.Start.Value <= now && e.End.Value >= now)
                .FirstOrDefaultAsync();

            if (currentEvent == null)
            {
                return NotFound();
            }

            return currentEvent;
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            var eventModel = await _context.Events.FindAsync(id);
            if (eventModel == null)
            {
                return NotFound();
            }

            _context.Events.Remove(eventModel);
            await _context.SaveChangesAsync();

            return NoContent();
        }


        [HttpPost("ack")]
        public async Task<IActionResult> ConfirmEvent()
        {
            var contentType = Request.ContentType?.ToLowerInvariant();

            if (string.IsNullOrEmpty(contentType) || !contentType.StartsWith("application/x-www-form-urlencoded"))
            {
                _logger.LogWarning("Content-Type não suportado: {ContentType}", contentType);
                return BadRequest($"Tipo de conteúdo não suportado: {contentType}");
            }

            var form = await Request.ReadFormAsync();

            // Validar campos obrigatórios
            if (!form.TryGetValue("EventId", out var idValue) || !int.TryParse(idValue, out var varEventId))
                return BadRequest("Campo 'EventId' ausente ou inválido.");

            if (!form.TryGetValue("Digits", out var digitsValue) || !int.TryParse(digitsValue, out var varDigits))
                return BadRequest("Campo 'Digits' ausente ou inválido.");

            // Campos opcionais
            var ackBy = form.TryGetValue("Title", out var ackByValue) ? ackByValue.ToString() : "Unknown";
            var callSid = form.TryGetValue("CallSid", out var callSidValue) ? callSidValue.ToString() : "Unknown";

            // Chamar serviço para registrar o ACK
            await _eventService.RegisterEventAckAsync(varEventId, ackBy);

            _logger.LogInformation("ID: {Id}, ACK: {Digits}, ACK By: {AckBy}, Number: {CallSid}",
                varEventId, varDigits, ackBy, callSid);

            return Ok(new { Message = "Confirmação registrada com sucesso." });
        }
    }
}