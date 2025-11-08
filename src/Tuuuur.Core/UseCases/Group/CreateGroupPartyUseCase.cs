using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Logging;
using Tuuuur.Core.Requests;
using Tuuuur.Core.Requests.Group;
using Tuuuur.Core.Responses;
using Tuuuur.Domain;
using Tuuuur.Domain.Bo;
using Tuuuur.Domain.Bo.Enum;
using Tuuuur.Domain.Errors;
using Tuuuur.Domain.Interfaces.Data;
using Tuuuur.Domain.Security;

namespace Tuuuur.Core.UseCases.Group;

internal class CreateGroupPartyUseCase(IUnitOfWork p_UnitOfWork, 
    ILogger<CreateGroupPartyUseCase> p_Logger,
    IUserRoleService p_UserRoleService): 
    ADbUseCase<CreateGroupPartyRequest, GenericEntityResponse<Party>>(p_Logger, p_UnitOfWork)
{
    protected override async Task<GenericEntityResponse<Party>> HandleLogic(CreateGroupPartyRequest p_Request, CancellationToken p_CancellationToken)
    {
        string v_UserEmail = p_UserRoleService.GetCurrentUserEmail();

        User v_User = await m_UnitOfWork.UserRepository.GetUserByEmailAsync(v_UserEmail, p_CancellationToken);
        
        if(v_User == null)
            return new GenericEntityResponse<Party>([new ErrorDto(DomainErrors.Data.NotFound, $"Queried object {nameof(User)} was not found, Key: {v_UserEmail}")]);
        
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
            PartyUsers = [new PartyUser(){ IdUser = v_User.Id }],
            IdUserHost = v_User.Id,
        };

        lock (InMemoryDataStore.PartyInProgress)
        {
            InMemoryDataStore.PartyInProgress.Add(v_Party);
        }
        
        return new GenericEntityResponse<Party>(v_Party);
    }
}