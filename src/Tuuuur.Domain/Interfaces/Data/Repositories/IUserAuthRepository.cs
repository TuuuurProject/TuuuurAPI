
using Tuuuur.Domain.Bo;
using Tuuuur.Domain.Interfaces.Data.Entities;

namespace Tuuuur.Domain.Interfaces.Data.Repositories;

public interface IUserAuthRepository : IGenericRepository
{
    Task<IMappingAddEntity<UserAuth, IEntity>> GenerateAuthCodeAsync(UserAuth p_UserAuth, CancellationToken p_CancellationToken = default);
    Task<UserAuth> GetUserAuthByUserIdAndCode(int p_UserId, string p_Code,
        CancellationToken p_CancellationToken = default);

    Task DeleteUserAuth(int p_UserAuthId, CancellationToken p_CancellationToken = default);
}