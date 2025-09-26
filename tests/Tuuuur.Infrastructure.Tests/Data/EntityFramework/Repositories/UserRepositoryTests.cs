using Microsoft.Extensions.Logging;
using Tuuuur.Domain.Bo;
using Tuuuur.Domain.Interfaces.Data.Entities;
using Tuuuur.Factory.Tests;
using Tuuuur.Infrastructure.Data.EntityFramework.Entities;
using Tuuuur.Infrastructure.Data.EntityFramework.Repositories;
using Tuuuur.Infrastructure.Tests.Fixtures;

namespace Tuuuur.Infrastructure.Tests.Data.EntityFramework.Repositories
{
    public class UserRepositoryTests : ADatabaseTests
    {
        private readonly Mock<ILogger<UserRepository>> m_MockLogger;

        public UserRepositoryTests(LocalDbFixture p_SqlServerFixture) : base(p_SqlServerFixture)
        {
            m_MockLogger = m_MockRepository.Create<ILogger<UserRepository>>();
        }

        private UserRepository CreateUserRepository()
        {
            return new UserRepository(m_SqlServerFixture.TestContext, m_Mapper, m_MockLogger.Object);
        }

        [Fact]
        public void GetUserByEmailAsync_ExpectedBehavior()
        {
            try
            {
                // Arrange
                UserRepository v_UserRepository = CreateUserRepository();
                UserUsr v_User = EfFactory.CreateUser().Generate();

                Check.ThatCode(async () =>
                {
                    _ = m_SqlServerFixture.TestContext.UserUsr.Add(v_User);
                    _ = await m_SqlServerFixture.TestContext.SaveChangesAsync();
                }).DoesNotThrow();

                // Assert
                Check.ThatCode(() => v_UserRepository.GetUserByEmailAsync(v_User.Email, CancellationToken.None))
                    .WhichResult().Considering().Properties.Excluding(nameof(User.Id), nameof(User.UserAuth), nameof(UserUsr.PartyUserPus), nameof(UserUsr.UserAuthUat), nameof(UserUsr.UserPartyQuestionUpq), nameof(UserUsr.PartyPty), nameof(UserUsr.EloElo)).IsEqualTo(v_User);
            }
            finally
            {
                ClearData(m_SqlServerFixture.TestContext);
            }
        }
        
        [Fact]
        public void GetUserByNickNameAsync_ExpectedBehavior()
        {
            try
            {
                // Arrange
                UserRepository v_UserRepository = CreateUserRepository();
                UserUsr v_User = EfFactory.CreateUser().Generate();

                Check.ThatCode(async () =>
                {
                    _ = m_SqlServerFixture.TestContext.UserUsr.Add(v_User);
                    _ = await m_SqlServerFixture.TestContext.SaveChangesAsync();
                }).DoesNotThrow();

                // Assert
                Check.ThatCode(() => v_UserRepository.GetUserByNickNameAsync(v_User.NickName, CancellationToken.None))
                    .WhichResult().Considering().Properties.Excluding(nameof(User.Id), nameof(User.UserAuth), nameof(UserUsr.PartyUserPus), nameof(UserUsr.UserAuthUat), nameof(UserUsr.UserPartyQuestionUpq), nameof(UserUsr.PartyPty), nameof(UserUsr.EloElo)).IsEqualTo(v_User);
            }
            finally
            {
                ClearData(m_SqlServerFixture.TestContext);
            }
        }

        [Fact]
        public void GetUserAsync_ExpectedBehavior()
        {
            try
            {
                // Arrange
                UserRepository v_UserRepository = CreateUserRepository();
                User v_User = BoFactory.CreateUser().Generate();

                Check.ThatCode(async () =>
                {
                    IMappingAddEntity<User, IEntity> v_MappingAddEntity = (await v_UserRepository.CreateUserAsync(v_User));
                    _ = v_MappingAddEntity.MapBoEntity;
                    _ = await m_SqlServerFixture.TestContext.SaveChangesAsync();
                    _ = v_MappingAddEntity.MapBoEntity.Id;
                }).DoesNotThrow();

                // Assert
                Check.ThatCode(() => m_SqlServerFixture.TestContext.UserUsr.First(p_P => p_P.NickName == v_User.NickName))
                    .WhichResult().Considering().Properties.Excluding(nameof(User.Id), nameof(User.UserAuth), nameof(UserUsr.UserAuthUat), nameof(UserUsr.PartyUserPus), nameof(UserUsr.UserPartyQuestionUpq), nameof(UserUsr.PartyPty), nameof(UserUsr.EloElo)).IsEqualTo(v_User);
            }
            finally
            {
                ClearData(m_SqlServerFixture.TestContext);
            }
        }
    }
}