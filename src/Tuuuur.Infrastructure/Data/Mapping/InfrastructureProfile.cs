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
        CreateMap<QuestionQst, Question>()
            .ForMember(p_Trg => p_Trg.Label, p_Opt => p_Opt.MapFrom(p_Src => p_Src.Question))
            .ForMember(p_Trg => p_Trg.IdDifficulty, p_Opt => p_Opt.MapFrom(p_Src => p_Src.IdDifficulty))
            .ForMember(p_Trg => p_Trg.Answer, p_Opt => p_Opt.MapFrom(p_Src => p_Src.AnswerAns))
            .ForMember(p_Trg => p_Trg.Difficulty, p_Opt => p_Opt.MapFrom(p_Src => p_Src.IdDifficultyNavigation))
            .ForMember(p_Trg => p_Trg.PartyQuestion, p_Opt => p_Opt.MapFrom(p_Src => p_Src.PartyQuestionPqt))
            .ForMember(p_Trg => p_Trg.QuestionTheme, p_Opt => p_Opt.MapFrom(p_Src => p_Src.QuestionThemeQth))
            .ReverseMap();

        CreateMap<PartyPty, Party>()
            .ForMember(p_Trg => p_Trg.Id, p_Opt => p_Opt.MapFrom(p_Src => p_Src.Id))
            .ForMember(p_Trg => p_Trg.Dt, p_Opt => p_Opt.MapFrom(p_Src => p_Src.Dt))
            .ForMember(p_Trg => p_Trg.Code, p_Opt => p_Opt.MapFrom(p_Src => p_Src.Code))
            .ForMember(p_Trg => p_Trg.IdPartyType, p_Opt => p_Opt.MapFrom(p_Src => p_Src.IdPartyType))
            .ForMember(p_Trg => p_Trg.PartyType, p_Opt => p_Opt.MapFrom(p_Src => p_Src.IdPartyTypeNavigation))
            .ForMember(p_Trg => p_Trg.IdUserHost, p_Opt => p_Opt.MapFrom(p_Src => p_Src.IdUserHost))
            .ForMember(p_Trg => p_Trg.Active, p_Opt => p_Opt.MapFrom(p_Src => p_Src.Active))
            .ForMember(p_Trg => p_Trg.Finish, p_Opt => p_Opt.MapFrom(p_Src => p_Src.Finish))
            .ForMember(p_Trg => p_Trg.PartyQuestions, p_Opt => p_Opt.MapFrom(p_Src => p_Src.PartyQuestionPqt))
            .ForMember(p_Trg => p_Trg.PartyUsers, p_Opt => p_Opt.MapFrom(p_Src => p_Src.PartyUserPus))
            .ForMember(p_Trg => p_Trg.User, p_Opt => p_Opt.MapFrom(p_Src => p_Src.IdUserHostNavigation))
            .ForMember(p_Trg => p_Trg.Score, p_Opt => p_Opt.Ignore())
            .ReverseMap();

        CreateMap<PartyQuestionPqt, PartyQuestion>()
            .ForMember(p_Trg => p_Trg.Id, p_Opt => p_Opt.MapFrom(p_Src => p_Src.Id))
            .ForMember(p_Trg => p_Trg.IdQuestion, p_Opt => p_Opt.MapFrom(p_Src => p_Src.IdQuestion))
            .ForMember(p_Trg => p_Trg.IdParty, p_Opt => p_Opt.MapFrom(p_Src => p_Src.IdParty))
            .ForMember(p_Trg => p_Trg.Order, p_Opt => p_Opt.MapFrom(p_Src => p_Src.Order))
            .ForMember(p_Trg => p_Trg.Party, p_Opt => p_Opt.Ignore())
            .ForMember(p_Trg => p_Trg.Question, p_Opt => p_Opt.MapFrom(p_Src => p_Src.IdQuestionNavigation))
            .ForMember(p_Trg => p_Trg.UserPartyQuestion, p_Opt => p_Opt.MapFrom(p_Src => p_Src.UserPartyQuestionUpq.FirstOrDefault()))
            .ReverseMap();
        
        CreateMap<PartyUserPus, PartyUser>()
            .ForMember(p_Trg => p_Trg.Id, p_Opt => p_Opt.MapFrom(p_Src => p_Src.Id))
            .ForMember(p_Trg => p_Trg.IdUser, p_Opt => p_Opt.MapFrom(p_Src => p_Src.IdUser))
            .ForMember(p_Trg => p_Trg.IdParty, p_Opt => p_Opt.MapFrom(p_Src => p_Src.IdParty))
            .ForMember(p_Trg => p_Trg.User, p_Opt => p_Opt.MapFrom(p_Src => p_Src.IdUserNavigation))
            .ForMember(p_Trg => p_Trg.Party, p_Opt => p_Opt.MapFrom(p_Src => p_Src.IdPartyNavigation))
            .ReverseMap();
        
        
        CreateMap<AnswerAns, Answer>()
            .ForMember(p_Trg => p_Trg.Question, p_Opt => p_Opt.MapFrom(p_Src => p_Src.IdQuestionNavigation))
            .ForMember(p_Trg => p_Trg.IdQuestion, p_Opt => p_Opt.MapFrom(p_Src => p_Src.IdQuestion))
            .ReverseMap();
        
        CreateMap<PartyTypePty, PartyType>()
            .ReverseMap();
        
        CreateMap<UserPartyQuestionUpq, UserPartyQuestion>()
            .ForMember(p_Trg => p_Trg.PartyQuestion, p_Opt => p_Opt.MapFrom(p_Src => p_Src.IdPartyQuestionNavigation))
            .ForMember(p_Trg => p_Trg.User, p_Opt => p_Opt.MapFrom(p_Src => p_Src.IdUserNavigation))
            .ForMember(p_Trg => p_Trg.Anwser, p_Opt => p_Opt.MapFrom(p_Src => p_Src.IdAnwserNavigation))
            .ReverseMap();
        
        CreateMap<QuestionThemeQth, QuestionTheme>()
            .ForMember(p_Trg => p_Trg.Question, p_Opt => p_Opt.MapFrom(p_Src => p_Src.IdQuestionNavigation))
            .ForMember(p_Trg => p_Trg.Theme, p_Opt => p_Opt.MapFrom(p_Src => p_Src.IdThemeNavigation))
            .ReverseMap();
    }
}