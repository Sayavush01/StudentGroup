using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using StudentGroup.DTOs.UserDtos;
using StudentGroup.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace StudentGroup.Configurations;
[Route("api/account")]
[ApiController]

public class AccountController
(
    IValidator<RegisterDto> registerValidator,
    UserManager<AppUser> userManager,
    RoleManager<IdentityRole> roleManager,
    IMapper mapper,
    IConfiguration config
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

        var roles = await userManager.GetRolesAsync(user);
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.UserName),
            new Claim("Fullname", user.FullName)
        };
        claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]));
        var result = await userManager.CheckPasswordAsync(user, loginDto.Password);
        if (!result)
            return BadRequest("Invalid username or password");
        var creds= new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
         var jwtSecurityToken = new System.IdentityModel.Tokens.Jwt.JwtSecurityToken(
            issuer: config["Jwt:Issuer"],
            audience: config["Jwt:Audience"],   
             claims: claims,
            expires: DateTime.Now.AddDays(7),
            signingCredentials: creds
        );
        var token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
        return Ok(new
        {
            token
        });
    }

    
}

