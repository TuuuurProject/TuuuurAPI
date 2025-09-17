using Microsoft.Extensions.Logging;
using Tuuuur.Domain.Bo;
using Tuuuur.Domain.Interfaces.Data.Entities;
using Tuuuur.Factory.Tests;
using Tuuuur.Infrastructure.Data.EntityFramework.Entities;
using Tuuuur.Infrastructure.Data.EntityFramework.Repositories;
using Tuuuur.Infrastructure.Tests.Fixtures;

namespace Tuuuur.Infrastructure.Tests.Data.EntityFramework.Repositories
{
    public class UserAuthRepositoryTests : ADatabaseTests
    {
        private readonly Mock<ILogger<UserAuthRepository>> m_MockLogger;

        public UserAuthRepositoryTests(LocalDbFixture p_SqlServerFixture) : base(p_SqlServerFixture)
        {
            m_MockLogger = m_MockRepository.Create<ILogger<UserAuthRepository>>();
        }

        private UserAuthRepository CreateUserAuthRepository()
        {
            return new UserAuthRepository(m_SqlServerFixture.TestContext, m_Mapper, m_MockLogger.Object);
        }

        [Fact]
        public void AddAuthCodeAsync_ExpectedBehavior()
        {
            try
            {
                // Arrange
                UserAuthRepository v_UserAuthRepository = CreateUserAuthRepository();
                User_USR v_User = EfFactory.CreateUser().Generate();
                UserAuth v_UserAuth = BoFactory.CreateUserAuth(v_User.Id).Generate();

                Check.ThatCode(async () =>
                {
                    _ = m_SqlServerFixture.TestContext.User_USR.Add(v_User);
                    _ = await m_SqlServerFixture.TestContext.SaveChangesAsync();
                }).DoesNotThrow();

                // Assert
                Check.ThatCode(async () =>
                {
                    IMappingAddEntity<UserAuth, IEntity> v_ProjectResult = await v_UserAuthRepository.AddAuthCodeAsync(v_UserAuth);
                    _ = await m_SqlServerFixture.TestContext.SaveChangesAsync();
                }).DoesNotThrow();
            }
            finally
            {
                ClearData(m_SqlServerFixture.TestContext);
            }
        }

        [Fact]
        public void GetUserAuthByUserIdAndCodeAsync_ExpectedBehavior()
        {
            try
            {
                // Arrange
                UserAuthRepository v_UserAuthRepository = CreateUserAuthRepository();
                User_USR v_User = EfFactory.CreateUser().Generate();

                Check.ThatCode(async () =>
                {
                    _ = m_SqlServerFixture.TestContext.User_USR.Add(v_User);
                    _ = await m_SqlServerFixture.TestContext.SaveChangesAsync();
                    v_User.Id = m_SqlServerFixture.TestContext.User_USR.First(p_P => p_P.NickName == v_User.NickName)
                        .Id;
                }).DoesNotThrow();

                UserAuth_UAT v_UserAuth = EfFactory.CreateUserAuth(v_User.Id).Generate();

                Check.ThatCode(async () =>
                {
                    _ = m_SqlServerFixture.TestContext.UserAuth_UAT.Add(v_UserAuth);
                    _ = await m_SqlServerFixture.TestContext.SaveChangesAsync();
                }).DoesNotThrow();

                // Assert

                Check.ThatCode(async () =>
                        await v_UserAuthRepository.GetUserAuthByUserIdAndCodeAsync(v_UserAuth.UserId, v_UserAuth.Code))
                    .WhichResult().Considering().Properties.Excluding(nameof(UserAuth.User)).IsEqualTo(v_UserAuth);
            }
            finally
            {
                ClearData(m_SqlServerFixture.TestContext);
            }
        }

        [Fact]
        public void DeleteUserAuthAsync_ExpectedBehavior()
        {
            try
            {
                // Arrange
                UserAuthRepository v_UserAuthRepository = CreateUserAuthRepository();
                User_USR v_User = EfFactory.CreateUser().Generate();

                Check.ThatCode(async () =>
                {
                    _ = m_SqlServerFixture.TestContext.User_USR.Add(v_User);
                    _ = await m_SqlServerFixture.TestContext.SaveChangesAsync();
                    v_User.Id = m_SqlServerFixture.TestContext.User_USR.First(p_P => p_P.NickName == v_User.NickName)
                        .Id;
                }).DoesNotThrow();

                UserAuth_UAT v_UserAuth = EfFactory.CreateUserAuth(v_User.Id).Generate();

                Check.ThatCode(async () =>
                {
                    _ = m_SqlServerFixture.TestContext.UserAuth_UAT.Add(v_UserAuth);
                    _ = await m_SqlServerFixture.TestContext.SaveChangesAsync();
                    v_UserAuth.Id = m_SqlServerFixture.TestContext.UserAuth_UAT.First(p_P => p_P.Code == v_UserAuth.Code)
                        .Id;
                }).DoesNotThrow();

                // Assert
                Check.ThatCode(async () =>
                {
                    await v_UserAuthRepository.DeleteUserAuthAsync(v_UserAuth.Id);
                    _ = await m_SqlServerFixture.TestContext.SaveChangesAsync();
                }).DoesNotThrow();

                Check.That(m_SqlServerFixture.TestContext.UserAuth_UAT
                        .FirstOrDefault(p => p.Code == v_UserAuth.Code))
                    .IsNull();

            }
            finally
            {
                ClearData(m_SqlServerFixture.TestContext);
            }
        }
    }
}