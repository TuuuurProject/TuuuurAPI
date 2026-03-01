using MediatR;
using Microsoft.Extensions.Logging;
using System.Data;
using Tuuuur.Core.Requests.Authentication;
using Tuuuur.Core.Requests.Tools;
using Tuuuur.Core.Responses;
using Tuuuur.Domain.Bo;
using Tuuuur.Domain.Emails.Models;
using Tuuuur.Domain.Errors;
using Tuuuur.Domain.Images;
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
/// <param name="p_RenderingService"></param>
/// <param name="p_EmailService"></param>
internal class RegistrationUseCase(
    IUnitOfWork p_UnitOfWork,
    ILogger<RegistrationUseCase> p_Logger,
    IMediator p_Mediator,
    IRenderingService p_RenderingService,
    IEmailService p_EmailService)
    : ADbUseCase<RegistrationRequest, EmptyResponse>(p_Logger, p_UnitOfWork)
{
    protected override async Task<EmptyResponse> HandleLogic(RegistrationRequest p_Request, CancellationToken p_CancellationToken)
    {
        await m_UnitOfWork.UserRepository.DeleteUserNotRegisteredAsync(p_CancellationToken);
        _ = m_UnitOfWork.Save();
        return await m_UnitOfWork.ExecutionStrategy(async () => await RegisterUserAsync(p_Request, p_CancellationToken));
    }

    private async Task<EmptyResponse> RegisterUserAsync(RegistrationRequest p_Request, CancellationToken p_CancellationToken)
    {
        try
        {
            m_UnitOfWork.BeginTransaction();
            if (await m_UnitOfWork.UserRepository.GetUserByEmailAsync(p_Request.User.Email, p_CancellationToken) != null)
                throw new DuplicateNameException("An user already exists with this email");

            if (await m_UnitOfWork.UserRepository.GetUserByNickNameAsync(p_Request.User.NickName, p_CancellationToken) != null)
                throw new DuplicateNameException("An user already exists with this nickname");

            StringResponse v_HashResponse = await p_Mediator.Send(new HashRequest(p_Request.User.Password), p_CancellationToken);

            // Forward errors if any
            if (!v_HashResponse.Success) return new EmptyResponse(v_HashResponse.Errors);

            p_Request.User.Password = v_HashResponse.Value;
            p_Request.User.IsNew = true;
            p_Request.User.IsGoogleUser = false;

            // Initialize Elo at 1000 for every theme so ranked matchmaking works from day one
            IEnumerable<Theme> v_Themes = await m_UnitOfWork.ThemeRepository.GetAllThemesAsync(p_CancellationToken);
            p_Request.User.Elo = v_Themes
                .Select(p_Theme => new Elo { IdTheme = p_Theme.Id, Value = 1000 })
                .ToList();

            IMappingAddEntity<User, IEntity> v_UserMap = await m_UnitOfWork.UserRepository.CreateUserAsync(p_Request.User, p_CancellationToken);
            m_UnitOfWork.Save();

            GenericEntityResponse<UserAuth> v_UserAuth = await p_Mediator.Send(new GenerateOptRequest(v_UserMap.MapBoEntity), p_CancellationToken);
            if (!v_UserAuth.Success) return new EmptyResponse(v_UserAuth.Errors);

            ConfirmAccountModel v_ModelToRender = new()
            {
                NickName = p_Request.User.NickName,
                TwoFactorCode = v_UserAuth.Value.Code,
            };

            string v_Content = await p_RenderingService.RenderAsync(v_ModelToRender);

            Dictionary<string, string> v_InlineImages = new()
            {
                { "LogoImage", Logo.GetDataUri() }
            };

            await p_EmailService.SendAsync(
                "Tuuuur - Confirmez votre e-mail",
                v_Content,
                [p_Request.User.Email],
                p_InlineImages: v_InlineImages,
                p_CancellationToken: p_CancellationToken);


            m_UnitOfWork.CommitTransaction();
            return new EmptyResponse();
        }
        catch (DuplicateNameException v_Ex)
        {
            m_UnitOfWork.RollbackTransaction();
            m_Logger.LogError(v_Ex, "An user already exists with this email");
            return new EmptyResponse([v_Ex.ToError(DomainErrors.Data.AlreadyExist)]);
        }
        catch (Exception v_Ex)
        {
            m_UnitOfWork.RollbackTransaction();
            m_Logger.LogError(v_Ex, "An error was thrown");
            return new EmptyResponse([v_Ex.ToError()]);
        }
    }
}