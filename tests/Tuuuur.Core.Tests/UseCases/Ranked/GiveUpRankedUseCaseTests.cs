using Microsoft.Extensions.Logging;
using Tuuuur.Core.Requests.Ranked;
using Tuuuur.Core.Responses;
using Tuuuur.Core.UseCases.Ranked;
using Tuuuur.Domain.Bo;
using Tuuuur.Domain.Errors;
using Tuuuur.Domain.Interfaces;
using Tuuuur.Domain.Interfaces.Data;
using Tuuuur.Domain.Notifications;
using Tuuuur.Factory.Tests;

namespace Tuuuur.Core.Tests.UseCases.Ranked;

public class GiveUpRankedUseCaseTests
{
    private readonly MockRepository m_MockRepository;
    private readonly Mock<IUnitOfWork> m_UnitOfWorkMock;
    private readonly Mock<ICacheService> m_CacheServiceMock;
    private readonly Mock<IRankedNotificationService> m_NotificationServiceMock;
    private readonly GiveUpRankedUseCase m_UseCase;

    public GiveUpRankedUseCaseTests()
    {
        m_MockRepository = new MockRepository(MockBehavior.Strict);
        m_UnitOfWorkMock = m_MockRepository.Create<IUnitOfWork>();
        Mock<ILogger<GiveUpRankedUseCase>> v_LoggerMock = new();
        m_CacheServiceMock = m_MockRepository.Create<ICacheService>();
        m_NotificationServiceMock = m_MockRepository.Create<IRankedNotificationService>();

        m_UseCase = new GiveUpRankedUseCase(
            m_UnitOfWorkMock.Object,
            v_LoggerMock.Object,
            m_CacheServiceMock.Object,
            m_NotificationServiceMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidRequest_ShouldSucceedAsync()
    {
        // Arrange
        Guid v_UserId = Guid.NewGuid();
        User v_User = BoFactory.CreateUser().Generate();
        v_User.Id = v_UserId;
        
        Guid v_PartyId = Guid.NewGuid();
        Party v_Party = BoFactory.CreateParty().Generate();
        v_Party.Id = v_PartyId;
        
        Question v_Question = BoFactory.CreateQuestion().Generate();
        GiveUpRankedRequest v_Request = new GiveUpRankedRequest(v_UserId);

        m_UnitOfWorkMock
            .Setup(p_P => p_P.UserRepository.GetUserByIdAsync(v_UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(v_User);
        m_CacheServiceMock
            .Setup(p_P => p_P.GetAsync<string>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(v_PartyId.ToString());
        m_CacheServiceMock
            .Setup(p_P => p_P.GetAsync<Party>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(v_Party);
        m_CacheServiceMock
            .Setup(p_P => p_P.GetAsync<int>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);
        m_CacheServiceMock
            .Setup(p_P => p_P.SortedSetGetByIndexAsync<Question>(
                It.IsAny<string>(), 
                It.IsAny<long>(), 
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(v_Question);
        m_CacheServiceMock
            .Setup(p_P => p_P.PublishAsync(
                It.IsAny<string>(), 
                It.IsAny<bool>(), 
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        m_CacheServiceMock
            .Setup(p_P => p_P.SetAsync(
                It.IsAny<string>(), 
                v_User, 
                It.IsAny<TimeSpan>(), 
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        EmptyResponse v_Result = await m_UseCase.Handle(v_Request, CancellationToken.None);

        // Assert
        v_Result.Success.Should().BeTrue();
        v_Result.Errors.Should().BeNull();
        m_MockRepository.VerifyAll();
    }

    [Fact]
    public async Task Handle_WithNullUser_ShouldReturnErrorAsync()
    {
        // Arrange
        Guid v_UserId = Guid.NewGuid();
        GiveUpRankedRequest v_Request = new GiveUpRankedRequest(v_UserId);

        m_UnitOfWorkMock
            .Setup(p_P => p_P.UserRepository.GetUserByIdAsync(v_UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User)null);

        // Act
        EmptyResponse v_Result = await m_UseCase.Handle(v_Request, CancellationToken.None);

        // Assert
        v_Result.Success.Should().BeFalse();
        v_Result.Errors.Should().ContainSingle();
        v_Result.Errors.First().Code.Should().Be(DomainErrors.Data.NotFound);
        m_MockRepository.VerifyAll();
    }

    [Fact]
    public async Task Handle_WithInvalidPartyIdInCache_ShouldReturnErrorAsync()
    {
        // Arrange
        User v_User = BoFactory.CreateUser().Generate();
        GiveUpRankedRequest v_Request = new GiveUpRankedRequest(v_User.Id);

        m_UnitOfWorkMock
            .Setup(p_P => p_P.UserRepository.GetUserByIdAsync(v_User.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(v_User);

        m_CacheServiceMock
            .Setup(p_P => p_P.GetAsync<string>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("invalid-guid");

        // Act
        EmptyResponse v_Result = await m_UseCase.Handle(v_Request, CancellationToken.None);

        // Assert
        v_Result.Success.Should().BeFalse();

        v_Result.Errors.Should().ContainSingle();
        v_Result.Errors.First().Code.Should().Be(DomainErrors.Data.NotFound);
        m_MockRepository.VerifyAll();
    }

    [Fact]
    public async Task Handle_WithNullPartyInCache_ShouldReturnErrorAsync()
    {
        // Arrange
        User v_User = BoFactory.CreateUser().Generate();
        Party v_Party = BoFactory.CreateParty().Generate();
        GiveUpRankedRequest v_Request = new GiveUpRankedRequest(v_User.Id);

        m_UnitOfWorkMock
            .Setup(p_P => p_P.UserRepository.GetUserByIdAsync(v_User.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(v_User);

        m_CacheServiceMock
            .Setup(p_P => p_P.GetAsync<string>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(v_Party.Id.ToString());

        m_CacheServiceMock
            .Setup(p_P => p_P.GetAsync<Party>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Party)null);

        // Act
        var v_Result = await m_UseCase.Handle(v_Request, CancellationToken.None);

        // Assert
        v_Result.Success.Should().BeFalse();
        v_Result.Errors.Should().ContainSingle();
        v_Result.Errors.First().Code.Should().Be(DomainErrors.Data.NotFound);
        m_MockRepository.VerifyAll();
    }

    [Fact]
    public async Task Handle_WithPartyIdMismatch_ShouldReturnErrorAsync()
    {
        // Arrange
        User v_User = BoFactory.CreateUser().Generate();
        Party v_Party = BoFactory.CreateParty().Generate();
        Guid v_DifferentPartyId = Guid.NewGuid();
        GiveUpRankedRequest v_Request = new GiveUpRankedRequest(v_User.Id);

        m_UnitOfWorkMock
            .Setup(p_P => p_P.UserRepository.GetUserByIdAsync(v_User.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(v_User);

        m_CacheServiceMock
            .Setup(p_P => p_P.GetAsync<string>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(v_DifferentPartyId.ToString());

        Party v_PartyWithDifferentId = BoFactory.CreateParty().Generate();
        v_PartyWithDifferentId.Id = v_Party.Id;

        m_CacheServiceMock
            .Setup(p_P => p_P.GetAsync<Party>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(v_PartyWithDifferentId);

        // Act
        EmptyResponse v_Result = await m_UseCase.Handle(v_Request, CancellationToken.None);

        // Assert
        v_Result.Success.Should().BeFalse();
        v_Result.Errors.Should().ContainSingle();
        v_Result.Errors.First().Code.Should().Be(DomainErrors.Data.NotFound);
        m_MockRepository.VerifyAll();
    }
}
