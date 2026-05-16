using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using StudentGroup.DTOs.UserDtos;
using StudentGroup.Models;

namespace StudentGroup.Configurations;
[Route("api/account")]
[ApiController]

public class AccountController
(
    IValidator<RegisterDto> registerValidator,
    UserManager<AppUser> userManager,
    RoleManager<IdentityRole> roleManager,
    IMapper mapper
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
}

