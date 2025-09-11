using Tuuuur.Domain.Bo;
using Tuuuur.Domain.Token;

namespace Tuuuur.Domain.Interfaces.Token;

public interface IJwtFactory
{
    JwtTokenResponse CreateToken(User p_UserInfos);
}