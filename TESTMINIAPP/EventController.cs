using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using StudentGroup.Controllers;
using StudentGroup.Data;
using StudentGroup.DTOs.EventDtos;
using StudentGroup.DTOs.OrganizerDtos;
using StudentGroup.DTOs.TicketDtos;
using StudentGroup.Entities;
using StudentGroup.Models;

namespace TESTMINIAPP;

public class EventControllerTests
{
    private readonly Mock<IMapper> _mapperMock;

    public EventControllerTests()
    {
        _mapperMock = new Mock<IMapper>();
    }

    private EventManagementDb GetDbContext()
    {
        var options = new DbContextOptionsBuilder<EventManagementDb>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new EventManagementDb(options);
    }

    [Fact]
    public async Task GetAllEvents_ShouldReturnOk()
    {
        var context = GetDbContext();

        context.Events.Add(new Event { Id = 1, Title = "Concert", Location = "Stadium" });
        context.Events.Add(new Event { Id = 2, Title = "Conference", Location = "Convention Center" });
        await context.SaveChangesAsync();

        var eventDtos = new List<EventGetdto>
        {
            new EventGetdto(),
            new EventGetdto()
        };

        _mapperMock
            .Setup(m => m.Map<List<EventGetdto>>(It.IsAny<List<Event>>()))
            .Returns(eventDtos);

        var controller = new EventController(context, _mapperMock.Object);

        var result = await controller.GetAllEvents();

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(eventDtos, okResult.Value);
    }

    [Fact]
    public async Task GetById_ShouldReturnNotFound_WhenEventDoesNotExist()
    {
        var context = GetDbContext();

        var controller = new EventController(context, _mapperMock.Object);

        var result = await controller.GetById(99);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task GetById_ShouldReturnOk_WhenEventExists()
    {
        var context = GetDbContext();

        var eventEntity = new Event
        {
            Id = 1,
            Title = "Concert",
            Location = "Stadium"
        };

        context.Events.Add(eventEntity);
        await context.SaveChangesAsync();

        var eventDto = new EventGetdto();

        _mapperMock
            .Setup(m => m.Map<EventGetdto>(It.IsAny<Event>()))
            .Returns(eventDto);

        var controller = new EventController(context, _mapperMock.Object);

        var result = await controller.GetById(1);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(eventDto, okResult.Value);
    }

    [Fact]
    public async Task CreateEvent_ShouldReturnBadRequest_WhenOrganizerDoesNotExist()
    {
        var context = GetDbContext();

        var dto = new EventCreateDto
        {
            OrganizerId = 99
        };

        var controller = new EventController(context, _mapperMock.Object);

        var result = await controller.CreateEvent(dto);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Organizer with ID 99 does not exist.", badRequest.Value);
    }

    [Fact]
    public async Task UpdateEvent_ShouldReturnNotFound_WhenEventDoesNotExist()
    {
        var context = GetDbContext();

        var dto = new EventUpdatedto();

        var controller = new EventController(context, _mapperMock.Object);

        var result = await controller.UpdateEvent(99, dto);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task DeleteEvent_ShouldReturnNotFound_WhenEventDoesNotExist()
    {
        var context = GetDbContext();

        var controller = new EventController(context, _mapperMock.Object);

        var result = await controller.DeleteEvent(99);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task DeleteEvent_ShouldReturnNoContent_WhenEventExists()
    {
        var context = GetDbContext();

        var eventEntity = new Event
        {
            Id = 1,
            Title = "Concert",
                Location = "Stadium"
        };

        context.Events.Add(eventEntity);
        await context.SaveChangesAsync();

        var controller = new EventController(context, _mapperMock.Object);

        var result = await controller.DeleteEvent(1);

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task GetTicketsForEvent_ShouldReturnNotFound_WhenEventDoesNotExist()
    {
        var context = GetDbContext();

        var controller = new EventController(context, _mapperMock.Object);

        var result = await controller.GetTicketsForEvent(99);

        var notFound = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal("Event not found.", notFound.Value);
    }

    [Fact]
    public async Task GetTicketsForEvent_ShouldReturnOk_WhenEventExists()
    {
        var context = GetDbContext();

        context.Events.Add(new Event { Id = 1, Title = "Concert", Location = "Stadium" });

        context.Tickets.Add(new Ticket
        {
            Id = 1,
            Type = "VIP",
            EventId = 1
        });

        await context.SaveChangesAsync();

        var ticketDtos = new List<TicketGetDto>
        {
            new TicketGetDto()
        };

        _mapperMock
            .Setup(m => m.Map<List<TicketGetDto>>(It.IsAny<List<Ticket>>()))
            .Returns(ticketDtos);

        var controller = new EventController(context, _mapperMock.Object);

        var result = await controller.GetTicketsForEvent(1);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(ticketDtos, okResult.Value);
    }

    [Fact]
    public async Task CreateTicketForEvent_ShouldReturnNotFound_WhenEventDoesNotExist()
    {
        var context = GetDbContext();

        var dto = new TicketCreate();

        var controller = new EventController(context, _mapperMock.Object);

        var result = await controller.CreateTicketForEvent(99, dto);

        var notFound = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal("Event not found.", notFound.Value);
    }

    [Fact]
    public async Task CreateTicketForEvent_ShouldReturnOk_WhenEventExists()
    {
        var context = GetDbContext();

        context.Events.Add(new Event
        {
            Id = 1,
            Title = "Concert",
            Location = "Stadium"
        });

        await context.SaveChangesAsync();

        var dto = new TicketCreate
        {
            EventId = 1
        };

        var ticket = new Ticket
        {
            Id = 1,
            Type = "VIP",
            EventId = 1
        };

        var ticketDto = new TicketGetDto();

        _mapperMock
            .Setup(m => m.Map<Ticket>(dto))
            .Returns(ticket);

        _mapperMock
            .Setup(m => m.Map<TicketGetDto>(It.IsAny<Ticket>()))
            .Returns(ticketDto);

        var controller = new EventController(context, _mapperMock.Object);

        var result = await controller.CreateTicketForEvent(1, dto);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(ticketDto, okResult.Value);
    }

    [Fact]
    public async Task GetOrganizerForEvent_ShouldReturnNotFound_WhenEventDoesNotExist()
    {
        var context = GetDbContext();

        var controller = new EventController(context, _mapperMock.Object);

        var result = await controller.GetOrganizerForEvent(99);

        var notFound = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal("Event not found.", notFound.Value);
    }

    [Fact]
    public async Task UploadBanner_ShouldReturnNotFound_WhenEventDoesNotExist()
    {
        var context = GetDbContext();

        var controller = new EventController(context, _mapperMock.Object);

        var result = await controller.UploadBanner(99, null);

        var notFound = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal("Event not found.", notFound.Value);
    }

    [Fact]
    public async Task UploadBanner_ShouldReturnBadRequest_WhenFileIsNull()
    {
        var context = GetDbContext();

        context.Events.Add(new Event
        {
            Id = 1,
            Title = "Concert",
            Location = "Stadium"
        });

        await context.SaveChangesAsync();

        var controller = new EventController(context, _mapperMock.Object);

        var result = await controller.UploadBanner(1, null);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("No file uploaded.", badRequest.Value);
    }
}