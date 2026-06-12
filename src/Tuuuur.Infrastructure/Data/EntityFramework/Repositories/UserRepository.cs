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
    public async Task<RankingPage> GetRankingPageAsync(Guid? p_UserId, int p_Page, int p_Size,
        CancellationToken p_CancellationToken = default)
    {
        int v_Skip = (p_Page - 1) * p_Size;

        long v_TotalCount = await CountAsync(p_CancellationToken: p_CancellationToken);

        // Get all users ordered by ELO to calculate ranking
        List<UserUsr> v_AllUsersList = await FindBy(null,
                p_Include: p_Includes => p_Includes
                    .Include(p_P => p_P.EloElo))
            .OrderByDescending(p_P => p_P.EloElo.Count != 0 ? p_P.EloElo.Sum(p_P => p_P.Value) / p_P.EloElo.Count : 0)
            .AsNoTracking()
            .AsSplitQuery()
            .ToListAsync(p_CancellationToken);

        List<UserUsr> v_UsersList = v_AllUsersList
            .Skip(v_Skip)
            .Take(p_Size)
            .ToList();

        IEnumerable<User> v_Users = Mapper.Map<IEnumerable<User>>(v_UsersList);

        int v_UserRanking = 0;
        int v_UserElo = 0;

        if (p_UserId.HasValue)
        {
            User v_User = await GetUserByIdAsync(p_UserId.Value, p_CancellationToken);
            if (v_User != null)
            {
                v_UserElo = v_User.GlobalElo;
                v_UserRanking = v_AllUsersList.FindIndex(p_UserUsr => p_UserUsr.Id == p_UserId.Value) + 1;
            }
        }

        RankingPage v_HistoryPage = new()
        {
            Users = v_Users,
            UserElo = v_UserElo,
            UserRanking = v_UserRanking,
            CurrentPage = p_Page,
            TotalPages = (int)Math.Ceiling((double)v_TotalCount / p_Size),
            TotalUsers = (int)v_TotalCount
        };
        return v_HistoryPage;
    }

    public async Task<User> GetUserByEmailAsync(string p_Email, CancellationToken p_CancellationToken = default)
    {
        UserUsr v_UserUsr = await FindBy(
            p_Usr => p_Usr.Email == p_Email,
            p_Include: p_Queryable =>
                p_Queryable.Include(p_Usr => p_Usr.EloElo))
            .SingleOrDefaultAsync(p_CancellationToken);
        if (v_UserUsr == null || v_UserUsr.IsDeleted)
            return null;
        return Mapper.Map<User>(v_UserUsr);
    }

    public async Task<User> GetUserByEmailOrNickNameAsync(string p_Login, CancellationToken p_CancellationToken = default)
    {
        if (p_Login.Contains('@'))
        {
            return await GetUserByEmailAsync(p_Login, p_CancellationToken);
        }
        return await GetUserByNickNameAsync(p_Login, p_CancellationToken);
    }

    public async Task<User> GetUserByNickNameAsync(string p_NickaName, CancellationToken p_CancellationToken = default)
    {
        UserUsr v_UserUsr = await FindBy(p_U => p_U.NickName == p_NickaName).SingleOrDefaultAsync(p_CancellationToken);
        if (v_UserUsr == null || v_UserUsr.IsDeleted)
            return null;
        return Mapper.Map<User>(v_UserUsr);
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

    private async Task DeleteUserAsync(Guid p_UserId, CancellationToken p_CancellationToken = default)
    {
        await DeleteAsync(p_UserId, p_CancellationToken);
    }

    public async Task<User> GetUserByIdAsync(Guid p_Id, CancellationToken p_CancellationToken = default)
    {
        UserUsr v_UserUsr = await FindBy(p_U => p_U.Id == p_Id,
            null,
            p_P => p_P
                .Include(p_UserUsr => p_UserUsr.EloElo)
                .ThenInclude(p_EloElo => p_EloElo.IdThemeNavigation)
        ).SingleOrDefaultAsync(p_CancellationToken);

        if (v_UserUsr == null || v_UserUsr.IsDeleted)
            return null;
        return Mapper.Map<User>(v_UserUsr);
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