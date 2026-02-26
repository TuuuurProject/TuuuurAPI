using System.Security.Cryptography;
using Microsoft.Extensions.Logging;
using Tuuuur.Core.Requests.Group;
using Tuuuur.Core.Responses;
using Tuuuur.Domain.Bo;
using Tuuuur.Domain.Bo.Enum;
using Tuuuur.Domain.Configuration;
using Tuuuur.Domain.Errors;
using Tuuuur.Domain.Interfaces;
using Tuuuur.Domain.Interfaces.Data;
using Tuuuur.Domain.Security;

namespace Tuuuur.Core.UseCases.Group;

internal class CreateGroupUseCase(IUnitOfWork p_UnitOfWork,
    ILogger<CreateGroupUseCase> p_Logger,
    IUserRoleService p_UserRoleService,
    ICacheService p_CacheService) :
    ADbUseCase<CreateGroupPartyRequest, GenericEntityResponse<GroupParty>>(p_Logger, p_UnitOfWork)
{
    protected override async Task<GenericEntityResponse<GroupParty>> HandleLogic(CreateGroupPartyRequest p_Request, CancellationToken p_CancellationToken)
    {
        string v_UserEmail = p_UserRoleService.GetEmail();
        
        User v_User = await m_UnitOfWork.UserRepository.GetUserByEmailAsync(v_UserEmail, p_CancellationToken);
        
        if (v_User == null)
            return new GenericEntityResponse<GroupParty>([new ErrorDto(DomainErrors.Data.NotFound, $"Queried object {nameof(User)} was not found, Key: {v_UserEmail}")]);

        string v_PartyCode = await p_CacheService.GetAsync<string>(RedisKeys.User.UserParty(v_User.Id), p_CancellationToken) ?? string.Empty;
        
        // If a party already exist, 
        if (v_PartyCode != string.Empty)
        {
            GroupParty v_ExistingParty = await p_CacheService.GetAsync<GroupParty>(RedisKeys.Party.ByCode(v_PartyCode), p_CancellationToken);
            List<User> v_UserInExistingParty = await p_CacheService.SetMembersAsync<User>(RedisKeys.Party.Users(v_PartyCode), p_CancellationToken: p_CancellationToken);
            foreach (User v_LocalUser in v_UserInExistingParty)
            {
                v_ExistingParty.PartyUsers.Add(new PartyUser { User =  v_LocalUser, IdUser = v_LocalUser.Id });
            }

            return new GenericEntityResponse<GroupParty>(v_ExistingParty);
        }
        
        string v_Code = string.Empty;
        
        // Necessary to avoid duplicated code
        do
        {
            int v_Number;
            using (RandomNumberGenerator v_Generator = RandomNumberGenerator.Create())
            {
                byte[] v_Bytes = new byte[4];
                v_Generator.GetBytes(v_Bytes);
                v_Number = BitConverter.ToInt32(v_Bytes, 0) & 0x7FFFFFFF;
            }

            int v_SixDigit = (v_Number % 900000) + 100000;
            string v_GeneratedCode = v_SixDigit.ToString();

            GroupParty v_ExistingParty = await p_CacheService.GetAsync<GroupParty>(RedisKeys.Party.ByCode(v_GeneratedCode), p_CancellationToken);
            if (v_ExistingParty == null)
            {
                v_Code = v_GeneratedCode;
            }
        }while(v_Code == string.Empty);

        // Create party with default config 
        // TODO: Defaults settings needs to be configurable in appsettings
        GroupParty v_Party = new()
        {
            IdPartyType = (int)PartyTypeType.Group,
            Code = v_Code,
            IdUserHost = v_User.Id,
            Dt = DateTime.Now,
            Active = true,
            InProgress = false,
            NbQuestions = 10,
        };
        
        await p_CacheService.SetAsync(RedisKeys.Party.ByCode(v_Party.Code), v_Party, p_CancellationToken: p_CancellationToken);
        await p_CacheService.SetAddAsync(RedisKeys.Party.Users(v_Party.Code), v_User, p_CancellationToken: p_CancellationToken);
        await p_CacheService.SetAsync(RedisKeys.User.UserParty(v_User.Id), v_Party.Code, p_CancellationToken: p_CancellationToken);
        
        v_Party.PartyUsers.Add(new PartyUser() { IdUser = v_User.Id, User = v_User });

        return new GenericEntityResponse<GroupParty>(v_Party);
    }
}