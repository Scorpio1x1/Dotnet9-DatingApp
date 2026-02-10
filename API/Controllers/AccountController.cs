using System;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

public class AccountController(
    UserManager<AppUser> userManager,
    ITokenService tokenService,
    AppDbContext context) : BaseApiController
{
    private static readonly TimeSpan RefreshTokenLifetime = TimeSpan.FromDays(30);

    [HttpPost("register")] // api/account/register
    public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto)
    {
        var user = new AppUser
        {
            DisplayName = registerDto.DisplayName,
            Email = registerDto.Email,
            UserName = registerDto.Email,
            Member = new Member
            {
                DisplayName = registerDto.DisplayName,
                Gender = registerDto.Gender,
                City = registerDto.City,
                Country = registerDto.Country,
                DateOfBirth = registerDto.DateOfBirth
            }
        };

        var result = await userManager.CreateAsync(user, registerDto.Password);

        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("identity", error.Description);
            }

            return ValidationProblem();
        }

        await userManager.AddToRoleAsync(user, "Member");

        await IssueRefreshTokenAsync(user, familyId: null);

        return await user.ToDto(tokenService);
    }

    [HttpPost("login")]
    public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
    {
        var user = await userManager.FindByEmailAsync(loginDto.Email);

        if (user == null) return Unauthorized("Invalid email address");

        var result = await userManager.CheckPasswordAsync(user, loginDto.Password);

        if (!result) return Unauthorized("Invalid password");

        await IssueRefreshTokenAsync(user, familyId: null);

        return await user.ToDto(tokenService);
    }

    [HttpPost("refresh-token")]
    public async Task<ActionResult<UserDto>> RefreshToken()
    {
        var refreshToken = Request.Cookies["refreshToken"];
        if (refreshToken == null) return NoContent();

        var refreshTokenHash = tokenService.HashRefreshToken(refreshToken);
        var existingToken = await context.RefreshTokens
            .FirstOrDefaultAsync(x => x.TokenHash == refreshTokenHash);

        if (existingToken == null) return Unauthorized();

        if (existingToken.Revoked || existingToken.Expires <= DateTime.UtcNow)
        {
            await RevokeTokenFamilyAsync(existingToken.FamilyId);
            await context.SaveChangesAsync();
            Response.Cookies.Delete("refreshToken");
            return Unauthorized();
        }

        var user = await userManager.FindByIdAsync(existingToken.UserId);
        if (user == null) return Unauthorized();

        var (newToken, rawToken) = BuildRefreshToken(user, existingToken.FamilyId);
        context.RefreshTokens.Add(newToken);
        existingToken.Revoked = true;
        existingToken.RevokedAt = DateTime.UtcNow;
        existingToken.ReplacedByTokenId = newToken.Id;
        await context.SaveChangesAsync();

        SetRefreshTokenCookie(rawToken);

        return await user.ToDto(tokenService);
    }

    private async Task IssueRefreshTokenAsync(AppUser user, string? familyId)
    {
        var (newToken, rawToken) = BuildRefreshToken(user, familyId);
        context.RefreshTokens.Add(newToken);
        await context.SaveChangesAsync();
        SetRefreshTokenCookie(rawToken);
    }

    private (RefreshToken Token, string RawToken) BuildRefreshToken(AppUser user, string? familyId)
    {
        var rawToken = tokenService.GenerateRefreshToken();
        var refreshToken = new RefreshToken
        {
            Id = Guid.NewGuid().ToString("N"),
            UserId = user.Id,
            TokenHash = tokenService.HashRefreshToken(rawToken),
            FamilyId = familyId ?? Guid.NewGuid().ToString("N"),
            Jti = Guid.NewGuid().ToString("N"),
            Expires = DateTime.UtcNow.Add(RefreshTokenLifetime),
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
            UserAgent = Request.Headers.UserAgent.ToString()
        };

        return (refreshToken, rawToken);
    }

    private void SetRefreshTokenCookie(string rawToken)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTime.UtcNow.Add(RefreshTokenLifetime)
        };

        Response.Cookies.Append("refreshToken", rawToken, cookieOptions);
    }

    private async Task RevokeTokenFamilyAsync(string familyId)
    {
        var tokens = await context.RefreshTokens
            .Where(x => x.FamilyId == familyId && !x.Revoked)
            .ToListAsync();

        foreach (var token in tokens)
        {
            token.Revoked = true;
            token.RevokedAt = DateTime.UtcNow;
        }
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<ActionResult> Logout()
    {
        var refreshToken = Request.Cookies["refreshToken"];
        if (refreshToken != null)
        {
            var refreshTokenHash = tokenService.HashRefreshToken(refreshToken);
            var existingToken = await context.RefreshTokens
                .FirstOrDefaultAsync(x => x.TokenHash == refreshTokenHash);

            if (existingToken != null)
            {
                await RevokeTokenFamilyAsync(existingToken.FamilyId);
                await context.SaveChangesAsync();
            }
        }

        Response.Cookies.Delete("refreshToken");

        return Ok();
    }
}