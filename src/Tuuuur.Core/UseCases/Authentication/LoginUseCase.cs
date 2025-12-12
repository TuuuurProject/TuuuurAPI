using Tuuuur.Core.Requests.Authentication;
using Tuuuur.Core.Requests.Tools;
using Tuuuur.Core.Responses;
using Tuuuur.Domain.Bo;
using Tuuuur.Domain.Interfaces.Data;
using MediatR;
using Microsoft.Extensions.Logging;
using Tuuuur.Domain.Emails.Models;
using Tuuuur.Domain.Errors;
using Tuuuur.Domain.Interfaces.Emails;
using Tuuuur.Domain.Images;

namespace Tuuuur.Core.UseCases.Authentication;

internal class LoginUseCase(
    IUnitOfWork p_UnitOfWork, 
    ILogger<LoginUseCase> p_Logger, 
    IMediator p_Mediator,     
    IRenderingService p_RenderingService, 
    IEmailService p_EmailService)
    : ADbUseCase<LoginRequest, EmptyResponse>(p_Logger, p_UnitOfWork)
{
    protected override async Task<EmptyResponse> HandleLogic(LoginRequest p_Request, CancellationToken p_CancellationToken)
    {
        User v_User = await m_UnitOfWork.UserRepository.GetUserByEmailOrNickNameAsync(p_Request.Login, p_CancellationToken);

        StringResponse v_HashResponse = await p_Mediator.Send(new HashRequest(p_Request.Password), p_CancellationToken);

        // Forward errors if any
        if (!v_HashResponse.Success) return new EmptyResponse(v_HashResponse.Errors);

        if (v_User is null || v_User.Password != v_HashResponse.Value || v_User.IsNew || v_User.IsGoogleUser)
            return new EmptyResponse([new ErrorDto(DomainErrors.Authentication.Invalid, "Invalid login and/or password")]);
            

        GenericEntityResponse<UserAuth> v_UserAuth = await p_Mediator.Send(new GenerateOptRequest(v_User),  p_CancellationToken);
        if(!v_UserAuth.Success) return new EmptyResponse(v_UserAuth.Errors);
            
        TwoFactorAuthModel v_ModelToRender = new()
        {
            NickName = v_User.NickName,
            TwoFactorCode = v_UserAuth.Value.Code,
        };
            
        string v_Content = await p_RenderingService.RenderAsync(v_ModelToRender);

        Dictionary<string, string> v_InlineImages = new()
        {
            { "LogoImage", Logo.GetFullPath() }
        };
                
        await p_EmailService.SendAsync(
            $"Tuuuur - Code de vérification 2FA",
            v_Content,
            [v_User.Email],
            p_InlineImages: v_InlineImages,
            p_CancellationToken: p_CancellationToken);            
        
        return new EmptyResponse();    
    }
}