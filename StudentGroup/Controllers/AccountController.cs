using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.IdentityModel.Tokens;
using StudentGroup.DTOs.UserDtos;
using StudentGroup.Models;
using StudentGroup.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Security.Cryptography;


namespace StudentGroup.Controllers;
[Route("api/account")]
[ApiController]

public class AccountController
(
    IValidator<RegisterDto> registerValidator,
    UserManager<AppUser> userManager,
    RoleManager<IdentityRole> roleManager,
    IMapper mapper,
    IConfiguration config,
    JwtService jwtService,
    EmailService emailService
    ) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
    {
        var validationResult = registerValidator.Validate(registerDto);
        if (!validationResult.IsValid)

            return BadRequest(validationResult.Errors);
        var user = await userManager.FindByEmailAsync(registerDto.Email);
        if (user != null)
            return BadRequest("Email is already in use");
        user = mapper.Map<AppUser>(registerDto);

        var result = await userManager.CreateAsync(user, registerDto.Password);
        if (!result.Succeeded)
            return BadRequest(result.Errors);

        await userManager.AddToRoleAsync(user, "Member");
        return Ok("User registered successfully");
    }

    [HttpGet]
    public async Task<IActionResult> CreateRole()
    {
       await roleManager.CreateAsync(new IdentityRole { Name = "Admin" });
         await roleManager.CreateAsync(new IdentityRole { Name = "Member" });
    
          return Ok("Roles created");
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
    {
        var user = await userManager.FindByNameAsync(loginDto.Username);

        if (user == null)
            return BadRequest("Invalid username or password");

        var result = await userManager.CheckPasswordAsync(user, loginDto.Password);

        if (!result)
            return BadRequest("Invalid username or password");

        if (!await userManager.IsEmailConfirmedAsync(user))
        {
            return BadRequest(new
            {
                message = "Email is not confirmed. Please confirm your email first."
            });
        }

        if (await userManager.GetTwoFactorEnabledAsync(user))
        {
            var code = await userManager.GenerateTwoFactorTokenAsync(user, TokenOptions.DefaultEmailProvider);

            var message = $@"
        <h2>Two-Factor Authentication</h2>
        <p>Your login code is:</p>
        <h3>{code}</h3>
    ";

            await emailService.SendEmailAsync(
                user.Email!,
                "Your 2FA Login Code",
                message
            );

            return Ok(new
            {
                message = "Two-factor code sent to your email.",
                requiresTwoFactor = true,
                email = user.Email
            });
        }

        var roles = await userManager.GetRolesAsync(user);

        var refreshToken = GenerateRefreshToken();

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);

        await userManager.UpdateAsync(user);

        return Ok(new
        {
            token = jwtService.GenerateToken(user, roles, config),
            refreshToken = refreshToken
        });

    }

    [HttpPost("verify-2fa")]
    public async Task<IActionResult> VerifyTwoFactor([FromBody] TwoFactorLoginDto dto)
    {
        var user = await userManager.FindByEmailAsync(dto.Email);

        if (user == null)
            return NotFound("User not found.");

        var isValid = await userManager.VerifyTwoFactorTokenAsync(
            user,
            TokenOptions.DefaultEmailProvider,
            dto.Code
        );

        if (!isValid)
            return BadRequest("Invalid two-factor code.");

        var roles = await userManager.GetRolesAsync(user);

        var refreshToken = GenerateRefreshToken();

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);

        await userManager.UpdateAsync(user);

        return Ok(new
        {
            token = jwtService.GenerateToken(user, roles, config),
            refreshToken = refreshToken
        });
    }
    [HttpGet("profile")]
    [Authorize]
    public IActionResult Profile()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var username = User.Identity?.Name;
        var fullName = User.FindFirstValue("FullName");
        var roles = User.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).ToList();
        return Ok(new
        {
            UserId = userId,
            Username = username,
            Roles = roles
        });
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
    {
        var user = await userManager.FindByEmailAsync(dto.Email);

        if (user == null)
            return NotFound("User not found.");

        var token = await userManager.GeneratePasswordResetTokenAsync(user);


        var encodedToken = WebEncoders.Base64UrlEncode(
            Encoding.UTF8.GetBytes(token)
        );

        var resetMessage = $@"
        <h2>Password Reset</h2>
        <p>Use this token to reset your password:</p>
        <p><b>{encodedToken}</b></p>
    ";

        await emailService.SendEmailAsync(
            dto.Email,
            "Reset your password",
            resetMessage
        );

        return Ok("Password reset token has been sent to your email.");
    }
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
    {
        var user = await userManager.FindByEmailAsync(dto.Email);

        if (user == null)
            return NotFound("User not found.");

        string decodedToken;
        try 
        {
            decodedToken = Encoding.UTF8.GetString(Microsoft.AspNetCore.WebUtilities.WebEncoders.Base64UrlDecode(dto.Token));
        }
        catch (FormatException) 
        {
            decodedToken = dto.Token; 
        }

        var result = await userManager.ResetPasswordAsync(user, decodedToken, dto.NewPassword);

        if (!result.Succeeded)
            return BadRequest(result.Errors);

        return Ok("Password has been reset successfully.");
    }


    [HttpPost("send-email-confirmation")]
    public async Task<IActionResult> SendEmailConfirmation([FromBody] ForgotPasswordDto dto)
    {
        var user = await userManager.FindByEmailAsync(dto.Email);

        if (user == null)
            return NotFound("User not found.");

        var token = await userManager.GenerateEmailConfirmationTokenAsync(user);

        var encodedToken = WebEncoders.Base64UrlEncode(
            Encoding.UTF8.GetBytes(token)
        );

        var confirmationMessage = $@"
        <h2>Email Confirmation</h2>
        <p>Use this token to confirm your email:</p>
        <p><b>{encodedToken}</b></p>
    ";

        await emailService.SendEmailAsync(
            dto.Email,
            "Confirm your email",
            confirmationMessage
        );

        return Ok("Email confirmation token has been sent to your email.");
    }


    [HttpPost("confirm-email")]
    public async Task<IActionResult> ConfirmEmail([FromBody] ForgotPasswordDto dto, [FromQuery] string token)
    {
        var user = await userManager.FindByEmailAsync(dto.Email);

        if (user == null)
            return NotFound("User not found.");

        string decodedToken;
        try 
        {
            decodedToken = Encoding.UTF8.GetString(
                WebEncoders.Base64UrlDecode(token)
            );
        }
        catch (FormatException) 
        {
            decodedToken = token; 
        }

        var result = await userManager.ConfirmEmailAsync(user, decodedToken);

        if (!result.Succeeded)
            return BadRequest(result.Errors);

        return Ok("Email confirmed successfully.");
    }


    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken(string refreshToken)
    {
        var user = userManager.Users.FirstOrDefault(u => u.RefreshToken == refreshToken);

        if (user == null || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            return Unauthorized("Invalid or expired refresh token.");

        var roles = await userManager.GetRolesAsync(user);

        var newAccessToken = jwtService.GenerateToken(user, roles, config);
        var newRefreshToken = GenerateRefreshToken();

        user.RefreshToken = newRefreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
            
        await userManager.UpdateAsync(user);

        return Ok(new
        {
            token = newAccessToken,
            refreshToken = newRefreshToken
        });

    }
    private static string GenerateRefreshToken()
    {
        var randomBytes = new byte[64];

        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);

        return Convert.ToBase64String(randomBytes);
    }
    [HttpPost("enable-2fa")]
  
    public async Task<IActionResult> EnableTwoFactor([FromBody] ForgotPasswordDto dto)
    {
        var user = await userManager.FindByEmailAsync(dto.Email);

        if (user == null)
            return NotFound("User not found.");

        await userManager.SetTwoFactorEnabledAsync(user, true);

        return Ok("Two-factor authentication enabled.");
    }
}

