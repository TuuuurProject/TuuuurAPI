using Tuuuur.Domain.Bo;

namespace Tuuuur.Domain.Interfaces.Data.Repositories;

public interface IRefreshTokenRepository : IGenericRepository
{
    Task<RefreshToken> GetRefreshTokenByTokenAsync(string p_Token, CancellationToken p_CancellationToken = default);
    Task<RefreshToken> CreateRefreshTokenAsync(RefreshToken p_RefreshToken, CancellationToken p_CancellationToken = default);
    Task<RefreshToken> UpdateRefreshTokenAsync(RefreshToken p_RefreshToken, CancellationToken p_CancellationToken = default);
    Task<IEnumerable<RefreshToken>> GetActiveRefreshTokensByUserIdAsync(int p_UserId, CancellationToken p_CancellationToken = default);
    Task RevokeAllUserRefreshTokensAsync(int p_UserId, CancellationToken p_CancellationToken = default);
}
