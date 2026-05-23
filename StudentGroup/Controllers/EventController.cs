using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentGroup.Data;
using StudentGroup.DTOs.EventDtos;
using StudentGroup.DTOs.OrganizerDtos;
using StudentGroup.DTOs.TicketDtos;
using StudentGroup.Entities;

namespace StudentGroup.Controllers
{
    [Route("api/events")]
    [ApiController]
    public class EventController : ControllerBase
    {
        private readonly EventManagementDb _context;
        private readonly IMapper _mapper;

        public EventController(EventManagementDb context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllEvents()
        {
            var events = await _context.Events.ToListAsync();
            var eventDtos = _mapper.Map<List<EventGetdto>>(events);

            return Ok(eventDtos);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var eventEntity = await _context.Events.FindAsync(id);

            if (eventEntity == null)
                return NotFound();

            var eventDto = _mapper.Map<EventGetdto>(eventEntity);

            return Ok(eventDto);
        }

        [HttpPost]
        public async Task<IActionResult> CreateEvent(EventCreateDto eventCreateDto)
        {
            var organizerExists = await _context.Organizers.AnyAsync(o => o.Id == eventCreateDto.OrganizerId);
            if (!organizerExists)
            {
                return BadRequest($"Organizer with ID {eventCreateDto.OrganizerId} does not exist.");
            }

            var eventEntity = _mapper.Map<Event>(eventCreateDto);

            await _context.Events.AddAsync(eventEntity);
            await _context.SaveChangesAsync();

            var result = _mapper.Map<EventGetdto>(eventEntity);

            return CreatedAtAction(nameof(GetById), new { id = eventEntity.Id }, result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEvent(int id, EventUpdatedto eventUpdateDto)
        {
            var eventEntity = await _context.Events.FindAsync(id);

            if (eventEntity == null)
                return NotFound();

            _mapper.Map(eventUpdateDto, eventEntity);

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            var eventEntity = await _context.Events.FindAsync(id);

            if (eventEntity == null)
                return NotFound();

            _context.Events.Remove(eventEntity);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("{eventId}/tickets")]
        public async Task<IActionResult> GetTicketsForEvent(int eventId)
        {
            var eventExists = await _context.Events.AnyAsync(e => e.Id == eventId);

            if (!eventExists)
                return NotFound("Event not found.");

            var tickets = await _context.Tickets
                .Where(t => t.EventId == eventId)
                .ToListAsync();

            var result = _mapper.Map<List<TicketGetDto>>(tickets);

            return Ok(result);
        }

        [HttpPost("{eventId}/tickets")]
        public async Task<IActionResult> CreateTicketForEvent(int eventId, TicketCreate dto)
        {
            var eventExists = await _context.Events.AnyAsync(e => e.Id == eventId);

            if (!eventExists)
                return NotFound("Event not found.");

            var ticket = _mapper.Map<Ticket>(dto);
            ticket.EventId = eventId;

            await _context.Tickets.AddAsync(ticket);
            await _context.SaveChangesAsync();

            var result = _mapper.Map<TicketGetDto>(ticket);

            return Ok(result);
        }

        [HttpGet("{eventId}/organizer")]
        public async Task<IActionResult> GetOrganizerForEvent(int eventId)
        {
            var eventEntity = await _context.Events
                .Include(e => e.Organizer)
                .FirstOrDefaultAsync(e => e.Id == eventId);

            if (eventEntity == null)
                return NotFound("Event not found.");

            var result = _mapper.Map<OrganizerGetDto>(eventEntity.Organizer);

            return Ok(result);
        }

        [HttpPost("{eventId}/banner")]
        public async Task<IActionResult> UploadBanner(int eventId, IFormFile file)
        {
            var eventEntity = await _context.Events.FindAsync(eventId);

            if (eventEntity == null)
                return NotFound("Event not found.");

            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            var folderPath = Path.Combine("wwwroot", "uploads", "events");

            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            var fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
            var filePath = Path.Combine(folderPath, fileName);

            using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream);

            eventEntity.BannerImageUrl = $"/uploads/events/{fileName}";

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Banner uploaded successfully.",
                bannerImageUrl = eventEntity.BannerImageUrl
            });
        }
    }
}