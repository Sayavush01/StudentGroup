using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using StudentGroup.Data;
using StudentGroup.DTOs.OrganizerDtos;
using StudentGroup.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StudentGroup.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
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
        public IActionResult GetAllOrganizers()
        {
            var organizers = _context.Organizers.ToList();
            var organizerDtos = _mapper.Map<List<OrganizerGetDto>>(organizers);
            return Ok(organizerDtos);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var organizerEntity = await _context.Organizers.FindAsync(id);
            if (organizerEntity == null)
            {
                return NotFound();
            }

            var organizerDto = _mapper.Map<OrganizerGetDto>(organizerEntity);
            return Ok(organizerDto);
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrganizer(OrganizerCreate organizerCreate)
        {
            var organizerEntity = _mapper.Map<Organizer>(organizerCreate);
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
            {
                return NotFound();
            }
            _mapper.Map(organizerUpdateDto, organizerEntity);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrganizer(int id)
        {
            var organizerEntity = await _context.Organizers.FindAsync(id);
            if (organizerEntity == null)
            {
                return NotFound();
            }
            _context.Organizers.Remove(organizerEntity);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}