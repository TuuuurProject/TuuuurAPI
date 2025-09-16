using AutoMapper;
using Microsoft.AspNetCore.Identity.Data;
using Tuuuur.Domain.Bo;

namespace Tuuuur.API.Requests.Mapping
{
    /// <summary>
    /// Mapper profile for requests to bo objects
    /// </summary>
    public class BodyRequestProfile : Profile
    {
        /// <summary>
        /// ctor containing mapping definition
        /// </summary>
        public BodyRequestProfile()
        {
            CreateMap<RegisterRequest, User>()
                .ForMember(p_Trg => p_Trg.Id, p_Opt => p_Opt.Ignore())
                .ForMember(p_Trg => p_Trg.Avatar, p_Opt => p_Opt.Ignore())
                .ForMember(p_Trg => p_Trg.ResetPasswordCode, p_Opt => p_Opt.Ignore())
                .ForMember(p_Trg => p_Trg.IsNew, p_Opt => p_Opt.Ignore())
                .ForMember(p_Trg => p_Trg.IsAdmin, p_Opt => p_Opt.Ignore())
                .ForMember(p_Trg => p_Trg.UserAuth, p_Opt => p_Opt.Ignore())
                .ReverseMap();
        }
    }
}