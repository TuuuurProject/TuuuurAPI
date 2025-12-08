using Microsoft.Extensions.Logging;
using Tuuuur.Core.Requests;
using Tuuuur.Core.Requests.Group;
using Tuuuur.Core.Responses;
using Tuuuur.Core.UseCases.Group;
using Tuuuur.Domain;
using Tuuuur.Domain.Bo;
using Tuuuur.Domain.Interfaces.Data;
using Tuuuur.Domain.Interfaces.Data.Entities;
using Tuuuur.Domain.Notifications;
using Tuuuur.Domain.Security;
using Tuuuur.Factory.Tests;

namespace Tuuuur.Core.Tests.UseCases.Group;

public class JoinGroupPartyUseCaseTests
{
    private readonly Mock<IUnitOfWork> m_UnitOfWorkMock;
    private readonly Mock<ILogger<JoinGroupPartyUseCase>> m_LoggerMock;
    private readonly Mock<IUserRoleService> m_UserRoleService;
    private readonly Mock<INotificationsService> m_NotificationService;

    private readonly JoinGroupPartyUseCase m_UseCase;
    
    public JoinGroupPartyUseCaseTests()
    {
        m_UnitOfWorkMock = new Mock<IUnitOfWork>();
        m_LoggerMock = new Mock<ILogger<JoinGroupPartyUseCase>>();
        m_UserRoleService = new Mock<IUserRoleService>();
        m_NotificationService = new Mock<INotificationsService>();

        m_UseCase = new JoinGroupPartyUseCase(m_UnitOfWorkMock.Object, m_LoggerMock.Object, m_UserRoleService.Object, m_NotificationService.Object);
    }
    /*
    [Fact]
    public async Task Handle_ExpectedAsync()
    {
        // Arrange
        User v_User = BoFactory.CreateUser().Generate();
        Party v_Party = BoFactory.CreateParty().Generate();
        m_UserRoleService.Setup(p_P => p_P.GetCurrentUserEmail()).Returns(v_User.Email);
        m_UnitOfWorkMock.Setup(p_U => p_U.UserRepository.GetUserByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(v_User);

        v_Party.Code = "543214";
        InMemoryDataStore.PartyInProgress.Add(v_Party);
        
        JoinGroupPartyRequest v_Request = new(v_Party.Code);

        // Act
        GenericEntityResponse<Party> v_Result = await m_UseCase.Handle(v_Request, CancellationToken.None);

        // Assert
        Assert.NotNull(v_Result);
        Assert.True(v_Result.Success);

        // Assert
        m_UnitOfWorkMock.Verify(p_Uow => p_Uow.UserRepository.GetUserByEmailAsync(v_User.Email, It.IsAny<CancellationToken>()), Times.Once);
        v_Result.Success.Should().BeTrue();
        v_Result.Errors.Should().BeNull();
    }*/
}