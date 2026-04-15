using Tuuuur.Domain.Bo;
using Tuuuur.Domain.Interfaces.Data.Entities;

namespace Tuuuur.Domain.Interfaces.Data.Repositories;

public interface IUserRepository : IGenericRepository
{
    Task<User> GetUserByEmailAsync(string p_Email, CancellationToken p_CancellationToken = default);
    Task<User> GetUserByEmailOrNickNameAsync(string p_Login, CancellationToken p_CancellationToken = default);
    Task<User> GetUserByNickNameAsync(string p_NickaName, CancellationToken p_CancellationToken = default);
    Task<IMappingAddEntity<User, IEntity>> CreateUserAsync(User p_User, CancellationToken p_CancellationToken = default);
    Task UpdateUserAsync(User p_User, CancellationToken p_CancellationToken = default);
    Task<User> GetUserByIdAsync(Guid p_Id, CancellationToken p_CancellationToken = default);
    Task<List<User>> GetUsersByIdsAsync(List<Guid> p_Ids, CancellationToken p_CancellationToken = default);
    Task DeleteUserNotRegisteredAsync(CancellationToken p_CancellationToken = default);
}