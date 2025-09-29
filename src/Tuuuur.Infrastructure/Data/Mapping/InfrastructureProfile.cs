using AutoMapper;
using Tuuuur.Domain.Bo;
using Tuuuur.Infrastructure.Data.EntityFramework.Entities;

namespace Tuuuur.Infrastructure.Data.Mapping;

internal class InfrastructureProfile : Profile
{
    public InfrastructureProfile()
    {
        CreateMap<UserUsr, User>()
            .ForMember(p_Trg => p_Trg.UserAuth, p_Opt => p_Opt.MapFrom(p_Src => p_Src.UserAuthUat))
            .ReverseMap();
        CreateMap<UserAuthUat, UserAuth>()
            .ReverseMap()
            .ForMember(p_Trg => p_Trg.User, p_Opt => p_Opt.Ignore());

        CreateMap<ThemeThm, Theme>() .ReverseMap();
        CreateMap<DifficultyDft, Difficulty>() .ReverseMap();
    }
}