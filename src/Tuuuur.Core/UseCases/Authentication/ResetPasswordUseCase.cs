using Ardalis.GuardClauses;
using MediatR;
using Microsoft.Extensions.Logging;
using Tuuuur.Core.Requests.Authentication;
using Tuuuur.Core.Requests.Tools;
using Tuuuur.Core.Responses;
using Tuuuur.Domain.Bo;
using Tuuuur.Domain.Interfaces.Data;

namespace Tuuuur.Core.UseCases.Authentication;

internal class ResetPasswordUseCase(
    IUnitOfWork p_UnitOfWork, 
    ILogger<ResetPasswordUseCase> p_Logger, 
    IMediator p_Mediator)
    : AUseCase(p_UnitOfWork, p_Logger), IRequestHandler<ResetPasswordRequest, EmptyResponse>
{
    [SuppressMessage("Style", "IDE1006:Styles d'affectation de noms", Justification = "Inherited named")]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public async Task<EmptyResponse> Handle(ResetPasswordRequest request, CancellationToken cancellationToken)
    {
        try
        {
            User v_User = await m_UnitOfWork.UserRepository.GetUserByEmailOrNickNameAsync(request.Login, cancellationToken) 
                          ?? throw new NotFoundException(request.Login, nameof(User));
            
            UserAuth v_UserAuth = await m_UnitOfWork.UserAuthRepository.GetUserAuthByUserIdAndCodeAsync(v_User.Id, request.Code, cancellationToken)
                                  ?? throw new NotFoundException(request.Code, nameof(UserAuth));
            
            if(v_User.IsGoogleUser)
                throw new NotFoundException(request.Login, nameof(User));
            
            StringResponse v_HashResponse = await p_Mediator.Send(new HashRequest(request.Password), cancellationToken);
            if (!v_HashResponse.Success) return new EmptyResponse(v_HashResponse.Errors);

            v_User.Password = v_HashResponse.Value;
            await m_UnitOfWork.UserRepository.UpdateUserAsync(v_User, cancellationToken);
            await m_UnitOfWork.UserAuthRepository.DeleteUserAuthAsync(v_UserAuth.Id, cancellationToken);
            _ = m_UnitOfWork.Save();
            
            return new EmptyResponse();
        }
        catch (Exception v_Ex)
        {
            m_Logger.LogError(v_Ex, "An error was thrown");
            return new EmptyResponse([v_Ex.ToError()]);
        }
    }
}