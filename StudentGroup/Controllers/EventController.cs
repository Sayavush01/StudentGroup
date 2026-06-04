using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentGroup.Data;
using StudentGroup.DTOs.EventDtos;
using StudentGroup.DTOs.OrganizerDtos;
using StudentGroup.DTOs.TicketDtos;
using StudentGroup.Entities;
using StudentGroup.Models;
using System.Security.Claims;

namespace StudentGroup.Controllers
{
    [Route("api/events")]
    [ApiController]
    [Authorize]
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
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var events = await _context.Events
                .Include(e => e.Organizer)
                .Where(e => e.Organizer.AppUserId == userId)
                .ToListAsync();
            var eventDtos = _mapper.Map<List<EventGetdto>>(events);

            return Ok(new ApiResponse<List<EventGetdto>>
            {
                Success = true,
                Message = "Events retrieved successfully.",
                Data = eventDtos
            });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var eventEntity = await _context.Events
                .Include(e => e.Organizer)
                .FirstOrDefaultAsync(e => e.Id == id && e.Organizer.AppUserId == userId);

            if (eventEntity == null)
                return NotFound();

            var eventDto = _mapper.Map<EventGetdto>(eventEntity);

            if (eventEntity == null)
            {
                return NotFound(new ApiResponse<string>
                {
                    Success = false,
                    Message = "Event not found.",
                    Data = null
                });
            }
            return Ok(new ApiResponse<EventGetdto>
            {
                Success = true,
                Message = "Event retrieved successfully.",
                Data = eventDto
            });
        }

        [HttpPost]
        public async Task<IActionResult> CreateEvent([FromBody] EventCreateDto eventCreateDto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var organizerExists = await _context.Organizers.AnyAsync(o => o.Id == eventCreateDto.OrganizerId && o.AppUserId == userId);
            if (!organizerExists)
            {
                return BadRequest(new ApiResponse<string>
                {
                    Success = false,
                    Message = $"Organizer with ID {eventCreateDto.OrganizerId} does not exist or does not belong to you."
                });
            }

            var eventEntity = _mapper.Map<Event>(eventCreateDto);

            await _context.Events.AddAsync(eventEntity);
            await _context.SaveChangesAsync();

            var result = _mapper.Map<EventGetdto>(eventEntity);

            return CreatedAtAction(
         nameof(GetById),
         new { id = eventEntity.Id },
         new ApiResponse<EventGetdto>
         {
             Success = true,
             Message = "Event created successfully.",
             Data = result
         });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEvent(int id, [FromBody] EventUpdatedto eventUpdateDto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var eventEntity = await _context.Events
                .Include(e => e.Organizer)
                .FirstOrDefaultAsync(e => e.Id == id && e.Organizer.AppUserId == userId);

            if (eventEntity == null)
                return NotFound();

            _mapper.Map(eventUpdateDto, eventEntity);

            await _context.SaveChangesAsync();

            return Ok(new ApiResponse<string>
            {
                Success = true,
                Message = "Event updated successfully."
            });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var eventEntity = await _context.Events
                .Include(e => e.Organizer)
                .FirstOrDefaultAsync(e => e.Id == id && e.Organizer.AppUserId == userId);

            if (eventEntity == null)
                return NotFound();

            _context.Events.Remove(eventEntity);
            await _context.SaveChangesAsync();

            return Ok(new ApiResponse<string>
            {
                Success = true,
                Message = "Event deleted successfully."
            });
        }

        [HttpGet("{eventId}/tickets")]
        public async Task<IActionResult> GetTicketsForEvent(int eventId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var eventExists = await _context.Events
                .Include(e => e.Organizer)
                .AnyAsync(e => e.Id == eventId && e.Organizer.AppUserId == userId);

            if (!eventExists)
                return NotFound("Event not found or access denied.");

            var tickets = await _context.Tickets
                .Where(t => t.EventId == eventId)
                .ToListAsync();

            var result = _mapper.Map<List<TicketGetDto>>(tickets);

            return Ok(result);
        }

        [HttpPost("{eventId}/tickets")]
        public async Task<IActionResult> CreateTicketForEvent(int eventId, [FromBody] TicketCreate dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var eventExists = await _context.Events
                .Include(e => e.Organizer)
                .AnyAsync(e => e.Id == eventId && e.Organizer.AppUserId == userId);

            if (!eventExists)
                return NotFound("Event not found or access denied.");

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
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var eventEntity = await _context.Events
                .Include(e => e.Organizer)
                .FirstOrDefaultAsync(e => e.Id == eventId && e.Organizer.AppUserId == userId);

            if (eventEntity == null)
                return NotFound("Event not found or access denied.");

            var result = _mapper.Map<OrganizerGetDto>(eventEntity.Organizer);

            return Ok(result);
        }

        [HttpPost("{eventId}/banner")]
        public async Task<IActionResult> UploadBanner(int eventId, IFormFile file)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var eventEntity = await _context.Events
                .Include(e => e.Organizer)
                .FirstOrDefaultAsync(e => e.Id == eventId && e.Organizer.AppUserId == userId);

            if (eventEntity == null)
                return NotFound("Event not found or access denied.");

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

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Banner uploaded successfully.",
                Data = new
                {
                    bannerImageUrl = eventEntity.BannerImageUrl
                }
            });
        }
    }
}