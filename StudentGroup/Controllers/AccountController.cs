using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using StudentGroup.DTOs.UserDtos;
using StudentGroup.Models;
using StudentGroup.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

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
    JwtService jwtService
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

        var roles = await userManager.GetRolesAsync(user);

        return Ok(new
        {
            token= jwtService.GenerateToken(user, roles, config)
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


    
}

