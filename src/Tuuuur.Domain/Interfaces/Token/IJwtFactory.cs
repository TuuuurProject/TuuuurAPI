using Tuuuur.Domain.Bo;
using Tuuuur.Domain.Interfaces.Data;
using Tuuuur.Domain.Token;

namespace Tuuuur.Domain.Interfaces.Token;

public interface IJwtFactory
{
    Task<JwtTokenResponse> CreateTokenAsync(User p_UserInfos, IUnitOfWork p_UnitOfWork, CancellationToken p_CancellationToken = default);
    JwtTokenResponse CreateAnonymousTokenAsync(User p_User);
    Guid? GetUserIdFromToken(string p_Token);
}