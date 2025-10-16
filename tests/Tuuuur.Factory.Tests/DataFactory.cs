using Bogus;
using Tuuuur.Domain.Bo;
using Tuuuur.Infrastructure.Data.EntityFramework.Entities;

namespace Tuuuur.Factory.Tests;

public static class EfFactory
{
    public static Faker<UserUsr> CreateUser()
    {
        return new Faker<UserUsr>()
            .RuleFor(p_O => p_O.NickName, p_F => p_F.Person.FirstName)
            .RuleFor(p_O => p_O.Email, p_F => p_F.Person.Email)
            .RuleFor(p_O => p_O.Password, p_F => p_F.Internet.Password(10))
            .RuleFor(p_O => p_O.ResetPasswordCode, p_F => p_F.Random.Guid())
            .RuleFor(p_O => p_O.Avatar, p_F => p_F.Random.Bytes(50))
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

public static class BoFactory
{
    public static Faker<User> CreateUser()
    {
        return new Faker<User>()
            .RuleFor(p_O => p_O.NickName, p_F => p_F.Person.FirstName)
            .RuleFor(p_O => p_O.Email, (p_F, p_O) => p_F.Internet.Email())
            .RuleFor(p_O => p_O.Password, p_F => p_F.Internet.Password(10))
            .RuleFor(p_O => p_O.ResetPasswordCode, p_F => p_F.Random.Guid())
            .RuleFor(p_O => p_O.Avatar, p_F => p_F.Random.Bytes(50))
            .RuleFor(p_O => p_O.IsNew, p_F => p_F.PickRandom(true, false))
            .RuleFor(p_O => p_O.IsAdmin, p_F => p_F.PickRandom(true, false));
    }
    public static Faker<UserAuth> CreateUserAuth(int p_UserId)
    {
        return new Faker<UserAuth>()
            .RuleFor(p_O => p_O.UserId, p_UserId)
            .RuleFor(p_O => p_O.Code, p_F => p_F.Random.AlphaNumeric(6));
    }
    
    public static Faker<Party> CreateParty()
    {
        return new Faker<Party>()
            .RuleFor(p_Party => p_Party.Id, _ => Guid.NewGuid())
            .RuleFor(p_Party => p_Party.Dt, p_F => p_F.Date.Recent(30))
            .RuleFor(p_Party => p_Party.Code, p_F => p_F.Random.AlphaNumeric(6))
            .RuleFor(p_Party => p_Party.IdPartyType, p_F => p_F.Random.Int(1, 10))
            .RuleFor(p_Party => p_Party.IdUserHost, p_F => p_F.Random.Int(1, 1000))
            .RuleFor(p_Party => p_Party.Active, p_F => p_F.Random.Bool())
            .RuleFor(p_Party => p_Party.PartyQuestions, _ => [])
            .RuleFor(p_Party => p_Party.PartyUsers, p_Faker => new Faker<PartyUser>().Generate(5));
    }
}