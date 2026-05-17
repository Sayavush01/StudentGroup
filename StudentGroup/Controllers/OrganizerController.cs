using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentGroup.Data;
using StudentGroup.DTOs.EventDtos;
using StudentGroup.DTOs.OrganizerDtos;
using StudentGroup.Entities;

namespace StudentGroup.Controllers
{
    [Route("api/organizers")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class OrganizerController : ControllerBase
    {
        private readonly EventManagementDb _context;
        private readonly IMapper _mapper;

        public OrganizerController(EventManagementDb context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllOrganizers()
        {
            var organizers = await _context.Organizers.ToListAsync();
            var organizerDtos = _mapper.Map<List<OrganizerGetDto>>(organizers);

            return Ok(organizerDtos);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var organizerEntity = await _context.Organizers.FindAsync(id);

            if (organizerEntity == null)
                return NotFound();

            var organizerDto = _mapper.Map<OrganizerGetDto>(organizerEntity);

            return Ok(organizerDto);
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrganizer(OrganizerCreate organizerCreateDto)
        {
            var organizerEntity = _mapper.Map<Organizer>(organizerCreateDto);

            await _context.Organizers.AddAsync(organizerEntity);
            await _context.SaveChangesAsync();

            var result = _mapper.Map<OrganizerGetDto>(organizerEntity);

            return CreatedAtAction(nameof(GetById), new { id = organizerEntity.Id }, result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateOrganizer(int id, OrganizerUpdateDto organizerUpdateDto)
        {
            var organizerEntity = await _context.Organizers.FindAsync(id);

            if (organizerEntity == null)
                return NotFound();

            _mapper.Map(organizerUpdateDto, organizerEntity);

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrganizer(int id)
        {
            var organizerEntity = await _context.Organizers.FindAsync(id);

            if (organizerEntity == null)
                return NotFound();

            _context.Organizers.Remove(organizerEntity);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("{organizerId}/events")]
        public async Task<IActionResult> GetEventsByOrganizer(int organizerId)
        {
            var organizerExists = await _context.Organizers.AnyAsync(o => o.Id == organizerId);

            if (!organizerExists)
                return NotFound("Organizer not found.");

            var events = await _context.Events
                .Where(e => e.OrganizerId == organizerId)
                .ToListAsync();

            var result = _mapper.Map<List<EventGetdto>>(events);

            return Ok(result);
        }

        [HttpPost("{organizerId}/logo")]
        public async Task<IActionResult> UploadLogo(int organizerId, IFormFile file)
        {
            var organizer = await _context.Organizers.FindAsync(organizerId);

            if (organizer == null)
                return NotFound("Organizer not found.");

            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            var folderPath = Path.Combine("wwwroot", "uploads", "organizers");

            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            var fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
            var filePath = Path.Combine(folderPath, fileName);

            using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream);

            organizer.LogoUrl = $"/uploads/organizers/{fileName}";

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Logo uploaded successfully.",
                logoUrl = organizer.LogoUrl
            });
        }
    }
}