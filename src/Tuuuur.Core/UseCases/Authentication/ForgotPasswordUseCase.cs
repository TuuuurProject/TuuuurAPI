using MediatR;
using Microsoft.Extensions.Logging;
using Tuuuur.Core.Requests.Authentication;
using Tuuuur.Core.Responses;
using Tuuuur.Core.Responses.Authentication;
using Tuuuur.Domain.Bo;
using Tuuuur.Domain.Emails.Models;
using Tuuuur.Domain.Images;
using Tuuuur.Domain.Interfaces.Data;
using Tuuuur.Domain.Interfaces.Emails;

namespace Tuuuur.Core.UseCases.Authentication;

internal class ForgotPasswordUseCase(
    IUnitOfWork p_UnitOfWork, 
    ILogger<ForgotPasswordUseCase> p_Logger, 
    IMediator p_Mediator,     
    IRenderingService p_RenderingService, 
    IEmailService p_EmailService)
    : AUseCase(p_UnitOfWork, p_Logger), IRequestHandler<ForgotPasswordRequest, EmptyResponse>
{

    [SuppressMessage("Style", "IDE1006:Styles d'affectation de noms", Justification = "Inherited named")]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public async Task<EmptyResponse> Handle(ForgotPasswordRequest request, CancellationToken cancellationToken)
    {
        try
        {
            User v_User = await m_UnitOfWork.UserRepository.GetUserByEmailOrNickNameAsync(request.Login, cancellationToken);

            // Return 200 OK to prevent email enumeration.
            if (v_User is null || v_User.IsNew)
                return new EmptyResponse();
            
            GenericEntityResponse<UserAuth> v_UserAuth = await p_Mediator.Send(new GenerateOptRequest(v_User),  cancellationToken);
            if(!v_UserAuth.Success) return new EmptyResponse(v_UserAuth.Errors);
            
            ForgotPasswordModel v_ModelToRender = new()
            {
                NickName = v_User.NickName,
                TwoFactorCode = v_UserAuth.Value.Code,
            };
            
            Dictionary<string, string> v_InlineImages = new()
            {
                { "LogoImage", Logo.GetFullPath() }
            };
            
            string v_Content = await p_RenderingService.RenderAsync(v_ModelToRender);
            
            await p_EmailService.SendAsync(
                $"Tuuuur - Code de réinitialisation du mot de passe",
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