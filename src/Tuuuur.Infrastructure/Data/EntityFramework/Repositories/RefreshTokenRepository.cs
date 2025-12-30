using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Tuuuur.Domain.Bo;
using Tuuuur.Domain.Interfaces.Data.Repositories;
using Tuuuur.Infrastructure.Data.EntityFramework.Entities;

namespace Tuuuur.Infrastructure.Data.EntityFramework.Repositories;

internal class RefreshTokenRepository(DbContext p_DbContext, IMapper p_Mapper, ILogger<RefreshTokenRepository> p_Logger)
    : GenericRepository<RefreshTokenRtk>(p_DbContext, p_Mapper, p_Logger), IRefreshTokenRepository
{
    public async Task<RefreshToken> GetRefreshTokenByTokenAsync(string p_Token, CancellationToken p_CancellationToken = default)
    {
        RefreshTokenRtk v_Entity = await FindBy(p_Rt => p_Rt.Token == p_Token)
            .Include(p_Rt => p_Rt.User)
            .FirstOrDefaultAsync(p_CancellationToken);
        return Mapper.Map<RefreshToken>(v_Entity);
    }

    public async Task<RefreshToken> CreateRefreshTokenAsync(RefreshToken p_RefreshToken, CancellationToken p_CancellationToken = default)
    {
        RefreshTokenRtk v_Entity = Mapper.Map<RefreshTokenRtk>(p_RefreshToken);
        await AddAsync(v_Entity, p_CancellationToken);
        return Mapper.Map<RefreshToken>(v_Entity);
    }

    public async Task<RefreshToken> UpdateRefreshTokenAsync(RefreshToken p_RefreshToken, CancellationToken p_CancellationToken = default)
    {
        RefreshTokenRtk v_Entity = Mapper.Map<RefreshTokenRtk>(p_RefreshToken);
        await UpdateAsync(v_Entity);
        return Mapper.Map<RefreshToken>(v_Entity);
    }

    public async Task<IEnumerable<RefreshToken>> GetActiveRefreshTokensByUserIdAsync(int p_UserId, CancellationToken p_CancellationToken = default)
    {
        List<RefreshTokenRtk> v_Entities = await FindBy(p_Rt =>
            p_Rt.UserId == p_UserId &&
            !p_Rt.IsRevoked &&
            p_Rt.ExpiresAt > DateTime.UtcNow)
            .ToListAsync(p_CancellationToken);
        return Mapper.Map<IEnumerable<RefreshToken>>(v_Entities);
    }

    public async Task RevokeAllUserRefreshTokensAsync(int p_UserId, CancellationToken p_CancellationToken = default)
    {
        List<RefreshTokenRtk> v_Entities = await FindBy(p_Rt =>
            p_Rt.UserId == p_UserId &&
            !p_Rt.IsRevoked)
            .ToListAsync(p_CancellationToken);

        foreach (RefreshTokenRtk v_Entity in v_Entities)
        {
            v_Entity.IsRevoked = true;
            v_Entity.RevokedAt = DateTime.UtcNow;
            await UpdateAsync(v_Entity);
        }
    }
}
