using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Tuuuur.Domain.Bo;
using Tuuuur.Domain.Interfaces.Data.Entities;
using Tuuuur.Domain.Interfaces.Data.Repositories;
using Tuuuur.Infrastructure.Data.EntityFramework.Entities;
using Tuuuur.Infrastructure.Data.Mapping;

namespace Tuuuur.Infrastructure.Data.EntityFramework.Repositories;

internal class UserPartyQuestionRepository(DbContext p_DbContext, IMapper p_Mapper, ILogger<UserPartyQuestionRepository> p_Logger)
    : GenericRepository<UserPartyQuestionUpq>(p_DbContext, p_Mapper, p_Logger), IUserPartyQuestionRepository
{
    public async Task<IMappingAddEntity<UserPartyQuestion, IEntity>> CreateUserPartyQuestionAsync(
        UserPartyQuestion p_Party, 
        CancellationToken p_CancellationToken = default)
    {
        IMappingAddEntity<UserPartyQuestion, UserPartyQuestionUpq> v_Mapping =
            new MappingAddEntity<UserPartyQuestion, UserPartyQuestionUpq>(Mapper, p_Party);

        await AddAsync(v_Mapping.DtoEntity, p_CancellationToken);
        return v_Mapping;
    }

    public async Task UpdateAsync(UserPartyQuestion p_UserPartyQuestion)
    {
        UserPartyQuestionUpq v_Entity = Mapper.Map<UserPartyQuestionUpq>(p_UserPartyQuestion);
        await UpdateAsync(v_Entity);
    }
}