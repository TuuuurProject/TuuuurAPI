using System.Security.Cryptography;
using Microsoft.Extensions.Logging;
using Tuuuur.Core.Requests.Group;
using Tuuuur.Core.Responses;
using Tuuuur.Domain.Bo;
using Tuuuur.Domain.Bo.Enum;
using Tuuuur.Domain.Errors;
using Tuuuur.Domain.Interfaces;
using Tuuuur.Domain.Interfaces.Data;
using Tuuuur.Domain.Security;

namespace Tuuuur.Core.UseCases.Group;

internal class CreateGroupPartyUseCase(IUnitOfWork p_UnitOfWork, 
    ILogger<CreateGroupPartyUseCase> p_Logger,
    IUserRoleService p_UserRoleService,
    ICacheService p_CacheService): 
    ADbUseCase<CreateGroupPartyRequest, GenericEntityResponse<Party>>(p_Logger, p_UnitOfWork)
{
    protected override async Task<GenericEntityResponse<Party>> HandleLogic(CreateGroupPartyRequest p_Request, CancellationToken p_CancellationToken)
    {
        string v_UserEmail = p_UserRoleService.GetCurrentUserEmail();
        
        User v_User = await m_UnitOfWork.UserRepository.GetUserByEmailAsync(v_UserEmail, p_CancellationToken);
        
        if(v_User == null)
            return new GenericEntityResponse<Party>([new ErrorDto(DomainErrors.Data.NotFound, $"Queried object {nameof(User)} was not found, Key: {v_UserEmail}")]);
        
        Guid? v_PartyId = await p_CacheService.GetAsync<Guid?>($"{nameof(User)}:{v_User.Id}:{nameof(Party)}", p_CancellationToken);

        // If a party already exist, 
        if (v_PartyId is not null)
        {
            Party v_ExistingParty = await p_CacheService.GetAsync<Party>($"{nameof(Party)}:{v_PartyId}", p_CancellationToken);
            List<int> v_UserInExistingParty = await p_CacheService.SetMembersAsync<int>($"{nameof(Party)}:{v_PartyId}:{nameof(User)}", p_CancellationToken: p_CancellationToken);
            foreach (int v_UserId in v_UserInExistingParty)
            {
                v_ExistingParty.PartyUsers.Add(new PartyUser { User = await m_UnitOfWork.UserRepository.GetUserByIdAsync(v_UserId, p_CancellationToken), IdUser =  v_UserId});
            }
            
            return new GenericEntityResponse<Party>(v_ExistingParty);
        }
        
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
            IdUserHost = v_User.Id,
            Dt =  DateTime.Now,
            Active =  true,
        };

        await p_CacheService.SetAsync($"{nameof(Party)}:{v_Party.Id}", v_Party, p_CancellationToken: p_CancellationToken);
        await p_CacheService.SetAsync($"{nameof(Party)}:{v_Party.Code}", v_Party, p_CancellationToken: p_CancellationToken);
        await p_CacheService.SetAddAsync($"{nameof(Party)}:{v_Party.Id}:{nameof(User)}", v_User.Id, p_CancellationToken: p_CancellationToken);
        await p_CacheService.SetAsync($"{nameof(User)}:{v_User.Id}:{nameof(Party)}", v_Party.Id, p_CancellationToken: p_CancellationToken);
        
        v_Party.PartyUsers.Add(new PartyUser(){IdUser =  v_User.Id, User =  v_User});
        
        return new GenericEntityResponse<Party>(v_Party);
    }
}