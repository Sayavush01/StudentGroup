
using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StudentGroup.Data;
using StudentGroup.Models;
using System;

namespace StudentGroup
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddDbContext<EventManagementDb>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            builder.Services.AddAutoMapper(typeof(Program));

            builder.Services.AddIdentity<AppUser, IdentityRole>(opt=>
                {
                    opt.Password.RequireDigit = false;
                    opt.Password.RequiredLength = 6;
                    opt.Password.RequireNonAlphanumeric = false;
                    opt.Password.RequireUppercase = false;
                    opt.Password.RequireLowercase = false;
                }
                )
                .AddEntityFrameworkStores<EventManagementDb>()
                .AddDefaultTokenProviders();
            //using (var scope = builder.Services.BuildServiceProvider().CreateScope())
            //{
            //    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
            //    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            //    if (!roleManager.RoleExistsAsync("Admin").Result)
            //    {
            //        var role = new IdentityRole { Name = "Admin" };
            //        roleManager.CreateAsync(role).Wait();
            //    }
            //    if (!userManager.Users.Any(u => u.UserName == "admin"))
            //    {
            //        var user = new AppUser { UserName = "admin", Email = "admin@example.com" };
            //        userManager.CreateAsync(user, "Admin@123").Wait();
            //        userManager.AddToRoleAsync(user, "Admin").Wait();
            //    }
            //}

                builder.Services.AddValidatorsFromAssemblyContaining<Program>();


            builder.Services.AddControllers();

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            app.UseStaticFiles();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
