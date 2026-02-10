using Microsoft.Extensions.Logging;
using Tuuuur.Core.Requests;
using Tuuuur.Core.Responses;
using Tuuuur.Core.UseCases.History;
using Tuuuur.Domain.Bo;
using Tuuuur.Domain.Errors;
using Tuuuur.Domain.Interfaces.Data;
using Tuuuur.Domain.Security;
using Tuuuur.Factory.Tests;

namespace Tuuuur.Core.Tests.UseCases.History;

public class GetHistoryUseCaseTests
{
    private readonly MockRepository m_MockRepository;
    private readonly Mock<IUnitOfWork> m_UnitOfWorkMock;
    private readonly Mock<IUserRoleService> m_UserRoleServiceMock;

    private readonly GetHistoryUseCase m_UseCase;

    public GetHistoryUseCaseTests()
    {
        m_MockRepository = new MockRepository(MockBehavior.Strict);
        m_UnitOfWorkMock = m_MockRepository.Create<IUnitOfWork>();
        Mock<ILogger<GetHistoryUseCase>> v_LoggerMock = new();
        m_UserRoleServiceMock = m_MockRepository.Create<IUserRoleService>();

        m_UseCase = new GetHistoryUseCase(
            m_UnitOfWorkMock.Object,
            v_LoggerMock.Object,
            m_UserRoleServiceMock.Object
        );
    }

    [Fact]
    public async Task Handle_UserNotFound_ReturnsError()
    {
        // Arrange
        string v_UserEmail = "test@example.com";
        Guid v_PartyId = Guid.NewGuid();
        GetHistoryRequest v_Request = new(v_PartyId);

        m_UserRoleServiceMock.Setup(p_Urs => p_Urs.GetCurrentUserEmail())
            .Returns(v_UserEmail);
        m_UnitOfWorkMock.Setup(p_U => p_U.UserRepository.GetUserByEmailAsync(v_UserEmail, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User)null);

        // Act
        GenericEntityResponse<PartyBase> v_Result = await m_UseCase.Handle(v_Request, CancellationToken.None);

        // Assert
        v_Result.Success.Should().BeFalse();
        v_Result.Errors.Should().NotBeNull();
        v_Result.Errors.Should().HaveCount(1);
        v_Result.Errors!.First().Code.Should().Be(DomainErrors.Data.NotFound);
        v_Result.Errors!.First().Description.Should().Contain(nameof(User));

        m_MockRepository.VerifyAll();
    }

    [Fact]
    public async Task Handle_ValidRequest_ReturnsParty()
    {
        // Arrange
        User v_User = BoFactory.CreateUser().Generate();
        Guid v_PartyId = Guid.NewGuid();
        Party v_Party = new Party
        {
            Id = v_PartyId,
            IdUserHost = v_User.Id
        };
        GetHistoryRequest v_Request = new(v_PartyId);

        m_UserRoleServiceMock.Setup(p_Urs => p_Urs.GetCurrentUserEmail())
            .Returns(v_User.Email);
        m_UnitOfWorkMock.Setup(p_U => p_U.UserRepository.GetUserByEmailAsync(v_User.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(v_User);
        m_UnitOfWorkMock.Setup(p_U => p_U.PartyRepository.GetByIdAsync(v_PartyId, v_User.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(v_Party);

        // Act
        GenericEntityResponse<PartyBase> v_Result = await m_UseCase.Handle(v_Request, CancellationToken.None);

        // Assert
        v_Result.Success.Should().BeTrue();
        v_Result.Errors.Should().BeNull();
        v_Result.Value.Should().NotBeNull();
        v_Result.Value.Should().Be(v_Party);
        v_Result.Value.Id.Should().Be(v_PartyId);

        m_MockRepository.VerifyAll();
    }

    [Fact]
    public async Task Handle_ValidRequest_CallsCorrectRepositoryMethods()
    {
        // Arrange
        User v_User = BoFactory.CreateUser().Generate();
        Guid v_PartyId = Guid.NewGuid();
        Party v_Party = new Party
        {
            Id = v_PartyId,
            IdUserHost = v_User.Id
        };
        GetHistoryRequest v_Request = new(v_PartyId);

        m_UserRoleServiceMock.Setup(p_Urs => p_Urs.GetCurrentUserEmail())
            .Returns(v_User.Email);
        m_UnitOfWorkMock.Setup(p_U => p_U.UserRepository.GetUserByEmailAsync(v_User.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(v_User);
        m_UnitOfWorkMock.Setup(p_U => p_U.PartyRepository.GetByIdAsync(v_PartyId, v_User.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(v_Party);

        // Act
        _ = await m_UseCase.Handle(v_Request, CancellationToken.None);

        // Assert
        m_UserRoleServiceMock.Verify(p_Urs => p_Urs.GetCurrentUserEmail(), Times.Once);
        m_UnitOfWorkMock.Verify(p_U => p_U.UserRepository.GetUserByEmailAsync(v_User.Email, It.IsAny<CancellationToken>()), Times.Once);
        m_UnitOfWorkMock.Verify(p_U => p_U.PartyRepository.GetByIdAsync(v_PartyId, v_User.Id, It.IsAny<CancellationToken>()), Times.Once);

        m_MockRepository.VerifyAll();
    }

    [Fact]
    public async Task Handle_PartyNotFound_ReturnsNull()
    {
        // Arrange
        User v_User = BoFactory.CreateUser().Generate();
        Guid v_PartyId = Guid.NewGuid();
        GetHistoryRequest v_Request = new(v_PartyId);

        m_UserRoleServiceMock.Setup(p_Urs => p_Urs.GetCurrentUserEmail())
            .Returns(v_User.Email);
        m_UnitOfWorkMock.Setup(p_U => p_U.UserRepository.GetUserByEmailAsync(v_User.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(v_User);
        m_UnitOfWorkMock.Setup(p_U => p_U.PartyRepository.GetByIdAsync(v_PartyId, v_User.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Party)null);

        // Act
        GenericEntityResponse<PartyBase> v_Result = await m_UseCase.Handle(v_Request, CancellationToken.None);

        // Assert
        v_Result.Success.Should().BeTrue();
        v_Result.Errors.Should().BeNull();
        v_Result.Value.Should().BeNull();

        m_MockRepository.VerifyAll();
    }
}
