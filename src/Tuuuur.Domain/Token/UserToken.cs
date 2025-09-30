using Tuuuur.Domain.Bo;

namespace Tuuuur.Domain.Token;

public record UserToken
{
    public User User { get; init; }

    public JwtTokenResponse Token { get; init; }
    public bool IsGoogleUser { get; init; }
}