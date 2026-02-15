using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Tuuuur.Domain.Bo;
using Tuuuur.Domain.Interfaces.Data.Entities;
using Tuuuur.Domain.Interfaces.Data.Repositories;
using Tuuuur.Infrastructure.Data.EntityFramework.Entities;
using Tuuuur.Infrastructure.Data.Mapping;

namespace Tuuuur.Infrastructure.Data.EntityFramework.Repositories;

internal class UserAuthRepository(DbContext p_DbContext, IMapper p_Mapper, ILogger<UserAuthRepository> p_Logger)
    : GenericRepository<UserAuthUat>(p_DbContext, p_Mapper, p_Logger), IUserAuthRepository
{
    public async Task<IMappingAddEntity<UserAuth, IEntity>> 
        AddAuthCodeAsync(UserAuth p_UserAuth, CancellationToken p_CancellationToken = default)
    {
        IMappingAddEntity<UserAuth, UserAuthUat> v_Mapping =
            new MappingAddEntity<UserAuth, UserAuthUat>(Mapper, p_UserAuth);

        await AddAsync(v_Mapping.DtoEntity, p_CancellationToken);
        return v_Mapping;
    }

    public async Task<UserAuth> GetUserAuthByUserIdAndCodeAsync(int p_UserId, string p_Code, CancellationToken p_CancellationToken = default)
    {
        await DeleteExpiredUserAuthsAsync(p_CancellationToken);
        UserAuthUat v_Entity = await FindBy(p_P => p_P.UserId == p_UserId && p_P.Code == p_Code).FirstOrDefaultAsync(p_CancellationToken);
        return Mapper.Map<UserAuth>(v_Entity);
    }
    
    public async Task DeleteUserAuthAsync(int p_UserAuthId, CancellationToken p_CancellationToken = default)
    {
        await DeleteAsync(p_UserAuthId, p_CancellationToken);
    }

    public async Task<long> CountOfUserAuthsByUserIdAsync(int p_UserId,
        CancellationToken p_CancellationToken = default)
    {
        await DeleteExpiredUserAuthsAsync(p_CancellationToken);
        long v_Data = await CountAsync(p_P => p_P.UserId == p_UserId, p_CancellationToken);
        return v_Data;
    }
    
    private async Task DeleteExpiredUserAuthsAsync(CancellationToken p_CancellationToken = default)
    {
        List<UserAuthUat> v_ExpiredEntities = await FindBy(p_Uat => p_Uat.ExpiresAt <= DateTime.UtcNow).ToListAsync(p_CancellationToken);

        if (v_ExpiredEntities.Count != 0)
        {
            await DeleteAsync(v_ExpiredEntities);

            _ = await DbContext.SaveChangesAsync(p_CancellationToken);
        }
    }
    
}