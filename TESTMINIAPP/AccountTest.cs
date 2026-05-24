using AutoMapper;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;
using StudentGroup.Controllers;
using StudentGroup.DTOs.UserDtos;
using StudentGroup.Models;
using StudentGroup.Services;

namespace TESTMINIAPP;

public class AccountControllerTests
{
    private readonly Mock<UserManager<AppUser>> _userManagerMock;
    private readonly Mock<RoleManager<IdentityRole>> _roleManagerMock;
    private readonly Mock<IValidator<RegisterDto>> _validatorMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IConfiguration> _configMock;
    private readonly Mock<JwtService> _jwtServiceMock;

    public AccountControllerTests()
    {
        _userManagerMock = MockUserManager();
        _roleManagerMock = MockRoleManager();
        _validatorMock = new Mock<IValidator<RegisterDto>>();
        _mapperMock = new Mock<IMapper>();
        _configMock = new Mock<IConfiguration>();
        _jwtServiceMock = new Mock<JwtService>();
    }

    [Fact]
    public async Task Register_ShouldReturnBadRequest_WhenValidationFails()
    {
        var dto = new RegisterDto();

        _validatorMock
            .Setup(v => v.Validate(dto))
            .Returns(new ValidationResult(new[]
            {
                new ValidationFailure("Email", "Email is required")
            }));

        var controller = CreateController();

        var result = await controller.Register(dto);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Register_ShouldReturnBadRequest_WhenEmailAlreadyExists()
    {
        var dto = new RegisterDto
        {
            Email = "test@gmail.com",
            Password = "Password123!"
        };

        _validatorMock
            .Setup(v => v.Validate(dto))
            .Returns(new ValidationResult());

        _userManagerMock
            .Setup(u => u.FindByEmailAsync(dto.Email))
            .ReturnsAsync(new AppUser());

        var controller = CreateController();

        var result = await controller.Register(dto);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Email is already in use", badRequest.Value);
    }

    [Fact]
    public async Task Register_ShouldReturnOk_WhenUserCreatedSuccessfully()
    {
        var dto = new RegisterDto
        {
            Email = "test@gmail.com",
            Password = "Password123!"
        };

        var user = new AppUser
        {
            Email = dto.Email,
            UserName = dto.Email
        };

        _validatorMock
            .Setup(v => v.Validate(dto))
            .Returns(new ValidationResult());

        _userManagerMock
            .Setup(u => u.FindByEmailAsync(dto.Email))
            .ReturnsAsync((AppUser?)null);

        _mapperMock
            .Setup(m => m.Map<AppUser>(dto))
            .Returns(user);

        _userManagerMock
            .Setup(u => u.CreateAsync(user, dto.Password))
            .ReturnsAsync(IdentityResult.Success);

        _userManagerMock
            .Setup(u => u.AddToRoleAsync(user, "Member"))
            .ReturnsAsync(IdentityResult.Success);

        var controller = CreateController();

        var result = await controller.Register(dto);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal("User registered successfully", okResult.Value);
    }

    [Fact]
    public async Task Login_ShouldReturnBadRequest_WhenUserNotFound()
    {
        var dto = new LoginDto
        {
            Username = "wronguser",
            Password = "Password123!"
        };

        _userManagerMock
            .Setup(u => u.FindByNameAsync(dto.Username))
            .ReturnsAsync((AppUser?)null);

        var controller = CreateController();

        var result = await controller.Login(dto);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Invalid username or password", badRequest.Value);
    }

    [Fact]
    public async Task Login_ShouldReturnBadRequest_WhenPasswordIsWrong()
    {
        var dto = new LoginDto
        {
            Username = "testuser",
            Password = "wrongpassword"
        };

        var user = new AppUser
        {
            UserName = dto.Username,
            Email = "test@gmail.com"
        };

        _userManagerMock
            .Setup(u => u.FindByNameAsync(dto.Username))
            .ReturnsAsync(user);

        _userManagerMock
            .Setup(u => u.CheckPasswordAsync(user, dto.Password))
            .ReturnsAsync(false);

        var controller = CreateController();

        var result = await controller.Login(dto);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Invalid username or password", badRequest.Value);
    }

    [Fact]
    public async Task ForgotPassword_ShouldReturnNotFound_WhenUserDoesNotExist()
    {
        var dto = new ForgotPasswordDto
        {
            Email = "notfound@gmail.com"
        };

        _userManagerMock
            .Setup(u => u.FindByEmailAsync(dto.Email))
            .ReturnsAsync((AppUser?)null);

        var controller = CreateController();

        var result = await controller.ForgotPassword(dto);

        var notFound = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal("User not found.", notFound.Value);
    }

    [Fact]
    public async Task ForgotPassword_ShouldReturnOk_WhenUserExists()
    {
        var dto = new ForgotPasswordDto
        {
            Email = "test@gmail.com"
        };

        var user = new AppUser
        {
            Email = dto.Email
        };

        _userManagerMock
            .Setup(u => u.FindByEmailAsync(dto.Email))
            .ReturnsAsync(user);

        _userManagerMock
            .Setup(u => u.GeneratePasswordResetTokenAsync(user))
            .ReturnsAsync("reset-token");

        var controller = CreateController();

        var result = await controller.ForgotPassword(dto);

        Assert.IsType<OkObjectResult>(result);
    }

    private AccountController CreateController()
    {
        return new AccountController(
            _validatorMock.Object,
            _userManagerMock.Object,
            _roleManagerMock.Object,
            _mapperMock.Object,
            _configMock.Object,
            _jwtServiceMock.Object
        );
    }

    private static Mock<UserManager<AppUser>> MockUserManager()
    {
        var store = new Mock<IUserStore<AppUser>>();

        return new Mock<UserManager<AppUser>>(
            store.Object,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null
        );
    }

    private static Mock<RoleManager<IdentityRole>> MockRoleManager()
    {
        var store = new Mock<IRoleStore<IdentityRole>>();

        return new Mock<RoleManager<IdentityRole>>(
            store.Object,
            null,
            null,
            null,
            null
        );
    }
}