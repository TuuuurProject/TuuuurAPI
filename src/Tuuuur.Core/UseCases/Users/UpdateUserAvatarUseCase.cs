using Ardalis.GuardClauses;
using MediatR;
using Microsoft.Extensions.Logging;
using Tuuuur.Core.Requests.Users;
using Tuuuur.Core.Responses;
using Tuuuur.Domain.Bo;
using Tuuuur.Domain.Errors;
using Tuuuur.Domain.Interfaces.Data;
using Tuuuur.Domain.Security;

namespace Tuuuur.Core.UseCases.Users;

internal class UpdateUserAvatarUseCase(
    IUnitOfWork p_UnitOfWork, 
    ILogger<UpdateUserAvatarUseCase> p_Logger,
    IUserRoleService p_UserRoleService)
    : AUseCase(p_UnitOfWork, p_Logger), IRequestHandler<UpdateUserAvatarRequest, GenericEntityResponse<User>>
{
    [SuppressMessage("Style", "IDE1006:Styles d'affectation de noms", Justification = "Inherited named")]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public async Task<GenericEntityResponse<User>> Handle(UpdateUserAvatarRequest request, CancellationToken cancellationToken)
    {
        try
        {
            string v_UserEmail = p_UserRoleService.GetCurrentUserEmail();
            
            User v_User = await m_UnitOfWork.UserRepository.GetUserByEmailAsync(v_UserEmail, cancellationToken) 
                          ?? throw new NotFoundException(v_UserEmail, nameof(User));
            
            v_User.Avatar = request.Avatar;
            
            await m_UnitOfWork.UserRepository.UpdateUserAsync(v_User, cancellationToken);
            m_UnitOfWork.Save();
            
            return new GenericEntityResponse<User>(v_User);
        }
        catch (NotFoundException v_Ex)
        {
            m_Logger.LogError(v_Ex, "Data not found");
            return new GenericEntityResponse<User>([v_Ex.ToError(DomainErrors.Data.NotFound)]);
        }
        catch (Exception v_Ex)
        {
            m_Logger.LogError(v_Ex, "An error was thrown");
            return new GenericEntityResponse<User>([v_Ex.ToError(DomainErrors.UnknowError)]);
        }
    }
}