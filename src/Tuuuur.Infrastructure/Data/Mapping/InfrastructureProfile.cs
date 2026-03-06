using AutoMapper;
using Tuuuur.Domain.Bo;
using Tuuuur.Infrastructure.Data.EntityFramework.Entities;
using Tuuuur.Infrastructure.Data.Mapping.Converters;

namespace Tuuuur.Infrastructure.Data.Mapping;

internal class InfrastructureProfile : Profile
{
    public InfrastructureProfile()
    {
        CreateMap<UserUsr, User>()
            .ForMember(p_Trg => p_Trg.UserAuth, p_Opt => p_Opt.MapFrom(p_Src => p_Src.UserAuthUat))
            .ForMember(p_Trg => p_Trg.IsInvitedUser, p_Opt => p_Opt.MapFrom(p_Src => false))
            .ForMember(p_Trg => p_Trg.Elo, p_Opt => p_Opt.MapFrom(p_Src => p_Src.EloElo))
            .ReverseMap();
        CreateMap<UserAuthUat, UserAuth>()
            .ReverseMap()
            .ForMember(p_Trg => p_Trg.User, p_Opt => p_Opt.Ignore());
        CreateMap<RefreshTokenRtk, RefreshToken>()
            .ReverseMap()
            .ForMember(p_Trg => p_Trg.User, p_Opt => p_Opt.Ignore());
        CreateMap<ThemeThm, Theme>().ReverseMap();
        CreateMap<DifficultyDft, Difficulty>().ReverseMap();
        CreateMap<QuestionQst, Question>()
            .ForMember(p_Trg => p_Trg.Label, p_Opt => p_Opt.MapFrom(p_Src => p_Src.Question))
            .ForMember(p_Trg => p_Trg.IdDifficulty, p_Opt => p_Opt.MapFrom(p_Src => p_Src.IdDifficulty))
            .ForMember(p_Trg => p_Trg.Answer, p_Opt => p_Opt.MapFrom(p_Src => p_Src.AnswerAns))
            .ForMember(p_Trg => p_Trg.Difficulty, p_Opt => p_Opt.MapFrom(p_Src => p_Src.IdDifficultyNavigation))
            .ForMember(p_Trg => p_Trg.PartyQuestion, p_Opt => p_Opt.MapFrom(p_Src => p_Src.PartyQuestionPqt))
            .ForMember(p_Trg => p_Trg.QuestionTheme, p_Opt => p_Opt.MapFrom(p_Src => p_Src.QuestionThemeQth))
            .ReverseMap();

        CreateMap<PartyPty, PartyBase>()
            .ForMember(p_Trg => p_Trg.Id, p_Opt => p_Opt.MapFrom(p_Src => p_Src.Id))
            .ForMember(p_Trg => p_Trg.Dt, p_Opt => p_Opt.MapFrom(p_Src => p_Src.Dt))
            .ForMember(p_Trg => p_Trg.IdPartyType, p_Opt => p_Opt.MapFrom(p_Src => p_Src.IdPartyType))
            .ForMember(p_Trg => p_Trg.PartyType, p_Opt => p_Opt.MapFrom(p_Src => p_Src.IdPartyTypeNavigation))
            .ForMember(p_Trg => p_Trg.IdUserHost, p_Opt => p_Opt.MapFrom(p_Src => p_Src.IdUserHost))
            .ForMember(p_Trg => p_Trg.Active, p_Opt => p_Opt.MapFrom(p_Src => p_Src.Active))
            .ForMember(p_Trg => p_Trg.Finish, p_Opt => p_Opt.MapFrom(p_Src => p_Src.Finish))
            .ForMember(p_Trg => p_Trg.User, p_Opt => p_Opt.MapFrom(p_Src => p_Src.IdUserHostNavigation))
            .ForMember(p_Trg => p_Trg.PartyDifficulty, p_Opt => p_Opt.MapFrom(p_Src => p_Src.PartyDifficultyPdf))
            .ForMember(p_Trg => p_Trg.PartyTheme, p_Opt => p_Opt.MapFrom(p_Src => p_Src.PartyThemePth))
            .ForMember(p_Trg => p_Trg.PartyQuestions, p_Opt => p_Opt.MapFrom(p_Src => p_Src.PartyQuestionPqt))
            .ForMember(p_Trg => p_Trg.Percent, p_Opt => p_Opt.Ignore())
            .ForMember(p_Trg => p_Trg.Time, p_Opt => p_Opt.Ignore())
            .ReverseMap();

        CreateMap<PartyPty, History>()
            .IncludeBase<PartyPty, PartyBase>()
            .ForMember(p_Trg => p_Trg.PartyQuestions, p_Opt => p_Opt.MapFrom(p_Src => p_Src.PartyQuestionPqt));

        CreateMap<PartyPty, Party>()
            .IncludeBase<PartyPty, PartyBase>()
            .ForMember(p_Trg => p_Trg.PartyUsers, p_Opt => p_Opt.MapFrom(p_Src => p_Src.PartyUserPus))
            .ForMember(p_Trg => p_Trg.NbQuestions, p_Opt => p_Opt.Ignore())
            .ForMember(p_Trg => p_Trg.InProgress, p_Opt => p_Opt.Ignore())
            .ReverseMap();

        CreateMap<PartyPty, GroupParty>()
            .IncludeBase<PartyPty, PartyBase>()
            .ForMember(p_Trg => p_Trg.PartyUsers, p_Opt => p_Opt.MapFrom(p_Src => p_Src.PartyUserPus))
            .ForMember(p_Trg => p_Trg.NbQuestions, p_Opt => p_Opt.MapFrom(p_Src => p_Src.PartyQuestionPqt.Count))
            .ForMember(p_Trg => p_Trg.PartyUsers, p_Opt => p_Opt.MapFrom(p_Src => p_Src.PartyUserPus))
            .ForMember(p_Trg => p_Trg.UserScores, p_Opt => p_Opt.MapFrom(p_Src => p_Src.PartyQuestionPqt.SelectMany(p_PartyQuestionPqt => p_PartyQuestionPqt.UserPartyQuestionUpq)))
            .ForMember(p_Trg => p_Trg.InProgress, p_Opt => p_Opt.Ignore())
            .ForMember(p_Trg => p_Trg.Code, p_Opt => p_Opt.Ignore())
            .ForMember(p_Trg => p_Trg.ScoreEachRound, p_Opt => p_Opt.Ignore())
            .AfterMap((p_Src, p_Dest, p_Ctx) =>
            {
                if (p_Dest == null)
                    return;
                
                if (p_Dest?.UserScores != null)
                {
                    p_Dest.UserScores = p_Dest.UserScores
                        .Where(p_UserScore => p_UserScore?.User != null)
                        .GroupBy(p_UserScore => p_UserScore.User.Id)
                        .Select(p_Grouping => new UserScore
                        {
                            Score = p_Grouping.Sum(p_UserScore => p_UserScore.Score),
                            User = p_Grouping.First().User
                        })
                        .OrderByDescending(p_UserScore => p_UserScore.Score)
                        .ToList();
                }

                try
                {
                    bool v_HasUserId = p_Ctx.Items != null && p_Ctx.Items.ContainsKey($"{nameof(User)}.{nameof(User.Id)}");
                    Guid? v_CtxItem = (Guid?)p_Ctx.Items?[$"{nameof(User)}.{nameof(User.Id)}"];

                    if (!v_CtxItem.HasValue)
                    {
                        return;
                    }

                    foreach (PartyQuestionPqt v_PartyQuestionPqt in p_Src.PartyQuestionPqt)
                    {
                        if (v_HasUserId)
                        {
                            v_PartyQuestionPqt.UserPartyQuestionUpq = v_PartyQuestionPqt.UserPartyQuestionUpq.Where(p_P => p_P.IdUser == v_CtxItem).ToList();
                        }
                    }
                
                    p_Dest.PartyQuestions = p_Ctx.Mapper.Map<List<PartyQuestion>>(p_Src.PartyQuestionPqt);
                }
                catch (Exception v_Exception)
                {
                    // ignored
                }
            })
            .ReverseMap();

        CreateMap<UserPartyQuestionUpq, UserScore>().ConvertUsing<UserPartyQuestionToUserScoreConverter>();

        CreateMap<PartyQuestionPqt, PartyQuestion>()
            .ForMember(p_Trg => p_Trg.Id, p_Opt => p_Opt.MapFrom(p_Src => p_Src.Id))
            .ForMember(p_Trg => p_Trg.IdQuestion, p_Opt => p_Opt.MapFrom(p_Src => p_Src.IdQuestion))
            .ForMember(p_Trg => p_Trg.Order, p_Opt => p_Opt.MapFrom(p_Src => p_Src.Order))
            .ForMember(p_Trg => p_Trg.Question, p_Opt => p_Opt.MapFrom(p_Src => p_Src.IdQuestionNavigation))
            .ForMember(p_Trg => p_Trg.UserPartyQuestion, p_Opt => p_Opt.MapFrom(p_Src => p_Src.UserPartyQuestionUpq.FirstOrDefault()))
            .ReverseMap();

        CreateMap<PartyUserPus, PartyUser>()
            .ForMember(p_Trg => p_Trg.Id, p_Opt => p_Opt.MapFrom(p_Src => p_Src.Id))
            .ForMember(p_Trg => p_Trg.IdUser, p_Opt => p_Opt.MapFrom(p_Src => p_Src.IdUser))
            .ForMember(p_Trg => p_Trg.IdParty, p_Opt => p_Opt.MapFrom(p_Src => p_Src.IdParty))
            .ForMember(p_Trg => p_Trg.User, p_Opt => p_Opt.MapFrom(p_Src => p_Src.IdUserNavigation))
            .ReverseMap();


        CreateMap<AnswerAns, Answer>()
            .ForMember(p_Trg => p_Trg.IdQuestion, p_Opt => p_Opt.MapFrom(p_Src => p_Src.IdQuestion))
            .ReverseMap();

        CreateMap<PartyTypePty, PartyType>()
            .ReverseMap();

        CreateMap<UserPartyQuestionUpq, UserPartyQuestion>()
            .ForMember(p_Trg => p_Trg.PartyQuestion, p_Opt => p_Opt.MapFrom(p_Src => p_Src.IdPartyQuestionNavigation))
            .ForMember(p_Trg => p_Trg.User, p_Opt => p_Opt.MapFrom(p_Src => p_Src.IdUserNavigation))
            .ForMember(p_Trg => p_Trg.Answer, p_Opt => p_Opt.MapFrom(p_Src => p_Src.IdAnswerNavigation))
            .ReverseMap();

        CreateMap<QuestionThemeQth, QuestionTheme>()
            .ForMember(p_Trg => p_Trg.Theme, p_Opt => p_Opt.MapFrom(p_Src => p_Src.IdThemeNavigation))
            .ReverseMap();

        CreateMap<PartyThemePth, PartyTheme>()
            .ForMember(p_Trg => p_Trg.Theme, p_Opt => p_Opt.MapFrom(p_Src => p_Src.IdThemeNavigation))
            .ReverseMap()
            .ForMember(p_Trg => p_Trg.IdThemeNavigation, p_Opt => p_Opt.Ignore());

        CreateMap<PartyDifficultyPdf, PartyDifficulty>()
            .ForMember(p_Trg => p_Trg.Difficulty, p_Opt => p_Opt.MapFrom(p_Src => p_Src.IdDifficultyNavigation))
            .ReverseMap()
            .ForMember(p_Trg => p_Trg.IdDifficultyNavigation, p_Opt => p_Opt.Ignore());
        
        CreateMap<EloElo, Elo>()
            .ForMember(p_Trg => p_Trg.Value, p_Opt => p_Opt.MapFrom(p_Src => p_Src.Value))
            .ForMember(p_Trg => p_Trg.IdTheme, p_Opt => p_Opt.MapFrom(p_Src => p_Src.IdTheme))
            .ForMember(p_Trg => p_Trg.Theme, p_Opt => p_Opt.MapFrom(p_Src => p_Src.IdThemeNavigation))
            .ReverseMap();
        
    }
}