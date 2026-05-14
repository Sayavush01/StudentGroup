using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using StudentGroup.Data;
using StudentGroup.DTOs.TicketDtos;
using StudentGroup.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StudentGroup.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TicketController : ControllerBase
    {
        private readonly EventManagementDb _context;
        private readonly IMapper _mapper;

        public TicketController(EventManagementDb context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet]
        public IActionResult GetAllTickets()
        {
            var tickets = _context.Tickets.ToList();
            var ticketDtos = _mapper.Map<List<TicketGetDto>>(tickets);
            return Ok(ticketDtos);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var ticketEntity = await _context.Tickets.FindAsync(id);
            if (ticketEntity == null)
            {
                return NotFound();
            }

            var ticketDto = _mapper.Map<TicketGetDto>(ticketEntity);
            return Ok(ticketDto);
        }

        [HttpPost]
        public async Task<IActionResult> CreateTicket(TicketCreate ticketCreate)
        {
            var ticketEntity = _mapper.Map<Ticket>(ticketCreate);
            await _context.Tickets.AddAsync(ticketEntity);
            await _context.SaveChangesAsync();
            var result = _mapper.Map<TicketGetDto>(ticketEntity);

            return CreatedAtAction(nameof(GetById), new { id = ticketEntity.Id }, result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTicket(int id, TicketUpdateDto ticketUpdateDto)
        {
            var ticketEntity = await _context.Tickets.FindAsync(id);
            if (ticketEntity == null)
            {
                return NotFound();
            }
            _mapper.Map(ticketUpdateDto, ticketEntity);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTicket(int id)
        {
            var ticketEntity = await _context.Tickets.FindAsync(id);
            if (ticketEntity == null)
            {
                return NotFound();
            }
            _context.Tickets.Remove(ticketEntity);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}