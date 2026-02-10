using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using API.DTOs;
using API.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

public class Seed
{
    public static async Task SeedUsers(UserManager<AppUser> userManager, AppDbContext context)
    {
        // This is completely unsafe and not regular but its for demonstration purposes, removed near end.
        if (!await userManager.Users.AnyAsync())
        {
            var memberData = await File.ReadAllTextAsync("Data/UserSeedData.json");
            var members = JsonSerializer.Deserialize<List<SeedUserDto>>(memberData);

            if (members == null)
            {
                Console.WriteLine("No members in seed data");
                return;
            }

            foreach (var member in members)
            {
                var user = new AppUser
                {
                    Id = member.Id,
                    Email = member.Email,
                    UserName = member.Email,
                    DisplayName = member.DisplayName,
                    ImageUrl = member.ImageUrl,
                    Member = new Member
                    {
                        Id = member.Id,
                        DisplayName = member.DisplayName,
                        Description = member.Description,
                        DateOfBirth = member.DateOfBirth,
                        ImageUrl = member.ImageUrl,
                        Gender = member.Gender,
                        City = member.City,
                        Country = member.Country,
                        LastActive = member.LastActive,
                        Created = member.Created
                    }
                };

                user.Member.Photos.Add(new Photo
                {
                    ImageUrl = member.ImageUrl!,
                    MemberId = member.Id
                });

                var result = await userManager.CreateAsync(user, "Pa$$w0rd");
                if (!result.Succeeded)
                {
                    Console.WriteLine(result.Errors.First().Description);
                }
                await userManager.AddToRoleAsync(user, "Member");
            }
        }

        var admin = await userManager.FindByEmailAsync("admin@test.com");
        if (admin == null)
        {
            admin = new AppUser
            {
                UserName = "admin@test.com",
                Email = "admin@test.com",
                DisplayName = "Admin"
            };

            await userManager.CreateAsync(admin, "Pa$$w0rd");
            await userManager.AddToRolesAsync(admin, ["Admin", "Moderator"]);
        }

        var hasAdminMember = await context.Members.AnyAsync(x => x.Id == admin.Id);
        if (!hasAdminMember)
        {
            var adminMember = new Member
            {
                Id = admin.Id,
                DisplayName = admin.DisplayName ?? "Admin",
                Gender = "other",
                City = "n/a",
                Country = "n/a",
                DateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-30)),
                Created = DateTime.UtcNow,
                LastActive = DateTime.UtcNow
            };

            context.Members.Add(adminMember);
            await context.SaveChangesAsync();
        }
    }
}