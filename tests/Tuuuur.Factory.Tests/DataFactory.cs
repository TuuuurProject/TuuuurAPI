using System.Diagnostics.CodeAnalysis;
using Bogus;
using Tuuuur.Domain.Bo;
using Tuuuur.Infrastructure.Data.EntityFramework.Entities;

namespace Tuuuur.Factory.Tests;

[ExcludeFromCodeCoverage]
public static class EfFactory
{
    public static Faker<UserUsr> CreateUser()
    {
        return new Faker<UserUsr>()
            .RuleFor(p_O => p_O.NickName, p_F => p_F.Person.FirstName)
            .RuleFor(p_O => p_O.Email, p_F => p_F.Person.Email)
            .RuleFor(p_O => p_O.Password, p_F => p_F.Internet.Password(10))
            .RuleFor(p_O => p_O.ResetPasswordCode, p_F => p_F.Random.Guid())
            .RuleFor(p_O => p_O.Avatar, p_F => p_F.Random.String())
            .RuleFor(p_O => p_O.IsNew, p_F => p_F.PickRandom(true, false))
            .RuleFor(p_O => p_O.IsAdmin, p_F => p_F.Random.Bool());
    }
    public static Faker<ThemeThm> CreateTheme()
    {
        return new Faker<ThemeThm>()
            .RuleFor(p_O => p_O.Icon, p_F => p_F.Random.Word())
            .RuleFor(p_O => p_O.Label, p_F => p_F.Random.Word());
    }
    public static Faker<DifficultyDft> CreateDifficulty()
    {
        return new Faker<DifficultyDft>()
            .RuleFor(p_O => p_O.Label, p_F => p_F.Random.Word());
    }
    public static Faker<UserAuthUat> CreateUserAuth(int v_UserId)
    {
        return new Faker<UserAuthUat>()
            .RuleFor(p_O => p_O.UserId, v_UserId)
            .RuleFor(p_O => p_O.ExpiresAt, p_F => p_F.Date.Future())
            .RuleFor(p_O => p_O.Code, p_F => p_F.Random.AlphaNumeric(6));
    }
}

[ExcludeFromCodeCoverage]
public static class BoFactory
{
    public static Faker<User> CreateUser()
    {
        return new Faker<User>()
            .RuleFor(p_O => p_O.NickName, p_F => p_F.Person.FirstName)
            .RuleFor(p_O => p_O.Email, (p_F, p_O) => p_F.Internet.Email())
            .RuleFor(p_O => p_O.Password, p_F => p_F.Internet.Password(10))
            .RuleFor(p_O => p_O.ResetPasswordCode, p_F => p_F.Random.Guid())
            .RuleFor(p_O => p_O.Avatar, p_F => p_F.Lorem.Paragraphs())
            .RuleFor(p_O => p_O.IsNew, p_F => p_F.PickRandom(true, false))
            .RuleFor(p_O => p_O.IsAdmin, p_F => p_F.PickRandom(true, false));
    }
    public static Faker<UserAuth> CreateUserAuth(int p_UserId)
    {
        return new Faker<UserAuth>()
            .RuleFor(p_O => p_O.UserId, p_UserId)
            .RuleFor(p_O => p_O.Code, p_F => p_F.Random.AlphaNumeric(6));
    }

    public static Faker<PartyBase> CreateParty()
    {
        return new Faker<PartyBase>()
            .RuleFor(p_Party => p_Party.Id, _ => Guid.NewGuid())
            .RuleFor(p_Party => p_Party.Dt, p_F => p_F.Date.Recent(30))
            .RuleFor(p_Party => p_Party.IdPartyType, p_F => p_F.Random.Int(1, 10))
            .RuleFor(p_Party => p_Party.IdUserHost, p_F => p_F.Random.Int(1, 1000))
            .RuleFor(p_Party => p_Party.Finish, p_F => p_F.Random.Bool())
            .RuleFor(p_Party => p_Party.NbQuestions, p_F => p_F.Random.Int(5, 20))
            .RuleFor(p_Party => p_Party.Percent, p_F => p_F.Random.Double(0, 100))
            .RuleFor(p_Party => p_Party.Users, _ => [])
            .RuleFor(p_Party => p_Party.Questions, _ => []);
    }

    public static Faker<GroupParty> CreateGroupParty()
    {
        return new Faker<GroupParty>()
            .RuleFor(p_Party => p_Party.Id, _ => Guid.NewGuid())
            .RuleFor(p_Party => p_Party.Dt, p_F => p_F.Date.Recent(30))
            .RuleFor(p_Party => p_Party.Code, p_F => p_F.Random.AlphaNumeric(6))
            .RuleFor(p_Party => p_Party.IdPartyType, p_F => p_F.Random.Int(1, 10))
            .RuleFor(p_Party => p_Party.IdUserHost, p_F => p_F.Random.Int(1, 1000))
            .RuleFor(p_Party => p_Party.InProgress, p_F => p_F.Random.Bool())
            .RuleFor(p_Party => p_Party.ScoreEachRound, p_F => p_F.Random.Bool())
            .RuleFor(p_Party => p_Party.Finish, p_F => p_F.Random.Bool())
            .RuleFor(p_Party => p_Party.NbQuestions, p_F => p_F.Random.Int(5, 20))
            .RuleFor(p_Party => p_Party.Percent, p_F => p_F.Random.Double(0, 100))
            .RuleFor(p_Party => p_Party.Users, _ => [])
            .RuleFor(p_Party => p_Party.Questions, _ => []);
    }

