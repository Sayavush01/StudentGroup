using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentGroup.Data;
using StudentGroup.DTOs.TicketDtos;
using StudentGroup.Entities;
using StudentGroup.Models;
using System.Security.Claims;

namespace StudentGroup.Controllers
{
    [Route("api/tickets")]
    [ApiController]
    [Authorize]
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
        public async Task<IActionResult> GetAllTickets()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var tickets = await _context.Tickets
                .Include(t => t.Event)
                .ThenInclude(e => e.Organizer)
                .Where(t => t.Event.Organizer.AppUserId == userId)
                .ToListAsync();

            var ticketDtos = _mapper.Map<List<TicketGetDto>>(tickets);

            return Ok(new ApiResponse<List<TicketGetDto>>
            {
                Success = true,
                Message = "Tickets retrieved successfully.",
                Data = ticketDtos
            });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var ticketEntity = await _context.Tickets
                .Include(t => t.Event)
                .ThenInclude(e => e.Organizer)
                .FirstOrDefaultAsync(t => t.Id == id && t.Event.Organizer.AppUserId == userId);

            if (ticketEntity == null)
            {
                return NotFound(new ApiResponse<string>
                {
                    Success = false,
                    Message = "Ticket not found or access denied.",
                    Data = null
                });
            }

            var ticketDto = _mapper.Map<TicketGetDto>(ticketEntity);

            return Ok(new ApiResponse<TicketGetDto>
            {
                Success = true,
                Message = "Ticket retrieved successfully.",
                Data = ticketDto
            });
        }

        [HttpPost]
        public async Task<IActionResult> CreateTicket([FromBody] TicketCreate ticketCreateDto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var eventExists = await _context.Events
                .Include(e => e.Organizer)
                .AnyAsync(e => e.Id == ticketCreateDto.EventId && e.Organizer.AppUserId == userId);

            if (!eventExists)
            {
                return BadRequest(new ApiResponse<string>
                {
                    Success = false,
                    Message = "Event does not exist or does not belong to you.",
                    Data = null
                });
            }

            var ticketEntity = _mapper.Map<Ticket>(ticketCreateDto);

            await _context.Tickets.AddAsync(ticketEntity);
            await _context.SaveChangesAsync();

            var result = _mapper.Map<TicketGetDto>(ticketEntity);

            return CreatedAtAction(
                nameof(GetById),
                new { id = ticketEntity.Id },
                new ApiResponse<TicketGetDto>
                {
                    Success = true,
                    Message = "Ticket created successfully.",
                    Data = result
                });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTicket(int id, [FromBody] TicketUpdateDto ticketUpdateDto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var ticketEntity = await _context.Tickets
                .Include(t => t.Event)
                .ThenInclude(e => e.Organizer)
                .FirstOrDefaultAsync(t => t.Id == id && t.Event.Organizer.AppUserId == userId);

            if (ticketEntity == null)
            {
                return NotFound(new ApiResponse<string>
                {
                    Success = false,
                    Message = "Ticket not found or access denied.",
                    Data = null
                });
            }

            _mapper.Map(ticketUpdateDto, ticketEntity);

            await _context.SaveChangesAsync();

            return Ok(new ApiResponse<string>
            {
                Success = true,
                Message = "Ticket updated successfully.",
                Data = null
            });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTicket(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var ticketEntity = await _context.Tickets
                .Include(t => t.Event)
                .ThenInclude(e => e.Organizer)
                .FirstOrDefaultAsync(t => t.Id == id && t.Event.Organizer.AppUserId == userId);

            if (ticketEntity == null)
            {
                return NotFound(new ApiResponse<string>
                {
                    Success = false,
                    Message = "Ticket not found or access denied.",
                    Data = null
                });
            }

            _context.Tickets.Remove(ticketEntity);
            await _context.SaveChangesAsync();

            return Ok(new ApiResponse<string>
            {
                Success = true,
                Message = "Ticket deleted successfully.",
                Data = null
            });
        }
    }
}