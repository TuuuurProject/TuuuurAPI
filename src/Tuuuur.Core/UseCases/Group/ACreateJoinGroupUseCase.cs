using MediatR;
using Microsoft.Extensions.Logging;
using Tuuuur.Core.Responses;
using Tuuuur.Domain.Bo;
using Tuuuur.Domain.Configuration;
using Tuuuur.Domain.Errors;
using Tuuuur.Domain.Interfaces;
using Tuuuur.Domain.Interfaces.Data;
using Tuuuur.Domain.Notifications;
using Tuuuur.Domain.Security;

namespace Tuuuur.Core.UseCases.Group;

internal abstract class ACreateJoinGroupUseCase<TRequest>(
    ILogger m_Logger,
    IUnitOfWork m_UnitOfWork,
    IUserRoleService p_UserRoleService,
    ICacheService p_CacheService)
    : ADbUseCase<TRequest, GenericEntityResponse<GroupParty>>(m_Logger, m_UnitOfWork)
    where TRequest : IRequest<GenericEntityResponse<GroupParty>>
{
    protected readonly ICacheService m_CacheService = p_CacheService;
    protected override async Task<GenericEntityResponse<GroupParty>> HandleLogic(TRequest p_Request, CancellationToken p_CancellationToken)
    {
        string v_UserEmail = p_UserRoleService.GetCurrentUserEmail();

        User v_User = await m_UnitOfWork.UserRepository.GetUserByEmailAsync(v_UserEmail, p_CancellationToken);
        
        if (v_User == null)
            return new GenericEntityResponse<GroupParty>([new ErrorDto(DomainErrors.Data.NotFound, $"Queried object {nameof(User)} was not found, Key: {v_UserEmail}")]);

        string v_PartyCode = await m_CacheService.GetAsync<string>(RedisKeys.User.UserParty(v_User.Id), p_CancellationToken);
        
        // If a party already exist, 
        if (v_PartyCode != null)
        {
            GroupParty v_ExistingParty = await m_CacheService.GetAsync<GroupParty>(RedisKeys.Party.ByCode(v_PartyCode), p_CancellationToken);
            List<int> v_UserInExistingParty = await m_CacheService.SetMembersAsync<int>(RedisKeys.Party.Users(v_PartyCode), p_CancellationToken: p_CancellationToken);
            foreach (int v_UserId in v_UserInExistingParty)
            {
                v_ExistingParty.PartyUsers.Add(new PartyUser { User = await m_UnitOfWork.UserRepository.GetUserByIdAsync(v_UserId, p_CancellationToken), IdUser = v_UserId });
            }

            return new GenericEntityResponse<GroupParty>(v_ExistingParty);
        }

        return await Process(p_Request, v_User, p_CancellationToken);
    }

    protected abstract Task<GenericEntityResponse<GroupParty>> Process(TRequest p_Request, User p_User, CancellationToken p_CancellationToken);
}