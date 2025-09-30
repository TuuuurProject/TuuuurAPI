
using AutoMapper;
using Microsoft.EntityFrameworkCore;
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
    public async Task<User> GetUserByEmailAsync(string p_Email, CancellationToken p_CancellationToken = default)
    {
        return Mapper.Map<User>(await FindBy(p_U => p_U.Email == p_Email).SingleOrDefaultAsync(p_CancellationToken));
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
}