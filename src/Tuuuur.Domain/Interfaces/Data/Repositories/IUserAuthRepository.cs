
using Tuuuur.Domain.Bo;
using Tuuuur.Domain.Interfaces.Data.Entities;

namespace Tuuuur.Domain.Interfaces.Data.Repositories;

public interface IUserAuthRepository : IGenericRepository
{
    Task<IMappingAddEntity<UserAuth, IEntity>> AddAuthCodeAsync(UserAuth p_UserAuth, CancellationToken p_CancellationToken = default);
    Task<UserAuth> GetUserAuthByUserIdAndCodeAsync(int p_UserId, string p_Code,
        CancellationToken p_CancellationToken = default);

    Task DeleteUserAuthAsync(int p_UserAuthId, CancellationToken p_CancellationToken = default);
}