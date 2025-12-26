using Microsoft.Extensions.Logging;
using Tuuuur.Core.Requests.Group;
using Tuuuur.Core.Responses;
using Tuuuur.Domain.Bo;
using Tuuuur.Domain.Configuration;
using Tuuuur.Domain.Errors;
using Tuuuur.Domain.Interfaces;
using Tuuuur.Domain.Interfaces.Data;
using Tuuuur.Domain.Notifications;
using Tuuuur.Domain.Security;

namespace Tuuuur.Core.UseCases.Group;

internal class EditGroupSettingsUseCase(
    IUnitOfWork p_UnitOfWork,
    ILogger<EditGroupSettingsUseCase> p_Logger,
    IGroupNotificationService p_GroupNotificationService,
    IUserRoleService p_UserRoleService,
    ICacheService p_CacheService) :
    ADbUseCase<EditGroupSettingsRequest, EmptyResponse>(p_Logger, p_UnitOfWork)
{
    protected override async Task<EmptyResponse> HandleLogic(EditGroupSettingsRequest p_Request, CancellationToken p_CancellationToken)
    {
        string v_UserEmail = p_UserRoleService.GetCurrentUserEmail();

        User v_User = await m_UnitOfWork.UserRepository.GetUserByEmailAsync(v_UserEmail, p_CancellationToken);

        if (v_User == null)
            return new EmptyResponse([new ErrorDto(DomainErrors.Data.NotFound, $"Queried object {nameof(User)} was not found, Key: {v_UserEmail}")]);

        Guid? v_Parties = await p_CacheService.GetAsync<Guid?>(RedisKeys.User.Party(v_User.Id), p_CancellationToken);

        if (v_Parties is null)
        {
            return new EmptyResponse([new ErrorDto(DomainErrors.Data.NotFound, $"Queried object {nameof(Party)} was not found")]);
        }

        Party v_Party = await p_CacheService.GetAsync<Party>(RedisKeys.Party.ById(v_Parties.Value), p_CancellationToken);

        List<int> v_UserInParty = await p_CacheService.SetMembersAsync<int>(RedisKeys.Party.Users(v_Party.Id), p_CancellationToken: p_CancellationToken);
        Guid v_CurrentParty = await p_CacheService.GetAsync<Guid>(RedisKeys.User.Party(v_User.Id), p_CancellationToken: p_CancellationToken);

        // If user is not in the party
        if (v_CurrentParty != v_Party.Id || v_Party.IdUserHost != v_User.Id )
            return new EmptyResponse([new ErrorDto(DomainErrors.Data.NotFound, $"Queried object {nameof(Party)} was not found")]);
        
        // Update party
        v_Party.NbQuestions = p_Request.NbQuestions;
        v_Party.PartyDifficulty = p_Request.DifficultiesIds
            .Select(p_Id => new PartyDifficulty { IdDifficulty = p_Id }).ToList();
        v_Party.PartyTheme = p_Request.ThemesIds
            .Select(p_Id => new PartyTheme() { IdTheme = p_Id }).ToList();

        await p_CacheService.SetAsync(RedisKeys.Party.ById(v_Party.Id), v_Party, p_CancellationToken: p_CancellationToken);
        
        // Notify all players via WebSocket that party is updated
        await p_GroupNotificationService.NotifyPartyUpdatedAsync(
            v_Party.Id.ToString(),
            v_Party
        );
        
        return new EmptyResponse();
    }
}