using System;

namespace API.Entities;

public class RefreshToken
{
    public string Id { get; set; } = null!;
    public string UserId { get; set; } = null!;
    public string TokenHash { get; set; } = null!;
    public string FamilyId { get; set; } = null!;
    public string Jti { get; set; } = null!;
    public DateTime Expires { get; set; }
    public DateTime Created { get; set; } = DateTime.UtcNow;
    public DateTime? RevokedAt { get; set; }
    public bool Revoked { get; set; } = false;
    public string? ReplacedByTokenId { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }

    public AppUser User { get; set; } = null!;
}
