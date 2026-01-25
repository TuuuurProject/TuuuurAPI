using MediatR;
using Microsoft.Extensions.Logging;
using Tuuuur.Core.Responses;
using Tuuuur.Domain.Bo;
using Tuuuur.Domain.Configuration;
using Tuuuur.Domain.Errors;
using Tuuuur.Domain.Interfaces;
using Tuuuur.Domain.Interfaces.Data;
using Tuuuur.Domain.Security;

namespace Tuuuur.Core.UseCases.Group;

internal abstract class ACreateJoinGroupUseCase<TRequest>(
    ILogger p_Logger,
    IUnitOfWork p_UnitOfWork,
    IUserRoleService p_UserRoleService,
    ICacheService p_CacheService)
    : ADbUseCase<TRequest, GenericEntityResponse<GroupParty>>(p_Logger, p_UnitOfWork)
    where TRequest : IRequest<GenericEntityResponse<GroupParty>>
{
    protected readonly ICacheService m_CacheService = p_CacheService;
    protected override async Task<GenericEntityResponse<GroupParty>> HandleLogic(TRequest p_Request, CancellationToken p_CancellationToken)
    {
        string v_UserEmail = p_UserRoleService.GetCurrentUserEmail();

        User v_User = await m_UnitOfWork.UserRepository.GetUserByEmailAsync(v_UserEmail, p_CancellationToken);

        if (v_User == null)
            return new GenericEntityResponse<GroupParty>([new ErrorDto(DomainErrors.Data.NotFound, $"Queried object {nameof(User)} was not found, Key: {v_UserEmail}")]);

        string v_PartyCode = await m_CacheService.GetAsync<string>(RedisKeys.User.UserParty(v_User.Id), p_CancellationToken) ?? string.Empty;

        // If a party already exist, 
        if (v_PartyCode != string.Empty)
        {
            GroupParty v_ExistingParty = await m_CacheService.GetAsync<GroupParty>(RedisKeys.Party.ByCode(v_PartyCode), p_CancellationToken);

            // Add users to party users list for the response
            List<int> v_UserInExistingParty = await m_CacheService.SetMembersAsync<int>(RedisKeys.Party.Users(v_PartyCode), p_CancellationToken: p_CancellationToken);
            foreach (int v_UserId in v_UserInExistingParty)
            {
                v_ExistingParty.Users.Add(await m_UnitOfWork.UserRepository.GetUserByIdAsync(v_UserId, p_CancellationToken));
            }

            return new GenericEntityResponse<GroupParty>(v_ExistingParty);
        }

        return await Process(p_Request, v_User, p_CancellationToken);
    }

    protected abstract Task<GenericEntityResponse<GroupParty>> Process(TRequest p_Request, User p_User, CancellationToken p_CancellationToken);
}