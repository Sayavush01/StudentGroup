using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using StudentGroup.Data;
using StudentGroup.DTOs.EventDtos;
using System.Threading.Tasks;

namespace StudentGroup.Controllers
{
    [Route("api/[controller]")]
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

        public IActionResult GetAllEvents()
        {
            var events = _context.Events.ToList();
            var eventDtos = _mapper.Map<List<DTOs.EventDtos.EventGetdto>>(events);
            return Ok(eventDtos);
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var eventEntity = await _context.Events.FindAsync(id);
            if (eventEntity == null)
            {
                return NotFound();
            }

            var eventDto = _mapper.Map<DTOs.EventDtos.EventGetdto>(eventEntity);
            return Ok(eventDto);
        }

        [HttpPost]
        public async Task<IActionResult> CreateEvent(EventCreateDto eventCreateDto)
        {
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
            {
                return NotFound();
            }
            _mapper.Map(eventUpdateDto, eventEntity);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            var eventEntity = await _context.Events.FindAsync(id);
            if (eventEntity == null)
            {
                return NotFound();
            }
            _context.Events.Remove(eventEntity);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
