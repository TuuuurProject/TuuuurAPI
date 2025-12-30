using System.Text.Json.Serialization;

namespace Tuuuur.Domain.Bo;

public record RefreshToken : IBOEntity
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string Token { get; set; }

    public DateTime ExpiresAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? RevokedAt { get; set; }

    public bool IsRevoked { get; set; }

    [JsonIgnore]
    public virtual User User { get; set; }

    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;

    public bool IsActive => !IsRevoked && !IsExpired;
}