    public static Faker<Question> CreateQuestion()
    {
        return new Faker<Question>()
            .RuleFor(p_Question => p_Question.Id, p_F => p_F.Random.Int(1, 1000))
            .RuleFor(p_Question => p_Question.Label, p_F => p_F.Lorem.Sentence())
            .RuleFor(p_Question => p_Question.Answers, _ => [])
            .RuleFor(p_Question => p_Question.Difficulty, _ => null)
            .RuleFor(p_Question => p_Question.Themes, _ => []);
    }

    public static Faker<Answer> CreateAnswer(int? p_QuestionId = null)
    {
        return new Faker<Answer>()
            .RuleFor(p_Answer => p_Answer.Id, p_F => p_F.Random.Int(1, 10000))
            .RuleFor(p_Answer => p_Answer.IdQuestion, p_QuestionId ?? new Faker().Random.Int(1, 1000))
            .RuleFor(p_Answer => p_Answer.Value, p_F => p_F.Lorem.Word())
            .RuleFor(p_Answer => p_Answer.Valid, p_F => p_F.Random.Bool());
    }

    public static Faker<QuestionTheme> CreateQuestionTheme(int? p_QuestionId = null, int? p_ThemeId = null)
    {
        return new Faker<QuestionTheme>()
            .RuleFor(p_Qt => p_Qt.Id, p_F => p_F.Random.Int(1, 10000))
            .RuleFor(p_Qt => p_Qt.IdQuestion, p_QuestionId ?? new Faker().Random.Int(1, 1000))
            .RuleFor(p_Qt => p_Qt.IdTheme, p_ThemeId ?? new Faker().Random.Int(1, 100))
            .RuleFor(p_Qt => p_Qt.Question, _ => null)
            .RuleFor(p_Qt => p_Qt.Theme, _ => null);
    }

    public static Faker<PartyQuestion> CreatePartyQuestion(int? p_QuestionId = null, Guid? p_PartyId = null)
    {
        return new Faker<PartyQuestion>()
            .RuleFor(p_Pq => p_Pq.Id, p_F => p_F.Random.Int(1, 10000))
            .RuleFor(p_Pq => p_Pq.IdQuestion, p_QuestionId ?? new Faker().Random.Int(1, 1000))
            .RuleFor(p_Pq => p_Pq.IdParty, p_PartyId ?? Guid.NewGuid())
            .RuleFor(p_Pq => p_Pq.Order, p_F => p_F.Random.Int(0, 50))
            .RuleFor(p_Pq => p_Pq.Party, _ => null)
            .RuleFor(p_Pq => p_Pq.Question, _ => null)
            .RuleFor(p_Pq => p_Pq.UserPartyQuestion, _ => null);
    }

    public static Faker<UserPartyQuestion> CreateUserPartyQuestion(int? p_PartyQuestionId = null, int? p_UserId = null)
    {
        return new Faker<UserPartyQuestion>()
            .RuleFor(p_Upq => p_Upq.Id, p_F => p_F.Random.Int(1, 10000))
            .RuleFor(p_Upq => p_Upq.IdPartyQuestion, p_PartyQuestionId ?? new Faker().Random.Int(1, 10000))
            .RuleFor(p_Upq => p_Upq.IdUser, p_UserId ?? new Faker().Random.Int(1, 1000))
            .RuleFor(p_Upq => p_Upq.DtPresentedAt, p_F => p_F.Date.Recent(1))
            .RuleFor(p_Upq => p_Upq.DtAnsweredAt, (p_F, p_Upq) => p_F.Date.Between(p_Upq.DtPresentedAt, p_Upq.DtPresentedAt.AddSeconds(15)))
            .RuleFor(p_Upq => p_Upq.IdAnswer, p_F => p_F.Random.Int(1, 10000))
            .RuleFor(p_Upq => p_Upq.Correct, p_F => p_F.Random.Bool())
            .RuleFor(p_Upq => p_Upq.Score, p_F => p_F.Random.Int(0, 1000))
            .RuleFor(p_Upq => p_Upq.AnswersOrder, _ => Guid.NewGuid())
            .RuleFor(p_Upq => p_Upq.Answer, _ => null)
            .RuleFor(p_Upq => p_Upq.PartyQuestion, _ => null)
            .RuleFor(p_Upq => p_Upq.User, _ => null);
    }

    public static Faker<GroupQuestion> CreateGroupQuestion()
    {
        return new Faker<GroupQuestion>()
            .RuleFor(p_Gq => p_Gq.Question, _ => null)
            .RuleFor(p_Gq => p_Gq.CurrentIndex, p_F => p_F.Random.Int(1, 20))
            .RuleFor(p_Gq => p_Gq.Score, p_F => p_F.Random.Int(0, 1000));
    }
}