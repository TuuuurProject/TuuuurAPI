using System.Security.Cryptography;
using Microsoft.Extensions.Logging;
using Tuuuur.Core.Requests.Group;
using Tuuuur.Core.Responses;
using Tuuuur.Domain;
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
    ICacheService<Party> p_CacheService): 
    ADbUseCase<CreateGroupPartyRequest, GenericEntityResponse<Party>>(p_Logger, p_UnitOfWork)
{
    protected override async Task<GenericEntityResponse<Party>> HandleLogic(CreateGroupPartyRequest p_Request, CancellationToken p_CancellationToken)
    {
        string v_UserEmail = p_UserRoleService.GetCurrentUserEmail();
        
        User v_User = await m_UnitOfWork.UserRepository.GetUserByEmailAsync(v_UserEmail, p_CancellationToken);
        
        if(v_User == null)
            return new GenericEntityResponse<Party>([new ErrorDto(DomainErrors.Data.NotFound, $"Queried object {nameof(User)} was not found, Key: {v_UserEmail}")]);
        
        IEnumerable<Party> v_Parties;
        
        //var res = await p_CacheService.GetDataAsync<object>("hr",  p_CancellationToken: p_CancellationToken);

        lock (InMemoryDataStore.PartyInProgress)
        {
            // Check if the user is already in party
            v_Parties  = InMemoryDataStore.PartyInProgress.Where(p_Party => p_Party.PartyUsers.Any(p_User => p_User.IdUser == v_User.Id)).ToList();
        }

        // If a party already exist, 
        if (v_Parties.Any())
        {
            return new GenericEntityResponse<Party>(v_Parties.First());
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
        };

        await p_CacheService.SetAsync(v_Party.Id.ToString(), v_Party, p_CancellationToken: p_CancellationToken);

        /*
        lock (InMemoryDataStore.PartyInProgress)
        {
            InMemoryDataStore.PartyInProgress.Add(v_Party);
            v_Party.PartyUsers.Add(new PartyUser{ IdUser = v_User.Id, User = v_User});
        }*/
        
        return new GenericEntityResponse<Party>(v_Party);
    }
}