using Tuuuur.Core.Requests.Authentication;
using Tuuuur.Core.Requests.Tools;
using Tuuuur.Core.Responses;
using Tuuuur.Core.Responses.Authentication;
using Tuuuur.Domain.Bo;
using Tuuuur.Domain.Interfaces.Data;
using Tuuuur.Domain.Interfaces.Token;
using Tuuuur.Domain.Token;
using MediatR;
using Microsoft.Extensions.Logging;
using Tuuuur.Domain.Errors;

namespace Tuuuur.Core.UseCases.Authentication;

internal class JwtAuthenticationUseCase : AUseCase, IRequestHandler<JwtAuthenticationRequest, JwtAuthenticationResponse>
{
    private readonly IJwtFactory m_JwtFactory;
    private readonly IMediator m_Mediator;

    public JwtAuthenticationUseCase(IUnitOfWork p_UnitOfWork, ILogger<JwtAuthenticationUseCase> p_Logger, IJwtFactory p_JwtFactory, IMediator p_Mediator) : base(p_UnitOfWork, p_Logger)
    {
        m_JwtFactory = p_JwtFactory;
        m_Mediator = p_Mediator;
    }

    [SuppressMessage("Style", "IDE1006:Styles d'affectation de noms", Justification = "Inherited named")]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public async Task<JwtAuthenticationResponse> Handle(JwtAuthenticationRequest request, CancellationToken cancellationToken)
    {
        try
        {
            await Task.Delay(5);
            /*User v_User = await m_UnitOfWork.UserRepository.GetUserByEmailAsync(request.Login, cancellationToken);

            StringResponse v_HashResponse = await m_Mediator.Send(new HashRequest(request.Password), cancellationToken);

            // Forward errors if any
            if (!v_HashResponse.Success) return new JwtAuthenticationResponse(v_HashResponse.Errors);

            if (v_User is null || v_User.Password != v_HashResponse.Value)
                return new JwtAuthenticationResponse(new[] { new ErrorDto(DomainErrors.Authentication.Missmatch, "Invalid login and/or password") });

            JwtTokenResponse v_TokenInfos = m_JwtFactory.CreateToken(v_User);*/

            // TMPORARY
            User v_User = new();
            JwtTokenResponse v_TokenInfos = new();

            return new JwtAuthenticationResponse(new UserToken
            {
                Token = v_TokenInfos,
                User = v_User
            });
        }
        catch (Exception v_Ex)
        {
            m_Logger.LogError(v_Ex, "An error was thrown");
            return new JwtAuthenticationResponse(new[] { v_Ex.ToError() });
        }
    }
}