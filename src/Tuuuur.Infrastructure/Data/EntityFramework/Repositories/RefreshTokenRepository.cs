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

    public async Task DeleteRefreshTokenForUserIdAsync(int p_UserId, CancellationToken p_CancellationToken = default)
    {
        IEnumerable<RefreshTokenRtk> v_RefreshTokens = await FindBy(p_P => p_P.UserId == p_UserId).ToListAsync(p_CancellationToken);
        await DeleteAsync(v_RefreshTokens, p_CancellationToken);
    }
}
