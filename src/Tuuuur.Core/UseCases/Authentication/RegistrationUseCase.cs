using MediatR;
using Microsoft.Extensions.Logging;
using System.Data;
using Tuuuur.Core.Configuration;
using Tuuuur.Core.Requests.Authentication;
using Tuuuur.Core.Requests.Tools;
using Tuuuur.Core.Responses;
using Tuuuur.Domain.Bo;
using Tuuuur.Domain.Emails.Models;
using Tuuuur.Domain.Errors;
using Tuuuur.Domain.Interfaces.Data;
using Tuuuur.Domain.Interfaces.Data.Entities;
using Tuuuur.Domain.Interfaces.Emails;

namespace Tuuuur.Core.UseCases.Authentication;

/// <summary>
/// UseCase for authentification
/// </summary>
/// <param name="p_UnitOfWork"></param>
/// <param name="p_Logger"></param>
/// <param name="p_Mediator"></param>
/// <param name="m_RenderingService"></param>
/// <param name="m_EmailService"></param>
/// <param name="m_WebsiteConfiguration"></param>
internal class RegistrationUseCase(
    IUnitOfWork p_UnitOfWork, 
    ILogger<RegistrationUseCase> p_Logger, 
    IMediator p_Mediator, 
    IRenderingService m_RenderingService, 
    IEmailService m_EmailService, 
    WebsiteConfiguration m_WebsiteConfiguration)
    : AUseCase(p_UnitOfWork, p_Logger), IRequestHandler<RegistrationRequest, UserResponse>
{

    [SuppressMessage("Style", "IDE1006:Styles d'affectation de noms", Justification = "Inherited named")]
    [SuppressMessage("ReSharper", "InconsistentNaming")]

    public async Task<UserResponse> Handle(RegistrationRequest request, CancellationToken cancellationToken)
    {
        try
        {
            if (await m_UnitOfWork.UserRepository.GetUserByEmailAsync(request.User.Email, cancellationToken) != null)
                throw new DuplicateNameException("An user already exists with this email");
            
            if (await m_UnitOfWork.UserRepository.GetUserByNickNameAsync(request.User.NickName, cancellationToken) != null)
                throw new DuplicateNameException("An user already exists with this nickname");

            StringResponse v_HashResponse = await p_Mediator.Send(new HashRequest(request.User.Password), cancellationToken);

            // Forward errors if any
            if (!v_HashResponse.Success) return new UserResponse(v_HashResponse.Errors);

            request.User.Password = v_HashResponse.Value;

            
            IMappingAddEntity<User, IEntity> v_UserMap = await m_UnitOfWork.UserRepository.CreateUserAsync(request.User, cancellationToken);

            m_UnitOfWork.Save();
            
            ConfirmAccountModel v_ModelToRender = new()
            {
                Fullname = request.User.FullName,
                ConfirmAccountAddress = $"{m_WebsiteConfiguration.BaseUri}/ConfirmEmail",
            };
			
            string v_Content = await m_RenderingService.RenderAsync(v_ModelToRender);

            await m_EmailService.SendAsync($"Confirmez votre e-mail pour rejoindre Tuuuur !", v_Content, new List<string>() { request.User.Email }, p_CancellationToken: cancellationToken);

            return new UserResponse(v_UserMap.MapBoEntity);
        }
        catch (DuplicateNameException v_Ex)
        {
            m_Logger.LogError(v_Ex, "An user already exists with this email");
            return new UserResponse([v_Ex.ToError(DomainErrors.Data.AlreadyExist)]);
        }
        catch (Exception v_Ex)
        {
            m_Logger.LogError(v_Ex, "An error was thrown");
            return new UserResponse([v_Ex.ToError()]);
        }
    }
}