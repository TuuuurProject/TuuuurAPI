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
        await DeleteExpitedRefreshTokenAsync(p_CancellationToken);
        RefreshTokenRtk v_Entity = await FindBy(p_Rt => p_Rt.Token == p_Token)
            .Include(p_Rt => p_Rt.User)
            .FirstOrDefaultAsync(p_CancellationToken);
        return Mapper.Map<RefreshToken>(v_Entity);
    }

    public async Task<RefreshToken> CreateRefreshTokenAsync(RefreshToken p_RefreshToken, CancellationToken p_CancellationToken = default)
    {
        await DeleteExpitedRefreshTokenAsync(p_CancellationToken);
        RefreshTokenRtk v_Entity = Mapper.Map<RefreshTokenRtk>(p_RefreshToken);
        await AddAsync(v_Entity, p_CancellationToken);
        return Mapper.Map<RefreshToken>(v_Entity);
    }

    public async Task DeleteRefreshTokenAsync(string p_Token, CancellationToken p_CancellationToken = default)
    {
        await DeleteExpitedRefreshTokenAsync(p_CancellationToken);
        RefreshTokenRtk v_RefreshTokens = await FindBy(p_P => p_P.Token == p_Token).FirstOrDefaultAsync(p_CancellationToken);
        if(v_RefreshTokens != null)
            await DeleteAsync(v_RefreshTokens, p_CancellationToken);
    }

    private async Task DeleteExpitedRefreshTokenAsync(CancellationToken p_CancellationToken = default)
    {
        List<RefreshTokenRtk> v_RefreshTokenRtks = await FindBy(p_P => p_P.ExpiresAt <= DateTime.UtcNow).ToListAsync(p_CancellationToken);
        
        if (v_RefreshTokenRtks.Count != 0)
        {
            await DeleteAsync(v_RefreshTokenRtks);

            _ = await DbContext.SaveChangesAsync(p_CancellationToken);
        }
    }
}
