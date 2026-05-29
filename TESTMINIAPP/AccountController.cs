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
    private readonly Mock<EmailService> _emailServiceMock;

    public AccountControllerTests()
    {
        _userManagerMock = MockUserManager();
        _roleManagerMock = MockRoleManager();
        _validatorMock = new Mock<IValidator<RegisterDto>>();
        _mapperMock = new Mock<IMapper>();
        _configMock = new Mock<IConfiguration>();
        _jwtServiceMock = new Mock<JwtService>();
        _emailServiceMock = new Mock<EmailService>(_configMock.Object);
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
    public async Task Login_ShouldReturnBadRequest_WhenEmailIsNotConfirmed()
    {
        var dto = new LoginDto { Username = "testuser", Password = "Password123!" };
        var user = new AppUser { UserName = dto.Username, Email = "test@gmail.com" };

        _userManagerMock.Setup(u => u.FindByNameAsync(dto.Username)).ReturnsAsync(user);
        _userManagerMock.Setup(u => u.CheckPasswordAsync(user, dto.Password)).ReturnsAsync(true);
        _userManagerMock.Setup(u => u.IsEmailConfirmedAsync(user)).ReturnsAsync(false);

        var controller = CreateController();
        var result = await controller.Login(dto);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Login_ShouldReturnOk_AndRequire2FA_When2FAIsEnabled()
    {
        var dto = new LoginDto { Username = "testuser", Password = "Password123!" };
        var user = new AppUser { Id = Guid.NewGuid().ToString(), UserName = dto.Username, Email = "test@gmail.com", FullName = "Test User" };

        _userManagerMock.Setup(u => u.FindByNameAsync(dto.Username)).ReturnsAsync(user);
        _userManagerMock.Setup(u => u.CheckPasswordAsync(user, dto.Password)).ReturnsAsync(true);
        _userManagerMock.Setup(u => u.IsEmailConfirmedAsync(user)).ReturnsAsync(true);
        _userManagerMock.Setup(u => u.GetTwoFactorEnabledAsync(user)).ReturnsAsync(true);
        _userManagerMock.Setup(u => u.GenerateTwoFactorTokenAsync(user, TokenOptions.DefaultEmailProvider)).ReturnsAsync("123456");

        var controller = CreateController();
        var result = await controller.Login(dto);

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task VerifyTwoFactor_ShouldReturnNotFound_WhenUserDoesNotExist()
    {
        var dto = new TwoFactorLoginDto { Email = "test@gmail.com", Code = "123456" };
        _userManagerMock.Setup(u => u.FindByEmailAsync(dto.Email)).ReturnsAsync((AppUser?)null);

        var controller = CreateController();
        var result = await controller.VerifyTwoFactor(dto);

        var notFound = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal("User not found.", notFound.Value);
    }

    [Fact]
    public async Task VerifyTwoFactor_ShouldReturnBadRequest_WhenCodeIsInvalid()
    {
        var dto = new TwoFactorLoginDto { Email = "test@gmail.com", Code = "123456" };
        var user = new AppUser { Id = Guid.NewGuid().ToString(), Email = dto.Email, UserName = "testuser", FullName = "Test User" };

        _userManagerMock.Setup(u => u.FindByEmailAsync(dto.Email)).ReturnsAsync(user);
        _userManagerMock.Setup(u => u.VerifyTwoFactorTokenAsync(user, TokenOptions.DefaultEmailProvider, dto.Code)).ReturnsAsync(false);

        var controller = CreateController();
        var result = await controller.VerifyTwoFactor(dto);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Invalid two-factor code.", badRequest.Value);
    }

    [Fact]
    public async Task VerifyTwoFactor_ShouldReturnOk_WhenCodeIsValid()
    {
        var dto = new TwoFactorLoginDto { Email = "test@gmail.com", Code = "123456" };
        var user = new AppUser { Id = Guid.NewGuid().ToString(), Email = dto.Email, UserName = "testuser", FullName = "Test User" };

        _userManagerMock.Setup(u => u.FindByEmailAsync(dto.Email)).ReturnsAsync(user);
        _userManagerMock.Setup(u => u.VerifyTwoFactorTokenAsync(user, TokenOptions.DefaultEmailProvider, dto.Code)).ReturnsAsync(true);
        _userManagerMock.Setup(u => u.GetRolesAsync(user)).ReturnsAsync(new List<string> { "Member" });
        _configMock.Setup(c => c["Jwt:Key"]).Returns(new string('a', 32)); // 256 bits needed for HMAC
        _configMock.Setup(c => c["Jwt:Issuer"]).Returns("issuer");
        _configMock.Setup(c => c["Jwt:Audience"]).Returns("audience");

        var controller = CreateController();
        var result = await controller.VerifyTwoFactor(dto);

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task EnableTwoFactor_ShouldReturnNotFound_WhenUserDoesNotExist()
    {
        var dto = new ForgotPasswordDto { Email = "notfound@gmail.com" };
        _userManagerMock.Setup(u => u.FindByEmailAsync(dto.Email)).ReturnsAsync((AppUser?)null);

        var controller = CreateController();
        var result = await controller.EnableTwoFactor(dto);

        var notFound = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal("User not found.", notFound.Value);
    }

    [Fact]
    public async Task EnableTwoFactor_ShouldReturnOk_WhenUserExists()
    {
        var dto = new ForgotPasswordDto { Email = "test@gmail.com" };
        var user = new AppUser { Email = dto.Email };

        _userManagerMock.Setup(u => u.FindByEmailAsync(dto.Email)).ReturnsAsync(user);
        _userManagerMock.Setup(u => u.SetTwoFactorEnabledAsync(user, true)).ReturnsAsync(IdentityResult.Success);

        var controller = CreateController();
        var result = await controller.EnableTwoFactor(dto);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal("Two-factor authentication enabled.", okResult.Value);
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
            _jwtServiceMock.Object,
            _emailServiceMock.Object
        );
    }

    private static Mock<UserManager<AppUser>> MockUserManager()
    {
        var store = new Mock<IUserStore<AppUser>>();

        return new Mock<UserManager<AppUser>>(
            store.Object,
            null!,
            null!,
            null!,
            null!,
            null!,
            null!,
            null!,
            null!
        );
    }

    private static Mock<RoleManager<IdentityRole>> MockRoleManager()
    {
        var store = new Mock<IRoleStore<IdentityRole>>();

        return new Mock<RoleManager<IdentityRole>>(
            store.Object,
            null!,
            null!,
            null!,
            null!
        );
    }
}