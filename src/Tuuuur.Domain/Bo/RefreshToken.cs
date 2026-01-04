namespace Tuuuur.Domain.Bo;

/// <summary>
/// Refresh token
/// </summary>
public class RefreshToken
{
    public int UserId { get; init; }
    public string Token { get; init; }
    public DateTime ExpiresAt { get; init; }
    public DateTime CreatedAt { get; init; }
    public User User { get; init; }
    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    public bool IsActive => !IsExpired;
}