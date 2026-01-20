using Microsoft.Extensions.Logging;
using Tuuuur.Core.Requests.Group;
using Tuuuur.Core.Responses;
using Tuuuur.Core.UseCases.Group;
using Tuuuur.Domain.Bo;
using Tuuuur.Domain.Interfaces;
using Tuuuur.Domain.Interfaces.Data;
using Tuuuur.Domain.Security;
using Tuuuur.Factory.Tests;

namespace Tuuuur.Core.Tests.UseCases.Group;

public class CreateGroupUseCaseTests
{
    private readonly MockRepository m_MockRepository;
    private readonly Mock<IUnitOfWork> m_UnitOfWorkMock;
    private readonly Mock<IUserRoleService> m_UserRoleServiceMock;
    private readonly Mock<ICacheService> m_CacheServiceMock;

    private readonly CreateGroupUseCase m_UseCase;
    
    public CreateGroupUseCaseTests()
    {
        m_MockRepository = new MockRepository(MockBehavior.Strict);
        m_UnitOfWorkMock = m_MockRepository.Create<IUnitOfWork>();
        Mock<ILogger<CreateGroupUseCase>> v_LoggerMock = m_MockRepository.Create<ILogger<CreateGroupUseCase>>();
        m_UserRoleServiceMock = m_MockRepository.Create<IUserRoleService>();
        m_CacheServiceMock = m_MockRepository.Create<ICacheService>();

        m_UseCase = new CreateGroupUseCase(
            m_UnitOfWorkMock.Object, 
            v_LoggerMock.Object, 
            m_UserRoleServiceMock.Object, 
            m_CacheServiceMock.Object
        );
    }
    
    [Fact]
    public async Task Handle_ExpectedAsync()
    {
        // Arrange
        User v_User = BoFactory.CreateUser().Generate();
        
        m_UserRoleServiceMock.Setup(p_P => p_P.GetCurrentUserEmail())
            .Returns(v_User.Email);
        m_UnitOfWorkMock.Setup(p_U => p_U.UserRepository.GetUserByEmailAsync(v_User.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(v_User);
        m_CacheServiceMock.Setup(p_Cs => p_Cs.GetAsync<string>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(string.Empty);
        m_CacheServiceMock.Setup(p_Cs => p_Cs.GetAsync<GroupParty>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((GroupParty)null);
        
        m_CacheServiceMock.Setup(p_Cs => p_Cs.SetAsync(
                It.IsAny<string>(),
                It.IsAny<GroupParty>(),
                It.IsAny<TimeSpan>(),
                It.IsAny<CancellationToken>()
            ))
            .Returns(Task.CompletedTask);
        m_CacheServiceMock.Setup(p_Cs => p_Cs.SetAsync(
                It.IsAny<string>(), 
                It.IsAny<string>(), 
                It.IsAny<TimeSpan>(), 
                It.IsAny<CancellationToken>()
            ))
            .Returns(Task.CompletedTask);
        m_CacheServiceMock.Setup(p_Cs => p_Cs.SetAddAsync(
                It.IsAny<string>(), 
                v_User.Id,
                It.IsAny<CancellationToken>()
            ))
            .ReturnsAsync(true);
        
        CreateGroupPartyRequest v_Request = new();

        // Act
        GenericEntityResponse<GroupParty> v_Result = await m_UseCase.Handle(v_Request, CancellationToken.None);

        // Assert
        Assert.NotNull(v_Result);
        v_Result.Success.Should().BeTrue();
        v_Result.Errors.Should().BeNull();
        v_Result.Value.Should().NotBeNull();
        v_Result.Value.Code.Should().HaveLength(6);
        v_Result.Value.IdUserHost.Should().Be(v_User.Id);

        m_MockRepository.VerifyAll();
    }
}