using System.Security.Cryptography;
using Microsoft.Extensions.Logging;
using Tuuuur.Core.Requests.Group;
using Tuuuur.Core.Responses;
using Tuuuur.Domain.Bo;
using Tuuuur.Domain.Bo.Enum;
using Tuuuur.Domain.Configuration;
using Tuuuur.Domain.Interfaces;
using Tuuuur.Domain.Interfaces.Data;
using Tuuuur.Domain.Notifications;
using Tuuuur.Domain.Security;

namespace Tuuuur.Core.UseCases.Group;

internal class CreateGroupUseCase(IUnitOfWork p_UnitOfWork,
    ILogger<CreateGroupUseCase> p_Logger,
    IUserRoleService p_UserRoleService,
    ICacheService p_CacheService) :
    ACreateJoinGroupUseCase<CreateGroupPartyRequest>(p_Logger, p_UnitOfWork, p_UserRoleService, p_CacheService)
{
    protected override async Task<GenericEntityResponse<Party>> Process(CreateGroupPartyRequest p_Request, User p_User, CancellationToken p_CancellationToken)
    {
        // Create it
        int v_Number;
        using (RandomNumberGenerator v_Generator = RandomNumberGenerator.Create())
        {
            byte[] v_Bytes = new byte[4];
            v_Generator.GetBytes(v_Bytes);
            v_Number = BitConverter.ToInt32(v_Bytes, 0) & 0x7FFFFFFF;
        }

        int v_SixDigit = (v_Number % 900000) + 100000;
        string v_Code = v_SixDigit.ToString();

        Party v_Party = new()
        {
            Id = Guid.NewGuid(),
            IdPartyType = (int)PartyTypeType.Group,
            Code = v_Code,
            IdUserHost = p_User.Id,
            Dt = DateTime.Now,
            Active = true,
        };

        await m_CacheService.SetAsync(RedisKeys.Party.ById(v_Party.Id), v_Party, p_CancellationToken: p_CancellationToken);
        await m_CacheService.SetAsync(RedisKeys.Party.ByCode(v_Party.Code), v_Party, p_CancellationToken: p_CancellationToken);
        await m_CacheService.SetAddAsync(RedisKeys.Party.Users(v_Party.Id), p_User.Id, p_CancellationToken: p_CancellationToken);
        await m_CacheService.SetAsync(RedisKeys.User.UserParty(p_User.Id), v_Party.Id, p_CancellationToken: p_CancellationToken);

        v_Party.PartyUsers.Add(new PartyUser() { IdUser = p_User.Id, User = p_User });

        return new GenericEntityResponse<Party>(v_Party);
    }
}