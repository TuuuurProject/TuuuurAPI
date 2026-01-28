using AutoMapper;
using System.Linq;
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
        CreateMap<RefreshTokenRtk, RefreshToken>()
            .ReverseMap()
            .ForMember(p_Trg => p_Trg.User, p_Opt => p_Opt.Ignore());
        CreateMap<ThemeThm, Theme>().ReverseMap();
        CreateMap<DifficultyDft, Difficulty>().ReverseMap();
        CreateMap<QuestionQst, Question>()
            .ForMember(p_Trg => p_Trg.Label, p_Opt => p_Opt.MapFrom(p_Src => p_Src.Question))
            .ForMember(p_Trg => p_Trg.Answers, p_Opt => p_Opt.MapFrom(p_Src => p_Src.AnswerAns))
            .ForMember(p_Trg => p_Trg.Difficulty, p_Opt => p_Opt.MapFrom(p_Src => p_Src.IdDifficultyNavigation))
            .ForMember(p_Trg => p_Trg.Themes, p_Opt => p_Opt.MapFrom(p_Src => p_Src.QuestionThemeQth.Select(p_P => p_P.IdThemeNavigation)))

            .ForMember(p_Trg => p_Trg.Index, p_Opt => p_Opt.Ignore())

            .ForMember(p_Trg => p_Trg.Ticks, p_Opt => p_Opt.MapFrom(p_Src =>
                (int)p_Src.PartyQuestionPqt
                    .SelectMany(p_Pqt => p_Pqt.UserPartyQuestionUpq ?? Enumerable.Empty<UserPartyQuestionUpq>())
                    .Where(p_Upq => p_Upq.DtAnsweredAt.HasValue && p_Upq.DtPresentedAt.HasValue)
                    .Select(p_Upq => p_Upq.DtAnsweredAt.Value.Ticks - p_Upq.DtPresentedAt.Value.Ticks)
                    .FirstOrDefault()))
            .ForMember(p_Trg => p_Trg.Index, p_Opt =>
            {
                p_Opt.PreCondition(p_Src => p_Src.PartyQuestionPqt != null);
                p_Opt.MapFrom(p_Src => p_Src.PartyQuestionPqt.Select(p_Pqt => p_Pqt.Order).FirstOrDefault());
            })
            .ForMember(p_Trg => p_Trg.Score, p_Opt =>
            {
                p_Opt.PreCondition(p_Src => p_Src.PartyQuestionPqt != null && p_Src.PartyQuestionPqt.SelectMany(p_Pqt => p_Pqt.UserPartyQuestionUpq ?? Enumerable.Empty<UserPartyQuestionUpq>()).Any());
                p_Opt.MapFrom(p_Src => p_Src.PartyQuestionPqt.SelectMany(p_Pqt => p_Pqt.UserPartyQuestionUpq ?? Enumerable.Empty<UserPartyQuestionUpq>()).Select(p_Upq => p_Upq.Score).FirstOrDefault());
            })
            .ForMember(p_Trg => p_Trg.IdUserAnswer, p_Opt =>
            {
                p_Opt.PreCondition(p_Src => p_Src.PartyQuestionPqt != null && p_Src.PartyQuestionPqt.SelectMany(p_Pqt => p_Pqt.UserPartyQuestionUpq ?? Enumerable.Empty<UserPartyQuestionUpq>()).Any());
                p_Opt.MapFrom(p_Src => p_Src.PartyQuestionPqt.SelectMany(p_Pqt => p_Pqt.UserPartyQuestionUpq ?? Enumerable.Empty<UserPartyQuestionUpq>()).Select(p_Upq => p_Upq.IdAnswer).FirstOrDefault());
            })
            .ForMember(p_Trg => p_Trg.Correct, p_Opt =>
            {
                p_Opt.PreCondition(p_Src => p_Src.PartyQuestionPqt != null && p_Src.PartyQuestionPqt.SelectMany(p_Pqt => p_Pqt.UserPartyQuestionUpq ?? Enumerable.Empty<UserPartyQuestionUpq>()).Any());
                p_Opt.MapFrom(p_Src => p_Src.PartyQuestionPqt.SelectMany(p_Pqt => p_Pqt.UserPartyQuestionUpq ?? Enumerable.Empty<UserPartyQuestionUpq>()).Select(p_Upq => p_Upq.Correct).FirstOrDefault());
            })
            .ForMember(p_Trg => p_Trg.DtPresentedAt, p_Opt =>
            {
                p_Opt.PreCondition(p_Src => p_Src.PartyQuestionPqt != null && p_Src.PartyQuestionPqt.SelectMany(p_Pqt => p_Pqt.UserPartyQuestionUpq ?? Enumerable.Empty<UserPartyQuestionUpq>()).Any());
                p_Opt.MapFrom(p_Src => p_Src.PartyQuestionPqt.SelectMany(p_Pqt => p_Pqt.UserPartyQuestionUpq ?? Enumerable.Empty<UserPartyQuestionUpq>()).Select(p_Upq => p_Upq.DtPresentedAt).FirstOrDefault());
            })
            .ForMember(p_Trg => p_Trg.DtAnsweredAt, p_Opt =>
            {
                p_Opt.PreCondition(p_Src => p_Src.PartyQuestionPqt != null && p_Src.PartyQuestionPqt.SelectMany(p_Pqt => p_Pqt.UserPartyQuestionUpq ?? Enumerable.Empty<UserPartyQuestionUpq>()).Any());
                p_Opt.MapFrom(p_Src => p_Src.PartyQuestionPqt.SelectMany(p_Pqt => p_Pqt.UserPartyQuestionUpq ?? Enumerable.Empty<UserPartyQuestionUpq>()).Select(p_Upq => p_Upq.DtAnsweredAt).FirstOrDefault());
            })
            .ForMember(p_Trg => p_Trg.AnswerSeed, p_Opt =>
            {
                p_Opt.PreCondition(p_Src => p_Src.PartyQuestionPqt != null && p_Src.PartyQuestionPqt.SelectMany(p_Pqt => p_Pqt.UserPartyQuestionUpq ?? Enumerable.Empty<UserPartyQuestionUpq>()).Any());
                p_Opt.MapFrom(p_Src => p_Src.PartyQuestionPqt.SelectMany(p_Pqt => p_Pqt.UserPartyQuestionUpq ?? Enumerable.Empty<UserPartyQuestionUpq>()).Select(p_Upq => p_Upq.AnswersOrder).FirstOrDefault());
            })
            .ReverseMap();

        CreateMap<PartyBase, PartyPty>()
            .ForMember(p_Trg => p_Trg.Id, p_Opt => p_Opt.MapFrom(p_Src => p_Src.Id))
            .ForMember(p_Trg => p_Trg.Dt, p_Opt => p_Opt.MapFrom(p_Src => p_Src.Dt))
            .ForMember(p_Trg => p_Trg.IdPartyType, p_Opt => p_Opt.MapFrom(p_Src => p_Src.IdPartyType))
            .ForMember(p_Trg => p_Trg.IdUserHost, p_Opt => p_Opt.MapFrom(p_Src => (int?)p_Src.IdUserHost))
            .ForMember(p_Trg => p_Trg.Finish, p_Opt => p_Opt.MapFrom(p_Src => p_Src.Finish))
            .ForMember(p_Trg => p_Trg.IdPartyTypeNavigation, p_Opt => p_Opt.MapFrom(p_Src => p_Src.PartyType))
            .ForMember(p_Trg => p_Trg.IdUserHostNavigation, p_Opt => p_Opt.MapFrom(p_PartyBase => p_PartyBase.UserHost))
            .ForMember(p_Trg => p_Trg.PartyDifficultyPdf, p_Opt => p_Opt.MapFrom(p_Src => p_Src.Difficulties ?? new List<Difficulty>())
            )
            .ForMember(p_Trg => p_Trg.PartyQuestionPqt, p_Opt => p_Opt.MapFrom(p_Src => p_Src.Questions ?? new List<Question>())
            )
            .ForMember(p_Trg => p_Trg.PartyThemePth, p_Opt => p_Opt.MapFrom(p_Src => p_Src.Themes ?? new List<Theme>())
            )
            .ForMember(p_Trg => p_Trg.PartyUserPus, p_Opt => p_Opt.MapFrom(p_Src => p_Src.Users ?? new List<User>())
            );


        CreateMap<PartyPty, PartyBase>()
            .ForMember(p_Trg => p_Trg.Id, p_Opt => p_Opt.MapFrom(p_Src => p_Src.Id))
            .ForMember(p_Trg => p_Trg.Dt, p_Opt => p_Opt.MapFrom(p_Src => p_Src.Dt))
            .ForMember(p_Trg => p_Trg.IdPartyType, p_Opt => p_Opt.MapFrom(p_Src => p_Src.IdPartyType))
            .ForMember(p_Trg => p_Trg.PartyType, p_Opt => p_Opt.MapFrom(p_Src => p_Src.IdPartyTypeNavigation))
            .ForMember(p_Trg => p_Trg.IdUserHost, p_Opt => p_Opt.MapFrom(p_Src => p_Src.IdUserHost))
            .ForMember(p_Trg => p_Trg.UserHost, p_Opt => p_Opt.MapFrom(p_Src => p_Src.IdUserHostNavigation))
            .ForMember(p_Trg => p_Trg.Finish, p_Opt => p_Opt.MapFrom(p_Src => p_Src.Finish))
            .ForMember(p_Trg => p_Trg.Questions,
                p_Opt => p_Opt.MapFrom(p_Src => p_Src.PartyQuestionPqt.Select(p_P => p_P.IdQuestionNavigation)))
            .ForMember(p_Trg => p_Trg.Users,
                p_Opt => p_Opt.MapFrom(p_Src => p_Src.PartyUserPus.Select(p_P => p_P.IdUserNavigation)))
            .ForMember(p_Trg => p_Trg.Difficulties,
                p_Opt => p_Opt.MapFrom(p_Src => p_Src.PartyDifficultyPdf.Select(p_P => p_P.IdDifficultyNavigation)))
            .ForMember(p_Trg => p_Trg.Themes,
                p_Opt => p_Opt.MapFrom(p_Src => p_Src.PartyThemePth.Select(p_P => p_P.IdThemeNavigation)))
            .ForMember(p_Trg => p_Trg.NbQuestions, p_Opt => p_Opt.MapFrom(p_P => p_P.PartyQuestionPqt.Count))
            .ForMember(p_Trg => p_Trg.Time, p_Opt => p_Opt.Ignore())
            .AfterMap((p_Src, p_Trg) =>
            {
                int v_Count = p_Src.PartyQuestionPqt.Count;
                if (p_Trg.Questions == null || p_Trg.Questions.Count == 0 || p_Trg.Questions.Contains(null))
                {
                    p_Trg.Percent = 0;
                    return;
                }

                int v_CorrectAnswersCount = p_Trg.Questions.Count(p_Question => p_Question.Correct);

                p_Trg.Percent = (double)v_CorrectAnswersCount * 100 / v_Count;
            });

        CreateMap<Question, PartyQuestionPqt>()
            .ForMember(d => d.IdQuestion, o => o.MapFrom(s => s.Id))
            .ForMember(d => d.Order, o => o.MapFrom(s => s.Index))
            .ForMember(d => d.IdParty, o => o.Ignore())
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.IdPartyNavigation, o => o.Ignore())
            .ForMember(d => d.IdQuestionNavigation, o => o.Ignore())
            .ForMember(d => d.UserPartyQuestionUpq, o => o.Ignore());

        CreateMap<Difficulty, PartyDifficultyPdf>()
            .ForMember(d => d.IdDifficulty, o => o.MapFrom(s => s.Id))
            .ForMember(d => d.IdParty, o => o.Ignore())
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.IdDifficultyNavigation, o => o.Ignore())
            .ForMember(d => d.IdPartyNavigation, o => o.Ignore());

        CreateMap<Theme, PartyThemePth>()
            .ForMember(d => d.IdTheme, o => o.MapFrom(s => s.Id))
            .ForMember(d => d.IdParty, o => o.Ignore())
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.IdPartyNavigation, o => o.Ignore())
            .ForMember(d => d.IdThemeNavigation, o => o.Ignore());

        CreateMap<User, PartyUserPus>()
            .ForMember(d => d.IdUser, o => o.MapFrom(s => s.Id))
            .ForMember(d => d.IdParty, o => o.Ignore())
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.IdPartyNavigation, o => o.Ignore())
            .ForMember(d => d.IdUserNavigation, o => o.Ignore());

        CreateMap<PartyPty, GroupParty>()
            .IncludeBase<PartyPty, PartyBase>()
            .ForMember(p_Trg => p_Trg.NbQuestions, p_Opt => p_Opt.Ignore())
            .ForMember(p_Trg => p_Trg.InProgress, p_Opt => p_Opt.Ignore())
            .ForMember(p_Trg => p_Trg.Code, p_Opt => p_Opt.Ignore())
            .ForMember(p_Trg => p_Trg.ScoreEachRound, p_Opt => p_Opt.Ignore())
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
            .ForMember(p_Trg => p_Trg.Question, p_Opt => p_Opt.MapFrom(p_Src => p_Src.IdQuestionNavigation))
            .ForMember(p_Trg => p_Trg.Theme, p_Opt => p_Opt.MapFrom(p_Src => p_Src.IdThemeNavigation))
            .ReverseMap();

        CreateMap<PartyThemePth, PartyTheme>()
            .ForMember(p_Trg => p_Trg.Theme, p_Opt => p_Opt.MapFrom(p_Src => p_Src.IdThemeNavigation))
            .ReverseMap();

        CreateMap<PartyDifficultyPdf, PartyDifficulty>()
            .ForMember(p_Trg => p_Trg.Difficulty, p_Opt => p_Opt.MapFrom(p_Src => p_Src.IdDifficultyNavigation))
            .ReverseMap();
    }
}