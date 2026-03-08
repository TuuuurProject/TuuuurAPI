
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.Logging;
using Tuuuur.Domain.Bo;
using Tuuuur.Domain.Interfaces.Data.Entities;
using Tuuuur.Domain.Interfaces.Data.Repositories;
using Tuuuur.Infrastructure.Data.EntityFramework.Entities;
using Tuuuur.Infrastructure.Data.Mapping;

namespace Tuuuur.Infrastructure.Data.EntityFramework.Repositories;

internal class UserRepository(DbContext p_DbContext, IMapper p_Mapper, ILogger<UserRepository> p_Logger)
    : GenericRepository<UserUsr>(p_DbContext, p_Mapper, p_Logger), IUserRepository
{
    private static readonly Func<IQueryable<UserUsr>, IIncludableQueryable<UserUsr, object>> m_SUserIncludes =
        p_Q => p_Q.Include(p_U => p_U.EloElo).ThenInclude(p_E => p_E.IdThemeNavigation);

    public async Task<User> GetUserByEmailAsync(string p_Email, CancellationToken p_CancellationToken = default)
    {
        return Mapper.Map<User>(await FindBy(p_U => p_U.Email == p_Email, p_Include: m_SUserIncludes).SingleOrDefaultAsync(p_CancellationToken));
    }

    public async Task<User> GetUserByEmailOrNickNameAsync(string p_Login, CancellationToken p_CancellationToken = default)
    {
        if (p_Login.Contains('@'))
        {
            return await GetUserByEmailAsync(p_Login, p_CancellationToken);
        }
        return Mapper.Map<User>(await FindBy(p_U => p_U.NickName == p_Login).SingleOrDefaultAsync(p_CancellationToken));
    }

    public async Task<User> GetUserByNickNameAsync(string p_NickaName, CancellationToken p_CancellationToken = default)
    {
        return Mapper.Map<User>(await FindBy(p_U => p_U.NickName == p_NickaName).SingleOrDefaultAsync(p_CancellationToken));
    }

    public async Task<IMappingAddEntity<User, IEntity>> CreateUserAsync(User p_User, CancellationToken p_CancellationToken = default)
    {
        IMappingAddEntity<User, UserUsr> v_Mapping =
            new MappingAddEntity<User, UserUsr>(Mapper, p_User);

        await AddAsync(v_Mapping.DtoEntity, p_CancellationToken);
        return v_Mapping;
    }

    public async Task UpdateUserAsync(User p_User, CancellationToken p_CancellationToken = default)
    {
        IMappingAddEntity<User, UserUsr> v_Mapping =
            new MappingAddEntity<User, UserUsr>(Mapper, p_User);

        await UpdateAsync(v_Mapping.DtoEntity);
    }

    public async Task DeleteUserAsync(Guid p_UserId, CancellationToken p_CancellationToken = default)
    {
        await DeleteAsync(p_UserId, p_CancellationToken);
    }

    public async Task<User> GetUserByIdAsync(Guid p_Id, CancellationToken p_CancellationToken = default)
    {
        UserUsr v_Entity = await FindBy(p_U => p_U.Id == p_Id, p_Include: m_SUserIncludes).SingleOrDefaultAsync(p_CancellationToken);

        return Mapper.Map<User>(v_Entity);
    }

    public async Task<List<User>> GetUsersByIdsAsync(List<Guid> p_Ids, CancellationToken p_CancellationToken = default)
    {
        List<UserUsr> v_Users = await FindBy(p_U => p_Ids.Contains(p_U.Id)).ToListAsync(p_CancellationToken);
        return Mapper.Map<List<User>>(v_Users);
    }

    /// <summary>
    /// Function to delete IsNew users if their password is not set in time
    /// </summary>
    /// <param name="p_CancellationToken"></param>
    public async Task DeleteUserNotRegisteredAsync(CancellationToken p_CancellationToken = default)
    {
        IEnumerable<UserUsr> v_Users = await FindBy(p_Usr => p_Usr.IsNew && p_Usr.UserAuthUat.Count == 0).ToListAsync(p_CancellationToken);
        foreach (UserUsr v_User in v_Users)
        {
            await DeleteUserAsync(v_User.Id, p_CancellationToken);
        }
    }
}