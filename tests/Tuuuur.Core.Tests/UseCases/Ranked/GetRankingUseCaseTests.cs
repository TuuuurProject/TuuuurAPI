using Microsoft.Extensions.Logging;
using Tuuuur.Core.Requests.Ranked;
using Tuuuur.Core.Responses;
using Tuuuur.Core.UseCases.Ranked;
using Tuuuur.Domain.Bo;
using Tuuuur.Domain.Interfaces.Data;
using Tuuuur.Domain.Interfaces.Services;
using Tuuuur.Domain.Security;

namespace Tuuuur.Core.Tests.UseCases.Ranked;

public class GetRankingUseCaseTests
{
    private readonly Mock<IUnitOfWork> m_UnitOfWorkMock;
    private readonly Mock<IUserRoleService> m_UserRoleServiceMock;
    private readonly Mock<IRankService> m_RankServiceMock;
    private readonly GetRankingUseCase m_UseCase;

    public GetRankingUseCaseTests()
    {
        m_UnitOfWorkMock = new Mock<IUnitOfWork>();
        Mock<ILogger<GetRankingUseCase>> v_LoggerMock = new();
        m_UserRoleServiceMock = new Mock<IUserRoleService>();
        m_RankServiceMock = new Mock<IRankService>();

        m_UseCase = new GetRankingUseCase(
            m_UnitOfWorkMock.Object,
            v_LoggerMock.Object,
            m_UserRoleServiceMock.Object,
            m_RankServiceMock.Object);
    }

    [Fact]
    public async Task Handle_WhenUserExists_ShouldReturnRankingWithTierAndDivision()
    {
        // Arrange
        string v_Email = "test@test.com";
        Guid v_UserId = Guid.NewGuid();
        User v_User = new() { Id = v_UserId, Email = v_Email, Elo = [new Elo { Value = 1200 }] };

        m_UserRoleServiceMock.Setup(p_S => p_S.GetEmail()).Returns(v_Email);
        m_UnitOfWorkMock
            .Setup(p_U => p_U.UserRepository.GetUserByEmailAsync(v_Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(v_User);
        m_UnitOfWorkMock
            .Setup(p_U => p_U.UserRepository.GetRankingPageAsync(v_UserId, 1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new RankingPage { UserRanking = 5, UserElo = 1200 });
        m_RankServiceMock
            .Setup(p_S => p_S.GetRankForElo(It.IsAny<int>()))
            .Returns((3, 2));

        GetRankingRequest v_Request = new(1, 10);

        // Act
        GenericEntityResponse<RankingPage> v_Result = await m_UseCase.Handle(v_Request, CancellationToken.None);

        // Assert
        v_Result.Should().NotBeNull();
        v_Result.Value.Should().NotBeNull();
        v_Result.Value.UserTier.Should().Be(3);
        v_Result.Value.UserDivision.Should().Be(2);
        v_Result.Value.UserRanking.Should().Be(5);
    }

    [Fact]
    public async Task Handle_WhenUserNotFound_ShouldReturnRankingWithoutTierAndDivision()
    {
        // Arrange
        string v_Email = "unknown@test.com";

        m_UserRoleServiceMock.Setup(p_S => p_S.GetEmail()).Returns(v_Email);
        m_UnitOfWorkMock
            .Setup(p_U => p_U.UserRepository.GetUserByEmailAsync(v_Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User)null);
        m_UnitOfWorkMock
            .Setup(p_U => p_U.UserRepository.GetRankingPageAsync(null, 1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new RankingPage { TotalUsers = 50 });

        GetRankingRequest v_Request = new(1, 10);

        // Act
        GenericEntityResponse<RankingPage> v_Result = await m_UseCase.Handle(v_Request, CancellationToken.None);

        // Assert
        v_Result.Should().NotBeNull();
        v_Result.Value.Should().NotBeNull();
        v_Result.Value.UserTier.Should().Be(0);
        v_Result.Value.UserDivision.Should().Be(0);
        v_Result.Value.TotalUsers.Should().Be(50);
        m_RankServiceMock.Verify(p_S => p_S.GetRankForElo(It.IsAny<int>()), Times.Never);
    }
}
