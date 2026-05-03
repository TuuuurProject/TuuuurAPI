using Microsoft.Extensions.Logging;
using Tuuuur.Core.Requests.Ranked;
using Tuuuur.Core.Responses;
using Tuuuur.Core.UseCases.Ranked;
using Tuuuur.Domain.Bo;
using Tuuuur.Domain.Bo.Enum;
using Tuuuur.Domain.Interfaces.Data;
using Tuuuur.Domain.Interfaces.Data.Repositories;
using Tuuuur.Domain.Security;

namespace Tuuuur.Core.Tests.UseCases.Ranked;

public class GetRankedUseCaseTests
{
    private readonly Mock<IUnitOfWork> m_UnitOfWorkMock;
    private readonly Mock<ILogger<GetRankedUseCase>> m_LoggerMock;
    private readonly Mock<IUserRepository> m_UserRepositoryMock;
    private readonly Mock<IPartyRepository> m_PartyRepositoryMock;
    private readonly Mock<IUserRoleService> m_UserRoleServiceMock;
    private readonly GetRankedUseCase m_UseCase;

    public GetRankedUseCaseTests()
    {
        m_UnitOfWorkMock = new Mock<IUnitOfWork>();
        m_LoggerMock = new Mock<ILogger<GetRankedUseCase>>();
        m_UserRepositoryMock = new Mock<IUserRepository>();
        m_PartyRepositoryMock = new Mock<IPartyRepository>();
        m_UserRoleServiceMock = new Mock<IUserRoleService>();

        m_UnitOfWorkMock.Setup(p_U => p_U.UserRepository).Returns(m_UserRepositoryMock.Object);
        m_UnitOfWorkMock.Setup(p_U => p_U.PartyRepository).Returns(m_PartyRepositoryMock.Object);

        m_UseCase = new GetRankedUseCase(
            m_UnitOfWorkMock.Object,
            m_LoggerMock.Object,
            m_UserRoleServiceMock.Object);
    }

    [Fact]
    public async Task Handle_WhenUserDoesNotExist_ShouldReturnError()
    {
        // Arrange
        string v_Email = "missing@test.com";
        m_UserRoleServiceMock.Setup(p_S => p_S.GetEmail()).Returns(v_Email);
        m_UserRepositoryMock
            .Setup(p_U => p_U.GetUserByEmailAsync(v_Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User)null);

        GetRankedRequest v_Request = new(Guid.NewGuid());

        // Act
        GenericEntityResponse<RankedParty> v_Result = await m_UseCase.Handle(v_Request, CancellationToken.None);

        // Assert
        v_Result.Success.Should().BeFalse();
        v_Result.Value.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WhenPartyIsNotRanked_ShouldReturnError()
    {
        // Arrange
        string v_Email = "player@test.com";
        Guid v_UserId = Guid.NewGuid();
        Guid v_PartyId = Guid.NewGuid();

        m_UserRoleServiceMock.Setup(p_S => p_S.GetEmail()).Returns(v_Email);
        m_UserRepositoryMock
            .Setup(p_U => p_U.GetUserByEmailAsync(v_Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User { Id = v_UserId });
        m_PartyRepositoryMock
            .Setup(p_P => p_P.GetRankedByIdAsync(v_PartyId, v_UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new RankedParty { Id = v_PartyId, IdPartyType = (int)PartyTypeType.Group });

        GetRankedRequest v_Request = new(v_PartyId);

        // Act
        GenericEntityResponse<RankedParty> v_Result = await m_UseCase.Handle(v_Request, CancellationToken.None);

        // Assert
        v_Result.Success.Should().BeFalse();
        v_Result.Value.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WhenPartyDoesNotExist_ShouldReturnError()
    {
        // Arrange
        string v_Email = "player@test.com";
        Guid v_UserId = Guid.NewGuid();
        Guid v_PartyId = Guid.NewGuid();

        m_UserRoleServiceMock.Setup(p_S => p_S.GetEmail()).Returns(v_Email);
        m_UserRepositoryMock
            .Setup(p_U => p_U.GetUserByEmailAsync(v_Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User { Id = v_UserId });
        m_PartyRepositoryMock
            .Setup(p_P => p_P.GetRankedByIdAsync(v_PartyId, v_UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((RankedParty)null);

        GetRankedRequest v_Request = new(v_PartyId);

        // Act
        GenericEntityResponse<RankedParty> v_Result = await m_UseCase.Handle(v_Request, CancellationToken.None);

        // Assert
        v_Result.Success.Should().BeFalse();
        v_Result.Value.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WhenRankedPartyExists_ShouldReturnParty()
    {
        // Arrange
        string v_Email = "player@test.com";
        Guid v_UserId = Guid.NewGuid();
        Guid v_PartyId = Guid.NewGuid();
        RankedParty v_Party = new()
        {
            Id = v_PartyId,
            IdPartyType = (int)PartyTypeType.Ranked
        };

        m_UserRoleServiceMock.Setup(p_S => p_S.GetEmail()).Returns(v_Email);
        m_UserRepositoryMock
            .Setup(p_U => p_U.GetUserByEmailAsync(v_Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User { Id = v_UserId });
        m_PartyRepositoryMock
            .Setup(p_P => p_P.GetRankedByIdAsync(v_PartyId, v_UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(v_Party);

        GetRankedRequest v_Request = new(v_PartyId);

        // Act
        GenericEntityResponse<RankedParty> v_Result = await m_UseCase.Handle(v_Request, CancellationToken.None);

        // Assert
        v_Result.Success.Should().BeTrue();
        v_Result.Value.Should().BeSameAs(v_Party);
    }
}