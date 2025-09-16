using AutoMapper;
using Tuuuur.Domain.Bo;
using Tuuuur.Infrastructure.Data.EntityFramework.Entities;

namespace Tuuuur.Infrastructure.Data.Mapping;

internal class InfrastructureProfile : Profile
{
    public InfrastructureProfile()
    {
        CreateMap<User_USR, User>()
            .ForMember(p_Trg => p_Trg.UserAuth, p_Opt => p_Opt.MapFrom(p_Src => p_Src.UserAuth_UAT))
            .ReverseMap();
        CreateMap<UserAuth_UAT, UserAuth>()
            .ReverseMap()
            .ForMember(p_Trg => p_Trg.User, p_Opt => p_Opt.Ignore());
    }
}