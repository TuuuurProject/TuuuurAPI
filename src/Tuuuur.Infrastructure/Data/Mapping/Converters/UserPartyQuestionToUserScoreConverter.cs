using AutoMapper;
using Tuuuur.Domain.Bo;
using Tuuuur.Infrastructure.Data.EntityFramework.Entities;

namespace Tuuuur.Infrastructure.Data.Mapping.Converters;

public class UserPartyQuestionToUserScoreConverter : ITypeConverter<UserPartyQuestionUpq, UserScore>
{
    [SuppressMessage("Style", "IDE1006:Styles d'affectation de noms", Justification = "Inherited named")]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public UserScore Convert(UserPartyQuestionUpq source, UserScore destination, ResolutionContext context)
    {
        User v_User;
        if (source.IdUser.HasValue)
        {
            v_User = context.Mapper.Map<User>(source.IdUserNavigation);
        }
        else
        {
            v_User = new User
            {
                Id = source.IdGuest ?? Guid.Empty,
                NickName = source.GuestNickname,
                IsInvitedUser = true
            };
        }
        UserScore v_UserScore = new()
        {
            User = v_User,
            Score = source.Score ?? 0
        };
        return v_UserScore;
    }
}
