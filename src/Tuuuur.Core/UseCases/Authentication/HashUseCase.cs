using MediatR;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;
using Tuuuur.Core.Requests.Tools;
using Tuuuur.Core.Responses;

namespace Tuuuur.Core.UseCases.Authentication;

internal class HashUseCase(ILogger<HashUseCase> p_Logger) : IRequestHandler<HashRequest, StringResponse>
{
    [SuppressMessage("Style", "IDE1006:Styles d'affectation de noms", Justification = "Inherited named")]
    public Task<StringResponse> Handle(HashRequest request, CancellationToken cancellationToken)
    {
        try
        {
            byte[] v_Message = Encoding.UTF8.GetBytes(request.Value);
            using SHA512 v_Alg = SHA512.Create();
            StringBuilder v_Hex = new();

            foreach (byte v_X in v_Alg.ComputeHash(v_Message))
            {
                v_Hex.AppendFormat("{0:x2}", v_X);
            }

            return Task.FromResult(new StringResponse(v_Hex.ToString()));
        }
        catch (Exception v_Ex)
        {
            p_Logger.LogError(v_Ex, "An error was thrown");
            return Task.FromResult(new StringResponse([v_Ex.ToError()]));
        }
    }
}