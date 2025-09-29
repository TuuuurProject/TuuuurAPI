using Tuuuur.Core.Requests.Authentication;
using Tuuuur.Core.Requests.Tools;
using Tuuuur.Core.Responses;
using Tuuuur.Core.Responses.Authentication;
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
    : AUseCase(p_UnitOfWork, p_Logger), IRequestHandler<LoginRequest, EmptyResponse>
{

    [SuppressMessage("Style", "IDE1006:Styles d'affectation de noms", Justification = "Inherited named")]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public async Task<EmptyResponse> Handle(LoginRequest request, CancellationToken cancellationToken)
    {
        try
        {
            User v_User = await m_UnitOfWork.UserRepository.GetUserByEmailOrNickNameAsync(request.Login, cancellationToken);

            StringResponse v_HashResponse = await p_Mediator.Send(new HashRequest(request.Password), cancellationToken);

            // Forward errors if any
            if (!v_HashResponse.Success) return new EmptyResponse(v_HashResponse.Errors);

            if (v_User is null || v_User.Password != v_HashResponse.Value || v_User.IsNew)
                return new EmptyResponse([new ErrorDto(DomainErrors.Authentication.Invalid, "Invalid login and/or password")]);
            

            GenericEntityResponse<UserAuth> v_UserAuth = await p_Mediator.Send(new GenerateOptRequest(v_User),  cancellationToken);
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
                p_CancellationToken: cancellationToken);            
            return new EmptyResponse();
        }
        catch (Exception v_Ex)
        {
            m_Logger.LogError(v_Ex, "An error was thrown");
            return new EmptyResponse([v_Ex.ToError()]);
        }
    }
}