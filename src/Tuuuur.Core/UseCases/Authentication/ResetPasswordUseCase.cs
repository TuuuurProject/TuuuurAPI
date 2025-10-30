using Ardalis.GuardClauses;
using MediatR;
using Microsoft.Extensions.Logging;
using Tuuuur.Core.Requests.Authentication;
using Tuuuur.Core.Requests.Tools;
using Tuuuur.Core.Responses;
using Tuuuur.Domain.Bo;
using Tuuuur.Domain.Errors;
using Tuuuur.Domain.Interfaces.Data;

namespace Tuuuur.Core.UseCases.Authentication;

internal class ResetPasswordUseCase(
    IUnitOfWork p_UnitOfWork, 
    ILogger<ResetPasswordUseCase> p_Logger, 
    IMediator p_Mediator)
    : ADbUseCase<ResetPasswordRequest, EmptyResponse>(p_Logger, p_UnitOfWork)
{
    protected override async Task<EmptyResponse> HandleLogic(ResetPasswordRequest p_Request, CancellationToken p_CancellationToken)
    {
        User v_User = await m_UnitOfWork.UserRepository.GetUserByEmailOrNickNameAsync(p_Request.Login, p_CancellationToken);
        
        if(v_User == null || v_User.IsGoogleUser)
            return new EmptyResponse([new ErrorDto(DomainErrors.Data.NotFound, $"Queried object {nameof(User)} was not found, Key: {p_Request.Login}")]);

        UserAuth v_UserAuth = await m_UnitOfWork.UserAuthRepository.GetUserAuthByUserIdAndCodeAsync(v_User.Id, p_Request.Code, p_CancellationToken);
            
        if(v_UserAuth == null)
            return new EmptyResponse([new ErrorDto(DomainErrors.Data.NotFound, $"Queried object {nameof(UserAuth)} was not found, Key: {p_Request.Code}")]);
            
        StringResponse v_HashResponse = await p_Mediator.Send(new HashRequest(p_Request.Password), p_CancellationToken);
        if (!v_HashResponse.Success) return new EmptyResponse(v_HashResponse.Errors);

        v_User.Password = v_HashResponse.Value;
        await m_UnitOfWork.UserRepository.UpdateUserAsync(v_User, p_CancellationToken);
        await m_UnitOfWork.UserAuthRepository.DeleteUserAuthAsync(v_UserAuth.Id, p_CancellationToken);
        _ = m_UnitOfWork.Save();
            
        return new EmptyResponse();
    }
}