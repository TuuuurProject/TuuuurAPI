using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;
using Tuuuur.Core.Requests.Tools;
using Tuuuur.Core.Responses;
using Tuuuur.Domain.Interfaces.Data;

namespace Tuuuur.Core.UseCases.Authentication;

internal class HashUseCase(
    ILogger<HashUseCase> p_Logger,
    IUnitOfWork p_UnitOfWork
    ) : ADbUseCase<HashRequest, StringResponse>(p_Logger, p_UnitOfWork)
{
    protected override Task<StringResponse> HandleLogic(HashRequest p_Request, CancellationToken p_CancellationToken)
    {
        byte[] v_Message = Encoding.UTF8.GetBytes(p_Request.Value);
        StringBuilder v_Hex = new();

        foreach (byte v_X in SHA512.HashData(v_Message))
        {
            v_Hex.Append($"{v_X:x2}");
        }

        return Task.FromResult(new StringResponse(v_Hex.ToString()));
    }
}