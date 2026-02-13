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

        string v_PartyCode = await p_CacheService.GetAsync<string>(RedisKeys.User.UserParty(v_User.Id), p_CancellationToken);

        if (v_PartyCode is null)
        {
            return new EmptyResponse([new ErrorDto(DomainErrors.Data.NotFound, $"Queried object {nameof(Party)} was not found")]);
        }
        GroupParty v_Party = await p_CacheService.GetAsync<GroupParty>(RedisKeys.Party.ByCode(v_PartyCode), p_CancellationToken);
        
        // If user is not in the party
        if (v_PartyCode != v_Party.Code || v_Party.IdUserHost != v_User.Id)
            return new EmptyResponse([new ErrorDto(DomainErrors.Data.NotFound, $"Queried object {nameof(Party)} was not found")]);

        // Update party
        IEnumerable<Difficulty> v_Difficulties = await m_UnitOfWork.DifficultyRepository.GetAllDifficultiesAsync(p_CancellationToken);
        IEnumerable<Theme> v_Themes = await m_UnitOfWork.ThemeRepository.GetAllThemesAsync(p_CancellationToken);
        v_Party.NbQuestions = p_Request.NbQuestions;
        v_Party.PartyDifficulty = p_Request.DifficultiesIds
            .Select(p_Id => new PartyDifficulty { IdDifficulty = p_Id, Difficulty = v_Difficulties.FirstOrDefault(p_P => p_P.Id == p_Id)}).ToList();
        v_Party.PartyTheme = p_Request.ThemesIds
            .Select(p_Id => new PartyTheme { IdTheme = p_Id, Theme = v_Themes.FirstOrDefault(p_P => p_P.Id == p_Id) }).ToList();
        v_Party.ScoreEachRound = p_Request.ScoreEachRound;

        await p_CacheService.SetAsync(RedisKeys.Party.ByCode(v_Party.Code), v_Party, p_CancellationToken: p_CancellationToken);
        
        List<int> v_UserIds = await p_CacheService.SetMembersAsync<int>(
            RedisKeys.Party.Users(v_Party.Code),
            CancellationToken.None
        );

        // Get all users in a single database query to avoid DbContext concurrency issues
        List<User> v_Users = await m_UnitOfWork.UserRepository.GetUsersByIdsAsync(v_UserIds, p_CancellationToken);

        v_Party.PartyUsers.AddRange(v_Users.Select(p_P => new PartyUser { User = p_P }));
        
        // Notify all players via WebSocket that party is updated
        await p_GroupNotificationService.NotifyPartyUpdatedAsync(
            v_Party.Code,
            v_Party
        );

        return new EmptyResponse();
    }
}