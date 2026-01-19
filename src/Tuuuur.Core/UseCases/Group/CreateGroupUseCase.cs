using System.Security.Cryptography;
using Microsoft.Extensions.Logging;
using Tuuuur.Core.Requests.Group;
using Tuuuur.Core.Responses;
using Tuuuur.Domain.Bo;
using Tuuuur.Domain.Bo.Enum;
using Tuuuur.Domain.Configuration;
using Tuuuur.Domain.Interfaces;
using Tuuuur.Domain.Interfaces.Data;
using Tuuuur.Domain.Security;

namespace Tuuuur.Core.UseCases.Group;

internal class CreateGroupUseCase(IUnitOfWork p_UnitOfWork,
    ILogger<CreateGroupUseCase> p_Logger,
    IUserRoleService p_UserRoleService,
    ICacheService p_CacheService) :
    ACreateJoinGroupUseCase<CreateGroupPartyRequest>(p_Logger, p_UnitOfWork, p_UserRoleService, p_CacheService)
{
    protected override async Task<GenericEntityResponse<GroupParty>> Process(CreateGroupPartyRequest p_Request, User p_User, CancellationToken p_CancellationToken)
    {

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

            GroupParty v_ExistingParty = await m_CacheService.GetAsync<GroupParty>(RedisKeys.Party.ByCode(v_GeneratedCode), p_CancellationToken);
            if (v_ExistingParty == null)
            {
                v_Code = v_GeneratedCode;
            }
        } while (v_Code == string.Empty);

        // Create party with default config 
        // TODO: Defaults settings needs to be configurable in appsettings
        GroupParty v_Party = new()
        {
            IdPartyType = (int)PartyTypeType.Group,
            Code = v_Code,
            IdUserHost = p_User.Id,
            Dt = DateTime.Now,
            Active = true,
            InProgress = false,
            NbQuestions = 10,
        };

        await m_CacheService.SetAsync(RedisKeys.Party.ByCode(v_Party.Code), v_Party, p_CancellationToken: p_CancellationToken);
        await m_CacheService.SetAddAsync(RedisKeys.Party.Users(v_Party.Code), p_User.Id, p_CancellationToken: p_CancellationToken);
        await m_CacheService.SetAsync(RedisKeys.User.UserParty(p_User.Id), v_Party.Code, p_CancellationToken: p_CancellationToken);

        v_Party.PartyUsers.Add(new PartyUser() { IdUser = p_User.Id, User = p_User });

        return new GenericEntityResponse<GroupParty>(v_Party);
    }
}