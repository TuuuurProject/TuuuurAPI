using Bogus;
using Tuuuur.Domain.Bo;
using Tuuuur.Infrastructure.Data.EntityFramework.Entities;

namespace Tuuuur.Factory.Tests;

public static class EfFactory
{
    private static readonly Faker m_GeneralFaker = new();
    public static Faker<User_USR> CreateUser()
    {
        return new Faker<User_USR>()
            .RuleFor(p_O => p_O.NickName, p_F => p_F.Person.FirstName)
            .RuleFor(p_O => p_O.Email, p_F => p_F.Person.Email)
            .RuleFor(p_O => p_O.Password, p_F => p_F.Internet.Password(10))
            .RuleFor(p_O => p_O.ResetPasswordCode, p_F => p_F.Random.Guid())
            .RuleFor(p_O => p_O.Avatar, p_F => p_F.Random.Bytes(50))
            .RuleFor(p_O => p_O.IsNew, p_F => p_F.PickRandom(true, false))
            .RuleFor(p_O => p_O.IsAdmin, p_F => p_F.Random.Bool());
    }
}

public static class BoFactory
{
    private static readonly Faker m_GeneralFaker = new();
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
}