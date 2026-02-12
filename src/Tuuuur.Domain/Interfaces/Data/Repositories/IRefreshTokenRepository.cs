using Tuuuur.Domain.Bo;
using Tuuuur.Domain.Interfaces.Data.Entities;

namespace Tuuuur.Domain.Interfaces.Data.Repositories;

public interface IRefreshTokenRepository : IGenericRepository
{
    Task<RefreshToken> GetRefreshTokenByTokenAsync(string p_Token, CancellationToken p_CancellationToken = default);
    Task<IMappingAddEntity<RefreshToken, IEntity>> CreateRefreshTokenAsync(RefreshToken p_RefreshToken, CancellationToken p_CancellationToken = default);
    Task DeleteRefreshTokenAsync(string p_Token, CancellationToken p_CancellationToken = default);
}
