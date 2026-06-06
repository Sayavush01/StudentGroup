using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentGroup.Data;
using StudentGroup.DTOs.EventDtos;
using StudentGroup.DTOs.OrganizerDtos;
using StudentGroup.Entities;
using StudentGroup.Models;
using System.Security.Claims;

namespace StudentGroup.Controllers
{
    [Route("api/organizers")]
    [ApiController]
    [Authorize]
    public class OrganizerController : ControllerBase
    {
        private readonly EventManagementDb _context;
        private readonly IMapper _mapper;
        private readonly FluentValidation.IValidator<OrganizerCreate> _createValidator;
        private readonly FluentValidation.IValidator<OrganizerUpdateDto> _updateValidator;

        public OrganizerController(
            EventManagementDb context, 
            IMapper mapper, 
            FluentValidation.IValidator<OrganizerCreate> createValidator, 
            FluentValidation.IValidator<OrganizerUpdateDto> updateValidator)
        {
            _context = context;
            _mapper = mapper;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllOrganizers()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var organizers = await _context.Organizers
                .Where(o => o.AppUserId == userId)
                .ToListAsync();

            var organizerDtos = _mapper.Map<List<OrganizerGetDto>>(organizers);

            return Ok(new ApiResponse<List<OrganizerGetDto>>
            {
                Success = true,
                Message = "Organizers retrieved successfully.",
                Data = organizerDtos
            });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var organizerEntity = await _context.Organizers
                .FirstOrDefaultAsync(o => o.Id == id && o.AppUserId == userId);

            if (organizerEntity == null)
            {
                return NotFound(new ApiResponse<string>
                {
                    Success = false,
                    Message = "Organizer not found or access denied.",
                    Data = null
                });
            }

            var organizerDto = _mapper.Map<OrganizerGetDto>(organizerEntity);

            return Ok(new ApiResponse<OrganizerGetDto>
            {
                Success = true,
                Message = "Organizer retrieved successfully.",
                Data = organizerDto
            });
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrganizer([FromBody] OrganizerCreate organizerCreateDto)
        {
            var validationResult = await _createValidator.ValidateAsync(organizerCreateDto);
            if (!validationResult.IsValid)
                return BadRequest(validationResult.Errors);

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var organizerEntity = _mapper.Map<Organizer>(organizerCreateDto);
            organizerEntity.AppUserId = userId;

            await _context.Organizers.AddAsync(organizerEntity);
            await _context.SaveChangesAsync();

            var result = _mapper.Map<OrganizerGetDto>(organizerEntity);

            return CreatedAtAction(
                nameof(GetById),
                new { id = organizerEntity.Id },
                new ApiResponse<OrganizerGetDto>
                {
                    Success = true,
                    Message = "Organizer created successfully.",
                    Data = result
                });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateOrganizer(int id, [FromBody] OrganizerUpdateDto organizerUpdateDto)
        {
            var validationResult = await _updateValidator.ValidateAsync(organizerUpdateDto);
            if (!validationResult.IsValid)
                return BadRequest(validationResult.Errors);

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var organizerEntity = await _context.Organizers
                .FirstOrDefaultAsync(o => o.Id == id && o.AppUserId == userId);

            if (organizerEntity == null)
            {
                return NotFound(new ApiResponse<string>
                {
                    Success = false,
                    Message = "Organizer not found or access denied.",
                    Data = null
                });
            }

            _mapper.Map(organizerUpdateDto, organizerEntity);

            await _context.SaveChangesAsync();

            return Ok(new ApiResponse<string>
            {
                Success = true,
                Message = "Organizer updated successfully.",
                Data = null
            });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrganizer(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var organizerEntity = await _context.Organizers
                .FirstOrDefaultAsync(o => o.Id == id && o.AppUserId == userId);

            if (organizerEntity == null)
            {
                return NotFound(new ApiResponse<string>
                {
                    Success = false,
                    Message = "Organizer not found or access denied.",
                    Data = null
                });
            }

            _context.Organizers.Remove(organizerEntity);
            await _context.SaveChangesAsync();

            return Ok(new ApiResponse<string>
            {
                Success = true,
                Message = "Organizer deleted successfully.",
                Data = null
            });
        }

        [HttpGet("{organizerId}/events")]
        public async Task<IActionResult> GetEventsByOrganizer(int organizerId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var organizerExists = await _context.Organizers
                .AnyAsync(o => o.Id == organizerId && o.AppUserId == userId);

            if (!organizerExists)
            {
                return NotFound(new ApiResponse<string>
                {
                    Success = false,
                    Message = "Organizer not found or access denied.",
                    Data = null
                });
            }

            var events = await _context.Events
                .Where(e => e.OrganizerId == organizerId)
                .ToListAsync();

            var result = _mapper.Map<List<EventGetdto>>(events);

            return Ok(new ApiResponse<List<EventGetdto>>
            {
                Success = true,
                Message = "Organizer events retrieved successfully.",
                Data = result
            });
        }

        [HttpPost("{organizerId}/logo")]
        public async Task<IActionResult> UploadLogo(int organizerId, IFormFile file)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var organizer = await _context.Organizers
                .FirstOrDefaultAsync(o => o.Id == organizerId && o.AppUserId == userId);

            if (organizer == null)
            {
                return NotFound(new ApiResponse<string>
                {
                    Success = false,
                    Message = "Organizer not found or access denied.",
                    Data = null
                });
            }

            if (file == null || file.Length == 0)
            {
                return BadRequest(new ApiResponse<string>
                {
                    Success = false,
                    Message = "No file uploaded.",
                    Data = null
                });
            }

            var folderPath = Path.Combine("wwwroot", "uploads", "organizers");

            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            var fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
            var filePath = Path.Combine(folderPath, fileName);

            using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream);

            organizer.LogoUrl = $"/uploads/organizers/{fileName}";

            await _context.SaveChangesAsync();

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Logo uploaded successfully.",
                Data = new
                {
                    logoUrl = organizer.LogoUrl
                }
            });
        }
    }
}