namespace Tuuuur.API.Requests.Authentification;

/// <summary>
/// Request to refresh an access token using a refresh token
/// </summary>
public class RefreshTokenApiRequest
{
    /// <summary>
    /// The refresh token to use for generating a new access token
    /// </summary>
    public string RefreshToken { get; set; }
}
