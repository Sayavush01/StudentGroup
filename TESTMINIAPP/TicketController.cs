using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using StudentGroup.Controllers;
using StudentGroup.Data;
using StudentGroup.DTOs.TicketDtos;
using StudentGroup.Entities;

namespace TESTMINIAPP;

public class TicketControllerTests
{
    private readonly Mock<IMapper> _mapperMock;

    public TicketControllerTests()
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
    public async Task GetAllTickets_ShouldReturnOk()
    {
        var context = GetDbContext();

        context.Tickets.Add(new Ticket { Id = 1, Type = "VIP" });
        context.Tickets.Add(new Ticket { Id = 2, Type = "Standard" });
        await context.SaveChangesAsync();

        var ticketDtos = new List<TicketGetDto>
        {
            new TicketGetDto(),
            new TicketGetDto()
        };

        _mapperMock
            .Setup(m => m.Map<List<TicketGetDto>>(It.IsAny<List<Ticket>>()))
            .Returns(ticketDtos);

        var controller = new TicketController(context, _mapperMock.Object);

        var result = await controller.GetAllTickets();

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(ticketDtos, okResult.Value);
    }

    [Fact]
    public async Task GetById_ShouldReturnNotFound_WhenTicketDoesNotExist()
    {
        var context = GetDbContext();

        var controller = new TicketController(context, _mapperMock.Object);

        var result = await controller.GetById(99);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task GetById_ShouldReturnOk_WhenTicketExists()
    {
        var context = GetDbContext();

        var ticket = new Ticket
        {
            Id = 1,
            Type = "VIP"
        };

        context.Tickets.Add(ticket);
        await context.SaveChangesAsync();

        var ticketDto = new TicketGetDto();

        _mapperMock
            .Setup(m => m.Map<TicketGetDto>(It.IsAny<Ticket>()))
            .Returns(ticketDto);

        var controller = new TicketController(context, _mapperMock.Object);

        var result = await controller.GetById(1);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(ticketDto, okResult.Value);
    }

    [Fact]
    public async Task CreateTicket_ShouldReturnBadRequest_WhenEventDoesNotExist()
    {
        var context = GetDbContext();

        var dto = new TicketCreate
        {
            EventId = 99
        };

        var controller = new TicketController(context, _mapperMock.Object);

        var result = await controller.CreateTicket(dto);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Event does not exist.", badRequest.Value);
    }

    [Fact]
    public async Task UpdateTicket_ShouldReturnNotFound_WhenTicketDoesNotExist()
    {
        var context = GetDbContext();

        var dto = new TicketUpdateDto();

        var controller = new TicketController(context, _mapperMock.Object);

        var result = await controller.UpdateTicket(99, dto);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task DeleteTicket_ShouldReturnNotFound_WhenTicketDoesNotExist()
    {
        var context = GetDbContext();

        var controller = new TicketController(context, _mapperMock.Object);

        var result = await controller.DeleteTicket(99);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task DeleteTicket_ShouldReturnNoContent_WhenTicketExists()
    {
        var context = GetDbContext();

        var ticket = new Ticket
        {
            Id = 1,
            Type = "VIP"
        };

        context.Tickets.Add(ticket);
        await context.SaveChangesAsync();

        var controller = new TicketController(context, _mapperMock.Object);

        var result = await controller.DeleteTicket(1);

        Assert.IsType<NoContentResult>(result);
    }
}