using System.Text.Json.Serialization;

namespace API.Entities;

public class Photo
{
    public int Id { get; set; }
    public required string ImageUrl { get; set; }
    public string? PublicId { get; set; }

    // Navigation Property
    [JsonIgnore]
    public Member Member { get; set; } = null!;

    public string MemberId { get; set; } = null!;
}
