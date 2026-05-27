using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using StudentGroup.Controllers;
using StudentGroup.Data;
using StudentGroup.DTOs.EventDtos;
using StudentGroup.DTOs.OrganizerDtos;
using StudentGroup.Entities;

namespace TESTMINIAPP;

public class OrganizerControllerTests
{
    private readonly Mock<IMapper> _mapperMock;

    public OrganizerControllerTests()
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
    public async Task GetAllOrganizers_ShouldReturnOk()
    {
        var context = GetDbContext();

        context.Organizers.Add(new Organizer { Id = 1, Name = "Organizer 1" });
        context.Organizers.Add(new Organizer { Id = 2, Name = "Organizer 2" });
        await context.SaveChangesAsync();

        var organizerDtos = new List<OrganizerGetDto>
        {
            new OrganizerGetDto(),
            new OrganizerGetDto()
        };

        _mapperMock
            .Setup(m => m.Map<List<OrganizerGetDto>>(It.IsAny<List<Organizer>>()))
            .Returns(organizerDtos);

        var controller = new OrganizerController(context, _mapperMock.Object);

        var result = await controller.GetAllOrganizers();

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(organizerDtos, okResult.Value);
    }

    [Fact]
    public async Task GetById_ShouldReturnNotFound_WhenOrganizerDoesNotExist()
    {
        var context = GetDbContext();

        var controller = new OrganizerController(context, _mapperMock.Object);

        var result = await controller.GetById(99);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task GetById_ShouldReturnOk_WhenOrganizerExists()
    {
        var context = GetDbContext();

        var organizer = new Organizer
        {
            Id = 1,
            Name = "Organizer 1"
        };

        context.Organizers.Add(organizer);
        await context.SaveChangesAsync();

        var organizerDto = new OrganizerGetDto();

        _mapperMock
            .Setup(m => m.Map<OrganizerGetDto>(It.IsAny<Organizer>()))
            .Returns(organizerDto);

        var controller = new OrganizerController(context, _mapperMock.Object);

        var result = await controller.GetById(1);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(organizerDto, okResult.Value);
    }

    [Fact]
    public async Task CreateOrganizer_ShouldReturnCreatedAtAction()
    {
        var context = GetDbContext();

        var dto = new OrganizerCreate();

        var organizer = new Organizer
        {
            Id = 1,
            Name = "Organizer 1"
        };

        var organizerDto = new OrganizerGetDto();

        _mapperMock
            .Setup(m => m.Map<Organizer>(dto))
            .Returns(organizer);

        _mapperMock
            .Setup(m => m.Map<OrganizerGetDto>(It.IsAny<Organizer>()))
            .Returns(organizerDto);

        var controller = new OrganizerController(context, _mapperMock.Object);

        var result = await controller.CreateOrganizer(dto);

        var createdResult = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(nameof(OrganizerController.GetById), createdResult.ActionName);
        Assert.Equal(organizerDto, createdResult.Value);
    }

    [Fact]
    public async Task UpdateOrganizer_ShouldReturnNotFound_WhenOrganizerDoesNotExist()
    {
        var context = GetDbContext();

        var dto = new OrganizerUpdateDto();

        var controller = new OrganizerController(context, _mapperMock.Object);

        var result = await controller.UpdateOrganizer(99, dto);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task UpdateOrganizer_ShouldReturnNoContent_WhenOrganizerExists()
    {
        var context = GetDbContext();

        var organizer = new Organizer
        {
            Id = 1,
            Name = "Organizer 1"
        };

        context.Organizers.Add(organizer);
        await context.SaveChangesAsync();

        var dto = new OrganizerUpdateDto();

        var controller = new OrganizerController(context, _mapperMock.Object);

        var result = await controller.UpdateOrganizer(1, dto);

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task DeleteOrganizer_ShouldReturnNotFound_WhenOrganizerDoesNotExist()
    {
        var context = GetDbContext();

        var controller = new OrganizerController(context, _mapperMock.Object);

        var result = await controller.DeleteOrganizer(99);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task DeleteOrganizer_ShouldReturnNoContent_WhenOrganizerExists()
    {
        var context = GetDbContext();

        var organizer = new Organizer
        {
            Id = 1,
            Name = "Organizer 1"
        };

        context.Organizers.Add(organizer);
        await context.SaveChangesAsync();

        var controller = new OrganizerController(context, _mapperMock.Object);

        var result = await controller.DeleteOrganizer(1);

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task GetEventsByOrganizer_ShouldReturnNotFound_WhenOrganizerDoesNotExist()
    {
        var context = GetDbContext();

        var controller = new OrganizerController(context, _mapperMock.Object);

        var result = await controller.GetEventsByOrganizer(99);

        var notFound = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal("Organizer not found.", notFound.Value);
    }

    [Fact]
    public async Task GetEventsByOrganizer_ShouldReturnOk_WhenOrganizerExists()
    {
        var context = GetDbContext();

        context.Organizers.Add(new Organizer
        {
            Id = 1,
            Name = "Organizer 1"
        });

        context.Events.Add(new Event
        {
            Id = 1,
            Title = "Concert",
            OrganizerId = 1
        });

        await context.SaveChangesAsync();

        var eventDtos = new List<EventGetdto>
        {
            new EventGetdto()
        };

        _mapperMock
            .Setup(m => m.Map<List<EventGetdto>>(It.IsAny<List<Event>>()))
            .Returns(eventDtos);

        var controller = new OrganizerController(context, _mapperMock.Object);

        var result = await controller.GetEventsByOrganizer(1);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(eventDtos, okResult.Value);
    }

    [Fact]
    public async Task UploadLogo_ShouldReturnNotFound_WhenOrganizerDoesNotExist()
    {
        var context = GetDbContext();

        var controller = new OrganizerController(context, _mapperMock.Object);

        var result = await controller.UploadLogo(99, null);

        var notFound = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal("Organizer not found.", notFound.Value);
    }

    [Fact]
    public async Task UploadLogo_ShouldReturnBadRequest_WhenFileIsNull()
    {
        var context = GetDbContext();

        context.Organizers.Add(new Organizer
        {
            Id = 1,
            Name = "Organizer 1"
        });

        await context.SaveChangesAsync();

        var controller = new OrganizerController(context, _mapperMock.Object);

        var result = await controller.UploadLogo(1, null);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("No file uploaded.", badRequest.Value);
    }
}