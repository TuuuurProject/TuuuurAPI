using Microsoft.Extensions.Logging;
using Tuuuur.Domain.Bo;
using Tuuuur.Factory.Tests;
using Tuuuur.Infrastructure.Data.EntityFramework.Entities;
using Tuuuur.Infrastructure.Data.EntityFramework.Repositories;
using Tuuuur.Infrastructure.Tests.Fixtures;

namespace Tuuuur.Infrastructure.Tests.Data.EntityFramework.Repositories;

public class RefreshTokenRepositoryTests : ADatabaseTests
{
    private readonly Mock<ILogger<RefreshTokenRepository>> m_MockLogger;

    public RefreshTokenRepositoryTests(LocalDbFixture p_SqlServerFixture) : base(p_SqlServerFixture)
    {
        m_MockLogger = m_MockRepository.Create<ILogger<RefreshTokenRepository>>();
    }

    private RefreshTokenRepository CreateRefreshTokenRepository()
    {
        return new RefreshTokenRepository(m_SqlServerFixture.TestContext, m_Mapper, m_MockLogger.Object);
    }

    [Fact]
    public async Task GetRefreshTokenByTokenAsync_WhenTokenExists_ShouldReturnTokenAsync()
    {
        try
        {
            // Arrange
            RefreshTokenRepository v_Repository = CreateRefreshTokenRepository();
            UserUsr v_User = EfFactory.CreateUser().Generate();

            RefreshTokenRtk v_RefreshToken = new()
            {
                UserId = v_User.Id,
                Token = "test-token-12345",
                ExpiresAt = DateTime.UtcNow.AddDays(90),
                CreatedAt = DateTime.UtcNow
            };

            _ = m_SqlServerFixture.TestContext.UserUsr.Add(v_User);
            _ = await m_SqlServerFixture.TestContext.SaveChangesAsync();

            v_RefreshToken.UserId = v_User.Id;
            _ = m_SqlServerFixture.TestContext.RefreshTokenRtk.Add(v_RefreshToken);
            _ = await m_SqlServerFixture.TestContext.SaveChangesAsync();

            // Act
            RefreshToken v_Result = await v_Repository.GetRefreshTokenByTokenAsync("test-token-12345", CancellationToken.None);

            // Assert
            Check.That(v_Result).IsNotNull();
            Check.That(v_Result.Token).IsEqualTo("test-token-12345");
            Check.That(v_Result.UserId).IsEqualTo(v_User.Id);
            Check.That(v_Result.User).IsNotNull();
            Check.That(v_Result.User.Id).IsEqualTo(v_User.Id);
        }
        finally
        {
            ClearData(m_SqlServerFixture.TestContext);
        }
    }

    [Fact]
    public async Task GetRefreshTokenByTokenAsync_WhenTokenDoesNotExist_ShouldReturnNullAsync()
    {
        try
        {
            // Arrange
            RefreshTokenRepository v_Repository = CreateRefreshTokenRepository();

            // Act
            RefreshToken v_Result = await v_Repository.GetRefreshTokenByTokenAsync("non-existent-token", CancellationToken.None);

            // Assert
            Check.That(v_Result).IsNull();
        }
        finally
        {
            ClearData(m_SqlServerFixture.TestContext);
        }
    }

    [Fact]
    public async Task CreateRefreshTokenAsync_ShouldCreateRefreshTokenAsync()
    {
        try
        {
            // Arrange
            RefreshTokenRepository v_Repository = CreateRefreshTokenRepository();
            UserUsr v_User = EfFactory.CreateUser().Generate();

            _ = m_SqlServerFixture.TestContext.UserUsr.Add(v_User);
            _ = await m_SqlServerFixture.TestContext.SaveChangesAsync();

            RefreshToken v_RefreshToken = new()
            {
                UserId = v_User.Id,
                Token = "new-token-67890",
                ExpiresAt = DateTime.UtcNow.AddDays(90),
                CreatedAt = DateTime.UtcNow
            };

            // Act
            RefreshToken v_Result = await v_Repository.CreateRefreshTokenAsync(v_RefreshToken, CancellationToken.None);
            _ = await m_SqlServerFixture.TestContext.SaveChangesAsync();

            // Assert
            Check.That(v_Result).IsNotNull();
            Check.That(v_Result.Token).IsEqualTo("new-token-67890");

            RefreshTokenRtk v_DbToken = m_SqlServerFixture.TestContext.RefreshTokenRtk
                .FirstOrDefault(rt => rt.Token == "new-token-67890");
            Check.That(v_DbToken).IsNotNull();
            Check.That(v_DbToken.UserId).IsEqualTo(v_User.Id);
        }
        finally
        {
            ClearData(m_SqlServerFixture.TestContext);
        }
    }
}
